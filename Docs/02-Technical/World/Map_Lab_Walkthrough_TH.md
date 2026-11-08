# Map Lab Walkthrough (TH)

MapLab คือชุดเครื่องมือทำแมพ 2D ที่ไม่แบน (เฟส 1 ของ [Map_Depth_Analysis_TH.md](../../01-Design/Map_Depth_Analysis_TH.md)) พื้นใช้ **เส้นโค้งจริง** ไม่ใช่ tile สี่เหลี่ยม และทุกอย่างสร้างด้วยเครื่องมือของ MapLab เอง (**ไม่ใช้ SpriteShape แล้ว**): ทางเดินเป็น **Path Shape** ([Path_Shape_Walkthrough_TH.md](./Path_Shape_Walkthrough_TH.md)) ส่วนที่ราบสูง หินถ้ำ กำแพงล้อม เสา และพื้นแบน (ลาน ทางแยก) เป็น **Extruded Shape** ([Extruded_Shape_Walkthrough_TH.md](./Extruded_Shape_Walkthrough_TH.md)) ภาพ demo ทั้งหมดเป็นสี flat สว่าง ไอคอนของแพ็กเกจดู [MapLab_Icons_Walkthrough_TH.md](./MapLab_Icons_Walkthrough_TH.md)

ตั้งแต่ 2026-10-23 MapLab เป็น **โมดูลแยก (standalone)** ที่ตั้งใจเอาไปต่อยอดและขายแยกได้ ไม่ใช่ส่วนหนึ่งของโค้ดเกม Pale Vertica การเชื่อมเข้ากับเกมจะตัดสินใจทีหลัง (ดูหัวข้อ 9)

**หน้าที่ของ MapLab จบที่ข้อมูล**: สร้างแมพ แล้ว bake เป็นไฟล์พื้นที่เดินได้ (JSON + ภาพ mask) ใครจะเอาไปทำ NavMesh, A* หรือให้ agent เดินอย่างไร เป็นเรื่องข้างนอก และอ่านไฟล์ได้เลยโดยไม่ต้องอ้างโค้ดของ MapLab

- อัปเดต: 2026-10-23
- โมดูล: `Assets/ThirdParty/MapLab/`
- การ bake และรูปแบบไฟล์: [Walkable_Area_Bake_Walkthrough_TH.md](./Walkable_Area_Bake_Walkthrough_TH.md)
- ตัวอย่างผู้ใช้ข้อมูล (NavMesh): [Navigation_Switch_Walkthrough_TH.md](./Navigation_Switch_Walkthrough_TH.md)

## 1. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| **จัดการทุกอย่างของแมพในที่เดียว** (Set, Bake, สถานะ) | **Tools > MapLab > MapLab Window** หรือปุ่ม Open MapLab Window บน MapLabMap ([MapLab_Window_Walkthrough_TH.md](./MapLab_Window_Walkthrough_TH.md)) |
| สร้างแมพใหม่ / Set ใหม่ | **GameObject > MapLab > Map** / **Set** |
| สร้างหรือสร้างฉาก demo ใหม่ | **Tools > MapLab > Developer > Build Demo Scene** |
| สร้างพื้นที่ยกสูง / กล่อง / เส้นทาง | **GameObject > MapLab > Plateau / Void / Ring / Box Shape, Path** |
| แก้จุด | ปุ่ม **Edit Points** ใน Inspector หรือเครื่องมือไอคอน MapLab ใน Tools overlay ของ Scene view |
| ตั้งค่าไอคอน (หลัง export จาก Figma) | **Tools > MapLab > Developer > Apply Icons** |
| วัด performance | **Tools > MapLab > Developer > Performance Report** ([MapLab_Performance_TH.md](./MapLab_Performance_TH.md)) |
| Bake พื้นนิ่งเป็นภาพ (prototype) | ปุ่ม **Bake Static Ground** / **Unbake** บน MapLabMap ([Static_Bake_Walkthrough_TH.md](./Static_Bake_Walkthrough_TH.md)) |
| Bake พื้นที่เดินได้ | ปุ่ม **Bake Walkable Area** บน MapLabMap หรือ **Tools > MapLab > Bake Walkable Area (Open Scenes)** |
| เรนเดอร์ภาพตรวจ | **Tools > MapLab > Developer > Render Demo Preview** (ภาพไปที่ `Logs/MapLab/`) |
| ตอน Unity ปิด | `Tools/FigmaToUnity/unity_batch.sh MapLab.Editor.MapLabSceneBuilder.BuildAndRenderBatch` |
| ฉาก demo | `Assets/ThirdParty/MapLab/Demo/MapLab_Demo.unity` |
| ฉากลองเล่นของเกม | `Assets/Scenes/MapLab/MapLab.unity` (ของฝั่งเกม ไม่ใช่ส่วนของ MapLab ดูหัวข้อ 9) |

