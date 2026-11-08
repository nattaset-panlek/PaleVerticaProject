"""Compare a Figma export with a Unity render (both 1920x1080).

Usage:
    python -I compare.py figma.png render.png --box x0 y0 x1 y1 [--zoom x0 y0 x1 y1 ...] [--out folder]

Writes:
    blend.png   50/50 mix of the window area: aligned parts look sharp, misaligned parts look doubled
    diff.png    absolute difference (bright = different)
    zoom_N.png  Figma on top, Unity below, 2x, for each --zoom box
Prints the mean difference of the box. It should go down after every fix.
"""
import os
import sys

from PIL import Image, ImageChops


def boxes(args, flag):
    out = []
    i = 0
    while i < len(args):
        if args[i] == flag:
            out.append(tuple(int(v) for v in args[i + 1:i + 5]))
            i += 5
        else:
            i += 1
    return out


def main():
    if len(sys.argv) < 3:
        sys.exit(__doc__)
    figma = Image.open(sys.argv[1]).convert("RGB").resize((1920, 1080))
    render = Image.open(sys.argv[2]).convert("RGB").resize((1920, 1080))
    rest = sys.argv[3:]
    out = rest[rest.index("--out") + 1] if "--out" in rest else "."
    os.makedirs(out, exist_ok=True)

    box = (boxes(rest, "--box") or [(0, 0, 1920, 1080)])[0]
    Image.blend(figma, render, 0.5).crop(box).save(os.path.join(out, "blend.png"))
    diff = ImageChops.difference(figma, render).crop(box)
    diff.save(os.path.join(out, "diff.png"))
    gray = diff.convert("L")
    pixels = gray.width * gray.height
    print(f"mean difference: {sum(gray.histogram()[i] * i for i in range(256)) / pixels:.2f}")

    for n, zoom in enumerate(boxes(rest, "--zoom")):
        w, h = zoom[2] - zoom[0], zoom[3] - zoom[1]
        sheet = Image.new("RGB", (w * 2, h * 4), (255, 0, 255))
        sheet.paste(figma.crop(zoom).resize((w * 2, h * 2)), (0, 0))
        sheet.paste(render.crop(zoom).resize((w * 2, h * 2)), (0, h * 2))
        path = os.path.join(out, f"zoom_{n}.png")
        sheet.save(path)
        print(f"zoom {n}: {path} (top = Figma, bottom = Unity)")


if __name__ == "__main__":
    main()
