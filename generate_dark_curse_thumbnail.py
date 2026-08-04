"""
生成黑暗法咒的缩略图图标。
从原始大图标缩放到 24x24，用于卡牌描述中的内联图标显示。
"""
from PIL import Image
import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
IMAGES_DIR = os.path.join(SCRIPT_DIR, "RasForSts2", "images", "ui")

SOURCE_BIG = os.path.join(IMAGES_DIR, "DarkCurse_large.png")
OUTPUT_SMALL = os.path.join(IMAGES_DIR, "DarkCurse_small.png")

TARGET_SIZE = 24

def generate_dark_curse_thumbnail():
    img = Image.open(SOURCE_BIG).convert("RGBA")

    # 等比缩放，保持宽高比
    img.thumbnail((TARGET_SIZE, TARGET_SIZE), Image.LANCZOS)

    # 创建 24x24 画布，居中放置
    canvas = Image.new("RGBA", (TARGET_SIZE, TARGET_SIZE), (0, 0, 0, 0))
    offset_x = (TARGET_SIZE - img.width) // 2
    offset_y = (TARGET_SIZE - img.height) // 2
    canvas.paste(img, (offset_x, offset_y), img)

    canvas.save(OUTPUT_SMALL)
    print(f"Generated: {OUTPUT_SMALL} ({canvas.size})")

if __name__ == "__main__":
    generate_dark_curse_thumbnail()
