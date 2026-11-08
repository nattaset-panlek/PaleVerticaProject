# MapLab Bake Layers + Paint Walkthrough (TH)

Bake ของในแมพเป็นภาพแยกเป็น **layer** เลือกได้ว่าแต่ละ layer เอาอะไร แล้วให้คนทำ art **วาดต่อ (paint)** บนภาพที่ bake ใน Photoshop / Krita / Photopea ได้ กด bake ใหม่กี่ครั้งงานวาดก็ไม่หาย มี 2 แบบ:

- **Ground layer**: ของที่อยู่ใต้ตัวละคร (พื้น ถนน ลาน เงา) เป็นภาพช่องใหญ่ (chunk)
- **Sorted layer**: ของที่ Y Sort กับตัวละคร (กำแพง ที่ราบ ชั้นบน เสา) เป็นภาพต่อชิ้น ตัวละครยังเดินหน้า/หลังได้ถูก และวาดลงกำแพงได้

- อัปเดต: 2026-10-23, **ช่วง 1-3 ครบ** (Ground layer + paint, Sorted layer, paint ของ Sorted, Flatten) แทน Static Ground เดิม
- โค้ด: `Assets/ThirdParty/MapLab/Editor/Layers/` (`MapLabLayerBaker`, `MapLabSortedBaker`, `MapLabPieceOwners`, `MapLabSortedManifest`, `MapLabLayerAtlas`, `MapLabLayerFlatten`, `MapLabLayerStack`, `MapLabLayerClaims`, `MapLabLayerNames`, `MapLabLayerFiles`, `MapLabChunkRenderer`, `MapLabPsd`, `MapLabPaintCanvas`, `MapLabPaintExchange`, `MapLabLayerEdits`), หน้าต่าง `Editor/Window/MapLabLayersView.cs`, runtime `Runtime/Baking/` (`MapLabBakeLayerSettings`, `MapLabChunkLayer`, `MapLabSortedLayer`, `MapLabBakedLayer`, `MapLabChunkStreamer`)
- เทสต์: `Tests/Editor/BakeLayerTests.cs`, `Tests/Editor/SortedLayerTests.cs`, `Tests/Editor/StaticBakeTests.cs`
- ตัวตรวจทั้งระบบบนฉาก demo: `Editor/MapLabLayerProbe.cs`, `Editor/MapLabSortedProbe.cs` (dev only)
- ที่มาและเหตุผล: [MapLab_Bake_Layers_Proposal_TH.md](./MapLab_Bake_Layers_Proposal_TH.md), หน้าต่าง: [MapLab_Window_Walkthrough_TH.md](./MapLab_Window_Walkthrough_TH.md)

## 1. แนวคิด: ภาพที่ระบบสร้าง แยกจากภาพที่คนวาด

```
ลำดับวาด (บน = วาดทีหลัง)          ใครเป็นเจ้าของ
Walls_paint_ivy          paint      art   (ตัดลงชิ้นกำแพงให้อัตโนมัติ)
Walls                    sorted     ระบบ  (เรียงกับตัวละคร ไม่ใช่ตามรายการนี้)
Shadows                  bake       ระบบ  (bake ใหม่ = เขียน Base ใหม่)
Ground_paint_moss        paint      art   (ระบบไม่เขียน ไม่ลบ)
Ground_paint             paint      art
Ground                   bake       ระบบ
```

- **Bake layer** = ภาพที่ระบบถ่ายจาก shape / sprite ในแมพ เก็บใน `Base/` เขียนใหม่ทุกครั้งที่ bake (ของเก่าย้ายไป `History~/`)
- **Paint layer** = ภาพที่คนวาด เก็บใน `Paint/<ชื่อ>/` ระบบ**ไม่เคย**เขียนทับ มีกี่ชั้นก็ได้ วางสลับระหว่าง bake layer ได้
- ทุกชั้นใช้ **ตารางช่อง (chunk) เดียวกัน ยึดกับจุด 0,0 ของโลก** ช่อง `(i, j)` คือพื้นที่ `[i x Chunk Size, (i+1) x Chunk Size)` ย้ายของไปกี่ครั้งช่องก็อยู่ที่เดิม งานวาดไม่เลื่อน
- ของจริงไม่ถูกลบ แค่ซ่อน กด **Show Live** ของ layer นั้นแล้วกลับมาแก้ได้ทุกอย่าง

## 2. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| ทุกอย่างของ layer | **Tools > MapLab > MapLab Window** ซ้ายล่าง BAKES > **Layers** หรือปุ่ม **Open Layers** บน Inspector ของ MapLabMap |
| Bake / Unbake ทุก layer ของทุกแมพที่เปิดอยู่ | **Tools > MapLab > Bake All Layers (Open Scenes)** / **Unbake All Layers (Open Scenes)** |
| ตรวจทั้งระบบบนฉาก demo (bake, paint, rebake, export, import) | **Tools > MapLab > Developer > Bake Layers Report** หรือ `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabLayerProbe.ReportBatch` |
| bake ฉาก demo ตอน Unity ปิด | `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabStaticBaker.BakeDemoBatch` |

