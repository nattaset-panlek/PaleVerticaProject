# MapLab Icons Walkthrough (TH)

ชุดไอคอนของแพ็กเกจ MapLab: ไอคอน component (หัว Inspector และรายการ Add Component), ไอคอนเครื่องมือแก้จุดใน Scene view, ไอคอนปุ่ม Add Tier และโลโก้ ออกแบบใน Figma แล้ว export เป็น PNG มาใช้ (ไม่ได้วาดด้วยโค้ดแล้ว)

- อัปเดต: 2026-10-23
- ต้นฉบับ: Figma `Whiper-of-pale` หน้า Preview, section **MapLab / Icon Sets (6 styles)**, เลือกใช้ **Set 5 / Tile** (node `1631:4186`): https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1631-4186
- โค้ด: `Assets/ThirdParty/MapLab/Editor/MapLabIconImporter.cs`, `MapLabIcons.cs`
- ไฟล์: `Assets/ThirdParty/MapLab/Editor/Icons/` (อยู่ใน Editor จึงไม่เข้า build เกม)

## 1. สไตล์ที่เลือก: Set 5 Tile

- สี่เหลี่ยมมุมมน (กริด 24, มุม 6) สีละหนึ่งเครื่องมือ + ลายเส้นสีขาว
- เส้น **4 px ที่ขนาด 96 px** (= 1 หน่วยบนกริด 24) หัวเส้นและมุมโค้งมน
- ใช้ได้ทั้ง Unity theme มืดและสว่างโดยไม่ต้องมีสองชุด เพราะสีพื้นของแผ่นเป็นตัวแยกจากพื้นหลัง
- อีก 5 ชุด (Line, Solid, Duotone, Flat Colour, Accent) ยังอยู่ใน section เดียวกันใน Figma สำหรับเปรียบเทียบ แต่ละชุดมีแถบตัวอย่างบน Inspector มืดและสว่างที่ 32 และ 16 px

| ไฟล์ | สีแผ่น | ใช้กับ |
|---|---|---|
| `MapLab_ExtrudedShape.png` (64 px) | เขียว `#45A66E` | `ExtrudedShape`, ปุ่ม/เครื่องมือ Edit Points ของ shape |
| `MapLab_Tier.png` (64 px) | เขียวน้ำทะเล `#2E9C93` | ปุ่ม Add Tier On Top |
| `MapLab_PathShape.png` (64 px) | ส้มทอง `#D9963A` | `PathShape`, เครื่องมือ Edit Points ของ path |
| `MapLab_Map.png` (64 px) | น้ำเงิน `#4A74E0` | `MapLabMap` |
| `MapLab_Occluder.png` (64 px) | เขียวใบไม้ `#5FA85A` | `OccluderFadeController` |
| `MapLab_Navigation.png` (64 px) | ส้ม `#E06F3A` | `NavigationManager` (ตัวอย่าง) |
| `MapLab_Logo.png` (256 px) | คราม `#5662E8` | โลโก้แพ็กเกจ |

ไอคอนของ script ถูกเก็บใน `.meta` ของแต่ละไฟล์ `.cs` แบบอ้าง GUID (`MonoImporter.SetIcon`) จึงย้ายโฟลเดอร์ได้

## 2. แก้หรืออัปเดตไอคอน

1. แก้ใน Figma (Set 5 / Tile) ให้ขนาด frame ของไอคอนยังเป็น 96 x 96
2. export frame `<ชื่อ> (96)` เป็น PNG ที่ scale **0.6667** (ได้ 64 px) โลโก้ที่ scale **2.6667** (ได้ 256 px)
3. เขียนทับไฟล์ชื่อเดิมใน `Editor/Icons/` (ชื่อตามตารางข้างบน)
4. กด **Tools > MapLab > Developer > Apply Icons** หรือ `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabIconImporter.ApplyBatch` ตอน Unity ปิด

Apply Icons ตั้ง import เป็น Editor GUI, เปิด mipmap (ย่อเหลือ 16 px แล้วไม่แตก), ไม่บีบอัด แล้วผูกไอคอนกับ script ทั้ง 5 ตัว ไม่วาดหรือแก้ภาพ

## 3. ผลที่ควรได้

Console: `[MapLab] 7 icons in .../Editor/Icons set up as editor icons; icon set on 5 scripts` หัว Inspector ของ component ทั้ง 5 ตัวมีไอคอน ปุ่ม **Edit Points** และ **Add Tier On Top** มีไอคอน และ Tools overlay ใน Scene view มีปุ่มไอคอนเดียวกันเมื่อเลือก shape

## 4. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| `icon set on 4 scripts` | ลบโฟลเดอร์ `Samples/` ไปแล้ว (ไม่มี `NavigationManager`) ปกติ |
| ไอคอนแตกหยักที่ 16 px | mipmap ปิดอยู่ กด Apply Icons ใหม่ |
| ไอคอนเบลอ | export เล็กกว่า 64 px ให้ export ใหม่ที่ scale 0.6667 |
| ลายเส้นบางมากที่ 16 px | ตั้งใจ: เส้น 4 px ที่ 96 เหลือราว 0.7 px ที่ 16 สีแผ่นยังบอกได้ว่าเป็นเครื่องมือไหน ถ้าต้องการเส้นหนาขึ้นเฉพาะขนาดเล็ก ให้ทำ frame ขนาด 32 แยกใน Figma |
