import math
import os
import random
from PIL import Image, ImageDraw, ImageFont, ImageFilter

OUTPUT_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
os.makedirs(OUTPUT_DIR, exist_ok=True)
FRAMES_DIR = os.path.join(OUTPUT_DIR, "frames")
os.makedirs(FRAMES_DIR, exist_ok=True)

WIDTH = 640
HEIGHT = 400
FPS = 24
TOTAL_FRAMES = 20

# Palette definitions
COLOR_BG_DARK = (18, 20, 26, 255)
COLOR_FLOOR = (26, 30, 40, 255)
COLOR_GRID = (36, 42, 56, 255)
COLOR_GRID_BRIGHT = (50, 60, 80, 255)

def get_font(size):
    try:
        return ImageFont.truetype("arial.ttf", size)
    except:
        return ImageFont.load_default()

FONT_TITLE = get_font(20)
FONT_SUB = get_font(13)
FONT_DMG = get_font(16)
FONT_CRIT = get_font(18)

def draw_background(d, style_name, subtitle):
    # Gradient/clean floor
    horizon = 270
    d.rectangle([0, 0, WIDTH, horizon], fill=COLOR_BG_DARK)
    d.rectangle([0, horizon, WIDTH, HEIGHT], fill=COLOR_FLOOR)
    d.line([0, horizon, WIDTH, horizon], fill=COLOR_GRID_BRIGHT, width=2)
    
    # Perspective grid lines
    for x in range(-120, WIDTH + 180, 70):
        d.line([x, horizon, x - 90, HEIGHT], fill=COLOR_GRID, width=1)
    for y in [295, 330, 370]:
        d.line([0, y, WIDTH, y], fill=COLOR_GRID, width=1)
        
    # Title & watermark
    d.text((24, 20), style_name, fill=(230, 235, 245, 255), font=FONT_TITLE)
    d.text((24, 46), subtitle, fill=(140, 150, 175, 255), font=FONT_SUB)
    d.text((WIDTH - 150, 22), "Pale Vertica VFX", fill=(80, 95, 120, 255), font=FONT_SUB)

