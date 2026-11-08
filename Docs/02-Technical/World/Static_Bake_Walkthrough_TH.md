# Static Ground Bake Walkthrough (TH)

> **ถูกแทนแล้ว (2026-10-23)** ด้วย [MapLab_Bake_Layers_Walkthrough_TH.md](./MapLab_Bake_Layers_Walkthrough_TH.md): bake เป็นหลาย layer, ตารางช่องยึดกับโลก, paint ที่ bake ใหม่ไม่ทับ เมนูเป็น **Bake All Layers / Unbake All Layers** และหน้า **Layers** ในหน้าต่าง หน้านี้เก็บไว้เป็นบันทึกผลวัดของ prototype (ข้อ 6, 7 ยังใช้ได้)

Prototype: bake ทุกอย่างที่ **นิ่งและอยู่ใต้ตัวละคร** (พื้น ถนน ลาน เงา) ของแมพ MapLab ให้เป็นภาพเป็นช่องๆ (chunk) แล้วซ่อนของจริงไว้ เกมจะวาดภาพไม่กี่ใบแทนของหลายสิบชิ้น และในแมพใหญ่จะเปิดเฉพาะ chunk ที่อยู่รอบกล้อง

- อัปเดต: 2026-10-23 (prototype พิสูจน์แนวคิด ยังไม่ได้ใช้ในเกม)
- โค้ด: `Assets/ThirdParty/MapLab/Editor/MapLabStaticBaker.cs`, `Runtime/Baking/MapLabBakedLayer.cs`, `Runtime/Baking/MapLabChunkStreamer.cs`, ตัววัด `Editor/MapLabBakeProbe.cs`
- เทสต์: `Tests/Editor/StaticBakeTests.cs`
- ภาพรวมโมดูล: [Map_Lab_Walkthrough_TH.md](./Map_Lab_Walkthrough_TH.md)

## 1. แนวคิด

| ชั้น | ตัวอย่าง | bake ได้ไหม |
|---|---|---|
| ใต้ตัวละคร และไม่ขยับ | หญ้า แผ่นพื้น ถนน ลาน ทางแยก เงาของ shape เงาพร็อพ | **ได้** วาดเป็นภาพ chunk |
| ระดับเดียวกับตัวละคร (Y Sort) | กำแพง ด้านบนที่ราบ เสา ลำต้นไม้ ก้อนหิน | ไม่ได้ ต้องเรียงหน้า/หลังกับตัวละครทีละชิ้น ถ้าแบนเป็นภาพเดียว ตัวละครจะอยู่หน้าหรือหลังทั้งภาพ |
| เหนือตัวละคร หรือขยับ | ใบไม้ที่จาง หมอก ฝุ่น (particle) ของที่มี Animator | ไม่ได้ |

- bake เก็บ **สีล้วน** (ปิดไฟจุดทั้งหมด ไฟ global เป็นสีขาว 1.0 และไม่มี post-processing) แล้ว chunk ใช้ material Sprite-Lit ปกติ ไฟ 2D ตอนเล่นจึงยังส่องพื้นเหมือนเดิม และไฟที่ขยับได้ยังใช้ได้
- ของจริงไม่ถูกลบ แค่ซ่อน (renderer ปิด หรือ shape ถูกตั้งเป็น baked) กด **Unbake** แล้วกลับมาแก้ได้ทุกอย่าง

## 2. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| Bake / Rebake / Unbake | **MapLab Window** (Tools > MapLab > MapLab Window) หน้า **Static ground** ทางซ้ายล่าง มีสถานะอัปเดตแล้ว/ล้าสมัยและเหตุผล หรือ Inspector ของ **MapLabMap** ปุ่ม **Bake Static Ground** / **Unbake** |
| ทุกแมพในฉากที่เปิดอยู่ | **Tools > MapLab > Bake Static Ground (Open Scenes)** / **Unbake Static Ground (Open Scenes)** |
| วัดผล live vs baked | **Tools > MapLab > Developer > Static Bake Report** หรือ `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabBakeProbe.ReportBatch` |
| bake ฉาก demo ตอน Unity ปิด | `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabStaticBaker.BakeDemoBatch` |

## 3. Input (ช่องบน MapLabMap)

| ช่อง | ค่าเริ่ม | ความหมาย |
|---|---|---|
| Actor Sorting Layer | Default | sorting layer ของตัวละคร |
| Actor Sorting Order | 6 | order ของตัวละคร ทุกอย่างที่วาด **ต่ำกว่า** (layer ต่ำกว่า หรือ layer เดียวกันแต่ order น้อยกว่า) จะถูก bake |
| Bake Chunk Size | 16 | ขนาด chunk เป็นหน่วยโลก (ปัดให้ขนาด x px/unit เป็นจำนวนเต็ม) |
| Bake Pixels Per Unit | 64 | ความละเอียดของภาพ (เทียบ: กล้อง ortho 6 ที่ 1080p แสดง 90 px ต่อหน่วย) |

