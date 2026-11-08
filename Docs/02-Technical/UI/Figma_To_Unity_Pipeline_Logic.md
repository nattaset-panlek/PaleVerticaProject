# Figma to Unity UGUI: Pipeline Logic & Agent Implementation Guide

> **เอกสารคู่มือเชิงเทคนิค (Technical Specification & Pipeline Logic)**  
> **หมวดหมู่**: UI / Tooling / Autonomous Agent Workflow  
> **เป้าหมาย**: อธิบายขั้นตอนการทำงานอย่างเป็นระบบ (Step-by-Step Logic) ในการแปลง UI จาก Figma สู่ Unity UGUI Prefab ในระดับความแม่นยำทางคณิตศาสตร์และสายตา (Pixel-Perfect Fidelity > 93-95%) โดยไม่ใช้การเดาสุ่ม

---

## 1. ภาพรวมสถาปัตยกรรม (Architectural Overview)

การแปลง UI จาก Figma เป็น Unity UGUI ไม่ใช่งานจัดเลย์เอาต์ตามความรู้สึก แต่เป็น **กระบวนการทางวิศวกรรมข้อมูลแบบวงรอบปิด (Closed-Loop Engineering Pipeline)** ซึ่งประกอบด้วย 8 ขั้นตอนหลัก:

```
[1. Figma Extraction] ---> [2. Asset Consolidation] ---> [3. Coordinate Mapping]
         │                            │                           │
         ▼                            ▼                           ▼
[6. Batchmode Snapshot] <-- [5. Prefab Synthesis]   <-- [4. Typography Setup]
         │
         ▼
[7. OpenCV Analysis]   ---> [8. Subpixel Fine-Tuning] ---> (Loop until >= 93-95%)
```

---

## 2. รายละเอียดขั้นตอนการทำงานของ Agent (Step-by-Step Logic)

### ขั้นตอนที่ 1: การเชื่อมต่อและดึงข้อมูลโครงสร้าง (Figma Node Extraction)
* **เป้าหมาย**: ดึงข้อมูล Raw JSON ของ Node ที่ต้องการจาก Figma (ผ่าน TalkToFigma MCP หรือ Figma REST API) พร้อมเรนเดอร์ภาพ Ground Truth Preview ขนาด 1920x1080 px
* **Logic การทำงานของ Agent**:
  1. ระบุ Node ID ของหน้าต่างหลัก (Root Window Frame เช่น `2170:1681` สำหรับ Socket Window หรือ `1786:24436` สำหรับ Refinement Window)
  2. ดึงค่า Bounding Box พิกัดสัมบูรณ์ของ Modal Frame:
     - `modalBox.x`, `modalBox.y`, `modalBox.width`, `modalBox.height`
  3. กวาดข้อมูล Children ในโครงสร้างแบบ Recursive เพื่อดึง Properties:
     - **Geometry**: `absoluteBoundingBox`, `cornerRadius`
     - **Fills / Strokes**: ชนิดของสี, Hex Color, Opacity, Linear/Radial Gradient
     - **Typography**: `characters`, `fontFamily`, `fontWeight`, `fontSize`, `textAlignHorizontal`, `letterSpacing`, `lineHeightPx`
  4. **การคัดกรอง Phantom Nodes**: ระวังโหนดที่ถูกซ่อน (Hidden) หรือโหนดที่ถูก Mask ไว้แต่ยังมีข้อมูลใน JSON ต้องตรวจสอบเปรียบเทียบกับภาพเรนเดอร์จริงเสมอ

---

