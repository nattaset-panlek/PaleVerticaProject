# Skill Icon Generation Walkthrough

ทำไอคอนสกิลทุกตัวในภาพเดียว: เอา prompt ไป gen กับ AI สร้างภาพ ได้ sheet แบบกริดมีเลข index แล้วตัด แปลงเป็น vector ใน Figma และนำเข้า Unity

- ข้อมูลสกิลอ้างอิง `Assets/Resources/Data/Skill.json` ณ 2026-10-24 มี 24 active skill
- ธีมอ้างอิง [UI Visual Language](../../01-Design/UI_Visual_Language.md): "pale ink on old maps", โทนขาวดำ, contour line, ลวดลาย celestial/gothic, ไอคอนเป็น glyph ตัน สีเดียว และใช้สีเฉพาะเมื่อมีความหมาย (ธาตุ)
- Prompt เขียนเป็นภาษาอังกฤษ เพราะ AI สร้างภาพส่วนใหญ่เข้าใจภาษาอังกฤษดีกว่า
- **Prompt แบบประกอบเสร็จแล้ว (copy ทีละกล่องไปวางได้เลย):** [Skill_Icon_Prompts_Full.md](./Skill_Icon_Prompts_Full.md)

---

## 1. ตาราง index (กริด 5x5 อ่านซ้ายไปขวา บนลงล่าง)

ถ้า AI เขียนเลขเพี้ยน ให้ยึด**ตำแหน่ง**ช่องเป็นหลัก ช่องที่ 1 คือซ้ายบน ช่องที่ 25 คือขวาล่าง

| ช่อง | SkillID | ชื่อ | ทำอะไรในเกม | ไอคอน | สีธาตุ |
|---|---|---|---|---|---|
| 01 | 1001 | Tackle | พุ่งชนระยะประชิด ผลักศัตรู | ไหล่พุ่งชน มีเส้นความเร็วและแรงกระแทก | ไม่มี |
| 02 | 1002 | Healing | ฮีลตัวเอง (Holy) | ฝ่ามือประคองแสงรูปกางเขนเล็ก | Holy |
| 03 | 1003 | Hellfire Blast | ไฟระเบิดเป็นรูปกรวยด้านหน้า | กรวยไฟพวยพุ่งไปข้างหน้า | Fire |
| 04 | 1004 | Divine Blessing | ออร่าศักดิ์สิทธิ์ ฮีลและบัฟรอบตัว (toggle) | วงแหวนรัศมีกับขนนกลอยขึ้น 3 อัน | Holy |
| 05 | 1005 | Earthshatter | ทุบพื้นแตกเป็นรูปกรวยและผลัก | หมัดทุบพื้น รอยแตกแผ่เป็นกรวย หินกระเด็น | Earth |
| 06 | 1006 | Lightning Strike | ลำสายฟ้าแนวเส้นตรง | ลำสายฟ้าตรงพาดผ่านไอคอน | Electric |
| 07 | 1007 | Shadow Dash | พุ่งตัวแบบเงา | เงาร่างพุ่งพร้อมภาพติดตาตามหลัง | Ghost |
| 08 | 1008 | Aura of Flames | วงไฟรอบตัว ทำดาเมจต่อเนื่อง (toggle) | วงแหวนไฟล้อมรอบร่าง | Fire |
| 09 | 1009 | Lightning Blessing | บัฟสายฟ้าให้ตัวเอง | ดาบหรือร่างที่มีประกายไฟฟ้า | Electric |
| 10 | 1010 | Fire Pila | เสาไฟปะทุ ณ จุดที่เล็ง | เสาไฟพุ่งจากวงเวทบนพื้น | Fire |
| 11 | 0 | Summon Valkyrie | อัญเชิญวาลคิรี | หมวกมีปีก หรือร่างนักรบมีปีกถือหอก | ไม่มี |
| 12 | 1011 | Perfect Parry | ปัดป้องสวนกลับ | ดาบไขว้กัน มีประกายตรงจุดปะทะ | ไม่มี |
| 13 | 1012 | Chain Lightning | สายฟ้ากระโดดต่อกันหลายเป้า | สายฟ้าเชื่อมจุด 3 จุด | Electric |
| 14 | 1013 | Blink Strike | วาร์ปไปฟันที่ตำแหน่ง | มีดแทงออกจากรอยแยกวาร์ป | ไม่มี |
| 15 | 1014 | Time Dilation | สนามทำให้เวลาช้าลง | นาฬิกาทรายในเกลียวหน้าปัด | Time |
| 16 | 1015 | Force Nova | คลื่นพลังผลักออกรอบทิศ | วงคลื่นกระแทกซ้อนกันแผ่ออก | ไม่มี |
| 17 | 1016 | Blink | วาร์ประยะสั้น (Electric) | ร่างหายไปเป็นประกาย แล้วโผล่อีกฝั่ง มีเส้นประเชื่อม | Electric |
| 18 | 1017 | Chornos Field | สนามเวลาขนาดใหญ่ | หน้าปัดนาฬิกาหรือแอสโตรเลบเป็นโดม | Time |
| 19 | 1019 | Mind Control | ควบคุมศัตรู | ดวงตาหรือศีรษะ มีเส้นหุ่นเชิดหรือเกลียวสะกด | Mind |
| 20 | 3001 | Wind Slash | ฟันลมเป็นกรวย | ใบมีดลมโค้งคม มีเส้นเคลื่อนไหว | Wind |
| 21 | 3002 | Crescent Sweep | กวาดดาบเป็นวงพระจันทร์เสี้ยว | วงดาบรูปจันทร์เสี้ยว | ไม่มี |
| 22 | 3003 | Cherry Blossom Execution | ท่าปิดฉากรอบตัว | ฟันคาตานะ มีกลีบซากุระร่วงรอบ | Petal |
| 23 | 1020 | Fire Bolt | ยิงลูกไฟ (projectile) | ลูกไฟพุ่งแนวทแยง | Fire |
| 24 | 1021 | Sprint | บัฟวิ่งเร็ว | รองเท้าหรือเท้า มีเส้นลมและลูกศรชี้ไปข้างหน้า | ไม่มี |
| 25 | - | (ว่าง) | สำรองไว้สำหรับสกิลใหม่ | กรอบเปล่า | - |

