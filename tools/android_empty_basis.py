"""Check empty -> entered -> cleared actual-material behavior without switches."""
from android_ui import *

LABEL = "Actual materials · comma separated; blank = elements"

def run(edit, top):
    assert not any(n.get("class") == "android.widget.CheckBox" for n in nodes()), "Unexpected toggle"
    basis = next(n for n in nodes() if n.get("content-desc") == LABEL)
    assert basis.get("enabled") == "true" and not basis.get("text"), "Initial basis must be empty and editable"
    edit("Target formula", "Fe2O3")
    screenshot("target-empty-materials")
    click_text("Generate editable recipe", scroll=True); click_text("Use recipe")
    values = {n.get("content-desc"): n.get("text") for n in nodes()}
    assert values.get("Material 1") == "Fe" and values.get("Molar parts 1") == "2"
    top(); click_text("Target composition ▾")
    edit(LABEL, "Fe2O3")
    click_text("Generate editable recipe", scroll=True); click_text("Use recipe")
    values = {n.get("content-desc"): n.get("text") for n in nodes()}
    assert values.get("Material 1") == "Fe2O3" and values.get("Molar parts 1") == "1", "Entered basis ignored"
    screenshot("target-entered-materials")
    top(); click_text("Target composition ▾")
    edit(LABEL, "%s")  # adb input converts %s into a space, exercising blank/whitespace behavior.
    click_text("Generate editable recipe", scroll=True); click_text("Use recipe")
    values = {n.get("content-desc"): n.get("text") for n in nodes()}
    assert values.get("Material 1") == "Fe" and values.get("Molar parts 1") == "2", "Clearing basis failed"
    screenshot("target-cleared-materials")
    top(); click_text("Target composition ▾")
    edit("Target formula", "Ba1-xKxFe2As2")
    edit(LABEL, "BaAs,Fe2As,KAs")
