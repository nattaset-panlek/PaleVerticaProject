# Walkable Area Bake Walkthrough (TH)

ผลลัพธ์หลักของ MapLab: กด Bake บนแมพ แล้วได้ **ไฟล์ข้อมูลพื้นที่เดินได้** (JSON + ภาพ mask) ใครจะเอาไปสร้าง NavMesh, A* grid หรือให้ agent เดินแบบไหนก็ได้ โดย **อ่านไฟล์อย่างเดียว ไม่ต้องอ้างโค้ดของ MapLab** การเดินของ agent เป็นเรื่องนอก MapLab ทั้งหมด

- อัปเดต: 2026-10-24 (หน้า Walkable area แบบ 4 ขั้น, กรอบลากได้ใน Scene view, บอกว่าอะไรเปลี่ยน; ต่อมาเพิ่มการ์ด **Walk on top** เป็นขั้นที่ 3 และไฟล์ `Walkable_Elevation.json` ดู [MapLab_Elevation_Walkthrough_TH.md](./MapLab_Elevation_Walkthrough_TH.md))
- โค้ด: `Assets/ThirdParty/MapLab/Runtime/MapLabMap.cs`, `Assets/ThirdParty/MapLab/Editor/MapLabWalkableBaker.cs` (+ `WalkableAreaJson`, `WalkableAreaTypes`, `MapLabMapEditor`), หน้าใน window `Editor/Window/MapLabBakeView.cs`, ตัวช่วย `Editor/Walkable/` (`MapLabWalkableArea` = Fit to Map และกริด, `MapLabWalkableObstacles` = นับ collider, `MapLabWalkableChanges` = บอกว่าอะไรเปลี่ยน, `MapLabWalkableScene` = กรอบและช่องแดงใน Scene view)
- แบบใน Figma: section **MapLab / Walkable page (6 options)** ใช้ Option 1 Four steps
- ภาพรวมโมดูล: [Map_Lab_Walkthrough_TH.md](./Map_Lab_Walkthrough_TH.md)
- ตัวอย่างผู้ใช้ข้อมูล: [Navigation_Switch_Walkthrough_TH.md](./Navigation_Switch_Walkthrough_TH.md)
- ผู้ใช้ข้อมูลฝั่งเกม (AI ของ Pale Vertica, ไม่ใช่ส่วนของ MapLab): [Walkable_Grid_Pathfinding_Walkthrough_TH.md](./Walkable_Grid_Pathfinding_Walkthrough_TH.md)

```
แมพ 1 แมพ = GameObject 1 ก้อน (เช่น [MapLab] Environment) ที่มี MapLabMap
        |  Bake (Editor เท่านั้น)
        v
<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Walkable.json        กริด เดินได้/ไม่ได้ + โพลิกอน (ขอบแมพ + รูของสิ่งกีดขวาง)
<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Walkable_Mask.png   ภาพขาวดำ 1 พิกเซลต่อช่อง ไว้ดูด้วยตา หรืออ่านแทน JSON ก็ได้
        |
ใครก็อ่านได้ (NavMesh, A*, flow field, ฯลฯ)
```

## 1. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| ตั้งค่าและ Bake แมพเดียว (ทางหลัก) | **Tools > MapLab > MapLab Window** เลือกแมพที่ toolbar แล้วกด **Walkable area** ใต้ BAKES ทางซ้าย (หรือปุ่ม **Open MapLab Window** บน MapLabMap) หน้านี้มี 5 ขั้นเรียงจากบนลงล่าง ดูหัวข้อ 3 |
| Bake เร็วจาก Inspector | เลือก GameObject ที่มี **MapLabMap** แล้วกดปุ่ม **Bake Walkable Area** (ภาพ mask แสดงใต้ปุ่มหลัง bake) |
| Bake ทุกแมพในฉากที่เปิด | **Tools > MapLab > Bake Walkable Area (Open Scenes)** |
| ฉาก demo | **Tools > MapLab > Developer > Build Demo Scene** bake ให้อัตโนมัติ |
| ตอน Unity ปิด | `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabSceneBuilder.BuildAndRenderBatch` (สร้าง demo + bake + เรนเดอร์) |
| ดูผลทับบนฉาก | `Logs/MapLab/MapLab_walkable.png` จาก **Tools > MapLab > Developer > Render Demo Preview** (ช่องแดง = เดินไม่ได้, เส้นเหลือง = โพลิกอนของสิ่งกีดขวาง, เส้นฟ้า = ขอบแมพ) |