> ชื่อ "Chornos Field" สะกดตาม `Skill.json` ตอนนี้ (น่าจะตั้งใจเป็น Chronos)

---

## 2. ขั้นตอนใช้งาน

1. **เลือกสไตล์** จากหัวข้อ 4 แล้วคัดลอก **Style block** ของสไตล์นั้น ต่อด้วย **Reference block** (หัวข้อ 2b) และ **Shared block** (หัวข้อ 3) รวมเป็น prompt เดียว
   - แนบภาพอ้างอิง 3 ภาพจาก `Skill_Icon_Reference/` ไปพร้อม prompt
2. **Gen ภาพ** อัตราส่วน 1:1 ขนาด 2048 px ขึ้นไป
   - ถ้า AI ทำได้ไม่ครบ 25 ช่อง ให้ใช้ fallback ในหัวข้อ 5 แบ่งเป็น 2 sheet
3. **ตรวจทีละช่อง** เทียบกับตารางหัวข้อ 1 ช่องไหนผิดหรือไม่สวย ให้ gen แก้เฉพาะช่องนั้นด้วย prompt ในหัวข้อ 6
4. **ตัดใน Figma**
   - วางภาพลงใน frame ขนาด 2048x2048
   - สร้าง frame ช่องละ 409.6 px ทั้ง 5x5 ช่อง (หรือใช้ grid layout 5 คอลัมน์) แล้วตั้งชื่อ frame ตามข้อ 6 ด้านล่าง
