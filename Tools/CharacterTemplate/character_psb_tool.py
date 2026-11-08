import argparse
import json
import os

from PIL import Image, ImageDraw, ImageFont
from psd_tools import PSDImage
from psd_tools.constants import Tag

CANVAS = 400
MAX_CANVAS_H = 480
CANVAS_H = CANVAS
SS = 4

PARTS = [
    {"name": "ArmL", "order": 11, "anchor": (274.49, 128.02), "joint": (245.38, 149.20), "box": (232, 92, 317, 164), "color": (236, 128, 120)},
    {"name": "Head", "order": 10, "anchor": (206.93, 268.54), "joint": (206.97, 168.40), "box": (62, 144, 352, 393), "color": (238, 204, 120)},
    {"name": "Body", "order": 9, "anchor": (206.50, 127.00), "joint": (206.97, 125.72), "box": (149, 59, 264, 195), "color": (122, 168, 228)},
    {"name": "ArmR", "order": 8, "anchor": (139.49, 128.02), "joint": (171.63, 149.20), "box": (97, 92, 182, 164), "color": (178, 86, 82)},
    {"name": "LegL", "order": 7, "anchor": (235.48, 57.00), "joint": (228.31, 83.04), "box": (213, 22, 258, 92), "color": (126, 206, 146)},
    {"name": "LegR", "order": 6, "anchor": (175.38, 57.00), "joint": (174.96, 83.04), "box": (153, 22, 198, 92), "color": (72, 150, 96)},
]
ROOT = (206.97, 168.40)
SHADOW = (207.87, 23.24)
GROUND_Y = 22
OUTLINE = (34, 30, 38, 255)


def top(y):
    return CANVAS_H - y


def shape_layer(part):
    big = Image.new("RGBA", (CANVAS * SS, CANVAS * SS), (0, 0, 0, 0))
    d = ImageDraw.Draw(big)
    fill = part["color"] + (255,)
    w = 7 * SS

    def S(p):
        return (p[0] * SS, top(p[1]) * SS)

    def capsule(a, b, r):
        d.line([S(a), S(b)], fill=OUTLINE, width=int((r * 2 + 7) * SS))
        for p in (a, b):
            c = S(p)
            rr = (r + 3.5) * SS
            d.ellipse([c[0] - rr, c[1] - rr, c[0] + rr, c[1] + rr], fill=OUTLINE)
        d.line([S(a), S(b)], fill=fill, width=int((r * 2 - 7) * SS))
        for p in (a, b):
            c = S(p)
            rr = (r - 3.5) * SS
            d.ellipse([c[0] - rr, c[1] - rr, c[0] + rr, c[1] + rr], fill=fill)

    n = part["name"]
    if n == "Head":
        box = [72 * SS, top(390) * SS, 342 * SS, top(150) * SS]
        d.ellipse(box, fill=fill, outline=OUTLINE, width=w)
    elif n == "Body":
        poly = [(170, 194), (244, 194), (262, 62), (152, 62)]
        d.polygon([S(p) for p in poly], fill=fill)
        d.line([S(p) for p in poly + [poly[0]]], fill=OUTLINE, width=w, joint="curve")
        neck = [S((188, 205)), S((226, 185))]
        d.rectangle([neck[0][0], neck[0][1], neck[1][0], neck[1][1]], fill=fill, outline=OUTLINE, width=w)
    elif n == "ArmL":
        capsule(part["joint"], (298, 110), 13)
    elif n == "ArmR":
        capsule(part["joint"], (116, 110), 13)
    elif n == "LegL":
        capsule(part["joint"], (234, 42), 16)
    elif n == "LegR":
        capsule(part["joint"], (176, 42), 16)
    return big.resize((CANVAS, CANVAS), Image.LANCZOS)


def guide_joints():
    im = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for p in PARTS:
        jx, jy = p["joint"][0], top(p["joint"][1])
        d.ellipse([jx - 7, jy - 7, jx + 7, jy + 7], outline=(255, 0, 200, 255), width=2)
        d.line([jx - 10, jy, jx + 10, jy], fill=(255, 0, 200, 255), width=1)
        d.line([jx, jy - 10, jx, jy + 10], fill=(255, 0, 200, 255), width=1)
        ax, ay = p["anchor"][0], top(p["anchor"][1])
        d.ellipse([ax - 3, ay - 3, ax + 3, ay + 3], fill=(0, 200, 255, 255))
    rx, ry = ROOT[0], top(ROOT[1])
    d.rectangle([rx - 4, ry - 4, rx + 4, ry + 4], outline=(255, 255, 255, 255), width=2)
    return im


