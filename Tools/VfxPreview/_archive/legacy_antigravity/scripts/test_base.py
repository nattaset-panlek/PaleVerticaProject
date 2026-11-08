import math
import os
import random
from PIL import Image, ImageDraw, ImageFont, ImageFilter

OUTPUT_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
os.makedirs(OUTPUT_DIR, exist_ok=True)

WIDTH = 640
HEIGHT = 400
FPS = 30
NUM_FRAMES = 24

def create_base_canvas():
    im = Image.new("RGBA", (WIDTH, HEIGHT), (22, 24, 30, 255))
    d = ImageDraw.Draw(im)
    # Floor perspective grid
    horizon_y = 280
    floor_color = (30, 34, 44, 255)
    d.rectangle([0, horizon_y, WIDTH, HEIGHT], fill=floor_color)
    d.line([0, horizon_y, WIDTH, horizon_y], fill=(45, 52, 68, 255), width=2)
    for x in range(0, WIDTH + 60, 60):
        d.line([x, horizon_y, x - 40, HEIGHT], fill=(38, 44, 58, 255), width=1)
    for y in [310, 345, 385]:
        d.line([0, y, WIDTH, y], fill=(38, 44, 58, 255), width=1)
    return im

print("Base test ready")
