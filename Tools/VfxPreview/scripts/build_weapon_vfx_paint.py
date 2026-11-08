import argparse
import math
import os
import random

from build_weapon_vfx_art import brush, chunk_layers, rotate, scale_pts, sub_path, twos
from vfx_effectcraft import (FRAMES, SIZE, TOOLS_DIR, PROJECT_ROOT, build, clamp01, crescent, ease_in_out_cubic,
                             ease_out_back, ease_out_expo, echo, export_to_unity, keyed, life, multi_layer, poly_layer,
                             shape_layer, star, turbulent)

SET_NAME = "WeaponPainterly"
C = SIZE * 0.5
DARK = 0.05
MID = 0.38
BRIGHT = 0.72
CORE = 1.0


def cool(start_value, end_value, born, length):
    return keyed(lambda f: round(start_value + (end_value - start_value) * ease_in_out_cubic(life(f, born, length)), 3))


def visible(born, until):
    return keyed(lambda f: 100.0 if born <= f <= until else 0.0)


def fan(seed, spikes, dir_deg, spread, r_base, len_min, len_max, width=0.55, sx=1.0, sy=1.0, grow=1.0, erode=0.0, base_sy=0.45):
    rng = random.Random(seed)
    tips = []
    for i in range(spikes):
        t = (i + 0.5) / spikes
        a = dir_deg - spread / 2 + spread * t + rng.uniform(-spread / spikes * 0.35, spread / spikes * 0.35)
        centre_bias = 1 - abs(t - 0.5) * 1.1
        ln = rng.uniform(len_min, len_max) * (0.55 + 0.45 * centre_bias)
        w = spread / spikes * width * rng.uniform(0.6, 1.2)
        tips.append((a, ln, w))
    pts = []
    a_start = math.radians(dir_deg - spread / 2 - 6)
    pts.append([r_base * math.cos(a_start), r_base * base_sy * math.sin(a_start)])
    for a, ln, w in tips:
        ln_e = ln * grow * (1 - 0.45 * erode)
        w_e = w * 2.4 * (1 - 0.6 * erode)
        shoulder = r_base + ln_e * 0.32
        al, ar = math.radians(a - w_e / 2), math.radians(a + w_e / 2)
        tip = math.radians(a + rng.uniform(-3, 3))
        lean = rng.uniform(-0.25, 0.25)
        pts.append([r_base * 0.8 * math.cos(al), r_base * 0.8 * math.sin(al)])
        pts.append([shoulder * math.cos(al + w_e * lean * 0.01), shoulder * math.sin(al + w_e * lean * 0.01)])
        pts.append([(r_base + ln_e) * math.cos(tip), (r_base + ln_e) * math.sin(tip)])
        pts.append([shoulder * 0.92 * math.cos(ar), shoulder * 0.92 * math.sin(ar)])
        pts.append([r_base * 0.8 * math.cos(ar), r_base * 0.8 * math.sin(ar)])
    a_end = math.radians(dir_deg + spread / 2 + 6)
    pts.append([r_base * math.cos(a_end), r_base * base_sy * math.sin(a_end)])
    back = math.radians(dir_deg + 180)
    pts.append([r_base * 0.6 * math.cos(back), r_base * 0.6 * base_sy * math.sin(back)])
    return [[round(x * sx, 2), round(y * sy, 2)] for x, y in pts]


