import base64
import glob
import json
import os

from vfx_effectcraft import FPS, OUTPUT_DIR, TOOLS_DIR

GALLERY_PATH = os.path.join(TOOLS_DIR, "index.html")
TEMPLATE_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "vfx_gallery_template.html")
SET_ORDER = ["WeaponV2Style", "SlashStylize", "WeaponArtStylized", "WeaponPainterly", "WeaponClean"]
SET_LABELS = {
    "WeaponV2Style": "V2 Style",
    "SlashStylize": "Slash Stylize",
    "WeaponArtStylized": "Art Stylized",
    "WeaponPainterly": "Painterly",
    "WeaponClean": "First Pass",
}
CATEGORY_ORDER = ["Fist", "Magic", "Spear", "Sword", "Slash"]

DESCRIPTIONS = {
    "VFX_Fist_Jab_V2": "หมัดตรง: วงแสงกลมกระแทก + เสี้ยวโค้งคลื่นกระแทก 3 ชั้นไปข้างหน้า + วงแหวนขยาย + เส้นความเร็ว + ประกาย",
    "VFX_Fist_Hook_V2": "หมัดเหวี่ยง: เสี้ยวโค้งแบบ Slash V2 ขนาดสั้น แล้วจบด้วยวงแสงและเสี้ยวคลื่นกระแทก",
    "VFX_Fist_Smash_V2": "ทุบพื้น: วงแหวนวงรีบนพื้น 2 ชั้น + เสี้ยวโค้งสาดขึ้นซ้ายขวา + วงแสงกลาง + ประกายพุ่งขึ้น",
    "VFX_Magic_Burst_V2": "ระเบิดเวท: วงแสงกลม + ใบมีดเสี้ยวโค้ง 4 ใบหมุนออก + วงแหวนคู่ + เศษเสี้ยวโค้ง",
    "VFX_Magic_Orb_V2": "รวมพลัง: หางเสี้ยวโค้ง 3 เส้นหมุนเข้าหาลูกแก้ว แล้วระเบิดเป็นกลีบเสี้ยวโค้ง 6 กลีบ + วงแหวน",
    "VFX_Magic_Sigil_V2": "วงเวท: วงแหวนปลายเรียว 2 วงค่อยๆ วาดสวนทาง + จุดกลมโคจร 4 จุด + แฟลชและวงพัลส์ปิดท้าย",
    "VFX_Spear_Thrust_V2": "แทงหอก: หอกแสงสามชั้นพุ่งตรง + เส้นข้างคู่ + เสี้ยวคลื่นกระแทกและวงแสงที่ปลาย",
    "VFX_Fist_Jab_Paint": "หมัดตรง: หนามแหลมพุ่งไปข้างหน้า 3 ชั้น (เงาดำ / กลาง / สว่าง) + จุด halftone + เส้นฉีก + เศษกระเด็น แกนเย็นลงจากขาวเป็นแดง",
    "VFX_Fist_Smash_Paint": "ทุบพื้น: รอยไหม้ดำบนพื้น + หนามระเบิดขึ้นฟ้า + ควันลอย + ระลอกพื้น + หินกับถ่าน (ตาม reference แถวล่าง)",
    "VFX_Magic_Burst_Paint": "ระเบิดมานา: โดมแสงล้อมด้วยริบบิ้นดำหมุน + เศษแหลม + ระลอกพื้น (ตาม reference แถวบน)",
    "VFX_Magic_Comet_Paint": "ลูกไฟเวท: ดาวหางพุ่งลงมากระแทกแล้วระเบิดเป็นหนาม + halftone + ระลอก (ใช้กับไม้เท้าที่ยิงไกล)",
    "VFX_Spear_Thrust_Paint": "แทงหอก: หอกสามชั้นมีเงาดำ + เส้นฉีกข้าง + หนามระเบิดที่ปลาย",
    "VFX_Sword_Slash_Paint": "ฟันดาบ: เสี้ยวพระจันทร์สี่ชั้นขอบฉีก + เส้นฟันเฉียงยาว + สะเก็ดไฟ (ตาม reference ภาพที่ 3)",
    "VFX_Fist_Jab_Art": "หมัดตรง: ประกายหยักสองชั้นยืดไปข้างหน้า + เส้นความเร็วลายพู่กัน + เศษชิ้นกระเด็น",
    "VFX_Fist_Hook_Art": "หมัดเหวี่ยง: เส้นโค้งหนามี Echo smear แล้วระเบิดเป็นประกายหยัก เหมาะเป็นท่าที่ 2 ของคอมโบ",
    "VFX_Fist_Smash_Art": "ทุบพื้น: ประกายแบนบนพื้น + เปลวพุ่งขึ้น (Turbulent Displace) + หินกระเด็น + ฝุ่นข้าง",
    "VFX_Fist_Rush_Art": "รัวหมัด: ประกายเล็ก 4 จุดต่อเนื่อง พร้อมเส้นพุ่งนำแต่ละหมัด",
    "VFX_Magic_Burst_Art": "ระเบิดเวท: ดาวแฉก 4 แฉกใหญ่ + ใบมีดกังหันหมุนออก (Echo) + วงแตกเป็นท่อน + ประกายระยิบ",
    "VFX_Magic_Sigil_Art": "วงเวท: วงแหวนปลายเรียวค่อยๆ วาด + อักษรรูนสี่เหลี่ยมข้าวหลามตัด + แฟลชปิดท้าย",
    "VFX_Magic_Orb_Art": "รวมพลัง: เส้นพลังหมุนเกลียวเข้าหาลูกแก้วแล้วแตกเป็นกลีบ มีจังหวะ charge ชัด",
    "VFX_Magic_Shard_Art": "เกล็ดผลึก: คริสตัลสองโทน (ด้านสว่าง/ด้านเงา) พุ่งออกรอบทิศ",
    "VFX_Spear_Thrust_Art": "แทงหอก: หัวหอกสองโทนมี Echo smear + ลมพันเกลียว + ประกายทะลุที่ปลาย",
    "VFX_Fist_Jab": "หมัดตรง: เส้นความเร็ว + ดาวกระแทก + วงคลื่นแนวตั้ง เร็วและคม เหมาะกับ attack ปกติ",
    "VFX_Fist_Hook": "หมัดเหวี่ยง: เส้นโค้งสั้นจากบนลงมากระแทก เหมาะเป็นท่าที่ 2 ของคอมโบ",
    "VFX_Fist_Smash": "ทุบหนัก: คลื่นกระแทกแบนบนพื้น + เศษหินกระเด็น เหมาะกับท่าปิดคอมโบหรือ critical",
    "VFX_Fist_Rush": "รัวหมัด: จุดกระแทกเล็ก 4 จุดต่อเนื่อง เหมาะกับ attack speed สูงหรือ multi-hit",
    "VFX_Magic_Burst": "ระเบิดเวท: แสงแตกเป็นรัศมี + วงแหวนคู่ + ประกายระยิบ อ่านง่าย ใช้ได้ทุกธาตุ",
    "VFX_Magic_Sigil": "วงเวท: วาดวงกลม + สามเหลี่ยมคู่ + อักษรรูนหมุน แล้วแฟลชปิด เหมาะกับจังหวะร่ายที่ตัวผู้ใช้",
    "VFX_Magic_Orb": "รวมพลัง: อนุภาคหมุนเข้าหาลูกแก้วแล้วระเบิดออก มีจังหวะ charge ชัดเจน",
    "VFX_Magic_Shard": "เกล็ดผลึก: เศษคริสตัลพุ่งออกรอบทิศ ให้ความรู้สึกแข็ง เหมาะกับน้ำแข็งหรือ arcane",
    "VFX_Spear_Thrust": "แทงหอก: หัวแหลมพุ่งตรง + เส้นข้างคู่ + วงทะลุ (ตัวอย่างว่าหอกมีได้ทั้งแทงและฟัน)",
    "VFX_Slash_Stylize_V2": "อ้างอิง: Slash Stylize V2 ที่ใช้ใน SlashVFX.prefab อยู่ตอนนี้",
}