กติกาเลือก (ดู `MapLabStaticBaker.IsStaticGround`): เป็น SpriteRenderer หรือ MeshRenderer, เปิดอยู่, ไม่อยู่ใต้ Animator, ถ้าอยู่ใน SortingGroup ใช้ layer/order ของกลุ่ม, และต้องเป็นลูกของ MapLabMap

## 4. ขั้นตอน

1. save ฉากก่อน (ไฟล์ภาพจะไปอยู่ข้างไฟล์ฉาก)
2. เลือก root ที่มี **MapLabMap** ตั้ง Actor Sorting Order ให้เท่ากับ order ของตัวละคร
3. กด **Bake Static Ground** ระบบจะ
   1. Unbake ของเก่า (ถ้ามี) และ Rebuild shape ทุกอัน
   2. เลือก renderer ตามกติกาข้อ 3 แล้วหากรอบที่ครอบทั้งหมด (ชิดพิกเซล)
   3. ปิด renderer อื่นทั้งฉากชั่วคราว ปิดไฟจุด ไฟ global เป็นขาว 1.0 แล้วใช้กล้องชั่วคราว (ไม่มี post) ถ่ายทีละ chunk
   4. เขียน `Chunk_<คอลัมน์>_<แถว>.png` ลง `<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Ground/` (โฟลเดอร์เดียวกับไฟล์ walkable ของแมพนั้น) (chunk ที่ว่างไม่เขียน) ตั้ง import เป็น Sprite, pivot ล่างซ้าย, Clamp, Bilinear, ไม่มี mipmap, บีบอัดคุณภาพสูง
   5. สร้าง `[MapLab] Baked Ground` ใต้ MapLabMap: chunk เป็น SpriteRenderer ที่ order ต่ำสุดของของที่ bake (demo: -100) + `MapLabBakedLayer` + `MapLabChunkStreamer` (ผูกกล้องหลักให้)
   6. ซ่อนต้นฉบับ: renderer ธรรมดาถูกปิด, Path Shape และ Extruded Shape แบบแบนถูกตั้ง Visual Baked, เงาบนพื้นของ Extruded Shape ถูกตั้ง Shadow Baked (ค่าพวกนี้ถูก save และอยู่รอดการ Rebuild)
4. แก้แมพ: กด **Unbake** แก้ แล้ว Bake ใหม่ (ภาพไม่อัปเดตเอง)

## 5. ผลที่ควรได้

```
[MapLab] Environment          MapLabMap
  ...ของจริงทั้งหมด (พื้น ถนน ลาน ถูกซ่อน; กำแพง ต้นไม้ หิน ยังเป็นของจริง)
  [MapLab] Baked Ground        MapLabBakedLayer + MapLabChunkStreamer
    Chunk 0,0 ... Chunk 3,2    SpriteRenderer (order -100)
```

Console: `[MapLab] Baked 68 ground renderers of '[MapLab] Environment' into 12 chunks (4 x 3 of 16 units, 64 px/unit) in Assets/ThirdParty/MapLab/Demo/MapLab_Demo_MapLab/Environment/Ground`

ฉาก demo ถูก bake ไว้แล้วที่ 64 px/unit (กด Unbake ได้)

## 6. ผลวัด (demo, RTX 3050 Ti Laptop, 1080p, ซ่อน particle ทั้งสองฝั่ง)

| | Live | Baked 64 px/unit | Baked 96 px/unit |
|---|---|---|---|
| ของพื้นในมุมเล่น (ortho 6) | 28 renderer / 28 draw call | **4 chunk** | 4 chunk |
| ของพื้นทั้งแมพ | 68 renderer | 12 chunk | 12 chunk |
| ภาพต่างจาก live (เฉลี่ย / สูงสุด / พิกเซลที่ต่างเกิน 12 จาก 255) | - | 0.85 / 97 / 0.79% | 0.52 / 93 / 0.26% |
| หน่วยความจำ texture (Editor วัดได้ / ในเกมราวครึ่งหนึ่ง) | - | 24 MB / ราว 12 MB | 54 MB / ราว 28 MB |
| ไฟล์ PNG บนดิสก์ | - | 1.7 MB | - |
| เวลา bake | - | 4.5 s | 6.0 s |
| เวลาเรนเดอร์ มุมเล่น / ทั้งแมพ | 2.18 / 2.27 ms | 1.84 / 2.30 ms | 2.41 / 2.98 ms |

สรุป:

