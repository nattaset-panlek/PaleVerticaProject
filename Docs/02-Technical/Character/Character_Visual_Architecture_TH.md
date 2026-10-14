# สรุปโครงสร้าง Component ของ Character ในโปรเจกต์ (Zero Clutter Architecture)

เอกสารนี้อธิบายโครงสร้าง Component ปัจจุบันของ Character ในโปรเจกต์ Realm of Hero และแนวทางรวมระบบ Visual ให้อยู่ใน Component เดียว เพื่อไม่ให้มี Script ติดบน GameObject เยอะจนรก

---

## 1. ปัจจุบันในโปรเจกต์เป็นอย่างไร? (Current State)

ในโปรเจกต์นี้ ตัวละครของคุณออกแบบไว้ดีมากอยู่แล้วด้วยหลัก **Facade Pattern**:
- **ระบบตรรกะ (Logic)**: รวมอยู่ใน `Character.cs` เพียงไฟล์เดียว โดยใช้ Pure C# Classes (`MovementSystem`, `AttributeSystem`, `SkillCaster`, `ExperienceSystem`) ภายใน โดย**ไม่ได้**สร้างเป็น MonoBehaviour แยกติดบน GameObject ให้รก
- **ระบบภาพ (Visual)**: ปัจจุบันมีเพียง Component เดียวคือ **`CharacterVisualController.cs`** ทำหน้าที่จัดการ Sprite ของชุด Costume (Head, Body, Arms, Legs)

---

## 2. ทางออก: รวมทุกระบบ Visual ไว้ใน `CharacterVisualController` เพียงตัวเดียว!

เรา**ไม่ต้องสร้างหลาย Component** มาแปะบน GameObject ให้รก โดยเราจะนำความสามารถใหม่ทั้งหมดรวมเข้ากับ **`CharacterVisualController.cs`** ที่มีอยู่แล้ว:

```text
[Character GameObject]
  │
  ├── Character.cs                 <-- คุม Logic ทั้งหมด (Stats, Movement, Skills)
  └── CharacterVisualController.cs <-- คุม Visual ทั้งหมดในตัวเดียว! (Costume, Hit Flash, 2D Facing, Squash)
        │
        ├── [ModelRoot]            <-- Transform สำหรับ Flip ซ้าย-ขวา
        │     ├── Head, Body, Arms, Legs (SpriteRenderers)
        │     └── Weapon Sockets
        │
        └── [Shadow]               <-- SpriteRenderer เงา (Ignore จาก Flash)
```

---

## 3. สิ่งที่ `CharacterVisualController` ตัวเดียวจะดูแลให้ทั้งหมด:

1. **โหลดชุด Costume (ระบบเดิมที่มีอยู่แล้ว)**: โหลด Sprite จาก Addressables ใส่ตามช่อง Head, Body, Arms, Legs
2. **Hit Flash & Damage Tint (เพิ่มเข้าไปในนี้)**: จัดการ `MaterialPropertyBlock` กะพริบขาวเมื่อโดนตี โดยค้นหา SpriteRenderers ใต้ `ModelRoot` อัตโนมัติ และเว้น `Shadow` ออก
3. **2D Facing & Flipping (เพิ่มเข้าไปในนี้)**: สลับ `ModelRoot.localScale.x = 1 / -1` ตามทิศทางเดินหรือเล็งเป้า
4. **Squash & Stretch (เพิ่มเข้าไปในนี้)**: ย่อ-ขยาย ModelRoot สั้นๆ เมื่อ Dash หรือกระโดด

---

## 4. ข้อดีของแนวทางนี้

1. **ไม่รก Inspector**: บน Prefab ตัวละครมีแค่ **`Character`** และ **`CharacterVisualController`** แค่ 2 ตัวเท่านั้น
2. **สร้างตัวละครใหม่ได้ทันที**: เมื่อสร้าง Prefab ตัวละครใหม่ แค่ลาก `ModelRoot` ใส่ใน `CharacterVisualController` ระบบ Hit Flash, Facing และ Costume จะทำงานให้อัตโนมัติทันที
3. **แก้โค้ดที่เดียว**: ไม่ต้องไล่เปิดดูทีละ Component หากต้องการปรับจูนเอฟเฟกต์ภาพ
