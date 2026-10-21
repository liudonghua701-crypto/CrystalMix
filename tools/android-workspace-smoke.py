"""Exercise the single-workspace Android UI on a disposable emulator."""
from android_ui import *

def edit(label, value):
    for _ in range(7):
        match = next((n for n in nodes() if n.get("content-desc") == label), None)
        if match is not None:
            b = list(map(int, re.findall(r"\d+", match.get("bounds"))))
            adb("shell", "input", "tap", str((b[0]+b[2])//2), str((b[1]+b[3])//2))
            adb("shell", "input", "keyevent", "123")
            adb("shell", "input", "keyevent", *(["67"] * 60))
            adb("shell", "input", "text", value)
            adb("shell", "input", "keyevent", "4")
            time.sleep(1)
            return
        adb("shell", "input", "swipe", "240", "750", "240", "300", "350")
    raise AssertionError("Field missing: " + label)

def top():
    for _ in range(5):
        adb("shell", "input", "swipe", "240", "300", "240", "750", "150")

def assert_toolbar():
    for label in ("+ Material", "Isotopes", "More"):
        n = next(n for n in nodes() if n.get("text") == label)
        b = list(map(int, re.findall(r"\d+", n.get("bounds"))))
        assert 0 <= b[0] < b[2] <= 360 and b[3]-b[1] >= 48, label

adb("shell", "monkey", "-p", PACKAGE, "-c", "android.intent.category.LAUNCHER", "1")
time.sleep(8)
assert adb("shell", "pidof", PACKAGE)
assert "Total: 10 g" in texts(), "Initial automatic calculation missing"
screenshot("workspace-default")
import os
if os.environ.get("RELEASE_TAG", "").startswith("android-v1.2."):
    import android_weighing
    android_weighing.run(edit)
edit("Total batch mass · g", "20")
assert "Total: 20 g" in texts(), "Mass edit did not calculate automatically"
edit("Molar parts 1", "2")
assert "Total: 20 g" in texts()
screenshot("workspace-edited")
top(); click_text("Target composition ▾")
edit("x (0–1)", "0.25")
click_text("Generate editable recipe", scroll=True); click_text("Use recipe")
assert "BaAs" in texts() and "0.75" in texts(), "Target did not fill editable recipe"
assert "Total: 20 g" in texts(), "Target application lost batch mass"
click_text("Isotopes")
edit("Search · Lu, Lutetium, [175Lu]", "175Lu")
click_text("Add [175Lu]", scroll=True)
assert "[175Lu]" in texts() and "More" in texts(), "Isotope did not return to recipe"
screenshot("workspace-isotope-added")
click_text("More"); click_text("Guide")
assert "One workspace" in texts(), "Updated Help missing"
screenshot("workspace-help")
click_text("Back to recipe")
click_text("中文")
assert "合计: 20 g" in texts()
screenshot("workspace-chinese")
adb("shell", "input", "keyevent", "3"); adb("shell", "am", "force-stop", PACKAGE)
adb("shell", "monkey", "-p", PACKAGE, "-c", "android.intent.category.LAUNCHER", "1")
time.sleep(3)
assert "合计: 20 g" in texts() and "BaAs" in texts(), "Recipe draft was not restored"
click_text("EN")
adb("shell", "wm", "size", "360x800")
adb("shell", "settings", "put", "system", "font_scale", "1.3")
time.sleep(3)
assert_toolbar(); screenshot("workspace-narrow-large-font")
assert "FATAL EXCEPTION" not in adb("logcat", "-d", "-s", "AndroidRuntime:E")
(OUT / "result.txt").write_text("PASS: install/startup, live mass and ratio edits, target application, isotope return, contextual Help, language switching, recipe restoration and 360dp/1.3x toolbar.\n", encoding="utf-8")
print((OUT / "result.txt").read_text())