## 3. หน้า Layers

```
Layers  o Up to date                         [Bake All] [Export v] [Import v]
STACK                                                     top draws last
 (eye) [img] Shadows                          Up to date        ^ v
 (eye)   [brush] Ground_paint_moss             3 chunks         ^ v
 (eye)   [brush] Ground_paint                  4 chunks         ^ v
 (eye) [img] Ground                           Up to date        ^ v
 [Add Layer] [Add Sorted Layer]
LAYER  Ground
 [Rebake] [Show Live] [Lock] [Flatten]
 Name [Ground      ] [Rename]
 WHAT TO BAKE      [x] Ground sprites n  [x] Roads n  [x] Flat shapes n  [ ] Shape shadows 0  [ ] Prop shadows 0   (n = จำนวนของที่ตรง)
 FROM SETS         [ ] Roads  [ ] Plaza ...      (none ticked = everything)
 FILES             Base/ 16 chunks, Paint/ 2 layers, History~/ older bakes [Open]
 [Remove Layer]
SETTINGS          Actor Sorting Layer / Order, Bake Chunk Size, Bake Pixels Per Unit
```

- **ตา**: ซ่อน/แสดงใน Scene view เท่านั้น (ใช้ Scene Visibility ของ Unity ไม่ถูก save, เกมยังวาด) ใช้เทียบงานวาดกับภาพ bake
- **ลูกศรขึ้น/ลง**: ย้ายลำดับวาด ใช้ได้ทั้ง bake และ paint (paint อยู่ใต้ layer ของมันเองก็ได้)
- **Lock**: art เป็นเจ้าของ layer นี้ทั้งหมด ปุ่ม Bake / Show Live หายไป Bake All และ Unbake All ข้าม layer นี้ กด Unlock จะถามยืนยัน เพราะ bake ครั้งถัดไปจะสร้างภาพใหม่จาก shape
- **Flatten**: รวม paint ของ layer ลงภาพ bake แล้ว Lock ให้ (ข้อ 9)
- **กรอบม่วง** ใต้ layer: "The bake changed under paint in N chunks: 1,0 2,0" แปลว่า bake ใหม่แล้ว Base ในช่องที่มีงานวาดเปลี่ยน งานวาดไม่หาย แต่ควรเปิดดูว่ายังเข้ากัน แล้วกด **Mark Reviewed**
- เลือกแถว paint: เห็นจำนวนช่องที่มีงานวาด โฟลเดอร์ และปุ่ม **Delete Paint Layer** (ย้ายโฟลเดอร์ไปถังขยะของระบบ กู้คืนได้)
- ดับเบิลคลิกแถว: เลือก GameObject ของ layer นั้นใน Hierarchy

## 4. Layer เลือกของอย่างไร

ของที่ bake ได้ต้องวาด **ต่ำกว่า** ตัวละคร (Actor Sorting Layer / Order บน MapLabMap) และไม่ขยับ (ไม่อยู่ใต้ Animator ไม่ใช่ particle) ระบบแบ่งเป็น 5 ชนิด:

| ชนิด (ติ๊ก) | คืออะไร | ตัวอย่างใน demo |
|---|---|---|
| Ground sprites | SpriteRenderer / MeshRenderer ธรรมดาบนพื้น | หญ้า แผ่นดิน มอส |
| Roads | Path Shape | ถนน 4 เส้น |
| Flat shapes | Extruded Shape แบบแบน (ไม่ Y Sort) | ลาน ทางแยก |
| Shape shadows | เงาบนพื้นของ Extruded Shape (ลูกชื่อ Shadow) | เงาหน้าผา กำแพง เสา |
| Prop shadows | sprite ใต้ตัวละครที่อยู่ใต้ object เดียวกับของที่วาดระดับตัวละคร | เงาก้อนหิน เงาต้นไม้ |

กติกาเมื่อหลาย layer รับของชิ้นเดียวกันได้:

1. layer ที่ **ติ๊ก Set** ได้ของก่อน (เฉพาะของใน Set ที่ติ๊ก)
2. ที่เหลือไปที่ layer ที่ไม่ติ๊ก Set ตัวที่**ล่างสุด**ในลำดับก่อน
3. ของ 1 ชิ้นอยู่ได้ layer เดียว ของที่ไม่มี layer ไหนรับ ยังเป็นของจริง (ท้ายหน้า Layers บอกจำนวน)

Layer ใหม่ (Add Layer) ยังไม่ติ๊กอะไร จึงไม่แย่งของจาก layer เดิม จนกว่าจะติ๊ก

**Sorted layer** มีชนิดเดียวคือ **Y-sorted shapes**: ทุกชิ้น (piece) ของ Extruded Shape ที่วาดระดับตัวละคร (แถบกำแพง ด้านบน ชั้นบน เสา กำแพงวง หินดำ) กรองด้วย Set ได้เหมือนกัน ชิ้นที่ Y Sort ไปได้เฉพาะ Sorted layer ส่วน Ground layer ไม่เคยรับ (เงาบนพื้นของ shape เดียวกันยังเป็นของ Ground layer)

## 5. ขั้นตอน: bake, วาด, นำเข้า

