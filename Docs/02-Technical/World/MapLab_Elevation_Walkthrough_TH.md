# MapLab Elevation Walkthrough (TH): Walk On Top, ทางลาด, API เดินขึ้นลง และตัวตรวจ Sort

ส่วนที่สร้างแล้วของงาน "เดินขึ้นลง Plateau" (ขั้น **P0** ถึง **P3**): Plateau เปิด **Walk On Top** ได้ ภาพของด้านบนตัดเป็นแถบให้ตัวละครยืนบนนั้นได้โดยภาพเรียงถูก, **ทางลาดและบันได** สร้างจากโหมด Edit Points ของ Plateau, **Bake** ของหน้า Walkable area เขียน **`Walkable_Elevation.json`** (ช่องไหนอยู่ชั้นหรือทางลาดไหน ความสูง หน้าผา ช่องเปิดของทางลาด sort bias) ข้าง `Walkable.json` ที่ไม่เปลี่ยน **API** `ElevationMap` / `ElevationBody` / `ElevationPathFormula` ให้ตัวละครเดินขึ้นลงจริง (Grid mode, คลิกเมาส์, หาเส้นทาง), **ฉาก demo** ที่เดินด้วย WASD หรือคลิก และมี **Elevation Sort Report** ไว้ตรวจ ดูแผนใน [MapLab_Elevation_Proposal_TH.md](./MapLab_Elevation_Proposal_TH.md)

- อัปเดต: 2026-10-24
- **เกม Pale Vertica ไม่ใช้ระบบนี้** (ตัดสินใจ 2026-10-25 หลังลองต่อเข้าเกมแล้วฝืนเกินไป): ฟีเจอร์ยังอยู่ใน MapLab ครบ แต่ฉากของเกมไม่ติ๊ก Walk On Top ไม่วางทางลาด และโค้ดเกมไม่อ่าน `Walkable_Elevation.json`
- โค้ด: `Assets/ThirdParty/MapLab/Runtime/Shapes/ExtrudedShape.cs` (`WalkOnTop`, `ActorFloor`, `SurfaceSortBias`, `BaseSortBias`), `RampShape.cs`, `RampFormula.cs`, `ExtrudeTypes.cs` (`RampStyle`, `RampCorners`), `Editor/RampShapeEditor.cs` (inspector + handle), `Editor/ShapePointHandles.cs` (วางทางลาดในโหมด Edit Points), `Runtime/MapLabMap.cs` (`StepHeight`, `ElevationData`, `ElevationMask`), `Editor/Elevation/` (`MapLabElevationBaker`, `WalkableElevationJson`, `WalkableElevationTypes`, `MapLabElevationProbe`, `MapLabElevationSortCheck`, `ElevationSortTypes`), การ์ด Walk on top ใน `Editor/Window/MapLabBakeView.cs`
- API (Runtime, namespace `MapLab.Elevation`): `Runtime/Elevation/` (`ElevationMap`, `ElevationPathFormula`, `ElevationBody`, `ElevationTypes`, และชนิดของไฟล์ `WalkableElevationJson`, `WalkableElevationTypes` ที่ย้ายมาจาก Editor)
- ตัวอย่าง: `Samples/Elevation/` (`Runtime/ElevationDemoController.cs`, `Editor/ElevationDemoBuilder.cs`, ฉาก `Scenes/MapLab_Elevation_Demo.unity`, ภาพ `Art/`)
- เทสต์: `Tests/Editor/ElevationSortTests.cs`, `ElevationBakeTests.cs`, `RampShapeTests.cs`, `ElevationApiTests.cs`
- เกี่ยวข้อง: [Extruded_Shape_Walkthrough_TH.md](./Extruded_Shape_Walkthrough_TH.md)

## 1. Walk On Top

| ทำอะไร | ที่ไหน |
|---|---|
| เปิดให้ยืนบนด้านบนได้ | เลือก Plateau (หรือ Ring, Box) ใน Hierarchy แล้วติ๊ก **Walk On Top** ใน Inspector (Plateau ที่สร้างใหม่จาก GameObject > MapLab > Plateau Shape ติ๊กให้แล้ว, Box และ Ring ไม่ติ๊ก, ของเดิมในฉากไม่เปลี่ยน) |
| ดูทุกชิ้นที่ยืนบนได้ ตั้ง Step height | MapLab Window > Walkable area การ์ด **3 Walk on top** |
| ดูว่าวาดเป็นกี่ชิ้น | บรรทัด "Drawn as ..." ใต้ช่อง Actor Height ใน Inspector |

ผลเมื่อติ๊ก:

