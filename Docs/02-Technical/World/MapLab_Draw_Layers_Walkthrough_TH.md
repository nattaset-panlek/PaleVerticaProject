# MapLab Layers Walkthrough (TH): เส้น Decorate และตารางชนิดวัตถุ

หน้า Layers มีสองเรื่องที่ต้องรู้:

1. **เส้น DECORATE** ใน STACK แบ่ง layer เป็นสองฝั่ง
   - **เหนือเส้น** คือ layer ปกติ ทำงานเหมือนเดิม
   - **ใต้เส้น** คือ **Decorate** ทุกอย่างใน set ของมันกลายเป็นภาพประกอบฉากหลัง มีได้แค่เงา ไม่มี collider และไม่ Y-sort
2. **ตาราง WHAT GOES INTO EACH LAYER** ใช้แทนช่องติ๊กแบบเดิม
   - แถวละหนึ่งชนิดวัตถุ บอกว่าชนิดนั้นคืออะไร และถูกวาดลง layer ไหน
   - หนึ่งชนิดลงได้ layer เดียว

ตั้งค่าทั้งหมดอยู่ที่แมพ (Environment) และเก็บลง Environment preset ได้

- อัปเดต: 2026-10-28
- ดีไซน์: Figma Side-Product `cX1TupNUM5KMKYm0uPGWWy` section **MapLab / Layers page v2: one table + Decorate line** (node `56:44283`) มี 3 การ์ด: layer ปกติ, Decorate, Scene + Inspector
- โค้ด runtime:
  - `Runtime/Shapes/ExtrudedShape.cs`: `SetDrawAsGround(ground, order, shadow)`, `DrawsAsGround`, `GroundOrder`, `GroundShadow`
  - `Runtime/Shapes/RampShape.cs`: ทำตาม Plateau แม่
  - `Runtime/Baking/MapLabGroundProp.cs`: จำค่าเดิมของ prop ไว้คืน และซ่อนเงาของ prop เมื่อปิด Cast shadows
  - `Runtime/Baking/MapLabBakeLayerSettings.cs`: `CastShadows`
- โค้ด editor:
  - `Editor/Layers/MapLabLayerStack.cs`: `DecorateMarker`, `DecorateLayers`, `DrawOrder`, `SetSide`, `Upgrade`
  - `MapLabDrawLayers.cs`: `Sync`, `EnsureShadowLayer`, `OrderOf`
  - `MapLabLayerTypes.cs`: `Owner`, `Targets`, `Route`, `Release`
  - `MapLabLayerClaims.cs`
- หน้าต่าง:
  - `Editor/Window/MapLabLayersView.cs`: หน้าเพจ, STACK, เส้น
  - `MapLabLayerTypesTable.cs`: ตาราง
  - `MapLabLayerDetailsView.cs`: รายละเอียด layer ปกติ, Decorate และ paint
- Inspector: `Editor/PointShapeEditor.cs` (กล่อง Decorate และช่องที่ถูกพักไว้เป็นสีเทา)
- เทสต์: `Tests/Editor/DrawLayerTests.cs`
- ตัวตรวจบนสำเนาฉาก demo: `Editor/MapLabDrawLayerProbe.cs` (ขั้นที่ 8 ของ World Report, dev only)
- เกี่ยวข้อง: [MapLab_Bake_Layers_Walkthrough_TH.md](./MapLab_Bake_Layers_Walkthrough_TH.md), [MapLab_World_Walkthrough_TH.md](./MapLab_World_Walkthrough_TH.md), [MapLab_Elevation_Walkthrough_TH.md](./MapLab_Elevation_Walkthrough_TH.md)

## 1. แนวคิด

```
STACK (บน = วาดทีหลัง)
Walls           tall shapes, Y-sort        <- layer ปกติ
Shadows         shape shadows, prop shadows
Ground_paint    paint
Ground          floor tiles, ground sprites, roads, flat shapes
---- DECORATE · shadows only, no collider ----
Terrace Decor   Terrace, Rubble            <- Decorate: เลือกแค่ set
Far Cliffs      Background
```

