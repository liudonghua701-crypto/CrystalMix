"""Native APK smoke checks on a disposable Android emulator, using only adb."""
import pathlib, re, subprocess, time, xml.etree.ElementTree as ET

OUT = pathlib.Path("smoke-artifacts")
OUT.mkdir(exist_ok=True)
PACKAGE = "io.github.liudonghua701.crystalmix"

def adb(*args):
    return subprocess.check_output(["adb", *args], text=True, timeout=40).strip()

def nodes():
    adb("shell", "uiautomator", "dump", "/sdcard/cm-window.xml")
    adb("pull", "/sdcard/cm-window.xml", str(OUT / "window.xml"))
    return list(ET.parse(OUT / "window.xml").iter("node"))

def click_text(label, scroll=False):
    for _ in range(6 if scroll else 1):
        for node in nodes():
            if node.get("text") == label:
                values = list(map(int, re.findall(r"\d+", node.get("bounds", ""))))
                if len(values) == 4 and values[3] > values[1]:
                    adb("shell", "input", "tap", str((values[0]+values[2])//2), str((values[1]+values[3])//2))
                    time.sleep(1)
                    return
        adb("shell", "input", "swipe", "240", "780", "240", "300", "350")
    raise AssertionError("Control not found: " + label)

def texts():
    return "\n".join(n.get("text", "") for n in nodes())

def screenshot(name):
    adb("shell", "screencap", "-p", "/sdcard/cm-screen.png")
    adb("pull", "/sdcard/cm-screen.png", str(OUT / (name + ".png")))

adb("shell", "monkey", "-p", PACKAGE, "-c", "android.intent.category.LAUNCHER", "1")
time.sleep(8)
assert adb("shell", "pidof", PACKAGE), "App did not remain running"
screenshot("recipe-initial")
click_text("Calculate masses", scroll=True)
adb("shell", "input", "swipe", "240", "780", "240", "250", "350")
assert "Total: 10 g" in texts(), "Default batch did not calculate"
screenshot("recipe-results")
click_text("Target")
assert "0.6" in texts(), "Target composition preview missing"
click_text("Generate editable recipe", scroll=True)
click_text("Use recipe")
assert "BaAs" in texts(), "Target did not populate actual material cards"
click_text("Isotopes")
search = next(n for n in nodes() if n.get("class") == "android.widget.EditText")
b = list(map(int, re.findall(r"\d+", search.get("bounds"))))
adb("shell", "input", "tap", str((b[0]+b[2])//2), str((b[1]+b[3])//2))
adb("shell", "input", "text", "175Lu")
adb("shell", "input", "keyevent", "4")
assert "[175Lu]" in texts(), "Isotope search failed"
screenshot("isotope-search")
click_text("Add [175Lu]", scroll=True)
click_text("Guide")
guide = "\n".join(n.get("text", "") for n in nodes() if n.get("class") == "android.widget.TextView")
assert "A small guide" in guide and not re.search(r"[\u4e00-\u9fff]", guide), "English help leaked Chinese"
screenshot("guide-english")
click_text("中文")
assert "配料计算使用说明" in texts(), "Chinese help missing"
screenshot("guide-chinese")
adb("shell", "input", "keyevent", "3")
adb("shell", "am", "force-stop", PACKAGE)
adb("shell", "monkey", "-p", PACKAGE, "-c", "android.intent.category.LAUNCHER", "1")
time.sleep(3)
assert "配料计算使用说明" in texts(), "Language/page draft did not survive restart"
log = adb("logcat", "-d", "-s", "AndroidRuntime:E")
assert "FATAL EXCEPTION" not in log, "Android runtime crash"
(OUT / "result.txt").write_text("PASS: APK installation, startup, mass calculation, target filling, isotope search/add, localized Help and restart persistence.\n", encoding="utf-8")
print((OUT / "result.txt").read_text())