## 2. โครงสร้างโมดูล

```
Assets/ThirdParty/MapLab/
  Runtime/    MapLab.asmdef              แกน: ของที่อยู่ในฉาก
    MapLabMap                            ตั้งค่าแมพ + ไฟล์ที่ bake (1 ตัวต่อแมพ)
    Sets/         MapLabSet               กลุ่มของวัตถุในแมพ (Set = GameObject ที่ลูกตรงๆ คือสมาชิก)
    Baking/       MapLabBakedLayer, MapLabChunkStreamer   (พื้นนิ่งที่ bake เป็นภาพ chunk)
    Shapes/       PointShape (ฐาน), ExtrudedShape, PathShape, ExtrudeFormula, ShapeMeshData, ExtrudeTypes
    Shaders/      MapLabShapeLit.shader (MapLab/Shape Lit)
    Materials/    MapLab_ShapeLit.mat (สร้างโดย Editor ครั้งแรกที่ใช้)
    Visuals/      OccluderFadeController
  Editor/     MapLab.Editor.asmdef       แกน: เฉพาะ Editor
    MapLabWalkableBaker, WalkableAreaJson, WalkableAreaTypes, MapLabMapEditor   (bake)
    PointShapeEditor, PointShapeTool, ExtrudedShapeTool, PathShapeTool, ShapePointHandles, ExtrudedShapeMenu   (เครื่องมือ shape)
    MapLabIcons, MapLabIconImporter, Icons/   (ไอคอนจาก Figma)
    MapLabSceneBuilder, MapLabGroundBuilder, MapLabArtPainter, MapLabPreviewRenderer, MapLabPerformanceProbe   (ฉาก demo และการวัด)
    MapLabStaticBaker, MapLabBakeProbe   (bake พื้นนิ่งเป็นภาพ chunk และตัววัด live vs baked)
    MapLabSets, MapLabBakeStatus, MapLabOutputPaths   (จัดการ Set, ตรวจว่า bake ล้าสมัยไหม, โฟลเดอร์ไฟล์ต่อแมพ)
    Window/   MapLabWindow, MapLabSetView, MapLabBakeView, MapLabWindowStyles   (หน้าต่าง MapLab)
  Samples/Navigation/                    ตัวอย่างผู้ใช้ข้อมูล ลบทิ้งได้
    Runtime/  MapLab.Samples.asmdef      NavigationManager, NavigationMode, NavMesh2DFactory, WalkableAreaData
    Editor/   MapLab.Samples.Editor.asmdef   DemoNavigationInstaller
  Tests/Editor/ MapLab.Tests.Editor.asmdef
    WalkableAreaBakeTests, ExtrudeFormulaTests, ExtrudedShapeTests, StaticBakeTests, NavMesh2DFactoryTests, NavigationManagerTests
  Demo/       MapLab_Demo.unity, MapLab_Volume.asset, Generated/
              MapLab_Demo_MapLab/Environment/   ไฟล์ bake ทั้งหมดของแมพนี้: Walkable.json, Walkable_Mask.png, Ground/Chunk_*.png
              MapLab_Demo_Walkable.json/.png    ไฟล์ bake รุ่นเก่า เก็บไว้เพราะ Assets/Scenes/SandBox.unity ยังอ้างอยู่
```