1. ด้านบนตัดเป็นแถบตาม **Sort Slice** เสมอ (ปกติถ้าสูงกว่า Actor Height จะรวมเป็นชิ้นเดียว) บรรทัด Drawn as เปลี่ยนจาก `one top piece + wall bands` เป็น `bands`
2. ขั้นบน (Tier) ที่วางบนรูปนี้ก็ตัดแถบ เพราะตัวละครบนด้านบนยืนข้างมัน (`ActorFloor` = ความสูงของด้านบนนี้)
3. **ไม่เปลี่ยน**: collider (footprint ยังขวางทั้งรูป) และ `Walkable.json` เกมที่อ่านแต่ไฟล์นั้นเห็น Plateau เป็นกำแพงเหมือนเดิม ข้อมูลความสูงอยู่ในไฟล์ elevation (หัวข้อ 3) ทางขึ้นคือทางลาด (หัวข้อ 2) ส่วนการเดินจริงในเกมรอ API (P3)
4. Void ติ๊กไม่ได้ผล (ไม่มีด้านบนให้ยืน)

### ตัวละครที่ยืนบนด้านบน: เลื่อนจุดเรียงเท่ากับ SurfaceSortBias

กฎที่ผ่าน P0: **ตัวละครบนด้านบนเรียงเหมือนขั้นบน (Tier) ของรูปนั้น** คือเลื่อนจุดเรียง (SortingGroup) ลง `SurfaceSortBias` แล้วยกภาพขึ้นเท่ากัน

```
Character root        <- ตำแหน่งบนพื้น (x, y)
  Sort Anchor         <- SortingGroup, local y = -SurfaceSortBias
    Visual            <- ภาพ, local y = ความสูงของด้านบน + SurfaceSortBias
```

- `SurfaceSortBias` = Sort Slice + 0.01 ของรูปที่ยืน (ค่าเริ่ม 0.51) บน Tier = ของแม่ + Sort Slice ของ Tier + 0.01 (ค่าเริ่ม 1.02)
- ตัวละคร ขั้นบน และ (ในอนาคต) ของบนด้านบน เลื่อนเท่ากันหมด จึงเรียงกันเองตาม y บนพื้นถูกต้อง
- ข้อจำกัด: ของบน **พื้นล่าง** ที่อยู่หน้าตัวละครไม่เกิน `SurfaceSortBias` และสูงพอจะซ้อนภาพกัน (เช่น ต้นไม้ชิดหน้าผา) จะถูกตัวละครวาดทับ ลด Sort Slice ของรูปที่เดินบนได้เป็น 0.25 แถบนี้เหลือ 0.26 (ชิ้นภาพเพิ่มราวสองเท่า)

## 2. ทางลาดและบันได (P2)

### สร้าง

1. เลือก Plateau (หรือ Tier) ที่ติ๊ก **Walk On Top** แล้ว
2. Inspector หัวข้อ **Ramps** กด **Add Ramp: Click an Edge** (เปิดโหมด Edit Points ให้เอง)
3. ใน Scene view เลื่อนเมาส์: กรอบสีส้มของทางลาดเกาะขอบที่ใกล้เมาส์ที่สุด คลิก = สร้างตรงนั้น, Esc = เลิก
4. ได้ GameObject **Ramp** เป็นลูกของ Plateau ถูกเลือกไว้ให้ ปรับต่อได้ทันที (Ctrl+Z ย้อนได้)

### ปรับ

| ค่า | Inspector | Scene view |
|---|---|---|
| Position | 0-1 ตำแหน่งบนเส้นรอบ footprint | ลากสี่เหลี่ยมกลางขอบบน: เลื่อนไปตามขอบ ข้ามมุมได้ |
| Width | 0.5-12 | ลากจุดที่มุมบนขวา |
| Length | 0.25-20 ระยะแนวราบ (สั้น = ชัน, Inspector บอกองศา) | ลากจุดกลางขอบล่าง |
| Angle | -60 ถึง 60 องศาจากแนวตั้งฉากกับขอบ (ทางลาดเฉียงเลียบหน้าผา) | ลากจุดกลางขอบล่างไปด้านข้าง, กด Shift ค้างเพื่อคงมุมแล้วปรับแค่ความยาว |
| Style | Ramp (ลาดเรียบ) / Stairs | |
| Steps | 2-32 ขั้น (Stairs) | |
| Side Walls | ผนังข้างใต้ทางลาด (เห็นเฉพาะด้านที่หันเข้ากล้อง) | |
| Surface/Wall Color, Texture | สีและลายของพื้นทางลาดกับผนัง | |
| Sort Slice | ความถี่ของแถบเรียงภาพเหมือน shape | |