**เหนือเส้น (layer ปกติ)**
- แต่ละ layer วาดชนิดวัตถุที่ตารางส่งมาให้
- sorted layer (เช่น Walls) bake ชนิด **Tall shapes** และยังเรียงกับตัวละครเหมือนเดิม

**ใต้เส้น (Decorate)**
- เอา **ทุกอย่าง** ใน set ที่เลือก: Plateau, Ring, Box, prop, sprite พื้น, ถนน
- ทั้งหมดกลายเป็นภาพประกอบฉากหลัง:
  - วาดเหนือพื้น แต่ใต้เงาและใต้ตัวละคร
  - ไม่ Y-sort, ไม่มี collider, เดินบนหลัง Plateau ไม่ได้
  - มีสวิตช์เดียวคือ **Cast shadows**
- ค่าเดิมของวัตถุถูกพักไว้ ย้าย layer ขึ้นเหนือเส้นเมื่อไรก็ได้คืนครบ

**ตาราง WHAT GOES INTO EACH LAYER (แทนช่องติ๊กแบบเดิม)**

| ชนิด | คืออะไร | ไปได้ที่ |
|---|---|---|
| Floor tiles | พื้นตารางที่ระบายในหน้า Floor tiles | layer ปกติ |
| Ground sprites | sprite แบนใต้ตัวละคร เช่น หญ้า ลาย decal | layer ปกติ |
| Roads | path shape | layer ปกติ |
| Flat shapes | Plateau, Ring, Box ที่ไม่มีความสูง | layer ปกติ |
| Tall shapes | กำแพง Plateau เสา ที่บังคนเดินด้านหลัง | sorted layer |
| Shape shadows | เงาของ shape ที่สูง | layer ปกติ |
| Prop shadows | เงาใต้ต้นไม้ หิน ลัง | layer ปกติ |

- **Keep live** แปลว่าไม่ bake ชนิดนั้นเลย วัตถุยังเป็น object ปกติในฉาก
- แถวที่ไฮไลต์สีฟ้าคือชนิดที่อยู่ใน layer ที่เลือกอยู่
- ของใน set ของ Decorate ไม่ผ่านตารางนี้

**Plateau ที่เป็น Decorate เปลี่ยนอะไรบ้าง**

| ก่อน (layer ปกติ) | หลัง (Decorate) |
|---|---|
| ตัดเป็นชิ้นตามแนว Y และ Y-sort กับตัวละคร | ชิ้นเดียว ไม่มี SortingGroup |
| sorting order 6 (เท่าตัวละคร) | order = เงาต่ำสุดของแมพ (ปกติ 1) - 4 x (ลำดับนับจากเส้นลงไป + 1)<br>- layer แรกใต้เส้นได้ -3, layer ถัดไปได้ -7, ...<br>- tier บวกทีละ 1, เงาของตัวเองลบ 1<br>- ผล: อยู่ใต้เงาของ Plateau อื่นทุกอัน |
| มี collider กันทางเดิน, Walk On Top ได้ | ไม่มี collider, Walk On Top ปิด, ramp ก็ไม่มี collider |
| bake ลง sorted layer | bake ลง Decorate layer พร้อมเงาของตัวเอง (ถ้าเปิด Cast shadows) |

**ลำดับตอน bake:** ภาพของ Decorate layer ถูกวางใต้ layer แรกที่มีเงา (เช่น `Shadows`) และเหนือ layer ปกติที่อยู่ต่ำกว่านั้น เงาจึงตกทับของตกแต่งเสมอ