CATEGORY_ALIASES = {"Pure": "Slash"}


def category(comp):
    parts = comp.split("_")
    name = parts[1] if len(parts) > 1 else "Other"
    return CATEGORY_ALIASES.get(name, name)


def entries():
    found = []
    sets = sorted(os.listdir(OUTPUT_DIR), key=lambda n: (SET_ORDER.index(n) if n in SET_ORDER else len(SET_ORDER), n))
    for set_name in sets:
        comps = []
        for sheet in glob.glob(os.path.join(OUTPUT_DIR, set_name, "*", "*_Sheet.png")):
            comp = os.path.basename(os.path.dirname(sheet))
            if os.path.basename(sheet) != comp + "_Sheet.png":
                continue
            frames = len(glob.glob(os.path.join(os.path.dirname(sheet), "frames", "*.png")))
            comps.append((comp, sheet, frames))
        comps.sort(key=lambda c: (CATEGORY_ORDER.index(category(c[0])) if category(c[0]) in CATEGORY_ORDER else 99, c[0]))
        for comp, sheet, frames in comps:
            found.append((set_name, comp, sheet, frames))
    return found


def main():
    data = []
    for set_name, comp, sheet, frames in entries():
        with open(sheet, "rb") as f:
            src = "data:image/png;base64," + base64.b64encode(f.read()).decode("ascii")
        label = "%s (%s)" % (category(comp), SET_LABELS.get(set_name, set_name))
        desc = DESCRIPTIONS.get(comp, "")
        data.append({"cat": label, "name": comp, "desc": desc + ("<br>" if desc else "") + "<code>projects/%s/%s.ecproj</code>" % (set_name, comp), "src": src, "frames": frames or 16})
    with open(TEMPLATE_PATH, encoding="utf-8") as f:
        page = f.read()
    html = page.replace("__DATA__", json.dumps(data, ensure_ascii=False)).replace("__FPS__", str(FPS))
    with open(GALLERY_PATH, "w", encoding="utf-8") as f:
        f.write(html)
    print(GALLERY_PATH, len(data), "effects")


if __name__ == "__main__":
    main()
