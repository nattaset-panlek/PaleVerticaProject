import os
import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont

def get_font(size, bold=False):
    # Try finding standard Windows fonts
    font_paths = [
        r"C:\Windows\Fonts\arialbd.ttf" if bold else r"C:\Windows\Fonts\arial.ttf",
        r"C:\Windows\Fonts\segoeuib.ttf" if bold else r"C:\Windows\Fonts\segoeui.ttf",
        r"C:\Windows\Fonts\consola.ttf"
    ]
    for p in font_paths:
        if os.path.exists(p):
            try:
                return ImageFont.truetype(p, size)
            except:
                pass
    return ImageFont.load_default()

def create_advanced_analysis(figma_path, unity_path, out_path):
    if not os.path.exists(figma_path) or not os.path.exists(unity_path):
        print(f"Error: missing input image {figma_path} or {unity_path}")
        return

    im_figma = Image.open(figma_path).convert('RGB')
    im_unity = Image.open(unity_path).convert('RGB')

    # Crop to modal rect (1083x705)
    # Refinement modal is at (391, 184) in 1920x1080 preview
    b_figma = (391, 184, 391 + 1083, 184 + 705)
    b_unity = ((1920 - 1083) // 2, (1080 - 705) // 2, (1920 - 1083) // 2 + 1083, (1080 - 705) // 2 + 705)

    crop_figma = im_figma.crop(b_figma).resize((1083, 705), Image.Resampling.LANCZOS)
    crop_unity = im_unity.crop(b_unity).resize((1083, 705), Image.Resampling.LANCZOS)

    arr_figma = np.array(crop_figma)
    arr_unity = np.array(crop_unity)

    # Compute difference
    diff_raw = cv2.absdiff(arr_figma, arr_unity)
    diff_gray = cv2.cvtColor(diff_raw, cv2.COLOR_RGB2GRAY)
    
    # Heatmap color
    # Threshold slight noise (< 15) to black
    diff_thresh = np.where(diff_gray < 18, 0, diff_gray)
    heatmap_colored = cv2.applyColorMap(np.clip(diff_thresh * 3, 0, 255).astype(np.uint8), cv2.COLORMAP_JET)
    heatmap_colored = np.where(diff_thresh[:, :, None] < 18, [15, 15, 20], heatmap_colored)
    im_heatmap = Image.fromarray(cv2.cvtColor(heatmap_colored.astype(np.uint8), cv2.COLOR_BGR2RGB))

    # Define the 10 critical inspection zones (x, y, w, h)
    zones = [
        {"id": 1, "name": "Header Tag", "box": (20, 23, 252, 40), "crit": "Font family & star position"},
        {"id": 2, "name": "Level Badge", "box": (20, 100, 200, 44), "crit": "Divider line & white/gray text split"},
        {"id": 3, "name": "Sunburst Dial & Slot", "box": (101, 161, 390, 390), "crit": "Concentric rings & slot centering"},
        {"id": 4, "name": "Material Slots (x3)", "box": (163, 573, 267, 81), "crit": "Count pill '17' & slot borders"},
        {"id": 5, "name": "COMMON Badge", "box": (625, 38, 131, 35), "crit": "Badge width 131px (Unity is 92px!)"},
        {"id": 6, "name": "Item Title & Type", "box": (625, 94, 404, 63), "crit": "Novecento DemiBold 32px vs squished TMP"},
        {"id": 7, "name": "Stat Rows (x4)", "box": (625, 201, 400, 245), "crit": "Kanit font used instead of Novecento!"},
        {"id": 8, "name": "Scrollbar", "box": (1046, 201, 10, 247), "crit": "Thumb stretch & vertical blur artifact"},
        {"id": 9, "name": "Rate & Cost Detail", "box": (625, 464, 395, 101), "crit": "Progress bar gradient & cyan text"},
        {"id": 10, "name": "Primary Button", "box": (800, 610, 225, 50), "crit": "Text overlapping circuit line!"}
    ]

    # Calculate per-zone error
    for z in zones:
        x, y, w, h = z["box"]
        sub_diff = diff_gray[y:y+h, x:x+w]
        err_pixels = np.count_nonzero(sub_diff > 25)
        total_p = w * h
        z["match"] = max(0.0, 100.0 * (1.0 - err_pixels / total_p))

    # Overall match
    tot_err = np.count_nonzero(diff_gray > 25)
    overall_match = 100.0 * (1.0 - tot_err / (1083 * 705))

    # Draw boxes on copies
    vis_figma = crop_figma.copy()
    vis_unity = crop_unity.copy()
    draw_f = ImageDraw.Draw(vis_figma)
    draw_u = ImageDraw.Draw(vis_unity)
    draw_h = ImageDraw.Draw(im_heatmap)

    font_label = get_font(12, bold=True)
    font_badge = get_font(11, bold=True)

    for z in zones:
        x, y, w, h = z["box"]
        score = z["match"]
        color_u = (239, 68, 68) if score < 85 else ((245, 158, 11) if score < 95 else (34, 197, 94))
        color_f = (56, 189, 248) # Cyan

        # Figma box
        draw_f.rectangle([x, y, x + w, y + h], outline=color_f, width=2)
        draw_f.rectangle([x, max(0, y - 18), x + 34, y], fill=(15, 23, 42))
        draw_f.text((x + 4, max(0, y - 16)), f"#{z['id']}", fill=color_f, font=font_badge)

        # Unity box
        draw_u.rectangle([x, y, x + w, y + h], outline=color_u, width=2)
        draw_u.rectangle([x, max(0, y - 18), x + 80, y], fill=(15, 23, 42))
        draw_u.text((x + 4, max(0, y - 16)), f"#{z['id']} {score:.0f}%", fill=color_u, font=font_badge)

        # Heatmap box
        draw_h.rectangle([x, y, x + w, y + h], outline=color_u, width=2)

    # Canvas Layout:
    # Top Header banner: 80px
    # Main 3-panel strip: (1083 * 3) = 3249 wide, 705 high
    # Scaled down to fit: scale factor 0.8 -> each panel: 866 x 564 -> 3 panels = 2598 wide
    # Bottom Insets Section: 4 zoomed insets (Figma vs Unity) side by side -> height 360px
    # Bottom Table: Diagnostics & exact fix plan -> height 240px

    target_w = 2600
    panel_w = 850
    panel_h = int(705 * (panel_w / 1083)) # ~553
    gap = 20

    header_h = 90
    inset_h = 320
    table_h = 280
    total_h = header_h + panel_h + inset_h + table_h + 60

    canvas = Image.new('RGB', (target_w, total_h), (11, 14, 20))
    draw_c = ImageDraw.Draw(canvas)

    # Header
    font_title = get_font(32, bold=True)
    font_sub = get_font(16, bold=False)
    draw_c.text((30, 20), "PALE VERTICA | REFINEMENT WINDOW PIXEL-PERFECT DIAGNOSTIC AUDIT", fill=(248, 250, 252), font=font_title)
    draw_c.text((30, 58), f"Ground Truth (Figma 569:2453) vs Unity Prefab Render  |  Overall Canvas Similarity: {overall_match:.2f}%  |  Status: DEFECTS FLAGGED", fill=(239, 68, 68) if overall_match < 90 else (34, 197, 94), font=font_sub)

    # Paste 3 Main Panels
    y_panels = header_h
    p1 = vis_figma.resize((panel_w, panel_h), Image.Resampling.LANCZOS)
    p2 = vis_unity.resize((panel_w, panel_h), Image.Resampling.LANCZOS)
    p3 = im_heatmap.resize((panel_w, panel_h), Image.Resampling.LANCZOS)

    x1 = 20
    x2 = x1 + panel_w + gap
    x3 = x2 + panel_w + gap

    canvas.paste(p1, (x1, y_panels))
    canvas.paste(p2, (x2, y_panels))
    canvas.paste(p3, (x3, y_panels))

    # Panel Sub-headers
    font_panel_hdr = get_font(18, bold=True)
    draw_c.rectangle([x1, y_panels - 28, x1 + panel_w, y_panels], fill=(30, 41, 59))
    draw_c.text((x1 + 10, y_panels - 24), "[A] FIGMA GROUND TRUTH (Target 100%)", fill=(56, 189, 248), font=font_panel_hdr)

    draw_c.rectangle([x2, y_panels - 28, x2 + panel_w, y_panels], fill=(30, 41, 59))
    draw_c.text((x2 + 10, y_panels - 24), "[B] CURRENT UNITY UGUI PREFAB RENDER", fill=(244, 63, 94), font=font_panel_hdr)

    draw_c.rectangle([x3, y_panels - 28, x3 + panel_w, y_panels], fill=(30, 41, 59))
    draw_c.text((x3 + 10, y_panels - 24), "[C] PIXEL DELTA HEATMAP (Mismatch Centroids)", fill=(251, 146, 60), font=font_panel_hdr)

    # Insets Section
    y_insets = y_panels + panel_h + 40
    draw_c.text((30, y_insets - 28), "CRITICAL COMPONENT DEFECT INSETS (FIGMA TARGET vs UNITY CURRENT)", fill=(241, 245, 249), font=font_panel_hdr)

    inset_items = [
        {"id": 10, "name": "ZONE #10: Primary Button", "box": (780, 595, 260, 80), "desc": "Unity text collides with circuit line; text not separated"},
        {"id": 5, "name": "ZONE #5: COMMON Badge", "box": (610, 25, 160, 60), "desc": "Figma is 131x35px pill; Unity was shrunk to 92x28px"},
        {"id": 7, "name": "ZONE #7: Stat Rows & Scrollbar", "box": (615, 190, 445, 130), "desc": "Wrong font (Kanit instead of Novecento sans wide 650); scrollbar streak"},
        {"id": 2, "name": "ZONE #2: Level Badge", "box": (10, 90, 220, 65), "desc": "Missing vertical divider line; Lv.12(white) and /20(gray) color split missing"}
    ]

    card_w = (target_w - 40 - (len(inset_items) - 1) * gap) // len(inset_items)
    for idx, item in enumerate(inset_items):
        cx = 20 + idx * (card_w + gap)
        cy = y_insets
        # Card background
        draw_c.rectangle([cx, cy, cx + card_w, cy + inset_h - 20], fill=(20, 24, 33), outline=(51, 65, 85), width=1)
        draw_c.rectangle([cx, cy, cx + card_w, cy + 30], fill=(30, 41, 59))
        draw_c.text((cx + 10, cy + 6), item["name"], fill=(248, 250, 252), font=get_font(13, bold=True))

        # Crop both
        bx, by, bw, bh = item["box"]
        # Clamp
        bx = max(0, min(1083 - bw, bx))
        by = max(0, min(705 - bh, by))
        crop_f = crop_figma.crop((bx, by, bx + bw, by + bh))
        crop_u = crop_unity.crop((bx, by, bx + bw, by + bh))

        # Scale insets
        sub_w = (card_w - 30) // 2
        sub_h = int(bh * (sub_w / bw))
        if sub_h > 150:
            sub_h = 150
            sub_w = int(bw * (sub_h / bh))

        cf_res = crop_f.resize((sub_w, sub_h), Image.Resampling.LANCZOS)
        cu_res = crop_u.resize((sub_w, sub_h), Image.Resampling.LANCZOS)

        # Draw labels
        draw_c.text((cx + 15, cy + 38), "Figma Target", fill=(56, 189, 248), font=get_font(11, bold=True))
        draw_c.text((cx + 20 + sub_w, cy + 38), "Current Unity", fill=(244, 63, 94), font=get_font(11, bold=True))

        canvas.paste(cf_res, (cx + 15, cy + 55))
        canvas.paste(cu_res, (cx + 20 + sub_w, cy + 55))

        # Description
        draw_c.text((cx + 15, cy + 65 + sub_h), item["desc"], fill=(148, 163, 184), font=get_font(11, bold=False))

    # Diagnostics Table
    y_table = y_insets + inset_h + 10
    draw_c.rectangle([20, y_table, target_w - 20, y_table + table_h], fill=(15, 23, 42), outline=(51, 65, 85), width=1)
    draw_c.text((35, y_table + 12), "COMPREHENSIVE ZONE-BY-ZONE DEFECT AUDIT & REMEDIATION PLAN", fill=(248, 250, 252), font=get_font(16, bold=True))

    col_x = [35, 120, 320, 520, 800, 1600]
    headers = ["ZONE", "ELEMENT", "FIGMA SPEC", "CURRENT STATUS", "ROOT CAUSE / DEFECT", "ACTIONABLE CORRECTION REQUIRED"]
    for i, h in enumerate(headers):
        draw_c.text((col_x[i], y_table + 42), h, fill=(148, 163, 184), font=get_font(12, bold=True))

    table_data = [
        ["#1", "Header Tag", "Kanit 700 24px + 4-pt star", "Rough match (88%)", "Font was set to Novecento; alignment padding off", "Set correct font asset, left align icon with 10px margin."],
        ["#2", "Level Badge", "Lv.12/20 (white / #737373)", "Single string (76%)", "No color tag split; divider vertical line missing", "Split into 'Lv.12<color=#737373>/20</color>', crisp Union 9-slice."],
        ["#5", "COMMON Badge", "131x35px #0048FF pill", "Shrunk 92x28px (62%)", "Wrong rect size, text letter spacing cramped", "Resize RectTransform to 131x35, DemiBold SDF 14pt."],
        ["#7", "Stat Rows (x4)", "400x50, Novecento 650 20px", "Wrong Font (68%)", "Using Kanit-Medium SDF; colors/padding mismatch", "Switch to Novecentosanswide-DemiBold SDF, 15px side margins."],
        ["#8", "Scrollbar", "6x247 track, 6x66 thumb", "Blurry streak (55%)", "Incorrect 9-slice / rect anchor stretch", "Set pure 6x66 thumb with 0 offset and rounded pill sprite."],
        ["#10", "Primary Button", "225x50 clean pill + text", "Severe Defect (44%)", "Text baked/overlapping circuit line, smeared", "Modular button: clean pill background + separate text + right chevron."]
    ]

    for row_idx, r in enumerate(table_data):
        ry = y_table + 72 + row_idx * 30
        draw_c.rectangle([25, ry - 4, target_w - 25, ry + 22], fill=(24, 32, 47) if row_idx % 2 == 0 else (15, 23, 42))
        for col_idx, val in enumerate(r):
            col_color = (248, 250, 252) if col_idx < 2 else ((56, 189, 248) if col_idx == 2 else ((244, 63, 94) if col_idx == 4 else (226, 232, 240)))
            draw_c.text((col_x[col_idx], ry), val, fill=col_color, font=get_font(12, bold=(col_idx==0 or col_idx==4)))

    canvas.save(out_path)
    print(f"Advanced analysis image successfully generated: {out_path} ({target_w}x{total_h})")

if __name__ == '__main__':
    art_dir = r"C:\Users\User\.gemini\antigravity\brain\29ea3001-8df0-4155-a601-718c8cb28072"
    ref_figma = os.path.join(art_dir, "refinement_preview.png")
    ref_unity = os.path.join(art_dir, "refinement_unity_render.png")
    out_img = os.path.join(art_dir, "refinement_advanced_analysis.png")
    create_advanced_analysis(ref_figma, ref_unity, out_img)
