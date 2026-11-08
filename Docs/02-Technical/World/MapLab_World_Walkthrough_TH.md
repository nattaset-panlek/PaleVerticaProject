# MapLab World Walkthrough (TH): Mega chunks, Floor tiles, Presets

แมพใหญ่มากแบ่งเป็น **mega chunk** ก้อนละ 4096 x 4096 px (4 x 4 chunk ของ 16 หน่วย ที่ 64 px/หน่วย) จัดการทั้งแมพในหน้าต่างเดียวที่เลื่อน ลาก และซูมได้ เพิ่มพื้นที่ข้างขอบแมพได้แบบ Terrain neighbour ปู **floor tile** ทีละช่องหรือเทเต็ม chunk และเก็บค่าที่ใช้บ่อยไว้เป็น **preset** (Plateau, Ring, Box, Flat, Void, Path, Environment)

- อัปเดต: 2026-10-24
- ดีไซน์ที่ใช้: Figma ไฟล์ `zZ5QnVfPMSFuiMFjpIGbOC` หน้า Preview, section **MapLab / Mega chunks viewport** (node `1665:23611`) และ component **MapLab / UI components: World viewport** (node `1664:21046`) ส่วนหน้า Presets ใช้ Option 2 ใน **MapLab / World + Presets (6 options)** (node `1657:18289`)
- โค้ด runtime: `Assets/ThirdParty/MapLab/Runtime/World/` (`MegaChunkFormula`, `FloorFormula`, `MapLabWorldData`, `MapLabMegaChunk`, `MapLabFloorPalette`, `MapLabFloor`), `Runtime/Presets/MapLabPreset.cs`
- โค้ด editor: `Editor/World/` (`MapLabWorldEdits`, `MapLabMegaStatus`, `MapLabMegaThumbnails`), `Editor/Presets/MapLabPresets.cs`, หน้าต่าง `Editor/Window/` (`MapLabMegaView`, `MapLabMegaCanvas`, `MapLabMegaInspector`, `MapLabMegaState`, `MapLabViewportCamera`, `MapLabGlyphs`, `MapLabFloorTilesView`, `MapLabPresetsView`, `MapLabWorldWindow`)
- ไฟล์ bake: `Editor/Layers/MapLabLayerFiles.cs` (โฟลเดอร์ `M_x_y`), `MapLabLayerBaker.cs` (bake เฉพาะ mega chunk), `MapLabPaintExchange.cs` (PSD ต่อ mega chunk)
- เทสต์: `Tests/Editor/MegaChunkTests.cs`, `WorldFloorTests.cs`, `PresetTests.cs`
- ตัวตรวจบนสำเนาฉาก demo: `Editor/MapLabWorldProbe.cs` (dev only)
- เกี่ยวข้อง: [MapLab_Bake_Layers_Walkthrough_TH.md](./MapLab_Bake_Layers_Walkthrough_TH.md), [MapLab_Window_Walkthrough_TH.md](./MapLab_Window_Walkthrough_TH.md)

## 1. แนวคิด

```
โลก (World)                       ไฟล์ bake ของ layer หนึ่ง
+------+------+------+            Layers/Ground/Base/
| -1,1 |  0,1 |  +   |              M_-1_0/Chunk_-4_0.png ... Chunk_-1_3.png   (16 ไฟล์)
+------+------+------+              M_0_0/ Chunk_0_0.png  ... Chunk_3_3.png
| -1,0 |  0,0 |  1,0 |              M_1_0/ ...
+------+------+------+            PSD สำหรับวาด: M_-1_0.psd, M_0_0.psd, ... (4096 x 4096 ต่อไฟล์)
```