def guide_boxes():
    im = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    for p in PARTS:
        x0, y0, x1, y1 = p["box"]
        d.rectangle([x0, top(y1), x1 - 1, top(y0) - 1], outline=p["color"] + (255,), width=1)
    return im


def guide_lines():
    im = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    gy = top(GROUND_Y)
    d.line([0, gy, CANVAS, gy], fill=(255, 120, 0, 255), width=1)
    cx = round(ROOT[0])
    for y in range(0, CANVAS, 8):
        d.line([cx, y, cx, y + 4], fill=(255, 120, 0, 200), width=1)
    sx, sy = SHADOW[0], top(SHADOW[1])
    d.ellipse([sx - 60, sy - 11, sx + 60, sy + 11], outline=(90, 90, 90, 255), width=1)
    d.rectangle([0, 0, CANVAS - 1, CANVAS - 1], outline=(160, 160, 160, 255), width=1)
    return im


def add_layer(group, image, name, opacity=255):
    bbox = image.getbbox()
    if bbox is None:
        return None
    crop = image.crop(bbox)
    return group.create_pixel_layer(crop, name=name, top=bbox[1], left=bbox[0], opacity=opacity)


def build_psd(out_path, reference_psb):
    psd = PSDImage.new("RGBA", (CANVAS, CANVAS), color=(0, 0, 0, 0))
    guide = psd.create_group(name="_Guide", open_folder=False)
    if reference_psb and os.path.exists(reference_psb):
        ref = PSDImage.open(reference_psb)
        sil = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
        acr = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
        for layer in ref.descendants():
            if layer.is_group() or layer.bbox == (0, 0, 0, 0):
                continue
            img = layer.topil().convert("RGBA")
            if layer.parent is not None and layer.parent is not ref and layer.parent.name == "_Guide":
                sil.alpha_composite(img, (layer.left, layer.top))
        for name in ["LegR", "LegL", "ArmR", "Body", "Head", "ArmL"]:
            layer = next(l for l in ref if l.name == name)
            acr.alpha_composite(layer.topil().convert("RGBA"), (layer.left, layer.top))
        add_layer(guide, acr, "_Guide_AcrxerReference", opacity=90)
        add_layer(guide, sil, "_Guide_Silhouette", opacity=80)
    add_layer(guide, guide_boxes(), "_Guide_Boxes")
    add_layer(guide, guide_lines(), "_Guide_Lines")
    add_layer(guide, guide_joints(), "_Guide_Joints")
    guide.visible = False
    for p in reversed(PARTS):
        add_layer(psd, shape_layer(p), p["name"])
    stamp_layer_ids(psd)
    psd.save(out_path)
    return psd


