# คู่มือและสรุปผลการพัฒนา: Milestone 1 Technical Art (Walkthrough)

เอกสารนี้สรุปผลการพัฒนาระบบ Technical Art แบบรวมศูนย์ (Centralized Architecture) ด้วย ScriptableObject Asset

---

## 1. ไฟล์ทั้งหมดในระบบ

| ไฟล์ | ตำแหน่งที่อยู่ | หน้าที่การทำงาน |
| :--- | :--- | :--- |
| **Settings Script** | `Assets/Script/CoreSystem/Visuals/EntityVisualSettings.cs` | คลาส ScriptableObject โครงสร้างการตั้งค่าภาพส่วนกลาง |
| **Settings Asset** | `Assets/Resources/Settings/EntityVisualSettings.asset` | ไฟล์ Asset กลางที่ `EntityVisualController` โหลดจาก Path อัตโนมัติ |
| **Shader** | `Assets/Materail/Shader/Character2DLitMaster.shader` | Master 2D Lit Shader รองรับ Hit Flash, Status Tint, Dissolve |
| **Material (Resources)** | `Assets/Resources/Materials/Character2DLitMaster.mat` | Master Material ใน Resources (โหลดอัตโนมัติ 100%) |
| **Visual Hub** | `Assets/Script/CoreSystem/Visuals/EntityVisualController.cs` | คอมโพเนนต์ Visual สากลติดบน Entity ทุกตัว (อ่านค่าจาก Settings.asset) |
| **Combat Juice** | `Assets/Script/CoreSystem/Visuals/CombatJuiceDirector.cs` | ระบบ Hit-Stop (หยุดเวลาชั่วขณะ) และสั่นหน้าจอเมื่อโจมตีโดน |

---

## 2. การตั้งค่าแบบรวมศูนย์ด้วย ScriptableObject Asset (`EntityVisualSettings.asset`)

ไฟล์ Asset สำหรับปรับแต่งถูกสร้างไว้ที่:
`Assets/Resources/Settings/EntityVisualSettings.asset`

คุณสามารถคลิกที่ไฟล์นี้ใน Unity Inspector เพื่อปรับแต่งค่า Visual ของทั้งเกมได้ในจุดเดียว:
- **Hit Flash**: ระยะเวลาแฟลช (`_flashDuration`), สีแฟลชปกติ (`_defaultFlashColor`), สีแฟลชเมื่อคริติคอล (`_criticalFlashColor`)
- **Status Tints**: สีพิษ (`_poisonColor`), สีแช่แข็ง (`_freezeColor`), สีไฟเผา (`_burnColor`), สีสตัน (`_stunColor`)
- **Dissolve**: ระยะเวลาสลายตัว (`_defaultDissolveDuration`), สีขอบไฟ (`_dissolveEdgeColor`)

> **ผลลัพธ์**: แก้ไขค่าในไฟล์ Settings นี้เพียงไฟล์เดียว **มอนสเตอร์และตัวละครทุกตัวในเกมจะเปลี่ยนตามทันที!**

---

## 3. วิธีการติดตั้งใช้งานบน Prefab (Zero Configuration Setup)

### สำหรับ Monster, Enemy, หรือ NPC ทั่วไป:
1. เปิด Prefab ขึ้นมา แล้วใส่คอมโพเนนต์ **`EntityVisualController`**
2. **ไม่ต้องตั้งค่าอะไรเลย!**
   - ชิ้นส่วน Model (ค้นหาชื่อ `Model` ให้อัตโนมัติ)
   - เงาใต้เท้า (ค้นหาชื่อ `Shadow` และคัดแยกออกให้อัตโนมัติ)
   - Material และค่าสี/เวลา (ดึงจาก `EntityVisualSettings.asset` ใน Resources ให้อัตโนมัติ)
3. **เสร็จสิ้น!**

---

## 4. ตัวอย่างการเรียกใช้งาน API ใน Code

```csharp
EntityVisualController visual = GetComponent<EntityVisualController>();

// 1. กะพริบขาวเมื่อโดนตี (ดึงสีและเวลาจาก EntityVisualSettings.asset กลางให้อัตโนมัติ)
visual.PlayHitFlash();

// 2. ย้อมสีสถานะ (ติดพิษเขียว / แช่แข็งฟ้า / ไฟเผาส้ม)
visual.SetStatusTint(EntityVisualSettings.Instance.PoisonColor, intensity: 0.8f);

// 3. สั่งสลายตัวตอนตาย (ดึงเวลาและสีขอบไฟจาก Settings.asset กลาง)
visual.PlayDissolve(onComplete: () => Destroy(gameObject));

// 4. หันหน้าซ้าย-ขวา 2D (สลับแกน X ของ Model โดยแกน Z ไม่หมุนเอียง)
visual.SetFacing(moveDirection.x);
```

---

## 5. วิธีทดสอบใน Unity Editor

1. กด **Play** ใน Unity Editor
2. คลิกขวาที่คอมโพเนนต์ `EntityVisualController` ใน Inspector
3. เลือก:
   - **`Test / Play Hit Flash`** -> ทดสอบกะพริบสีขาว
   - **`Test / Toggle Status Tint (Poison)`** -> ทดสอบย้อมสีพิษ
