# MapLab Performance (TH)

ผลวัดจริงของ MapLab และเครื่องมือวัด (**Tools > MapLab > Developer > Performance Report**)

- วัดเมื่อ: 2026-10-23, Unity 6000.4.8f1, NVIDIA GeForce RTX 3050 Ti Laptop, 1920 x 1080, batch mode
- ฉาก: `Assets/ThirdParty/MapLab/Demo/MapLab_Demo.unity` (แมพ 50 x 34 หน่วย: Plateau 3 ขั้น, Void Rock, Ring Wall, เสา 4, ลาน, ทางแยก, ถนน 4 เส้น, พร็อพ sprite, ไม่มีตัวละคร)
- โค้ดวัด: `Assets/ThirdParty/MapLab/Editor/MapLabPerformanceProbe.cs`
- ผลดิบ (รอบล่าสุด): `Logs/MapLab/MapLab_performance.txt`

## 1. สรุป: ก่อนและหลังปรับ

| เรื่อง | รอบแรก | หลังรวมชิ้น | หลังเพิ่มเงา (ปัจจุบัน) |
|---|---|---|---|
| Draw call ของ shape | 181 | **50** (-72%) | 57 (+7 เงาบนพื้น) |
| SortingGroup | 119 | 42 | 42 |
| Mesh renderer | 125 | 48 | 55 |
| Vertex | 18,602 | 7,606 | 9,074 |
| หน่วยความจำ mesh | 922 KB | 374 KB | 447 KB |
| สร้าง mesh ทั้งฉาก (โหลด / Play ครั้งเดียว) | 26.4 ms | 16.6 ms | 19.9 ms |
| ลาก slider บน Plateau 3 ขั้น | 15.9 ms | 7.9 ms | 12.3 ms |
| เรนเดอร์ shape ที่มุมเล่น (ortho 6) | +0.88 ms | +0.34 ms | +0.12 ms |
| เรนเดอร์ shape ทั้งแมพ (ortho 17) | +1.74 ms | +0.39 ms | +0.56 ms |
| CPU ต่อเฟรม (script) | 0 | 0 | 0 (ไม่มี `Update`) |
| Bake พื้นที่เดิน (Editor) | 2.9 ms | 3.4 ms | ไม่เปลี่ยน |
| NavMesh ของตัวอย่าง (ครั้งเดียวตอน Start) | 14.2 ms | 14.6 ms | 18.7 ms |

ค่าเวลาแกว่งได้ราว 15 ถึง 30% ระหว่างรอบ (NavMesh ไม่ได้แก้โค้ดเลยแต่ขยับจาก 14.2 เป็น 18.7) ให้ดูแนวโน้ม ไม่ใช่ทศนิยม

## 2. ทำอะไรไปบ้าง

1. **ด้านบนที่สูงกว่าตัวละครเป็นชิ้นเดียว** (ช่อง Actor Height ค่าเริ่ม 1.2, ตัวละคร Pale Vertica สูงราว 0.87) เหลือตัดแถบเฉพาะกำแพง ส่วนขั้นบนที่ฐานสูงกว่า Actor Height เป็นชิ้นเดียวทั้งก้อน เหตุผลอยู่ใน [Extruded_Shape_Walkthrough_TH.md](./Extruded_Shape_Walkthrough_TH.md) หัวข้อ 5
2. **lip และโพรงดำอยู่ใน draw call เดียวกับด้านบน** shader อ่าน UV ช่อง 1 เพื่อรู้ว่า vertex ไหนเป็นสีเรียบ submesh ลดจาก 4 เหลือ 2 (กำแพง, ด้านบน)
3. **เงา flat card** รูปบนพื้นละ 1 renderer (+1 draw call) ขั้นบนใส่เงาไว้ใน piece เดิม (+0) สร้างเฉพาะขอบที่หันหนีแสง 4 vertex ต่อขอบ ไม่มีไล่สี
4. **แก้บั๊กการเรียง**: renderer ในชิ้นเคยเป็น order 0 ในกลุ่ม order 6 ทำให้ Unity วาดเงาที่ order 1 ทับรูป (รูปดูโปร่ง) ตอนนี้ตั้งเท่ากันแล้ว

## 3. รายละเอียด shape (ปัจจุบัน)

| Shape | วาดเป็น | ชิ้น | Draw call | Vertex | Rebuild ms |
|---|---|---|---|---|---|
| Plateau (รวม 2 ขั้นบน) | ด้านบน 1 ชิ้น + แถบกำแพง + เงา | 11 | 11 | 1,646 | 12.3 (รวมขั้นบน) |
| Tier 2 / Tier 3 | ชิ้นเดียว (เงาอยู่ข้างใน) | 1 / 1 | 2 / 2 | 1,646 / 1,646 | 9.4 / 4.7 |
| Void Rock | ด้านบน 1 ชิ้น + แถบกำแพง + เงา | 10 | 10 | 1,168 | 2.1 |
| Ring Wall | ด้านบน 1 ชิ้น + แถบกำแพง + เงา | 14 | 14 | 1,400 | 1.5 |
| เสา Box (ต่อต้น) | ด้านบน + กำแพง + เงา | 3 | 3 | 18 | 0.2 |
| ลาน / ทางแยก (แบน) | ชิ้นเดียว ไม่มีเงา | 1 | 1 | 894 / 234 | 1.6 / 0.9 |
| ถนน (ต่อเส้น) | ชิ้นเดียว | 1 | 1 | 82 ถึง 102 | 0.05 |