- **ภาพเหมือนเดิม**: ส่วนต่างเฉลี่ยต่ำกว่า 1 จาก 255 ที่ต่างคือเส้นขอบที่นุ่มลงเพราะภาพถูกขยายจาก 64 เป็นราว 90 px ต่อหน่วย (ดู `Logs/MapLab/MapLab_bake_diff_64.png`) ไม่มีรอยต่อระหว่าง chunk
- **draw call ของพื้นลด 7 เท่า** ในมุมเล่น (28 เหลือ 4)
- **เวลาเฟรมของ demo ไม่ต่างเกิน noise** เพราะพื้นของ demo เบาอยู่แล้ว (sprite ส่วนใหญ่ batch กันได้) และภาพใหญ่ใช้ bandwidth มากขึ้น (96 px/unit ช้ากว่า live ในมุมทั้งแมพ) จะคุ้มชัดเมื่อพื้นมีของซ้อนหลายชั้น (decal, แผ่นพื้น, เงา, รอยทาง หลายร้อยชิ้น) หรือ shader พื้นแพง
- **ราคาคือหน่วยความจำ**: 64 px/unit ราว 4 KB ต่อตารางหน่วย แมพ 1,000 x 1,000 หน่วยจะราว 4 GB จึงต้องโหลด chunk จากดิสก์ตามกล้อง (ข้อ 7)

## 7. Open world: วาดเฉพาะที่เห็น

- Unity ไม่วาด chunk ที่อยู่นอกกล้องอยู่แล้ว (frustum culling)
- `MapLabChunkStreamer` เปิดเฉพาะ chunk ที่ทับกับกล้อง + Margin (ค่าเริ่ม 2 หน่วย) ที่เหลือปิด renderer ทำงานก่อนกล้องเรนเดอร์ แต่เทียบแค่ช่วง index ของ chunk ถ้ากล้องยังอยู่ช่องเดิมจะออกทันที (ไม่มี allocation) รายละเอียดล่าสุดดู [MapLab_Bake_Layers_Walkthrough_TH.md](./MapLab_Bake_Layers_Walkthrough_TH.md) ข้อ 10
- demo: กล้องมุมเล่นเปิด 6 จาก 12 chunk, ย้ายไป 30 หน่วยเหลือ 4
- **ยังไม่ทำ**: การโหลด/ปล่อย texture จริง (ตอนนี้ภาพทุกใบยังถูกอ้างจากฉาก จึงอยู่ในหน่วยความจำทั้งหมด) ขั้นต่อไปคือให้ chunk โหลดผ่าน Addressables ตอนเข้าใกล้ และปล่อยตอนออกห่าง ซึ่งเสียบได้ที่ `ShowArea`

## 8. ข้อจำกัด

| เรื่อง | สถานะ |
|---|---|
| ของที่อยู่ระหว่างช่วงที่ bake แต่ไม่ถูก bake (หมอก -50) | วาดทับ chunk ทั้งหมด (เดิมอยู่ใต้เงาที่ order 1) demo มองไม่ออก |
| ไฟจุดที่ "อบ" ไว้ในพื้น (lightmap) | ไม่ทำ ไฟยังคำนวณตอนเล่นเหมือนเดิม |
| แก้แมพแล้วภาพไม่อัปเดตเอง | ต้อง Unbake แล้ว Bake ใหม่ |
| ความคมเทียบ live | ขึ้นกับ px/unit, 64 นุ่มลงเล็กน้อยที่ 1080p, 96 ใกล้ live แต่ใช้หน่วยความจำ 2.25 เท่า |
| กล้องหมุน / perspective | ไม่รองรับ (streamer ใช้กล้อง orthographic) |

## 9. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| "Save the scene before baking" | ฉากยังไม่เคย save ไม่มีที่วางไฟล์ภาพ |
| พื้นมืดหรือสีเพี้ยนสองเท่าหลัง bake | chunk ไม่ได้ใช้ material Sprite-Lit (ไฟถูกคูณไม่ครบ) หรือ bake ตอนไฟ global ถูกล็อกด้วยสคริปต์อื่น ให้ Bake ใหม่ |
| กำแพงหรือตัวละครหายไปในภาพ chunk | Actor Sorting Order ตั้งสูงเกิน ของที่ order 6 เลยถูก bake ด้วย ตั้งให้เท่ากับ order ของตัวละคร |
| เห็นทั้งของจริงและภาพซ้อนกัน | ต้นฉบับถูกเปิดกลับด้วยมือ กด Unbake แล้ว Bake ใหม่ |
| chunk มุมแมพหายไป | ช่องนั้นไม่มีอะไรเลย (โปร่งทั้งช่อง) ระบบไม่สร้าง ปกติ |
| batch ค้างที่ "Unity is open... Waiting" | ไฟล์ `Temp/UnityLockfile` ค้าง ลบได้เมื่อไม่มี Unity.exe ทำงานอยู่ |
