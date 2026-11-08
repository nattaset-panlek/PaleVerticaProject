import os
import subprocess
from PIL import Image, ImageOps

CLI_PATH = r"C:\Program Files\EffectCraft\effectcraft-cli.exe"
PROJ_PATH = r"F:\GameProject\Pale Vertica\Tools\VfxPreview\slash_vfx.ecproj"
FRAMES_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview\slash_frames"
OUTPUT_DIR = r"F:\GameProject\Pale Vertica\Tools\VfxPreview"
UNITY_VFX_DIR = r"F:\GameProject\Pale Vertica\Assets\Image\VFX"

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(UNITY_VFX_DIR, exist_ok=True)

TOTAL_FRAMES = 16
FPS = 30
FRAME_SIZE = 256
GRID_COLS = 4
GRID_ROWS = 4
SHEET_SIZE = (GRID_COLS * FRAME_SIZE, GRID_ROWS * FRAME_SIZE) # 1024x1024

def render_frames():
    print(f"Rendering {TOTAL_FRAMES} frames from EffectCraft...")
    frame_images = []
    
    for f in range(TOTAL_FRAMES):
        t = f / float(FPS)
        frame_file = os.path.join(FRAMES_DIR, f"slash_frame_{f:02d}.png")
        cmd = [
            CLI_PATH,
            "--project", PROJ_PATH,
            "render-frame",
            "--time", f"{t:.4f}",
            "--out", frame_file,
            "--transparent"
        ]
        res = subprocess.run(cmd, capture_output=True, text=True)
        if res.returncode != 0:
            print(f"Error rendering frame {f}: {res.stderr}")
        else:
            print(f"Rendered frame {f:02d} ({t:.3f}s)")
            
        im = Image.open(frame_file).convert("RGBA")
        frame_images.append(im)
        
    return frame_images

def build_spritesheet(frames):
    print(f"Assembling {GRID_COLS}x{GRID_ROWS} Sprite Sheet ({SHEET_SIZE[0]}x{SHEET_SIZE[1]})...")
    sheet = Image.new("RGBA", SHEET_SIZE, (0, 0, 0, 0))
    
    for idx, frame in enumerate(frames):
        col = idx % GRID_COLS
        row = idx // GRID_COLS
        x = col * FRAME_SIZE
        y = row * FRAME_SIZE
        sheet.paste(frame, (x, y))
        
    # Save main grayscale sheet
    out_sheet_preview = os.path.join(OUTPUT_DIR, "VFX_Slash_Grayscale_Sheet.png")
    out_sheet_unity = os.path.join(UNITY_VFX_DIR, "VFX_Slash_Grayscale_Sheet.png")
    
    sheet.save(out_sheet_preview)
    sheet.save(out_sheet_unity)
    print(f"Saved Sprite Sheet -> {out_sheet_preview}")
    print(f"Saved Unity Asset -> {out_sheet_unity}")
    
    # Save animated GIF preview (with black background so white glow is visible)
    gif_frames = []
    for f in frames:
        bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (15, 18, 24, 255))
        bg.alpha_composite(f)
        gif_frames.append(bg.convert("RGB"))
        
    gif_path = os.path.join(OUTPUT_DIR, "VFX_Slash_Grayscale_Preview.gif")
    gif_frames[0].save(
        gif_path,
        save_all=True,
        append_images=gif_frames[1:],
        duration=int(1000 / FPS),
        loop=0
    )
    print(f"Saved Animated GIF -> {gif_path}")
    
    # Generate Sample Tint Previews (demonstrating how Unity shaders tint this grayscale base)
    tints = [
        ("Fire_Orange", (255, 130, 30)),
        ("Frost_Cyan", (80, 210, 255)),
        ("Void_Purple", (210, 80, 255)),
        ("Holy_Gold", (255, 220, 70))
    ]
    
    for name, tint_rgb in tints:
        tint_frames = []
        for f in frames:
            r, g, b, a = f.split()
            # Tint grayscale RGB by multiply
            tr = r.point(lambda v: int(v * tint_rgb[0] / 255))
            tg = g.point(lambda v: int(v * tint_rgb[1] / 255))
            tb = b.point(lambda v: int(v * tint_rgb[2] / 255))
            tinted = Image.merge("RGBA", (tr, tg, tb, a))
            
            bg = Image.new("RGBA", (FRAME_SIZE, FRAME_SIZE), (15, 18, 24, 255))
            bg.alpha_composite(tinted)
            tint_frames.append(bg.convert("RGB"))
            
        t_gif_path = os.path.join(OUTPUT_DIR, f"VFX_Slash_Tint_{name}.gif")
        tint_frames[0].save(
            t_gif_path,
            save_all=True,
            append_images=tint_frames[1:],
            duration=int(1000 / FPS),
            loop=0
        )
        print(f"Saved Tint Demo -> {t_gif_path}")

def main():
    frames = render_frames()
    build_spritesheet(frames)
    print("Done! All assets generated successfully.")

if __name__ == "__main__":
    main()
