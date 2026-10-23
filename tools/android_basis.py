"""Verify that a disabled precursor basis cannot constrain a new target."""
from android_ui import *

def run(edit, top):
    toggle = next(n for n in nodes() if n.get("text") == "Use selected actual materials")
    assert toggle.get("checked") == "false", "Actual materials unexpectedly enabled by default"
    basis = next(n for n in nodes() if n.get("content-desc") == "Actual materials · comma separated; blank = elements")
    assert basis.get("enabled") == "false", "Disabled basis is still editable"
    edit("Target formula", "Fe2O3")
    screenshot("target-basis-disabled")
    click_text("Generate editable recipe", scroll=True); click_text("Use recipe")
    page = texts()
    assert "Fe" in page and "O" in page and "BaAs" not in page, "Preset arsenides constrained Fe2O3"
    screenshot("target-elemental-result")
    top(); click_text("Target composition ▾")
    edit("Target formula", "Ba1-xKxFe2As2")
    click_text("Use selected actual materials", scroll=True)
    basis = next(n for n in nodes() if n.get("content-desc") == "Actual materials · comma separated; blank = elements")
    assert basis.get("enabled") == "true", "Opt-in did not enable basis"
