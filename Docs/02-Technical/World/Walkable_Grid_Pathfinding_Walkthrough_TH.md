# Walkable Grid Pathfinding Walkthrough (TH)

ระบบหาทางของ AI (`AgentCore`) ฝั่งเกม ที่อ่าน **ไฟล์พื้นที่เดินได้ที่ MapLab bake ไว้** (`Walkable.json`) แล้วทำ A* บนกริดนั้น (แทน `PathfinderGrid` เดิมที่ยิง `Physics2D.OverlapCircle` ทีละช่อง ซึ่งลบแล้ว)

- อัปเดต: 2026-10-24 (AI หยุดที่ปลายเส้นทางเมื่อเป้าเดินไม่ถึง, สุ่มจุด Roam เฉพาะที่เดินได้, ไม่เดินตรงเมื่อกริดบอกว่าไม่มีทาง)
- โค้ด (namespace `CoreSystem.Pathfinding`, โฟลเดอร์ `Assets/Script/CoreSystem/Pathfinding/`):
  - `WalkableGrid`: ข้อมูลกริด (เดินได้/ไม่ได้, ระยะเผื่อตัว agent, กลุ่มพื้นที่ที่เดินถึงกันได้)
  - `WalkableGridLoader`: อ่าน `Walkable.json` (format v1) เป็น `WalkableGrid`
  - `GridPathFormula`: A* 8 ทิศ + ตัดจุดเลี้ยว (line of sight) คืนรายการจุดเดิน และบอกผลเป็น `PathOutcome`; `SnapPoint` ขยับจุดที่อยู่ในสิ่งกีดขวางออกมา
  - `PathOutcome`: ผลการหาทาง `NoGrid` (ไม่มีกริดตรงจุดเริ่ม), `NoRoute` (มีกริดแต่ไม่มีทาง), `ReachesTarget` (ไปถึงเป้าจริง), `StopsShort` (ไปได้แค่ช่องว่างที่ใกล้เป้าที่สุด)
  - `WalkableGridManager`: component ในฉาก (ฉากละ 1 ตัว) ถือไฟล์ JSON ของทุกแมพในฉาก
- ผู้ใช้: `AgentCore.FindPath` / `AgentCore.SnapPoint` → `AgentHandle.NavigateTo` (Roam, Chase, CommandMove, CommandFollow) และการสุ่มจุด Roam
- **ไม่อ้างโค้ด MapLab เลย**: เกมอ่านแค่ไฟล์ JSON ตามรูปแบบใน [Walkable_Area_Bake_Walkthrough_TH.md](./Walkable_Area_Bake_Walkthrough_TH.md) หัวข้อ 5 และ MapLab ไม่รู้ว่ามีระบบนี้
- ระบบเดิม `PathfinderGrid` + `Pathfinder2D` + `PathNode` ถูกลบแล้ว (2026-10-24) ฉากที่ไม่มี `WalkableGridManager` AI จะเดินตรงไปหาเป้า

```
MapLab Bake (Editor) -> <ฉาก>_MapLab/<แมพ>/Walkable.json
                              |
WalkableGridManager.Awake  -> WalkableGridLoader.TryLoad(json, Agent Radius)  -> WalkableGrid (1 ตัวต่อแมพ)
                              |  raise OnGridsChanged
AgentCore  (ฟัง OnGridsChanged -> ล้างเส้นทางเก่าของทุก agent)
  AgentHandle.NavigateTo(dest)  ทุก PathRefreshRate วินาที
    AgentCore.FindPath(from, to, list) -> PathOutcome
      ReachesTarget  เดินตามจุด แล้วเดินเข้าเป้า
      StopsShort     เดินตามจุด แล้วหยุดที่จุดสุดท้าย (เป้าอยู่ในสิ่งกีดขวางหรือในที่ที่ไปไม่ถึง)
      NoRoute        หยุดอยู่กับที่ (มีกริดแต่ไม่มีทาง หรือเกินงบการค้นหา)
      NoGrid         เดินตรงไปหาเป้า (ฉากไม่มี manager หรือ AI อยู่นอกทุกแมพ)
```