def draw_character(d, frame_idx, swing_type="slash"):
    # Attacker base anchor at (180, 270)
    base_x = 180
    base_y = 270
    
    # Motion states
    # 0..4: Windup (pull back)
    # 5..8: Swing / Thrust (lunge forward)
    # 9..12: Impact & hitstop (peak extension)
    # 13..19: Recovery (slide back)
    
    if frame_idx <= 4:
        # Windup
        t = frame_idx / 4.0
        offset_x = -10 * t
        body_angle = -8 * t
        arm_angle = -45 - 30 * t
        weapon_lead = -50 * t
        leg_spread = 24
    elif frame_idx <= 8:
        # Swing forward!
        t = (frame_idx - 4) / 4.0
        offset_x = -10 + 35 * t
        body_angle = -8 + 24 * t
        arm_angle = -75 + 155 * t
        weapon_lead = -50 + 170 * t
        leg_spread = 24 + 16 * t
    elif frame_idx <= 12:
        # Peak impact / slight recoil
        t = (frame_idx - 8) / 4.0
        offset_x = 25 - 5 * t
        body_angle = 16 - 4 * t
        arm_angle = 80 + 10 * (1 - t)
        weapon_lead = 120
        leg_spread = 38 - 4 * t
    else:
        # Recovery
        t = (frame_idx - 12) / 8.0
        offset_x = 20 * (1 - t)
        body_angle = 12 * (1 - t)
        arm_angle = 80 * (1 - t) - 45 * t
        weapon_lead = 120 * (1 - t)
        leg_spread = 34 - 10 * t

    px = base_x + offset_x
    py = base_y
    
    # Shadow
    d.ellipse([px - 32, py - 8, px + 32, py + 8], fill=(12, 14, 18, 160))
    
    # Legs (HD-2D Knight style)
    boot_col = (45, 50, 65, 255)
    armor_col = (130, 140, 165, 255)
    gold_trim = (210, 175, 75, 255)
    cape_col = (160, 40, 45, 255)
    skin_col = (235, 195, 165, 255)
    cloth_col = (35, 40, 55, 255)
    
    # Left Leg (back)
    d.rectangle([px - leg_spread, py - 36, px - leg_spread + 10, py - 4], fill=boot_col)
    # Right Leg (front)
    d.rectangle([px + leg_spread - 12, py - 38, px + leg_spread - 2, py - 4], fill=boot_col)
    
    # Cape (flowing)
    cape_drift = -16 - (10 if 5 <= frame_idx <= 10 else 0)
    d.polygon([(px - 8, py - 70), (px - 22 + cape_drift, py - 20), (px - 6 + cape_drift, py - 18), (px + 6, py - 65)], fill=cape_col)
    
    # Torso Armor
    d.rectangle([px - 14, py - 72, px + 14, py - 36], fill=armor_col)
    d.rectangle([px - 14, py - 46, px + 14, py - 40], fill=gold_trim)
    d.rectangle([px - 4, py - 72, px + 4, py - 40], fill=gold_trim)
    
    # Head & Helmet
    head_y = py - 92
    d.ellipse([px - 13, head_y, px + 13, head_y + 24], fill=armor_col)
    # Visor slit
    d.rectangle([px - 2, head_y + 9, px + 11, head_y + 13], fill=(15, 18, 24, 255))
    # Helmet Plume
    d.polygon([(px - 6, head_y), (px - 2, head_y - 12), (px + 4, head_y)], fill=cape_col)
    
    # Weapon & Arm
    arm_rad = math.radians(arm_angle)
    shoulder_x = px + 6
    shoulder_y = py - 62
    
    # Hand pos
    hand_len = 28
    hand_x = shoulder_x + math.cos(arm_rad) * hand_len
    hand_y = shoulder_y + math.sin(arm_rad) * hand_len
    
    # Draw Arm
    d.line([shoulder_x, shoulder_y, hand_x, hand_y], fill=armor_col, width=7)
    d.ellipse([hand_x - 4, hand_y - 4, hand_x + 4, hand_y + 4], fill=gold_trim)
    
    # Weapon Blade Tip
    if swing_type == "slash":
        blade_len = 65
        blade_angle = arm_angle + 10
    elif swing_type == "flame":
        blade_len = 75
        blade_angle = arm_angle + 12
    elif swing_type == "dagger":
        blade_len = 40
        blade_angle = arm_angle + 5
    elif swing_type == "spear":
        blade_len = 95
        blade_angle = arm_angle - 25
    elif swing_type == "hammer":
        blade_len = 60
        blade_angle = arm_angle + 15
        
    blade_rad = math.radians(blade_angle)
    tip_x = hand_x + math.cos(blade_rad) * blade_len
    tip_y = hand_y + math.sin(blade_rad) * blade_len
    
    # Draw Weapon
    if swing_type in ["slash", "flame"]:
        # Steel/Flame Sword
        blade_col = (220, 230, 245, 255) if swing_type == "slash" else (255, 170, 70, 255)
        d.line([hand_x, hand_y, tip_x, tip_y], fill=blade_col, width=4)
        # Crossguard
        guard_dx = -math.sin(blade_rad) * 10
        guard_dy = math.cos(blade_rad) * 10
        d.line([hand_x + guard_dx, hand_y + guard_dy, hand_x - guard_dx, hand_y - guard_dy], fill=gold_trim, width=3)
    elif swing_type == "dagger":
        # Dual Daggers
        d.line([hand_x, hand_y, tip_x, tip_y], fill=(190, 130, 255, 255), width=3)
        # Second offhand dagger
        d.line([hand_x - 12, hand_y + 6, hand_x - 12 + math.cos(blade_rad + 0.4) * 35, hand_y + 6 + math.sin(blade_rad + 0.4) * 35], fill=(160, 90, 240, 255), width=3)
    elif swing_type == "spear":
        # Long Spear
        shaft_end_x = hand_x - math.cos(blade_rad) * 25
        shaft_end_y = hand_y - math.sin(blade_rad) * 25
        d.line([shaft_end_x, shaft_end_y, tip_x, tip_y], fill=(150, 110, 75, 255), width=4)
        d.line([tip_x - math.cos(blade_rad) * 20, tip_y - math.sin(blade_rad) * 20, tip_x, tip_y], fill=(180, 225, 255, 255), width=6)
    elif swing_type == "hammer":
        # Warhammer
        d.line([hand_x, hand_y, tip_x, tip_y], fill=(120, 95, 70, 255), width=5)
        # Hammer Head
        head_w = 26
        head_h = 16
        hx = tip_x
        hy = tip_y
        d.rectangle([hx - 12, hy - 14, hx + 14, hy + 14], fill=(180, 170, 150, 255), outline=gold_trim, width=2)
        
    return (px, py), (hand_x, hand_y), (tip_x, tip_y)

