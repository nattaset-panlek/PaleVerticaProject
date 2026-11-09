# Lore & Content Route

จุดเริ่มต้นสำหรับเนื้อเรื่อง Lore และ Content ทั้งหมดของ Pale Vertica อัปเดตล่าสุด 2026-11-10

## อยากดูอะไร ไปที่ไหน

| อยากดู | ไปที่ |
|---|---|
| แก่นเรื่อง โลก ลำดับเหตุการณ์ | [World Bible หัวข้อ 1-2](./World_Bible_TH.md) |
| ตัวละครและนามสำคัญ | [World Bible หัวข้อ 3](./World_Bible_TH.md) |
| สถานที่ | [World Bible หัวข้อ 4](./World_Bible_TH.md) |
| กลุ่มอิทธิพล | [World Bible หัวข้อ 5](./World_Bible_TH.md) |
| บทและตอนจบ | [World Bible หัวข้อ 6](./World_Bible_TH.md) |
| Corruption, Shadow Taint และกฎของโลก | [World Bible หัวข้อ 7](./World_Bible_TH.md) |
| จุดที่ยังขัดกันและรอตัดสินใจ | [World Bible หัวข้อ 8](./World_Bible_TH.md) |
| Lore ที่อยู่ในคำบรรยายไอเทม (77 ชิ้น) | [Item Lore Index](./Item_Lore_Index_TH.md) |
| เควสต์ บทสนทนา NPC มอนสเตอร์ที่มีในเกมตอนนี้ | [In-Game Content](./In_Game_Content_TH.md) |

## ไฟล์จริงของ Content

ไฟล์เหล่านี้ต้องอยู่ที่เดิม เพราะเกมโหลดจาก `Resources/Data` ให้แก้ผ่าน **Window > Pale Vertica > Game Data Workbench**

| เนื้อหา | ไฟล์ | คู่มือ |
|---|---|---|
| เควสต์ | `Assets/Resources/Data/Quest.json` | [Quest Data](../02-Technical/Quests/Quest_Data_Walkthrough.md) |
| บทสนทนา | `Assets/Resources/Data/Dialogue.json` | [Dialogue Editor](../02-Technical/Quests/Dialogue_Editor_Walkthrough.md), [NPC Dialogue](../02-Technical/Character/NPC_Dialogue_Walkthrough.md) |
| ตัวละคร NPC มอนสเตอร์ | `Assets/Resources/Data/Character.json` | [Monster Data](../02-Technical/Character/Monster_Data_Walkthrough.md), [Alchemite Monsters](../02-Technical/Character/Alchemite_Monsters_Walkthrough.md) |
| กลุ่มอิทธิพล | `Assets/Resources/Data/Faction.json` | [World Bible หัวข้อ 5](./World_Bible_TH.md) |
| เหตุการณ์ในโลก | `Assets/Resources/Data/Event.json` | [Event System](../02-Technical/World/Event_System_Walkthrough.md) |
| ไอเทมและคำบรรยาย | `Assets/Resources/Data/Item.json`, `ItemSet.json` | [Items](../02-Technical/Data/Items.md) |
| ร้านค้า | `Assets/Resources/Data/Shop.json` | [Shop Data](../02-Technical/Items/Shop_Data_Walkthrough.md) |
| Drop | `Assets/Resources/Data/DropTable.json` | [Loot Drop](../02-Technical/Items/Loot_Drop_Walkthrough.md) |
| สกิล | `Assets/Resources/Data/Skill.json`, `SupportSkill.json`, `UniquePassive.json` | [Active Skills](../02-Technical/Data/Active_Skills.md), [Support Skills](../02-Technical/Data/Support_Skills.md), [Unique Passives](../02-Technical/Data/Unique_Passives.md) |
| Skill Tree | `Assets/Resources/Data/SkillTree.json` | [SkillTree API](../02-Technical/SkillTree/SkillTreeAPI.md) |
| ฉาก | `Assets/Scenes/Town.unity`, `Field.unity`, `Title.unity` | [Game Core](../02-Technical/World/Game_Core_Walkthrough.md) |

## เอกสารต้นทาง

ใช้อ้างอิงเท่านั้น เนื้อเรื่องใหม่ให้เขียนที่ World Bible

| เอกสาร | มีอะไร |
|---|---|
| [GameDesignDoc.md](../01-Design/GameDesignDoc.md) | GDD ระบบเกม และเนื้อเรื่อง v0.1 ท้ายไฟล์ |
| `D:\Games\GDD Tools\Pale_Vertica_GDD.json` | GDD 24 โมดูล เนื้อเรื่องอยู่ที่ `mod_pv_story`, `mod_world`, `mod_entities` อยู่นอก git และบางส่วนเก่าแล้ว ดูวิธีเปิดใน [GDD Tools](../02-Technical/Tools/GDD_Tools_Walkthrough.md) |
| [Map Design Analysis](../01-Design/GAME_PERSPECTIVE_AND_MAP_DESIGN_ANALYSIS.md) | แนวคิดแผนที่ รวมถึงพื้นที่บัลลังก์ซีดจาง |

## กฎของโฟลเดอร์นี้

- เนื้อเรื่องใหม่หรือการเปลี่ยน Lore ให้แก้ `World_Bible_TH.md` ก่อน แล้วค่อยแก้ไฟล์ข้อมูลเกม
- ข้อความในเกมให้แก้ใน Game Data Workbench ไฟล์ `Item_Lore_Index_TH.md` และ `In_Game_Content_TH.md` เป็นเพียงภาพรวม ณ วันที่ระบุ
- ถ้าเจอจุดขัดกัน ให้เพิ่มในหัวข้อ 8 ของ World Bible จนกว่าจะตัดสินใจ