- ความสูง: ต่ำสุด = ฐานของ Plateau (พื้นหรือด้านบนของแม่ถ้าเป็น Tier), สูงสุด = ด้านบนของ Plateau แก้ความสูงหรือจุดของ Plateau แล้วทางลาดตามเอง (เก็บตำแหน่งเป็นสัดส่วนบนเส้นรอบ)
- ทางลาดบน Tier พาจากด้านบนของแม่ขึ้น Tier ใช้วิธีเดียวกัน
- collider ของทางลาด = footprint ของมัน `Walkable.json` (v1) จึงเห็นทางลาดเป็นสิ่งกีดขวางเหมือน Plateau (โลกแบบปีนไม่ได้)
- ภาพ: ทางลาดวาดหน้าผนังของ Plateau ที่มันพิง ตัด Y-sort เป็นแถบ ตัวละครบนทางลาดเลื่อนจุดเรียงลง `SurfaceSortBias` ของทางลาด (ค่าเริ่ม 0.51 บนพื้น, เท่ากับ Tier ถ้าทางลาดอยู่บน Tier)
- บันได: ความสูงที่ยืนเป็นขั้นตามลูกขั้น (ไม่ไล่เรียบ) ทางลาดแบบ Ramp ไล่เรียบ
- ยังไม่ทำ: bake ภาพทางลาดลง sorted layer (ทางลาดวาด live เสมอ), preset ของทางลาด, ของบนทางลาด (หินที่เป็นลูกของทางลาดยังไม่นับเป็นสิ่งกีดขวางของทางลาด)

## 3. ไฟล์ Walkable_Elevation.json (P1, ทางลาดเพิ่มใน P2)

กด **Bake** หรือ **Bake Again** ในหน้า Walkable area (หรือปุ่ม Bake Walkable Area บน MapLabMap, เมนู Bake Walkable Area (Open Scenes)) แล้วได้ไฟล์เพิ่มสองไฟล์ในโฟลเดอร์ของแมพ เฉพาะเมื่อมี shape ที่ติ๊ก Walk On Top อย่างน้อยหนึ่งชิ้น (เอาออกหมดแล้ว Bake = ลบไฟล์ให้)

```
<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Walkable.json             v1 ไม่เปลี่ยน (Plateau ยังเป็นสิ่งกีดขวาง)
<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Walkable_Elevation.json   กริดเดียวกัน + ชั้น ความสูง หน้าผา sort bias
<ชื่อฉาก>_MapLab/<ชื่อแมพ>/Walkable_Elevation.png    1 พิกเซลต่อช่อง: ดำ = เดินไม่ได้, เทาเข้ม = พื้น, ยิ่งสูงยิ่งสว่าง
```

ไฟล์ใส่ไว้ที่ MapLabMap ช่อง **Elevation Data** / **Elevation Mask** (หัวข้อ Heights) ให้เอง

### รูปแบบ (version 1)

```json
{
  "version": 1,
  "walkable": "Walkable.json",
  "bounds": { "x": -25.0, "y": -17.0, "width": 50.0, "height": 34.0 },
  "cellSize": 0.5, "columns": 100, "rows": 68,
  "stepHeight": 0.25,
  "levels": [
    { "id": 0, "name": "Ground", "height": 0.0, "sortBias": 0.0, "below": 0 },
    { "id": 1, "name": "Cliffs/Plateau", "height": 1.5, "sortBias": 0.51, "below": 0 },
    { "id": 2, "name": "Cliffs/Plateau/Tier 2", "height": 2.5, "sortBias": 1.02, "below": 1 }
  ],
  "cellLevel": "<base64, 1 ไบต์ต่อช่อง>",
  "cliffs": [ { "level": 1, "inner": false, "points": [ ... ], "openings": [ { "from": {..}, "to": {..}, "ramp": 4 } ] } ],
  "ramps": [
    { "id": 4, "name": "Cliffs/Plateau/Ramp", "lower": 0, "upper": 1, "lowHeight": 0.0, "highHeight": 1.5,
      "stairs": false, "steps": 0, "polygon": [ 4 จุด ], "top": [ 2 จุด ], "bottom": [ 2 จุด ] }
  ]
}
```