## 2. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| หน้า Layers | **MapLab Window** > BAKES > **Layers** หน้าต่างกว้างเกิน 720 px แบ่งเป็นสองคอลัมน์: ซ้ายคือ STACK, ขวาคือรายละเอียด layer ที่เลือก |
| เลือกว่าชนิดไหนลง layer ไหน | เลือก layer ปกติ แล้วใช้ตาราง **WHAT GOES INTO EACH LAYER** คอลัมน์ **DRAWN INTO LAYER** |
| ทำ Decorate | ปุ่ม **Add Decorate Layer** ใต้ STACK หรือกดลูกศรลงที่ layer ที่อยู่ติดเส้น |
| ย้าย Decorate กลับขึ้นไป | ปุ่ม **Move Above the Line** ในรายละเอียด หรือกดลูกศรขึ้นที่ layer บนสุดใต้เส้น |
| ดูว่า Plateau เป็น Decorate อยู่ไหม | Inspector ของ Plateau มีกล่อง "Decorate · ชื่อ layer" และช่อง Y Sort, Sorting Order, Walk On Top, Generate Collision เป็นสีเทา (kept) |
| ตรวจทั้งระบบบนสำเนาฉาก demo | **Tools > MapLab > Developer > World Report** หรือ `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabWorldProbe.ReportBatch` (ขั้นที่ 8) |

## 3. เลือกว่าชนิดไหนลง layer ไหน

1. คลิก layer ปกติใน STACK (เช่น `Ground`)
2. ดูตาราง **WHAT GOES INTO EACH LAYER** แถวที่ไฮไลต์คือของที่ layer นี้วาดอยู่
3. จะย้ายชนิดไหน ให้กด dropdown ในคอลัมน์ **DRAWN INTO LAYER** ของแถวนั้น แล้วเลือก layer ใหม่ หรือ **Keep live (not baked)**
4. ผล:
   - ชนิดนั้นออกจาก layer เดิมทันที
   - ข้อความใต้ชื่อ layer ใน STACK เปลี่ยนตาม
   - layer ที่เกี่ยวข้องขึ้น Out of date
5. กด **Bake** หรือ **Bake All**

**ส่วนเสริม:** ถ้าอยากให้ layer ปกติเอาเฉพาะของใน set บาง set ให้ใช้หัวข้อ **ONLY FROM SETS** ในรายละเอียด (ไม่จำเป็น: ไม่เลือก = เอาทั้งแมพ)

## 4. ทำ Plateau และ prop ให้เป็นของตกแต่ง (Decorate)

ต้องมี: Plateau หรือ prop อยู่ใน set (เช่น Set "Terrace") ถ้ายังไม่มี ให้สร้าง set แล้วลากของเข้าไป

1. กด **Add Decorate Layer** จะได้ layer ใหม่ชื่อ `Decorate` อยู่ใต้เส้น
   - เปลี่ยนชื่อได้ในหัวข้อ NAME เช่น `Terrace Decor`
   - ชื่อขึ้นต้นด้วย `[` ไม่ได้
2. หัวข้อ **FROM SETS** กด **+ Add Set** แล้วเลือก `Terrace`
   - Decorate ที่ยังไม่มี set จะไม่เอาอะไรเลย
3. ผล:
   - ใน Scene ตัวละครเดินทับ Plateau ได้ และไม่ถูกบัง
   - เงาของ Plateau อื่นตกทับได้
   - หัวข้อ **OBJECTS** แสดงรายชื่อ Plateau และ prop ที่ถูกเอามา พร้อมปุ่มเล็งไปที่ object
4. ตอนสร้าง Decorate ครั้งแรก ถ้าเงายังปนอยู่กับ layer อื่น (เช่น `Ground` ที่เก็บทุกชนิด) ระบบจะแยกเงาออกเป็น layer ใหม่ชื่อ `Shadows` เหนือ layer นั้นให้เอง เพื่อให้เงาตกทับของตกแต่งตอน bake
5. ถ้าไม่อยากให้ของตกแต่งมีเงา ให้ปิด **Cast shadows** ในหัวข้อ WHAT HAPPENS TO THEM
   - Plateau ไม่สร้างเงา
   - prop ซ่อน renderer เงาของตัวเอง (renderer ที่อยู่ใต้ตัวละครใน prop นั้น)
