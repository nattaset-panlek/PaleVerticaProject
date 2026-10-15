# NonPlayerCharacter & AgentCore Integration Walkthrough

เอกสารอธิบายการทำงานร่วมกันระหว่าง `NonPlayerCharacter` และ `AgentCore` ที่ออกแบบมาเพื่อรองรับการทำงานของ NPC, Minion, หรือระบบ Quest ที่ต้องสามารถควบคุมและสั่งการ Entity จากภายนอกได้

---

## 1. การทำงานของ AgentCore ที่ถูกเพิ่มเติม

เพื่อให้ `AgentCore` ซึ่งเป็นระบบ AI พื้นฐานสามารถรับคำสั่งตรงจากภายนอกได้ (ข้ามลูปสายตาและการตัดสินใจปกติของ AI) จึงได้มีการเพิ่ม State ใหม่เข้าไป 2 อย่างคือ:

- `AIState.CommandMove`: สถานะที่ AI จะถูกบังคับให้เดินไปยังพิกัดเป้าหมาย (ใช้ A* Pathfinding หากมี)
- `AIState.CommandFollow`: สถานะที่ AI จะถูกบังคับให้เดินตามเป้าหมาย (Transform) ไปเรื่อยๆ โดยรักษาระยะห่างที่กำหนดไว้

### Public API ที่เชื่อมต่อไว้ใน AgentCore:
- `SetCommandMove(Vector3 destination)`: สั่งให้เดินเป้าหมาย เมื่อถึงที่หมายแล้วจะกลับเป็นสถานะ `Idle`
- `SetCommandFollow(Transform target)`: สั่งให้เดินตามเป้าหมาย หากเป้าหมายขยับหนีก็จะตามไปเรื่อยๆ จนกว่าเป้าหมายจะหายไปหรือถูกยกเลิกคำสั่ง
- `ClearCommand()`: ยกเลิกคำสั่งทั้งหมด และให้ AI กลับเข้าสู่ลูปทำงานปกติ

> [!NOTE] 
> เวลา AI อยู่ในสถานะ `CommandMove` หรือ `CommandFollow` ตัว `AgentCore` จะปิดการแจ้งเตือนและการไล่ล่าชั่วคราว เพื่อป้องกันไม่ให้ NPC วิ่งไปตีมอนสเตอร์ในระหว่างที่ผู้เล่นกำลังทำเควสต์หรือดำเนินคัตซีน

---

## 2. โครงสร้างของ NonPlayerCharacter

สคริปต์ `NonPlayerCharacter.cs` หน้าที่หลักคือการเป็น Entity สำหรับจัดการ NPC สืบทอดมาจากคลาส `Character` พื้นฐาน และบังคับ require `AgentCore` เสมอ ช่วยให้อยู่ในโครงสร้างเดียวกับฝั่ง `PlayerCharacter` และ `EnemyCharacter`

### API และ ฟังก์ชันหลัก:

| คำสั่ง | คำอธิบาย |
|--------|----------|
| `InitializeNPC(string, FactionType)` | ใช้สำหรับตั้งค่า ID เพื่อลิงก์ข้อมูลกับ Database และตั้งฝักฝ่าย (Faction) โดยพื้นฐานคือ Neutral |
| `CommandMoveTo(Vector3)` | สั่งให้ NPC ตัวนี้เดินไปยังพิกัดที่กำหนด |
| `CommandFollow(Transform)` | สั่งให้ NPC เดินตาม Transform ของเกมอ็อบเจ็กต์อื่น เช่น ตามตัว Player |
| `ClearCommands()` | ยกเลิกการสั่งเดินหรือเดินตาม |
| `SetInteractable(bool)` | เปิด/ปิด สถานะการพูดคุยหรือโต้ตอบของ NPC (สามารถนำไปเช็คเงื่อนไขก่อนเปิด UI ได้) |

---

