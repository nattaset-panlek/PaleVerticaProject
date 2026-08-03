import json
import os
import tempfile

# กำหนดพาธไปยังไฟล์ JSON ที่คุณเพิ่งโหลดไป
MASTER_JSON_PATH = "skills_master.json"
ICON_CRAFTER_PATH = "icon_crafter.py"
OUTPUT_DIR = "Assets/Addressable/Sprite/Skill/Icon/"

# สร้างโฟลเดอร์ปลายทาง
os.makedirs(OUTPUT_DIR, exist_ok=True)

with open(MASTER_JSON_PATH, "r", encoding="utf-8") as f:
    skill_data = json.load(f)

for skill_key, spec in skill_data.items():
    print(f"กำลังประมวลผล: {skill_key}...")
    
    # สร้าง Temp JSON file สำหรับแต่ละสกิล
    with tempfile.NamedTemporaryFile(mode='w', delete=False, suffix='.json') as temp_f:
        json.dump(spec, temp_f)
        temp_path = temp_f.name
        
    # วาดโหมดสี (Icon Mode)
    color_output = os.path.join(OUTPUT_DIR, f"Icon_{skill_key}.png")
    os.system(f'python {ICON_CRAFTER_PATH} draw --spec "{temp_path}" --output "{color_output}" --mode icon')
    
    # วาดโหมดสัญลักษณ์ขาว (Symbol Mode)
    symbol_output = os.path.join(OUTPUT_DIR, f"Symbol_{skill_key}.png")
    os.system(f'python {ICON_CRAFTER_PATH} draw --spec "{temp_path}" --output "{symbol_output}" --mode symbol')
    
    # ลบไฟล์ Temp
    os.remove(temp_path)

print("✅ วาดสัญลักษณ์ครบทุกสกิลเรียบร้อยแล้ว!")
