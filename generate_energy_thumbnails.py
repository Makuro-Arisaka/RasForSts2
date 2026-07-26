"""
生成希拉角色的能量缩略图。
从原始大能量图标缩放到指定尺寸。
"""
from PIL import Image
import os

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
IMAGES_DIR = os.path.join(SCRIPT_DIR, "RasForSts2", "images")

SOURCE_BIG = os.path.join(IMAGES_DIR, "energy_xila_big.png")
OUTPUT_SPRITE_FONT = os.path.join(IMAGES_DIR, "xila_energy_icon.png")

TARGET_SIZE = 24

def generate_energy_thumbnail():
    img = Image.open(SOURCE_BIG).convert("RGBA")
    
    # 等比缩放，保持宽高比
    img.thumbnail((TARGET_SIZE, TARGET_SIZE), Image.LANCZOS)
    
    # 创建 24x24 画布，居中放置
    canvas = Image.new("RGBA", (TARGET_SIZE, TARGET_SIZE), (0, 0, 0, 0))
    offset_x = (TARGET_SIZE - img.width) // 2
    offset_y = (TARGET_SIZE - img.height) // 2
    canvas.paste(img, (offset_x, offset_y), img)
    
    canvas.save(OUTPUT_SPRITE_FONT)
    print(f"Generated: {OUTPUT_SPRITE_FONT} ({canvas.size})")

if __name__ == "__main__":
    generate_energy_thumbnail()
