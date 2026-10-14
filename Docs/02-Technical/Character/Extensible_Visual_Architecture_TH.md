# สถาปัตยกรรม EntityVisualController: รองรับการต่อขยาย (Extensible Visual Architecture)

เอกสารนี้อธิบายการออกแบบ **`EntityVisualController`** ให้เป็นระบบเปิดที่รองรับฟีเจอร์ใหม่ๆ ในอนาคต (เช่น Outline, Status Tints, Dissolve, Ghost Trail) ได้ทันที **โดยไม่ต้องรื้อหรือแก้โค้ดเก่า**

---

## 1. ปัญหาเดิม vs สถาปัตยกรรมแบบเปิด (Open-Closed Principle)

| ปัญหาของการเขียนแบบเดิม (Hardcoded) | สถาปัตยกรรมแบบเปิด (Extensible Design) |
| :--- | :--- |
| ต้องเขียนโค้ดแก้ Material ทีละตัว ทุกครั้งที่เพิ่ม Effect | ทุก Effect สื่อสารผ่าน **`MaterialPropertyBlock`** และ Master Material เดียวกัน |
| เพิ่ม Outline แล้วกระทบ Hit Flash หรือทับกับ Costume | แต่ละ Effect มี Channel การควบคุมของตัวเอง ไม่ทับซ้อนกัน |
| มีชุดหรืออาวุธใหม่เพิ่มเข้ามา Effect เก่าไม่แสดงผล | มีระบบ **`RegisterRenderer()` / `RefreshRenderers()`** ชิ้นส่วนใหม่จะได้รับ Effect ทุกตัวทันที |
| ต้องลบ/เขียน Script ใหม่ทุกครั้งที่มีระบบ Visual ใหม่ | มี **Public API** มาตรฐาน (`SetOutline`, `SetFlash`, `SetStatusTint`, `SetDissolve`) |

---

## 2. โครงสร้างการทำงานภายในของ `EntityVisualController`

```text
[EntityVisualController] (ศูนย์กลางจัดการ Renderers & Visual State)
  │
  ├── [Renderer Registry]
  │     ├── BodyRenderers (Head, Body, Arms, Legs, Weapons) <-- แคชไว้อัตโนมัติ + รองรับเพิ่มชิ้นส่วน runtime
  │     └── ShadowRenderer (คัดแยกออก ไม่ให้โดน Effect ใดๆ)
  │
  ├── [MaterialPropertyBlock Hub] (ส่งค่าให้ Shader กลาง)
  │     ├── _FlashAmount / _FlashColor      <-- ควบคุมโดย Hit Flash
  │     ├── _StatusColor / _StatusIntensity <-- ควบคุมโดย Poison, Freeze, Burn
  │     ├── _OutlineColor / _OutlineWidth   <-- ควบคุมโดย Target Selection Outline
  │     └── _DissolveAmount                 <-- ควบคุมโดย Death Dissolve
  │
  └── [Public Visual API] (ให้ระบบภายนอกเรียกใช้ได้ทันทีโดยไม่ต้องแก้ไส้ใน)
        ├── PlayHitFlash(Color color, float duration)
        ├── SetOutline(bool active, Color outlineColor)
        ├── SetStatusEffect(StatusType type, Color color, float intensity)
        ├── SetDissolve(float progress)
        ├── SetFacingDirection(float horizontalDir)
        └── RegisterDynamicRenderer(SpriteRenderer newPart)
```

---

## 3. ฟีเจอร์ทั้งหมดที่สถาปัตยกรรมนี้รองรับได้ทันที (ทำอะไรได้อีกบ้าง?)

1. **ระบบ Outline เมื่อเลือกเป้าหมาย (Target Selection Outline)**:
   - สั่ง `visualController.SetOutline(true, Color.red)` สำหรับศัตรู หรือ `Color.yellow` สำหรับเป้าหมายที่เลือก
   - Master Shader จะวาดขอบรอบ `BodyRenderers` ทุกชิ้นพร้อมกันโดยอัตโนมัติ และเว้นเงาใต้เท้าไว้
2. **ระบบสถานะผิดปกติ (Status Tints & Overlays)**:
   - ติดพิษ (Poison) -> `SetStatusEffect(Poison, Color.green)` กะพริบเขียวเป็นจังหวะ
   - แช่แข็ง (Freeze) -> ย้อมสีฟ้าและลดความเร็วอนิเมชัน
   - ติดไฟ (Burn) -> ย้อมสีส้มแดงพร้อม Particle เผาไหม้
3. **ระบบสลายตัวตอนตาย (Death Dissolve Effect)**:
   - เมื่อมอนสเตอร์ตาย ดักฟัง `OnDeath` แล้วค่อยๆ ปรับ `_DissolveAmount` จาก 0 ถึง 1 ให้ตัวละครค่อยๆ สลายตัวเป็นขี้เถ้า
4. **ระบบเงาติดตาตอนพุ่ง Dash (Ghost Trail / Afterimage)**:
   - ดึงรายชื่อ `BodyRenderers` ไปสร้าง Snapshot เงาตามหลังขณะพุ่ง Dash
5. **รองรับการเปลี่ยนชุด Costume / สลับอาวุธแบบ Dynamic**:
   - เมื่อ `CharacterVisualController` โหลดชุดใหม่เข้ามา ระบบจะเรียก `RegisterDynamicRenderer()` ทำให้อาวุธหรือหมวกใหม่ได้รับผลของ Hit Flash, Outline และ Shader ทันทีโดยไม่ต้องตั้งค่าใหม่

---

## 4. แผนการเพิ่มฟีเจอร์ใหม่โดยไม่กระทบของเก่า (Zero-Friction Extension)

หากในอนาคตต้องการเพิ่มฟีเจอร์ใหม่ เช่น **"ระบบเรืองแสงตอน Enrage"**:
- **ขั้นตอนที่ 1**: เพิ่ม Parameter ใน Shader (เช่น `_EmissionColor`)
- **ขั้นตอนที่ 2**: เพิ่ม Method ใน `EntityVisualController` เช่น `SetEnrageGlow(bool active, Color color)`
- **ขั้นตอนที่ 3**: จบ! ไม่ต้องแก้โครงสร้าง Prefab ไม่ต้องแก้ Character.cs และไม่กระทบ Hit Flash หรือ Outline เดิม