def draw_target_dummy(d, frame_idx, hit_start=7, hit_end=12):
    base_x = 420
    base_y = 270
    is_hit = (hit_start <= frame_idx <= hit_end)
    hit_phase = (frame_idx - hit_start) if is_hit else 0
    
    # Shake & knockback on hit
    shake_x = 0
    shake_y = 0
    flash_white = False
    if is_hit:
        shake_x = 10 if hit_phase % 2 == 0 else -6
        shake_y = -3 if hit_phase == 1 else 0
        if hit_phase == 0 or hit_phase == 1:
            flash_white = True
            
    tx = base_x + (14 if is_hit else 0) + shake_x
    ty = base_y + shake_y
    
    # Shadow
    d.ellipse([tx - 26, ty - 6, tx + 26, ty + 6], fill=(12, 14, 18, 150))
    
    # Training Dummy / Monster Body
    wood_col = (255, 255, 255, 255) if flash_white else (160, 110, 70, 255)
    straw_col = (255, 255, 255, 255) if flash_white else (210, 175, 110, 255)
    post_col = (110, 75, 45, 255)
    
    # Central Pole
    d.rectangle([tx - 6, ty - 85, tx + 6, ty - 2], fill=post_col)
    
    # Straw Body
    d.ellipse([tx - 22, ty - 75, tx + 22, ty - 25], fill=straw_col, outline=wood_col, width=2)
    # Rope binds
    d.line([tx - 18, ty - 55, tx + 18, ty - 55], fill=wood_col, width=2)
    d.line([tx - 16, ty - 40, tx + 16, ty - 40], fill=wood_col, width=2)
    
    # Head (Sack)
    d.ellipse([tx - 16, ty - 98, tx + 16, ty - 70], fill=straw_col, outline=wood_col, width=2)
    # Target Bullseye on chest
    target_col = (220, 50, 50, 255) if not flash_white else (255, 200, 200, 255)
    d.ellipse([tx - 8, ty - 58, tx + 8, ty - 42], fill=target_col)
    d.ellipse([tx - 4, ty - 54, tx + 4, ty - 46], fill=(255, 255, 255, 255))
    
    return (tx, ty), is_hit, hit_phase

# ==========================================
# 5 DISTINCT ATTACK VFX PARTICLES
# ==========================================

