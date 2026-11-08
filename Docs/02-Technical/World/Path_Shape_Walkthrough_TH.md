# Path Shape Walkthrough (TH)

เส้นทาง (ถนน ทางดิน ลำธารแบน) ของ MapLab: ลากจุดเป็นเส้นโค้งเปิด แล้วระบบสร้างแถบกว้างตาม Width พร้อม texture ไล่ตามความยาว ใช้แทน SpriteShape (MapLab ไม่ใช้ SpriteShape แล้ว)

- อัปเดต: 2026-10-23
- โค้ด: `Assets/ThirdParty/MapLab/Runtime/Shapes/PathShape.cs` (ใช้ฐาน `PointShape` ร่วมกับ Extruded Shape), Editor `PathShapeTool`, `PointShapeEditor`, `ShapePointHandles`, `ExtrudedShapeMenu.CreatePath`
- ภาพรวมโมดูล: [Map_Lab_Walkthrough_TH.md](./Map_Lab_Walkthrough_TH.md)

## 1. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| สร้าง | **GameObject > MapLab > Path** |
| แก้จุด | ปุ่ม **Edit Points** ใน Inspector หรือเครื่องมือไอคอนถนนใน Tools overlay ของ Scene view |
| ตัวอย่าง | `[MapLab] Environment/Ground/Paths` ในฉาก demo (Road West, Road East, Branch North, Branch South) |

## 2. ค่าใน Inspector

| ช่อง | ความหมาย |
|---|---|
| Smoothness | จุดย่อยต่อช่วงโค้ง |
| Width | ความกว้างของแถบ (หน่วยโลก) |
| Color | สีคูณกับ texture |
| Texture | ภาพแถบ: แกนนอนของภาพไล่ตามความยาวเส้นทาง แกนตั้งคือความกว้าง (ล่าง = ขวาของเส้น, บน = ซ้าย) demo ใช้ `MapLab_PathEdge` (ทางดินมีขอบ ด้านนอกโปร่งใส) |
| Texture Length | ภาพ 1 รอบยาวกี่หน่วยตามเส้นทาง |
| Material | `MapLab_ShapeLit.mat` |
| Sorting Order | ต่ำกว่าตัวละครเสมอ (demo: -92) |

ไม่มี collider (เส้นทางเดินได้)

## 3. ขั้นตอน

1. สร้าง Path แล้วกด **Edit Points**
2. ลากลูกศร/สี่เหลี่ยม **Move all points** กลางเส้นเพื่อย้ายทั้งเส้น (ไม่ขยับ pivot หรือ Position), ลากสี่เหลี่ยมสีขาว, คลิกจุดเขียวเพื่อเพิ่มจุด, **Shift** + คลิกเพื่อลบ (ต้องเหลืออย่างน้อย 2), **Ctrl/Cmd** + คลิกเพื่อทำมุมหัก
3. ปรับ Width และ Texture Length ให้ลายไม่ยืด
4. ทางแยก: ให้ปลายเส้นทางจมอยู่ใต้พื้นแบน (Extruded Shape Height 0 ที่ order สูงกว่า) รอยต่อจะถูกบัง แบบ `Junction` ใน demo

## 4. ผลที่ควรได้

```
Road West (PathShape)
  Strip     MeshRenderer แถบเดียว (ลูกที่ระบบสร้างเอง ไม่ถูก save)
```

## 5. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| ลายยืดหรือหด | Texture Length ไม่ตรงกับสัดส่วนภาพ (ภาพ 256 x 192 px ที่ 128 px/หน่วย = 2 หน่วยต่อรอบเมื่อ Width 1.5) |
| โค้งแคบแล้วแถบพับ | โค้งแคบกว่าครึ่ง Width เพิ่มจุดหรือลด Width |
| เส้นทางอยู่บนตัวละคร | Sorting Order สูงเกิน |
| ภาพขาวทั้งแถบ | ไม่ได้ใส่ Texture |
