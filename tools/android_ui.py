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
            if node.get("text", "").casefold() == label.casefold():
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

