from PIL import Image, ImageDraw
import os

POWER_SIZE = 150
CARD_DIR = r"f:\桌面\Mod\RasForSts2\RasForSts2\images\cards"
POWER_DIR = r"f:\桌面\Mod\RasForSts2\RasForSts2\images\powers"

CARD_TO_POWER_MAP = {
    "MoonlightGreatsword": "MoonlightGreatswordPower",
    "MoonlightShield": "MoonlightShieldPower",
    "MoonlightStaff": "MoonlightStaffPower",
    "MoonlightBlades": "MoonlightBladesPower",
    "QueenHarp": "QueenHarpPower",
    "HeroDetermination": "HeroDeterminationPower",
    "CurseReveal": "CurseRevealPower",
    "MixedBomb": "MixedBombPower",
    "AgileStep": "AgileStepPower",
    "WindFist": "WindFistPower",
}


def center_crop_to_square(img: Image.Image, size: int) -> Image.Image:
    w, h = img.size
    side = min(w, h)
    left = (w - side) // 2
    top = (h - side) // 2
    img = img.crop((left, top, left + side, top + side))
    return img.resize((size, size), Image.LANCZOS)


def make_circle(img: Image.Image, size: int) -> Image.Image:
    img = img.convert("RGBA")
    mask = Image.new("L", (size, size), 0)
    draw = ImageDraw.Draw(mask)
    draw.ellipse((0, 0, size, size), fill=255)
    result = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    result.paste(img, (0, 0), mask)
    return result


def process_card_to_power(card_name: str, power_name: str) -> str | None:
    card_path = os.path.join(CARD_DIR, f"{card_name}.png")
    power_path = os.path.join(POWER_DIR, f"{power_name}.png")

    if not os.path.exists(card_path):
        print(f"[SKIP] Card image not found: {card_path}")
        return None

    img = Image.open(card_path).convert("RGBA")
    img = center_crop_to_square(img, POWER_SIZE)
    img = make_circle(img, POWER_SIZE)
    img.save(power_path, "PNG")
    print(f"[OK] {card_name}.png -> {power_name}.png")
    return power_path


def main():
    os.makedirs(POWER_DIR, exist_ok=True)
    count = 0
    for card_name, power_name in CARD_TO_POWER_MAP.items():
        if process_card_to_power(card_name, power_name):
            count += 1
    print(f"\nDone! Processed {count}/{len(CARD_TO_POWER_MAP)} power icons.")


if __name__ == "__main__":
    main()