def render_style_1_razor_slash(frame_idx):
    # Style 1: Steel Razor Crescent Slash (High Speed Physical)
    im = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_background(d, "01. Steel Razor Arc (ฟันดาบกายภาพ)", "Sharp Crescent Trail + Directional Steel Sparks + Hit Starburst")
    
    char_pos, hand_pos, tip_pos = draw_character(d, frame_idx, "slash")
    target_pos, is_hit, hit_phase = draw_target_dummy(d, frame_idx, hit_start=7, hit_end=11)
    
    # VFX Particles
    vfx_layer = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    vd = ImageDraw.Draw(vfx_layer)
    
    # Frames 5..9: Crescent Slash Arc
    if 5 <= frame_idx <= 10:
        arc_t = (frame_idx - 5) / 5.0
        alpha = int(255 * (1.0 - arc_t * 0.7))
        
        # Draw sweeping crescent arc
        center_x = char_pos[0] + 60
        center_y = char_pos[1] - 65
        radius = 110
        
        # Arc sweeps from -60 deg to +75 deg
        start_deg = -70 + arc_t * 30
        end_deg = start_deg + 120 + (1 - arc_t) * 40
        
        # Outer glow cyan ribbon
        vd.arc([center_x - radius - 10, center_y - radius - 10, center_x + radius + 10, center_y + radius + 10],
               start=start_deg, end=end_deg, fill=(90, 210, 255, int(alpha * 0.6)), width=14)
        # Inner razor sharp white core
        vd.arc([center_x - radius, center_y - radius, center_x + radius, center_y + radius],
               start=start_deg + 10, end=end_deg, fill=(255, 255, 255, alpha), width=5)
        # Forward blade tip spark
        tip_rad = math.radians(end_deg)
        tx = center_x + math.cos(tip_rad) * radius
        ty = center_y + math.sin(tip_rad) * radius
        vd.ellipse([tx - 6, ty - 6, tx + 6, ty + 6], fill=(255, 255, 255, alpha))
        
    # Frames 7..14: Hit Impact & Directional Sparks
    if 7 <= frame_idx <= 14:
        sp_t = (frame_idx - 7) / 7.0
        impact_x = 410
        impact_y = 215
        
        # Expanding 4-point star on hit (frames 7..9)
        if frame_idx <= 9:
            star_sz = 35 * (1 - (frame_idx - 7) * 0.3)
            star_col = (255, 255, 255, 255)
            vd.line([impact_x - star_sz, impact_y, impact_x + star_sz, impact_y], fill=star_col, width=4)
            vd.line([impact_x, impact_y - star_sz, impact_x, impact_y + star_sz], fill=star_col, width=4)
            vd.line([impact_x - star_sz * 0.6, impact_y - star_sz * 0.6, impact_x + star_sz * 0.6, impact_y + star_sz * 0.6], fill=(120, 220, 255, 220), width=2)
            vd.line([impact_x - star_sz * 0.6, impact_y + star_sz * 0.6, impact_x + star_sz * 0.6, impact_y - star_sz * 0.6], fill=(120, 220, 255, 220), width=2)
            
        # Directional sparks spraying rightwards
        random.seed(42)
        for i in range(16):
            ang = random.uniform(-45, 45)
            speed = random.uniform(35, 110)
            rad = math.radians(ang)
            dist = speed * sp_t
            sx = impact_x + math.cos(rad) * dist
            sy = impact_y + math.sin(rad) * dist + (sp_t * sp_t * 25) # gravity
            sp_alpha = int(255 * (1.0 - sp_t))
            length = random.uniform(6, 16) * (1.0 - sp_t * 0.5)
            ex = sx + math.cos(rad) * length
            ey = sy + math.sin(rad) * length
            col = (255, 255, 255, sp_alpha) if i % 2 == 0 else (100, 210, 255, sp_alpha)
            vd.line([sx, sy, ex, ey], fill=col, width=2)
            
        # Damage Text Floating
        dmg_y = 175 - sp_t * 35
        d.text((impact_x - 10, dmg_y), "1,480", fill=(255, 240, 200, int(255 * (1 - sp_t * 0.5))), font=FONT_DMG)

    im.alpha_composite(vfx_layer)
    return im