## 1. เปิดที่ไหน

| ทำอะไร | ที่ไหน |
|---|---|
| ใส่ระบบให้ฉาก | Hierarchy เลือก GameObject ใดก็ได้ในฉาก (แนะนำ object แยก เช่น `Navigation`) > Add Component > **Pale Vertica > World > Walkable Grid Manager** |
| เติมไฟล์ JSON ใหม่ | คลิกขวาที่หัว component > **Find Baked Walkable Data** |
| ใส่ให้ SandBox ตอน Unity ปิด | `Tools/FigmaToUnity/unity_batch.sh CoreSystem.Editor.World.WalkableGridSceneInstaller.InstallSandBoxBatch` (สร้างหรืออัปเดต object `Walkable Grid` ใน `SandBox.unity` ให้ใช้ `SandBox_MapLab/Environment/Walkable.json` แล้ว save ฉาก) ฉากอื่นเรียก `WalkableGridSceneInstaller.Install(scenePath, jsonPath...)` จาก editor script (`Assets/Script/Editor/World/`) |
| ดูกริดในฉาก | เลือก object ที่มี Walkable Grid Manager (Gizmos เปิด): กรอบฟ้า = ขอบแมพ, แดง = สิ่งกีดขวางจาก bake, ส้ม = ช่องที่ปิดเพราะ Agent Radius |
| ดูเส้นทางของ AI ตอนเล่น | เลือกตัว AI: เส้นฟ้าอมเขียว + วงกลมเล็ก = จุดเดินที่เหลือ (`AgentGizmos`) |

## 2. Input (ช่องของ Walkable Grid Manager)

| ช่อง | ค่าเริ่มต้น | ความหมาย |
|---|---|---|
| Walkable Data | เติมให้เองตอน Add Component | รายการ `Walkable.json` ทุกแมพในฉาก (`<ฉาก>_MapLab/*/Walkable.json`) แมพละ 1 ไฟล์ |
| Agent Radius | 0.25 | รัศมีตัว AI (หน่วยโลก) ช่องที่จุดกลางอยู่ใกล้สิ่งกีดขวางน้อยกว่าค่านี้จะเดินไม่ได้ และเส้นตรงที่ตัดมุมต้องเว้นระยะนี้จากสิ่งกีดขวาง collider ตัวละครรัศมี 0.25 |
| Max Search Cells | 20000 | งบการค้นหาต่อครั้ง (จำนวนช่องที่ขยาย) เกินแล้วถือว่าหาไม่เจอ |
| Snap Cells | 4 | ถ้าจุดเริ่มอยู่ในสิ่งกีดขวาง หรือเป้าอยู่ในสิ่งกีดขวาง / ในพื้นที่ที่เดินไปไม่ถึง ให้ขยับไปช่องที่ใช้ได้ที่ใกล้ที่สุดไม่เกินกี่ช่อง |

ช่องใน `AgentProfile` ที่เกี่ยว: **Use Pathfinding** (ปิด = เดินตรงเสมอ), **Path Refresh Rate** (หาเส้นทางใหม่ทุกกี่วินาที ค่าเริ่ม 0.5)

## 3. ขั้นตอน