5. **แปลงเป็น vector** ด้วยปลั๊กอิน image trace หรือ vectorizer
   - สไตล์ 1, 2 และ 5 แปลงง่ายที่สุด (path น้อย)
   - สไตล์ 3 และ 6 ได้ path เยอะกว่า ต้องลด detail
   - ลบพื้นหลังและเลข index ออกก่อนใช้งานจริง
6. **ตั้งชื่อ** `SkillIcon_{ช่อง}_{SkillID}_{Name}` เช่น `SkillIcon_23_1020_FireBolt`
7. **นำเข้า Unity**
   - export PNG ขนาด 256x256 ไปที่ `Assets/Addressable/Sprite/Skill/Icon/`
   - ตั้ง `IconPath` ของสกิลผ่าน Pale Vertica GameDataWorkbench (ไม่ต้องแก้ JSON ด้วยมือ)
   - ไอคอนที่ไม่มี path จะใช้ `Icon_Default.png`
8. **สกิลใหม่ในอนาคต** ใช้ช่อง 25 ก่อน ถ้าเกิน ให้เปลี่ยน Shared block เป็นกริด 6x6 (36 ช่อง) แล้วเพิ่มแถวในตารางหัวข้อ 1

### ข้อผิดพลาดที่เจอบ่อย

- **AI ใส่ชื่อสกิลหรือข้อความแปลกๆ ในภาพ:** Shared block สั่ง "no other text" ไว้แล้ว ถ้ายังมี ให้ลบตอนตัด
- **เลข index ผิดหรือซ้ำ:** ยึดตำแหน่งช่องแทนเลข
- **ช่องไม่เท่ากัน หรือไอคอนล้นข้ามช่อง:** gen ใหม่ หรือใช้ fallback 2 sheet
- **รายละเอียดเล็กเกิน:** ไอคอนจะแสดงที่ hotbar ประมาณ 48 px ต้องดูออกจาก silhouette อย่างเดียว ให้ลองย่อดูก่อน
- **ใช้สีเกินความหมาย:** ตามธีม สีมีไว้บอกธาตุเท่านั้น ส่วนที่เหลือเป็นขาวดำ

---

## 2b. ภาพอ้างอิงจาก Figma (แนบไปกับ prompt)