def render_style_2_flame_cleave(frame_idx):
    # Style 2: Blazing Inferno Cleave (Elemental Fire Attack)
    im = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_background(d, "02. Inferno Cleave (ดาบเพลิงปะทุ)", "Molten Fire Trail + Embers Explosion + Ground Heat Scorch")
    
    char_pos, hand_pos, tip_pos = draw_character(d, frame_idx, "flame")
    target_pos, is_hit, hit_phase = draw_target_dummy(d, frame_idx, hit_start=7, hit_end=12)
    
    vfx_layer = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    vd = ImageDraw.Draw(vfx_layer)
    
    # Ground scorch under target
    if frame_idx >= 7:
        sc_t = min(1.0, (frame_idx - 7) / 12.0)
        sc_alpha = int(220 * (1.0 - sc_t * 0.7))
        vd.ellipse([380, 264, 460, 276], fill=(220, 60, 20, sc_alpha))
        vd.ellipse([395, 267, 445, 273], fill=(255, 180, 40, sc_alpha))
        
    # Frames 5..10: Molten Fire Trail
    if 5 <= frame_idx <= 11:
        ft_t = (frame_idx - 5) / 6.0
        alpha = int(255 * (1.0 - ft_t * 0.6))
        center_x = char_pos[0] + 65
        center_y = char_pos[1] - 65
        radius = 115
        start_deg = -65 + ft_t * 25
        end_deg = start_deg + 135
        
        # Wide orange fire ribbon
        vd.arc([center_x - radius - 18, center_y - radius - 18, center_x + radius + 18, center_y + radius + 18],
               start=start_deg, end=end_deg, fill=(230, 70, 20, int(alpha * 0.5)), width=24)
        # Inner yellow hot core
        vd.arc([center_x - radius - 6, center_y - radius - 6, center_x + radius + 6, center_y + radius + 6],
               start=start_deg + 15, end=end_deg, fill=(255, 190, 40, int(alpha * 0.8)), width=12)
        # Core white flame
        vd.arc([center_x - radius, center_y - radius, center_x + radius, center_y + radius],
               start=start_deg + 30, end=end_deg, fill=(255, 255, 210, alpha), width=5)
        
    # Frames 7..16: Fire Explosion & Floating Embers
    if 7 <= frame_idx <= 17:
        exp_t = (frame_idx - 7) / 10.0
        impact_x = 412
        impact_y = 215
        
        # Fireball burst (early frames)
        if frame_idx <= 11:
            fb_r = 16 + 28 * ((frame_idx - 7) / 4.0)
            fb_a = int(230 * (1 - (frame_idx - 7) / 5.0))
            vd.ellipse([impact_x - fb_r, impact_y - fb_r, impact_x + fb_r, impact_y + fb_r], fill=(255, 100, 20, fb_a))
            vd.ellipse([impact_x - fb_r * 0.6, impact_y - fb_r * 0.6, impact_x + fb_r * 0.6, impact_y + fb_r * 0.6], fill=(255, 230, 80, fb_a))
            
        # Floating crackling embers
        random.seed(101)
        for i in range(22):
            ang = random.uniform(-140, 40)
            speed = random.uniform(40, 130)
            rad = math.radians(ang)
            dist = speed * exp_t
            ex = impact_x + math.cos(rad) * dist
            ey = impact_y + math.sin(rad) * dist - (exp_t * 35) # float up!
            em_a = int(255 * (1.0 - exp_t))
            em_sz = random.uniform(2, 5) * (1.0 - exp_t * 0.5)
            col = (255, 240, 100, em_a) if i % 3 == 0 else ((255, 120, 30, em_a) if i % 3 == 1 else (200, 40, 20, em_a))
            vd.ellipse([ex - em_sz, ey - em_sz, ex + em_sz, ey + em_sz], fill=col)
            
        dmg_y = 175 - exp_t * 35
        d.text((impact_x - 12, dmg_y), "2,840 FIRE", fill=(255, 160, 60, int(255 * (1 - exp_t * 0.5))), font=FONT_DMG)

    im.alpha_composite(vfx_layer)
    return im