| Assembly | namespace | อ้างอิงได้ |
|---|---|---|
| `MapLab` | `MapLab`, `MapLab.Shapes`, `MapLab.Visuals` | Unity เท่านั้น |
| `MapLab.Editor` | `MapLab.Editor` | `MapLab`, URP (Core, Universal, Universal 2D) |
| `MapLab.Samples` | `MapLab.Samples.Navigation` | Unity เท่านั้น (**ไม่อ้าง `MapLab`** อ่านแค่ไฟล์ JSON) |
| `MapLab.Samples.Editor` | `MapLab.Samples.Navigation.Editor` | `MapLab.Samples`, `MapLab.Editor` (ฟัง event ของตัวสร้างฉาก demo) |
| `MapLab.Tests.Editor` | `MapLab.Tests` | `MapLab`, `MapLab.Editor`, `MapLab.Samples`, Test Framework |

โค้ดเกม (`Assembly-CSharp`) มองเห็น `MapLab` ได้ แต่ MapLab มองไม่เห็นโค้ดเกม ถ้าใน MapLab มีบรรทัด `using CoreSystem...` จะ compile ไม่ผ่าน นี่คือสิ่งที่กันไม่ให้มันผูกกับเกมโดยไม่ตั้งใจ

**สิ่งที่โมดูลต้องมีในโปรเจกต์ปลายทาง**: URP ที่ใช้ 2D Renderer (Transparency Sort Mode = Custom Axis 0, 1, 0), โมดูล AI ในตัว (`com.unity.modules.ai`, ใช้เฉพาะตัวอย่าง Navigation) และ Test Framework ถ้าจะรันเทสต์ ไม่ต้องใช้ SpriteShape

## 3. Input

| Input | ใช้อย่างไร |
|---|---|
| ภาพ placeholder | `MapLabArtPainter` วาดภาพสี flat ลง `Demo/Generated/*.png` **เฉพาะไฟล์ที่ยังไม่มี** ถ้าเอาภาพจริงมาแทนด้วยชื่อเดิม การ build ครั้งต่อไปจะไม่ทับ ถ้าอยากให้วาดใหม่ ลบ `.png` แต่เก็บ `.meta` ไว้ (GUID เดิม ของที่อ้างอยู่ไม่หลุด) |
| Volume profile | `Demo/MapLab_Volume.asset` ถูกตั้งค่าใหม่ทุกครั้งที่ build (bloom เบา, ไม่มี vignette, contrast 0) เพื่อให้ demo สว่างแบบ flat ถ้าจะปรับเอง ให้แก้ค่าใน `MapLabSceneBuilder.LoadOrCreateVolumeProfile` |
| Layer ชื่อ `Player` | ถ้ามี ใบไม้จะจางเฉพาะเมื่อ collider ใน layer นี้เดินเข้า ถ้าไม่มี จะจางกับทุก layer |

ย้ายโฟลเดอร์ `MapLab` ไปที่อื่นใน `Assets/` ได้ เครื่องมือหาตำแหน่งเองจากไฟล์ `MapLab.Editor.asmdef`

## 4. ขั้นตอน

1. ปิดฉากที่แก้ค้างไว้ (เมนูจะถามให้ save ก่อน)
2. กด **Tools > MapLab > Developer > Build Demo Scene** ระบบจะ
   1. สร้าง `MapLab_Demo.unity` ใหม่พร้อม Main Camera (orthographic ขนาด 6) ถ้ายังไม่มีไฟล์
   2. ลบ root `[MapLab] Environment` เดิมแล้วสร้างใหม่ทั้งก้อน (กล้องและของอื่นที่วางเองในฉากไม่ถูกแตะ)
   3. สร้างพื้น (Path Shape 4 เส้น, ทางแยกและลานเป็น Extruded Shape แบน), Extruded Shape (Plateau, Void Rock, Ring Wall, เสา Box 4 ต้น), พร็อพ, แสง (Global Light ขาว 1.0 + ไฟจุดเบาๆ), หมอกพื้นจางๆ และ Volume
   4. ใส่ MapLabMap ที่ root แล้ว **bake** พื้นที่เดินได้เป็น `Demo/MapLab_Demo_MapLab/Environment/Walkable.json` และ `Walkable_Mask.png`
   5. ส่ง event `OnEnvironmentBuilt` (ถ้ามีโฟลเดอร์ Samples ตัวอย่าง Navigation จะใส่ NavigationManager ที่อ่านไฟล์นั้น)
   6. เปิด Post Processing บนกล้อง (ตัวที่ tag `MainCamera` หรือกล้องตัวแรก)
   7. save ฉาก