| ฟิลด์ | ความหมาย |
|---|---|
| `bounds`, `cellSize`, `columns`, `rows` | กริดเดียวกับ `Walkable.json` ทุกช่อง |
| `stepHeight` | ต่างกันไม่เกินนี้ก้าวข้ามได้ |
| `levels` | ชั้นละหนึ่ง shape ที่ติ๊ก Walk On Top (ชั้น 0 = พื้น): `name` = เส้นทางใน Hierarchy ใต้แมพ, `height` = ความสูงของด้านบน, `sortBias` = ระยะที่ตัวละครบนชั้นนี้ต้องเลื่อนจุดเรียงลง (หัวข้อ 1), `below` = ชั้นที่มันตั้งอยู่ |
| `cellLevel` | base64 ของไบต์ละช่อง เรียงแบบเดียวกับ `cells` ของ v1 (`i = row * columns + column`, row 0 ล่างสุด): `id` ของชั้น **หรือของทางลาด** (ทางลาดได้ id ต่อจากชั้นสุดท้าย), **255 = เดินไม่ได้** (254 ไม่ใช้) |
| `cliffs` | เส้นขอบของชั้นที่สูงกว่า step height: นอกทวนเข็ม, `inner: true` = ขอบในของ Ring ตามเข็ม พิกัดปัดที่ 0.001 ใช้สร้าง collider หรือ NavMesh ต่อชั้น, `openings` = ช่วงขอบที่ทางลาดมาแตะ (เว้นไว้ไม่ต้องสร้างกำแพง) |
| `ramps` | ทางลาด: `lower` ชั้นที่ปลายล่างแตะ, `upper` ชั้นที่ปลายบนพาขึ้นไป, `lowHeight`/`highHeight`, `polygon` footprint, `top`/`bottom` เส้นปลายบนและล่าง (จุดซ้ายไปขวาตามลำดับเดียวกัน), `stairs`/`steps` |

### ช่องไหนอยู่ชั้นไหน

1. ดู **จุดกลางช่อง**: อยู่ใน footprint ของ shape ที่ Walk On Top ไหน ใช้ชั้นที่ด้านบนสูงที่สุด (Tier ชนะแม่, Ring ไม่นับช่องในลาน) ไม่อยู่ในไหนเลย = พื้น (0)
2. Tier ที่ **ไม่** ติ๊ก Walk On Top บน shape ที่ติ๊ก = ช่องใต้มันเดินไม่ได้ (255)
3. collider ขวางเฉพาะชั้นที่มันยืน: ของที่เป็น **ลูกของ shape** (หิน ต้นไม้บนหลังคา) ขวางชั้นของ shape นั้น, ของอื่นขวางพื้น, collider footprint ของ Plateau ขวางชั้นที่ Plateau ตั้งอยู่ (ช่องพื้นที่แตะหน้าผาจึงเดินไม่ได้เหมือน v1) ยกเว้น shape ที่สูงไม่เกิน step height ซึ่งก้าวขึ้นได้ (ช่องรอบๆ เดินได้ และไม่มี cliff)
4. ช่องที่ collider ทับแม้บางส่วน = เดินไม่ได้ (กติกาเดียวกับ v1, ใช้ physics query ตัวเดียวกัน)

กติกาการเดินระหว่างช่องติดกัน (ฝั่งผู้อ่าน):

1. `id` เดียวกัน (ชั้นเดียวกัน หรือทางลาดเดียวกัน) เดินได้
2. ชั้นกับชั้น: ความสูงต่างไม่เกิน `stepHeight` เดินได้ นอกนั้นคือหน้าผา
3. ทางลาดกับชั้น: เดินได้เมื่อ **ข้ามเส้น `top` ไปยังชั้น `upper`** หรือ **ข้ามเส้น `bottom` ไปยังชั้น `lower`** เท่านั้น ด้านข้างของทางลาดเดินออกไม่ได้ (เหมือนมีราวกัน) วิธีเช็ก: เอาจุดกลางของช่องฝั่งชั้นไปหาค่า v ตามแนวลาด (0 ที่ `top`, 1 ที่ `bottom`) เผื่อหนึ่งช่อง e = min(0.5, cellSize / ความยาวทางลาด): ไปชั้น `upper` ได้เมื่อ v <= e, ไปชั้น `lower` ได้เมื่อ v >= 1 - e (ช่องที่ติดปลายทางลาดอาจทับขอบทางลาดเล็กน้อยเพราะช่องเป็นของทางลาดก็ต่อเมื่อจุดกลางอยู่ในทางลาด) และอีกสองข้อ (เพิ่ม 2026-10-24 หลังเทสต์ฝั่งเกมเจอการก้าวจากด้านข้างใกล้ปลายทางลาดเข้ากลางทางลาด กระโดด 0.3 ถึง 0.5): **ช่องฝั่งชั้นต้องอยู่หน้าช่องเปิด** คือค่า u ตามแนวกว้าง (0 ที่ซ้าย 1 ที่ขวา) อยู่ใน 0 ถึง 1 เว้นแต่จุดกลางเลยเส้นปลายออกไปแล้ว (v <= 0 ที่ `top`, v >= 1 ที่ `bottom`), และ **ช่องฝั่งทางลาดต้องห่างเส้นปลายนั้นไม่เกิน 1.5 ช่อง** (v <= 1.5e ที่ `top`, v >= 1 - 1.5e ที่ `bottom`) `ElevationMap.CanStep` ทำตามนี้
4. ความสูงบนทางลาด: ไล่จาก `highHeight` ที่ `top` ถึง `lowHeight` ที่ `bottom`, บันได (`stairs`) เป็นขั้น: ขั้นที่ `floor(v * steps)` สูง `lerp(highHeight, lowHeight, ขั้น / steps)`