1. Bake แมพใน MapLab ก่อน (ปุ่ม **Bake Walkable Area** บน `MapLabMap`) จะได้ `<ฉาก>_MapLab/<แมพ>/Walkable.json`
2. save ฉาก (ระบบหาไฟล์จากที่อยู่ของฉาก)
3. Add Component **Walkable Grid Manager** บน object ในฉาก ช่อง Walkable Data จะถูกเติมด้วย `Walkable.json` ทุกไฟล์ใต้ `<ฉาก>_MapLab/` (ถ้าว่าง: ฉากยังไม่ save หรือยังไม่ได้ bake ให้กด **Find Baked Walkable Data** หลัง bake)
4. ตั้ง Agent Radius ให้ตรงกับตัวละคร (ปกติ 0.25)
5. เลือก object นั้นใน Scene view ตรวจว่าช่องแดงตรงกับกำแพง หิน ต้นไม้
6. กด Play ให้ AI ไล่ผู้เล่นอ้อมสิ่งกีดขวาง เลือกตัว AI ดูเส้นทาง
7. แก้แมพเมื่อไร ต้อง Bake ใหม่ (ไฟล์ JSON ถูกเขียนทับที่เดิม Walkable Data ไม่ต้องตั้งใหม่)

## 4. ผลที่ควรได้

- AI เดินเป็นเส้นตรงระหว่างจุดเลี้ยว ไม่เดินขั้นบันไดตามกริด และไม่ตัดมุมสิ่งกีดขวาง
- เป้าอยู่ในสิ่งกีดขวาง (เช่น ผู้เล่นยืนชิดหิน): AI หยุดที่ช่องว่างที่ใกล้เป้าที่สุด ฝั่งที่ใกล้ตัว AI และ**ไม่ดันเข้าไปต่อ** (ก่อน 2026-10-24 พอถึงปลายทางแล้ว AI เดินตรงเข้าหาเป้าที่อยู่ในกำแพงจนชน) ถ้าเป้าขยับเกิน 1 หน่วย AI หาทางใหม่ทันที
- เป้าอยู่ในพื้นที่ที่เดินไปไม่ถึง (เช่น บนที่สูงที่ไม่มีทางขึ้น): ถ้ามีช่องที่ไปถึงได้ห่างจากเป้าไม่เกิน Snap Cells จะไปหยุดตรงนั้น ถ้าไม่มี AI ยืนรอ (ไม่เดินตรงเข้ากำแพง) และ CommandMove ไปจุดแบบนี้จบที่ปลายทางแล้วกลับเป็น Idle
- Roam: สุ่มจุดในรัศมีบ้านได้สูงสุด 6 ครั้ง เอาจุดแรกที่เดินถึงได้จริง ถ้าทุกจุดอยู่ในสิ่งกีดขวาง ใช้ช่องว่างที่ใกล้จุดแรกแทน ถ้าไม่มีเลยยืนอยู่ที่เดิม (ก่อนหน้านี้ ally ใน SandBox ซึ่งเกิดชิดหน้าผา Plateau ได้จุดในหน้าผาราว 1 ใน 3 แล้วดันกำแพงจนหมดเวลา 8 วินาที)
- เวลาที่วัดได้ (กริด 100 x 68 ของฉาก demo และ SandBox, คู่จุดสุ่ม 300 คู่ นอก Unity): เฉลี่ยราว 0.2 ms ต่อการหาทาง 1 ครั้ง, ช้าสุดราว 2.5 ms (รวมครั้งแรก)

## 5. กติกาของการหาทาง (`GridPathFormula`)

