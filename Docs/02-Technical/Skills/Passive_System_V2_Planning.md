# Passive Skill System V2: Gameplay Pipeline & Interceptors

## 1. การวิเคราะห์ระบบปัจจุบัน (Current Architecture Analysis)

จากการตรวจสอบโคดปัจจุบัน ระบบการคำนยณ Damage และ Heal ถูกเขียนไว้ที่ **`DamageEffect.cs`** และ **`HealEffect.cs`** โดยมีการทำงานเป็นเส้นตรงดังนี้:
1. รับ `SkillPayload`
2. คำนวณตัวคูณ (Element, Size, Race, Critical)
3. หักลบ Defense
4. **แก้ไขลบ/บวกค่าใน `AttributeSystem` โดยตรง**
5. ส่ง Event `TriggerOnHit` / `TriggerOnTakeDamage` (เป็น Event ที่เกิดขึ้นหลังจากหักล้างค่าไปแล้ว)

### ปัญหาที่พบในการทำ Passive ขั้นสูง:
- **Passive "ดูดเลือด":** ต้องรู้ดาเมจสุดท้ายก่อนหัก DEF เพื่อนำมา Heal ตัวเอง (ปัจจุบันทำได้ยากเพราะ Logic กระจัดกระจาย)
- **Passive "เปลี่ยน Heal HP เป็น Mana":** ปัจจุบัน `HealEffect` แก้ไขค่า HP โดยตรง ไม่มีการเปิดช่องให้ใครมาขอแก้ไขกลางทาง (Intercept)
- **Passive "Damage to Mana first":** ต้องสามารถดักดาเมจที่กำลังจะหัก HP แล้วย้ายไปหัก MP แทน

---

## 2. การออกแบบระบบใหม่ (Proposed Design: Gameplay Pipeline)

เราจะใช้รูปแบบ **Interceptor Pattern** หรือ **Middleware** เพื่อให้การทำงานยืดหยุ่นและรองรับ Passive ที่ "เปลี่ยนกฎเกม" ได้ในอนาคต

### โครงสร้างหลัก (Core Components)

1.  **PassiveInstance (Base Class):**
    เป็น ScriptableObject หรือ Class พื้นฐานที่ Passive ทุกตัวต้องสืบทอด โดยจะมี "Hook" สำหรับเหตุการณ์ต่างๆ เช่น:
    - `OnProcessDamageDealt(DamageContext context)`: ก่อนทำดาเมจใส่คนอื่น (ใช้ทำ Life Steal)
    - `OnProcessDamageReceived(DamageContext context)`: ก่อนโดนดาเมจเอง (ใช้ทำ Damage to Mana)
    - `OnProcessHealReceived(HealContext context)`: ก่อนได้รับการเพิ่มเลือด (ใช้เปลี่ยน HP -> MP)

2.  **GameplayContext (Data Bundles):**
    สร้าง Class สำหรับถือข้อมูลที่สามารถ "แก้ไขได้" ในระหว่างทาง
    - `DamageContext`: [Caster, Target, RawDamage, FinalDamage, DamageType, IsModified, etc.]
    - `HealContext`: [Caster, Target, Amount, AttributeType, etc.]

3.  **CharacterPassiveManager:**
    Component ที่ติดอยู่กับ `Character` ทำหน้าที่:
    - เก็บรายการ Passive ที่ "ติดตั้ง" อยู่ (Equipped)
    - มีฟังก์ชัน `RunPipeline(Context context)` เพื่อรัน Hook ของ Passive ทุกตัวตามลำดับ Priority

---

## 3. ตัวอย่างตรรกะการทำงาน (Logic Examples)

### กรณี: Passive "ดาเมจเข้ามาที่ Mana ก่อน (Damage to Mana)"
1. `DamageEffect` คำนวณ FinalDamage ได้ 100 หน่วย
2. `DamageEffect` เรียก `target.PassiveManager.RunPipeline(context)`
3. Passive 'ManaShield' ทำงาน:
   - ตรวจสอบว่า target มี MP หรือไม่
   - หักลบ `context.FinalDamage` ออกไป 50 (ย้ายไปหัก MP ของ target แทน)
   - แก้ไข `context.FinalDamage` เหลือ 50 หน่วย
4. `DamageEffect` ทำงานต่อโดยหัก HP ของ target แค่ 50 หน่วย

### กรณี: Passive "เปลี่ยนการเพิ่มเลือดทุกอย่างให้เป็น Mana"
1. `HealEffect` กำลังจะเพิ่มเลือด 200 หน่วย (TargetAttribute = HP)
2. เรียก `target.PassiveManager.RunPipeline(healContext)`
3. Passive 'SoulConverter' ทำงาน:
   - เปลี่ยน `healContext.TargetAttribute` จาก **HP** เป็น **MP**
4. `HealEffect` ทำงานต่อโดยเพิ่ม Mana ให้ target แทนเลือด

---

## 4. สิ่งที่ต้องแก้ไขและจุดที่ควรระวัง (Impact & Risks)

### จุดที่ต้องแก้ไข (Modifications Required):
- **Character.cs:** เพิ่มรายการ `List<PassiveInstance> activePassives` และระบบ Equip/Unequip
- **DamageEffect.cs / HealEffect.cs:** ฝังจุดเรียก (Injection Point) สำหรับ Pipeline ก่อนที่จะมีการเข้าถึง Attribute จริงๆ
- **IEntity.cs:** อาจต้องเพิ่ม Property ให้เข้าถึง PassiveManager ได้ง่ายขึ้น

### จุดที่ควรระวัง (Critical Risks):
- **Order of Effects (Priority):** ถ้ามี Passive สองตัว ตัวแรกบอก "Damage x2" ตัวที่สองบอก "Damage to Mana" ลำดับการคำนวณสำคัญมาก (ควรใช้รหัส Priority 0-100)
- **Infinite Loops:** Passive ดูดเลือด (ดาเมจ -> ฮีล) ไปเจอ Passive เปลี่ยนฮีลเป็นดาเมจ (ฮีล -> ดาเมจ) อาจเกิดลูปไม่รู้จบ ต้องมีระบบป้องกัน (Flags เช่น `context.IsProcessed`)
- **Performance:** การรัน Pipeline ทุกครั้งที่ตีหรือฮีลอาจกระทบ Frame rate หากมี Passive จำนวนมหาศาล (ควรใช้แคชหรือสแกนเฉพาะตัวที่มี Hook นั้นๆ)

---

## 5. แผนการดำเนินงาน (Next Steps)

1. สร้าง **Hook Interface** และ **Context Classes**
2. สร้าง **Base Passive Class** (ScriptableObject)
3. เพิ่ม **Passive Manager** ลงใน Character
4. ปรับปรุง **Effect System** ให้รองรับการ Intercept
5. สร้างตัวอย่าง Passive พิลึกๆ ตามที่คุณต้องการเพื่อทดสอบระบบ