Sort Slice ตอนนี้มีผลกับกำแพงเท่านั้น: Plateau ชั้นล่าง 0.25 = 22 draw call, 0.5 = 14, 1 = 10, 4 = 7 (รวมขั้นบน)

## 4. ที่ยังแพงอยู่ และวิธีลดต่อ

| # | เรื่อง | วิธี | ราคา |
|---|---|---|---|
| 1 | แถบกำแพงยังเป็น 1 draw call ต่อแถบ (Ring 13, Plateau 9) | ตั้ง Sort Slice เป็น 1 สำหรับกำแพงยาวๆ (-30% ของกำแพง) | ไม่ต้องเขียนโค้ด ลำดับหยาบขึ้น |
| 2 | MaterialPropertyBlock ทำให้ไม่เข้า SRP Batcher | ใช้ material instance ต่อชุด texture แทน | ต่ำ |
| 3 | สร้าง mesh ใหม่ทุกครั้งที่โหลดฉาก (ราว 20 ms) | bake mesh ลง asset ตอน build | กลาง |
| 4 | ขั้นบนแพงขึ้นตอนแก้ (Tier 2 จาก 5 เป็น 9 ms) เพราะเงาต้องตัดกับขอบของแม่ (ขอบ x ขอบ) และสุ่มเส้นขอบของแม่ซ้ำ | cache footprint ของแม่ระหว่าง Rebuild ครั้งเดียว | ต่ำ |
| 5 | NavMesh ของตัวอย่างสร้างตอน Start | bake NavMesh ลงไฟล์คู่กับ JSON | ต่ำ (ฝั่งตัวอย่าง) |

ถ้าแมพใหญ่ขึ้น 4 เท่าที่ความหนาแน่นเท่ากัน คาดว่าราว 230 draw call และโหลดราว 80 ms ใช้ได้บน PC ถ้าจะลงมือถือควรทำข้อ 1 ถึง 3

## 5. ข้อควรรู้เรื่องการวัด

- เวลาเรนเดอร์รอ GPU ทุกเฟรม (อ่าน pixel 1 จุด) จึงสูงกว่าตอนเล่นจริงที่ CPU กับ GPU ทำงานซ้อนกัน ใช้ดูส่วนต่าง (มี shape กับซ่อน shape) มากกว่าค่าจริง และส่วนต่างที่ต่ำกว่า 0.5 ms อยู่ในระดับเดียวกับ noise
- ฉาก demo ไม่มีตัวละคร แสงจุดมีน้อย ฉากเกมจริงจะหนักกว่านี้ทั้งก้อน
- ยืนยันด้วย Profiler และ Frame Debugger ใน Play mode ก่อนตัดสินใจ optimize ต่อ

## 6. รันเครื่องมือวัด

| ทำอะไร | ที่ไหน |
|---|---|
| วัดใน Editor | **Tools > MapLab > Developer > Performance Report** (เปิดฉาก demo วัด แล้วเปิดฉากใหม่โดยไม่ save) |
| ตอน Unity ปิด | `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabPerformanceProbe.ReportBatch` |
| ผล | `Logs/MapLab/MapLab_performance.txt` และ Console |

สิ่งที่วัด: แต่ละ shape/path วาดแบบไหน (Drawn as), จำนวนชิ้น draw call vertex (รวมเงา) และเวลา Rebuild (median 20 ครั้ง), Rebuild ทั้งฉาก, Sort Slice 0.25 ถึง 4 บน Plateau ชั้นล่าง, เวลา bake, เวลาสร้าง NavMesh ของตัวอย่าง และเวลาเรนเดอร์ 1080p สองมุม (ทั้งหมด / ซ่อน shape)

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| "Build the MapLab demo scene first" | ยังไม่มีฉาก demo ให้ Build Demo Scene ก่อน |
| ไม่มีหัวข้อ NavMesh | ลบโฟลเดอร์ Samples ไปแล้ว |
| เวลาแกว่งมาก | Unity กำลัง import/compile อยู่ หรือเครื่องทำงานอื่น ให้รันซ้ำ |
| batch ค้างที่ "Unity is open... Waiting" ทั้งที่ปิด Unity แล้ว | มีไฟล์ `Temp/UnityLockfile` ค้างจากรอบที่ compile ไม่ผ่าน ลบได้เมื่อแน่ใจว่าไม่มี Unity.exe ทำงานอยู่ |
