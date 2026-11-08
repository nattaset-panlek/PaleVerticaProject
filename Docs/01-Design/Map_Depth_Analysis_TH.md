# วิเคราะห์ความลึกของแผนที่: ทำแมพแบบ Ragnarok ใน Pale Vertica ยากแค่ไหน

- วันที่: 2026-10-23
- สถานะ: เอกสารวิเคราะห์ ไม่มีการแก้โค้ด ฉาก prefab หรือ Project Settings
- ขอบเขต: เทียบวิธีทำแมพของโปรเจกต์ `D:\Application Project\Ragnarok Offline` กับระบบโลกของ Pale Vertica ตอนนี้ แล้วเสนอทางเลือกและแผนเป็นเฟส

**สัญลักษณ์ความมั่นใจ**

| ป้าย | ความหมาย |
|---|---|
| (ไม่มีป้าย) | เห็นเองในไฟล์หรือค่า setting ที่อ้างไว้ |
| **[อนุมาน]** | สรุปจากโค้ดที่อ่าน แต่ยังไม่ได้รันหรือทดสอบ |
| **[ความรู้ทั่วไป]** | ความรู้เรื่อง Unity หรือ RO ที่ไม่ได้ตรวจในโปรเจกต์นี้ ต้องลองใน Unity ก่อนเชื่อ |

---

## 0. สรุปสั้น

1. **Ragnarok Offline ไม่ได้ "สร้าง" แมพ** มันเป็น client ที่เขียนเองด้วย C# (.NET 10) + OpenGL ไม่ใช่ Unity และอ่านแมพสำเร็จรูปของ Gravity จาก `data.grf` (ไฟล์ `.gnd` `.gat` `.rsw` `.rsm`) ในโปรเจกต์ไม่มีเครื่องมือสร้างหรือแก้แมพเลย และไฟล์เหล่านั้นเป็นลิขสิทธิ์ของ Gravity จึงเอามาใช้ใน Pale Vertica ไม่ได้
2. **ข้อค้นพบสำคัญ: ความสูงของพื้นใน RO มีผลแค่ภาพ** การเดินหาทาง ระยะ และการตัดสินว่าเดินได้ ทำบนกริด 2D ทั้งหมด (`PathFinder.cs` ไม่ใช้ความสูง, ระยะเป็นจำนวนช่องแบบ Chebyshev) ความสูงใช้แค่วาดตัวละครให้ยืนบนเนิน คลิกเลือกช่อง และวาด cursor ให้แนบพื้น
3. **แปลว่า gameplay ของเราไม่ใช่ตัวขวาง** Physics2D, Targeting, PathfinderGrid ของ Pale Vertica อยู่บนระนาบ XY อยู่แล้ว ซึ่งตรงกับโมเดลของ RO ถ้าเราให้ความสูงเป็นแค่ภาพ (แกน Z) ระบบต่อสู้เกือบทั้งหมดใช้ต่อได้
4. **ตัวขวางจริงคือ ภาพและคอนเทนต์**: กล้องตอนนี้เป็น orthographic มองตรง ไม่เอียง (`SandBox.unity` rotation = identity), แปลงเมาส์ด้วย `ScreenToWorldPoint` ซึ่งใช้ได้กับกล้องแบบนี้เท่านั้น, ตัวละครมีแค่ 2 ทิศ (กลับด้านด้วย scale.x = -1), ใน SandBox ยังไม่มีพื้นเลย ไม่มี Tilemap ไม่มี Light2D ไม่มี Volume และยังไม่มี pipeline ทำฉาก 3D
5. **ความยากรวม**: แบบ RO ที่ "หน้าตาเหมือน" (พื้น 3D + ตัวละคร 2D + กล้องเอียงตายตัว) อยู่ระดับ **กลางถึงสูง** ส่วนที่แพงที่สุดคืองานอาร์ตฉาก 3D ไม่ใช่โค้ด ส่วนแบบ RO เต็มรูปแบบ (กล้องหมุนได้ + ความสูงมีผลต่อ gameplay) อยู่ระดับ **สูงมาก** และเกินกว่าที่ RO เองทำ
6. **คำแนะนำ**: ทาง B (HD-2D ไฮบริด) แบบจำกัดขอบเขต: กล้องเอียงตายตัวไม่หมุน, gameplay อยู่บน XY เหมือนเดิม, ความสูงเป็นแค่ภาพ, พื้นที่เดินได้ราบ เริ่มจากงานพื้นฐาน 2D ที่ใช้ได้ทุกทาง (เฟส 1) แล้วทำต้นแบบ 1 ฉากพร้อมเกณฑ์ go/no-go ถ้าไม่ผ่านหรือทีมไม่มีคนทำอาร์ต 3D ให้ถอยไปทาง A2 (2D ที่ใช้ภาพ pre-render จาก 3D) โดยไม่เสียงานที่ทำไปแล้ว

---

## 1. Ragnarok Offline ทำแมพอย่างไร

### 1.1 ภาพรวมของโปรเจกต์

| เรื่อง | ที่เห็น | หลักฐาน |
|---|---|---|
| Engine | ไม่มี engine: WinForms + OpenGL (GL 1.x fixed function + GL 2/3 ผ่าน `GlExt.cs`) | `README.md`, `RagnarokOffline.csproj` (`net10.0-windows`), `AGENTS.md` หัวข้อ Code map |
| แหล่งแมพ | อ่านจาก `data.grf` ของ kRO โดยตรง | `src/World/GameWorld.cs:56-62` โหลด `data\{map}.rsw`, `.gnd`, `.gat` |
| เครื่องมือทำแมพ | ไม่มี ไม่มี editor ไม่มีโค้ดเขียนไฟล์แมพ (`tools/` มีแค่ `setup.ps1` กับปลั๊กอิน Figma) | `ls tools`, ค้น `editor`/write gat ใน `src` ไม่พบ |
| ลิขสิทธิ์ | ไฟล์จาก GRF เป็นของ Gravity ห้าม commit | `README.md` หัวข้อ Getting Started |
| เครื่องมือสร้างแมพ RO ของชุมชน | มีโปรแกรมอย่าง BrowEdit ที่ใช้สร้างไฟล์ฟอร์แมตนี้ | **[ความรู้ทั่วไป]** ไม่มีในโฟลเดอร์ |

### 1.2 พื้นและกริด (GND / GAT / RSW)