### ขั้นตอนที่ 2: การตรวจสอบและรวมทรัพยากรภาพ (Asset Consolidation Strategy)
* **เป้าหมาย**: ป้องกันการส่งออกไฟล์ภาพซ้ำซ้อน รักษาระดับความคมชัดของภาพ และคงความเป็น Dynamic UI Component
* **Logic การทำงานของ Agent**:
  1. **กฎการใช้ซ้ำระดับโปรเจกต์ (Zero-Export for Primitives)**:
     - ไม่ส่งออกไฟล์ PNG สี่เหลี่ยมมุมโค้งใหม่เด็ดขาด หากในโปรเจกต์มี Standard Sprite อยู่แล้ว
     - ใช้ `Assets/Art/UI/Old/rounded.png` (GUID: `9df5b6060bad4fb4b9b050483f64c324`, 9-Slice Border: `20, 20, 20, 20`) เป็นแกนกลางสำหรับกรอบ Header, Tag, Badge และ Stat Row
     - ใช้ `Assets/Art/UI/Old/Circle.png` สำหรับวงกลมพื้นหลัง
  2. **มาตรฐานสีขาวบริสุทธิ์ (Pure White Dynamic Tinting)**:
     - ห้ามฝังสี (เช่น สีฟ้า `#0048ff` หรือสีน้ำตาล) ลงในเนื้อไฟล์ 9-Slice PNG เพราะเมื่อนำไปใส่สีใน Unity `Image.color` จะเกิดการคูณสีซ้ำซ้อน (Double-Tinting Bug) ทำให้สีมืดลง
     - คงเนื้อภาพฐานให้เป็นสีขาว (`#ffffff`) 100% แล้วกำหนดสีผ่านสคริปต์ C# หรือ Inspector
  3. **การแยกส่วนประกอบปุ่มหลัก (Button Decoupling)**:
     - ห้ามบันทึกปุ่ม Action Button เป็นภาพแบนก้อนเดียว (Monolithic Flattened Image)
     - ใช้เทคนิค Inpainting ลบข้อความออกจากปุ่ม ให้เหลือเฉพาะพื้นหลังและเส้นลายวงจร (`btn_primary_clean_290x50.png`)
     - แยกข้อความปุ่มออกมาเป็น Child GameObject ที่มีคอมโพเนนต์ `TextMeshProUGUI` แยกต่างหาก เพื่อให้เปลี่ยนข้อความและฟอนต์ได้แบบไดนามิก
  4. **การส่งออกรูปภาพตามมาตราส่วนจริง (1:1 Export Scale Standard)**:
     - ใช้มาตราส่วน **1:1** เท่านั้น ห้ามคิดแทนหรืออัปสเกล (เช่น 2x / 4x) โดยพลการถ้าไม่ได้สั่งโดยเด็ดขาด เพื่อป้องกันความคลาดเคลื่อนของขนาดและพิกัดเลย์เอาต์
  5. **กับดัก Transparent Padding และขนาดรูปภาพผิดสัดส่วน (Shrinkage Trap)**:
     - *ปัญหาที่พบ (Case Mandala Dial)*: ไฟล์รูปภาพที่ดาวน์โหลดมามักมีพื้นที่ว่างโปร่งใส (Transparent Margin / Padding) ครอบรอบนอกตัวกราฟิก แต่ในการวิเคราะห์ Bounding Box ของ Figma สคริปต์ไปดึงขนาดเฉพาะโหนดเวกเตอร์ลูกด้านใน (เช่น Bounding Box แนบชิด 460x460) เมื่อนำรูปภาพที่มีช่องว่างรอบนอกไปใส่ใน `Image` ของ Unity ที่เปิด `preserveAspect = true` ขนาดของลวดลายจริงจะถูกบีบย่อลงจนเล็กกว่าขนาดที่ควรจะเป็นใน Figma อย่างเห็นได้ชัด
     - *แนวทางแก้ไขเชิงระบบ*:
       - ตรวจสอบ Bounding Box ของไฟล์ภาพจริง (Non-transparent Alpha Bounds) เสมอ
       - หาก Export มาพร้อมกรอบว่าง ต้องตั้งขนาด RectTransform ให้เท่ากับขนาดของ Exported Slice Frame จริงๆ หรือทำ Tight Trim ตัดพื้นที่ว่างรอบนอกออกก่อนนำเข้า Unity เพื่อให้ Bounding Box ในสคริปต์ตรงกับภาพที่แสดงจริงแบบ 1:1 ป้องกันปัญหาภาพหดตัวใน Unity

---