- **Mega chunk** = chunk N x N ที่รวมกันได้ไม่เกิน **Mega Chunk Pixels** (ค่าเริ่ม 4096) ค่าเริ่มคือ chunk 16 หน่วย ที่ 64 px/หน่วย = 1024 px ดังนั้นได้ 4 x 4 chunk = 64 หน่วย = 4096 px ตารางยึดจุด 0,0 ของโลกเหมือน chunk
- **World** (`World.asset` ในโฟลเดอร์ bake ของแมพ) เก็บว่ามี mega chunk ไหนบ้าง แต่ละก้อนมี **base tile** (tile พื้นทั้งก้อน) และ tile ที่ทาทับ (ค่าเริ่ม 64 x 64 tile ต่อก้อน = tile ละ 1 หน่วย)
- **Floor** (`[MapLab] Floor` ใต้ MapLabMap) วาด tile แบบ live เป็น mesh ไม่กี่ชิ้นต่อ mega chunk (tile ที่ติดกันรวมเป็นสี่เหลี่ยมเดียว) แล้ว **bake ลง ground layer ที่ติ๊ก Floor tiles** เหมือนของอื่น
- **Bake** เขียน chunk ลงโฟลเดอร์ `M_x_y` ของ mega chunk ที่มันอยู่ และ bake เฉพาะ mega chunk ที่เลือกได้ (ไม่ต้อง bake ทั้งแมพ)
- **PSD** export หนึ่งไฟล์ต่อ mega chunk ขนาด 4096 x 4096 Photoshop จึงไม่ค้างแม้แมพจะใหญ่หลายหมื่น px

## 2. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| จัดการ mega chunk, ทา floor, bake / export ต่อ mega chunk | **Tools > MapLab > MapLab Window** แผงซ้าย WORLD > **Mega chunks** |
| เปิด viewport เป็นหน้าต่างแยก (จอที่สอง) | ปุ่มลูกศรออกนอกกรอบ มุมขวาบนของหน้า Mega chunks (หน้าต่าง **MapLab World**) |
| แก้รายการ tile (ชื่อ สี texture ขนาด) | แผงซ้าย WORLD > **Floor tiles** |
| preset | แผงซ้าย LIBRARY > **Presets** |
| export / import PSD ต่อ mega chunk จากหน้า Layers | หน้า Layers ปุ่ม **Export > PSD per Mega Chunk (folder)**, **Import > Folder of Mega Chunk PSDs** |
| ตรวจทั้งระบบบนสำเนาฉาก demo | **Tools > MapLab > Developer > World Report** หรือ `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabWorldProbe.ReportBatch` (Unity ต้องปิด) |

## 3. เริ่มโลกของแมพ

ต้องมี: ฉากที่ save แล้ว และ MapLabMap หนึ่งตัว

1. เปิด MapLab Window เลือกแมพที่ dropdown ซ้ายบน แล้วกด WORLD > **Mega chunks**
2. หน้าแรกขึ้นว่า "This map has no mega chunks yet" กด **Create World**
3. ผลที่ได้:
   - `<ฉาก>_MapLab/<แมพ>/World.asset` และ `Floor Palette.asset` (6 tile: Grass, Meadow, Dirt, Stone, Sand, Snow)
   - `[MapLab] Floor` ใต้แมพ
   - mega chunk ครอบ **Bounds** ของแมพ (base tile = Grass)
   - ground layer ล่างสุดถูกติ๊ก **Floor tiles** ให้ (ถ้ายังไม่มี layer ไหนรับ floor)
4. viewport ซูมให้เห็นทั้งแมพ

## 4. Viewport: เลื่อน ลาก ซูม

```
[Mega chunks 48 mega chunks · 8 x 6 · 4096 x 4096 px each]  [Bake Out of Date (3)] [Export PSD per Mega Chunk] [pop out]
[select][pan] | [add][remove] | [paint][fill][erase][pick] | Grid Labels States        [-] [25%] [+] [fit] [frame]
 M |  14   15   16   17 ...      <- ไม้บรรทัด: ซูมไกลเป็นเลข mega chunk (M) ใกล้เป็นเลข chunk (C)
15 |  +   [16,14 *]  [17,14 *]   <- chip พิกัด, จุดสถานะ bake, ช่อง + ข้างขอบ
 ...                       [MAP]  <- mini map
```

| ทำอะไร | ทำยังไง |
|---|---|
| ซูม | ล้อเมาส์ (ซูมตรงเคอร์เซอร์), ปุ่ม `-` `+`, กดช่อง % เลือก 1% ถึง 400% (100% = pixel ที่ bake ตรงกับ pixel จอ) |
| เลื่อน | ลากปุ่มกลาง, Alt + ลาก (ยกเว้นเครื่องมือทา floor), เครื่องมือ Pan (H), หรือ scrollbar ขวาและล่าง |
| ดูทั้งแมพ / ไปที่ที่เลือก | Home หรือปุ่ม fit / F หรือปุ่ม frame, ดับเบิลคลิก mega chunk = ซูมเข้าก้อนนั้น |
| กระโดด | คลิกหรือลากบน mini map มุมขวาล่าง |
| เลือก | Select (V): คลิก mega chunk, Shift/Ctrl คลิกเพิ่ม-เอาออก, ลากกล่องเลือกหลายก้อน, Esc ล้าง |
| เปลี่ยนเครื่องมือด้วยคีย์ | V Select, H Pan, A Add, B Paint, G Fill, E Erase, I Pick, `[` `]` ขนาดแปรง, Delete ลบที่เลือก |