| ไฟล์ | เก็บอะไร | รายละเอียดจากโค้ด |
|---|---|---|
| **GND** (พื้นที่เห็น) | กริดของ "cube" ขนาด `Width x Height` แต่ละช่องมีความสูง 4 มุม (`H0..H3`) และหน้าได้ 3 หน้า: บน (`Up`), ผนังหน้า (`Front`), ผนังขวา (`Right`) แต่ละหน้าอ้าง texture, UV 4 มุม, lightmap index, สี | `src/Formats/MapFormats.cs:17-60` |
| | ขนาดช่องคือ `Zoom` (อ่านจากไฟล์) | `MapFormats.cs:36`, ใช้ใน `MapRenderer.cs:92` |
| | Lightmap อบไว้แล้ว: ต่อชิ้นมี brightness (เงา) + RGB (สีแสง) | `MapFormats.cs:27,40-41` |
| **GAT** (กริดเดินได้) | ละเอียดกว่า GND 2 เท่า (2x2 ช่องต่อ 1 ช่อง GND), ต่อช่องมีความสูง 4 มุม + type | `MapFormats.cs:62-82` |
| | เดินได้เมื่อ type เป็น 0 หรือ 3, มี `Blocked` สำหรับของที่ปิดช่องชั่วคราว (Ice Wall) | `MapFormats.cs:86-94` |
| | `HeightAt` หาความสูงแบบ bilinear (ค่าลบ = สูงขึ้น) | `MapFormats.cs:96-106` |
| | 1 ช่อง GAT = 5 หน่วยโลก (ทุกจุดใช้ `* 5`) | `src/World/Entities.cs:103`, `Game.cs:385` |
| **RSW** (ฉาก) | ชื่อไฟล์ GND/GAT, ระดับน้ำ ชนิดน้ำ คลื่น, ทิศแสงอาทิตย์ (longitude/latitude), diffuse, ambient, ความเข้มเงา, รายการโมเดล (ตำแหน่ง หมุน สเกล) | `MapFormats.cs:109-173` |
| | วัตถุชนิด 2, 3, 4 ถูกข้ามไป (ในฟอร์แมต RO คือแสง เสียง เอฟเฟกต์) | `MapFormats.cs:166-168`, ความหมายของชนิด **[ความรู้ทั่วไป]** |

สรุป: พื้น RO คือ **heightfield บนกริด + ผนังแนวตั้งตามขอบช่อง** ไม่ใช่ mesh อิสระ ทำให้มีหน้าผา ขั้นบันได และเนินได้ แต่ทุกอย่างยังยึดกริด

### 1.3 กล้อง

| ค่า | ตัวเลข | หลักฐาน |
|---|---|---|
| Projection | Perspective | `src/Render/Camera.cs:27` |
| FOV แนวตั้ง | 0.42 rad (ประมาณ 24 องศา) คอมเมนต์บอกว่า RO ใช้ FOV แคบ | `Camera.cs:14` |
| มุมก้ม (pitch) | ค่าเริ่ม 0.80 rad (ประมาณ 46 องศา), ปรับได้ 0.62 ถึง 1.05 rad (ประมาณ 35 ถึง 60 องศา) ด้วย Shift + ลากขวา | `Camera.cs:12`, `Game.cs:40,158` |
| หมุนรอบตัว (yaw) | หมุนได้อิสระด้วยการลากเมาส์ขวา | `Game.cs:155-159` |
| ซูม | ระยะกล้อง 190, ซูมได้ 90 ถึง 420 | `Camera.cs:13`, `Game.cs:164` |
| ติดตาม | เป้าคือเท้าผู้เล่น + 4 หน่วยขึ้นบน แบบ lerp | `Game.cs:177-178` |

FOV 24 องศาแคบมาก ภาพจึงใกล้ orthographic **[อนุมาน]** ข้อนี้สำคัญกับเรา เพราะแปลว่ากล้อง orthographic ที่เอียงลงก็ได้ "ฟีล RO" ส่วนใหญ่แล้ว

### 1.4 ตัวละคร 2D ในโลก 3D

| เรื่อง | วิธีของ RO Offline | หลักฐาน |
|---|---|---|
| Billboard | ทุกเลเยอร์ของเฟรมวาดเป็นสี่เหลี่ยมหันหน้าเข้ากล้อง (ใช้แกน Right/Up ของกล้อง) | `src/Render/SpriteRenderer.cs:75-117` |
| กันจมพื้น | ดึง billboard เข้าหากล้อง 12 หน่วยตามเส้นจากเท้าไปตา โดยขนาดบนจอเท่าเดิม เท้าจึงยังตรงกับเงา | `SpriteRenderer.cs:82-89` |
| ลำดับภายในตัว | depth bias: ตัว 0, อาวุธ 0.15 (หรือ -0.15 ถ้าอยู่หลัง), หัว 0.3 | `SpriteRenderer.cs:73-74`, `Game.cs:784` |
| เรียงหลายตัว | เรียงจากไกลไปใกล้ตามระยะยกกำลังสองถึงตากล้อง แล้ววาด ใช้ depth test + alpha test 0.05 | `Game.cs:641-752` |
| ทิศของสไปรต์ | 8 ทิศ คิดเทียบกับ yaw ของกล้อง | `Camera.cs:75-85` |
| ขนาด | 1 พิกเซลสไปรต์ = 5/35 หน่วยโลก คือ 1 ช่องเดิน = 35 พิกเซลสไปรต์ | `SpriteRenderer.cs:37` |
| ความสูงเท้า | `FootWorld` เอาความสูงจาก GAT มาใส่แกนตั้ง | `src/World/Entities.cs:100-104` |
| เงา | สไปรต์ `data\sprite\shadow` สี่เหลี่ยมแบนวางที่ความสูงเท้า + 0.3, alpha 0.6, ไม่เอียงตามเนิน | `Game.cs:647-671` |

### 1.5 แสง น้ำ โมเดล หมอก เอฟเฟกต์

| เรื่อง | วิธีของ RO Offline | หลักฐาน |
|---|---|---|
| แสงพื้น | vertex shading จากทิศอาทิตย์ (ambient + diffuse x N.L) ด้วย normal ที่เฉลี่ยรอบมุม แล้วคูณ lightmap เงา (`DST_COLOR, ZERO`) และบวก lightmap สี (`ONE, ONE`) | `src/Render/MapRenderer.cs:53-60,105-124,224-251` |
| แสงแบบ realtime | ไม่มี ไม่มี shadow map | อ่านทั้ง `MapRenderer.cs` |
| น้ำ | ระนาบแบนที่ระดับน้ำ เฉพาะช่องที่พื้นต่ำกว่าน้ำ ใช้ texture 32 เฟรมวนเป็นแอนิเมชัน alpha 0.6 ค่าคลื่น (`WaveHeight` ฯลฯ) อ่านมาแต่ไม่ได้ใช้ | `MapRenderer.cs:164-184,253-264` |
| โมเดล 3D | RSM 1.x แบบนิ่ง (ใช้ keyframe หมุนอันแรกเท่านั้น), RSM2 ไม่รองรับ (throw) รวม mesh ตาม texture แล้วแรเงาทีละหน้าจากทิศอาทิตย์ | `src/Formats/Rsm.cs:40,95`, `MapRenderer.cs:186-215` |
| หมอก | ไม่มี ค่าคงที่ GL fog มีอยู่แต่ปิดไว้ | `src/Gl.cs:22`, `PostFx.cs:235` |
| Post-processing | bloom, FXAA, sharpen, saturation, contrast, vignette, depth of field (โฟกัสตัวละคร), motion blur ของกล้อง | `src/Render/PostFx.cs:7-10,244-274` |
| เอฟเฟกต์สกิล | ไฟล์ STR (keyframe ของสี่เหลี่ยม texture) วางที่เท้าเป้าหมายตามความสูงพื้น | `src/Render/Effects.cs:5,170-189` |
| Cursor บนพื้น | แบ่งสี่เหลี่ยมย่อย 3x3 ให้แนบเนิน | `src/Ui/TileCursor.cs` (`GroundQuad`) |
| คลิกพื้น | ยิง ray จากกล้อง เดินทีละ 2 หน่วยจนต่ำกว่าพื้น แล้ว bisection 8 รอบ | `Game.cs:391-415` |