def fan_layer(name, seed, spikes, dir_deg, spread, r_base, len_min, len_max, born, hold, erode_len, value, center=None, width=0.55, sx=1.0, sy=1.0, ragged=True, boil=True, base_sy=0.45):
    def path(f):
        if f < born:
            return fan(seed, spikes, dir_deg, spread, r_base * 0.2, 0.2, 0.3, width, sx, sy, base_sy=base_sy)
        q = f if f <= born + 1 else twos(f)
        g = ease_out_back(life(f, born, 2), 2.0)
        e = ease_in_out_cubic(life(q, born + hold, erode_len))
        s = seed * 13 + (q // 2 if boil else 0)
        return fan(s, spikes, dir_deg, spread, r_base * (1 + 0.15 * e), len_min, len_max, width, sx, sy, g, e, base_sy)
    fx = [turbulent(4, 12, seed, 2, 60)] if ragged else []
    return poly_layer(name, keyed(path), visible(born, born + hold + erode_len), center=center or [C, C], fx=fx, value=value)


def halftone(name, seed, center, born, until, radius, count, dot, spread_x=1.0, spread_y=1.0, value=MID, drift=(0.0, 0.0)):
    rng = random.Random(seed)
    shapes = []
    for i in range(count):
        a = rng.uniform(0, 2 * math.pi)
        r = radius * math.sqrt(rng.uniform(0, 1))
        cx, cy = r * math.cos(a) * spread_x, r * math.sin(a) * spread_y
        d = dot * (1 - r / radius * 0.6) * rng.uniform(0.8, 1.1)
        shapes.append([[round(cx + d * math.cos(2 * math.pi * k / 8), 2), round(cy + d * math.sin(2 * math.pi * k / 8), 2)] for k in range(8)])
    pos = keyed(lambda f: [round(center[0] + drift[0] * life(f, born, until - born), 2), round(center[1] + drift[1] * life(f, born, until - born), 2)])
    scl = keyed(lambda f: [0, 0] if f < born else [round(100 * ease_out_back(life(f, born, 2)) * (1 - 0.7 * life(twos(f), born + 2, until - born - 2)), 1)] * 2)
    return multi_layer(name, shapes, pos, visible(born, until), scl, None, value=value)


def smoke(name, seed, base, born, length, height, width, value=0.12):
    rng = random.Random(seed)
    sway = rng.uniform(-18, 18)
    def path(f):
        t = ease_out_expo(life(twos(f), born, length))
        top = height * (0.3 + 0.7 * t)
        bottom = top * 0.6 * t
        line = [[sway * (k / 15) ** 1.5 * t + 6 * math.sin(k * 0.6 + t * 3), -(bottom + (top - bottom) * k / 15)] for k in range(16)]
        return brush(line, width * (1 - 0.5 * t) + 2, 0.4, 1.1, 0.8)
    return poly_layer(name, keyed(path), visible(born, born + length), center=base, fx=[turbulent(10, 14, seed, 2, 50)], value=value)


def ripple(name, center, born, length, r0, r1, thick, sy=0.32, value=MID):
    def path(f):
        t = ease_out_expo(life(twos(f), born, length))
        r = r0 + (r1 - r0) * t
        pts = crescent(r, thick * (1 - t) + 0.8, 200 + 40 * t, 520 - 30 * t, peak=0.5, bias=0.5, n=40)
        return scale_pts(pts, 1.0, sy)
    return poly_layer(name, keyed(path), visible(born, born + length), center=center, value=value)


def chips(prefix, count, seed, origin, born, cone, width, dist, size, value, life_frames=8, gravity=0.0):
    layers = chunk_layers(prefix, count, seed, origin, born, cone, width, dist, size, life_frames, gravity)
    for layer in layers:
        layer["color"] = [value, value, value]
    return layers


def paint_fist_jab():
    layers = []
    layers.append(halftone("00_Dots", 4, [C + 30, C + 10], 2, 10, 46, 38, 2.6, 1.3, 0.9, DARK, (16, 6)))
    layers.append(fan_layer("01_Shadow", 11, 9, 0, 170, 12, 55, 100, 1, 3, 7, DARK, sx=1.15))
    layers.append(fan_layer("02_Mid", 12, 8, 0, 150, 10, 40, 78, 1, 2, 6, cool(MID, 0.22, 3, 6), sx=1.1))
    layers.append(fan_layer("03_Bright", 13, 7, 0, 130, 8, 28, 58, 1, 2, 5, cool(BRIGHT + 0.15, MID, 2, 6)))
    layers.append(fan_layer("04_Core", 14, 6, 0, 360, 4, 10, 26, 1, 1, 3, CORE, ragged=False))
    rng = random.Random(15)
    for i in range(4):
        y = rng.uniform(-22, 22)
        def path(f, y=y, i=i):
            t = ease_out_expo(life(f, 0, 4))
            head = -14 - 50 * (1 - t)
            tail = head - 70 * (1 - 0.7 * t)
            line = [[tail + (head - tail) * k / 15, y * (1 - 0.3 * k / 15)] for k in range(16)]
            return brush(line, 6.5 * (1 - 0.5 * t), 0.8, 1.0, 1.4)
        layers.append(poly_layer("05_Tear_%d" % i, keyed(path), visible(0, 4), fx=[turbulent(4, 6, i)], value=BRIGHT if i % 2 else DARK))
    layers += chips("06_ChipDark", 5, 16, [C, C], 3, 0, 120, 80, 7.0, DARK)
    layers += chips("07_ChipLit", 4, 17, [C, C], 2, -10, 100, 70, 5.0, BRIGHT)
    return layers


def paint_fist_smash():
    ground = [C, C + 40]
    layers = []
    def splat(f):
        t = ease_out_expo(life(f, 1, 4))
        rng = random.Random(30 + twos(f) // 4)
        pts = []
        for k in range(28):
            a = 2 * math.pi * k / 28
            r = (70 + rng.uniform(-14, 18) * (1 if k % 2 else 0.4)) * (0.3 + 0.7 * t)
            pts.append([round(r * math.cos(a) * 1.35, 2), round(r * math.sin(a) * 0.3, 2)])
        return pts
    layers.append(poly_layer("00_Splat", keyed(splat), visible(1, 14), center=ground, fx=[turbulent(6, 10, 3)], value=DARK))
    layers.append(ripple("01_Ripple", ground, 6, 9, 30, 110, 7, value=0.3))
    for k in range(3):
        layers.append(smoke("02_Smoke_%d" % k, 40 + k, [ground[0] + (k - 1) * 34, ground[1] - 6], 4 + k, 11, 110 - k * 15, 26))
    layers.append(halftone("03_Dots", 41, [C + 44, C - 6], 2, 11, 40, 34, 2.8, 1.0, 1.3, MID, (10, -14)))
    layers.append(fan_layer("04_Shadow", 42, 11, -90, 200, 16, 70, 125, 1, 3, 8, DARK, center=ground, width=0.5, base_sy=0.25))
    layers.append(fan_layer("05_Mid", 43, 9, -90, 170, 14, 50, 95, 1, 2, 7, cool(MID + 0.1, 0.2, 3, 7), center=ground, base_sy=0.25))
    layers.append(fan_layer("06_Bright", 44, 7, -90, 130, 10, 34, 72, 1, 2, 6, cool(BRIGHT + 0.2, MID, 2, 6), center=ground, base_sy=0.25))
    layers.append(fan_layer("07_Core", 45, 5, -90, 90, 6, 18, 44, 1, 1, 4, CORE, center=ground, ragged=False, base_sy=0.25))
    layers += chips("08_Ember", 6, 46, ground, 2, -90, 140, 95, 5.0, BRIGHT, 10, 40)
    layers += chips("09_Rock", 5, 47, ground, 1, -90, 170, 80, 8.0, DARK, 9, 70)
    return layers


def paint_magic_burst():
    layers = []
    center = [C, C + 18]
    layers.append(ripple("00_Ripple", center, 5, 10, 24, 105, 8, value=cool(BRIGHT, 0.25, 6, 8)))
    layers.append(ripple("01_Ripple2", center, 8, 7, 14, 70, 5, value=MID))
    rng = random.Random(50)
    for i in range(6):
        r = rng.uniform(52, 82)
        a0 = rng.uniform(0, 360)
        span = rng.uniform(110, 200)
        sy = rng.uniform(0.45, 0.7)
        tilt = rng.uniform(-18, 18)
        def path(f, r=r, a0=a0, span=span, sy=sy, tilt=tilt):
            q = f if f < 3 else twos(f)
            t = ease_out_expo(life(q, 0, 9))
            a = a0 + 140 * t
            pts = crescent(r * (0.6 + 0.4 * t), 9 * (1 - 0.6 * life(q, 4, 8)) + 1, a, a + span * (0.4 + 0.6 * t), peak=0.75, bias=0.5, n=40)
            return rotate(scale_pts(pts, 1.0, sy), tilt)
        layers.append(poly_layer("02_Ribbon_%d" % i, keyed(path), visible(0, 12), center=[C, C], fx=[turbulent(3, 8, i)], value=DARK))
    def dome(f, s):
        g = ease_out_back(life(f, 1, 3), 1.6) * (1 - 0.8 * ease_in_out_cubic(life(f, 6, 6)))
        pts = [[round(s * g * math.cos(math.pi + math.pi * k / 24) * 1.25, 2), round(s * g * math.sin(math.pi + math.pi * k / 24), 2)] for k in range(25)]
        return pts + [[round(s * g * 1.25 * math.cos(math.pi * k / 8) * 0.95, 2), round(s * g * 0.18 * math.sin(math.pi * k / 8), 2)] for k in range(9)]
    layers.append(poly_layer("03_DomeBody", keyed(lambda f: dome(f, 40)), visible(1, 12), center=center, fx=[turbulent(4, 10, 7)], value=cool(BRIGHT, MID, 4, 7)))
    layers.append(poly_layer("04_DomeCore", keyed(lambda f: dome(f, 22)), visible(1, 10), center=[center[0] + 6, center[1] - 4], value=CORE))
    for i in range(4):
        a = rng.uniform(-150, -30)
        ln = rng.uniform(46, 70)
        def path(f, a=a, ln=ln):
            t = ease_out_expo(life(f, 1, 4))
            pts = [[0, -4], [ln * t, 0], [0, 4]]
            return rotate(pts, a)
        layers.append(poly_layer("05_Shard_%d" % i, keyed(path), visible(1, 6), center=center, value=cool(CORE, MID, 2, 4)))
    layers += chips("06_Speck", 7, 51, center, 2, -90, 260, 90, 4.0, DARK, 11)
    return layers


def paint_magic_comet():
    layers = []
    impact = [C + 18, C + 26]
    start = [C - 90, C - 100]
    def head(f):
        t = ease_in_out_cubic(life(f, 0, 5))
        return [start[0] + (impact[0] - start[0]) * t, start[1] + (impact[1] - start[1]) * t]
    angle = math.degrees(math.atan2(impact[1] - start[1], impact[0] - start[0]))
    def comet(f, length, width):
        h = head(min(f, 5))
        line = [[h[0] - math.cos(math.radians(angle)) * length * (1 - k / 15) - C, h[1] - math.sin(math.radians(angle)) * length * (1 - k / 15) - C] for k in range(16)]
        return brush(line, width, 0.82, 0.6, 1.8)
    layers.append(poly_layer("00_Tail", keyed(lambda f: comet(f, 70, 34)), visible(0, 5), fx=[turbulent(9, 10, 1)], value=DARK))
    layers.append(poly_layer("01_Body", keyed(lambda f: comet(f, 50, 24)), visible(0, 5), fx=[turbulent(5, 9, 2)], value=MID))
    layers.append(poly_layer("02_Core", keyed(lambda f: comet(f, 30, 13)), visible(0, 5), value=CORE))
    layers.append(fan_layer("03_Shadow", 61, 10, -90, 190, 14, 60, 105, 5, 3, 7, DARK, center=impact, base_sy=0.3))
    layers.append(fan_layer("04_Mid", 62, 8, -90, 160, 12, 44, 82, 5, 2, 6, cool(MID + 0.15, 0.2, 7, 6), center=impact, base_sy=0.3))
    layers.append(fan_layer("05_Bright", 63, 6, -95, 120, 8, 30, 64, 5, 2, 5, cool(CORE, MID, 6, 6), center=impact, base_sy=0.3))
    layers.append(halftone("06_Dots", 64, [impact[0] + 36, impact[1] - 24], 6, 13, 36, 30, 2.6, 1.0, 1.2, DARK, (12, -12)))
    layers.append(ripple("07_Ripple", impact, 8, 8, 20, 90, 7, value=MID))
    layers += chips("08_Spark", 6, 65, impact, 5, -90, 160, 90, 4.5, BRIGHT, 9, 30)
    return layers


def paint_spear_thrust():
    head = keyed(lambda f: -100 + 150 * ease_out_expo(life(f, -0.5, 3)))
    tail = keyed(lambda f: -125 + 160 * ease_in_out_cubic(life(twos(f), 1, 9)))
    span = lambda f: clamp01((head[f] - tail[f]) / 110.0)
    def lance(f, w, offset=0.0, y=0.0):
        line = [[tail[f] + (head[f] + offset - tail[f]) * k / 19, y] for k in range(20)]
        return brush(line, w * (0.4 + 0.6 * span(f)), 0.88, 0.5, 2.0)
    layers = [
        poly_layer("00_Shadow", keyed(lambda f: lance(f, 30, 8, 4)), visible(0, 9), fx=[turbulent(6, 8, 1)], value=DARK),
        poly_layer("01_Mid", keyed(lambda f: lance(f, 18, 4)), visible(0, 8), fx=[turbulent(4, 8, 2)], value=cool(MID + 0.1, 0.2, 3, 6)),
        poly_layer("02_Core", keyed(lambda f: lance(f, 7)), visible(0, 6), value=CORE),
    ]
    rng = random.Random(70)
    for i in range(4):
        y = rng.uniform(-24, 24)
        def path(f, y=y):
            q = f if f < 3 else twos(f)
            t = life(q, 0, 8)
            h = head[q] - 20 - abs(y)
            tl = tail[q] + 30
            line = [[tl + (h - tl) * k / 15, y * (1 + 0.4 * t)] for k in range(16)]
            return brush(line, 4.5 * (1 - 0.6 * t), 0.75, 1.2, 1.4)
        layers.append(poly_layer("03_Tear_%d" % i, keyed(path), visible(0, 8), fx=[turbulent(4, 5, 10 + i)], value=BRIGHT if i % 2 else DARK))
    tip = [C + 50, C]
    layers.append(fan_layer("04_TipShadow", 71, 7, 0, 110, 8, 30, 62, 2, 2, 6, DARK, center=tip))
    layers.append(fan_layer("05_TipBright", 72, 5, 0, 80, 6, 20, 42, 2, 1, 5, cool(CORE, MID, 3, 5), center=tip))
    layers.append(halftone("06_Dots", 73, [C + 20, C + 22], 2, 10, 34, 28, 2.4, 1.6, 0.6, MID, (20, 4)))
    layers += chips("07_Chip", 5, 74, tip, 3, 0, 90, 60, 5.0, DARK)
    return layers


def paint_sword_slash():
    r = 84
    head = keyed(lambda f: -60 + 210 * ease_out_expo(life(f, -0.6, 4)))
    tail = keyed(lambda f: -56 + 200 * ease_in_out_cubic(life(twos(f) if f > 2 else f, 1.5, 10)))
    thick = lambda f: clamp01((head[f] - tail[f]) / 160.0)
    center = [C - 10, C - 10]
    def tier(f, radius, w, bias, cut=0.0):
        return crescent(radius, w * (0.35 + 0.65 * thick(f)), tail[f] + (head[f] - tail[f]) * cut, head[f], peak=0.8, bias=bias, n=48)
    layers = [
        poly_layer("00_Shadow", keyed(lambda f: tier(f, r - 4, 40, 0.35)), visible(0, 11), center=center, fx=[turbulent(7, 9, 1), echo(1)], value=DARK),
        poly_layer("01_Mid", keyed(lambda f: tier(f, r, 26, 0.55, 0.1)), visible(0, 10), center=center, fx=[turbulent(5, 9, 2)], value=cool(MID + 0.1, 0.2, 4, 6)),
        poly_layer("02_Bright", keyed(lambda f: tier(f, r + 4, 13, 0.75, 0.3)), visible(0, 8), center=center, fx=[turbulent(3, 9, 3)], value=cool(BRIGHT + 0.15, MID, 3, 5)),
        poly_layer("03_Core", keyed(lambda f: tier(f, r + 7, 5, 0.9, 0.5)), visible(0, 6), center=center, value=CORE),
    ]
    def streak(f, w, value_off=0.0):
        t = ease_out_expo(life(f, 1, 4))
        a = math.radians(32)
        x0, x1 = -120 + 20 * life(twos(f), 4, 8) * 4, -120 + 230 * t
        line = [[(x0 + (x1 - x0) * k / 19) * math.cos(a), (x0 + (x1 - x0) * k / 19) * math.sin(a) + 30] for k in range(20)]
        return brush(line, w * (1 - 0.5 * life(f, 4, 6)), 0.7, 1.4, 1.2)
    layers.append(poly_layer("04_StreakShadow", keyed(lambda f: streak(f, 15)), visible(1, 9), fx=[turbulent(9, 6, 4)], value=DARK))
    layers.append(poly_layer("05_Streak", keyed(lambda f: streak(f, 7)), visible(1, 8), fx=[turbulent(6, 6, 5)], value=cool(BRIGHT, MID, 4, 5)))
    hx = center[0] + r * math.cos(math.radians(150))
    hy = center[1] + r * math.sin(math.radians(150))
    layers += chips("06_Ember", 7, 80, [hx, hy], 3, 150, 120, 70, 4.0, BRIGHT, 9, 20)
    return layers


RECIPES = {
    "VFX_Fist_Jab_Paint": paint_fist_jab,
    "VFX_Fist_Smash_Paint": paint_fist_smash,
    "VFX_Magic_Burst_Paint": paint_magic_burst,
    "VFX_Magic_Comet_Paint": paint_magic_comet,
    "VFX_Spear_Thrust_Paint": paint_spear_thrust,
    "VFX_Sword_Slash_Paint": paint_sword_slash,
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("names", nargs="*", help="recipe names (default: all)")
    parser.add_argument("--force", action="store_true", help="regenerate the .ecproj even if it exists (discards hand edits)")
    parser.add_argument("--unity", action="store_true", help="copy the sheets into Assets/Image/VFX/Weapon")
    args = parser.parse_args()
    for name in args.names or list(RECIPES):
        sheet = build(name, RECIPES[name](), SET_NAME, args.force)
        print("built", name)
        if args.unity:
            print("  unity", export_to_unity(sheet))


if __name__ == "__main__":
    main()