## 2. Input (ช่องของ MapLabMap)

| ช่อง | ความหมาย |
|---|---|
| Bounds | สี่เหลี่ยมพื้นที่แมพบนระนาบ XY (กรอบสีฟ้าเมื่อเลือก object) กริดเริ่มที่มุมซ้ายล่างของกรอบนี้ |
| Cell Size | ขนาดช่องกริด หน่วยโลก (ฉาก demo ใช้ 0.5) |
| Obstacle Layers | กรอง layer ของ collider ที่นับเป็นสิ่งกีดขวาง ค่าเริ่มต้น Everything |
| Walkable Data / Walkable Mask | ไฟล์ผลลัพธ์ ระบบใส่ให้เองหลัง bake ครั้งแรก ครั้งต่อไปจะเขียนทับไฟล์เดิม |

**อะไรนับเป็นสิ่งกีดขวาง**: Collider2D ที่ (1) อยู่ใต้ GameObject ที่มี MapLabMap (2) เปิดอยู่ (3) ไม่ใช่ trigger (4) อยู่ใน Obstacle Layers (5) ทับกับ Bounds  ของนอก root (เช่น ตัวละคร) ไม่ถูกนับ

| ชนิด collider | กริด | โพลิกอนใน `holes` |
|---|---|---|
| PolygonCollider2D | ช่องที่ทับ = เดินไม่ได้ | 1 รูต่อ 1 path |
| BoxCollider2D | เหมือนกัน | สี่เหลี่ยม 4 จุด |
| CircleCollider2D | เหมือนกัน | 16 เหลี่ยมที่ครอบวงกลม (ไม่เล็กกว่าวงจริง) |
| EdgeCollider2D | ช่องที่เส้นผ่าน = เดินไม่ได้ | ไม่มี (เส้นไม่มีพื้นที่) |
| ชนิดอื่น (Capsule, Composite, Tilemap) | เหมือนกัน | สามเหลี่ยมหลายรูปจาก mesh ของ collider |

## 3. ขั้นตอน (หน้า Walkable area ใน MapLab Window)

ก่อนเริ่ม: ใส่ทุกอย่างของแมพไว้ใต้ GameObject เดียวที่มี **MapLabMap** (GameObject > MapLab > Map สร้างให้) และ **save ฉาก** (ไฟล์ผลลัพธ์ไปอยู่ในโฟลเดอร์ `<ชื่อฉาก>_MapLab/` ข้างไฟล์ฉาก)

หน้านี้มีการ์ด 5 ใบ เลขในวงกลมบอกสถานะ: **เขียว** = เรียบร้อย, **ม่วง** = ต้องทำ, **เทา** = รอขั้นก่อนหน้า ทำจากบนลงล่างจนเขียวทุกใบ

```
[1] Area                       [Fit to Map]
    -25..25 x -17..17 units, cell 0.5 = 100 x 68 cells
    Drag the blue frame in the Scene view to change it.
    Cell size [0.5]   > Exact numbers
[2] What blocks walking        [Select All]
    16 colliders under this map   (Circle 9) (Polygon 7)
    Layers [Everything]
    Triggers never block walking. Not counted: 2 triggers.
[3] Walk on top                [Select All]
    2 surfaces to stand on, 1 ramp   (Plateau 1.5) (Tier 2 2.5)
    Nothing leads up to Tier 2 yet: it rises more than the step height and has no ramp. Select it, press Add Ramp ...
    Step height [0.25]
[4] Bake                       [Bake Again]
    Out of date: Boulder x2 moved or changed (100 x 68 cells, 83.7% walkable)
    [x] Show in the Scene view: the area frame and blocked cells in red
[5] Use it in the game
    Walkable.json            133 KB  (o)
    Walkable_Mask.png          1 KB  (o)
    Walkable_Elevation.json   20 KB  (o)   <- เฉพาะแมพที่มี Walk On Top
    Walkable_Elevation.png     1 KB  (o)
```

