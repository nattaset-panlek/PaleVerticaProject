# ระบบสนับสนุนบัฟจากอุปกรณ์สวมใส่ (Equipment Buff System Support)

ปัจจุบันระบบ **ยังไม่มี** `EquipmentEffect` แบบเฉพาะเจาะจงที่ใช้สำหรับใส่บัฟแบบถาวร (ที่มีไอคอนบัฟแสดงบนหน้าจอ HUD) ทันทีที่ผู้เล่นสวมใส่อุปกรณ์

อย่างไรก็ตาม ตัวเกมมีกลไกที่รองรับผลลัพธ์ในลักษณะเดียวกันโดยไม่เสียทรัพยากร (เช่น มานา หรือ คูลดาวน์) ดังนี้:

## 1. การเพิ่มค่าสถานะโดยตรง (Direct Attribute Bonuses - ไม่มีค่าใช้จ่าย)
* **การทำงาน**: ใช้ผ่าน `AttributeBonusEffect`, `ConditionalBonusEffect`, หรือ `ScalingBonusEffect`
* **พฤติกรรม**: ค่าสถานะจะถูกบวกเพิ่มไปยังหน้าต่างตัวละครโดยตรงเมื่อสวมใส่ และถูกหักออกเมื่อถอดออก
* **ทรัพยากรที่ใช้**: ไม่มี
* **ไอคอนบัฟบนหน้าจอ (HUD)**: ไม่มี

## 2. การสั่งร่ายสกิลบัฟอัตโนมัติ (Triggered Buffs via Auto-Cast - ไม่มีค่าใช้จ่าย/ไม่มีคูลดาวน์)
* **การทำงาน**: ใช้ผ่าน `AutoCastEffect`
* **พฤติกรรม**: สั่งร่ายสกิลโดยอัตโนมัติ (ซึ่งสกิลดังกล่าวสามารถเป็นสกิลบัฟที่แสดงไอคอนบน HUD ได้) เมื่อเกิดเงื่อนไข เช่น `OnHit` (เมื่อโจมตีโดน) หรือ `OnTakeDamage` (เมื่อได้รับความเสียหาย)
* **ทรัพยากรที่ใช้**: ไม่มี (ข้ามการใช้ Mana, Cooldown และ Cast Time ปกติทั้งหมด)
* **ไอคอนบัฟบนหน้าจอ (HUD)**: มี (จะแสดงตามระยะเวลาคงเหลือของบัฟนั้นๆ)

---

## ข้อเสนอแนะ: การออกแบบระบบ Passive Buff บนอุปกรณ์ (Permanent HUD Icons)
หากต้องการให้อุปกรณ์ที่สวมใส่แสดงไอคอนบัฟบนหน้าจอ HUD ตลอดเวลาที่สวมใส่ เราสามารถเพิ่มคลาสผลกระทบใหม่ได้ดังนี้:

```csharp
public class PassiveBuffEffect : EquipmentEffect
{
    public string BuffName;
    public AttributeType Attribute;
    public float Value;
    public Sprite BuffIcon;

    public override void Apply(Character player, ItemInstance instance = null)
    {
        // 1. แอด Modifier เข้าไปในระบบค่าสถานะ
        // 2. ลงทะเบียนบัฟนี้กับ BuffSystem โดยระบุระยะเวลาเป็นอนันต์ (หรือจนกว่าจะโดน Remove)
    }

    public override void Remove(Character player, ItemInstance instance = null)
    {
        // 1. นำ Modifier ออกจากระบบค่าสถานะ
        // 2. สั่งลบบัฟนี้ออกจาก BuffSystem
    }
}
```