### 1.6 Gameplay ใช้ความสูงไหม: ไม่

- หาทาง: A* 8 ทิศบน GAT, ห้ามตัดมุม, **ไม่มีต้นทุนความสูง** (`src/World/PathFinder.cs:9-48`, เช็กมุมที่บรรทัด 37)
- ระยะ: `DistanceTo` = max(|dx|, |dy|) เป็นจำนวนช่อง (`Entities.cs:106`)
- ตำแหน่งตัวละครเป็นช่องบนกริด ความสูงถูกดึงมาเฉพาะตอนวาด (`FootWorld`)

**ผลต่อเรา**: "แมพแบบ RO" ในเชิง gameplay คือกริด 2D ที่มีช่องเดินได้/ไม่ได้ ซึ่ง Pale Vertica มีอยู่แล้ว ส่วนที่ต่างคือการนำเสนอภาพ

---

## 2. ระบบโลกของ Pale Vertica ตอนนี้

### 2.1 กล้อง

| ค่า | ตัวเลข | หลักฐาน |
|---|---|---|
| Projection | Orthographic, size 3 ในฉาก แต่ `CameraController.Awake` ตั้งเป็น `DefaultZoom` = 6 | `Assets/Scenes/SandBox.unity` (Main Camera: `orthographic: 1`, `orthographic size: 3`), `CameraController.cs:48-49` |
| มุม | ไม่เอียง ไม่หมุน (`m_LocalRotation` identity, z = -10) มองตรงลงระนาบ XY | `SandBox.unity` Main Camera |
| ซูม | 3 ถึง 12 ด้วยลูกล้อ | `CameraController.cs:23-25,91-112` |
| ติดตาม | lerp ตำแหน่ง XY ของเป้า, มี shake, offset, bounds (ปิดอยู่) | `CameraController.cs:117-140`, `UseBounds: 0` ในฉาก |
| Post-processing บนกล้อง | ปิด (`m_RenderPostProcessing: 0`) | `SandBox.unity` |

เอกสาร `GAME_PERSPECTIVE_AND_MAP_DESIGN_ANALYSIS.md` ข้อ 2.1 เขียนว่า "Tilted ~60 to 75 degrees" แต่กล้องจริงไม่เอียง มุม 3/4 ที่เห็นมาจากการวาดอาร์ตเท่านั้น

### 2.2 Render pipeline และแสง

| เรื่อง | ค่า | หลักฐาน |
|---|---|---|
| Pipeline | URP 17.4.0 + **2D Renderer** เป็น renderer เดียว | `Packages/manifest.json`, `ProjectSettings/GraphicsSettings.asset:40` ชี้ `UniversalRP.asset`, `UniversalRP.asset:19-20` ชี้ `Renderer2D.asset` |
| การเรียงโปร่งใส | **Custom Axis (0, 1, 0)** คือ Y-sort ทำงานแล้ว | `Assets/Settings/Renderer2D.asset:33-34` |
| แสง 2D | Light texture scale 0.5, blend style ค่าเริ่ม 4 แบบ | `Renderer2D.asset:36-49` |
| Post-process data | มีผูกไว้ | `Renderer2D.asset:56` |
| URP asset | HDR เปิด, MSAA 1 (ปิด), render scale 1, color grading mode 0 | `UniversalRP.asset:26,28,29,80` |
| Light2D ที่ใช้งานจริง | **ไม่มี** ไม่มีใน SandBox ไม่มีใน prefab ใด และไม่มีสคริปต์อ้าง `Light2D` เจอแค่ใน scene template | ค้น `m_ShapeLightFalloffSize`: พบเฉพาะ `Assets/Settings/Scenes/URP2DSceneTemplate.unity` |
| Volume | ไม่มีใน scene/prefab (มีไฟล์ `Assets/DefaultVolumeProfile.asset` แต่ไม่ได้ตรวจเนื้อหา) | ค้น `m_IsGlobal` ไม่พบ |
| วัสดุตัวละคร | ใน prefab ทุก SpriteRenderer ของ `Skeletom.prefab` ใช้ guid `a97c1056...` = `Sprite-Lit-Default` ของ URP (ตรงกับ `.meta` ใน package) **แต่ตอนรัน** `EntityVisualController.RefreshRenderers` เปลี่ยนเป็น `Character2DLitMaster` ซึ่ง frag ไม่อ่าน light texture ของ 2D เลย ตัวละครจึง **ไม่รับ Light2D ตอน Play** แม้ชื่อจะมีคำว่า Lit | `EntityVisualController.cs:127-149`, `Assets/Materials/Shader/Character2DLitMaster.shader:111-143` |
| Sorting layers | มีแค่ `Default` | `ProjectSettings/TagManager.asset` |
| Physics layers | Player(3), Water(4), UI(5), Enemy(6), Ocstacle(7), UIInteract(8) | `TagManager.asset` |

### 2.3 Physics2D

ทุกการหาเป้าและการชนอยู่บนระนาบ XY:

- Targeting: `OverlapCircleAll` ใน `SphereAOETargeting`, `SingleTargeting`, `ConeTargeting`, `TargetLocationTargeting`, `SingleUnitTargeting`; `OverlapBoxAll` ใน `BoxTargeting`; `CircleCastAll` ใน `LinePierceTargeting` (ทั้งหมดใน `Assets/Script/CoreSystem/Skills/Logic/Targeting/`)
- Projectile: `[RequireComponent(typeof(Rigidbody2D))]`, `OnTriggerEnter2D`, ระเบิดด้วย `OverlapCircleAll` (`Skills/Logic/Projectile/ProjectileBehaviour.cs:12,66`)
- อื่นๆ: `ChainLightningSkill`, การตรวจจับของ AI (`AgentHandle`), การคลิกด้วย `OverlapPoint` ใน `PlayerCore`, `PlayerInteraction`, `UI/CursorManager`
- ตัวละคร: Rigidbody2D แบบ Dynamic, gravity 0, interpolate ปิด, CircleCollider2D รัศมี 0.25 (`Skeletom.prefab`, Player ใน `SandBox.unity`)

### 2.4 การเคลื่อนที่และ PathfinderGrid

