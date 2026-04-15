# AdditionalValue Standard — Realm of Hero

> ระบบ **AdditionalChain** คือชั้นการคำนวณที่เพิ่มเข้าหลัง pipeline ปกติของ Attribute จบแล้ว  
> ใช้สำหรับ Skill, Buff, Debuff, หรือ Effect พิเศษที่ต้องการแทรก value โดยไม่กระทบสูตรหลัก

---

## Pipeline ปกติ (Normal Pipeline)

```
BaseValue
  + StatFlatBonus (จาก STR, VIT, ฯลฯ)
  + PassiveFlatBonus (จาก Skill Tree)
  + Modifier.Flat
  × (1 + PercentAdd)
  × PercentMult
= preFinalValue
```

จากนั้น **AdditionalChain** เริ่มทำงาน:

```
preFinalValue → Entry[order=0] → Entry[order=1] → ... → CurrentValue
```

---

## Order Convention (มาตรฐาน)

| Order | ชื่อ Layer           | คืออะไร                                                    | Tag มาตรฐาน      |
|------:|---------------------|------------------------------------------------------------|-----------------|
| 0     | **Base Override**   | ขั้นแรกสุด — scale ออกก่อนทุกอย่าง                       | *(custom)*      |
| 1     | **Skill Bonus**     | Bonus จาก Active Skill ที่กำลังใช้งาน                    | `SkillBuff_*`   |
| 2     | **Passive Tree** ★  | Bonus จาก Passive Skill Tree Node (Flat + Percent×)       | `"Passive"`     |
| 3     | **Buff Effect**     | Buff ชั่วคราว เช่น Potion, Shrine, Ally Skill             | `Buff_*`        |
| 4     | **Debuff Effect**   | Debuff จากศัตรู เช่น Curse, Armor Break                   | `Debuff_*`      |
| 5     | **Cap / Clamp**     | จำกัดค่า Max/Min หลังทุกอย่าง apply แล้ว                 | `System_*`      |
| 6+    | **Custom / Future** | สำรองสำหรับระบบใหม่                                       | *(custom)*      |

> ★ **Order 2 / Tag `"Passive"`** ถูก reserve ไว้สำหรับ `ApplyPassiveStats()` เท่านั้น  
> ห้ามใช้ tag นี้จากระบบอื่น เพราะ `ApplyPassiveStats()` จะ `RemoveByTag("Passive")` ทุกครั้งที่รีเซ็ต

---

## ตัวอย่างการคำนวณ

### Case A — ใช้ปกติ
```
preFinal = 100
Entry(order=0, *6)  → 100 * 6 = 600
Entry(order=1, +9)  → 600 + 9 = 609
CurrentValue = 609
```

### Case B — ใช้ Skill พิเศษ (สลับ op)
```
preFinal = 100
Entry(order=0, +6)  → 100 + 6 = 106
Entry(order=1, *9)  → 106 * 9 = 954
CurrentValue = 954
```

---

## API Reference

### เพิ่ม Entry
```csharp
// ผ่าน AttributeSystem (แนะนำ)
var entry = attributeSystem.AddAdditional(
    AttributeType.ATK,   // attribute เป้าหมาย
    order: 1,            // ลำดับ
    AdditionalOp.Add,    // +
    value: 50f,
    tag: "SkillBuff_Slash"
);

// หรือสร้าง entry แยกแล้วส่งเข้า
var e = new AdditionalEntry(order: 0, AdditionalOp.Multiply, 1.5f, "EliteBuff");
attributeSystem.AddAdditional(AttributeType.MATK, e);
```

### ลบ Entry
```csharp
// ลบด้วย reference
attributeSystem.RemoveAdditional(AttributeType.ATK, entry);

// ลบทุก entry ที่มี tag นี้
attributeSystem.RemoveAdditionalByTag(AttributeType.ATK, "SkillBuff_Slash");
```

### เคลียร์
```csharp
// เคลียร์ attribute เดียว
attributeSystem.ClearAdditional(AttributeType.ATK);

// เคลียร์ทั้งหมดทุก attribute
attributeSystem.ClearAllAdditional();
```

### อ่านค่า Chain
```csharp
var attr = attributeSystem.GetAttribute(AttributeType.ATK);
foreach (var e in attr.Additional.Entries)
    Debug.Log(e); // "(1,+50 [SkillBuff_Slash])"
```

---

## Tag Convention (แนะนำ)

| รูปแบบ Tag              | ใช้กับ                        |
|------------------------|-------------------------------|
| `SkillBuff_{SkillName}` | Active Skill ที่ใช้งาน        |
| `Buff_{EffectName}`     | Buff ชั่วคราวจาก Item/Shrine  |
| `Debuff_{Source}`       | Debuff จากศัตรู               |
| `Passive_{NodeId}`      | Node จาก Skill Tree พิเศษ     |
| `System_{Name}`         | ระบบ Internal                 |

---

## หมายเหตุ

- `AdditionalChain` **ไม่ได้** รีเซ็ตอัตโนมัติ — ต้อง Remove หรือ Clear เอง
- ใช้ **Tag** เสมอเมื่อ Entry มาจาก Skill/Buff ชั่วคราว เพื่อ Remove ได้ถูกต้อง
- `Additional.Apply()` จะ return ค่าเดิมถ้า chain ว่างเปล่า (no-op)