1. save ฉาก (ไฟล์ภาพไปอยู่ข้างไฟล์ฉาก)
2. เปิดหน้า Layers ตั้ง layer: แมพใหม่มี layer **Ground** (ติ๊กทุกชนิด) ให้ ถ้าอยากแยกเงา กด Add Layer ตั้งชื่อ Shadows ติ๊ก Shape shadows + Prop shadows แล้วเอาสองช่องนี้ออกจาก Ground
3. กด **Bake All** (หรือ Bake ของ layer เดียว) ระบบจะ
   1. คืนของจริงของ layer นั้น Rebuild shape แล้วเลือกของตามข้อ 4
   2. หาว่าของแต่ละชิ้น **ทับช่องไหนจริงๆ** จากรูปสามเหลี่ยมของมัน (ไม่ใช้กรอบ bounds เพราะ mesh ของ shape มีกรอบที่ขยายไว้ให้ Y Sort ซึ่งใหญ่กว่าตัวจริงมาก) ช่องที่ไม่มีอะไรทับเลยจะไม่ถูกถ่าย
   3. ทำ **ลายนิ้วมือของแต่ละช่อง** จากของทุกชิ้นที่ทับช่องนั้น (ตำแหน่ง ขนาด หมุน สี sprite/mesh material texture และ order) แล้วเทียบกับที่จดไว้ครั้งก่อนใน `Data~/Chunks.json`
   4. ถ่าย **เฉพาะช่องที่ลายนิ้วมือเปลี่ยน** (หรือยังไม่เคยถ่าย) ด้วยกล้องชั่วคราว (ปิดไฟจุด ไฟ global ขาว 1.0 ไม่มี post) เขียน `Base/<mega chunk>/Chunk_<i>_<j>.png` ช่องที่ไม่เปลี่ยนใช้ไฟล์เดิม ช่องที่ของหายไปหมดถูกลบ ไฟล์เดิมของช่องที่ถูกเขียนใหม่หรือลบย้ายไป `History~/<วันเวลา>/` (เก็บ 5 ครั้งล่าสุด Unity มองไม่เห็นโฟลเดอร์นี้)
   5. สร้าง `Layer: <ชื่อ>` ใต้ `[MapLab] Bake Layers` แล้วซ่อนของจริง
   6. เทียบช่องที่เขียนใหม่กับของเดิม ช่องที่เปลี่ยนและมีงานวาดอยู่ ขึ้นกรอบม่วง
   7. Console บอกว่าถ่ายใหม่กี่ช่อง ใช้ของเดิมกี่ช่อง เช่น "rendered 3 changed chunks (0 came out empty), kept 13 unchanged, removed 0"

   แก้แมพเล็กน้อยแล้วกด Bake ซ้ำจึงเร็วมาก (ถ่ายแค่ช่องที่ของชิ้นนั้นอยู่ ทั้งตำแหน่งเดิมและตำแหน่งใหม่) ถ้าอยากถ่ายใหม่ทุกช่อง (เช่น หลังเปลี่ยน render pipeline หรือสงสัยว่าภาพไม่ตรง) กด **Rebake Every Chunk** ในส่วน FILES ของ layer นั้น เปลี่ยน Chunk Size หรือ Pixels Per Unit ระบบถ่ายใหม่ทั้งหมดเอง
4. **Export > Photoshop File (.psd)** ได้ไฟล์เดียวทั้งแมพ:
   ```
   MapLab Grid (origin -2 -2, 4x4 chunks of 16 u, 64 px per u)   ซ่อน ล็อก  เส้นช่อง + ชื่อช่อง
   Shadows                ล็อก   ภาพ bake (ดูเทียบ)
   Ground_paint_moss            งานวาดเดิม วาดต่อได้
   Ground_paint
   Ground                 ล็อก
   ```
   ค่าเริ่มเก็บที่ `<โปรเจกต์>/MapLabPaint/` (นอก Assets, Unity ไม่ import) ครั้งต่อไปจำโฟลเดอร์ล่าสุด
5. วาดในโปรแกรม: วาดลง layer ที่ชื่อตาม syntax (ข้อ 6) จะสร้าง layer หรือกลุ่มใหม่ก็ได้ วางลำดับตามที่อยากให้เกมวาด **ห้ามเปลี่ยนขนาด canvas** และอย่าเปลี่ยนชื่อ layer Grid
6. save เป็น PSD (8 bit, RGB) แล้ว **Import > Photoshop File (.psd)** ระบบจะ
   - อ่านเฉพาะ layer / กลุ่ม ที่ชื่อตรง syntax ตัดเป็น `Paint/<ชื่อ>/Chunk_<i>_<j>.png` ช่องที่ลบจนว่างจะถูกลบไฟล์ออก
   - จดลำดับจากไฟล์ลงลำดับของแมพ
   - หน้าต่างสรุปบอกว่านำเข้ากี่ชั้น กี่ช่อง และข้ามอะไรไปเพราะอะไร
7. แก้แมพต่อ: **Show Live** ของ layer ที่จะแก้ แก้ แล้ว **Bake** ของ layer นั้น paint ยังอยู่ครบ