## 3. ตัวอย่างการนำไปใช้งานจริง (Walkthrough)

นี่คือตัวอย่างกรณีใช้งานจริงผ่านระบบ QuestManager หรือ Events:

### กรณี 1: สั่ง NPC เดินนำทางไปที่จุดๆ หนึ่ง
```csharp
public NonPlayerCharacter theGuardNPC;
public Transform destinationPoint;

public void StartEscortQuest()
{
    // บังคับเปลี่ยนฝ่าย ป้องกันมอนสเตอร์โจมตี (ถ้าจำเป็น)
    theGuardNPC.InitializeNPC("Guard_01", FactionType.Player);
    
    // สั่งเดินนำทาง
    theGuardNPC.CommandMoveTo(destinationPoint.position);
}
```

### กรณี 2: ให้ NPC หรือสัตว์เลี้ยงเดินตามผู้เล่น
```csharp
public NonPlayerCharacter petAnimal;
public Transform playerTransform;

public void OnPetSummon()
{
    // ตั้งให้ไม่สามารถโจมตีหรือโต้ตอบคนอื่นได้ และสั่งให้เดินตาม player
    petAnimal.SetInteractable(false);
    petAnimal.CommandFollow(playerTransform);
}

public void OnPetDismiss()
{
    petAnimal.ClearCommands();
    // เพิ่มอนิเมชั่นวิ่งหนี หรือ ลบทิ้ง
}
```

## สรุป
ระบบนี้ถูกออกแบบมาแบบ Decoupled แยกออกมาจาก AI ปกติแต่แชร์คอร์เดียวกัน ทำให้ NPC ยังได้ประโยชน์จาก Pathfinding และ Animation การเดิน ของตัวละครพื้นฐานโดยไม่ต้องเขียนใหม่ทั้งหมด ง่ายต่อการเรียกใช้จากทั้ง Event Graph และ Code โดยตรง

---

## 4. ระบบการจัดตารางเวลาและกิจวัตร (NPC Routine & Schedule)

นอกจากการสั่งการจากภายนอกแบบแมนนวล (Manual Command) แล้ว ระบบนี้ยังมาพร้อมกับ **NPC Routine System** ที่ช่วยให้คุณสร้างตารางชีวิตประจำวันให้ NPC ได้ด้วย

ระบบประกอบด้วย 2 ส่วนหลัก:
1. **GlobalGameTime** (สคริปต์กลางของเกม): เป็นนาฬิกาจำลองของโลก สามารถปรับพิกัดเวลาได้ว่าจะให้เวลาเดินเร็วแค่ไหน (1 วินาทีของจริง = กี่นาทีในเกม) รับผิดชอบแค่ส่ง Event ออกมาว่าเวลาขยับแล้ว
2. **NPCRoutine** (แปะไว้กับตัวละครคู่กับ NonPlayerCharacter): นำรายการตารางเวลามาเช็คกับ GlobalGameTime เมื่อตรงเงื่อนไขจะสั่งทำงานทันทีและทำในฟังก์ชันอัตโนมัติ (เช่น ตัดการ Roam เผื่อสั่งให้ยืนนิ่งเฉยๆ)

### วิธีการรัน Routine
1. ต้องให้แน่ใจว่าใน Scene มี GameObject ที่ใส่สคริปต์ `GlobalGameTime.cs` อยู่ 1 อันเสมอ
2. ในตัวละครที่ต้องการตั้ง Routine ให้ติดตั้งคู่กับสคริปต์ `NPCRoutine.cs`
3. ที่สคริปต์ `NPCRoutine` ให้เพิ่ม Element ในตัวแปร List ของ Daily Schedule
   - ใส่เวลาที่ต้องการ (`Hour`, `Minute`)
   - เลือก Action เช่น ให้เดินไปตลาดยามเช้า (`MoveToPoint`) และลาก Transform ของตำแหน่งตลาดเข้าช่อง Target Point