ช่องกลางทางลาดอยู่บนพื้นที่ของชั้นล่างก็จริง แต่ collider ของ Plateau ที่ทางลาดพิงและ collider ของทางลาดเองไม่นับขวางทางลาด ส่วนของอื่นบนชั้นล่าง (หิน) ยังขวาง

### อ่านไฟล์โดยไม่อ้าง MapLab

```csharp
[System.Serializable]
public class MyElevation
{
    public int version;
    public float cellSize;
    public int columns;
    public int rows;
    public float stepHeight;
    public Level[] levels;
    public string cellLevel;

    [System.Serializable] public struct Level { public int id; public string name; public float height; public float sortBias; public int below; }
}

var elevation = UnityEngine.JsonUtility.FromJson<MyElevation>(elevationJson.text);
byte[] levelOfCell = System.Convert.FromBase64String(elevation.cellLevel);
int level = levelOfCell[row * elevation.columns + column];
bool walkable = level != 255;
```

เทสต์ `ElevationBakeTests.Files_AreReadableWithoutMapLabTypes` อ่านแบบนี้

## 4. ใช้ในเกม: ElevationMap และ ElevationBody (P3)

ทั้งหมดอยู่ใน assembly `MapLab` (Runtime) namespace `MapLab.Elevation` ไม่บังคับใช้: เกมที่อยากเขียนเองอ่านไฟล์ตามหัวข้อ 3 ได้เหมือนเดิม

### ตั้งตัวละคร

```
Hero                 <- ElevationBody (+ สคริปต์ควบคุมของเกม) ตำแหน่ง = จุดบนพื้น
  Sort Anchor        <- SortingGroup (order เท่ากับตัวละครอื่น)
    Visual           <- sprite / animator ทั้งหมด
```

1. ใส่ **ElevationBody** ที่ root ของตัวละคร (เพิ่ม component แล้ว Reset จะหา `Sort Anchor` และ `Visual` ให้ถ้าตั้งชื่อตามนี้)
2. **Elevation Data** = `Walkable_Elevation.json` ของแมพ (ช่อง Elevation Data บน MapLabMap หลัง Bake)
3. **Collision**: `Grid` (ค่าเริ่ม) = ห้ามเดินตกหน้าผาและห้ามเดินทะลุเข้า Plateau ตามแผนที่, `None` = ไม่ห้าม (เกมจัดการเอง)
4. ขยับด้วย `body.Move(delta)` แทนการตั้ง `transform.position` เอง (ถ้าโค้ดอื่นขยับ transform ให้ติ๊ก **Sync When Moved** หรือเรียก `body.Sync()` หลังขยับ)
5. ElevationBody ยก `Visual` ขึ้นตามความสูงและเลื่อน `Sort Anchor` ลงตาม sort bias ให้เอง ไม่ต้องคำนวณ

| ElevationBody | ทำอะไร |
|---|---|
| `Move(Vector2 delta)` | ขยับผ่าน `Constrain` (Grid) คืนระยะที่ขยับได้จริง |
| `Teleport(Vector2 ground)`, `Sync()`, `SetMap(...)`, `SetParts(anchor, visual)` | วาง / อ่านสถานะใหม่ / เปลี่ยนแผนที่ / เปลี่ยนชิ้นภาพ |
| `Region`, `Level`, `Height`, `SortBias`, `IsOnRamp`, `Ground`, `Map` | สถานะตอนนี้ (`Level` = ชั้นล่าสุดที่ยืน ไม่ใช่ทางลาด) |
| `OnLevelChanged(from, to)`, `OnRampEntered(id)`, `OnRampExited(id)`, `OnHeightChanged(h)` | event (ไม่ poll ใน Update ยิงตอน Move / Teleport / Sync) |