ทางเลือก PNG: **Export > PNG File per Layer** ได้ `01_Ground.png`, `02_Ground_paint.png` ... + ไฟล์ Grid วาดแล้วเก็บไว้ในโฟลเดอร์เดิม (ชื่อไฟล์ใช้ syntax เดียวกัน ตัวเลขหน้าชื่อคือลำดับ) แล้ว **Import > PNG Folder** (PNG ได้ไม่เกิน 16384 px ต่อด้าน PSD ได้ 30000)

## 6. Syntax ชื่อ paint

`<ชื่อ bake layer>_paint` หรือ `<ชื่อ bake layer>_paint_<คำอะไรก็ได้>` ไม่สนตัวพิมพ์เล็ก/ใหญ่

| ชื่อใน PSD | ผล |
|---|---|
| `Ground_paint`, `ground_paint` | paint ของ Ground (ชั้นเดียวกัน) |
| `ground_paint_moss`, `Ground_paint_หญ้า 2` | paint อีกชั้นของ Ground |
| กลุ่มชื่อ `Ground_paint_rocks` | paint 1 ชั้น ทุก layer ที่มองเห็นในกลุ่มรวมกันแบบ Normal |
| `Ground` (ตรงชื่อ bake layer) | ภาพ bake ไว้ดูเทียบ ไม่นำเข้า ใช้แค่ลำดับ |
| `Cave_paint` แต่ไม่มี layer Cave | ข้าม รายงาน "no bake layer called 'Cave'" |
| `sketch idea`, `Ground_painting` | ไม่ตรง syntax ข้าม (ใช้ร่างได้อิสระ) |
| กลุ่มที่ชื่อไม่ตรง เช่น `WIP` | ระบบเข้าไปหา layer ข้างในที่ชื่อตรง syntax |

- ชื่อ bake layer ห้ามมีคำว่า `_paint` และห้ามมีตัวอักษรที่ใช้ตั้งชื่อโฟลเดอร์ไม่ได้
- **Rename** bake layer ในหน้าต่าง: โฟลเดอร์และชื่อ paint ที่ผูกอยู่เปลี่ยนตาม (`Ground_paint_moss` เป็น `Field_paint_moss`) ไฟล์ PSD ที่ส่งออกไปก่อนหน้าต้องแก้ชื่อเอง หรือ Export ใหม่
- paint layer ที่ซ่อนอยู่ในโปรแกรมวาดก็ถูกนำเข้า (ซ่อนไว้แค่ตอนวาด) แต่ layer ที่ซ่อนอยู่**ข้างใน**กลุ่ม paint ไม่ถูกรวม

## 7. Sorted layer (กำแพง ที่ราบ เสา)

ทำไมไม่แบนเป็นภาพเดียว: ตัวละครต้องเดินหน้า/หลังแต่ละชิ้นได้ จึงต้องเป็นภาพต่อชิ้น และแต่ละชิ้นต้องถูกเรียงเหมือนชิ้น mesh เดิมทุกประการ

ตอน bake (กด Bake ของ Sorted layer):

1. คืนของจริง Rebuild shape แล้วเลือกทุกชิ้นที่ Y Sort ตามชนิด/Set
2. ถ่ายทั้ง layer เป็นภาพรวมทีละ chunk (ไฟเหมือน Ground layer) เก็บใน `Data~/Composite/`
3. หาว่า **แต่ละพิกเซลเป็นของชิ้นไหน**: ชิ้นที่วาดทีหลังสุดและทึบตรงพิกเซลนั้น (ส่วนโปร่งอย่างเงาบนชั้นบนไม่แย่งความเป็นเจ้าของ) เก็บใน `Data~/Owners.png`
4. ตัดภาพรวมเป็น `Base/Piece_NNNN.png` หนึ่งไฟล์ต่อชิ้น เอาเฉพาะพิกเซลที่ชิ้นนั้นเป็นเจ้าของ + ขอบเพื่อนบ้าน 1 พิกเซล (กันรอยต่อเป็นเส้นตอนภาพถูกย่อขยาย) ส่วนที่ถูกชิ้นหน้าบังไม่ต้องเก็บ เพราะชิ้นหน้าวาดทับเสมอ แม้ตัวละครจะยืนอยู่ระหว่างกลาง
5. สร้าง `Layer: <ชื่อ>` มีลูก `Piece NNNN` วางที่ **ตำแหน่งเดิมของชิ้น mesh** พร้อม SortingGroup ที่ layer/order เดิม (Unity เรียง SortingGroup ตามตำแหน่ง Transform ของกลุ่ม วัดแล้ว ภาพข้างในจึงตัดพอดีส่วนที่เห็นได้ ไม่ต้องเท่าขอบเขต mesh) ข้างในมี `Base` (SpriteRenderer) และ `Paint: <ชื่อ>` ตาม paint ของ layer
6. สั่ง `SortingGroup.UpdateAllSortingGroups()` หลังสร้าง (ถ้าเขียนสคริปต์ที่สร้างกลุ่มแล้วถ่ายภาพทันทีใน batch mode ต้องเรียกเองก่อน Render ไม่งั้นกลุ่มใหม่ยังเรียงผิด)
7. สร้าง Sprite Atlas `<ชื่อ layer>.spriteatlasv2` ครอบ `Base/` และ `Cut/` ครั้งแรกครั้งเดียว Unity แพ็กทุกชิ้นลง texture เดียว วาดรวมกันได้
8. ซ่อนของจริง (Visual ของ shape) collider ยังเป็นของจริงตลอด