3. กด **Tools > MapLab > Developer > Render Demo Preview** เพื่อได้ภาพ 3 ภาพ:
   - `MapLab_overview.png` (ทั้งแมพ ortho 17)
   - `MapLab_gameplay.png` (ortho 6 ระยะเดียวกับตอนเล่น)
   - `MapLab_walkable.png` (มุมเดียวกับ overview ทับด้วยช่องที่เดินไม่ได้สีแดง โพลิกอนสิ่งกีดขวางสีเหลือง ขอบแมพสีฟ้า)

ฉาก demo ไม่มีผู้เล่นหรือตัวละคร จึงยังกด Play แล้วเดินดูไม่ได้ ถ้าต้องการลองเดิน ใช้ฉากลองเล่นของเกม (หัวข้อ 9)

## 5. ผลที่ควรได้

```
Main Camera               Camera ortho 6 + Post Processing
[MapLab] Environment      MapLabMap (Bounds 50 x 34, Cell Size 0.5, Walkable Data = MapLab_Demo_MapLab/Environment/Walkable.json)
  Ground
    Grass Base            SpriteRenderer แบบ Tiled (หญ้าสีเรียบ 4x4 หน่วยต่อรอบ)
    Ground Patches        แผ่นหญ้า/ดินสีอ่อน หมุนสุ่ม เพื่อตัดการซ้ำของลาย
  Roads (Set)             Road West, Road East, Branch North, Branch South (PathShape)
  Plaza (Set)             Junction (แบน ลายดิน), Plaza (แบน ลายหิน + ขอบ), Pillar x4 (Box สูง 2.2)
  Cliffs (Set)            Plateau (สูง 1.75) > Tier 2 (+1.25) > Tier 3 (+1), Void Rock (สูง 2)
  Ruins (Set)             Ring Wall (สูง 1.2)
  Props (Set)             Boulder x7, Tree x2 (ลำต้น + Canopy ที่จางลงตอนผู้ชมเดินเข้าใต้)
  [MapLab] Baked Ground   chunk ภาพพื้น (หลัง BakeDemoBatch หรือกด Bake Static Ground)
  Lights                  Global Light (ขาว 1.0) + ไฟจุด 4 ดวง (เบา)
  Atmosphere              Global Volume, Ground Mist (จางมาก), Dust Motes
  Navigation              NavigationManager ของตัวอย่าง (Auto, อ่าน Walkable.json ของแมพ) มีเฉพาะเมื่อมีโฟลเดอร์ Samples
```

### ลำดับการวาด (ทุกอย่างอยู่ใน sorting layer `Default`)

| Order | ของ |
|---|---|
| -100 | หญ้าพื้น |
| -96 | แผ่นมอส |
| -92 / -91 / -90 | ทางเดิน / ทางแยก / ลาน |
| -50 | หมอกพื้น |
| 1 / 2 | เงาพร็อพ และเงา flat card ของ Extruded Shape (ลูก `Shadow`) / เงาสัมผัสของพร็อพ |
| **6** | **พร็อพ และ Extruded Shape ที่เปิด Y Sort: เรียงตาม Y กับตัวละครที่ใช้ order 6 เหมือนกัน** (shape เรียงตามจุดบนพื้นเป็นแถบๆ) |
| 95 | ฝุ่น |
| 100 | ใบไม้ที่บัง |

Y-sort ต้องตั้ง Transparency Sort Mode เป็น Custom Axis (0, 1, 0) ใน Renderer2D ของโปรเจกต์ (Pale Vertica ตั้งไว้แล้วใน `Renderer2D.asset`) และทำงานเฉพาะของที่ **order เท่ากัน** เพราะ Unity เทียบ sorting layer และ order ก่อนตำแหน่ง Y

## 6. แก้เส้นโค้ง