รายละเอียดเปลี่ยนตามระยะซูม: ไกลเห็นภาพย่อ (สีของ tile) + จุดสถานะ + ช่อง + ใกล้ขึ้นเห็นเส้น chunk 4 x 4 และเลข chunk ใกล้มากเห็นเส้น tile และแปรง

## 5. เพิ่ม / ลบ mega chunk (แบบ Terrain neighbour)

- เครื่องมือ **Add (A)** หรือ Select: ช่องเส้นประมี **+** โผล่ทุกที่ที่ติดขอบแมพ (บน ล่าง ซ้าย ขวา ไม่นับทแยง)
  - คลิก **+** = เพิ่มหนึ่งก้อน
  - ลากผ่านหลายช่อง = เพิ่มเป็นบล็อก (กล่องที่ลากต้องแตะขอบแมพอย่างน้อยหนึ่งช่อง) ปล่อยเมาส์แล้วเพิ่มทันที Undo ได้
  - base tile ของก้อนใหม่ตั้งที่ inspector หัวข้อ NEW MEGA CHUNKS, ปุ่ม **Fill the Map Bounds** เติมทุกก้อนที่ Bounds แตะ
- เครื่องมือ **Remove**: คลิก mega chunk (หรือ Delete กับที่เลือกไว้) จะถามก่อนลบ ลบแค่ข้อมูล floor ของก้อนนั้น **ไฟล์ bake และ paint บนดิสก์ยังอยู่**

## 6. Floor tiles

### รายการ tile (หน้า Floor tiles)

1. WORLD > **Floor tiles** เลือก palette (ใช้ร่วมหลายแมพได้) หรือกด **New Palette**
2. คลิกแถว tile แล้วแก้ **Name**, **Colour** (ย้อมสี texture, ไม่มี texture = สีล้วน), **Texture** (ภาพต่อกันได้ Wrap Mode = Repeat), **Texture Size** (หน่วยต่อหนึ่งรอบ texture)
3. **Add Tile** เพิ่มท้ายรายการ, **Remove Last** ลบได้แค่ตัวท้ายสุด เพื่อให้เลข tile ที่ทาไว้แล้วไม่เลื่อน

### ทา tile (หน้า Mega chunks)

| เครื่องมือ | ทำอะไร |
|---|---|
| **Paint (B)** | ลากทา tile ที่เลือกใน inspector ขนาดแปรง 1-16 tile (`[` `]`), Shift = ลบ, Alt = ดูด tile |
| **Fill (G)** | คลิก = เท chunk ที่อยู่ใต้เมาส์ทั้งช่อง, Shift + คลิก = ตั้ง base tile ของทั้ง mega chunk และล้าง paint (ถ้าเลือกไว้หลายก้อนจะทำทุกก้อน) |
| **Erase (E)** | ลาก tile กลับไปเป็น base tile |
| **Pick (I)** | คลิกเอา tile ใต้เมาส์มาใส่แปรง |

- inspector หัวข้อ **THIS MEGA CHUNK** ตั้ง base tile ของก้อนได้โดย paint ไม่หาย, **Clear Paint** ล้างที่ทาไว้
- floor วาด live ที่ Sorting Order -101 คือใต้ของทุกอย่าง (sprite พื้นของ demo -100, มอส -96, ถนน -92, ลาน -90) ถ้าแมพมี sprite พื้นใหญ่อยู่แล้วจะบัง floor ไว้ ให้ลบ sprite นั้นหรือยก Floor Order ขึ้นในหน้า Mega chunks (ตอนไม่ได้เลือกก้อนไหน)
- ทุกการแก้ทำให้ mega chunk นั้นเป็น **Out of date** (จุดสีม่วง) จนกว่าจะ bake ใหม่

## 7. Bake ต่อ mega chunk