def render_style_3_shadow_x_cut(frame_idx):
    # Style 3: Void / Assassin Dual Dagger X-Cut (Dark Multi-Hit & Critical)
    im = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_background(d, "03. Void Twin-Fang (เงามีดคู่สังหาร)", "Dual X-Cross Slash + Void Tendrils + Chromatic Critical Star")
    
    char_pos, hand_pos, tip_pos = draw_character(d, frame_idx, "dagger")
    target_pos, is_hit, hit_phase = draw_target_dummy(d, frame_idx, hit_start=6, hit_end=12)
    
    vfx_layer = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    vd = ImageDraw.Draw(vfx_layer)
    
    # Frames 5..10: Fast Twin X-Cut Slashes
    if 5 <= frame_idx <= 11:
        x_t = (frame_idx - 5) / 6.0
        alpha = int(255 * (1.0 - x_t * 0.6))
        cx = 405
        cy = 215
        length = 75 * min(1.0, x_t * 2.0)
        
        # Slash 1: Top-Left to Bottom-Right
        vd.line([cx - length, cy - length * 0.8, cx + length, cy + length * 0.8], fill=(140, 40, 240, int(alpha * 0.6)), width=12)
        vd.line([cx - length, cy - length * 0.8, cx + length, cy + length * 0.8], fill=(230, 180, 255, alpha), width=4)
        
        # Slash 2: Bottom-Left to Top-Right (slight delay)
        if frame_idx >= 6:
            x2_t = (frame_idx - 6) / 5.0
            l2 = 70 * min(1.0, x2_t * 2.0)
            vd.line([cx - l2, cy + l2 * 0.8, cx + l2, cy - l2 * 0.8], fill=(200, 50, 255, int(alpha * 0.6)), width=12)
            vd.line([cx - l2, cy + l2 * 0.8, cx + l2, cy - l2 * 0.8], fill=(255, 230, 255, alpha), width=4)
            
    # Frames 7..16: Critical Hit Shockwave & Void Tendrils
    if 7 <= frame_idx <= 17:
        cr_t = (frame_idx - 7) / 10.0
        cx = 405
        cy = 215
        
        # Expanding purple distortion ring
        ring_r = 10 + 65 * cr_t
        ring_a = int(220 * (1.0 - cr_t))
        vd.ellipse([cx - ring_r, cy - ring_r, cx + ring_r, cy + ring_r], outline=(180, 60, 255, ring_a), width=3)
        
        # Chromatic Critical Flare (Diamond shape)
        if frame_idx <= 11:
            fl_sz = 38 * (1 - (frame_idx - 7) / 5.0)
            vd.polygon([(cx, cy - fl_sz), (cx + fl_sz * 0.4, cy), (cx, cy + fl_sz), (cx - fl_sz * 0.4, cy)], fill=(255, 255, 255, 240))
            vd.polygon([(cx - fl_sz, cy), (cx, cy + fl_sz * 0.4), (cx + fl_sz, cy), (cx, cy - fl_sz * 0.4)], fill=(230, 130, 255, 200))
            
        # Void Tendrils / Motes flying
        random.seed(88)
        for i in range(16):
            ang = random.uniform(0, 360)
            spd = random.uniform(30, 95)
            rad = math.radians(ang)
            dist = spd * cr_t
            vx = cx + math.cos(rad) * dist
            vy = cy + math.sin(rad) * dist
            v_a = int(240 * (1.0 - cr_t))
            v_sz = random.uniform(2, 5) * (1 - cr_t * 0.4)
            vd.ellipse([vx - v_sz, vy - v_sz, vx + v_sz, vy + v_sz], fill=(160, 40, 240, v_a))
            
        # Critical Damage Text
        dmg_y = 175 - cr_t * 35
        d.text((cx - 20, dmg_y), "3,890 CRIT!", fill=(255, 90, 240, int(255 * (1 - cr_t * 0.5))), font=FONT_CRIT)

    im.alpha_composite(vfx_layer)
    return im