| ElevationMap | ทำอะไร |
|---|---|
| `ElevationMap.Load(TextAsset)` / `Parse(json)` | อ่านไฟล์ (Load เก็บ cache ต่อไฟล์) ปฏิเสธ version อื่น |
| `RegionAt(point)`, `IsWalkable`, `IsRamp`, `TryGetLevel`, `TryGetRamp` | ช่องนี้เป็นชั้นหรือทางลาดไหน (`Outside` = -1, `Blocked` = 255) |
| `HeightAt(point)`, `SortBias(region)` | ความสูงที่ยืน (บันไดเป็นขั้น) และระยะเลื่อนจุดเรียง |
| `CanStep(from, to)`, `CanWalkStraight(a, b)`, `Constrain(from, to)` | กติกาการเดิน, เดินตรงได้ไหม, ขยับให้ได้มากที่สุดแล้วไถลตามขอบ |
| `ScreenToGround(visualPoint, ...)` / `ScreenToGround(camera, screenPoint, ...)` | คลิกเมาส์ -> จุดบนพื้นและชั้น เลือกพื้นที่อยู่หน้าสุด (y บนพื้นน้อยสุด) คลิกโดนผนังหน้าผาได้ false |
| `ElevationPathFormula.FindPath(map, from, to, path)` | A* บนช่องตามกติกาเดียวกัน แล้วตัดมุมด้วย `CanWalkStraight` |

ตัวอย่างโค้ดคลิกเดิน:

```csharp
if (body.Map.ScreenToGround(camera, mouseScreen, out Vector2 ground, out int region)
    && ElevationPathFormula.FindPath(body.Map, body.Ground, ground, path))
{
    // ทุกเฟรม: body.Move(ทิศไปหาจุดถัดไปใน path * speed * dt)
}
```

กติกาที่ API ใช้ (ตรงกับหัวข้อ 3): ตัดสินที่ **ระดับช่อง** (ใช้จุดกลางช่อง) การเดินกับ A* จึงตรงกันเสมอ, ปลายทางลาดเผื่อหนึ่งช่องที่เส้น `top` และ `bottom` (ช่องชั้นต้องอยู่หน้าช่องเปิด และช่องทางลาดต้องห่างเส้นไม่เกิน 1.5 ช่อง) ด้านข้างทางลาดออกไม่ได้

กติกาเกม เช่น "สกิลใช้ข้ามชั้นไม่ได้" ให้เกมเช็กเอง: `attacker.Level == target.Level` (หรือเทียบ `Region`)

## 5. ฉาก demo (ตัวละครเดินขึ้นลงทางลาด)

| ทำอะไร | ที่ไหน |
|---|---|
| สร้าง / สร้างใหม่ | **Tools > MapLab > Samples > Build Elevation Demo** (Unity ปิด: `Unity.exe -batchmode -executeMethod MapLab.Samples.Elevation.Editor.ElevationDemoBuilder.BuildBatch`) |
| เล่น | เปิด `Samples/Elevation/Scenes/MapLab_Elevation_Demo.unity` กด Play |
| ถ่ายภาพลำดับการเดิน (dev) | **Tools > MapLab > Samples > Render Elevation Walk** หรือ `BuildAndRenderBatch` |

ในฉาก: Plateau สูง 1.5 + Tier สูง 1 (เปิด Walk On Top ทั้งคู่), South Ramp, West Stairs, Tier Ramp, ตัวละคร **Hero** (ElevationBody + `ElevationDemoController`) และวงกลม Click Marker

1. Play แล้วกด **WASD / ลูกศร** เดิน: เดินชนหน้าผาแล้วไถล เดินเข้าทางลาดจากปลายล่างได้ ภาพลอยขึ้นตามความสูง
2. **คลิกซ้าย** ที่ไหนก็ได้ (บนพื้น หลังคา Tier หรือทางลาด) ตัวละครหาเส้นทางผ่านทางลาดแล้วเดินไปเอง วงกลมบอกจุดที่คลิก
3. ใช้ Input System (โปรเจกต์นี้) หรือ Input แบบเก่าถ้าไม่มี package (define `MAPLAB_INPUT_SYSTEM` ตั้งให้เองจาก asmdef)

ไฟล์ที่ builder สร้าง: ฉาก, `Samples/Elevation/Art/Elevation_*.png`, โฟลเดอร์ bake `MapLab_Elevation_Demo_MapLab/Elevation Demo/` (Walkable.json, Walkable_Elevation.json และ mask)

## 6. Elevation Sort Report (เครื่องมือ dev)

| ทำอะไร | ที่ไหน |
|---|---|
| รันใน Unity | **Tools > MapLab > Developer > Elevation Sort Report** (ถาม save ฉากก่อน แล้วเปิดฉากเดิมกลับให้) |
| รันตอน Unity ปิด | `"D:/Unity/6000.4.8f1/Editor/Unity.exe" -batchmode -projectPath "F:/GameProject/Pale Vertica" -executeMethod MapLab.Editor.MapLabElevationProbe.ReportBatch -logFile Logs/Batch/elevation.log` |