1. **Area**: กด **Fit to Map** ระบบหากรอบที่คลุมทุก shape, sprite และ collider ใต้แมพพอดี (ไม่เผื่อขอบ เพราะช่องนอกภาพพื้นจะกลายเป็นช่องเดินได้) แล้วปัดออกให้ลงเส้นกริดของ Cell size (ไม่นับภาพที่ bake แล้วของ Layers เพราะภาพเหล่านั้นเต็มช่อง chunk) อยากปรับเอง: ลาก**กรอบสีฟ้า**ใน Scene view (จุดกลางขอบและมุม ลากแล้วปัดลงเส้นกริด กด Ctrl+Z ย้อนได้) หรือเปิด **Exact numbers** แล้วพิมพ์ ถ้ามี collider โผล่นอกกรอบ เลขจะเป็นม่วงและบอกจำนวน
2. **What blocks walking**: ดูว่ามี collider กี่ชิ้นและชนิดอะไร กด **Select All** เพื่อเลือกทั้งหมดใน Hierarchy (เช็กว่าไม่มีตัวละครหรือของที่ไม่ควรขวางปนมา) บรรทัดล่างบอกของที่ไม่นับ: trigger, อยู่ layer อื่น, ปิดอยู่ ถ้าเป็น 0 ชิ้นการ์ดจะเป็นม่วง: ใส่ Collider2D ที่ไม่ใช่ trigger (Plateau และ Ring ใส่ให้เอง)
3. **Walk on top** (ไม่บังคับ): รายชื่อ shape ที่ติ๊ก **Walk On Top** และความสูงของด้านบน กด **Select All** เพื่อเลือกทั้งหมด ช่อง **Step height** (ค่าเริ่ม 0.25) = ความสูงที่ก้าวขึ้นได้เองโดยไม่ต้องมีทางลาด (ขอบถนน ลานเตี้ย) บรรทัดแรกนับทางลาดด้วย บรรทัด "Nothing leads up to ..." บอกที่ที่สูงกว่า step height และยังไม่มีทางลาดพาขึ้นไป (วิธีใส่ทางลาด: [MapLab_Elevation_Walkthrough_TH.md](./MapLab_Elevation_Walkthrough_TH.md) หัวข้อ 2) การ์ดเป็นม่วงเมื่อมี shape ที่เดินบนได้ทับกันโดยไม่ได้เป็นพ่อลูก (ที่สูงกว่าชนะ) หรือมีทางลาดที่พาไปยังด้านบนที่ไม่ได้ติ๊ก Walk On Top ถ้าไม่มี shape ไหนติ๊ก การ์ดเขียวพร้อมคำว่า Optional และไม่มีไฟล์ elevation
4. **Bake**: กด **Bake** (ครั้งแรก) หรือ **Bake Again** ถ้าล้าสมัย ข้อความจะบอก**ชื่อ**ของที่ขยับ เพิ่ม หรือลบ และค่าไหนเปลี่ยน (area, cell size, layers) ติ๊ก **Show in the Scene view** เพื่อเห็นกรอบและช่องที่เดินไม่ได้เป็นสีแดงใน Scene view (ช่องแดงคือผล bake ล่าสุด ถ้าขยับของแล้วยังไม่ bake ช่องแดงจะยังอยู่ที่เดิม)
5. **Use it in the game**: ไฟล์ที่ได้และขนาด (มี `Walkable_Elevation.json` / `.png` เพิ่มเมื่อมี Walk On Top) ปุ่มเป้า = เลือกไฟล์ใน Project เกมอ่าน `Walkable.json` (TextAsset) โดยไม่ต้องมีโค้ด MapLab ดูหัวข้อ 5