6. กด **Bake** แล้ว bake Walkable area ใหม่ (Plateau ไม่กันทางเดินแล้ว)

**อยากให้กลับเป็นหน้าผาเหมือนเดิม:** กด **Move Above the Line** หรือเอา set ออก

## 5. แมพเก่าที่ใช้เส้น ACTORS

แมพที่ตั้งค่าด้วยระบบเดิม (เส้น `[Actors]` และช่องติ๊ก "draw them as ground") จะถูกแปลงให้เองเมื่อเปิดหน้า Layers หรือ bake

- layer ที่เคยติ๊ก "draw them as ground" ย้ายลงไปใต้เส้น DECORATE และเก็บ set เดิมไว้
- เส้น `[Actors]` หายไป
- layer ที่เคยอยู่เหนือเส้น Actors (overhead) กลายเป็น layer ปกติ
  - ระบบใหม่ไม่มี layer ที่วาดทับตัวละครแล้ว
  - ใบไม้และป้ายที่ order สูงกว่าตัวละครยังวาดเป็นของ live ตามเดิม

## 6. Environment preset

Environment preset เก็บข้อมูลของแต่ละ layer:
- ชนิดที่ส่งมา
- อยู่ใต้เส้นหรือเปล่า
- Cast shadows
- ชื่อ set

ตอน Apply:
1. เพิ่ม layer ที่ไม่มี
2. ย้ายฝั่งให้ตรงกับ preset (ข้าม layer ที่ Lock)
3. จับคู่ set ตามชื่อกับ set ในฉากนั้น (set ที่ไม่มีในฉากถูกข้าม)
4. แยกเงาให้ถ้ามี Decorate แล้ว sync ทันที

## 7. ผิดบ่อย

- **Decorate ไม่เอาอะไรเลย**: ยังไม่ได้เลือก set หรือ set ไม่มี Plateau ระดับบนสุด (tier ทำตามแม่เอง ไม่ต้องอยู่ใน set)
- **ลูกศรลงของ Walls ไม่ขึ้นตอนอยู่ติดเส้น**: sorted layer ลงไปเป็น Decorate ไม่ได้ ให้ย้าย set ของ Plateau ไปใส่ Decorate layer แทน
- **ย้าย layer ลงใต้เส้นแล้วของหายจากตาราง**: ชนิดที่ layer นั้นเคยถืออยู่กลายเป็น Keep live ให้ส่งไป layer อื่นในตาราง
  - ถ้าย้ายกลับขึ้นมา layer นั้นได้ชนิดเดิมคืน เว้นชนิดที่ layer อื่นถือไปแล้ว
- **ชนิด Tall shapes เลือก layer ไม่ได้**: ยังไม่มี sorted layer ให้กด **Add Layer > Sorted layer**
- **ตัวละครยังเดินชน**: ยังไม่ได้ bake Walkable area ใหม่
- **แก้ Y Sort, Sorting Order, Walk On Top หรือ Generate Collision ของ Plateau แล้วไม่เห็นผล**: ตอนเป็น Decorate ค่าเหล่านี้ถูกพักไว้ (ช่องเป็นสีเทาและมีคำว่า kept) จะกลับมาใช้เมื่อย้าย layer ขึ้นเหนือเส้น
- **Prop เป็น Decorate แล้วพุ่มไม้ยังบังตัวละคร**: พุ่มเป็น object แยกที่ไม่ได้อยู่ใต้ prop ใน set เดียวกัน ให้ย้ายพุ่มเข้าไปอยู่ใต้ prop นั้น หรือใส่ set ของพุ่มใน Decorate ด้วย
- **เงาของ Plateau อื่นหายไปบนของตกแต่ง (หลัง bake)**: ไม่มี layer ปกติที่ถือชนิดเงาเลย หรือเงาถูกตั้งเป็น Keep live ให้ส่ง Shape shadows และ Prop shadows ไปที่ layer `Shadows` แล้ว Bake ใหม่