Input: ไม่มี ตัวตรวจสร้างฉากชั่วคราวในหน่วยความจำเอง (ไม่ save ไม่แตะฉากของคุณ): Plateau สูง 1.5 เปิด Walk On Top, Tier สูง 1, ทางลาดด้านใต้ (South Ramp) และบันไดด้านตะวันตก (West Stairs), ตัวละครแทน 14 ตัว (บนด้านบน หน้า/หลัง Tier, บน Tier, ขอบตะวันออก, บนทางลาดล่าง/กลาง/บน, กลางบันได, พื้นหลัง/หน้า/ข้าง/ข้างทางลาด) และต้นไม้ 2 ต้นชิดหน้าผา

ขั้นตอนที่มันทำ (ส่วน Sort):

1. ลองแต่ละแบบของการเลื่อนจุดเรียง: `None` (ไม่เลื่อน), `SnapToSlice` (เลื่อนแค่พ้นแถบที่ยืน), `LikeATier` (เลื่อน SurfaceSortBias ซึ่งคือกฎที่เลือก) แล้ว `LikeATier_FineSlice` (Sort Slice 0.25)
2. ทุก 0.04 หน่วยบนภาพของตัวละครและต้นไม้แต่ละตัว เทียบลำดับที่ Unity วาดจริง (จุดเรียง y ต่ำกว่าวาดทีหลัง) กับความลึกจริง (y บนพื้นของตัวละคร, จุดบนด้านบนลบความสูง, กำแพงที่ขอบ footprint) ต่างกันไม่ถึง 0.02 ไม่นับ
3. ถ่ายภาพแต่ละแบบ และภาพที่มีจุดบอกตำแหน่งที่ผิด

ผลอยู่ที่ `Logs/MapLab/MapLab_elevation.txt` และ `Logs/MapLab/MapLab_elevation_<แบบ>.png` / `_misses.png` (จุดม่วงแดง = ถูกวาดไว้หลังของที่ควรอยู่หลังมัน, จุดแดง = วาดทับของที่ควรบังมัน)

### ผลที่ควรได้ (รัน 2026-10-24, ราว 4 วินาที)

| แบบ | miss | ที่ผิด |
|---|---|---|
| None | 967 | ตัวละครบนที่สูงทุกตัวถูกด้านบนหรือ Tier ทับ ตัวสีส้มหน้า Tier หายเกือบหมด |
| SnapToSlice | 352 | ตัวสีส้มที่ยืนหน้า Tier ถูกกำแพง Tier ทับ |
| LikeATier | 185 | ไม่มีตัวละครถูกรูปทับเลย เหลือตัวละครบนที่สูงวาดทับต้นไม้ที่อยู่หน้าไม่เกิน 0.51 (ต้นหน้าผา 0.5, ต้นข้างขอบ 0.2) |
| LikeATier_FineSlice | 57 | เหลือแค่ต้นข้างขอบตะวันออกที่ห่าง 0.2 |

ทุกแบบมี miss เล็กๆ 2 จุดของต้นไม้ข้างขอบตะวันออกกับแถบด้านบน (ต่าง 0.04) เป็นพฤติกรรมเดิมของการตัดแถบ ไม่เกี่ยวกับ Walk On Top

ส่วน Demo (ถ้ามีฉาก demo ที่ bake แล้ว, ผล 2026-10-24: `Walkable.json` เหมือนเดิมทุกไบต์, ใส่ทางลาดด้านใต้ให้ Plateau, Tier 2, Tier 3 แล้วไม่มีชั้นไหนถูกตัดขาด, bake ราว 30 ms): เปิด `Demo/MapLab_Demo.unity` ในหน่วยความจำ (ไม่ save) คำนวณ `Walkable.json` ใหม่เทียบกับไฟล์บนดิสก์ ติ๊ก Walk On Top ให้ Plateau และ Tier ของ demo (ไม่นับเสา Box) แล้วเทียบ `Walkable.json` อีกครั้ง ใส่ทางลาดด้านใต้ให้ทุกชั้น คำนวณไฟล์ elevation และถ่าย `Logs/MapLab/MapLab_elevation_demo.png` (ช่องอยู่ที่ตำแหน่งบนพื้น) กับ `MapLab_elevation_demo_raised.png` (ยกแต่ละช่องตามความสูงของชั้น ควรตรงกับด้านบนที่วาดอยู่พอดี) ฟ้า/เขียว/ส้ม = ชั้น 1/2/3, ม่วงชมพู = ทางลาด, แดง = เดินไม่ได้, เส้นเหลือง = หน้าผา