Console จะแสดงเช่น `Baked .../MapLab_Demo_MapLab/Environment/Walkable.json: 100x68 cells of 0.5, 83.7% walkable, 213 obstacle outlines` ไม่มีการ bake เองอัตโนมัติ แก้สิ่งกีดขวางแล้วต้องกด Bake ใหม่

**ระบบรู้ได้อย่างไรว่าอะไรเปลี่ยน**: ตอน bake ระบบจดลายนิ้วมือของ collider แต่ละชิ้นไว้ที่ `<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Data~/WalkableObstacles.json` (Unity ไม่ import โฟลเดอร์ที่ลงท้าย `~`) ชิ้นหนึ่งระบุด้วยเส้นทางใน Hierarchy ใต้แมพ ถ้าเปลี่ยนชื่อหรือย้ายพ่อ ระบบจะเห็นเป็น "ลบ" กับ "เพิ่ม" แทน "ขยับ" ไม่มีผลกับผล bake

## 4. ผลที่ควรได้

ฉาก demo (`Assets/ThirdParty/MapLab/Demo/MapLab_Demo.unity`) ตรวจแล้ว 2026-10-24:

| การ์ด | ที่ควรเห็น |
|---|---|
| 1 Area | `-25..25 x -17..17 units, cell 0.5 = 100 x 68 cells` เลขเขียว ไม่มี collider โผล่นอกกรอบ กด **Fit to Map** แล้วได้ `-28..28 x -20..20` = 112 x 80 ช่อง เพราะภาพพื้น `Ground/Grass Base` กว้างกว่ากรอบที่ตั้งไว้ (ถ้าไม่อยากได้ ให้ Ctrl+Z หรือลากกรอบกลับ) |
| 2 What blocks walking | `16 colliders` ชิป Circle 9 (หิน 7, ลำต้น 2) และ Polygon 7 (Extruded Shape) บรรทัดล่าง `Not counted: 2 triggers.` |
| 3 Walk on top | ฉาก demo ยังไม่มี shape ไหนติ๊ก Walk On Top: เขียว ข้อความ Optional (ตัวตรวจ Elevation Sort Report ลองเปิดให้ Plateau ของ demo ในหน่วยความจำ ดู [MapLab_Elevation_Walkthrough_TH.md](./MapLab_Elevation_Walkthrough_TH.md)) |
| 4 Bake | เขียว `Up to date (100 x 68 cells, 83.7% walkable)` ติ๊ก Show in the Scene view แล้วเห็นกรอบสีฟ้าและช่องแดง 69 สี่เหลี่ยม (ช่องติดกันรวมเป็นสี่เหลี่ยมเดียว) ขยับหินหนึ่งก้อนแล้วรอครึ่งวินาที เลข 4 เป็นม่วงและบอกชื่อหิน (ต้อง Bake Again หนึ่งครั้งก่อนเพื่อสร้าง `Data~/WalkableObstacles.json`) |
| 5 Use it in the game | `Walkable.json` และ `Walkable_Mask.png` พร้อมขนาด |

การนับ collider ใช้ราว 3 ms และการจดลายนิ้วมือราว 10 ms บนแมพ demo


- ไฟล์ `Walkable.json` และ `Walkable_Mask.png` ในโฟลเดอร์ของแมพ `<ชื่อฉาก>_MapLab/<ชื่อแมพ>/` (ชื่อแมพตัดส่วนในวงเล็บเหลี่ยมออก เช่น `[MapLab] Environment` เป็น `Environment` ถ้ามีหลายแมพชื่อซ้ำ ตัวถัดไปต่อท้ายด้วยเลข) ไฟล์ bake อื่นของแมพเดียวกันอยู่ในโฟลเดอร์นี้ด้วย
- เปิดดูสถานะ (อัปเดตแล้ว / ล้าสมัย) และกด Bake ได้จากหน้า **Walkable area** ใน MapLab Window ([MapLab_Window_Walkthrough_TH.md](./MapLab_Window_Walkthrough_TH.md))
- ฉาก demo: `Assets/ThirdParty/MapLab/Demo/MapLab_Demo_MapLab/Environment/Walkable.json` กริด 100 x 68 ช่อง (0.5 หน่วย) เดินได้ราว 84% มี 213 โพลิกอน (Extruded Shape: footprint ของ Plateau, Void Rock, เสา Box 4 ต้น และ Ring Wall ซึ่ง collider มีรูตรงกลางจึงถูกส่งออกเป็นสามเหลี่ยม, หิน 7, ลำต้น 2) ลานกลาง Ring Wall และพื้นด้านหลังที่สูงเดินได้ เพราะ collider ของ Extruded Shape คือ footprint บนพื้น ไม่ใช่ภาพที่ลอยขึ้น