Export จาก section **Assets & References** (หน้า `WockSpace` > `90 · Archive & References`, [Figma node 1786-25732](https://www.figma.com/design/zZ5QnVfPMSFuiMFjpIGbOC/Whiper-of-pale?node-id=1786-25732)) ผ่าน Talk to Figma เมื่อ 2026-10-24 เก็บไว้ที่ [`Skill_Icon_Reference/`](./Skill_Icon_Reference/):

| ไฟล์ | มีอะไร | AI ควรเรียนรู้อะไร |
|---|---|---|
| `PV_Ref_A_IconGlyphs.png` | glyph ไอคอนของเกม 24 แบบ (เอาตัวซ้ำออกแล้ว) บนพื้นดำของ UI | ภาษาของ glyph: รูปทรงตัน เส้นหนาเท่ากัน ปลายและมุมโค้งมน เรขาคณิตเรียบง่าย (diamond, ดาว 4 แฉก, ส่วนโค้ง, วงแหวน) สีเดียว |
| `PV_Ref_B_SkillFrames.png` | skill slot บน hotbar (glyph ดำบนกระดาษลาย contour) และ node ของ skill tree (diamond = skill, hexagon = class, วงกลม = attribute) | ขนาดและตำแหน่งของ glyph ในกรอบ, โทนกลับขาวดำ |
| `PV_Ref_C_IconInSkillNode.png` | glyph วางใน diamond skill node | ภาพของไอคอนสกิลที่เสร็จแล้ว (glyph กินพื้นที่ราว 45-50% ของกรอบ) |

Reference block (วางต่อจาก Style block):

```text
REFERENCE: three reference images are attached. Match their glyph language exactly: bold flat shapes, uniform thick strokes with fully rounded caps and corners, simple geometric construction (diamonds, four-point stars, arcs, rings), no inner detail lines, one solid colour per glyph, the glyph filling about half of its frame. Image A is the game's existing icon set, image B shows the hotbar skill slot and skill-tree node frames the icons sit in, image C shows icons inside the diamond skill node. Do not copy these exact symbols or any numbers from the references; design new symbols for the skills below in the same visual language.
```

> AI ส่วนใหญ่รับภาพอ้างอิงได้หลายภาพ ถ้ารับได้ภาพเดียว ใช้ `PV_Ref_A_IconGlyphs.png`
> export ใหม่ได้ด้วยสคริปต์ Bun ที่ส่ง `export_node_as_image` ผ่าน `ws://localhost:3055` channel `flowux` (ดู [Figma Design Workflow Walkthrough](./Figma_Design_Workflow_Walkthrough.md))

---

## 2c. ไอคอนชุด v1 ใน Figma (vector พร้อมใช้)

วาดเป็น vector ตามภาพที่ AI gen ด้วยสไตล์ 1 (Ink Glyph) เมื่อ 2026-10-24 ภาพตัวอย่าง: [`PV_SkillIcons_v1_Figma.png`](./Skill_Icon_Reference/PV_SkillIcons_v1_Figma.png)

- **ตำแหน่ง:** เดิมอยู่หน้า **Preview** > section **PV / Skill Icons** (`1688:3607`) > frame **Skill Icon Sheet** (`1689:3608`) แต่ตอนตรวจ 2026-11-08 ไม่มีในไฟล์แล้ว
- **ไอคอนแต่ละตัว:** frame 256x256 พื้นโปร่งใส ชื่อ `SkillIcon_{ช่อง}_{SkillID}_{Name}` เช่น `SkillIcon_23_1020_FireBolt` export ทีละตัวได้เลย (PNG 256 หรือ SVG) พื้นช่องสีดำและเลข index เป็นของ sheet ไม่ติดไปกับไอคอน
- **ใช้ element เดิมจาก Figma (geometry เดียวกับต้นฉบับ แค่หมุน ย่อขยาย หรือเปลี่ยนสี):**

| Element ต้นฉบับ | ใช้ในไอคอน |
|---|---|
| ดาบ `UI_WeaponRight_Icon` (`1786:25867`) | 09 Lightning Blessing (ตั้งตรง ทำให้เรียว), 12 Perfect Parry (ไขว้ 2 เล่ม), 14 Blink Strike, 21 Crescent Sweep, 22 Cherry Blossom Execution (เรียวเป็นคาตานะ) |
| ไฟ `UI_Skills_Icon` (`1786:25767`) | 10 Fire Pila |
| ดาว 4 แฉก `UI_StarGylp_Icon` (`1786:25768`) | 01 Tackle, 02 Healing, 12 Perfect Parry, 16 Force Nova, 18 Chornos Field |
| หมวก `UI_Helmet_Icon` (`1786:25863`) | 11 Summon Valkyrie |

- **สีธาตุ:** ใช้ตาม COLOUR RULE ใน Shared block (Fire #FF6A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #8F86E0, Time #A6AAF2, Mind #B05CFF, Wind #7FE0C8, Petal #F4A6C0) ส่วนที่ไม่มีธาตุเป็น #F2F2F2
- **ไอคอนที่วาดใหม่ (ไม่มี element เดิมที่ใช้ได้):** ไฟของ 03, 08 และ 23 ไม่ได้ใช้ glyph ไฟเดิม เพราะ glyph นั้นมีรูกลวงข้างใน เมื่อหมุนหรือย่อแล้วอ่านไม่ออก จึงวาดเปลวไฟใหม่ในภาษาเดียวกัน
- **ยังไม่ได้ทำ:** export เข้า Unity (`Assets/Addressable/Sprite/Skill/Icon/`) และตั้ง `IconPath` ใน Workbench ตามขั้นตอนข้อ 7 ในหัวข้อ 2

---

## 2d. ชุดที่ใช้ในเกมตอนนี้ (Stlye_0 2)

ไอคอนที่ใช้ในเกมจริง เป็นชุดที่ trace จากภาพ AI สไตล์ "วงกลม" ลง Figma แล้วจัดระเบียบเมื่อ 2026-10-24

- **ใน Figma:** หน้า **WockSpace** > `90 · Archive & References` > `Group 2367` > frame **Stlye_0 2** (`1786:27949`)
  - แต่ละสกิลเป็น frame ขนาดเท่าช่อง (204.8 px) ชื่อ `SkillIcon_{ช่อง}_{SkillID}_{Name}` ข้างในมี group `Glyph`
  - เศษจากการ trace (เลข index, ช่อง 25) ย้ายไปไว้ใน group `_TraceLeftovers` ซึ่งซ่อนไว้ ไม่ได้ลบ
- **ใน Unity:** export เป็น PNG 256x256 พื้นโปร่งใส ที่ `Assets/Addressable/Sprite/Skill/Icon/SkillIcon_xx_ID_Name.png`
  - โฟลเดอร์นี้อยู่ใต้ entry `Assets/Addressable/Sprite` ของ Addressables group **Sprites** จึงโหลดได้ทันที ไม่ต้องลงทะเบียนเพิ่ม
- **`IconPath` ใน `Skill.json`:** ตั้งเป็น path เต็มของไฟล์ข้างบนครบ 24 สกิล `Database` โหลดผ่าน `SkillIconLoader` ตอนเริ่มเกม
- **สีไอคอน:** ไอคอนสกิลมีสีของตัวเองทุกตัว ห้ามให้ Image ย้อมสีทับ
  - Skill Manage (รายการและสกิลที่เลือก) และแถบ cast/action progress ตั้งสี Image เป็นขาวอยู่แล้ว
  - Hotbar (`Skill Slot.prefab`) ตั้งสี Image เป็นเทาเข้ม จึงเพิ่ม `HotbarSlotView._keepSkillIconColors` (ค่าเริ่มต้นเปิด): ไอคอนสกิลแสดงสีเดิม ส่วนไอคอนไอเท็มและไอคอน default ยังใช้สีของ prefab เหมือนเดิม
- **พื้นหลังเข้มใน hotbar:** ชุดนี้มีส่วนสีขาวเยอะ บนช่องกระดาษสีอ่อนจะมองไม่ชัด `HotbarSlotView` จึงสร้าง `[Runtime] SkillIconBackplate` ไว้ข้างหลังไอคอน เป็น Image สี่เหลี่ยมมุมมนสีเข้ม แสดงเฉพาะตอนที่ไอคอนสกิลใช้สีของตัวเอง ตั้งค่าได้ใน Inspector หัวข้อ Icon Colour:
  - `_showSkillIconBackplate` (เปิด/ปิด)
  - `_skillIconBackplateColor` (ค่าเริ่มต้น #141313)
  - `_skillIconBackplateRadius` (10 px)
  - `_skillIconBackplatePadding` (ขยายออกจากกรอบไอคอน 2 px)
  - สร้างจากโค้ดตอน runtime จึงไม่ต้องแก้ prefab ช่องที่เป็นไอเท็ม ไอคอน default และช่องว่าง จะซ่อนพื้นหลังนี้
- **เปลี่ยนไอคอนทีหลัง:** แก้ใน Figma แล้ว export frame `SkillIcon_...` ทับไฟล์เดิมที่ scale 1.25 (ได้ 256 px) โดยไม่ต้องแก้ JSON

---

## 3. Shared block (ใช้ร่วมกับทุกสไตล์)

```text
A single square sprite sheet of 24 skill icons for "Pale Vertica", a mature dark-fantasy 2D action RPG.

LAYOUT: one 2048x2048 image, a strict grid of exactly 5 rows and 5 columns (25 equal square cells) with even thin gutters. Read left-to-right, top-to-bottom. Every cell holds one icon centred inside the same rounded-square frame (corner radius about 10% of the cell). In the top-left corner of each cell put a small index tag with a two-digit number, 01 to 25, in a clean sans-serif. Cell 25 is an empty frame with only its index tag. No skill names, no other text, no watermark, no signature.

READABILITY: every icon must still read at 48x48 px. One bold central silhouette per icon, strong figure-ground contrast, no tiny details, identical line weight, identical lighting direction (top-left) and identical icon scale in every cell.

COLOUR RULE: the sheet is greyscale except for one element accent colour per icon, used only on the magical or elemental part. Accents: Fire #FF7A1A, Holy #F2D27A, Electric #5FE3FF, Earth #C8913A, Ghost #9A86C8, Time #A9C4FF, Mind #C04BFF, Wind #7FE0C8, Petal #F4A6C0. Skills marked "none" use no accent.

CELLS (index - skill - icon subject - accent):
01 Tackle - a shoulder charging forward with speed lines and an impact burst - none
02 Healing - an open palm cradling a small glowing cross of light, rising sparks - Holy
03 Hellfire Blast - a wide cone of fire erupting forward - Fire
04 Divine Blessing - a radiant halo ring with three feathers rising above it - Holy
05 Earthshatter - a fist striking the ground, cracks spreading in a cone, flying rocks - Earth
06 Lightning Strike - one straight lightning beam piercing across the icon - Electric
07 Shadow Dash - a dark silhouette dashing with ghostly afterimages behind it - Ghost
08 Aura of Flames - a ring of flames encircling a standing figure - Fire
09 Lightning Blessing - an upright sword wrapped in crackling sparks - Electric
10 Fire Pila - a tall pillar of fire bursting from a magic circle on the ground - Fire
11 Summon Valkyrie - a winged helmet above a spear, summoned from light - none
12 Perfect Parry - two crossed blades with a bright flash at the contact point - none
13 Chain Lightning - a lightning bolt jumping between three node points - Electric
14 Blink Strike - a dagger thrusting out of a torn teleport rift - none
15 Time Dilation - an hourglass inside a slowing spiral clock ring - Time
16 Force Nova - concentric shockwave rings bursting outward from a centre point - none
17 Blink - a figure dissolving into sparks and reappearing on the other side, dotted arc between - Electric
18 Chornos Field - a large astrolabe clock face forming a dome over the ground - Time
19 Mind Control - an eye with puppet strings hanging from it - Mind
20 Wind Slash - a sharp curved blade of wind with motion streaks - Wind
21 Crescent Sweep - a sword arc shaped like a crescent moon - none
22 Cherry Blossom Execution - a katana slash circled by falling cherry petals - Petal
23 Fire Bolt - a fireball streaking diagonally upward-right with a short trail - Fire
24 Sprint - a boot with wind speed lines and a forward chevron arrow - none
25 (empty frame)
```

---

## 4. Style blocks (6 สไตล์ ต่างกันแต่อยู่ในธีมเดียวกัน)

เรียงเป็น Style block, Reference block (หัวข้อ 2b) แล้วตามด้วย Shared block

### Style 1: Ink Glyph (Dark UI)
เหมือนไอคอนในหน้าต่างโทนมืดของเกมมากที่สุด และแปลงเป็น vector ง่ายที่สุด

```text
STYLE: flat vector glyph icons that match a minimal occult game UI. Each frame is a near-black rounded square (#141313) with a 2px light-grey outline and a thin inner bevel line, and very faint topographic contour lines in the frame background (about 8% opacity). The glyph is bold, flat, solid off-white (#F2F2F2): no gradients, no shading, no texture, crisp geometric shapes, clean edges ready for vector tracing. Only the element part of the glyph uses its accent colour. Quiet, mature, high contrast.
```

### Style 2: Paper Glyph (Light UI)
คู่กับหน้าต่างโทนสว่าง ภาพดูเหมือนหมึกดำปั๊มบนกระดาษ

```text
STYLE: flat vector glyph icons for a light paper-toned game UI. Each frame is a paper-white rounded square (#F2F2F2) with a 2px near-black outline (#141313) and white topographic contour lines over a slightly darker band (#E6E6E6) at the top of the frame. The glyph is bold, flat, solid near-black ink (#141313), like a crisp ink stamp: no gradients, no shading, clean edges ready for vector tracing. Only the element part of the glyph uses its accent colour.
```

### Style 3: Cartographer's Engraving
ลายเส้นแกะสลักแบบแผนที่โบราณ

```text
STYLE: antique atlas engraving. Each frame is a rounded square drawn like an old map cartouche, with a small compass-star ornament in the bottom-right corner. Icons are monochrome dark-sepia ink line art on aged parchment (#EDE3CF), with bold outer contours and simple parallel hatching for shadow (keep hatching sparse so it reads at 48 px). The element part gets a single flat watercolour-like wash of its accent colour, inside the lines. Looks like an illustration from a cartographer's grimoire.
```

### Style 4: Gothic Lancet (Stained Glass)
อิงซุ้มประตู lancet ของการ์ด Class Mastery

```text
STYLE: gothic stained glass. Inside each rounded-square cell, the icon sits in a tall pointed lancet-arch window. Thick black leading lines divide the image into flat colour panes. Most panes are muted greys from #2A2A2A to #D9D9D9; only 1 or 2 panes use the element accent colour. Tiny star dots in the background panes for a celestial feel. Flat colours only: no glow, no texture, no gradients. Sacred and ceremonial.
```

### Style 5: Celestial Sigil
แต่ละสกิลเป็นตราเวทเรขาคณิต อิงสัญลักษณ์ compass star ของ Forge/Socket Mastery

```text
STYLE: arcane sigils. Each skill is drawn as an occult seal: the core symbol is built from thin precise geometric lines, circles, compass stars and sacred-geometry construction lines, in silver-white (#E6E6E6) on deep black (#09090A) rounded-square frames with a 2px grey outline. Consistent 3px line weight, one or two faint orbit rings around the symbol, one small accent-coloured mark for the element. Minimal, elegant, engraved-silver feel. Line art only, easy to trace to vector.
```

### Style 6: HD-2D Cel-shaded
สไตล์พรีเมียมเหมือนไอคอนเกม JRPG แนว HD-2D ยังแรเงาแบบ flat เป็นชั้น จึงแปลงเป็น vector ได้

```text
STYLE: premium HD-2D JRPG skill icons, cel-shaded. Each frame is a near-black rounded square (#141313) with a 2px light-grey outline and faint topographic contour lines. Icons are semi-detailed with exactly three flat tones per colour (shadow, base, highlight), a thick dark outline and a crisp rim light from the top-left. Greyscale for everything except the element accent, which may use its own three tones. No soft gradients, no airbrush, no noise, so the art can be traced to vector shapes.
```

---

## 5. Fallback: แบ่งเป็น 2 sheet (ถ้า AI ทำ 25 ช่องไม่ได้)

แทนบรรทัด LAYOUT ใน Shared block ด้วยข้อความด้านล่าง แล้วใส่ CELLS เฉพาะครึ่งที่ต้องการ

```text
LAYOUT: one 2048x1536 image, a strict grid of exactly 3 rows and 4 columns (12 equal square cells) with even thin gutters, read left-to-right, top-to-bottom. Every cell holds one icon centred in the same rounded-square frame, with a small two-digit index tag in its top-left corner using the numbers given below. No other text.
```

- **Sheet A:** CELLS 01-12
- **Sheet B:** CELLS 13-24 (เลข index ยังเป็น 13-24 ไม่ต้องนับใหม่)

---

## 6. แก้เฉพาะช่องเดียว

```text
[STYLE BLOCK]
One single skill icon for "Pale Vertica", 1024x1024, centred inside the rounded-square frame described above, no index tag, no text. Subject: [ICON SUBJECT FROM THE TABLE]. Accent colour: [ACCENT OR none], used only on the element part. Must read clearly at 48x48 px.
```