| เรื่อง | ที่เห็น | หลักฐาน |
|---|---|---|
| เดิน | `transform.position += direction * speed * dt` ตรงๆ ไม่ผ่าน Rigidbody2D | `CoreSystem/Movement/MovementSystem.cs:31` |
| ใครใช้ pathfinding | เฉพาะ AI ที่ `UsePathfinding` เปิด | `CoreSystem/Agent/AgentHandle.cs:403-408` |
| กริด | ไม่จำกัดขนาด สร้างช่องตอนถูกถาม, ช่องละ 1 หน่วย, เดินได้เมื่อ `OverlapCircle(r 0.4, ObstacleLayers)` ไม่เจออะไร | `Pathfinding/PathfinderGrid.cs:15-23,69-86` |
| ค่าในฉาก | NodeSize 1, ObstacleLayers = bit 128 (layer 7 `Ocstacle`), radius 0.4 | `SandBox.unity` (PathfinderGrid) |
| A* | 8 ทิศ, heuristic Manhattan, จำกัด 5000 รอบ, **ไม่เช็กการตัดมุม** (ต่างจาก RO) | `Pathfinding/Pathfinder2D.cs:52,111-112`, `PathfinderGrid.cs:94-109` |
| อายุของกริด | singleton + `DontDestroyOnLoad`, cache ไม่ถูกล้างเอง | `PathfinderGrid.cs:28-39,44-47` |

**[อนุมาน]** ถ้าเปลี่ยนฉากไปแมพอื่น `PathfinderGrid` ของฉากใหม่จะถูก Destroy และตัวเก่าที่มี cache ของแมพเดิมจะค้างอยู่ ต้องแก้ก่อนมีหลายแมพ (และ Coding Standard ห้าม `DontDestroyOnLoad` อยู่แล้ว)

### 2.5 ฉากและ Tilemap

- `SandBox.unity` **ไม่มีพื้นเลย**: ไม่มี Grid, Tilemap, สไปรต์พื้น SpriteRenderer 7 ตัวในฉากคือชิ้นส่วนผู้เล่น 6 ชิ้น + เงา 1 ชิ้น ส่วน prefab ที่วางคือ Ally, Dummy, RefinementMerchant, HealthLocalStatus
- ไม่มีสคริปต์ไหนใช้ `Tilemap`/`TileBase`
- มี tile palette `Assets/Image/Tiltle/Ground.prefab`: cell 1x1, layout rectangle, 16 tile (Floor, FloorGrass, FloorRock, FloorRockSlime อย่างละ 4) จาก texture 128x128 ที่ PPU 64
- `Assets/Scenes/Stage/` ว่าง

**ความละเอียด texture (texel density) ไม่เท่ากัน**: tile ใช้ 64 px ต่อหน่วย ส่วนตัวละครใช้ PPU 100 คูณสเกล rig 0.234 คือประมาณ 427 px ต่อหน่วย (`Docs/02-Technical/Character/Character_Asset_Standard.md` ข้อ 4) ต่างกันราว 6.7 เท่า ที่ 1080p กับ ortho size 6 จอแสดง 90 px ต่อหน่วย และที่ซูมสุด (size 3) 180 px ต่อหน่วย **[อนุมาน]** พื้น 64 px ต่อหน่วยจะดูเบลอกว่าตัวละครชัดเจน ควรกำหนดมาตรฐานพื้นอย่างน้อย 128 ถึง 180 px ต่อหน่วยก่อนเริ่มวาดฉาก

### 2.6 ตัวละคร

จาก `Character_Asset_Standard.md` และ `Skeletom.prefab`:

- PSB + 2D Animation, 6 ชิ้น (Head, Body, ArmL, ArmR, LegL, LegR) สูงประมาณ 0.87 หน่วย (เล็กกว่า 1 ช่อง tile)
- **2 ทิศ**: วาดหันซ้าย แล้วกลับด้าน `Model` ด้วย `scale.x = -1` ตอนเดินขวา
- `Model` มี SortingGroup (Default, order 6) ชิ้นส่วนเรียงภายในกลุ่ม ตัวละครเรียงกันด้วย Y-sort
- `Model/Shadow` เป็นวงรีดำ alpha 0.25 มี SortingGroup แบบ Sort At Root (order 1) ให้เงาอยู่ใต้ตัวละครทุกตัว และคลิปแอนิเมชันอ้างตำแหน่งนี้

### 2.7 Indicator และ VFX

| ระบบ | วิธีวาง | หลักฐาน |
|---|---|---|
| Circle/Cone/Line indicator | SpriteRenderer บนระนาบ XY, หมุนรอบ Z (`Quaternion.Euler(0, 0, angle - 90)`), สเกลตามเส้นผ่านศูนย์กลาง | `Skills/Visuals/Indicators/CircleIndicator.cs:10-12,32,47`, `ConeIndicator.cs:100`, `LineIndicator.cs:109` |
| Indicator ของ prefab | Instantiate ที่ `TargetPosition` | `SkillIndicatorManager.cs:163` |
| Ground eruption | spawn ที่ `TargetPosition` สเกล X,Y = AOE x 2 | `Skills/Visuals/GroundEruptionVisual.cs:11-17` |
| Projectile | หมุนด้วย `LookRotation(Vector3.forward, aim)` คือหมุนรอบ Z | `Skills/Logic/Projectile/SpawnProjectileEffect.cs` |
| Spell Cascade | เลื่อนตั้งฉากใน XY: `(-d.y, d.x, 0)` | `Skills/Modifiers/SpellCascadeSupportModifier.cs:42-53` |
| UI ตามตัวละคร | `WorldToScreenPoint` ใช้ได้กับกล้องทุกแบบ | `UI/DamageTextBehaviour.cs:75`, `UI/UIFollowTarget.cs:76` |
| เมาส์เป็นตำแหน่งโลก | `ScreenToWorldPoint` แล้วตั้ง z = 0 | `Player/PlayerCore.cs:215`, `UI/CursorManager.cs:174`, `Skills/SkillCaster.cs:707` (เฉพาะ gizmo ใน editor) |

### 2.8 เอกสารเดิมกับของจริง

`GAME_PERSPECTIVE_AND_MAP_DESIGN_ANALYSIS.md` เป็นเป้าหมายการออกแบบ ไม่ใช่สถานะปัจจุบัน:

| เอกสารเดิมบอก | ของจริง |
|---|---|
| กล้องเอียง 60 ถึง 75 องศา | ไม่เอียง (rotation identity) |
| dynamic 2D lighting, normal-mapped cliff | ไม่มี Light2D ที่ใช้งาน ไม่มี normal map |
| soft drop shadow ด้วย `LeTai.TrueShadow` | TrueShadow อยู่ใน `Assets/ThirdParty` แต่สคริปต์ใน `Assets/Script` ไม่ได้ใช้ มีแค่ตัวสร้าง UI อ้างภาพ `Circle.png` จากโฟลเดอร์ demo ของมัน (`Editor/UI/RefinementAndSocketPrefabBuilder.cs:57`) ตัว asset เป็นเงาของ uGUI **[ความรู้ทั่วไป]** ไม่ใช่เงาในโลก |
| `WorldMediator` สร้าง actor จาก JSON | ไม่มีคลาสนี้ใน `Assets/Script` |
| ลำดับเลเยอร์จาก `map_system_prototype.html` | ไฟล์นี้เป็นหน้าเว็บ HTML ที่ root ของ repo (1,105 บรรทัด) ไม่ใช่ของใน Unity |
| Tilemap หลายเลเยอร์ + elevation | ยังไม่มี Tilemap ในฉากใด |