| เรื่อง | กติกา |
|---|---|
| ช่องของกริด | ช่อง (column, row) ครอบ `bounds.x + column * cellSize` ถึงช่องถัดไป แถว 0 อยู่ล่างสุด ตรงกับไฟล์ที่ bake |
| ทิศการเดิน | 8 ทิศ ตรง = 1, ทแยง = 1.414 ทแยงได้เฉพาะเมื่อช่องข้างทั้งสองเดินได้ (ไม่ตัดมุม) |
| Agent Radius | ช่องที่จุดกลางห่างจากช่องสิ่งกีดขวางน้อยกว่ารัศมีถูกปิด (ช่อง 0.5 + รัศมี 0.25 ไม่ปิดอะไรเพิ่ม, รัศมี 0.3 ปิดเพิ่ม 1 ชั้น) |
| กลุ่มพื้นที่ (region) | ตอนโหลด ช่องที่เดินถึงกันได้ได้เลขกลุ่มเดียวกัน ถ้าเป้าอยู่คนละกลุ่มกับจุดเริ่มจะไม่ค้นหาทั้งแมพให้เสียเวลา แต่ขยับเป้าเข้ากลุ่มเดียวกัน (Snap Cells) |
| ตัดจุดเลี้ยว | จากจุดปัจจุบันเดินหน้าไปจุดถัดไปบนเส้นทางตราบที่ยังมองเห็น (เส้นกลางผ่านเฉพาะช่องที่เดินได้ และเส้นขนานซ้ายขวาห่าง Agent Radius ไม่ทับช่องสิ่งกีดขวาง) |
| ผลลัพธ์ | รายการจุดเดินในโลก ไม่รวมจุดเริ่ม จุดสุดท้ายคือเป้าจริงถ้าเป้าเดินได้ ไม่งั้นเป็นจุดกลางช่องที่ขยับไป ถ้าจุดเริ่มถูกขยับ จุดแรกคือจุดกลางช่องนั้น |
| หลายแมพในฉาก | ใช้กริดของแมพที่มีจุดเริ่มอยู่ ไม่หาทางข้ามแมพ |

## 6. เหตุการณ์ (event)

| Event | ใครส่ง | ใครฟัง |
|---|---|---|
| `WalkableGridManager.OnReady` (static) | ตอน `Awake` ของ manager | ระบบที่อาจเริ่มก่อน manager |
| `WalkableGridManager.OnGridsChanged` (static) | โหลดกริด (`Awake`, `Load()`, `SetWalkableData`) และตอน manager ถูกทำลาย | `AgentCore` ล้างเส้นทางของทุก agent ให้หาใหม่ทันที |

เปลี่ยนแมพระหว่างเล่น (เช่น เปิดประตูแล้วใช้ไฟล์ที่ bake อีกแบบ): `WalkableGridManager.Instance.SetWalkableData(newJson)` ไม่ต้องแจ้ง AI เอง

## 7. สถานะและสิ่งที่ยังไม่ได้ทำ

| เรื่อง | สถานะ |
|---|---|
| `PathfinderGrid` + `Pathfinder2D` + `PathNode` + `PathfinderGridTests` | ลบแล้ว 2026-10-24 พร้อม object `PathfinderGrid` ใน SandBox และทางสำรองใน `AgentCore` (ฉากใน `Assets/_Recovery/` ยังอ้าง script ที่หายไป เป็นไฟล์กู้คืนของ Unity ไม่ได้ใช้) |
| `Assets/Scenes/SandBox.unity` | มี object `Walkable Grid` (WalkableGridManager) ใช้ `Assets/Scenes/SandBox_MapLab/Environment/Walkable.json` แล้ว (2026-10-24) AI ในฉากนี้ใช้กริดใหม่ (ทดสอบตอนเล่นแล้ว เดินอ้อมสิ่งกีดขวางได้) |
| สิ่งกีดขวางที่เปลี่ยนตอนเล่น (ประตู กำแพงพัง) | ไม่อยู่ในไฟล์ bake ใช้ `SetWalkableData` กับไฟล์ที่ bake ไว้หลายแบบ หรือทำระบบเพิ่มทีหลัง |
| AI หลบกันเอง | ยังไม่มี (เหมือนเดิม) physics ของตัวละครดันกันเอง |
| `Flee` และการขยับตอนต่อสู้ (`ComputeCombatPosition`) | ยังเดินตรงเหมือนเดิม ไม่ใช้การหาทาง (ตอนสู้หรือหนีชิดกำแพงยังดันกำแพงได้) |
| งบการค้นหาบนแมพใหญ่ | เกิน Max Search Cells แล้วได้ `NoRoute` AI จะยืนรอแทนการเดินตรง ถ้าแมพใหญ่มาก (เช่น 320 x 320 ช่อง) และ AI ยืนนิ่งตอนไล่ไกลๆ ให้เพิ่ม Max Search Cells |