def render_style_4_thunder_pierce(frame_idx):
    # Style 4: Lightning Thrust / Spear (High Velocity Piercing)
    im = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_background(d, "04. Thunder Lance (หอกอสุนีบาตทะลวง)", "Linear Thrust Shockwave + Branching Electric Arcs + Penetration Point")
    
    char_pos, hand_pos, tip_pos = draw_character(d, frame_idx, "spear")
    target_pos, is_hit, hit_phase = draw_target_dummy(d, frame_idx, hit_start=6, hit_end=11)
    
    vfx_layer = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    vd = ImageDraw.Draw(vfx_layer)
    
    # Frames 5..9: Piercing Lightning Beam
    if 5 <= frame_idx <= 10:
        bm_t = (frame_idx - 5) / 5.0
        alpha = int(255 * (1.0 - bm_t * 0.5))
        sx = hand_pos[0]
        sy = hand_pos[1]
        ex = sx + 320 * min(1.0, bm_t * 2.5)
        ey = sy
        
        # Outer blue aura
        vd.line([sx, sy, ex, ey], fill=(60, 160, 255, int(alpha * 0.6)), width=16)
        # Inner pure white high-voltage core
        vd.line([sx, sy, ex, ey], fill=(240, 250, 255, alpha), width=5)
        # Piercing arrowhead cone at tip
        vd.polygon([(ex + 18, ey), (ex - 12, ey - 14), (ex - 4, ey), (ex - 12, ey + 14)], fill=(255, 255, 255, alpha))
        
    # Frames 6..15: Branching Electric Arcs
    if 6 <= frame_idx <= 15:
        el_t = (frame_idx - 6) / 9.0
        impact_x = 415
        impact_y = 215
        
        # Generate jagged lightning bolts
        random.seed(frame_idx * 13)
        for b in range(5):
            curr_x = impact_x + random.uniform(-20, 20)
            curr_y = impact_y + random.uniform(-15, 15)
            bolt_pts = [(curr_x, curr_y)]
            for step in range(4):
                curr_x += random.uniform(-35, 35)
                curr_y += random.uniform(-30, 30)
                bolt_pts.append((curr_x, curr_y))
            b_alpha = int(240 * (1.0 - el_t))
            vd.line(bolt_pts, fill=(130, 220, 255, b_alpha), width=2)
            
        # Conical shockwave expanding rightwards
        cone_dist = 60 * el_t
        c_alpha = int(210 * (1.0 - el_t))
        vd.arc([impact_x + cone_dist - 20, impact_y - 35, impact_x + cone_dist + 30, impact_y + 35],
               start=-70, end=70, fill=(100, 200, 255, c_alpha), width=3)
        
        dmg_y = 175 - el_t * 35
        d.text((impact_x - 10, dmg_y), "2,150 ELEC", fill=(130, 220, 255, int(255 * (1 - el_t * 0.5))), font=FONT_DMG)

    im.alpha_composite(vfx_layer)
    return im

def render_style_5_holy_hammer(frame_idx):
    # Style 5: Radiant Judgment / Heavy Hammer (Holy Ground Smash)
    im = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    draw_background(d, "05. Radiant Judgment (ค้อนศักดิ์สิทธิ์ทุบพื้น)", "Heavy Overhead Swing + Ground Fissure + Radial Golden Pillar")
    
    char_pos, hand_pos, tip_pos = draw_character(d, frame_idx, "hammer")
    target_pos, is_hit, hit_phase = draw_target_dummy(d, frame_idx, hit_start=8, hit_end=14)
    
    vfx_layer = Image.new("RGBA", (WIDTH, HEIGHT), (0, 0, 0, 0))
    vd = ImageDraw.Draw(vfx_layer)
    
    # Frames 8..18: Massive Ground Fissure & Golden Ring
    if frame_idx >= 8:
        sm_t = (frame_idx - 8) / 11.0
        impact_x = 410
        impact_y = 268
        
        # Ground fissure cracks
        fissure_alpha = int(240 * (1.0 - sm_t * 0.7))
        vd.line([impact_x - 50, impact_y - 2, impact_x + 60, impact_y + 3], fill=(255, 200, 60, fissure_alpha), width=4)
        vd.line([impact_x - 15, impact_y, impact_x - 35, impact_y + 12], fill=(255, 170, 40, fissure_alpha), width=3)
        vd.line([impact_x + 20, impact_y, impact_x + 45, impact_y - 8], fill=(255, 170, 40, fissure_alpha), width=3)
        
        # Radial expanding gold shockwave on floor
        ring_w = 20 + 90 * sm_t
        ring_h = (20 + 90 * sm_t) * 0.35 # perspective ellipse
        r_alpha = int(230 * (1.0 - sm_t))
        vd.ellipse([impact_x - ring_w, impact_y - ring_h, impact_x + ring_w, impact_y + ring_h], outline=(255, 215, 80, r_alpha), width=3)
        
        # Upward Holy Light Pillar (frames 8..13)
        if frame_idx <= 13:
            pil_t = (frame_idx - 8) / 5.0
            pil_w = 40 * (1.0 - pil_t * 0.4)
            pil_a = int(210 * (1.0 - pil_t))
            vd.rectangle([impact_x - pil_w, 80, impact_x + pil_w, impact_y], fill=(255, 240, 150, int(pil_a * 0.4)))
            vd.rectangle([impact_x - pil_w * 0.4, 60, impact_x + pil_w * 0.4, impact_y], fill=(255, 255, 220, pil_a))
            
        # Rising golden divine sparkles
        random.seed(55)
        for i in range(18):
            rx = impact_x + random.uniform(-50, 50)
            ry = impact_y - random.uniform(10, 160 * sm_t)
            s_a = int(240 * (1.0 - sm_t))
            # 4-point cross spark
            sz = random.uniform(3, 7)
            vd.line([rx - sz, ry, rx + sz, ry], fill=(255, 255, 200, s_a), width=2)
            vd.line([rx, ry - sz, rx, ry + sz], fill=(255, 255, 200, s_a), width=2)
            
        dmg_y = 165 - sm_t * 30
        d.text((impact_x - 24, dmg_y), "4,520 HEAVY!", fill=(255, 225, 90, int(255 * (1 - sm_t * 0.5))), font=FONT_CRIT)

    im.alpha_composite(vfx_layer)
    return im

