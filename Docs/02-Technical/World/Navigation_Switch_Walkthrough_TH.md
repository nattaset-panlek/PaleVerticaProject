# Navigation Switch Walkthrough (TH)

**ตัวอย่างผู้ใช้ข้อมูล** ที่มากับแพ็กเกจ MapLab: อ่านไฟล์พื้นที่เดินได้ที่ bake ไว้ ([Walkable_Area_Bake_Walkthrough_TH.md](./Walkable_Area_Bake_Walkthrough_TH.md)) แล้วสร้าง NavMesh บนระนาบ XY พร้อมสวิตช์ต่อแมพ Auto / Grid / NavMesh ผู้เรียกไม่ต้องรู้ว่าแมพใช้แบบไหน

- อัปเดต: 2026-10-23
- โค้ด: `Assets/ThirdParty/MapLab/Samples/Navigation/` namespace `MapLab.Samples.Navigation`
  - `Runtime/` assembly `MapLab.Samples`: `NavigationManager`, `NavigationMode`, `NavMesh2DFactory`, `WalkableAreaData`
  - `Editor/` assembly `MapLab.Samples.Editor`: `DemoNavigationInstaller` (ใส่ NavigationManager ให้ฉาก demo)
- **ไม่อ้างแกน MapLab**: assembly `MapLab.Samples` ไม่มี reference ไป `MapLab` เลย อ่านแค่ไฟล์ JSON ผ่าน class ของตัวเอง (`WalkableAreaData`) ลบโฟลเดอร์ `Samples/` ทิ้งได้โดยแกนไม่พัง
- ไม่ต้องติดตั้ง package: ใช้ NavMesh runtime ของ Unity (`com.unity.modules.ai`)
- สถานะ: **เกมไม่ได้ใช้ตัวอย่างนี้** AI ของ Pale Vertica เลือกทางที่ 2 ในหัวข้อ 3 (ตัวอ่านของเกมเอง) ดู [Walkable_Grid_Pathfinding_Walkthrough_TH.md](./Walkable_Grid_Pathfinding_Walkthrough_TH.md)

## 1. ทำงานอย่างไร

```
MapLab Bake -> <ฉาก>_MapLab/<แมพ>/Walkable.json
                  |
NavigationManager.Rebuild()
  WalkableAreaData.Parse(json)           อ่าน bounds + holes
  NavMesh2DFactory.Build(bounds, holes)  พื้นสี่เหลี่ยม + เจาะรูตามโพลิกอน (ตัดเป็นสามเหลี่ยมแบบ ear clipping รองรับรูปเว้า)
                  |
ผู้เรียก -> NavigationManager.FindPath(from, to)
       Mode = Grid                      -> NavigationManager.GridPathfinder
       Mode = Auto, มี NavMesh          -> NavMesh.CalculatePath
       Mode = Auto, ไม่มี NavMesh        -> GridPathfinder
       Mode = NavMesh, ยังไม่มี NavMesh  -> GridPathfinder + เตือนใน Console ครั้งเดียว
       ฉากไม่มี NavigationManager       -> GridPathfinder
       ไม่ได้ตั้ง GridPathfinder         -> null (ไม่มีเส้นทาง)
```

- ผลที่ได้คือรายการจุดเลี้ยว (corner) ไม่รวมจุดเริ่ม ผู้เรียกเดินตามเองด้วยระบบเดินของตัวเอง
- `GridPathfinder` คือ `Func<Vector2, Vector2, List<Vector2>>` แบบ static ตัวอย่างนี้ไม่มีกริดของตัวเอง ผู้ใช้ส่งเข้ามา (จะเขียน A* จาก `cells` ในไฟล์ที่ bake ก็ได้ `WalkableAreaData.IsWalkable` อ่านช่องให้แล้ว)
- NavMesh ถูกสร้างบนระนาบ XY (หมุน -90 องศารอบแกน X) เพื่อให้ตรงกับ Physics2D

## 2. ตั้งค่าในแมพ

1. Bake แมพก่อน (ได้ไฟล์ `<ฉาก>_MapLab/<แมพ>/Walkable.json`)
2. สร้าง GameObject แล้ว Add Component **NavigationManager** (ฉากละ 1 ตัว)
3. ตั้งค่า:

| ช่อง | ค่า |
|---|---|
| Mode | `Auto` (แนะนำ), `Grid` (บังคับใช้ `GridPathfinder`), `NavMesh` (ตั้งใจใช้ NavMesh และอยากเห็นคำเตือนถ้าสร้างไม่ได้) |
| Walkable Data | ไฟล์ JSON ที่ bake ไว้ |
| Build On Start | เปิด: สร้าง NavMesh ตอน `Start` (ถ้าไม่มี Walkable Data จะข้ามไปเงียบๆ) |
| Agent Radius | ระยะห่างจากสิ่งกีดขวาง 0.3 (collider ตัวละครของ Pale Vertica รัศมี 0.25) |
| Sample Distance | ระยะที่ยอมดึงจุดเริ่ม/ปลายเข้าหา NavMesh 1.5 |

4. ถ้าต้องการให้แมพที่ไม่มี NavMesh ยังหาทางได้ ให้ตั้ง `NavigationManager.GridPathfinder` ก่อนเรียก `FindPath`
5. กด Play แล้วเลือก object NavigationManager จะเห็นสามเหลี่ยมของ NavMesh เป็นเส้นสีเขียว

ฉาก demo ได้ NavigationManager อัตโนมัติ: ตอน **Build Demo Scene** ตัวสร้างฉากของแกนส่ง event `MapLabSceneBuilder.OnEnvironmentBuilt` และ `DemoNavigationInstaller` ของตัวอย่างนี้เป็นคนฟังแล้วใส่ `[MapLab] Environment/Navigation` ให้ แกนไม่รู้ว่ามีตัวอย่างนี้อยู่