## 8. การทดสอบ

- `Assets/Script/Editor/Tests/WalkableGridTests.cs`: อ่านหัวไฟล์และบิต (แถว 0 อยู่ล่าง), ปฏิเสธ version อื่น / base64 เสีย / บิตไม่พอ / ขนาดผิด, Agent Radius ปิดช่องถูกต้อง, แบ่งกลุ่มพื้นที่, อ่านไฟล์ bake ของฉาก demo ได้ตรงกับภาพ mask ทุกช่อง, manager โหลด TextAsset และส่ง `OnGridsChanged`, ข้ามไฟล์เสียพร้อมคำเตือน
- `Assets/Script/Editor/Tests/GridPathFormulaTests.cs`: ผลการหาทาง (`ReachesTarget` / `StopsShort` / `NoRoute` / `NoGrid`), `SnapPoint` เก็บจุดที่เดินได้และย้ายจุดในสิ่งกีดขวางมาฝั่งที่ไปถึง, ที่โล่งเดินตรง, อ้อมกำแพงผ่านช่องว่าง, ไม่ลอดมุมทแยง, เป้าในสิ่งกีดขวาง, จุดเริ่มในสิ่งกีดขวาง, งบการค้นหา, จุดเริ่มนอกกริด, line of sight ผ่านมุม, เว้นระยะตัว agent, หาทางอ้อมสิ่งกีดขวางบนไฟล์ bake จริงของฉาก demo (ทุกช่วงไม่ทับสิ่งกีดขวาง)
- รัน: **Window > General > Test Runner > EditMode** หรือตอน Unity ปิด `Tools/FigmaToUnity/unity_batch.sh tests`

## 9. ข้อผิดพลาดที่พบบ่อย

| อาการ | สาเหตุ / วิธีแก้ |
|---|---|
| AI ยังเดินชนกำแพงตรงๆ | ฉากไม่มี Walkable Grid Manager, Walkable Data ว่าง, `AgentProfile.UsePathfinding` ปิด, AI อยู่นอก Bounds ของแมพ หรืออยู่ในสถานะ Attack / Flee ที่ยังเดินตรง |
| AI ยืนนิ่งทั้งที่เห็นเป้า | ได้ `NoRoute`: เป้าอยู่คนละฝั่งของกำแพงที่ปิดทาง หรือเกิน Max Search Cells ดูช่องแดงใน Gizmos ของ manager |
| Walkable Data ว่างหลัง Add Component | ฉากยังไม่ save หรือยังไม่ได้ bake แมพ ให้ bake แล้วกด **Find Baked Walkable Data** |
| Console: `[WalkableGridManager] Walkable: ... version` | ไฟล์ไม่ใช่ format v1 หรือไม่ใช่ไฟล์จาก MapLab ใส่ `Walkable.json` ไม่ใช่ `Walkable_Mask.png` หรือไฟล์อื่น |
| Console: "Another manager is already active" | มี Walkable Grid Manager 2 ตัวในฉากที่เปิดพร้อมกัน เก็บไว้ตัวเดียวแล้วใส่ไฟล์ทุกแมพในตัวนั้น |
| AI ไม่ยอมเข้าทางแคบ | Agent Radius ใหญ่เกินจนปิดทาง ดูช่องส้มใน Gizmos ลดรัศมี หรือ bake ด้วย Cell Size ที่เล็กลง |
| เส้นทางทะลุสิ่งกีดขวางใหม่ | ยังไม่ได้ bake ใหม่หลังแก้แมพ |
| แก้ Walkable Data ตอน Play แล้วไม่มีผล | ค่าถูกอ่านตอน `Awake` เรียก `SetWalkableData` หรือ `Load()` แทน |
