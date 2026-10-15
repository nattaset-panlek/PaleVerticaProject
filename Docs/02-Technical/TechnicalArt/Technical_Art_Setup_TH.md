# Realm of Hero: แผนงานและคู่มือการติดตั้ง Shader, Material และ EntityVisualController

เอกสารนี้อธิบายอย่างชัดเจนว่า **Shader และ Material สร้างที่ไหน วางไว้ตรงไหน และเชื่อมต่อกับ `EntityVisualController` อย่างไร** เพื่อให้เห็นภาพตั้งแต่ต้นจนจบ

---

## 1. ผังการเชื่อมต่อ (Where Shader, Material & Script Connect)

```text
[1. ไฟล์ Shader]
Assets/Materials/Shader/Character2DLitMaster.shader
       │
       ▼ (คลิกขวา Create -> Material)
[2. ไฟล์ Material กลาง]
Assets/Materials/Character2DLitMaster.mat
       │
       ▼ (ลากมาใส่ในช่อง Master Material ช่องเดียว)
[3. คอมโพเนนต์บนตัวละคร: EntityVisualController.cs]
       ├── ModelRoot        <-- ลาก GameObject ส่วน Model ของตัวละครมาใส่
       ├── ShadowRenderer   <-- ลาก Sprite เงาใต้เท้ามาใส่ (ระบบจะกันไม่ให้โดนแฟลช)
       └── MasterMaterial   <-- ลาก Character2DLitMaster.mat มาใส่ช่องนี้ช่องเดียว!
```

---

## 2. วิธีการทำงานอัตโนมัติ (ทำไมไม่ต้องลาก Material ใส่ทีละชิ้นส่วน)

เมื่อเริ่มเกม (`Awake`):
1. `EntityVisualController` จะค้นหา `SpriteRenderer` ทั้งหมดที่อยู่ใต้ `ModelRoot` (เช่น Head, Body, Arms, Legs, Weapons)
2. `EntityVisualController` จะนำ **`MasterMaterial` ไปแปะให้ทุก `SpriteRenderer` ลูกให้อัตโนมัติทันที**
3. **ผลลัพธ์**: คุณไม่ต้องเสียเวลาคลิกเข้าไปลาก Material ใส่ทีละชิ้นส่วนใน Prefab เลยแม้แต่ชิ้นเดียว!

---

## 3. การควบคุม Effect ตอนเล่นเกม (Runtime Communication)

เมื่อเกิดเหตุการณ์ในเกม `EntityVisualController` จะคุยกับ Shader ผ่าน **`MaterialPropertyBlock`**:

```text
[เหตุการณ์: โดนโจมตี]
       │
       ▼
EntityVisualController.PlayHitFlash()
       │
       ▼
ส่งค่า _FlashAmount = 1.0 และ _FlashColor = White เข้า MaterialPropertyBlock
       │
       ▼
Master Shader แสดงผลกะพริบสีขาวบนหน้าจอ 0.1 วินาที (โดยเงาใต้เท้าไม่กะพริบ)
```

```text
[เหตุการณ์: เมาส์ชี้เลือกเป้าหมาย]
       │
       ▼
EntityVisualController.SetOutline(true, Color.red)
       │
       ▼
ส่งค่า _OutlineWidth = 1.0 และ _OutlineColor = Red เข้า MaterialPropertyBlock
       │
       ▼
Master Shader วาดเส้นขอบเรืองแสงรอบตัวละครทันที
```

---

## 4. ลำดับขั้นตอนการพัฒนา (Sprint Backlog & Action Plan)

- [ ] **ขั้นตอนที่ 1: สร้าง Master 2D Character Shader**
  - ตำแหน่งไฟล์: `Assets/Materials/Shader/Character2DLitMaster.shader`
  - รองรับ: URP 2D Lit, `_FlashAmount`, `_FlashColor`, `_OutlineColor`, `_OutlineWidth`, `_StatusColor`, `_DissolveAmount`

- [ ] **ขั้นตอนที่ 2: สร้าง Master Material**
  - ตำแหน่งไฟล์: `Assets/Materials/Character2DLitMaster.mat`
  - เลือกใช้ Shader `Character2DLitMaster`

- [ ] **ขั้นตอนที่ 3: สร้าง `EntityVisualController.cs`**
  - ตำแหน่งไฟล์: `Assets/Script/CoreSystem/Visuals/EntityVisualController.cs`
  - มีช่อง Inspector: `ModelRoot`, `ShadowRenderer`, และ `MasterMaterial`
  - ทำระบบฉีด Material ให้ SpriteRenderers ลูกอัตโนมัติ
  - ทำระบบ Hit-Flash (`PlayHitFlash`) ข้ามเงา
  - ทำระบบ 2D Facing Flip (`SetFacing` สลับ `modelRoot.localScale.x`)
  - ทำระบบเตรียมพร้อมสำหรับ Outline (`SetOutline`)

- [ ] **ขั้นตอนที่ 4: สร้าง Combat Juice (`CombatJuiceDirector.cs`)**
  - ตำแหน่งไฟล์: `Assets/Script/CoreSystem/Visuals/CombatJuiceDirector.cs`
  - สั่ง Hit-Stop (หยุดเวลาสั้นๆ 0.03 - 0.06 วิ) และสั่นหน้าจอเมื่อโจมตีโดน
