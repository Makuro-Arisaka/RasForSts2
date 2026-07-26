import os

POWER_DIR = r"f:\桌面\Mod\RasForSts2\Scripts\Powers"

POWER_NAMES = [
    "AgileStepPower",
    "MoonlightGreatswordPower",
    "MoonlightShieldPower",
    "MoonlightStaffPower",
    "MoonlightBladesPower",
    "QueenHarpPower",
    "HeroDeterminationPower",
    "CurseRevealPower",
    "MixedBombPower",
]

for name in POWER_NAMES:
    filepath = os.path.join(POWER_DIR, f"{name}.cs")
    if not os.path.exists(filepath):
        print(f"[SKIP] Not found: {filepath}")
        continue

    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()

    old_line = 'placeholder.png'
    new_line = f'{name}.png'

    if old_line in content:
        content = content.replace(old_line, new_line)
        with open(filepath, "w", encoding="utf-8") as f:
            f.write(content)
        print(f"[OK] {name}.cs: placeholder.png -> {name}.png")
    else:
        print(f"[WARN] {name}.cs: placeholder.png not found")

print("\nDone!")
