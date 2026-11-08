"""Measure a UI sprite: size, meta borders, and colour changes along a row and a column.

Usage:
    python -I measure_art.py path/to/sprite.png [--row Y] [--col X]

Use the printed transitions to find the outer stroke, the inner panel and the header band,
then set the 9-slice border (left, bottom, right, top) so corners and bands are not stretched.
"""
import os
import re
import sys

from PIL import Image


def transitions(pixels, threshold=12):
    out = []
    prev = None
    for i, p in enumerate(pixels):
        if prev is None or max(abs(a - b) for a, b in zip(p, prev)) > threshold:
            out.append((i, p))
        prev = p
    return out


def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    path = sys.argv[1]
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    args = sys.argv[2:]
    row = int(args[args.index("--row") + 1]) if "--row" in args else h // 2
    col = int(args[args.index("--col") + 1]) if "--col" in args else w // 2
    print(f"{path}: {w} x {h}")

    meta = path + ".meta"
    if os.path.exists(meta):
        text = open(meta, encoding="utf-8").read()
        for key in ("spriteMode", "spritePixelsToUnits", "spriteMeshType", "spriteBorder"):
            m = re.search(rf"{key}: (.*)", text)
            if m:
                print(f"  {key}: {m.group(1)}")
        for m in re.finditer(r"name: (\S+)\n(?:.*\n){0,12}?\s+border: (\{.*\})", text):
            print(f"  sprite {m.group(1)} border {m.group(2)}")

    print(f"row y={row} (x, RGBA) where the colour changes:")
    for x, p in transitions([im.getpixel((x, row)) for x in range(w)]):
        print(f"  x={x} {p}")
    print(f"column x={col} (y, RGBA) where the colour changes:")
    for y, p in transitions([im.getpixel((col, y)) for y in range(h)]):
        print(f"  y={y} {p}")


if __name__ == "__main__":
    main()