---

## 3. คำตอบ: ยากแค่ไหน และต้องทำอะไร

### 3.1 เทียบทีละระบบ

| ระบบ | RO Offline | Pale Vertica | ช่องว่าง |
|---|---|---|---|
| กริด gameplay | GAT 2D, ความสูงไม่มีผล | PathfinderGrid 2D บน XY | **แทบไม่มี** |
| การชน/หาเป้า | ช่องและระยะ Chebyshev | Physics2D บน XY | ไม่มี ถ้าความสูงเป็นแค่ภาพ |
| พื้นที่เห็น | heightfield + ผนัง + lightmap | ยังไม่มีพื้น | ใหญ่ (ต้องสร้างทั้งระบบและอาร์ต) |
| กล้อง | perspective FOV 24, ก้ม 46, หมุนได้ | ortho ไม่เอียง | กลาง |
| ตัวละคร | billboard 8 ทิศ | PSB 2 ทิศ วางบน XY | เล็ก ถ้ากล้องไม่หมุน / ใหญ่ ถ้าหมุน |
| เงาตัวละคร | blob แบนบนพื้น | blob ใน Model/Shadow | เล็ก |
| คลิกพื้น | ray กับ heightfield | ScreenToWorldPoint | เล็ก |
| Indicator | วาดแนบเนิน | sprite บน XY | ไม่มี ถ้าพื้นราบ / กลาง ถ้าไม่ราบ |
| แสง | lightmap อบ + vertex sun | ไม่มีแสง (Sprite-Lit พร้อมรับ) | กลาง |
| โมเดล 3D | RSM 1.x | ไม่มี pipeline | กลางถึงใหญ่ (ส่วนใหญ่เป็นอาร์ต) |
| น้ำ | ระนาบ texture เคลื่อนไหว | ไม่มี | เล็ก |
| Post | bloom, DoF, grade, vignette | pipeline รองรับ แต่ปิดอยู่ | เล็ก |
| เครื่องมือทำแมพ | ไม่มี (ใช้แมพของ Gravity) | ไม่มี | ใหญ่ เท่ากันทั้งคู่ |

### 3.2 สิ่งที่ต้องเปลี่ยนหรือเพิ่ม

ขนาดเป็นการเดาคร่าวๆ สำหรับนักพัฒนา 1 คน **[อนุมาน]**: S = 1 ถึง 3 วัน, M = 1 ถึง 2 สัปดาห์, L = 3 สัปดาห์ขึ้นไป

**สำหรับทาง B (HD-2D ไฮบริด, gameplay อยู่บน XY, ความสูงเป็นแค่ภาพบนแกน Z)**

| # | ระบบ | ไฟล์ / พื้นที่ | ต้องทำ | ขนาด | ความเสี่ยง |
|---|---|---|---|---|---|
| 1 | กล้อง | `CoreSystem/Controllers/CameraController.cs` | เพิ่มมุมก้มคงที่ (หมุนรอบแกน X), เลือก ortho เอียงหรือ perspective FOV แคบ, ติดตามบน XY, bounds | S | ortho size เดิมไม่บอกระยะที่เห็นบนพื้นแล้ว ต้องจูนซูมใหม่ |
| 2 | เมาส์เป็นจุดบนพื้น | `Player/PlayerCore.cs:215`, `UI/CursorManager.cs:174`, `Skills/SkillCaster.cs:707` | แทน `ScreenToWorldPoint` ด้วย ray ตัดระนาบ z = 0 (หรือตัดความสูงพื้น) รวมไว้ที่เดียว | S | ถ้าพลาดจุดใดจุดหนึ่ง การเล็งสกิลจะเพี้ยน |
| 3 | คลิกเลือกตัวละคร | `OverlapPoint` ใน `PlayerCore`, `Player/PlayerInteraction.cs`, `UI/CursorManager.cs` | ภาพตัวละครตั้งขึ้นจากพื้น หัวไม่ได้อยู่เหนือ collider บนพื้น ต้องเลือกด้วยกรอบบนจอแบบ `ScreenBounds` ของ RO หรือ collider แยกสำหรับคลิก | S ถึง M | เลือกผิดตัวเมื่อยืนซ้อน |
| 4 | ตัวละครหันเข้ากล้อง | prefab ฐาน (`Skeletom.prefab`) ส่วน `Model` และ Character Asset tool | หมุน `Model` ตามมุมกล้องครั้งเดียว (กล้องไม่หมุน จึงไม่ต้องทำทุกเฟรม) แต่ `Shadow` ต้องยังนอนบนพื้น | S (โค้ด) + แก้ prefab ซึ่งต้องได้รับอนุญาตก่อน | คลิปแอนิเมชันอ้าง `Model/Shadow` ถ้าคลิปคีย์ rotation จะทับค่า ต้องตรวจคลิปก่อน |
| 5 | ความสูงเชิงภาพ | ใหม่: ตัวอ่านความสูงพื้น + component ตั้ง z ของ `Model` | แบบ `FootWorld` ของ RO | S (พื้นราบ) / M (heightmap) | ต้องทำตอนตำแหน่งเปลี่ยน ไม่ poll ของที่ไม่ขยับ (AGENTS ข้อ 6) |
| 6 | Indicator บนพื้นไม่ราบ | `Skills/Visuals/Indicators/*` | พื้นราบ: ใช้ได้เลยเพราะอยู่บน XY / พื้นไม่ราบ: ฉายลงพื้นด้วย shader หรือ decal | 0 / M | 2D Renderer ไม่มี Decal Projector **[ความรู้ทั่วไป]** |
| 7 | VFX | `Skills/Visuals/*Visual.cs` + prefab VFX | แยกส่วนที่นอนพื้น (วง, รอยแตก) กับส่วนที่ตั้ง (เสา, ระเบิด) ส่วนที่ตั้งต้องหันกล้อง Particle แบบ Billboard หันกล้องเองอยู่แล้ว **[ความรู้ทั่วไป]** | M (งานคอนเทนต์) | VFX ที่วาดสำหรับมุมตรงจะดูแบนติดพื้น |
| 8 | Projectile | `ProjectileBehaviour.cs`, `ProjectileVisual.cs` | collider อยู่ XY ต่อไป ยกเฉพาะภาพขึ้นแกน -Z | S | ไม่มากนัก |
| 9 | Sorting layers | `ProjectSettings/TagManager.asset` | เพิ่ม Ground / Objects / Foreground; Custom Axis Y ใช้ต่อได้ เพราะกล้องเอียงตายตัว ของที่ Y มากกว่าก็อยู่ไกลกว่าเสมอ **[อนุมาน]** | S | แก้ Project Settings ต้องขออนุญาต |
| 10 | พื้นและพร็อพ 3D | ใหม่: pipeline Blender เป็น FBX หรือ ProBuilder (ยังไม่ได้ติดตั้ง) หรือตัวสร้าง mesh จาก heightmap | Unity Terrain ใช้ไม่ได้ในแนวทางนี้ เพราะ Terrain ตั้งฉากกับแกน Y และไม่รองรับการหมุน **[ความรู้ทั่วไป]** | M ถึง L | งานอาร์ตเป็นก้อนใหญ่สุดของทั้งแผน |
| 11 | แสง | `Renderer2D.asset` หรือเปลี่ยนเป็น Universal Renderer | ต้นแบบใน 2D Renderer: Light2D + วัสดุแบบ Sprite-Lit สำหรับ mesh ถ้าต้องการเงาจริง หมอก decal ค่อยพิจารณาเปลี่ยน renderer | M / L | mesh 3D รับ Light2D ได้ดีแค่ไหน และ Light2D กับกล้องเอียง ยังไม่ได้ทดสอบ **[ต้องตรวจใน Unity]** |
| 12 | Pathfinding | `PathfinderGrid.cs`, `Pathfinder2D.cs` | ใช้ต่อได้ (หน้าผาคือ collider ใน layer `Ocstacle`) แก้ 2 เรื่อง: กริดค้างข้ามฉาก (`DontDestroyOnLoad`) และการตัดมุม | S | ข้ามแมพแล้ว AI เดินตามกริดของแมพเก่า |
| 13 | เดินชนกำแพง | `Movement/MovementSystem.cs:31` | ขยับ transform ตรงๆ บน Rigidbody2D แบบ Dynamic ให้ฟิสิกส์ดันออกทีหลัง | S ถึง M | อาจทะลุกำแพงบางหรือสั่นที่มุม **[อนุมาน ต้องทดสอบ]** |
| 14 | ข้อมูลแมพ การโหลด warp spawn | ยังไม่มี | ต้องมีถ้าจะมีหลายแมพ ไม่ว่าทางไหน | M | ไม่เกี่ยวกับ 3D โดยตรง |