- ทุกรูปแก้แบบเดียวกัน: เลือก object (เส้นทางใต้ `Paths`, ลาน `Plaza`, ที่สูง/เสาใต้ `Shapes`) กด **Edit Points** แล้วลากจุดใน Scene view (Shift + คลิก = ลบ, Ctrl/Cmd + คลิก = มุมหัก, จุดเขียว = เพิ่มจุด)
- ทางแยก: ให้ปลายทางเดินจมอยู่ใต้ `Junction` หรือ `Plaza` (order สูงกว่า) รอยต่อจะถูกบัง
- ที่สูง: ลาก slider **Height** ด้านบนลอยขึ้น ฐานและ collider อยู่ที่ footprint เดิม แล้ว Bake Walkable Area ใหม่
- **การกด Build อีกครั้งจะลบทุกอย่างใต้ `[MapLab] Environment`** ถ้าจะเก็บงานที่ลากเอง ให้ย้าย object ออกจาก root นั้นก่อน หรือแก้พิกัดใน `MapLabGroundBuilder`

## 7. เปลี่ยนภาพ placeholder เป็นภาพจริง

ภาพที่สร้างให้เป็นภาพทดสอบที่โค้ดวาดเอง (ไม่มีลิขสิทธิ์ของคนอื่นปน) จะวาดเอง หรือใช้ภาพจาก AI ก็ได้ ถ้าจะขาย ต้องแน่ใจว่าเรามีสิทธิ์ขายภาพนั้น ขอให้ทำตามนี้:

| ภาพ | กติกา |
|---|---|
| สไตล์ | สี flat 2 ถึง 3 โทน ขอบคม เส้นขอบเข้มกว่า ไม่ใช้ noise หรือไล่สีนุ่ม |
| Fill (`GrassFill`, `DirtFill`, `CobbleFill`) | ต่อขอบได้ทั้ง 4 ด้าน (seamless), Wrap Mode = Repeat |
| `PathEdge` (ลายของ Path Shape) | แกนนอนไล่ตามความยาวเส้นทาง ต่อขอบซ้ายขวาได้, นอกแถบเป็นโปร่งใส |
| `WallStone` (ลายกำแพงของ Extruded Shape) | ภาพขาวเทา (สีจริงมาจาก Wall Color) ต่อขอบได้ทั้ง 4 ด้าน Wrap = Repeat แถวบนสุดของภาพจะชิดขอบบนของกำแพง |
| `TopGrass`, `TopStone` (ลายด้านบนของ Extruded Shape) | ภาพขาวเทา (สีจริงมาจาก Top Color) ต่อขอบได้ทั้ง 4 ด้าน Wrap = Repeat |
| `MossPatch`, `SoftShadow` | ภาพขาว สีจริงมาจากสีของ SpriteRenderer |
| พร็อพ | pivot ที่จุดแตะพื้น, Sort Point = Pivot บน SpriteRenderer |
| ทั้งหมด | 128 พิกเซลต่อหน่วย (ที่ 1080p ซูม 6 จอแสดง 90 px ต่อหน่วย จึงยังคม) |

วิธีแทน: เขียนไฟล์ทับชื่อเดิมใน `Demo/Generated/` แล้วตรวจ import setting ให้เหมือนเดิม จะลบไฟล์เพื่อให้วาดใหม่ก็ได้

## 8. ของบังหน้า (OccluderFadeController)

- วางบน object ที่มี SpriteRenderer ของสิ่งที่บัง (เช่นใบไม้) + Collider2D แบบ **Trigger** ครอบพื้นที่ที่เท้าผู้ชมจะถูกบัง
- ตั้ง object นั้นเป็น layer **Ignore Raycast** เพื่อไม่ให้การคลิกเลือกเป้าด้วย `Physics2D.OverlapPoint` ชนกับ trigger นี้
- ค่า: `Renderers` (ภาพที่จะจาง), `Viewer Layers` (layer ของ collider ที่ทำให้จาง ปกติคือ layer ของผู้เล่น), `Faded Alpha` (0.35), `Fade Duration` (0.25 วินาที)
- ทำงานจาก trigger enter/exit เท่านั้น ไม่มีการเช็กใน `Update` และไม่รู้จักระบบผู้เล่นของเกมใดๆ
- collider ที่เดินเข้าต้องมี Rigidbody2D (ฝั่งใดฝั่งหนึ่ง) ไม่อย่างนั้น Unity ไม่ส่ง trigger event