# ==========================================
# GENERATION ENGINE
# ==========================================

STYLES = [
    {"id": "style_1_razor", "name": "Razor Arc", "func": render_style_1_razor_slash},
    {"id": "style_2_flame", "name": "Inferno Cleave", "func": render_style_2_flame_cleave},
    {"id": "style_3_shadow", "name": "Void Twin-Fang", "func": render_style_3_shadow_x_cut},
    {"id": "style_4_thunder", "name": "Thunder Lance", "func": render_style_4_thunder_pierce},
    {"id": "style_5_holy", "name": "Radiant Judgment", "func": render_style_5_holy_hammer},
]

def main():
    print("Starting VFX Animation Generation...")
    for style in STYLES:
        s_id = style["id"]
        s_name = style["name"]
        print(f"Rendering {s_name} ({TOTAL_FRAMES} frames)...")
        
        frames = []
        for f in range(TOTAL_FRAMES):
            frame_img = style["func"](f)
            frames.append(frame_img)
            
            # Save individual frame
            frame_path = os.path.join(FRAMES_DIR, f"{s_id}_frame_{f:02d}.png")
            frame_img.save(frame_path)
            
        # Export Animated GIF
        gif_path = os.path.join(OUTPUT_DIR, f"{s_id}.gif")
        frames[0].save(
            gif_path,
            save_all=True,
            append_images=frames[1:],
            duration=int(1000 / FPS),
            loop=0
        )
        print(f"-> Generated {gif_path}")
        
        # Export Spritesheet (horizontal strip or 4x5 grid)
        cols = 5
        rows = 4
        sheet_w = WIDTH * cols // 2
        sheet_h = HEIGHT * rows // 2
        sheet = Image.new("RGBA", (sheet_w, sheet_h), (0, 0, 0, 0))
        for idx, frm in enumerate(frames):
            r = idx // cols
            c = idx % cols
            thumb = frm.resize((WIDTH // 2, HEIGHT // 2), Image.LANCZOS)
            sheet.paste(thumb, (c * (WIDTH // 2), r * (HEIGHT // 2)))
        sheet_path = os.path.join(OUTPUT_DIR, f"{s_id}_sheet.png")
        sheet.save(sheet_path)
        print(f"-> Generated Spritesheet {sheet_path}")
        
    print("All VFX styles generated successfully!")

if __name__ == "__main__":
    main()
