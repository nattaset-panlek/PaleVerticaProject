import math

def get_slash_frame_params(f, total_frames=16):
    # f ranges from 0 to 15
    # Progress t from 0 to 1
    t = f / (total_frames - 1)
    
    # Timing phases:
    # 0 to 0.25 (frames 0-4): Rapid expansion of the head
    # 0.25 to 0.65 (frames 4-10): Peak arc and tail catching up
    # 0.65 to 1.0 (frames 10-15): Wisps stretching, fading out
    
    # Head angle: starts at 200 deg, sweeps clockwise through 90 (bottom), 0 (right), to -90 / 270 (top/left)
    # Total head sweep: from 200 deg down to -80 deg (sweeps across 280 degrees!)
    
    if t < 0.25:
        # Snap expansion
        p = (t / 0.25) ** 0.8
        start_ang = 200 - p * 30    # tail moves slightly: 200 -> 170
        end_ang   = 200 - p * 190   # head moves fast: 200 -> 10
        opacity = min(1.0, 0.4 + p * 0.6)
        thickness_scale = 0.5 + p * 0.5
    elif t < 0.65:
        # Full swing and tail catchup
        p = (t - 0.25) / 0.40
        start_ang = 170 - p * 180   # tail sweeps: 170 -> -10
        end_ang   = 10  - p * 90    # head sweeps: 10 -> -80
        opacity = 1.0
        thickness_scale = 1.0 - p * 0.2
    else:
        # Dissipation and fade
        p = (t - 0.65) / 0.35
        start_ang = -10 - p * 60
        end_ang   = -80 - p * 20
        opacity = max(0.0, 1.0 - p * 1.0)
        thickness_scale = max(0.2, 0.8 - p * 0.6)
        
    return {
        "frame": f,
        "start_ang": round(start_ang, 1),
        "end_ang": round(end_ang, 1),
        "span": round(abs(start_ang - end_ang), 1),
        "opacity": round(opacity, 2),
        "thickness": round(thickness_scale, 2)
    }

for f in range(16):
    print(get_slash_frame_params(f))
