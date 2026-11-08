import os
import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont

def get_font(size, bold=False):
    font_paths = [
        r"C:\Windows\Fonts\segoeuib.ttf" if bold else r"C:\Windows\Fonts\segoeui.ttf",
        r"C:\Windows\Fonts\arialbd.ttf" if bold else r"C:\Windows\Fonts\arial.ttf",
        r"C:\Windows\Fonts\consola.ttf"
    ]
    for p in font_paths:
        if os.path.exists(p):
            try:
                return ImageFont.truetype(p, size)
            except:
                pass
    return ImageFont.load_default()

def create_socket_analysis(figma_path, unity_path, out_path):
    if not os.path.exists(figma_path) or not os.path.exists(unity_path):
        print(f"Error: missing input image {figma_path} or {unity_path}")
        return

    im_figma = Image.open(figma_path).convert('RGB')
    im_unity = Image.open(unity_path).convert('RGB')

    b_figma = (394, 189, 394 + 1083, 189 + 705)
    b_unity = ((1920 - 1083) // 2, (1080 - 705) // 2, (1920 - 1083) // 2 + 1083, (1080 - 705) // 2 + 705)

    crop_figma = im_figma.crop(b_figma).resize((1083, 705), Image.Resampling.LANCZOS)
    crop_unity = im_unity.crop(b_unity).resize((1083, 705), Image.Resampling.LANCZOS)

    arr_figma = np.array(crop_figma)
    arr_unity = np.array(crop_unity)

    diff_raw = cv2.absdiff(arr_figma, arr_unity)
    diff_gray = cv2.cvtColor(diff_raw, cv2.COLOR_RGB2GRAY)
    
    # Difference map (clean minimal visualization: white background, subtle red highlight on differences)
    diff_mask = np.clip(diff_gray.astype(np.float32) * 2.5, 0, 255).astype(np.uint8)
    diff_heat = np.full((705, 1083, 3), 255, dtype=np.uint8) # clean white bg
    # Blend difference into light reddish/orange tint for clear visibility
    diff_heat[:, :, 0] = 255
    diff_heat[:, :, 1] = 255 - diff_mask
    diff_heat[:, :, 2] = 255 - diff_mask
    im_heatmap = Image.fromarray(diff_heat)

    zones = [
        {"id": 1, "name": "Header Tag (Socket Mastery)", "box": (17, 18, 266, 40), "crit": "5px pill & hollow star icon"},
        {"id": 2, "name": "Close Button", "box": (1015, 20, 35, 35), "crit": "Centered 24x24 cross icon"},
        {"id": 3, "name": "Rarity Badge (COMMON)", "box": (622, 33, 131, 35), "crit": "131px width & #0048ff blue tint"},
        {"id": 4, "name": "Item Title & Type", "box": (622, 82, 404, 75), "crit": "Dragon Tooth Pendant & Talisman"},
        {"id": 5, "name": "Mandala Dial & Item Slot", "box": (56, 123, 460, 460), "crit": "460x460 Mandala & 79x79 slot"},
        {"id": 6, "name": "Sockets Grid (2x2)", "box": (703, 208, 250, 235), "crit": "4x 90px slots with Gem & Link icons"},
        {"id": 7, "name": "Consumption & Cost", "box": (622, 535, 400, 35), "crit": "Consumption & 5,000 credits"},
        {"id": 8, "name": "Primary Action Button", "box": (732, 605, 290, 50), "crit": "btn_primary_clean with Infuse Socket"}
    ]

    crop_figma.save(os.path.join(os.path.dirname(out_path), "socket_figma_modal.png"))
    crop_unity.save(os.path.join(os.path.dirname(out_path), "socket_unity_modal.png"))

    for z in zones:
        x, y, w, h = z["box"]
        sub_diff = diff_gray[y:y+h, x:x+w]
        err_pixels = np.count_nonzero(sub_diff > 25)
        total_p = w * h
        z["match"] = max(0.0, 100.0 * (1.0 - err_pixels / total_p))
        print(f"Zone {z['id']} ({z['name']}): {z['match']:.2f}%")

    tot_err = np.count_nonzero(diff_gray > 25)
    overall_match = 100.0 * (1.0 - tot_err / (1083 * 705))

    vis_figma = crop_figma.copy()
    vis_unity = crop_unity.copy()
    draw_f = ImageDraw.Draw(vis_figma)
    draw_u = ImageDraw.Draw(vis_unity)
    draw_h = ImageDraw.Draw(im_heatmap)

    font_badge = get_font(11, bold=True)

    for z in zones:
        x, y, w, h = z["box"]
        score = z["match"]
        color_u = (220, 38, 38) if score < 85 else ((217, 119, 6) if score < 92 else (22, 163, 74))
        color_f = (2, 132, 199)

        # Clean thin bounding boxes
        draw_f.rectangle([x, y, x + w, y + h], outline=color_f, width=2)
        draw_f.rectangle([x, max(0, y - 18), x + 34, y], fill=(255, 255, 255), outline=color_f, width=1)
        draw_f.text((x + 4, max(0, y - 16)), f"#{z['id']}", fill=color_f, font=font_badge)

        draw_u.rectangle([x, y, x + w, y + h], outline=color_u, width=2)
        draw_u.rectangle([x, max(0, y - 18), x + 75, y], fill=(255, 255, 255), outline=color_u, width=1)
        draw_u.text((x + 4, max(0, y - 16)), f"#{z['id']} {score:.1f}%", fill=color_u, font=font_badge)

        draw_h.rectangle([x, y, x + w, y + h], outline=(203, 213, 225), width=1)

    canvas_w = 1083 * 3 + 40
    canvas_h = 705 + 320
    # Clean minimal light background (#F8FAFC)
    final_canvas = Image.new('RGB', (canvas_w, canvas_h), (248, 250, 252))
    draw_c = ImageDraw.Draw(final_canvas)

    f_title = get_font(22, bold=True)
    f_sub = get_font(13, bold=False)
    f_panel = get_font(15, bold=True)
    f_score = get_font(20, bold=True)
    f_card = get_font(12, bold=False)
    f_card_bold = get_font(12, bold=True)

    # Header Bar
    draw_c.text((25, 18), "SOCKET WINDOW - UI FIDELITY AUDIT", fill=(15, 23, 42), font=f_title)
    draw_c.text((25, 48), "Figma Ground Truth (Node 2170:1681) vs Unity Prefab Snapshot - Deterministic Layout Verification", fill=(100, 116, 139), font=f_sub)

    # Score Pill
    pill_w = 260
    pill_h = 42
    pill_x = canvas_w - pill_w - 25
    pill_y = 18
    match_bg = (220, 252, 231) if overall_match >= 92 else (254, 243, 199)
    match_border = (134, 239, 172) if overall_match >= 92 else (252, 211, 77)
    match_fg = (21, 128, 61) if overall_match >= 92 else (180, 83, 9)

    draw_c.rounded_rectangle([pill_x, pill_y, pill_x + pill_w, pill_y + pill_h], radius=6, fill=match_bg, outline=match_border, width=1)
    draw_c.text((pill_x + 18, pill_y + 10), f"Overall Match: {overall_match:.2f}%", fill=match_fg, font=f_score)

    y_panels = 80
    p1_x = 10
    p2_x = 1083 + 20
    p3_x = 1083 * 2 + 30

    final_canvas.paste(vis_figma, (p1_x, y_panels + 32))
    final_canvas.paste(vis_unity, (p2_x, y_panels + 32))
    final_canvas.paste(im_heatmap, (p3_x, y_panels + 32))

    # Clean panel headers
    panel_bg = (241, 245, 249)
    panel_border = (226, 232, 240)
    for px, label in [(p1_x, "[1] FIGMA GROUND TRUTH (1083 x 705)"),
                      (p2_x, "[2] UNITY NATIVE SNAPSHOT (Socket Window.prefab)"),
                      (p3_x, "[3] DIFFERENCE MAP (Red = Drift)")]:
        draw_c.rectangle([px, y_panels, px + 1083, y_panels + 30], fill=panel_bg, outline=panel_border, width=1)
        draw_c.text((px + 15, y_panels + 6), label, fill=(51, 65, 85), font=f_panel)

    # Component Audit Matrix
    y_audit = y_panels + 705 + 45
    draw_c.text((25, y_audit), "COMPONENT VERIFICATION BREAKDOWN", fill=(15, 23, 42), font=f_panel)

    grid_y = y_audit + 28
    card_w = (canvas_w - 40) // 4 - 10
    card_h = 60

    for idx, z in enumerate(zones):
        col = idx % 4
        row = idx // 4
        cx = 10 + col * (card_w + 12)
        cy = grid_y + row * (card_h + 10)

        score = z["match"]
        c_badge = (34, 197, 94) if score >= 92 else ((245, 158, 11) if score >= 80 else (239, 68, 68))
        draw_c.rounded_rectangle([cx, cy, cx + card_w, cy + card_h], radius=4, fill=(255, 255, 255), outline=(226, 232, 240), width=1)
        draw_c.rectangle([cx, cy, cx + 5, cy + card_h], fill=c_badge)

        draw_c.text((cx + 14, cy + 6), f"Zone #{z['id']}: {z['name']}", fill=(15, 23, 42), font=f_card_bold)
        draw_c.text((cx + 14, cy + 24), f"Fidelity: {score:.1f}%  |  Box: {z['box'][2]}x{z['box'][3]}px", fill=c_badge, font=f_card_bold)
        draw_c.text((cx + 14, cy + 40), f"{z['crit']}", fill=(100, 116, 139), font=f_card)

    final_canvas.save(out_path)
    print(f"Generated Socket Window Analysis: {out_path} | Overall Match: {overall_match:.2f}%")

if __name__ == '__main__':
    art_dir = r"C:\Users\User\.gemini\antigravity\brain\29ea3001-8df0-4155-a601-718c8cb28072"
    f_path = os.path.join(art_dir, "socket_preview.png")
    u_path = os.path.join(art_dir, "socket_unity_render.png")
    out_path = os.path.join(art_dir, "socket_advanced_analysis.png")
    create_socket_analysis(f_path, u_path, out_path)
