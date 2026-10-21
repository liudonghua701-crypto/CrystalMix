"""Measured-mass scaling checks, before the general workspace scenarios."""
from android_ui import *

def run(edit):
    click_text("Use actual mass", scroll=True)
    edit("Actual mass · g", "0")
    apply = next(n for n in nodes() if n.get("text", "").casefold() == "apply scale")
    assert apply.get("enabled") == "false", "Zero actual mass enabled Apply"
    edit("Actual mass · g", "5")
    assert "Se" in texts(), "Other material preview missing"
    screenshot("weighing-preview")
    click_text("Cancel")
    assert "Total: 10 g" in texts(), "Cancel mutated the batch"
    click_text("Use actual mass", scroll=True)
    edit("Actual mass · g", "5")
    click_text("Apply scale")
    page = texts()
    total = float(re.search(r"Total: ([0-9.]+) g", page).group(1))
    expected = 5 * (55.845 + 78.971) / 55.845
    assert abs(total-expected) < 1e-7, "Scaled total incorrect"
    assert "5 g" in page, "Anchor mass not applied"
    values = {n.get("content-desc"): n.get("text") for n in nodes()}
    assert values.get("Molar parts 1") == "1", "Scaling changed molar parts"
    screenshot("weighing-applied")