Paint ของ Sorted layer:

- ตั้งชื่อตาม syntax เดิม เช่น `Walls_paint`, `Walls_paint_ivy` วาดบน canvas เดียวกับพื้นใน PSD (layer `Walls` ใน PSD คือภาพรวมของกำแพงทั้งหมดตามที่เห็นในเกม)
- เก็บเป็น chunk ยึดโลกเหมือน paint ของพื้น (`Paint/<ชื่อ>/Chunk_i_j.png` ของ art) แล้วระบบ **ตัดลงแต่ละชิ้นเอง** ตามแผนที่เจ้าของพิกเซล เป็น `Cut/<ชื่อ>/Piece_NNNN.png` (ของระบบ สร้างใหม่ทุกครั้งที่ bake หรือ import)
- ผล: งานวาดบนกำแพงเคลื่อนหน้า/หลังตัวละครไปกับชิ้นกำแพงนั้น ถ้า shape ถูกหั่นชิ้นใหม่ (แก้รูป เปลี่ยน Sort Slice) งานวาดไม่หลุด เพราะต้นฉบับอยู่ในพิกัดโลก
- งานวาดที่อยู่นอกตัวกำแพง (ไม่มีชิ้นไหนเป็นเจ้าของ) ไม่แสดงใน Sorted layer ให้วาดใน paint ของ Ground layer แทน
- ลำดับใน STACK ของ paint ของ Sorted layer: ใช้เรียงเฉพาะระหว่าง paint ของ layer เดียวกัน (ชั้นบนอยู่บน)

## 8. ลำดับการวาด (sorting) ในเกม

Sorted layer เรียงกับตัวละครเหมือนของจริงทุกชิ้น (ข้อ 7) ไม่ขึ้นกับรายการ ส่วน Ground layer ทุก chunk ใช้ sorting layer ของตัวละคร order ถูกแจกตามลำดับใน STACK จากล่างขึ้นบน:

- bake layer ที่ bake แล้ว: ใช้ order **ต่ำสุดของของที่ bake** (demo: Ground = -100, Shadows = 1) ของที่ไม่ได้ bake ซึ่งอยู่ระหว่างนั้น (หมอก -50) ยังอยู่ที่เดิม
- paint: order ถัดจากชั้นใต้มัน +1 (demo: Ground_paint -99)
- bake layer ที่ยัง Live: ของจริงวาดที่ order ของตัวเอง paint ที่อยู่เหนือมันได้ order สูงกว่าของจริงชิ้นบนสุดของ layer นั้น
- ทุกชั้นถูกกดให้อยู่ใต้ Actor Sorting Order เสมอ (stack สูงมากจะถูกเลื่อนลงทั้งกอง)

## 9. Flatten

ใช้เมื่องานวาดของ layer เสร็จแล้ว และอยากให้ภาพนั้นเป็นภาพสุดท้าย:

1. เลือก layer ที่ bake แล้วและมี paint กด **Flatten** (หน้าต่างถามยืนยัน และเตือนถ้ามี paint ของ layer นี้อยู่ห่างจากมันในรายการ เพราะหลังรวมทุกชั้นจะวาดที่ตำแหน่งของ layer)
2. ระบบรวม paint ทุกชั้นของ layer ลงภาพ bake ตามลำดับ (Ground: ทีละ chunk, Sorted: ทีละชิ้น และภาพรวมใน `Data~`)
3. ย้ายโฟลเดอร์ paint (และ `Cut/` ของ Sorted) ไปเก็บที่ `History~/<วันเวลา>_flattened/` แล้ว **Lock** layer
4. ผล: เกมวาดภาพเดียวต่อช่อง/ชิ้น ไม่มี paint แยกแล้ว
5. ถ้าจะกลับไปแก้: Unlock (หน้าต่างเตือนว่า bake ครั้งถัดไปจะสร้างภาพจาก shape ใหม่) แล้วคัดลอกโฟลเดอร์ paint จาก `History~` กลับมาที่ `Paint/` ด้วยมือ

## 10. ไฟล์