## 5. รูปแบบไฟล์ (version 1)

```json
{
  "version": 1,
  "bounds": { "x": -25.0, "y": -17.0, "width": 50.0, "height": 34.0 },
  "cellSize": 0.5,
  "columns": 100,
  "rows": 68,
  "cells": "<base64>",
  "outline": [ { "x": -25.0, "y": -17.0 }, ... ],
  "holes": [ { "points": [ { "x": ..., "y": ... }, ... ] }, ... ]
}
```

| ฟิลด์ | ความหมาย |
|---|---|
| `version` | รุ่นของรูปแบบ ถ้าเปลี่ยนรูปแบบจะเพิ่มเลขนี้ ผู้อ่านควรเช็กก่อนใช้ |
| `bounds` | กรอบแมพ หน่วยโลก ระนาบ XY |
| `cellSize`, `columns`, `rows` | ขนาดช่องและจำนวนช่อง กริดเริ่มที่ (`bounds.x`, `bounds.y`) |
| `cells` | base64 ของ bitset: ช่อง (column, row) อยู่ที่บิต `i = row * columns + column`, ไบต์ `i / 8`, บิต `i % 8` (บิตต่ำก่อน) **1 = เดินได้**, row 0 คือแถวล่างสุด |
| `outline` | ขอบแมพ ทวนเข็มนาฬิกา (ตอนนี้เท่ากับ `bounds`) |
| `holes` | โพลิกอนของสิ่งกีดขวาง ตามเข็มนาฬิกา ไม่ปิดจุดซ้ำท้าย พิกัดปัดที่ 0.001 |

กติกาที่ผู้อ่านต้องรู้:

- ช่องที่ collider ทับแม้บางส่วนนับเป็นเดินไม่ได้ (ระวังผิวของ Physics2D: ถ้าห่างจากขอบ collider ไม่ถึงราว 0.01 หน่วยก็นับว่าทับ)
- **ไม่มีการเผื่อขนาดตัว agent** ถ้า agent รัศมี 0.3 ให้ผู้อ่านขยายเอง (ขยายช่องที่เดินไม่ได้ออก หรือตั้ง agent radius ตอนสร้าง NavMesh)
- `holes` เป็นรูปร่างของ collider ตรงๆ อาจทับกันเอง หรือเลยขอบ `outline` ได้ ถ้าต้องการโพลิกอนเดียวที่สะอาด ให้ผู้อ่าน union/clip เอง กริดคือข้อมูลที่ "สะอาด" แล้ว
- ภาพ PNG: ขาว = เดินได้ ดำ = ไม่ได้ 1 พิกเซลต่อช่อง ด้านบนของภาพคือทิศเหนือ (y มาก) import แบบ Point, ไม่บีบอัด, Read/Write เปิด, ไม่ปรับขนาดเป็นกำลังสอง

### อ่านไฟล์โดยไม่อ้าง MapLab

เขียน class ของตัวเองที่ชื่อฟิลด์ตรงกับ JSON แล้วใช้ `JsonUtility`:

```csharp
[System.Serializable]
public class MyWalkableArea
{
    public int version;
    public Area bounds;
    public float cellSize;
    public int columns;
    public int rows;
    public string cells;
    public Polygon[] holes;

    [System.Serializable] public struct Area { public float x, y, width, height; }
    [System.Serializable] public struct Polygon { public UnityEngine.Vector2[] points; }
}

var area = UnityEngine.JsonUtility.FromJson<MyWalkableArea>(walkableJson.text);
byte[] bits = System.Convert.FromBase64String(area.cells);
bool walkable = (bits[i >> 3] & (1 << (i & 7))) != 0;
```

