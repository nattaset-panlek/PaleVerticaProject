import os
import sys
from PIL import Image, ImageChops, ImageDraw

def find_modal_bounds(im):
    # Modal is dark with black border, background is gray #434343
    # Scan from edges to find the modal rectangle
    w, h = im.size
    bg_color = im.getpixel((10, 10))
    
    # We know the modal is 1083x705 in 1920x1080
    # Let's find the left edge
    left, right, top, bottom = 0, w, 0, h
    for x in range(0, w // 2):
        col = [im.getpixel((x, y)) for y in range(h // 4, 3 * h // 4, 10)]
        if any(abs(c[0] - bg_color[0]) > 25 for c in col):
            left = x
            break
            
    for x in range(w - 1, w // 2, -1):
        col = [im.getpixel((x, y)) for y in range(h // 4, 3 * h // 4, 10)]
        if any(abs(c[0] - bg_color[0]) > 25 for c in col):
            right = x + 1
            break
            
    for y in range(0, h // 2):
        row = [im.getpixel((x, y)) for x in range(w // 4, 3 * w // 4, 10)]
        if any(abs(c[0] - bg_color[0]) > 25 for c in row):
            top = y
            break
            
    for y in range(h - 1, h // 2, -1):
        row = [im.getpixel((x, y)) for x in range(w // 4, 3 * w // 4, 10)]
        if any(abs(c[0] - bg_color[0]) > 25 for c in row):
            bottom = y + 1
            break
            
    return left, top, right, bottom

def compare_images(img1_path, img2_path, out_path, title="Comparison", b1=None, b2=None):
    if not os.path.exists(img1_path) or not os.path.exists(img2_path):
        print(f"Missing image: {img1_path} or {img2_path}")
        return 0.0

    im1 = Image.open(img1_path).convert('RGB')
    im2 = Image.open(img2_path).convert('RGB')

    if b1 is None:
        b1 = find_modal_bounds(im1)
    if b2 is None:
        b2 = ((1920 - 1083) // 2, (1080 - 705) // 2, (1920 - 1083) // 2 + 1083, (1080 - 705) // 2 + 705)

    crop1 = im1.crop(b1).resize((1083, 705), Image.Resampling.LANCZOS)
    crop2 = im2.crop(b2).resize((1083, 705), Image.Resampling.LANCZOS)

    diff = ImageChops.difference(crop1, crop2)
    diff_data = diff.getdata()
    total_pixels = len(diff_data)
    different_pixels = sum(1 for p in diff_data if max(p) > 25)
    match_percentage = 100.0 * (1.0 - (different_pixels / total_pixels))

    diff_visual = diff.point(lambda p: min(255, p * 4))

    w, h = crop1.size
    side_by_side = Image.new('RGB', (w * 3, h))
    side_by_side.paste(crop1, (0, 0))
    side_by_side.paste(crop2, (w, 0))
    side_by_side.paste(diff_visual, (w * 2, 0))

    side_by_side.save(out_path)
    print(f"[{title}] Modal Bounds 1: {b1} | Bounds 2: {b2}")
    print(f"[{title}] Crop Match: {match_percentage:.2f}% | Saved: {out_path}")
    return match_percentage

if __name__ == '__main__':
    art_dir = r"C:\Users\User\.gemini\antigravity\brain\29ea3001-8df0-4155-a601-718c8cb28072"
    ref_figma = os.path.join(art_dir, "refinement_preview.png")
    ref_unity = os.path.join(art_dir, "refinement_unity_render.png")
    ref_out = os.path.join(art_dir, "refinement_comparison.png")

    soc_figma = os.path.join(art_dir, "socket_preview.png")
    soc_unity = os.path.join(art_dir, "socket_unity_render.png")
    soc_out = os.path.join(art_dir, "socket_comparison.png")

    compare_images(ref_figma, ref_unity, ref_out, "Refinement", b1=(391, 184, 391 + 1083, 184 + 705))
    compare_images(soc_figma, soc_unity, soc_out, "Socket", b1=(394, 189, 394 + 1083, 189 + 705))