### ขั้นตอนที่ 3: การแปลงพิกัดทางคณิตศาสตร์ (Mathematical Coordinate Mapping)
* **เป้าหมาย**: แปลงตำแหน่งจาก Figma World Space สู่ Unity UGUI Local Canvas โดยไม่มีความคลาดเคลื่อนจากการเดา
* **Logic การทำงานของ Agent**:
  1. **การแก้ปัญหา Canvas Offset Trap**:
     - ใน Figma จอ 1920x1080 มักวาง Modal เยื้องซ้ายหรือขวาตามเส้นไกด์ (เช่น `x = 394`)
     - ใน Unity หน้าต่างหลักมักจัดไว้กึ่งกลางจอ (`pivot = (0.5, 0.5)` ที่พิกัด `(0, 0)`)
     - ดังนั้น การคิดพิกัดของวัตถุภายในหน้าต่างทั้งหมด **ต้องตัดพิกัดสัมบูรณ์ของจอทิ้ง** และคิดเทียบกับ **ขอบบน-ซ้ายของ Modal Root** เท่านั้น
  2. **สูตรการแปลงพิกัด Top-Left (Zero-Guesswork Equation)**:
     - กำหนดให้ Anchor ของ Child ทุกชิ้นเป็น Top-Left:
       - `anchorMin = new Vector2(0f, 1f)`
       - `anchorMax = new Vector2(0f, 1f)`
       - `pivot = new Vector2(0f, 1f)`
     - คำนวณพิกัด Unity:
       $$\text{anchoredPosition.x} = \text{node.x} - \text{modalRoot.x}$$
       $$\text{anchoredPosition.y} = -(\text{node.y} - \text{modalRoot.y})$$
       $$\text{sizeDelta} = (\text{node.width}, \text{node.height})$$
  3. **การแปลงตำแหน่งสำหรับ Inner Child (Pivot Center)**:
     - หากวัตถุเป็น Child ของวัตถุอื่น (เช่น ไอคอนดาวใน Tag หรือข้อความบนปุ่ม) และต้องการใช้ Pivot กึ่งกลาง `(0.5, 0.5)`:
       $$\text{offset.x} = (\text{child.x} + \frac{\text{child.w}}{2}) - (\text{parent.x} + \frac{\text{parent.w}}{2})$$
       $$\text{offset.y} = (\text{parent.y} + \frac{\text{parent.h}}{2}) - (\text{child.y} + \frac{\text{child.h}}{2})$$

---

### ขั้นตอนที่ 4: การจับคู่แบบอักษรและ TextMesh Pro (Typography Mapping)
* **เป้าหมาย**: กำหนดฟอนต์ น้ำหนัก และโหมดการตัดข้อความให้ตรงกับ Figma ทุกประการ
* **Logic การทำงานของ Agent**:
  1. **Canonical Font Registry**:
     - `Novecentosanswide-DemiBold SDF` (GUID: `ed472c2881e9b894fb40179e7c9b4332`): หัวข้อไอเทม, ค่าสเตตัส, ข้อความเน้น
     - `Novecentosanswide-Normal SDF` (GUID: `a66aeea0855f6544883cbd79dc661f3a`): คำอธิบาย, ชนิดไอเทม, อัตราสิ้นเปลือง
     - `Kanit-Bold SDF` (GUID: `0063cb4c629a50d499805b5a4274846a`): ป้าย Tag หัวเรื่อง
     - `NotoSansThai-SemiBold SDF` (GUID: `57d8cea201ff3e04b8aef8e01a97988a`): ป้ายปุ่มแอ็กชัน
  2. **การตรวจสอบ SDF Font Atlas (Glyph Verification)**:
     - ตรวจสอบไฟล์ `.asset` ของฟอนต์ว่ามี `m_GlyphTable` ครบถ้วน ไม่ว่างเปล่า ป้องกันปัญหาข้อความหายใน Game View
  3. **การควบคุมการตัดคำและตัวพิมพ์ (Casing & Word Wrapping)**:
     - ตรวจสอบ Text Case ใน Figma อย่างเคร่งครัด: Title Case (`"Socket Mastery"`) แตกต่างจาก UPPERCASE (`"SOCKET MASTERY"`)
     - สำหรับข้อความบรรทัดเดียว (Single-line Header/Badge): ต้องตั้งค่า `enableWordWrapping = false` และ `overflowMode = TextOverflowModes.Overflow` เพื่อไม่ให้ข้อความตกบรรทัดเมื่อเกิดผลต่างระดับซับพิกเซล
  4. **การชดเชยเส้นฐานแนวตั้ง (Vertical Baseline Offset)**:
     - ฟอนต์ TextMesh Pro อาจมีระยะเยื้องตามธรรมชาติ (Ascender / Cap-Height) ต่างจาก SVG Text ในเว็บประมาณ 3-7 px ต้องชดเชยค่า Y เพื่อให้แนวตัวอักษรซ้อนทับกันพอดี

---