**เพิ่มเติมเฉพาะทาง C (RO เต็มรูปแบบ)**

| # | ระบบ | ต้องทำ | ขนาด | ความเสี่ยง |
|---|---|---|---|---|
| C1 | กล้องหมุนได้ | ตัวละครต้องมี 8 ทิศ แต่ rig ตอนนี้มี 2 ทิศ หรือเปลี่ยนตัวละครเป็น 3D | L (อาร์ต) | งานอาร์ตตัวละครเพิ่มหลายเท่า |
| C2 | ความสูงมีผลต่อ gameplay | ย้าย query ราว 16 จุดและ `OnTriggerEnter2D` (หัวข้อ 2.3) จาก Physics2D ไป Physics 3D หรือเขียน query เอง, projectile ข้ามเนิน, ระยะคิดความสูง | L | แตะระบบต่อสู้ทั้งระบบ RO เองก็ไม่ได้ทำ |
| C3 | Pathfinding รู้ความสูง | ต้นทุนความชัน ขั้นบันได หลายชั้น | M | |
| C4 | เครื่องมือและฟอร์แมตแมพของเรา | แบบ GND/GAT/RSW: editor ทาสี texture ความสูง ผนัง ช่องเดินได้ วางโมเดล | L | ต้องมี walkthrough ตาม AGENTS ข้อ 7 |
| C5 | Indicator ฉายลงพื้นทุกแบบ | decal หรือ shader | M | |

### 3.3 สิ่งที่ใช้ต่อได้เลย (ทาง A และ B)

- `CombatCore` และสูตรทั้งหมด (ไม่เกี่ยวกับมุมกล้อง)
- Targeting และ Projectile ที่ใช้ Physics2D ทั้งหมด เพราะระนาบ gameplay ยังเป็น XY
- `PathfinderGrid` / `Pathfinder2D` (แก้เล็กน้อยตามข้อ 12)
- Y-sort แบบ Custom Axis และ SortingGroup ต่อตัวละคร
- rig PSB + 2D Animation (เปลี่ยนแค่การหมุนของ `Model`)
- Indicator แบบ sprite ทั้งหมด บนพื้นราบ
- ข้อมูลตำแหน่งใน `SkillPayload` (z = 0) รวมถึง Spell Cascade
- UI ที่ตามตัวละครด้วย `WorldToScreenPoint`
- วัสดุ Sprite-Lit-Default บนตัวละคร (พร้อมรับ Light2D)

### 3.4 สามทิศทาง

| | A: 2D ล้วน หลอกความลึก | B: HD-2D ไฮบริด | C: 3D แบบ RO เต็มรูปแบบ |
|---|---|---|---|
| ภาพ | ortho ตรงเหมือนเดิม, Tilemap/ภาพวาด, แสง 2D, เงา, หมอก, parallax | พื้นและพร็อพ 3D, ตัวละคร 2D หันกล้อง, กล้องเอียงตายตัว | พื้น heightfield, กล้องหมุนได้ |
| Gameplay | XY เหมือนเดิม | XY เหมือนเดิม, ความสูงเป็นภาพ (แกน Z) | ความสูงมีผล |
| แก้โค้ด | แทบไม่มี (ข้อ 12 ถึง 14) | ข้อ 1 ถึง 14 ส่วนใหญ่ขนาด S | ทั้งหมด + C1 ถึง C5 |
| งานอาร์ต | สไปรต์ฉาก, ทางเลือก A2: โมเดลใน Blender แล้ว render เป็นภาพ (pre-render) | โมเดล 3D low-poly + texture มุมบนลงล่าง | เท่ากับ B + ตัวละคร 8 ทิศ |
| ความยาก | ต่ำถึงกลาง | กลางถึงสูง | สูงมาก |
| ข้อดี | เสี่ยงต่ำสุด ใช้ pipeline เดิมได้หมด | ได้ความลึกจริง parallax และแสงตามความสูง ตรงเสา "HD-2D" ใน AGENTS | เหมือน RO ที่สุด |
| ข้อเสีย | ไม่มี parallax จริงตอนกล้องขยับ, หลายชั้นความสูงทำยากใน 2D, อาร์ตต้องวาดทีละชิ้น | ต้องมีคนทำ 3D, Unity Terrain ใช้ไม่ได้, อาร์ต 3/4 เดิมต้องวาดใหม่เป็นมุมบนลงล่าง, ไม่มี decal ใน 2D Renderer | แตะระบบต่อสู้ทั้งหมด, อาร์ตตัวละครเพิ่มหลายเท่า, ทำมากกว่าที่ RO ทำ |

### 3.5 คำแนะนำ

เลือก **B แบบจำกัดขอบเขต** แต่เข้าทางเฟส:

