# MapLab Window Walkthrough (TH)

หน้าต่างเดียวสำหรับจัดการแมพ MapLab: ดูวัตถุทั้งหมดในแมพ จัดเป็น **Set** (กลุ่ม) เพิ่ม/เอาออก/หาตำแหน่ง และดูกับสั่ง **Bake** ทุกอย่างของแมพ พร้อมสถานะว่าอัปเดตแล้วหรือล้าสมัย

- อัปเดต: 2026-10-23
- ดีไซน์: Figma `Whiper-of-pale` หน้า Preview section **MapLab / Window final (Split View, cool tones)** (node `1639:8230`) ตัวเลือกอื่นอยู่ใน section **MapLab / Window (6 layouts)** และ **Window v2**
- Figma component library: section **MapLab / UI components** (13 component set: Icon, Set Tag, Status, Button, Caption, Set Row, Bake Row, Object Row, File Row, Set Header, Bake Header, Toolbar, Status Bar) และ **MapLab / Window built from components** (2 หน้าจอประกอบจาก instance ล้วน แก้ component แล้วทั้งสองจอตาม)
- คู่มือเริ่มจากศูนย์ (infographic 12 การ์ด ภาษาไทย): Figma section **MapLab / Guide: start from zero** (node `1641:10982`)
- โค้ด: `Assets/ThirdParty/MapLab/Editor/Window/` (`MapLabWindow` ประกอบหน้าจอ, `MapLabSetView`, `MapLabBakeView`, ชิ้นที่ใช้ซ้ำ `MapLabWidgets` ตรงกับ component ใน Figma, สี/สไตล์ `MapLabWindowStyles`), `Editor/MapLabSets.cs`, `MapLabBakeStatus.cs`, `MapLabOutputPaths.cs`, Runtime `Runtime/Sets/MapLabSet.cs`
- เทสต์: `Tests/Editor/MapLabSetsTests.cs`

## 1. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| เปิดหน้าต่าง | **Tools > MapLab > MapLab Window** หรือปุ่ม **Open MapLab Window** บนสุดของ Inspector ของ MapLabMap |
| สร้างแมพใหม่ | **GameObject > MapLab > Map** (หรือปุ่ม Create Map ในหน้าต่าง ถ้าฉากยังไม่มีแมพ) |
| สร้าง Set ใหม่ | ปุ่ม **New Set** บน toolbar ของหน้าต่าง หรือ **GameObject > MapLab > Set** (ต้องเลือกแมพหรือของในแมพก่อน) |
| เครื่องมือนักพัฒนา (demo, รายงาน, ไอคอน) | **Tools > MapLab > Developer > ...** |

## 2. หน้าตา

```
+---------------------------------------------------------------+
| [MapLab] Environment v | Search............ |        New Set |  <- toolbar
+------------------+--------------------------------------------+
| SETS             |  | Plaza   6 objects   Locate All  Select All  ... |
|  All objects  22 |    Name               Type                  |
|  | Cliffs      2 |  [ ] Plaza            Flat shape     (+) (-) |
|  | Plaza       6 |  [ ] Pillar           Box shape      (+) (-) |
|  | Roads       4 |                                            |
|  | Ruins       1 |  ลากวัตถุมาวางตรงนี้ หรือเลือกแล้วกด Add Selected |
|  | Props       9 |                                            |
|  | Unassigned  0 |  [Add Selected (0)]  [Remove Selected (2)] |
| BAKES            |                                            |
|  Walkable area o |                                            |
|  Layers        o |                                            |
+------------------+--------------------------------------------+
| o ข้อความสถานะที่สำคัญที่สุด                           [Rebake] |  <- status bar
+---------------------------------------------------------------+
(+) = Locate (ปุ่มเป้า)   (-) = เอาออกจาก Set   o = จุดสถานะ
```

สี (โทนเย็นทั้งหมด): ปุ่มหลักสีน้ำเงิน ปุ่มรองสีเทาแบบ Unity, **teal = อัปเดตแล้ว**, **ม่วงอ่อน = ล้าสมัย**, เทา = ยังไม่ได้ bake แถบสีของแต่ละ Set เลือกได้ 6 สีโทนเย็น ไอคอนของ shape ใช้สีเดิม

## 3. Set คืออะไร