### ขั้นตอนที่ 5: การประกอบ Prefab และเชื่อมโยง C# Component (Prefab Synthesis)
* **เป้าหมาย**: สร้างโครงสร้าง Hierarchy ของ Prefab และผูก Reference ใน C# Script ให้ทำงานได้จริง
* **Logic การทำงานของ Agent**:
  1. **โครงสร้างสคริปต์ C# Builder**:
     - สร้างคลาส Editor Utility (เช่น `RefinementAndSocketPrefabBuilder.cs`) โดยจัดกลุ่มด้วย `#region` อย่างเคร่งครัด
     - ใช้เมธอด `PrefabUtility.SaveAsPrefabAsset()` เพื่อสร้าง Prefab ลงโฟลเดอร์ `Assets/Prefab/Game Page/`
  2. **การสร้าง Dynamic Controls ที่ถูกต้อง**:
     - **Progress Bar**: ห้ามเปลี่ยนขนาด Image กว้าง-ยาวด้วยมือ ให้ใช้ `UnityEngine.UI.Slider` แบบ `m_Interactable: false`, `m_Direction: LeftToRight` และผูก `m_FillRect` เข้ากับ Child RectTransform
     - **Button**: ผูกคอมโพเนนต์ `Button` พร้อมกำหนด `targetGraphic` ไปยัง Background Image
  3. **การผูก Serialized Properties อัตโนมัติ**:
     - ใช้ `SerializedObject` และ `FindProperty()` เพื่อเชื่อมโยงตัวแปรทั้งหมด เช่น `_headerTitle`, `_itemNameText`, `_infuseButton`, `_buttonText`
     - จัดการ List / Array ด้วย `ClearArray()`, `InsertArrayElementAtIndex()` และกำหนด `objectReferenceValue`
     - เรียก `ApplyModifiedPropertiesWithoutUndo()` ก่อนบันทึก Prefab

---