1. **กล้องไม่หมุน**: ตัวละครเรามี 2 ทิศ การหมุนกล้องแบบ RO จะบังคับให้ทำ 8 ทิศ
2. **Orthographic เอียงก่อน**: RO ใช้ FOV แค่ 24 องศา ภาพใกล้ ortho อยู่แล้ว ortho ยังทำให้ระยะการต่อสู้บนจอคงที่
3. **พื้นที่เดินได้ราบ** ความสูงเป็นของตกแต่ง หน้าผาเป็นพื้นที่เดินไม่ได้ (collider ใน `Ocstacle`) ทำให้ indicator และ targeting ไม่ต้องแก้
4. **ทำต้นแบบใน 2D Renderer ก่อน** ไม่ต้องแก้ pipeline และ Y-sort ใช้ต่อได้ ค่อยตัดสินใจเปลี่ยนเป็น Universal Renderer เมื่อพิสูจน์ได้ว่าต้องการเงา realtime หมอกตามความลึก หรือ decal
5. **ถ้าทีมไม่มีคนทำ 3D** ให้ไปทาง A2 (pre-render จาก 3D เป็นภาพ 2D) ได้หน้าตาใกล้เคียงโดยไม่ต้องแก้กล้อง

เหตุผลหลัก: gameplay ของเราเป็นกริด 2D เหมือน RO อยู่แล้ว ส่วนที่ต้องแก้จริงเป็นงาน S หลายชิ้นรอบกล้องกับการคลิก ไม่ใช่การรื้อระบบ ความเสี่ยงใหญ่ที่เหลือคือต้นทุนอาร์ตและผลลัพธ์ของแสงใน 2D Renderer ซึ่งต้นแบบ 1 ฉากตอบได้

### 3.6 แผนเป็นเฟส (ถูกไปแพง)

| เฟส | ทำอะไร | ขนาด | ผลที่ได้ |
|---|---|---|---|
| 0: ตัดสินใจ | เลือกมุมกล้อง, กำหนด texel density ของฉาก (แนะนำ 128 ถึง 180 px ต่อหน่วย), ยืนยันว่ามีคนทำ 3D หรือไม่, ทำภาพ mock 1 ภาพ | S | ทิศทางที่ทีมตกลง |
| 1: พื้นฐานที่ใช้ได้ทุกทาง | ฉากทดสอบใหม่ (ไม่แตะ SandBox) ด้วย Tilemap พื้น, พร็อพ pivot ที่ฐาน, sorting layers, เงา blob + contact shadow, Global Light2D + ไฟจุด, Volume (bloom, color, vignette), หมอก particle, ของบังหน้าแบบจาง แก้ข้อ 12 (กริดข้ามฉาก, ตัดมุม) | S ถึง M | ฉาก 2D ที่ไม่แบน ซึ่งเป็นทาง A อยู่แล้ว |
| 2: ต้นแบบ B (1 ฉาก) | กล้อง ortho เอียง, ray ลงพื้น, `Model` หันกล้อง (ทำใน prefab สำเนา), พื้น mesh ราบ + พร็อพ 3D 3 ถึง 5 ชิ้น, ลอง indicator และ VFX ของจริง | M | ข้อมูลพอตัดสิน go/no-go |
| 3: B ใช้งานจริง | ความสูงเชิงภาพจาก heightmap, VFX audit, pipeline คอนเทนต์ฉาก, ข้อมูลแมพ การโหลด warp spawn, walkthrough ของเครื่องมือ | L | แมพแรกที่เล่นได้ |
| 4: ถ้าจำเป็น | เปลี่ยนเป็น Universal Renderer เพื่อเงาจริง หมอก DoF decal หรือฟีเจอร์ทาง C | L | หน้าตาใกล้ Octopath / RO มากขึ้น |

**สถานะเฟส 1 (2026-10-23)**: เริ่มแล้วในฉาก `Assets/Scenes/MapLab/MapLab.unity` ตาม [Map_Lab_Walkthrough_TH.md](../02-Technical/World/Map_Lab_Walkthrough_TH.md) ต่างจากแผนเดิม 2 ข้อตามที่ขอเพิ่ม:
- พื้นใช้ **SpriteShape (เส้นโค้งจริง)** แทน Tilemap: ทางเดิน ทางแยก ลาน หน้าผา ไม่ยึดช่องสี่เหลี่ยม
- การหาทางเป็น **สวิตช์ต่อแมพ** (`NavigationManager`: Auto / Grid / NavMesh) ดู [Navigation_Switch_Walkthrough_TH.md](../02-Technical/World/Navigation_Switch_Walkthrough_TH.md)
- ยังไม่ได้เพิ่ม sorting layers (ต้องแก้ Project Settings) และยังไม่ได้ใส่ contact shadow ให้ prefab ตัวละคร (ต้องแก้ prefab) จึงใช้ order ใน layer `Default` ไปก่อน
- วันเดียวกัน งานเฟส 1 ถูกแยกเป็นโมดูล standalone `Assets/ThirdParty/MapLab/` (asmdef ของตัวเอง ไม่อ้างโค้ดเกม ตั้งใจเอาไปต่อยอดและขายแยก) ฉาก demo ย้ายไปที่ `Assets/ThirdParty/MapLab/Demo/MapLab_Demo.unity` และ AI ของเกมกลับไปใช้กริดเดิมจนกว่าจะตัดสินใจเรื่องการเชื่อม
- ผลลัพธ์ของ MapLab คือไฟล์พื้นที่เดินได้ที่ bake ไว้ (JSON กริด + โพลิกอน และภาพ mask) ตาม [Walkable_Area_Bake_Walkthrough_TH.md](../02-Technical/World/Walkable_Area_Bake_Walkthrough_TH.md) ใครจะหาทางแบบไหนก็อ่านไฟล์นี้ NavMesh กลายเป็นตัวอย่างผู้ใช้ข้อมูลใน `Samples/` ไม่ใช่ส่วนของแกน

**เกณฑ์ go/no-go หลังเฟส 2**

1. ตัวละครเดินอ้อมหน้า/หลังพร็อพ 3D แล้วลำดับการวาดถูกทุกมุม
2. แสงบน mesh กับบนสไปรต์ดูเป็นโลกเดียวกัน
3. เล็งสกิลและคลิกเลือกตัวละครแม่นเท่าเดิม
4. เวลาทำพร็อพต่อชิ้นและต่อแมพรับได้
5. เฟรมเรตไม่ตกจากเดิมอย่างมีนัย

ไม่ผ่านข้อ 1 ถึง 3: ถอยไปทาง A โดยเก็บงานเฟส 1 ไว้ทั้งหมด ไม่ผ่านข้อ 4: ไปทาง A2

---

## 4. ทำแมพ 2D ไม่ให้แบน (ถูกไปแพง)