- **Set = GameObject ที่มี component `MapLabSet` อยู่ในแมพ** ลูกตรงๆ ของมันคือสมาชิก จึงเห็นใน Hierarchy เหมือนโฟลเดอร์ และวัตถุ 1 ชิ้นอยู่ได้ Set เดียว
- เพิ่มเข้า Set = ย้ายไปเป็นลูกของ Set (ตำแหน่งในโลกไม่เปลี่ยน) เอาออก = ย้ายกลับไปเป็นลูกของแมพ ทุกอย่าง Undo ได้ (Ctrl+Z)
- **Unassigned** = shape ของ MapLab (Plateau, Void, Ring, Box, Path) ที่ยังไม่อยู่ใน Set ใด ขั้นบน (Tier) ไม่นับแยก เพราะติดไปกับที่ราบของมันเสมอ
- **All objects** = สมาชิกทุก Set + Unassigned
- ใส่อะไรใน Set ก็ได้ที่อยู่ในแมพเดียวกัน (พร็อพ ต้นไม้ ไฟ) ยกเว้น: ตัวแมพเอง, ตัว Set เอง, ของในแมพอื่น, ชิ้นที่ระบบสร้าง (Visual/Collision/Shadow), Tier และ chunk ที่ bake
- ลบ Set (เมนู ... > Delete Set) **ไม่ลบวัตถุ** สมาชิกจะกลับไปเป็นลูกของแมพ

## 4. ขั้นตอน: จัดของเป็น Set

1. เปิด MapLab Window เลือกแมพจาก dropdown ซ้ายบน
2. กด **New Set** ตั้งชื่อในช่องที่ขึ้นมา แล้วกด Enter (Esc = ยกเลิก)
3. เพิ่มของเข้า Set ได้ 3 ทาง:
   - เลือกวัตถุใน Hierarchy หรือ Scene แล้วกด **Add Selected (n)** (n = จำนวนที่เพิ่มได้จริง)
   - ลากวัตถุจาก Hierarchy มาวางบนหน้ารายการของ Set หรือบนชื่อ Set ทางซ้าย
   - หน้า **Unassigned**: เลือกปลายทางใน "Add selected to" แล้วกด Add
4. หาตำแหน่ง: กดปุ่มเป้าท้ายแถว (หรือดับเบิลคลิกแถว) จะเลือกวัตถุ, ping ใน Hierarchy และเลื่อนกล้อง Scene ไปหา **Locate All** ทำทั้ง Set
5. เลือก: คลิกแถว = เลือก, Ctrl/Cmd+คลิก = เพิ่ม/ถอน, Shift+คลิก = เลือกเป็นช่วง แถวที่ถูกเลือกใน Unity จะเป็นสีน้ำเงิน
6. เอาออก: ปุ่ม **-** ท้ายแถว หรือ **Remove Selected (n)**
7. ที่ราบที่มีชั้นบน (Tier): คอลัมน์ Type บอกจำนวน เช่น "Plateau, 2 tiers on top" และมีลูกศรพับ/กางหน้าแถว ชั้นบนแสดงเป็นแถวเยื้องข้างใต้ (ชื่อ, Tier 2 / Tier 3, ความสูงด้านบน) คลิกเพื่อเลือกชั้นนั้นมาแก้ใน Inspector ดับเบิลคลิกหรือปุ่มเป้าเพื่อเลื่อนกล้องไปหา ชั้นบนเอาออกจาก Set แยกไม่ได้ (ติดไปกับที่ราบ)
8. เปลี่ยนชื่อ/สี/ลบ: ปุ่ม **...** บนหัว Set หรือคลิกขวาที่ชื่อ Set ทางซ้าย

## 5. ขั้นตอน: Bake

ไฟล์ bake ทุกอย่างของแมพอยู่ที่เดียว:

```
<โฟลเดอร์ฉาก>/<ชื่อฉาก>_MapLab/<ชื่อแมพ>/
    Walkable.json         พื้นที่เดินได้ (อ่านได้โดยไม่ต้องมีโค้ด MapLab)
    Walkable_Mask.png     ภาพขาวดำของพื้นที่เดินได้
    Layers/<layer>/Base/  ภาพที่ bake ของแต่ละ layer (Paint/ = งานวาดของ art, History~/ = bake เก่า)
```

ชื่อแมพตัดส่วนในวงเล็บเหลี่ยมออก (`[MapLab] Environment` เป็น `Environment`)

