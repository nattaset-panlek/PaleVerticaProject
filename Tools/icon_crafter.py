import os
import sys
import json
import math
import argparse
import glob
from PIL import Image, ImageDraw, ImageFilter, ImageStat, ImageChops

def get_hex_from_rgb(rgb):
    return "#{:02X}{:02X}{:02X}".format(rgb[0], rgb[1], rgb[2])

def analyze_image(filepath):
    try:
        with Image.open(filepath) as img:
            img = img.convert("RGBA")
            width, height = img.size
            total_pixels = width * height
            
            alpha = img.getchannel("A")
            bbox = alpha.getbbox()
            
            if not bbox:
                return {
                    "file": os.path.basename(filepath),
                    "path": filepath,
                    "dimensions": [width, height],
                    "is_empty": True
                }
            
            non_zero = alpha.point(lambda p: 255 if p > 0 else 0)
            stat = ImageStat.Stat(non_zero)
            coverage = (stat.mean[0] / 255.0)
            
            bbox_width = bbox[2] - bbox[0]
            bbox_height = bbox[3] - bbox[1]
            aspect_ratio = round(bbox_width / float(bbox_height if bbox_height > 0 else 1), 3)
            
            cropped = img.crop(bbox)
            rgb_img = cropped.convert("RGB")
            
            small_img = rgb_img.resize((64, 64), Image.Resampling.LANCZOS)
            colors = small_img.getcolors(64 * 64)
            
            unique_color_count = len(colors) if colors else 0
            style_guess = "smooth" if unique_color_count > 64 else "flat"
            
            dominant_hex = []
            try:
                quantized = rgb_img.quantize(colors=5)
                palette = quantized.getpalette()
                if palette:
                    for i in range(min(5, len(palette) // 3)):
                        r, g, b = palette[i*3], palette[i*3+1], palette[i*3+2]
                        dominant_hex.append(get_hex_from_rgb((r, g, b)))
            except Exception:
                dominant_hex = ["#FFFFFF"]
            
            filename = os.path.splitext(os.path.basename(filepath))[0]
            tokens = [t.lower() for t in filename.replace("-", "_").split("_") if t]
            
            return {
                "file": os.path.basename(filepath),
                "path": filepath.replace("\\", "/"),
                "tokens": tokens,
                "dimensions": [width, height],
                "bbox": bbox,
                "aspect_ratio": aspect_ratio,
                "coverage": round(coverage, 3),
                "style": style_guess,
                "dominant_colors": dominant_hex
            }
    except Exception as e:
        return {
            "file": os.path.basename(filepath),
            "error": str(e)
        }

def run_analyze(target_dir, output_file):
    results = {}
    search_pattern = os.path.join(target_dir, "**", "*.png")
    files = glob.glob(search_pattern, recursive=True)
    
    for filepath in files:
        data = analyze_image(filepath)
        filename_key = os.path.splitext(os.path.basename(filepath))[0].lower()
        results[filename_key] = data
        
    os.makedirs(os.path.dirname(os.path.abspath(output_file)), exist_ok=True)
    with open(output_file, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=4)
    print(f"Analyzed {len(files)} images. Saved to {output_file}")

def parse_color(color_str, default=(0, 0, 0, 255)):
    if not color_str:
        return default
    if isinstance(color_str, list) or isinstance(color_str, tuple):
        return tuple(color_str)
    color_str = str(color_str).strip()
    if color_str.startswith("#"):
        hex_val = color_str.lstrip("#")
        if len(hex_val) == 6:
            r = int(hex_val[0:2], 16)
            g = int(hex_val[2:4], 16)
            b = int(hex_val[4:6], 16)
            return (r, g, b, 255)
        elif len(hex_val) == 8:
            r = int(hex_val[0:2], 16)
            g = int(hex_val[2:4], 16)
            b = int(hex_val[4:6], 16)
            a = int(hex_val[6:8], 16)
            return (r, g, b, a)
    return default

def create_gradient_layer(width, height, grad_spec):
    layer = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    grad_type = grad_spec.get("type", "linear")
    start_col = parse_color(grad_spec.get("start", "#000000"))
    end_col = parse_color(grad_spec.get("end", "#FFFFFF"))
    
    if grad_type == "radial":
        center_x = grad_spec.get("center_x", width / 2.0)
        center_y = grad_spec.get("center_y", height / 2.0)
        max_dist = grad_spec.get("radius", math.hypot(width, height) / 2.0)
        
        for y in range(height):
            for x in range(width):
                dist = math.hypot(x - center_x, y - center_y)
                factor = min(1.0, max(0.0, dist / max_dist if max_dist > 0 else 0))
                r = int(start_col[0] + (end_col[0] - start_col[0]) * factor)
                g = int(start_col[1] + (end_col[1] - start_col[1]) * factor)
                b = int(start_col[2] + (end_col[2] - start_col[2]) * factor)
                a = int(start_col[3] + (end_col[3] - start_col[3]) * factor)
                layer.putpixel((x, y), (r, g, b, a))
    else:
        angle = math.radians(grad_spec.get("angle", 0))
        cos_a = math.cos(angle)
        sin_a = math.sin(angle)
        max_proj = abs(width * cos_a) + abs(height * sin_a)
        
        for y in range(height):
            for x in range(width):
                proj = (x * cos_a + y * sin_a)
                factor = min(1.0, max(0.0, (proj + max_proj / 2.0) / max_proj if max_proj > 0 else 0))
                r = int(start_col[0] + (end_col[0] - start_col[0]) * factor)
                g = int(start_col[1] + (end_col[1] - start_col[1]) * factor)
                b = int(start_col[2] + (end_col[2] - start_col[2]) * factor)
                a = int(start_col[3] + (end_col[3] - start_col[3]) * factor)
                layer.putpixel((x, y), (r, g, b, a))
    return layer

def draw_star(draw, center_x, center_y, outer_r, inner_r, points, rotation, fill, outline, width):
    angle_step = math.pi / points
    poly_points = []
    curr_angle = math.radians(rotation)
    for i in range(points * 2):
        r = outer_r if i % 2 == 0 else inner_r
        px = center_x + math.cos(curr_angle) * r
        py = center_y + math.sin(curr_angle) * r
        poly_points.append((px, py))
        curr_angle += angle_step
    draw.polygon(poly_points, fill=fill, outline=outline, width=width)

def run_draw(spec_input, output_file, mode="icon"):
    if os.path.exists(spec_input):
        with open(spec_input, "r", encoding="utf-8") as f:
            spec = json.load(f)
    else:
        spec = json.loads(spec_input)
        
    canvas_spec = spec.get("canvas", {})
    width = canvas_spec.get("width", 512)
    height = canvas_spec.get("height", 512)
    
    if mode == "symbol" or spec.get("mode", "").lower() == "symbol":
        bg_color = parse_color(canvas_spec.get("symbol_background", "#00000000"))
        layers_to_draw = spec.get("symbol_layers", None)
        if layers_to_draw is None:
            layers_to_draw = []
            for l in spec.get("layers", []):
                role = l.get("role", "").lower()
                if role in ["background", "bg", "frame", "base", "fx"]:
                    continue
                if l.get("type", "").lower() == "gradient" and role != "symbol":
                    continue
                sym_l = dict(l)
                if sym_l.get("fill"):
                    sym_l["fill"] = sym_l.get("symbol_fill", "#FFFFFF")
                if sym_l.get("outline"):
                    sym_l["outline"] = sym_l.get("symbol_outline", "#FFFFFF")
                layers_to_draw.append(sym_l)
    else:
        bg_color = parse_color(canvas_spec.get("background", "#00000000"))
        layers_to_draw = spec.get("layers", [])
    
    base_img = Image.new("RGBA", (width, height), bg_color)
    
    for layer in layers_to_draw:
        l_type = layer.get("type", "").lower()
        l_img = Image.new("RGBA", (width, height), (0, 0, 0, 0))
        draw = ImageDraw.Draw(l_img)
        
        fill_col = parse_color(layer.get("fill", None), None) if layer.get("fill") else None
        out_col = parse_color(layer.get("outline", None), None) if layer.get("outline") else None
        line_w = layer.get("width", 1)
        
        if l_type == "rect":
            bounds = [layer.get("x", 0), layer.get("y", 0), layer.get("x", 0) + layer.get("w", 100), layer.get("y", 0) + layer.get("h", 100)]
            radius = layer.get("radius", 0)
            if radius > 0:
                draw.rounded_rectangle(bounds, radius=radius, fill=fill_col, outline=out_col, width=line_w)
            else:
                draw.rectangle(bounds, fill=fill_col, outline=out_col, width=line_w)
        elif l_type == "circle" or l_type == "ellipse":
            cx = layer.get("x", width // 2)
            cy = layer.get("y", height // 2)
            rx = layer.get("rx", layer.get("r", 50))
            ry = layer.get("ry", layer.get("r", 50))
            bounds = [cx - rx, cy - ry, cx + rx, cy + ry]
            draw.ellipse(bounds, fill=fill_col, outline=out_col, width=line_w)
        elif l_type == "polygon":
            pts = [tuple(p) for p in layer.get("points", [])]
            if pts:
                draw.polygon(pts, fill=fill_col, outline=out_col, width=line_w)
        elif l_type == "star":
            draw_star(draw, layer.get("x", width // 2), layer.get("y", height // 2), layer.get("outer_r", 100), layer.get("inner_r", 50), layer.get("points", 5), layer.get("rotation", 0), fill_col, out_col, line_w)
        elif l_type == "line":
            pts = [tuple(p) for p in layer.get("points", [])]
            if len(pts) >= 2:
                draw.line(pts, fill=out_col or fill_col, width=line_w, joint=layer.get("joint", None))
        elif l_type == "gradient":
            grad_img = create_gradient_layer(width, height, layer.get("gradient", {}))
            mask_type = layer.get("mask_shape", "rect")
            mask_img = Image.new("L", (width, height), 0)
            mask_draw = ImageDraw.Draw(mask_img)
            if mask_type == "circle":
                cx = layer.get("x", width // 2)
                cy = layer.get("y", height // 2)
                r = layer.get("r", 100)
                mask_draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=255)
            elif mask_type == "rect":
                bounds = [layer.get("x", 0), layer.get("y", 0), layer.get("x", 0) + layer.get("w", width), layer.get("y", 0) + layer.get("h", height)]
                mask_draw.rectangle(bounds, fill=255)
            l_img = Image.composite(grad_img, l_img, mask_img)
        
        blur_val = layer.get("blur", 0)
        if blur_val > 0:
            l_img = l_img.filter(ImageFilter.GaussianBlur(blur_val))
            
        opacity = layer.get("opacity", 1.0)
        if opacity < 1.0:
            alpha = l_img.getchannel("A")
            alpha = alpha.point(lambda p: int(p * opacity))
            l_img.putalpha(alpha)
            
        blend_mode = layer.get("blend", "normal").lower()
        if blend_mode == "add":
            base_img = ImageChops.add(base_img, l_img)
        elif blend_mode == "multiply":
            base_img = ImageChops.multiply(base_img, l_img)
        elif blend_mode == "screen":
            base_img = ImageChops.screen(base_img, l_img)
        else:
            base_img.alpha_composite(l_img)
            
    os.makedirs(os.path.dirname(os.path.abspath(output_file)), exist_ok=True)
    base_img.save(output_file, "PNG")
    print(f"Rendered [{mode.upper()}] saved to {output_file}")

def print_template():
    sample = {
        "canvas": {"width": 512, "height": 512, "background": "#00000000"},
        "layers": [
            {"role": "background", "type": "circle", "x": 256, "y": 256, "r": 220, "fill": "#1F2421", "outline": "#DCE1DE", "width": 8},
            {"role": "background", "type": "gradient", "mask_shape": "circle", "x": 256, "y": 256, "r": 200, "gradient": {"type": "radial", "start": "#FF5500", "end": "#330000"}},
            {"role": "symbol", "type": "star", "x": 256, "y": 256, "outer_r": 150, "inner_r": 60, "points": 6, "rotation": 15, "fill": "#FFCC00", "outline": "#FFFFFF", "width": 4, "blur": 2},
            {"role": "symbol", "type": "star", "x": 256, "y": 256, "outer_r": 140, "inner_r": 55, "points": 6, "rotation": 15, "fill": "#FFFFA0"}
        ]
    }
    print(json.dumps(sample, indent=4))

def main():
    parser = argparse.ArgumentParser(description="AI Icon Crafter CLI Tool")
    subparsers = parser.add_subparsers(dest="command", help="Available commands")
    
    p_analyze = subparsers.add_parser("analyze", help="Analyze PNG assets and output Data.json")
    p_analyze.add_argument("--dir", default="F:/GameProject/Realm of Hero/My project/Assets/Addressable", help="Directory to scan")
    p_analyze.add_argument("--output", default="F:/GameProject/Realm of Hero/My project/Tools/Data.json", help="Output JSON path")
    
    p_draw = subparsers.add_parser("draw", help="Draw icon from JSON specification")
    p_draw.add_argument("--spec", required=True, help="Path to JSON file or inline JSON string")
    p_draw.add_argument("--output", required=True, help="Output PNG path")
    p_draw.add_argument("--mode", choices=["icon", "symbol"], default="icon", help="Render mode: icon (full color) or symbol (white monochrome symbol)")
    
    subparsers.add_parser("template", help="Print a starter JSON template")
    
    args = parser.parse_args()
    if args.command == "analyze":
        run_analyze(args.dir, args.output)
    elif args.command == "draw":
        run_draw(args.spec, args.output, args.mode)
    elif args.command == "template":
        print_template()
    else:
        parser.print_help()

if __name__ == "__main__":
    main()