| # | เทคนิค | ทำอย่างไร | ขนาด | เข้ากับ pipeline เราไหม |
|---|---|---|---|---|
| 1 | Color grading + vignette + bloom | ใส่ Global Volume, เปิด Post Processing บนกล้อง (ตอนนี้ปิด) | S | เข้า: `Renderer2D.asset` มี post-process data แล้ว ถ้าต้องการ grade แบบ HDR ต้องเปลี่ยน color grading mode ใน `UniversalRP.asset` |
| 2 | เงาตกและเงาสัมผัส | มี blob เงาแล้ว (alpha 0.25) เพิ่มวงรีเล็กเข้มกว่าใต้เท้าเป็น contact shadow, พร็อพมีเงาวาดในภาพ | S | เข้า: ใช้ `Model/Shadow` ที่มีอยู่ (ถ้าแก้ prefab ต้องขออนุญาต) |
| 3 | Y-sort + pivot ที่ฐาน | Y-sort เปิดแล้ว เพิ่ม sorting layers และกฎ pivot ของพร็อพ (pivot ที่จุดแตะพื้น) | S | เข้าเต็มที่ |
| 4 | ของบังหน้า (foreground occluder) | หลังคา ต้นไม้ ซุ้ม จางลงตอนผู้เล่นเดินเข้าใต้ ใช้ trigger enter/exit ไม่ poll ใน `Update` | S ถึง M | เข้า |
| 5 | หมอกและ particle บรรยากาศ | สไปรต์หมอกเลื่อนช้า, ฝุ่น, ใบไม้, หิ่งห้อย | S | เข้า: texture หมอกเป็น grayscale แล้วย้อมสีใน Unity ตามกฎ VFX ของโปรเจกต์ |
| 6 | Parallax หลายชั้น | ชั้นเหวหรือฉากหลังเลื่อนช้ากว่ากล้อง, ชั้นบังหน้าเลื่อนเร็วกว่า | S ถึง M | เข้า: กล้อง ortho ต้องใช้สคริปต์ (กล้อง perspective ได้เอง) |
| 7 | Pre-render จาก 3D (A2) | ปั้นฉากใน Blender, render มุมตายตัวเป็นภาพชิ้นใหญ่ (และ normal pass ถ้าต้องการ) | M (อาร์ต) | เข้า: ยังเป็น 2D ล้วน |
| 8 | Normal map + แสง 2D | Light2D + Secondary Texture `_NormalMap` บนสไปรต์, ShadowCaster2D | M | เข้า: ตัวละครใช้ Sprite-Lit-Default แล้ว แต่ต้องทำ normal map ให้ PSB และ tile ทุกชิ้น, light texture scale 0.5 ทำให้ขอบแสงนุ่ม |
| 9 | หลายชั้นความสูง (elevation) | collider และ sorting แยกต่อชั้น, ทางลาดสลับชั้น, กริดหาทางต่อชั้น | M ถึง L | เข้าแต่ยุ่ง: `PathfinderGrid` ต้องรู้จักชั้น |
| 10 | Depth of field | สไปรต์ไม่เขียน depth ใน 2D Renderer DoF ของ URP จึงอาจไม่ได้ผลตามคาด ทางเลือกคือเบลอเฉพาะเลเยอร์ฉากหลัง | M | **[ต้องตรวจใน Unity]** |
| 11 | กล้องเอียง / perspective แบบ HD-2D | ทาง B ทั้งหมด | L | ดูหัวข้อ 3 |

ข้อ 1 ถึง 6 ทำได้ทันทีในระบบปัจจุบันโดยไม่แตะโค้ดเกม (นอกจากตัวจางของข้อ 4) และทั้งหมดใช้ต่อได้ถ้าภายหลังไปทาง B

---

## 5. สิ่งที่ต้องให้ตัดสินใจ และสิ่งที่ยังไม่ได้ตรวจ

**ต้องตัดสินใจ**

1. ยอมให้กล้องไม่หมุนได้ไหม (ถ้าต้องหมุน ต้องรับงานตัวละคร 8 ทิศ)
2. มีคนทำโมเดล 3D หรือจะใช้ asset pack ไหม (ต้องดูไลเซนส์)
3. อนุญาตให้สร้างฉากทดสอบใหม่, เพิ่ม sorting layers, และทำ prefab ตัวละครสำเนาสำหรับต้นแบบไหม

**ยังไม่ได้ตรวจ (ต้องลองใน Unity)**

- Light2D ทำงานถูกต้องกับกล้องเอียงหรือกล้อง perspective แค่ไหน
- MeshRenderer ใน 2D Renderer: ตั้ง sorting layer และรับ Light2D ได้ด้วยวัสดุแบบไหน
- Sprite-Lit-Default ใน Universal Renderer แสดงผลแบบไม่รับแสง 3D และ `SpriteRenderer.shadowCastingMode` ตั้งผ่านสคริปต์ให้สไปรต์ทอดเงาได้ (ใช้เมื่อถึงเฟส 4)
- คลิปแอนิเมชันตัวละครคีย์ rotation ของ `Model/Shadow` หรือไม่
- การเดินแบบขยับ transform ตรงๆ ทะลุ collider บางหรือไม่
- เนื้อหาใน `Assets/DefaultVolumeProfile.asset`

---

## 6. ไฟล์ที่ใช้เป็นหลักฐาน

**Ragnarok Offline** (`D:\Application Project\Ragnarok Offline`, อ่านอย่างเดียว ไม่ได้คัดลอกอะไร)

- `README.md`, `AGENTS.md`, `RagnarokOffline.csproj`
- `src/Formats/MapFormats.cs`, `src/Formats/Rsm.cs`
- `src/Render/Camera.cs`, `src/Render/MapRenderer.cs`, `src/Render/SpriteRenderer.cs`, `src/Render/Effects.cs`, `src/Render/PostFx.cs`
- `src/Game.cs`, `src/Ui/TileCursor.cs`, `src/World/PathFinder.cs`, `src/World/Entities.cs`, `src/World/GameWorld.cs`

**Pale Vertica**

- `AGENTS.md`, `.claude/skills/pale-vertica-dev/SKILL.md`, `Docs/01-Design/GAME_PERSPECTIVE_AND_MAP_DESIGN_ANALYSIS.md`, `Docs/02-Technical/Character/Character_Asset_Standard.md`
- `Packages/manifest.json`, `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/TagManager.asset`, `ProjectSettings/QualitySettings.asset`
- `Assets/Settings/UniversalRP.asset`, `Assets/Settings/Renderer2D.asset`
- `Assets/Scenes/SandBox.unity`, `Assets/Image/Tiltle/Ground.prefab`, `Assets/Image/FloorGrass.png.meta`, `Assets/Addressable/Prefab/Entity/Enemy/Skeletom.prefab`
- `Assets/Script/CoreSystem/Controllers/CameraController.cs`, `Movement/MovementSystem.cs`, `Pathfinding/PathfinderGrid.cs`, `Pathfinding/Pathfinder2D.cs`, `Agent/AgentHandle.cs`, `Player/PlayerCore.cs`, `Skills/SkillCaster.cs`, `Skills/Logic/Targeting/*`, `Skills/Logic/Projectile/*`, `Skills/Visuals/*`, `Skills/Modifiers/SpellCascadeSupportModifier.cs`, `UI/CursorManager.cs`