## 9. ขอบเขตของโมดูล และการเชื่อมกับเกมภายหลัง

กฎที่ต้องรักษา:

1. ห้ามอ้างโค้ด ฉาก prefab หรือ layer เฉพาะของ Pale Vertica จากในโมดูล (asmdef กันโค้ดให้แล้ว ส่วนชื่อฉาก ชื่อ layer และ path ต้องระวังเอง)
2. ทิศทางการพึ่งพามีทางเดียว: เกมเรียก MapLab ได้ MapLab ไม่เรียกเกม
3. ผลลัพธ์ที่ให้คนอื่นใช้คือ **ไฟล์** (JSON + PNG ที่ bake) ไม่ใช่ API ผู้ใช้อ่านไฟล์ได้โดยไม่อ้างโค้ด MapLab ถ้าจะเปลี่ยนรูปแบบไฟล์ ต้องเพิ่ม `version`
4. ค่าที่ตั้งจากภายนอกได้: `Viewer Layers` ของ `OccluderFadeController` และ (ในตัวอย่าง) `NavigationManager.GridPathfinder`
5. ตัวอย่างใน `Samples/` เป็นผู้ใช้ข้อมูลเหมือนคนอื่น แกนห้ามอ้างมัน (แกนส่ง event ได้ ตัวอย่างเป็นคนฟัง)
6. เอกสารอยู่ใน `Docs/` ตามกฎของโปรเจกต์ ไฟล์คู่มือที่จะแนบไปกับแพ็กเกจทำตอนแพ็กของขาย

สิ่งที่ถูกตัดออกตอนแยก (เคยผูกกับเกมในเฟส 1):

| เคยผูก | ตอนนี้ | ถ้าจะต่อกลับ |
|---|---|---|
| `AgentHandle.NavigateTo` เรียก `NavigationManager.FindPath` | ต่อแล้วฝั่งเกม (2026-10-24): `AgentCore.TryFindPath` อ่าน `Walkable.json` ผ่าน `WalkableGridManager` ของเกมเอง (ไม่อ้าง MapLab) `PathfinderGrid` + `Pathfinder2D` เดิมถูกลบแล้ว ฉากที่ไม่มี manager AI เดินตรง | ใส่ Walkable Grid Manager ในฉาก ดู [Walkable_Grid_Pathfinding_Walkthrough_TH.md](./Walkable_Grid_Pathfinding_Walkthrough_TH.md) |
| `OccluderFadeController` ฟัง `PlayerCore.OnPlayerChanged` | ดูจาก `Viewer Layers` | ตั้ง `Viewer Layers` = `Player` (ผู้เล่นของเกมอยู่ layer 3) |
| ตัวสร้างฉากคัดลอก `SandBox.unity` และหา `CameraController` | สร้างฉาก demo เปล่าเอง หา Main Camera | ตัดสินใจทีหลังว่าเกมจะใช้ MapLab สร้างแมพจริงอย่างไร |
| ตัวเรนเดอร์สลับวัสดุตัวละครเป็นของ `EntityVisualController` (ภาพ `MapLab_gameplay_runtime.png`) | เอาออก | ปัญหาตัวละครไม่รับแสง 2D เป็นของฝั่งเกม ต้องตรวจด้วยเครื่องมือฝั่งเกมตอนแก้ shader |
| สิ่งกีดขวางอยู่ layer `Ocstacle` | ในฉาก demo อยู่ layer `Default` การ bake นับทุก collider ที่ไม่ใช่ trigger ใต้ root ของแมพ (กรอง layer ได้ถ้าต้องการ) | ถ้าเกมอ่านไฟล์ที่ bake ก็ไม่ต้องพึ่ง layer นี้ ถ้ายังยิง physics เอง ให้ตั้ง layer ให้ตรง |