```
<โฟลเดอร์ฉาก>/<ชื่อฉาก>_MapLab/<ชื่อแมพ>/
    Walkable.json, Walkable_Mask.png
    Layers/
        Ground/
            Base/Chunk_-2_-2.png ...            ระบบเขียน (bake ใหม่ = เขียนใหม่)
            Paint/Ground_paint/Chunk_0_0.png    art (ระบบไม่เขียน)
            Paint/Ground_paint_moss/...
            History~/20261023_221530/           Base เก่า 5 ครั้งล่าสุด (Unity ไม่ import)
            Data~/Chunks.json                   ลายนิ้วมือของแต่ละช่อง ใช้ตัดสินว่าช่องไหนต้องถ่ายใหม่ (ลบได้ bake ครั้งถัดไปจะถ่ายใหม่ทั้งหมด)
        Shadows/
            Base/...
        Walls/                                  Sorted layer
            Walls.spriteatlasv2                 atlas ของทุกชิ้น (สร้างครั้งแรก)
            Base/Piece_0000.png ...             ภาพต่อชิ้น (ระบบ)
            Cut/Walls_paint_ivy/Piece_0003.png  paint ที่ตัดลงชิ้น (ระบบ สร้างใหม่ทุกครั้ง)
            Paint/Walls_paint_ivy/Chunk_0_0.png art (ระบบไม่เขียน)
            Data~/Composite/Chunk_i_j.png       ภาพรวมของ layer (Export และการเตือนช่องที่เปลี่ยน)
            Data~/Owners.png, Pieces.json       เจ้าของพิกเซล และรายการชิ้น (ตำแหน่ง ขนาด sorting)
            History~/...
```

ฉาก:

```
[MapLab] Environment              MapLabMap (เก็บ layer, ลำดับ, ช่องที่เปลี่ยนใต้งานวาด)
  [MapLab] Bake Layers            MapLabChunkStreamer (เปิดเฉพาะช่องรอบกล้อง ทุก layer)
    Layer: Ground                 MapLabChunkLayer + MapLabBakedLayer (รายการของที่ซ่อน)
    Paint: Ground_paint           MapLabChunkLayer
    Paint: Ground_paint_moss      MapLabChunkLayer
    Layer: Shadows                MapLabChunkLayer + MapLabBakedLayer
    Layer: Walls                  MapLabSortedLayer + MapLabBakedLayer
      Piece 0000                  SortingGroup (order เดิม)
        Base                      SpriteRenderer
        Paint: Walls_paint_ivy    SpriteRenderer (order ในกลุ่ม 1)
```

`MapLabChunkLayer`, `MapLabSortedLayer` และ `MapLabChunkStreamer` เป็น runtime (ใช้ในเกม) ส่วนการ bake, PSD และหน้าต่างเป็น Editor ทั้งหมด

**Streamer ตอนเล่น** (`MapLabChunkStreamer`, ทำงานเฉพาะใน Play mode, เกมไม่ต้องเรียกอะไร):

- ทำงาน **ก่อนกล้องเรนเดอร์ทุกครั้ง** (`RenderPipelineManager.beginCameraRendering` และ `Camera.onPreCull` สำหรับ built-in) จึงใช้ตำแหน่งกล้องล่าสุดของเฟรมเสมอ ไม่ขึ้นกับว่าสคริปต์กล้องของเกมรันก่อนหรือหลัง
- กรอบที่กล้องเห็นคิดจากมุมทั้ง 4 ของกล้องจริง (`ViewportToWorldPoint`) รองรับ Pixel Perfect Camera และ projection ที่ปรับเอง แล้วขยายด้วย Margin (ค่าเริ่ม 2 หน่วย)
- แปลงเป็นช่วงช่อง chunk ถ้ายังเท่าเดิม (กล้องยังอยู่ช่องเดิม) จบทันที ไม่จองหน่วยความจำ
- ช่วงเปลี่ยน: เปิด/ปิด `SpriteRenderer.enabled` ของ chunk ทุก layer และชิ้นของ Sorted layer ด้วยกรอบช่วงช่องเดียวกัน (ชิ้นกำแพงจึงเปิดพร้อม chunk ที่มันอยู่ ไม่ช้ากว่า) ไม่ปิด GameObject เพื่อไม่ให้ SortingGroup ของกำแพงต้องสร้างลำดับใหม่
- กล้องที่ใช้: Viewer ที่ตั้งไว้ตอน bake (กล้อง MainCamera ของฉาก) ถ้าว่าง ใช้กล้องเกมแบบ orthographic ตัวแรกที่เรนเดอร์ กล้องอื่น (เช่น minimap) เห็นสถานะตามกล้องหลัก ถ้ามี minimap ที่มองกว้าง ให้ปิด component นี้ หรือเพิ่ม Margin
- **ไม่ได้ลดหน่วยความจำ**: ภาพทุกใบยังอยู่ใน RAM/VRAM เพราะฉากอ้างถึง และของจริงที่ถูก bake ยังอยู่ในฉาก (ซ่อนไว้) ช่วยแค่เวลาวาด/culling ต่อเฟรม

## 11. ผลที่ควรได้ (ฉาก demo)

ผลจาก `MapLabLayerProbe` (2026-10-23, `Logs/MapLab/MapLab_layers.txt`, ภาพ `Logs/MapLab/MapLab_layers.png`):