- **Bake All** (หน้า Layers) เขียน `Layers/<layer>/Base/M_x_y/Chunk_i_j.png` ไฟล์แบบเก่าที่วางแบนอยู่ใน `Base/` ถูกย้ายเข้าโฟลเดอร์ของมันเองตอน bake (GUID เดิม) และโฟลเดอร์ paint ของ layer นั้นก็ถูกจัดเหมือนกัน
- **Bake** ใน inspector ของ mega chunk ที่เลือก หรือ **Bake Out of Date (n)** ที่หัวหน้า = bake ใหม่เฉพาะ chunk ใน mega chunk นั้นของทุก ground layer ที่ไม่ได้ Lock chunk ของก้อนอื่นไม่ถูกเขียน (ถ้า layer ยังไม่เคย bake หรือเปลี่ยน chunk size / px ต่อหน่วย จะ bake ทั้ง layer แทน) chunk ที่มีของใหม่แต่ยังไม่มีไฟล์ จะถูก bake ไปด้วยเพื่อไม่ให้มีรู
- sorted layer (กำแพง) ยัง bake ต่อชิ้นเหมือนเดิม ปุ่ม Bake ของ mega chunk ไม่แตะ sorted layer
- จุดสถานะบนแต่ละก้อน: เขียว = Up to date, ม่วง = floor เปลี่ยนหลัง bake, เทา = ยังไม่มีไฟล์ bake ในก้อนนี้
- ของเก่าเก็บใน `History~/<เวลา>_mega` 5 ชุดล่าสุดเหมือนเดิม

## 8. PSD หนึ่งไฟล์ต่อ mega chunk

1. หน้า Mega chunks ปุ่ม **Export PSD per Mega Chunk** (ทุกก้อน) หรือ inspector **Export PSD** (เฉพาะที่เลือก) หรือหน้า Layers **Export > PSD per Mega Chunk (folder)**
2. เลือกโฟลเดอร์ ได้ไฟล์ `M_x_y.psd` ละ 4096 x 4096 px มีทุก layer และชั้น `MapLab Grid (origin ..., 4x4 chunks ...)` เหมือน export เดิม
3. วาดใน layer ชื่อ `<layer>_paint` หรือ `<layer>_paint_<ชื่อ>` (กติกาเดิมของ Bake Layers) **ห้ามย่อ ครอป หรือแก้ชั้น MapLab Grid**
4. **Import PSD** (inspector) หรือ **Import > Folder of Mega Chunk PSDs** เลือกโฟลเดอร์เดิม ระบบอ่านทุกไฟล์ แต่ละไฟล์เขียนเฉพาะ chunk ของ mega chunk ตัวเองลง `Paint/<ชื่อ>/M_x_y/`
- ยัง export ทั้งแมพเป็นไฟล์เดียวได้ (**One PSD for the Whole Map**) สำหรับแมพเล็ก

## 9. Presets (LIBRARY > Presets)

| ชนิด | มาจาก | เก็บอะไร | ไม่เก็บ |
|---|---|---|---|
| Plateau, Box, Flat, Ring, Void | ExtrudedShape (Box = 4 จุดมุมแหลม, Flat = สูง 0) | ความสูง สี texture ขอบ เงา การเรียง collider ความโค้ง material | จุด footprint, ตำแหน่ง, สถานะ bake |
| Path | PathShape | กว้าง สี texture ความยาว texture order | จุดของเส้น |
| Environment | MapLabMap | cell size, obstacle layers, actor layer/order, chunk size, px/หน่วย, mega chunk px, รายการ bake layer (ชื่อ ชนิด sorted), floor palette, tile ต่อด้าน | Bounds, ไฟล์ walkable, set ของ layer, paint, world |

1. ตั้งค่าของชิ้นหนึ่งให้ถูกใจ เลือกมันใน Hierarchy แล้วกด **From Selection** (หัวหน้า Presets) ได้ asset ใน `Assets/MapLab Presets/` (เปลี่ยนที่ด้วยปุ่ม **Folder**) ชิ้นต้นฉบับกลายเป็น user ของ preset
2. คลิก preset ในรายการ แก้ค่าในหัวข้อ SETTINGS ได้เลย (2 คอลัมน์) ถ้าติ๊ก **Keep the N users in sync when I change this** ทุกชิ้นที่ใช้ preset นี้ในฉากที่เปิดอยู่จะเปลี่ยนตามทันที
3. **Create in Scene** สร้างชิ้นใหม่ที่กลาง Scene view ใต้แมพปัจจุบัน (footprint เริ่มต้นตามชนิด), **Apply to Selection** ใส่ค่าให้ชิ้นที่เลือกที่เป็นชนิดเดียวกัน (footprint ไม่เปลี่ยน), **Select Users** เลือกทุกชิ้นที่ใช้
4. **Update from Selection** ดึงค่าจากชิ้นที่เลือกเข้า preset, **Rename** เปลี่ยนชื่อไฟล์, **Delete** ย้ายไปถังขยะ (ชิ้นที่ใช้อยู่เก็บค่าไว้ แค่หลุดลิงก์)
- Environment preset ตอน Apply: เพิ่ม bake layer ที่ยังไม่มี และตั้งชนิดของ layer ชื่อเดียวกัน (ข้าม layer ที่ Lock) ไม่ลบ layer ไม่แตะ set หรือ paint, ถ้าแมพมี world จะเปลี่ยน palette (และ tile ต่อด้านถ้ายังไม่มีก้อนไหนถูกทา)