ตัวอย่างที่ใช้งานจริง: `Assets/ThirdParty/MapLab/Samples/Navigation/Runtime/WalkableAreaData.cs` (เขียน class ของตัวเองแบบนี้ ไม่อ้าง assembly `MapLab`) และเทสต์ `WalkableAreaBakeTests.SampleReader_ReadsTheBakedJsonWithoutMapLabTypes` ตรวจว่าตัวอ่านแยกนี้อ่านไฟล์ที่ bake ได้ถูกต้อง

## 6. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| "Save the scene first" | ฉากยังไม่เคย save ไฟล์ผลลัพธ์ถูกเขียนข้างไฟล์ฉาก (การ์ด 4 เตือนไว้ด้วย) |
| ติ๊ก Walk On Top แล้วการ์ด Bake บอก "Plateau (walk on top) added" | ตั้งใจ: ไฟล์ elevation ต้อง bake ใหม่ กด Bake Again |
| ติ๊ก Walk On Top แล้ว `Walkable.json` ไม่เปลี่ยน Plateau ยังขวาง | ตั้งใจ: v1 คือโลกแบบปีนไม่ได้เสมอ ข้อมูลความสูงอยู่ใน `Walkable_Elevation.json` |
| เอา Walk On Top ออกหมดแล้วไฟล์ elevation หาย | ตั้งใจ: Bake ลบ `Walkable_Elevation.json` / `.png` ที่ไม่มีอะไรใช้แล้ว |
| ไม่เห็นกรอบสีฟ้าใน Scene view | ต้องเปิดหน้า Walkable area ค้างไว้และติ๊ก Show in the Scene view (กรอบแสดงเฉพาะตอนเปิดหน้านี้) |
| ลากกรอบแล้วกระโดดทีละช่อง | ตั้งใจ: กรอบปัดลงเส้นกริดของ Cell size เสมอ |
| Fit to Map ได้กรอบใหญ่เกิน | มีของอยู่ไกลใต้แมพ (เช่น sprite พื้นหลังขนาดใหญ่) ย้ายออกจาก root ของแมพ หรือลากกรอบเอง |
| ข้อความบอก "obstacles or settings changed" แทนชื่อ | bake จากรุ่นก่อนที่ยังไม่มี `Data~/WalkableObstacles.json` กด Bake Again ครั้งเดียว |
| สิ่งกีดขวางไม่ถูกนับ | เป็น trigger, ปิดอยู่, อยู่นอก root ของแมพ, ไม่อยู่ใน Obstacle Layers หรืออยู่นอก Bounds (การ์ด 2 บอกจำนวนที่ไม่นับแต่ละแบบ การ์ด 1 บอกจำนวนที่โผล่นอกกรอบ) |
| ศัตรูหรือผู้เล่นในฉากกลายเป็นสิ่งกีดขวาง | วางไว้ใต้ root ของแมพ ให้ย้ายออก |
| ภาพเลยเส้นเหลืองใน `MapLab_walkable.png` | ภาพกับ collider ไม่ตรงกัน (Extruded Shape สร้าง collider จากรูปเดียวกับภาพจึงตรงเสมอ ปัญหานี้เกิดกับพร็อพหรือ SpriteShape ที่ทำ collider แยก) แก้ collider หรือภาพ แล้ว bake ใหม่ |
| แก้แมพแล้ว agent ยังเดินทะลุ | ยังไม่ได้ bake ใหม่ หรือผู้ใช้ข้อมูลยังไม่ได้โหลดไฟล์ใหม่ |
| Bake ใน Prefab Mode แล้วกริดโล่งหมด | การหาช่องที่ทับใช้ physics ของฉากหลัก ให้ bake ในฉาก ไม่ใช่ใน Prefab Mode |
| ตัวเลขใน JSON ยาวเช่น `-21.339000701904298` | `JsonUtility` เขียน float เป็นทศนิยมยาว ค่าจริงถูกปัดที่ 0.001 แล้ว อ่านได้ปกติ |