| ขั้น | ผล |
|---|---|
| Bake All | Ground: 41 ชิ้น เป็น 16 chunk (4.8 s), Shadows: 27 ชิ้น เป็น 7 chunk (1.5 s), Walls (Sorted): 9 shape 42 ชิ้น เป็น 42 ภาพ + atlas (7-10 s) ทั้งหมด 16 หน่วย 64 px/unit |
| Walls เทียบของจริง | วางตัวละครจำลอง 28 ตัว (หน้า หลัง กลาง ข้าง ของทุก shape) ถ่ายก่อนและหลัง bake: ทุกตัวอยู่หน้า/หลังตรงกัน ภาพต่างเฉลี่ย 0.17 (มุมลาน) และ 0.45 (ทั้งแมพ) จาก 255 ที่ต่างคือลาย texture ที่นุ่มลงจากการย่อขยาย |
| Flatten | Shadows (paint ชั่วคราว): 2 chunk ถูกเขียนใหม่, Walls: paint ถูกตัดลง 6 ชิ้น แล้วรวมเข้า 7 ภาพ ทั้งสอง layer ถูก Lock, โฟลเดอร์ paint ย้ายไป `History~` ครบ จากนั้น Unlock + Rebake กลับเป็นปกติ |
| นำเข้า PSD ตัวอย่าง | 3 paint layer (`Ground_paint` 4 chunk, `Ground_paint_moss` จากกลุ่ม 3 chunk, `Walls_paint_ivy` ตัดลงกำแพง 5 ชิ้น); ข้าม `Cave_paint` (ไม่มี layer Cave) และ `sketch idea` (ไม่ตรง syntax) |
| ลำดับ | Ground, Ground_paint, Ground_paint_moss, Shadows, Walls, Walls_paint_ivy |
| Rebake Ground และ Walls | ไฟล์ paint ไม่เปลี่ยนเลย (เทียบ hash ทุกไฟล์), paint ของกำแพงถูกตัดลงชิ้นใหม่ครบ, ไม่มีช่องถูกเตือน |
| ขยับลาน 0.5 หน่วย แล้ว Rebake | เตือน 2 ช่องที่มีงานวาด (0,-1 และ 0,0); ย้ายกลับ + Mark Reviewed เหลือ 0, paint ยังไม่เปลี่ยน |
| Export PSD | 4096 x 4096 px, 7 layer: Ground (ล็อก), Ground_paint, Ground_paint_moss, Shadows (ล็อก), Walls (ล็อก, ภาพรวมกำแพง), Walls_paint_ivy, MapLab Grid (ล็อก ซ่อน) |
| Import ไฟล์ที่ Export | paint ได้ไฟล์เหมือนเดิมทุก byte |
| Order ในเกม | Ground -100, Ground_paint -99, Ground_paint_moss -98, Shadows 1, ตัวละคร 6 (หมอก -50 อยู่ระหว่าง Ground กับ Shadows เหมือนตอน live) |

เทียบกับ prototype เดิม (`MapLabBakeProbe`, ซ่อน particle และ paint ทั้งสองฝั่ง):

| | Prototype (1 ภาพรวม, ตารางตามขอบของ) | Layers (Ground + Shadows + Walls, ตารางยึดโลก) |
|---|---|---|
| ภาพทั้งแมพ (64 px/unit) | 12 chunk | 23 chunk + 42 ภาพกำแพง (ใน atlas) |
| chunk ที่วาดในมุมเล่น | 4 | 8 |
| หน่วยความจำ texture ของ chunk (Editor วัด) | 24 MB | 46 MB (ภาพกำแพงทั้งหมดราว 1 MB บนดิสก์) |
| ภาพต่างจาก live (เฉลี่ย / สูงสุด) | 0.85 / 97 | 1.21 / 110 (กำแพงถูก bake ด้วย จึงนุ่มลงเพิ่ม; ที่ 96 px/unit 0.80 / 123) |
| เวลาเรนเดอร์ มุมเล่น / ทั้งแมพ | 1.84 / 2.30 ms | 1.74 / 2.00 ms (live 2.15 / 2.43) |
| เวลา bake ทั้งหมด | 4.5 s | 14.3 s |

ราคาของการแยก layer: ทุก layer มี chunk ของตัวเอง layer เงาที่โปร่งเกือบทั้งช่องก็ยังเป็นภาพเต็มช่อง ถ้าไม่ต้องการ paint แยกระหว่างพื้นกับเงา ให้ใช้ layer เดียว (ติ๊กทุกชนิด) จะได้จำนวน chunk ใกล้ prototype

## 12. ข้อจำกัด

