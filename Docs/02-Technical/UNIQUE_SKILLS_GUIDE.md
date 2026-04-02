# 📘 คู่มือการสร้าง Unique Skill (Advanced Developer Guide)

ยินดีต้อนรับสู่ระบบ **Unique Skill**! ระบบนี้ถูกสร้างขึ้นเพื่อทลายขีดจำกัดของระบบ Skill เดิมที่ล็อคไว้แค่ Targeting และ Effect พื้นฐาน ตอนนี้คุณสามารถเขียน Code C# เพื่อกำหนดพฤติกรรมของสกิลได้อย่างอิสระ 100%

---

## 🏗️ สถาปัตยกรรม (Architecture)
ระบบใช้หลักการ **Polymorphism** โดยเปลี่ยนให้ `ActiveSkill` มีเมธอด `virtual void Execute`. 
- **Standard Skill:** ใช้ Logic พื้นฐานที่อยู่ใน `ActiveSkill.Execute` (ดึงเป้าหมาย -> รัน Effect -> เล่น Visual)
- **Unique Skill:** คือ Class ที่สืบทอดจาก `ActiveSkill` และทำการ `override` เมธอด `Execute` เพื่อเขียน Logic ของตัวเอง

---

## 🚶‍♂️ ขั้นตอนการสร้าง (Walkthrough)

### 1. สร้าง Class ใหม่
สร้างไฟล์ใน Folder `Assets/Script/CoreSystem/Skills/UniqueSkill/` โดยตั้งชื่อตามสกิลของคุณ
```csharp
using CoreSystem.Entity;
using UnityEngine;

namespace CoreSystem.Skills.UniqueSkill
{
    [CreateAssetMenu(fileName = "MySkill", menuName = "Skill System/Unique/My Skill")]
    public class MySkill : ActiveSkill 
    {
        public override void Execute(SkillPayload payload, SkillInstance instance, SkillCaster caster)
        {
            // เขียน Code ของคุณที่นี่!
        }
    }
}
```

### 2. ลงทะเบียนใน Database (Skill.json)
เพื่อให้ระบบ Converter รู้จักคลาสใหม่ของคุณ ต้องระบุ `CustomClassName` ใน JSON:
```json
{
    "SkillID": 9999,
    "CustomClassName": "CoreSystem.Skills.UniqueSkill.MySkill",
    "SkillName": "อิสระในการเขียน Code"
}
```

---

## 💡 ตัวอย่างการนำไปใช้งาน (Examples)

### 1. ⚔️ สกิลเรียกสมุน (Summoning)
**Concept:** สร้าง Object ใหม่ขึ้นมาในฉาก และกำหนดระยะเวลาคงอยู่
- **วิธีทำ:** ใช้ `GameObject.Instantiate` เพื่อสร้าง Minion Prefab
- **จุดเด่น:** สามารถเลือกว่าจะเป็น "ชั่วคราว" (ใช้ `Destroy(obj, duration)`) หรือ "ถาวร" ได้
- **Code:** [SummonSkill.cs](file:///f:/GameProject/Realm%20of%20Hero/My%20project/Assets/Script/CoreSystem/Skills/UniqueSkill/SummonSkill.cs)

### 🛡️ 2. ระบบ Parry (จังหวะสวนกลับ)
**Concept:** เปิด "หน้าต่างเวลา" สั้นๆ เพื่อรับการโจมตีแล้วสวนกลับ
- **วิธีทำ:** เมื่อกดใช้สกิล ให้ตัวละครเข้าสู่สถานะ `IsParrying` ในช่วงเวลาหนึ่ง
- **จุดเด่น:** สร้าง Mechanic ที่ต้องใช้ความแม่นยำ (Timing) เพิ่มความสนุกให้การเล่น
- **Code:** [ParrySkill.cs](file:///f:/GameProject/Realm%20of%20Hero/My%20project/Assets/Script/CoreSystem/Skills/UniqueSkill/ParrySkill.cs)

### ⚡ 3. สายฟ้าชิ่ง (Chain Lightning)
**Concept:** โจมตีเป้าหมายแรก แล้วกระโดดไปหาเป้าหมายถัดไปที่ใกล้ที่สุด
- **วิธีทำ:** ใช้ `Physics2D.OverlapCircleAll` วนลูปหาศัตรูในระยะ และใช้ `HashSet` เพื่อไม่ให้ชิ่งกลับตัวเดิม
- **จุดเด่น:** จัดการศัตรูเป็นกลุ่มได้อย่างมีประสิทธิภาพ
- **Code:** [ChainLightningSkill.cs](file:///f:/GameProject/Realm%20of%20Hero/My%20project/Assets/Script/CoreSystem/Skills/UniqueSkill/ChainLightningSkill.cs)

### 👻 4. Blink Strike (วาร์ปปลิดชีพ)
**Concept:** เทเลพอร์ตไปข้างหลังเป้าหมายแล้วโจมตีทันที
- **วิธีทำ:** เปลี่ยน `transform.position` ของผู้ใช้ไปยังจุดที่ต้องการ ก่อนจะสั่งรันดาเมจ
- **จุดเด่น:** ให้ความรู้สึกรวดเร็วและเป็น Assassin
- **Code:** [BlinkStrikeSkill.cs](file:///f:/GameProject/Realm%20of%20Hero/My%20project/Assets/Script/CoreSystem/Skills/UniqueSkill/BlinkStrikeSkill.cs)

### 🌀 5. Time Dilation (หน่วงเวลา)
**Concept:** สร้างอาณาเขตที่ทำให้ทุกอย่างในนั้นช้าลง
- **วิธีทำ:** ทำงานร่วมกับระบบ `ContinuousArea` เพื่อลงโทษศัตรูที่อยู่ในวง
- **จุดเด่น:** สร้างความยุทธศาสตร์ในการควบคุมพื้นที่ (Crowd Control)
- **Code:** [TimeDilationSkill.cs](file:///f:/GameProject/Realm%20of%20Hero/My%20project/Assets/Script/CoreSystem/Skills/UniqueSkill/TimeDilationSkill.cs)

---

## 🛠️ เคล็ดลับเพิ่มเติม
- **เรียกใช้ Logic เดิม:** ถ้าต้องการให้สกิลทำดาเมจปกติด้วย แค่เรียก `base.Execute(payload, instance, caster);` ไว้ในบรรทัดแรกของฟังก์ชัน
- **เข้าถึงข้อมูลตัวละคร:** ใช้ `caster.CharacterRef` เพื่อดึงข้อมูล `AttributeSystem` หรือตำแหน่งของผู้ใช้ได้ทันที
- **Visual Effects:** คุณสามารถสั่ง `VisualManager.Instance.PlayVisual(...)` เองได้เลยถ้าต้องการ Effect ที่ซับซ้อนกว่าใน JSON

> ระบบนี้จะช่วยให้เกมของคุณมีความลึกและแตกต่าง (Unique) สมชื่อคลาสครับ! 🎮