## 10. ไฟล์ที่เกิดขึ้น

| ไฟล์ | ที่อยู่ | ลบได้ไหม |
|---|---|---|
| `World.asset` | `<ฉาก>_MapLab/<แมพ>/` | ลบ = mega chunk และ floor ที่ทาหาย (ไฟล์ bake ยังอยู่) |
| `Floor Palette.asset` | ที่เดียวกัน (หรือที่เลือกเอง) | ห้ามลบถ้ายังมี world ใช้อยู่ |
| `M_x_y/` | `Layers/<layer>/Base/`, `Paint/<ชื่อ>/` | สร้างใหม่ได้ด้วย bake (paint สร้างใหม่ไม่ได้) |
| `<ชื่อ>.asset` (preset) | `Assets/MapLab Presets/` | ลบได้ ชิ้นที่ใช้อยู่ไม่เสียค่า |

## 11. ผิดบ่อย

- **ปุ่ม Create World เทา**: ฉากยังไม่ save
- **floor ไม่ถูก bake / ยังขึ้นสองชั้น**: ไม่มี ground layer ไหนติ๊ก **Floor tiles** ไปติ๊กที่หน้า Layers แล้ว Bake
- **texture ของ tile ไม่ต่อกัน**: Wrap Mode ของภาพไม่ใช่ Repeat (หน้า Floor tiles เตือนให้)
- **ลบ tile กลางรายการไม่ได้**: ตั้งใจ เลข tile ที่ทาไว้จะผิด ให้เปลี่ยนชื่อ/สีแทน
- **เปลี่ยน Mega Chunk Pixels หรือ Chunk Size แล้วโฟลเดอร์เก่ายังอยู่**: Bake All ใหม่หนึ่งครั้ง ระบบจัดไฟล์เข้าโฟลเดอร์ใหม่ให้
- **Import ไม่เข้า**: ชั้น MapLab Grid ถูกแก้ หรือเปลี่ยนขนาด canvas, หรือชื่อ layer ไม่ตรงกติกา `<layer>_paint...`
- **Tiles per Side เทา**: มีก้อนที่ทาแล้ว ล็อกไว้เพื่อไม่ให้ tile ที่ทาเลื่อน
- **preset ไม่เข้ากับชิ้นที่เลือก**: Plateau/Box/Flat/Ring/Void ใช้กับ shape, Path กับถนน, Environment กับ MapLabMap เท่านั้น

## 12. ตรวจทั้งระบบ (dev)

`Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabWorldProbe.ReportBatch` คัดลอกฉาก demo ไปที่ `Demo/WorldProbe/` แล้ว: สร้าง world, ทา floor, Bake All (ตรวจว่าทุก chunk อยู่ในโฟลเดอร์ `M_x_y`), ทาเพิ่มในก้อนเดียวแล้ว bake เฉพาะก้อนนั้น (ตรวจว่าไฟล์ก้อนอื่นไม่เปลี่ยน), export PSD ต่อ mega chunk (ตรวจ 4096 x 4096), ใส่ paint ในไฟล์หนึ่งแล้ว import ทั้งโฟลเดอร์ (ตรวจว่าลงโฟลเดอร์ของก้อนนั้น), preset (apply, create, sync, environment) แล้วถ่ายภาพ จากนั้นลบสำเนาทิ้งทั้งหมด

ผลอยู่ที่ `Logs/MapLab/MapLab_world.txt`, ภาพ `MapLab_world_shot.png` (แมพที่ bake แล้ว) และ `MapLab_world_map.png` (ภาพย่อ mega chunk แบบที่ viewport วาด), PSD ที่ `Logs/MapLab/WorldProbe/`