| เรื่อง | สถานะ |
|---|---|
| Sorted layer ช้ากว่า Ground layer | ถ่ายภาพรวม + หาเจ้าของพิกเซล + ตัดและ import ทีละชิ้น demo 42 ชิ้นใช้ 7-10 s (import ทุกชิ้นรวดเดียวแล้ว) |
| draw call ของ Sorted layer | ยังเป็นชิ้นตามแถบเรียง ลดได้ไม่มากเท่า Ground layer (ชิ้นละ 1 draw ไม่ใช่ 2 เหมือน mesh, และรวม batch ได้เมื่อชิ้นติดกันในลำดับและอยู่ใน atlas เดียว) ประโยชน์หลักคือวาดลงกำแพงได้ |
| สคริปต์ที่สร้าง/ย้าย SortingGroup แล้ว Render ทันที (batch, เครื่องมือถ่ายภาพ) | ต้องเรียก `SortingGroup.UpdateAllSortingGroups()` ก่อน ไม่งั้นได้ลำดับเก่า (ใน Editor ปกติและในเกม Unity อัปเดตให้ทุกเฟรม) |
| Blend mode อื่นนอกจาก Normal, clipping mask, layer mask | รวมแบบ Normal / ไม่อ่าน mask หน้าต่างสรุปบอกชื่อ layer ให้ Merge / Apply ก่อน |
| Adjustment layer, layer effect, smart object | ไม่อ่าน ให้ Rasterize ก่อน |
| PSD 16 / 32 bit, CMYK, PSB, ZIP | ไม่รับ บอกเหตุผลตอนนำเข้า (ตั้ง 8 Bits/Channel + RGB Color) |
| เปลี่ยน Bake Pixels Per Unit หลังวาด | นำเข้าไฟล์เก่าไม่ได้ (บอกให้ตั้งค่ากลับ) paint เดิมในแมพยังแสดงถูกที่ แค่ความละเอียดต่างจาก Base |
| เปลี่ยน Bake Chunk Size หลังวาด | PSD เก่ายังนำเข้าได้ (ระบบตัดช่องใหม่จากพิกัดโลก) แต่ paint chunk เดิมบนดิสก์จะไม่ตรงช่องใหม่: Export ก่อนเปลี่ยน แล้ว Import หลังเปลี่ยน |
| canvas ใหญ่ | ทั้งแมพเป็น canvas เดียว demo 4096 x 4096 px; เกิน 30000 px ต่อด้านต้องลด px/unit หรือ Bounds |
| กลับจาก Flatten | ไม่มีปุ่ม: Unlock แล้วคัดลอก paint จาก `History~/<เวลา>_flattened/` กลับเอง |

## 13. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| "Save the scene before baking" | ฉากยังไม่เคย save |
| ตอนเล่น กำแพงหรือถนนที่ bake โผล่ช้าเมื่อเดินกลับมา | แก้แล้ว 2026-10-24 (ชิ้นกำแพงเคยรอให้กล้องข้ามเส้น chunk ก่อนถึงจะเปิด) ถ้ายังเห็นที่ขอบจอ: กล้องเลื่อนเร็วมาก หรือกล้องที่เรนเดอร์ไม่ใช่ Viewer ให้เพิ่ม Margin บน `[MapLab] Bake Layers` หรือลากกล้องหลักใส่ Viewer |
| แก้ของแล้วกด Bake แต่ภาพไม่เปลี่ยน ("rendered 0 changed chunks") | สิ่งที่แก้ไม่ได้อยู่ในลายนิ้วมือ (เช่น ค่าใน shader ที่ตั้งจากสคริปต์อื่น) กด Rebake Every Chunk |
| layer ใหม่ bake แล้ว "nothing to bake" | ยังไม่ติ๊กชนิด หรือ layer ล่างกว่ารับของไปหมดแล้ว (ข้อ 4) |
| นำเข้าแล้ว "No paint layer found" | ไม่มี layer ไหนชื่อตรง syntax ดูข้อ 6 |
| "The file is W x H px but ..." | ขนาด canvas ถูกเปลี่ยน หรือแมพโตขึ้นหลัง Export: Export ใหม่ แล้วย้ายงานวาดไปไฟล์ใหม่ |
| "exported at 64 px/unit and the map now bakes at 96" | ตั้ง Bake Pixels Per Unit กลับ หรือ Export ใหม่ |
| งานวาดหายหลัง Rebake | ไม่ควรเกิด: Rebake เขียนแค่ `Base/` ดูว่าไฟล์ PSD ถูก Import หลังวาดแล้วหรือยัง (งานที่ยังอยู่แค่ใน PSD ไม่อยู่ในเกม) |
| กรอบม่วงไม่หาย | กด Mark Reviewed หลังเปิดดูแล้ว ระบบสะสมช่องที่เปลี่ยนไว้จนกว่าจะกด |
| Sorted layer: "nothing to bake" | ไม่มี shape ที่ Y Sort ตรงกับ Set ที่ติ๊ก หรือ shape ถูกตั้ง Y Sort ปิดและ order ต่ำกว่าตัวละคร (แบบนั้นเป็น Flat shapes ของ Ground layer) |
| วาดลงกำแพงแล้วบางส่วนหาย | ส่วนที่อยู่นอกตัวกำแพงไม่มีชิ้นไหนเป็นเจ้าของ (ข้อ 7) ให้วาดส่วนนั้นใน paint ของ Ground layer |
| เห็นเส้นรอยต่อระหว่างชิ้นกำแพง | ไม่ควรเกิด (ระบบเผื่อขอบ 1 พิกเซล) ถ้าเห็น ให้ตรวจว่า import ของภาพชิ้นเป็น Bilinear + Clamp และไม่มี mipmap (ระบบตั้งให้ตอน bake) |
| ฉากเก่ามี `[MapLab] Baked Ground` | bake จากรุ่นก่อน กด Bake All หรือ Unbake All ครั้งเดียว ระบบคืนของจริงแล้วลบทิ้งให้ โฟลเดอร์ `Ground/` เก่าลบเองได้ |