def build_map(out_path, psd_path):
    psd = PSDImage.open(psd_path)
    k = 2
    pad = 40
    legend_w = 470
    W, H = CANVAS * k + pad * 2 + legend_w, CANVAS * k + pad * 2
    im = Image.new("RGBA", (W, H), (245, 243, 240, 255))
    d = ImageDraw.Draw(im)
    font = ImageFont.load_default(size=15)
    small = ImageFont.load_default(size=12)
    title = ImageFont.load_default(size=20)
    ox, oy = pad, pad
    for y in range(0, CANVAS, 10):
        for x in range(0, CANVAS, 10):
            c = (232, 232, 232, 255) if (x // 10 + y // 10) % 2 == 0 else (250, 250, 250, 255)
            d.rectangle([ox + x * k, oy + y * k, ox + (x + 10) * k - 1, oy + (y + 10) * k - 1], fill=c)
    for v in range(0, CANVAS + 1, 50):
        d.line([ox + v * k, oy, ox + v * k, oy + CANVAS * k], fill=(200, 200, 200, 255))
        d.line([ox, oy + v * k, ox + CANVAS * k, oy + v * k], fill=(200, 200, 200, 255))
        d.text((ox + v * k - 8, oy + CANVAS * k + 4), str(v), fill=(120, 120, 120, 255), font=small)
        d.text((ox - 30, oy + CANVAS * k - v * k - 6), str(v), fill=(120, 120, 120, 255), font=small)
    parts_img = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    for name in ["LegR", "LegL", "ArmR", "Body", "Head", "ArmL"]:
        layer = next(l for l in psd if l.name == name)
        parts_img.alpha_composite(layer.topil().convert("RGBA"), (layer.left, layer.top))
    big = parts_img.resize((CANVAS * k, CANVAS * k), Image.NEAREST)
    faded = Image.new("RGBA", big.size, (0, 0, 0, 0))
    faded = Image.blend(faded, big, 0.85)
    im.alpha_composite(faded, (ox, oy))
    d = ImageDraw.Draw(im)
    gy = oy + top(GROUND_Y) * k
    d.line([ox, gy, ox + CANVAS * k, gy], fill=(255, 120, 0, 255), width=2)
    d.text((ox + 6, gy + 4), f"ground y={GROUND_Y} (feet)", fill=(200, 90, 0, 255), font=small)
    cx = ox + ROOT[0] * k
    for y in range(oy, oy + CANVAS * k, 12):
        d.line([cx, y, cx, y + 6], fill=(255, 120, 0, 255), width=1)
    for p in PARTS:
        x0, y0, x1, y1 = p["box"]
        d.rectangle([ox + x0 * k, oy + top(y1) * k, ox + x1 * k, oy + top(y0) * k], outline=tuple(int(c * 0.6) for c in p["color"]) + (255,), width=2)
        jx, jy = ox + p["joint"][0] * k, oy + top(p["joint"][1]) * k
        d.ellipse([jx - 9, jy - 9, jx + 9, jy + 9], outline=(220, 0, 170, 255), width=3)
        ax, ay = ox + p["anchor"][0] * k, oy + top(p["anchor"][1]) * k
        d.ellipse([ax - 5, ay - 5, ax + 5, ay + 5], fill=(0, 170, 230, 255))
        lx, ly = ox + x0 * k + 4, oy + top(y1) * k + 3
        if p["name"] in ("LegL", "LegR"):
            ly = oy + top(y0) * k - 20
        d.text((lx, ly), p["name"], fill=(20, 20, 20, 255), font=font)
    rx, ry = ox + ROOT[0] * k, oy + top(ROOT[1]) * k
    d.rectangle([rx - 6, ry - 6, rx + 6, ry + 6], outline=(0, 0, 0, 255), width=2)
    lx = ox + CANVAS * k + pad
    y = pad
    d.text((lx, y), "Pale Vertica character layer map", fill=(20, 20, 20, 255), font=title)
    y += 34
    lines = [
        "Canvas 400 x 400 px, PPU 100, file .psb",
        "Coordinates: x from left, y from BOTTOM (Unity)",
        "Photoshop y (from top) = 400 - y",
        "",
        "Layer panel, top to bottom (draw order):",
    ]
    for p in PARTS:
        jx, jy = p["joint"]
        lines.append(f"  {p['name']:<5} sort {p['order']:>2}  joint ({jx:.0f}, {jy:.0f})  box {p['box'][0]}-{p['box'][2]} x {p['box'][1]}-{p['box'][3]}")
    lines += [
        "  _Guide (hidden group, never imported)",
        "",
        "Magenta ring = joint (rotation pivot).",
        "   The part MUST cover it with opaque pixels,",
        "   and so must the part it attaches to.",
        "Blue dot = sprite anchor (pivot set by the tool).",
        "Square = character origin (0,0) in the prefab.",
        "Orange line = ground; dashed = center x 207.",
        "",
        "Front view: L = the character's left =",
        "right side of the image; it draws in front.",
        "Head turned slightly left; the game mirrors",
        "the model when walking right.",
        "",
        "1 px = 0.00234 world units; full body ~0.87 u.",
    ]
    for line in lines:
        d.text((lx, y), line, fill=(30, 30, 30, 255), font=small if line.startswith("  ") else font)
        y += 20
    for i, p in enumerate(PARTS):
        d.rectangle([lx + 330, pad + 34 + 100 + i * 20 + 3, lx + 344, pad + 34 + 100 + i * 20 + 15], fill=p["color"] + (255,))
    im.convert("RGB").save(out_path)


SHEET_ORDER = ["Preview", "Head", "Body", "ArmL", "ArmR", "LegL", "LegR"]
GRID_COLOR = (255, 0, 255, 255)
SCREEN_COLOR = (0, 0, 0, 0)


def to_grey(image):
    result = image.copy()
    pixels = result.load()
    for y in range(result.height):
        for x in range(result.width):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue
            v = 70 if (r + g + b) / 3 < 80 else 175
            pixels[x, y] = (v, v, v, a)
    return result


def build_pose_guide(out_path, psd_path, size):
    psd = PSDImage.open(psd_path)
    body = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    for name in ["LegR", "LegL", "ArmR", "Body", "Head", "ArmL"]:
        layer = next(l for l in psd if l.name == name)
        body.alpha_composite(layer.topil().convert("RGBA"), (layer.left, layer.top))
    guide = Image.new("RGBA", (size, size), SCREEN_COLOR)
    guide.alpha_composite(to_grey(body).resize((size, size), Image.LANCZOS))
    guide.save(out_path)


def build_sheet_template(out_path, psd_path, columns, rows, cell):
    psd = PSDImage.open(psd_path)
    shapes = {}
    for layer in psd:
        if layer.is_group():
            continue
        img = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
        img.alpha_composite(layer.topil().convert("RGBA"), (layer.left, layer.top))
        shapes[layer.name] = img
    preview = Image.new("RGBA", (CANVAS, CANVAS), (0, 0, 0, 0))
    for name in ["LegR", "LegL", "ArmR", "Body", "Head", "ArmL"]:
        preview.alpha_composite(shapes[name])
    shapes["Preview"] = preview
    W, H = columns * cell, rows * cell
    sheet = Image.new("RGBA", (W, H), SCREEN_COLOR)
    font = ImageFont.load_default(size=max(12, cell // 22))
    for i in range(columns * rows):
        c, r = i % columns, i // columns
        x0, y0 = c * cell, r * cell
        if i < len(SHEET_ORDER):
            name = SHEET_ORDER[i]
            ghost = to_grey(shapes[name]).resize((cell, cell), Image.LANCZOS)
            sheet.alpha_composite(ghost, (x0, y0))
            d = ImageDraw.Draw(sheet)
            label = f"{i + 1}. {name}" + (" (full character, master)" if name == "Preview" else " (cut from cell 1)")
            d.text((x0 + 10, y0 + cell - max(14, cell // 22) - 8), label, fill=(40, 40, 40, 255), font=font)
            jx, jy = None, None
            for p in PARTS:
                if p["name"] == name:
                    jx, jy = p["joint"][0], top(p["joint"][1])
            if jx is not None:
                k = cell / CANVAS
                cx, cy = x0 + jx * k, y0 + jy * k
                d.ellipse([cx - 6, cy - 6, cx + 6, cy + 6], outline=(255, 0, 200, 255), width=2)
            gy = y0 + top(GROUND_Y) * cell / CANVAS
            d.line([x0, gy, x0 + cell, gy], fill=(255, 140, 0, 160), width=1)
        else:
            d = ImageDraw.Draw(sheet)
            d.text((x0 + 10, y0 + 8), "(leave empty)", fill=(120, 120, 120, 255), font=font)
    d = ImageDraw.Draw(sheet)
    for c in range(columns + 1):
        x = min(W - 2, c * cell)
        d.rectangle([max(0, x - 2), 0, x + 1, H], fill=GRID_COLOR)
    for r in range(rows + 1):
        y = min(H - 2, r * cell)
        d.rectangle([0, max(0, y - 2), W, y + 1], fill=GRID_COLOR)
    sheet.save(out_path)


def stamp_layer_ids(psd, first_id=1001):
    for i, layer in enumerate(psd.descendants()):
        layer.tagged_blocks.set_data(Tag.LAYER_ID, first_id + i)


def alpha_at(image, x, y):
    px, py = int(round(x)), int(round(top(y)))
    if not (0 <= px < image.width and 0 <= py < image.height):
        return 0
    best = 0
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            X, Y = min(max(px + dx, 0), image.width - 1), min(max(py + dy, 0), image.height - 1)
            best = max(best, image.getpixel((X, Y))[3])
    return best


def assemble(parts_dir, name, out_dir):
    global CANVAS_H
    images, problems, notes = {}, [], []
    height = None
    for p in PARTS:
        path = os.path.join(parts_dir, p["name"] + ".png")
        if not os.path.exists(path):
            problems.append(f"missing {p['name']}.png")
            continue
        img = Image.open(path).convert("RGBA")
        height = height or img.height
        if img.width != CANVAS or img.height != height or not CANVAS <= img.height <= MAX_CANVAS_H:
            problems.append(f"{p['name']}.png is {img.size[0]}x{img.size[1]}; every part must be the same {CANVAS} px wide canvas, {CANVAS}-{MAX_CANVAS_H} px tall (extra height only above), with the part drawn in place")
            continue
        if img.getchannel("A").getbbox() is None:
            problems.append(f"{p['name']}.png is empty")
            continue
        images[p["name"]] = img
    if problems:
        for line in problems:
            print("ERROR", line)
        return None
    CANVAS_H = height
    body = images["Body"]
    for p in PARTS:
        img = images[p["name"]]
        if alpha_at(img, *p["joint"]) < 128:
            problems.append(f"{p['name']} does not cover its joint {p['joint'][0]:.0f},{p['joint'][1]:.0f} (y from bottom)")
        if p["name"] != "Body" and alpha_at(body, *p["joint"]) < 128:
            notes.append(f"Body does not cover the {p['name']} joint; a gap can open when it rotates")
    feet = min(CANVAS_H - images[n].getchannel("A").getbbox()[3] for n in ("LegL", "LegR"))
    if abs(feet - GROUND_Y) > 14:
        notes.append(f"feet end at y={feet}; ground is y={GROUND_Y}")
    psd = PSDImage.new("RGBA", (CANVAS, CANVAS_H), color=(0, 0, 0, 0))
    for p in reversed(PARTS):
        img = images[p["name"]]
        bbox = img.getchannel("A").getbbox()
        psd.create_pixel_layer(img.crop(bbox), name=p["name"], top=bbox[1], left=bbox[0])
    stamp_layer_ids(psd)
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, name + ".psb")
    psd.save(out_path)
    for line in problems:
        print("ERROR", line)
    for line in notes:
        print("WARN ", line)
    print("written", out_path)
    return out_path


def find_source_layer(psd, path):
    nodes, layer = list(psd), None
    for name in path.split("/"):
        matches = [l for l in nodes if l.name == name]
        if not matches:
            return None
        layer = matches[0]
        nodes = list(layer) if layer.is_group() else []
    return None if layer is None or layer.is_group() else layer


def scale_part(img, scale):
    size = (max(1, round(img.width * scale)), max(1, round(img.height * scale)))
    return img.convert("RGBa").resize(size, Image.LANCZOS).convert("RGBA")


def place_part(src_img, src_left, src_top, src_height, joint, target, scale):
    jx, jy = joint[0] - src_left, (src_height - joint[1]) - src_top
    part = scale_part(src_img, scale)
    x = round(target[0] - jx * scale)
    y = round(top(target[1]) - jy * scale)
    canvas = Image.new("RGBA", (CANVAS, CANVAS_H), (0, 0, 0, 0))
    canvas.alpha_composite(part, (max(x, 0), max(y, 0)), (max(-x, 0), max(-y, 0)))
    lost = sum(part.getchannel("A").histogram()[1:]) - sum(canvas.getchannel("A").histogram()[1:])
    return canvas, lost, (x, y, x + part.width, y + part.height)


def write_preview(images, out_path):
    preview = Image.new("RGBA", (CANVAS, CANVAS_H), (210, 210, 210, 255))
    for p in sorted(PARTS, key=lambda p: p["order"]):
        preview.alpha_composite(images[p["name"]])
    d = ImageDraw.Draw(preview)
    for p in PARTS:
        x, y = p["joint"]
        d.ellipse([x - 3, top(y) - 3, x + 3, top(y) + 3], outline=(255, 0, 255, 255), width=1)
    d.line([0, top(GROUND_Y), CANVAS, top(GROUND_Y)], fill=(255, 140, 0, 255))
    preview.save(out_path)


def port(spec_path, source_override, name_override, out_dir, work_dir):
    with open(spec_path, encoding="utf-8-sig") as f:
        spec = json.load(f)
    name = name_override or spec["name"]
    source = source_override or os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(spec_path)), spec["source"]))
    scale = float(spec["scale"])
    global CANVAS_H
    CANVAS_H = int(spec.get("canvasHeight", CANVAS))
    if not CANVAS <= CANVAS_H <= MAX_CANVAS_H:
        print(f"ERROR canvasHeight {CANVAS_H} must be {CANVAS}-{MAX_CANVAS_H} (extra room is added above the character only)")
        return None
    if not os.path.exists(source):
        print("ERROR source not found:", source)
        return None
    psd = PSDImage.open(source)
    problems, images = [], {}
    for p in PARTS:
        entry = spec["parts"].get(p["name"])
        if entry is None:
            problems.append(f"spec has no entry for {p['name']}")
            continue
        layer = find_source_layer(psd, entry["layer"])
        if layer is None:
            problems.append(f"{p['name']}: layer '{entry['layer']}' not found in {os.path.basename(source)} (use Group/Layer paths)")
            continue
        img = layer.topil()
        if img is None:
            problems.append(f"{p['name']}: layer '{entry['layer']}' has no pixels")
            continue
        img = img.convert("RGBA")
        part_scale = float(entry.get("scale", scale))
        canvas, lost, rect = place_part(img, layer.left, layer.top, psd.height, entry["joint"], p["joint"], part_scale)
        if lost > 0:
            problems.append(f"{p['name']}: {lost} opaque px fall outside the {CANVAS}x{CANVAS_H} canvas at scale {part_scale} (part spans x {rect[0]}..{rect[2]}, y-from-top {rect[1]}..{rect[3]}); lower the scale")
        if alpha_at(canvas, *p["joint"]) < 128:
            problems.append(f"{p['name']}: source joint {entry['joint']} is not on an opaque pixel of '{entry['layer']}'; move it onto the part")
        images[p["name"]] = canvas
    if problems:
        for line in problems:
            print("ERROR", line)
        print("nothing written")
        return None
    parts_dir = os.path.join(work_dir, name)
    os.makedirs(parts_dir, exist_ok=True)
    for part_name, img in images.items():
        img.save(os.path.join(parts_dir, part_name + ".png"))
    write_preview(images, os.path.join(parts_dir, "_Preview.png"))
    print("parts and preview written to", parts_dir)
    return assemble(parts_dir, name, out_dir)


if __name__ == "__main__":
    ap = argparse.ArgumentParser(description="Pale Vertica character PSB tool")
    sub = ap.add_subparsers(dest="command", required=True)
    t = sub.add_parser("template", help="write Character_Template.psd and Character_LayerMap.png")
    t.add_argument("--out", required=True)
    t.add_argument("--reference", default="")
    t.add_argument("--cell", type=int, default=400)
    a = sub.add_parser("assemble", help="build a standard PSB from six 400x400 part PNGs (ArmL.png, Head.png, Body.png, ArmR.png, LegL.png, LegR.png)")
    a.add_argument("--parts", required=True)
    a.add_argument("--name", required=True)
    a.add_argument("--out", required=True)
    po = sub.add_parser("port", help="build a standard PSB from layers of a PSB made for another rig or project, using a JSON spec (layer path and joint per part, one scale)")
    po.add_argument("--spec", required=True)
    po.add_argument("--source", default="")
    po.add_argument("--name", default="")
    po.add_argument("--out", required=True)
    po.add_argument("--work", required=True)
    args = ap.parse_args()
    if args.command == "template":
        os.makedirs(args.out, exist_ok=True)
        psd_path = os.path.join(args.out, "Character_Template.psd")
        build_psd(psd_path, args.reference)
        build_map(os.path.join(args.out, "Character_LayerMap.png"), psd_path)
        build_sheet_template(os.path.join(args.out, "Character_SheetTemplate_1x1.png"), psd_path, 3, 3, args.cell)
        build_sheet_template(os.path.join(args.out, "Character_SheetTemplate_4x3.png"), psd_path, 4, 3, args.cell)
        build_pose_guide(os.path.join(args.out, "Character_PoseGuide.png"), psd_path, 1024)
        print("written", psd_path)
    elif args.command == "port":
        port(args.spec, args.source, args.name, args.out, args.work)
    else:
        assemble(args.parts, args.name, args.out)
