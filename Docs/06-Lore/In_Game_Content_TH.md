# In-Game Content

ภาพรวม Content ที่มีอยู่ในข้อมูลเกมจริง ณ 2026-11-10 ตัวเลขและข้อความอาจเปลี่ยน ให้ถือไฟล์ใน `Assets/Resources/Data/` เป็นหลัก

## เควสต์: สายถนนสายเก่า

| Id | ชื่อ | ผู้ให้ | ส่งที่ | ต้องผ่านก่อน | เป้าหมาย |
|---|---|---|---|---|---|
| 1 | Bones on the Road / กระดูกบนเส้นทาง | Old Man | Old Man | - | กำจัด Skeletom 3 ตัว |
| 2 | A Word with the Smith / คุยกับช่างตีเหล็ก | Old Man | Blacksmith | 1 | คุยกับ Blacksmith |
| 3 | The Old Road / ถนนสายเก่า | Old Man | Old Man | 1, 2 | ถาม Blacksmith, กำจัด Skeletom 2 ตัว, ตอบคำถาม Alchemist, พก Minor Health Potion, กำจัด Skeletom Captain, รายงาน Old Man (ทำตามลำดับ) |

**เรื่องย่อ:** ถนนไม่ได้อันตรายเพราะโครงกระดูกเดินกันเอง มีผู้นำอยู่เบื้องหลัง ผู้เล่นตามเบาะแสจาก Old Man ไปยัง Blacksmith และ Alchemist จนกำจัด Skeletom Captain ได้

**เหตุการณ์ที่ผูกกัน:** Event 1 `Old Man's Gift` ทำงานตามความคืบหน้าของเควสต์ 3 และให้ไอเทม 2001

## บทสนทนา

| Id | ใช้กับ | ประโยคเปิด |
|---|---|---|
| 1 | Alchemist | Need a potion, traveler? |
| 2 | Blacksmith | Bring me steel and I will make it sing. |
| 3 | Shopkeeper | Fair prices, mostly. |
| 4 | Old Man | These roads were safer once. |
| 101 | รับเควสต์ 3 | You cleared the bones. Good. |
| 102 | เควสต์ 3, ถาม Blacksmith | Bones do not walk on their own. |
| 103 | เควสต์ 3, ตอบ Alchemist | Before I help: what stops a wound from bleeding? |
| 104 | เควสต์ 3, รายงาน Old Man | The captain is gone? |
| 105 | ส่งเควสต์ 3 | Take this. You earned it. |

## ตัวละคร

| กลุ่ม | รายการ |
|---|---|
| NPC (Faction Neutral) | 2001 Alchemist, 2002 Blacksmith, 2003 Shopkeeper, 2004 Old Man |
| Skeletom | 1001 Skeletom, 1002 Archer, 1003 Captain, 1004 Scout, 1005 Warrior, 1006 Guard, 1007 Brute, 1008 Marksman |
| Alchemite | 1009 Slime, 1010 Gas, 1011 Golem |

## อื่นๆ

| ประเภท | ตอนนี้มี |
|---|---|
| ไอเทม | 202 ชิ้น ในจำนวนนี้ 77 ชิ้นมี Lore ดู [Item Lore Index](./Item_Lore_Index_TH.md) |
| กลุ่มอิทธิพล | 13 กลุ่ม ดู [World Bible](./World_Bible_TH.md) |
| ร้านค้า | `general_merchant` 8 รายการ |
| Drop Table | 3 ตาราง |
| ฉากเล่นจริง | Title, Town, Field |

ยังไม่มีเนื้อหาของ Prologue, Chapter 1-3, Final Chapter และตัวละครหลักในข้อมูลเกม