## 3. ต่อกับ Pale Vertica (เลือกทางที่ 2 แล้ว 2026-10-24)

ทางเลือก ไม่ต้องแก้ MapLab ทั้งสองทาง เกมเลือกทางที่ 2 แบบเขียนตัวอ่านใหม่ (`WalkableGridLoader`, `GridPathFormula`, `WalkableGridManager`) แล้วลบ `PathfinderGrid` + `Pathfinder2D` ทิ้ง (2026-10-24) ข้อ 1 ด้านล่างเป็นบันทึกทางเลือกเดิม คลาสที่อ้างไม่มีแล้ว ดู [Walkable_Grid_Pathfinding_Walkthrough_TH.md](./Walkable_Grid_Pathfinding_Walkthrough_TH.md):

1. **ใช้ตัวอย่างนี้**: ตอนเริ่มฉากตั้ง `NavigationManager.GridPathfinder = (from, to) => PathfinderGrid.Instance != null ? Pathfinder2D.FindPath(from, to) : null;` แล้วใน `AgentHandle.NavigateTo` เปลี่ยน `PathfinderGrid.Instance != null` เป็น `NavigationManager.IsAvailable` และ `Pathfinder2D.FindPath` เป็น `NavigationManager.FindPath`
2. **เขียนตัวอ่านของเกมเอง**: ให้ `PathfinderGrid` อ่าน `cells` จาก JSON แทนการยิง `OverlapCircle` ทีละช่อง (ดูรูปแบบไฟล์ใน walkthrough ของการ bake)

## 4. สร้างใหม่ระหว่างเล่น

- เรียก `NavigationManager.Instance.Rebuild()` หลังเปลี่ยนไฟล์ข้อมูล
- ฟัง `NavigationManager.OnNavMeshBuilt` ถ้าระบบไหนต้องรู้ว่า NavMesh เปลี่ยน
- สิ่งกีดขวางที่เปลี่ยนตอนเล่น (ประตูเปิด กำแพงพัง) ไม่อยู่ในไฟล์ที่ bake ต้องจัดการเองนอกตัวอย่างนี้ (เช่น NavMeshObstacle) หรือ bake หลายเวอร์ชัน

## 5. ข้อจำกัดตอนนี้

| เรื่อง | สถานะ |
|---|---|
| NavMeshAgent (หลบกันเองระหว่าง AI) | ยังไม่ทำ ขั้นต่อไปคือใช้ NavMeshAgent แบบ `updatePosition = false` แล้วป้อน `desiredVelocity` เข้าระบบเดินของผู้เรียก |
| กริดในตัวอย่าง | ไม่มี A* ในตัว ต้องส่ง `GridPathfinder` เข้ามา |
| โพลิกอนที่ตัดกันเอง | ear clipping อาจได้สามเหลี่ยมไม่ครบ ส่วนที่ขาดจะเดินได้ (collider ปกติไม่เป็นแบบนี้) |
| ความสูง | ไม่มี ทุกอย่างอยู่บนระนาบเดียว |
| ทดสอบ NavMesh กับ AI ตอนเล่นจริง | ยังไม่ได้ทำ (ฉาก demo ยังไม่มีตัวเดิน และยังไม่ได้ต่อกับเกม) |

## 6. การทดสอบ

- `Assets/ThirdParty/MapLab/Tests/Editor/NavMesh2DFactoryTests.cs`: NavMesh อยู่บนระนาบ XY, จุดในกำแพงเดินไม่ได้, เส้นทางอ้อมกำแพง, สิ่งกีดขวางรูปตัว L (เว้า) ไม่กินพื้นที่ในรอยเว้า
- `Assets/ThirdParty/MapLab/Tests/Editor/NavigationManagerTests.cs`: ไม่มี NavMesh และไม่มี `GridPathfinder` ได้ null, มี `GridPathfinder` ได้ผลจากตัวนั้น
- `Assets/ThirdParty/MapLab/Tests/Editor/WalkableAreaBakeTests.cs`: ตัวอ่านของตัวอย่างอ่านไฟล์ที่ bake ได้ และสร้าง NavMesh ได้
- ของฝั่งเกม: `Assets/Script/Editor/Tests/WalkableGridTests.cs`, `GridPathFormulaTests.cs`
- รัน: **Window > General > Test Runner > EditMode** หรือ `Tools/FigmaToUnity/unity_batch.sh tests` ตอน Unity ปิด

## 7. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| Console: "has no Walkable Data" | เรียก `Rebuild()` โดยยังไม่ได้ใส่ไฟล์ JSON ให้ bake แมพแล้วลากไฟล์ใส่ |
| `FindPath` คืน null ตลอด | ไม่มี NavMesh และไม่ได้ตั้ง `GridPathfinder`, หรือจุดเริ่ม/ปลายห่างจาก NavMesh เกิน `Sample Distance` |
| เส้นทางทะลุสิ่งกีดขวาง | สิ่งกีดขวางไม่ถูก bake (ดูข้อผิดพลาดใน walkthrough ของการ bake) หรือยังไม่ได้ bake ใหม่หลังแก้แมพ |
| AI ของเกมเดินตรงชนกำแพง | ฉากนั้นไม่มี Walkable Grid Manager ดู [Walkable_Grid_Pathfinding_Walkthrough_TH.md](./Walkable_Grid_Pathfinding_Walkthrough_TH.md) |
| ฉาก `Assets/Scenes/MapLab/MapLab.unity` ไม่มี NavMesh | ฉากนั้นสร้างก่อนมีการ bake NavigationManager ในฉากไม่มี Walkable Data จึงข้ามการสร้าง |
