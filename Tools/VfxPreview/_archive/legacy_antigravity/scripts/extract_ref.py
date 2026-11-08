import os
from PIL import Image

src = r"C:\Users\User\.gemini\antigravity\brain\0ae823de-468f-43f5-978b-f7649fa1de46\.user_uploaded\media_1792253871464_71384d0c.png"
out_dir = r"F:\GameProject\Pale Vertica\Tools\VfxPreview\ref_frames"
os.makedirs(out_dir, exist_ok=True)

im = Image.open(src)
cols, rows = 6, 5
cw, ch = 125, 150

for r in range(3): # First 3 rows = 18 frames
    for c in range(cols):
        idx = r * cols + c
        cell = im.crop((c * cw, r * ch, (c + 1) * cw, (r + 1) * ch))
        cell.save(os.path.join(out_dir, f"ref_{idx:02d}.png"))

print(f"Extracted 18 reference frames into {out_dir}")