## 7. ผิดบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| ติ๊ก Walk On Top แล้วตัวละครยังขึ้นไปไม่ได้ | ตั้งใจ: P0 แก้แค่ภาพ การเดินมาใน P1 ถึง P3 |
| ตัวละครบนด้านบนถูกด้านบนทับเท้า | ยังไม่ได้เลื่อนจุดเรียง (Sort Anchor) ลง SurfaceSortBias หรือ SortingGroup อยู่ที่ root แทน Sort Anchor |
| ตัวละครบนด้านบนวาดทับต้นไม้ที่อยู่หน้าหน้าผา | อยู่ในแถบ SurfaceSortBias ลด Sort Slice ของรูปนั้น หรือวางของสูงให้ห่างหน้าผามากกว่าแถบ |
| Tier ที่อยู่บนรูปที่ติ๊ก Walk On Top ไม่มีเงา | ตั้งใจตอนนี้: เงาของ Tier มีเฉพาะตอนรวมเป็นชิ้นเดียว แต่ Tier ต้องตัดแถบเพราะมีคนยืนข้างมัน |
| `unity_batch.sh` ค้างรอ "Unity is open" ทั้งที่ปิดโปรเจกต์นี้แล้ว | สคริปต์รอทุก Unity.exe รวมโปรเจกต์อื่น ปิดทุกตัว หรือรัน Unity.exe ตรงตามตารางหัวข้อ 2 (เช็กว่าไม่มี `Temp/UnityLockfile` ของโปรเจกต์นี้) |
| ไม่มีไฟล์ `Walkable_Elevation.json` หลัง Bake | ไม่มี shape ไหนติ๊ก Walk On Top (ดูการ์ด 3) |
| ช่องรอบ Plateau เตี้ยๆ เดินไม่ได้ | Plateau สูงกว่า Step height เพิ่ม Step height ในการ์ด 3 หรือที่ MapLabMap |
| หินบนหลังคาขวางพื้นข้างล่างแทน | หินไม่ได้เป็นลูกของ Plateau ลากเข้าไปใต้ Plateau ใน Hierarchy |
| การ์ด 3 ม่วง "Overlapping" | shape สองชิ้นที่เดินบนได้ทับกันโดยไม่ได้เป็นพ่อลูก ที่สูงกว่าชนะ แยกออกจากกัน หรือทำเป็น Tier |
| ปุ่ม Add Ramp ไม่มี | เลือก ExtrudedShape (Plateau, Tier) ไม่ใช่ Ramp หรือ GameObject ลูกอื่น |
| การ์ด 3 ม่วง "... leads to a top that is not Walk On Top" | ทางลาดอยู่บน shape ที่ไม่ติ๊ก Walk On Top กดปุ่ม Turn On ใน Inspector ของทางลาด |
| ทางลาดของ Tier ยื่นเลยขอบ Plateau ข้างล่าง | ความยาวเกินพื้นที่บนแม่ ลดความยาวหรือเลื่อนตำแหน่ง (ปลายล่างที่ตกขอบจะไม่เชื่อมกับชั้นไหน) |
| ตัวละครบนบันไดที่ลาดไปทางซ้าย/ขวา ถูกขั้นข้างๆ บังเท้าไม่ครบ | ข้อจำกัด: ภาพตัวละครกว้างกว่าขั้น ขั้นที่สูงกว่าข้างตัวควรบังขาบางส่วน แต่ตัวละครเรียงเป็นจุดเดียว ผิดได้ไม่เกินหนึ่งขั้น บันไดที่ลาดเข้าหากล้องไม่มีปัญหานี้ |
| ตัวละครไม่ลอยขึ้นบนหลังคา | ไม่ได้ใส่ Elevation Data หรือ Visual ไม่ได้ผูกกับ ElevationBody, หรือขยับ transform เองโดยไม่ Sync |
| ตัวละครเดินผ่านหน้าผาได้ | Collision เป็น None หรือขยับ transform เองแทน `Move` |
| คลิกบนหลังคาแล้วตัวละครวิ่งไปพื้นข้างหลัง | ใช้ `Camera.ScreenToWorldPoint` ตรงๆ ต้องผ่าน `ScreenToGround` |
| หาเส้นทางไม่เจอ | ปลายทางเป็นช่องที่เดินไม่ได้ หรือชั้นนั้นไม่มีทางลาดเชื่อม (การ์ด 3 บอก) |
| `InvalidOperationException ... version` ตอนโหลด | ไฟล์ elevation รุ่นใหม่กว่าโค้ด อัปเดต MapLab หรือ Bake ใหม่ |
| รันจากเมนูแล้วฉากหาย | ตัวตรวจเปิดฉากใหม่ชั่วคราวแล้วเปิดฉากที่ save ไว้กลับ ฉากที่ยังไม่เคย save จะได้ฉากเปล่าแทน save ก่อนรัน |