เพียงเท่านี้เมื่อเวลาเกมถึงจุดที่กำหนด NPC จะตื่นตัวและเปลี่ยนพฤติกรรมเอง!

---

## 5. ระบบ NPC อิสระแบบสมบูรณ์ (Fully Autonomous NPC System - V2 Conceptual)

เพื่อรองรับความต้องการให้ NPC คล้ายคลึงกับ "ผู้เล่นบอท" (Agent) ที่สามารถดูแลจัดการตัวเองหรือถูกสั่งการแบบเจาะจงได้ในหลายระดับ (เช่น พฤติกรรมกดใช้งาน Potion เอง, เก็บจับของ, เปลี่ยนชุดเกราะ, หรือการรับชุดคำสั่งซับซ้อนจากฉาก) ระบบจะถูกอัปเกรดและต่อยอดเป็น 3 โครงสร้างหลัก:

### 5.1 ระบบช่องเก็บของอิสระ (NPC Inventory System)
ปัจจุบัน `AgentCore` และ `NonPlayerCharacter` ตอบสนองเพียงแค่ Event และการโจมตี การยกระดับใน V2 คือการเพิ่ม Component `NPCInventory` เข้าไปประทับในฐานะ `Character`
- **Loot & Gather:** NPC สามารถสแกนและเก็บไอเทมที่ตกอยู่บนพื้น (ItemDrop) เข้ากระเป๋าตนเองได้
- **Resource Usage:** หากค่า HP มุ่งสู่ขีดอันตราย NPC สามารถค้นหา Potion ในกระเป๋าและกด Trigger ใช้งานเองผ่าน `SkillCaster` หรือ `AttributeSystem`
- **Dynamic Equipment:** NPC สามารถทำการเรียก `Equip()` เพื่อสวมใส่อาวุธที่มีดาเมจแรงที่สุดในคลังอาวุธตนเองได้

### 5.2 สมองกลเพื่อการบรรลุเป้าหมาย (NPC Brain / Goal-Oriented AI)
เพื่อให้ NPC เล่นเกมได้ด้วยตัวเอง จะมีสคริปต์ระดับสั่งการเช่น `NPCBrain` (แบบ Goal-Oriented Action Planning หรือ Utility AI) ทำหน้าที่เหนือ `AgentCore`
- **คิดแปลความปรารถนา (Goals):** ประเมินคุณค่าว่าต้องทำอะไรก่อน-หลัง เช่น [ขาดแคลน Gold] > [ค้นหามอนสเตอร์] > [โจมตี] > [ลูทของ]
- **สั่งต่อยอด:** สมองกลระดับสูงจะเป็นฝ่ายเรียก `AgentCore.SetCommandMove()` หรือสั่งเปลี่ยน State เองอัตโนมัติ

### 5.3 ส่วนต่อขยายภายนอกขั้นสูง (Director Management API)
ยกระดับ API ของ `NonPlayerCharacter` ให้ผู้สร้างเกมสามารถยิงคำสั่งบริหารเมืองเข้ามาได้โดยตรง เพื่อใช้รันระบบหลังม่าน:
```csharp
// ตัวอย่าง API ที่ถูกขยายให้ NonPlayerCharacter
public void GiveItemToNPC(int itemId, int amount);
public void ForceEquipItem(EquipmentSlot slot, Equipment newEquip);
public void AssignHighLevelGoal(GoalType newGoal, Transform targetOrArea);
```

**สรุป:**
ด้วยโครงสร้าง V2 นี้ จะพลิกโฉมจากบอทโง่ให้กลายเป็น **Self-Playing Entity** อย่างแท้จริง ตอบโจทย์เกมที่ต้องการโลกที่มีชีวิตซึ่งตัวสคริปต์ต่างๆ สามารถมีวิวัฒนาการหรือเอาชีวิตรอดในระบบสมดุลไอเทมเกมได้ด้วยตัวเอง