| หน้า | แสดง | ปุ่ม |
|---|---|---|
| **Walkable area** | การ์ด 4 ขั้น: 1 Area (กรอบ, Cell size, collider ที่โผล่นอกกรอบ), 2 What blocks walking (จำนวนและชนิด collider, Layers, ของที่ไม่นับ), 3 Bake (สถานะพร้อมชื่อของที่เปลี่ยน, ติ๊กแสดงกรอบและช่องแดงใน Scene view), 4 Use it in the game (ไฟล์ JSON และ mask) รายละเอียด: [Walkable_Area_Bake_Walkthrough_TH.md](./Walkable_Area_Bake_Walkthrough_TH.md) | **Fit to Map**, **Select All**, **Bake** / **Bake Again**, ลากกรอบสีฟ้าใน Scene view |
| **Layers** | ลำดับ layer (bake และ paint) พร้อมสถานะทีละชั้น, ของที่แต่ละ layer เอา (ชนิด, Set), ช่องที่เปลี่ยนใต้งานวาด, โฟลเดอร์ Base / Paint / History~, ค่า Actor Sorting / Chunk Size / Pixels Per Unit (รายละเอียด: [MapLab_Bake_Layers_Walkthrough_TH.md](./MapLab_Bake_Layers_Walkthrough_TH.md)) | **Bake All**, **Export** / **Import** (PSD, PNG), **Add Layer** / **Add Sorted Layer** (กำแพง ที่ราบ เสา), ต่อ layer: **Bake** / **Rebake**, **Show Live**, **Lock**, **Flatten**, ลูกศรขึ้นลง |

**อัปเดตแล้วหรือล้าสมัย**: ตอน bake ระบบจดลายนิ้วมือของสิ่งที่ใช้ bake ไว้

- Walkable: รูปร่างและตำแหน่งของ collider ที่กีดขวางในแมพ + Bounds, Cell Size, Obstacle Layers เปลี่ยนสีของ shape ไม่ทำให้ล้าสมัย
- Layers (ทีละ layer): ตำแหน่งและค่าทุกช่องของของที่ bake ไป + ชนิดและ Set ที่ติ๊ก + มีของชิ้นใหม่ที่ layer นี้รับแต่ยังไม่อยู่ในภาพไหม + ค่า Chunk Size / Pixels Per Unit; สถานะรวมทางซ้ายเป็นม่วงถ้ามี layer ที่ bake แล้วล้าสมัย ปุ่ม Rebake ที่แถบล่าง bake ใหม่เฉพาะ layer ที่ล้าสมัย

หน้าต่างตรวจใหม่เองหลังมีการแก้ในฉาก (รอให้หยุดแก้ 0.4 วินาที) และตอนกลับมาที่หน้าต่าง แถบล่างสุดบอกเรื่องที่ควรทำก่อนพร้อมปุ่มเดียว

## 6. ผลที่ควรได้ (ฉาก demo)

- SETS: All objects 22, Cliffs 2 (Plateau มี 2 ขั้น, Void Rock), Plaza 6 (Junction, Plaza, Pillar x4), Roads 4, Ruins 1 (Ring Wall), Props 9 (Boulder x7, Tree x2), Unassigned 0
- BAKES: Walkable area teal (Up to date, 100 x 68 cells, 83.7% walkable), Layers teal (2 of 2 layers baked) ถ้ารัน `BakeDemoBatch` แล้ว
- ลองขยับ Road West แล้วรอครึ่งวินาที: Layers เป็นม่วง "Out of date: 1 baked layers changed" แถว Ground ในหน้า Layers เป็น Out of date และแถบล่างมีปุ่ม Rebake

## 7. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| "No MapLab map in the open scenes" | ฉากยังไม่มี MapLabMap กด Create Map หรือ GameObject > MapLab > Map |
| Add Selected (0) กดไม่ได้ | ของที่เลือกอยู่นอกแมพนี้, เป็น Tier, เป็นชิ้นที่ระบบสร้าง หรืออยู่ใน Set นี้แล้ว |
| เพิ่ม Tier เข้า Set ไม่ได้ | ตั้งใจ: ขั้นต้องอยู่บนที่ราบของมัน ให้เพิ่มที่ราบชั้นล่างแทน ขั้นจะตามไปเอง |
| "Save the scene first" ตอน Bake | ไฟล์ bake ถูกเขียนข้างไฟล์ฉาก ต้อง save ฉากก่อน |
| สถานะเป็น "Bake again to start tracking changes" | ไฟล์ bake มาจากรุ่นก่อนที่ยังไม่จดลายนิ้วมือ กด Bake Again ครั้งเดียว |
| Layer ล้าสมัยตลอดแม้ไม่ได้แก้ | มีของชิ้นใหม่ที่ layer นั้นรับและยังเปิดอยู่ (ข้อความจะบอกจำนวน) เปลี่ยนชนิด/Set ที่ติ๊ก หรือเปลี่ยน Chunk Size / Pixels Per Unit หลัง bake กด Rebake |
| ไฟล์ bake เก่า `<ฉาก>_Walkable.json` ยังอยู่ | ไฟล์จากรุ่นก่อนโฟลเดอร์ต่อแมพ ลบได้ถ้าไม่มีฉากไหนอ้าง (demo: SandBox.unity ยังอ้างไฟล์เก่าอยู่ จึงเก็บไว้) |