`Assets/Scenes/MapLab/MapLab.unity` (ฉากที่คัดลอกจาก SandBox ไว้ลองเล่น) ยังอยู่ฝั่งเกม เก็บภาพแมพที่ build ไว้ก่อนแยก เครื่องมือของ MapLab ไม่สร้างฉากนี้ใหม่แล้ว ฉากนี้ไม่มี MapLabMap และ NavigationManager ในฉากไม่มีไฟล์ข้อมูล จึงไม่สร้าง NavMesh ภาพและ Volume ของฉากนี้ชี้ไปที่ `Assets/ThirdParty/MapLab/Demo/` (อ้างด้วย GUID จึงยังทำงาน และตอนนี้เป็นภาพสี flat แล้ว) ส่วนภาพและ profile ของ SpriteShape เดิม (หน้าผา ทางเดิน ทางแยก ลาน) และภาพเสาแบบเก่า ที่ demo ไม่ใช้แล้ว ถูกย้ายไป `Assets/Scenes/MapLab/Legacy/` พร้อม `.meta` เพื่อให้ฉากนี้ยังแสดงได้เหมือนเดิม ใบไม้ในฉากนี้ไม่มีค่า `Viewer Layers` ที่บันทึกไว้ จึงใช้ค่าเริ่มต้นคือจางกับทุก layer (ผู้เล่นและศัตรูที่เดินเข้าใต้ต้นไม้ทำให้จางทั้งคู่)

ก่อนขายยังต้องทำ: ชื่อสินค้าและ namespace ที่ไม่ชนกับของคนอื่น (ตอนนี้ใช้ `MapLab`), ฉาก demo ที่มีตัวเดินลองได้, คู่มือที่แนบไปกับแพ็กเกจ (รวมรูปแบบไฟล์ที่ bake), ตรวจสิทธิ์ของภาพทุกชิ้น, ลองนำเข้าในโปรเจกต์เปล่าที่มีแค่ URP 2D

## 10. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| compile ไม่ผ่านหลังเพิ่ม `using CoreSystem...` ในโมดูล | ตั้งใจให้เป็นแบบนั้น ห้ามอ้างโค้ดเกมจาก MapLab ให้ส่งค่าผ่านจุดต่อในหัวข้อ 9 แทน |
| เมนู Tools > MapLab ไม่ขึ้น | assembly `MapLab.Editor` compile ไม่ผ่าน ดู Console (มักเป็นเพราะไม่มี URP ในโปรเจกต์) |
| พร็อพสูงวาดทับตัวละครที่ยืนข้างหน้า | Sort Point ยังเป็น Center ให้ตั้งเป็น Pivot และ pivot อยู่ที่ฐาน |
| พร็อพอยู่หน้าหรือหลังตัวละครเสมอ | order ไม่ใช่ 6 หรือ Renderer2D ไม่ได้ตั้ง Custom Axis |
| คลิกศัตรูใต้ต้นไม้ไม่ติด | trigger ของใบไม้ไม่ได้อยู่ layer Ignore Raycast |
| ใบไม้ไม่จาง | collider ที่เดินเข้าไม่อยู่ใน `Viewer Layers` หรือไม่มี Rigidbody2D |
| ทางเดินเป็นเส้นตรงแข็ง / มีแถบเพี้ยน | ภาพ edge ไม่ใช่ Full Rect หรือ fill ไม่ได้ตั้ง Repeat |
| ภาพยังเป็นแบบเก่าหลัง build | ตัววาดไม่ทับไฟล์ที่มีอยู่ ลบ `.png` (เก็บ `.meta`) แล้ว build ใหม่ |
| ภาพพรีวิวไม่มีหมอก | หมอกจางมากโดยตั้งใจ (ถูกจำลองล่วงหน้า 20 วินาทีก่อนถ่าย) ถ้าต้องการหนาขึ้น ปรับใน `MapLabSceneBuilder.BuildAtmosphere` |
| ใน `Logs/MapLab/` ยังมี `MapLab_gameplay_editor.png` / `_runtime.png` | ภาพเก่าจากก่อนแยกโมดูล ตัวเรนเดอร์ไม่สร้างไฟล์ชื่อนี้แล้ว ลบได้ |