### ขั้นตอนที่ 6: การเรนเดอร์ภาพแบบ Headless Batchmode (Snapshot Pipeline)
* **เป้าหมาย**: เรนเดอร์ Prefab ที่สร้างเสร็จออกมาเป็นภาพ PNG ขนาด 1920x1080 px โดยไม่ต้องเปิด Unity Editor ด้วยมือ
* **Logic การทำงานของ Agent**:
  1. สร้าง Preview Scene ชั่วคราวด้วย `EditorSceneManager.NewPreviewScene()`
  2. ติดตั้ง Orthographic Camera และ Canvas Scaler (ความละเอียดอ้างอิง 1920x1080)
  3. สั่ง Rebuild เลย์เอาต์:
     - `Canvas.ForceUpdateCanvases();`
     - `LayoutRebuilder.MarkLayoutForRebuild(...);`
  4. เรนเดอร์กล้องลงใน `RenderTexture(1920, 1080, 24)` แล้วอ่านค่าพิกเซลผ่าน `Texture2D.ReadPixels()`
  5. บันทึกผลลัพธ์ลงเป็นไฟล์ PNG (เช่น `socket_unity_render.png`)
  6. สั่งรันคำสั่งผ่าน PowerShell Native (ไม่ใช้ `cmd`):
     ```powershell
     $p = Start-Process "D:\Unity\6000.4.8f1\Editor\Unity.exe" -ArgumentList "-batchmode", "-quit", "-projectPath", "`"F:\GameProject\Pale Vertica`"", "-executeMethod", "CoreSystem.Editor.UI.RefinementAndSocketPrefabBuilder.BuildSocketAndSnapshotBatch", "-logFile", "`"F:\GameProject\Pale Vertica\snapshot_batch.log`"" -PassThru; $p.WaitForExit()
     ```

---

### ขั้นตอนที่ 7: การตรวจสอบและสร้างภาพวิเคราะห์ด้วย OpenCV (Diagnostic Audit)
* **เป้าหมาย**: ตรวจสอบผลต่างระดับพิกเซล แยกแยะโซนที่มีปัญหา วัดขนาด รูปทรง รัศมีขอบมน และคำนวณคะแนนความแม่นยำอย่างเป็นรูปธรรม
* **มาตรฐานภาพวิเคราะห์ (Analysis Image Theme Standard)**:
  - ใช้สไตล์ **Minimal Simple Light Theme** สะอาดตา สบายตา ตัวหนังสือเข้มบนพื้นหลังสว่าง อ่านง่าย ไม่ใช้สไตล์ Dark Sci-Fi หรือเอฟเฟกต์ซับซ้อนที่รกสายตา
* **Logic การวิเคราะห์ความแม่นยำสูง (High-Precision Audit)**:
  1. ครอปเฉพาะขอบเขตหน้าต่าง Modal (ขนาด 1083x705 px) จากทั้งภาพ Figma และภาพเรนเดอร์ Unity
  2. คำนวณความต่างสมบูรณ์ของสีพิกเซล (Raw Absolute Difference):
     $$\text{diff} = |\text{Image}_{\text{figma}} - \text{Image}_{\text{unity}}|$$
  3. คำนวณค่า Thresholded Similarity:
     - พิกเซลที่มีค่าความต่าง $\le 25$ ถือว่าตรงกัน (ยอมรับความต่างจากการเกลี่ยขอบ Anti-Aliasing และ Shader)
     - คำนวณร้อยละความแม่นยำภาพรวม (Overall Match Percentage)
  4. **การตรวจสอบสัดส่วนรูปภาพและรัศมีขอบมน (Image Scale & Corner Radius Verification)**:
     - ตรวจสอบว่ารูปภาพที่นำมาใส่มีขนาดจริงเท่ากับรูปใน Figma หรือไม่ โดยตัดปัญหาพื้นที่โปร่งใส (Transparent Padding) ที่ทำให้ `preserveAspect` ย่อรูปภาพลงโดยไม่ได้ตั้งใจ
     - ตรวจวัดรัศมีมุมโค้ง (Corner Radius) ของ 9-slice Sprite ให้สอดคล้องกับค่า `cornerRadius` ใน Figma
  5. **การคำนวณจุดกึ่งกลางมวลซับพิกเซล (Centroid Moment Analysis)**:
     - สกัดรูปทรงหรือข้อความในแต่ละโซนออกมาหาค่า Moment:
       $$x_c = \frac{m_{10}}{m_{00}}, \quad y_c = \frac{m_{01}}{m_{00}}$$
     - คำนวณระยะคลาดเคลื่อนจริง:
       $$\Delta x = x_{c,\text{unity}} - x_{c,\text{figma}}, \quad \Delta y = y_{c,\text{unity}} - y_{c,\text{figma}}$$
  6. สร้างภาพเปรียบเทียบแบบ Minimal Light: [Figma Ground Truth] | [Unity Snapshot] | [Difference Map] พร้อมตารางสรุปคะแนนแต่ละโซนแบบเรียบง่าย ชัดเจน

---

### ขั้นตอนที่ 8: การปรับแต่งย้อนกลับแบบวงรอบปิด (Closed-Loop Fine-Tuning)
* **เป้าหมาย**: นำผลต่างที่ได้จากขั้นตอนที่ 7 กลับไปแก้ไขพิกัดในสคริปต์ C# จนกว่าคะแนนความแม่นยำจะผ่านเกณฑ์ (>93-95%)
* **Logic การทำงานของ Agent**:
  1. หากพบโซนที่คะแนนต่ำกว่า 90%:
     - อ่านค่า $\Delta x, \Delta y$ จากขั้นตอนที่ 7
     - ปรับแก้ค่า `anchoredPosition` ใน C# Builder ด้วยระยะที่ตรงกันข้ามกับ Delta
  2. สั่งรัน Batchmode ซ้ำ (ขั้นตอนที่ 6)
  3. ตรวจวัดผลคะแนนใหม่ (ขั้นตอนที่ 7)
  4. เมื่อความแม่นยำรวมแตะระดับเป้าหมาย ถือว่าการแปลง UI เสร็จสมบูรณ์

---

## 3. สรุปความคุ้มค่าและผลการทดสอบจริง (Real-World Case Study)

| หน้าต่าง UI (Window) | ความแม่นยำเดิม (Legacy) | ความแม่นยำหลังผ่าน Pipeline | ระยะเวลาประมวลผล |
|---|:---:|:---:|:---:|
| **Refinement Window** (`569:2453`, now `1786:24436`) | 71.40% | **94.85%** | ~50 วินาที |
| **Socket Window** (`2170:1681`) | 89.64% | **93.03%** | ~55 วินาที |

---

## 4. แหล่งอ้างอิงและไฟล์ที่เกี่ยวข้อง (References)

* **สคริปต์สร้าง Prefab ใน Unity**: [RefinementAndSocketPrefabBuilder.cs](file:///f:/GameProject/Pale%20Vertica/Assets/Script/Editor/UI/RefinementAndSocketPrefabBuilder.cs)
* **คอมโพเนนต์ C# หน้าต่าง Socket**: [SocketWindow.cs](file:///f:/GameProject/Pale%20Vertica/Assets/Script/UI/Socket/SocketWindow.cs)
* **คอมโพเนนต์ C# หน้าต่าง Refinement**: [RefinementWindow.cs](file:///f:/GameProject/Pale%20Vertica/Assets/Script/UI/Refinement/RefinementWindow.cs)
* **สคริปต์ตรวจวัดและสร้าง Heatmap**: `Tools/UICompare/generate_socket_analysis.py`
* **ตัวอย่างผลลัพธ์ Heatmap**: [socket_advanced_analysis.png](file:///C:/Users/User/.gemini/antigravity/brain/29ea3001-8df0-4155-a601-718c8cb28072/socket_advanced_analysis.png)
