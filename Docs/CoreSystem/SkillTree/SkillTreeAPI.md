# SkillTree System — API Reference

> **สร้างขึ้น:** 2026-04-04
> **ไฟล์ JSON ต้นทาง:** `Assets/Resources/Data/SkillTree.json`
> **Namespace:** `CoreSystem.SkillTree`

---

## 📁 โครงสร้าง Folder

```
Assets/Script/CoreSystem/SkillTree/
├── Models/
│   └── SkillTreeModels.cs      — Data classes (SkillTreeNode, SkillTreeEdge, SkillTreeStats)
├── SkillTreeLoader.cs          — อ่านและพาร์ส JSON (ใช้ Newtonsoft.Json)
├── SkillTreeDatabase.cs        — Static database + logic ทั้งหมด
└── SkillTreeManager.cs         — MonoBehaviour Singleton สำหรับ Unity Scene
```

---

## 🗂 Data Models (`SkillTreeModels.cs`)

### `SkillTreeNode`
| Field | Type | คำอธิบาย |
|-------|------|-----------|
| `id` | `string` | รหัสไม่ซ้ำ เช่น `"n44_309"` |
| `label` | `string` | ชื่อโหนด เช่น `"Attribute Lucky"` |
| `kind` | `string` | `"active"` หรือ `"passive"` |
| `role` | `string` | `"base"` = จุดอาชีพ / `"normal"` = ทั่วไป |
| `x`, `y` | `float` | ตำแหน่งบน canvas |
| `isActive` | `bool` | ผู้เล่นอัปแล้วหรือไม่ |
| `shape` | `string` | `"circle"` / `"rect"` / `"poly"` |
| `polySides` | `int` | จำนวนด้าน (ใช้กับ shape = poly) |
| `iconPath` | `string` | Path ไอคอนใน Resources |
| `extras` | `Dictionary<string, float>` | Stat bonus เช่น `{"LUK": 1}` |
| `Position` *(property)* | `Vector2` | แปลง x,y → Vector2 |
| `IsBaseClass` *(property)* | `bool` | ตรวจว่าเป็นจุดอาชีพ |
| `HasStats` *(property)* | `bool` | มี stat bonus ไหม |

### `SkillTreeEdge`
| Field | Type | คำอธิบาย |
|-------|------|-----------|
| `id` | `string` | รหัส Edge เช่น `"e977"` |
| `from` | `string` | id ของ Node ต้นทาง |
| `to` | `string` | id ของ Node ปลายทาง |
| `style` | `string` | `"curve"` / `"straight"` |

### `SkillTreeStats`
| Field | Type | คำอธิบาย |
|-------|------|-----------|
| `STR`, `AGI`, `INT`, `VIT`, `DEX`, `LUK` | `float` | รวม stat จากโหนดที่ active |
| `RawStats` | `Dictionary<string, float>` | รวมทุก stat key รวมถึง custom key |

---

## ⚙️ SkillTreeDatabase API (Static)

### Initialization
```csharp
// โหลดข้อมูล — เรียกอัตโมัติจาก Database.Initialize()
SkillTreeDatabase.Initialize();

// รีโหลดใหม่ (เช่นตอน Hot-Reload ใน Editor)
SkillTreeDatabase.Reload();
```

### Node API
```csharp
// ดึง Node จาก id
SkillTreeNode node = SkillTreeDatabase.GetNode("n44_309");

// ดึงทุก Node
List<SkillTreeNode> all = SkillTreeDatabase.GetAllNodes();

// ดึงเฉพาะจุดอาชีพ (role = "base")
List<SkillTreeNode> classes = SkillTreeDatabase.GetBaseClassNodes();

// ดึงโหนดที่ผู้เล่น unlock ไว้แล้ว
List<SkillTreeNode> active = SkillTreeDatabase.GetActiveNodes();

// ค้นหาจากชื่อ (ไม่ case-sensitive)
List<SkillTreeNode> found = SkillTreeDatabase.FindNodesByLabel("Attribute Lucky");

// ดึงโหนดที่มี stat bonus
List<SkillTreeNode> statNodes = SkillTreeDatabase.GetStatNodes();
```

### Edge / Connection API
```csharp
// ดึงทุก Edge
List<SkillTreeEdge> edges = SkillTreeDatabase.GetAllEdges();

// ดึง Edge ที่ออกจากโหนดนี้
List<SkillTreeEdge> out = SkillTreeDatabase.GetEdgesFrom("n44_309");

// ดึง id ของโหนดที่เชื่อมต่อออกไป
List<string> neighbors = SkillTreeDatabase.GetNeighborIds("n44_309");

// ตรวจว่า unlock ได้ไหม (prerequisite ต้องครบ)
bool canUnlock = SkillTreeDatabase.CanUnlock("n44_309");
```

### Stats API
```csharp
// คำนวณ stats จากโหนดที่ isActive ทั้งหมด
SkillTreeStats stats = SkillTreeDatabase.GetActiveStats();
Debug.Log(stats.STR);   // ค่า STR รวม
Debug.Log(stats.LUK);   // ค่า LUK รวม

// คำนวณ stats จาก node ids ที่กำหนดเอง (สำหรับ preview/planner)
var preview = SkillTreeDatabase.GetStatsForNodes(new[] { "n44_309", "n45_581" });
```

### Unlock / Reset API
```csharp
// ตั้งสถานะ active ของโหนด (Runtime state)
SkillTreeDatabase.SetNodeActive("n44_309", true);

// รีเซ็ตโหนดทั้งหมด
SkillTreeDatabase.ResetAllNodes();

// โหลด save data กลับมา (หลัง Load Game)
SkillTreeDatabase.ApplySaveData(activeNodeIds);

// ดึงรายการ active ids สำหรับบันทึก
List<string> toSave = SkillTreeDatabase.GetSaveData();
```

---

## 🕹 SkillTreeManager API (MonoBehaviour)

> วาง `SkillTreeManager` บน GameObject ใน Scene หรือใน Database prefab

### Node
```csharp
SkillTreeNode n    = SkillTreeManager.Instance.GetNode("n44_309");
List<SkillTreeNode> all     = SkillTreeManager.Instance.GetAllNodes();
List<SkillTreeNode> classes = SkillTreeManager.Instance.GetBaseClassNodes();
List<SkillTreeNode> active  = SkillTreeManager.Instance.GetActiveNodes();
List<SkillTreeNode> found   = SkillTreeManager.Instance.FindNodes("Lucky");
```

### Connection
```csharp
bool ok         = SkillTreeManager.Instance.CanUnlock("n44_309");
List<string> nb = SkillTreeManager.Instance.GetNeighbors("n44_309");
```

### Unlock / Reset
```csharp
// Unlock โหนด (ตรวจ prerequisite + SP อัตโนมัติ)
bool success = SkillTreeManager.Instance.TryUnlockNode("n44_309");

// รีเซ็ตโหนดเดียว (คืน SP)
SkillTreeManager.Instance.TryResetNode("n44_309");

// รีเซ็ตทุกโหนด (คืน SP ทั้งหมด)
SkillTreeManager.Instance.ResetAll();
```

### Stats
```csharp
SkillTreeStats stats = SkillTreeManager.Instance.GetCurrentStats();
// นำ stats ไป apply บน PlayerData
playerData.BonusSTR += (int)stats.STR;
playerData.BonusLUK += (int)stats.LUK;
```

### Skill Points
```csharp
SkillTreeManager.Instance.SkillPoints = 10;       // ตั้งค่า
SkillTreeManager.Instance.AddSkillPoints(5);      // เพิ่ม
int sp = SkillTreeManager.Instance.SkillPoints;   // อ่าน
```

### Save / Load
```csharp
// ก่อน Save Game
List<string> saveData = SkillTreeManager.Instance.GetSaveData();
PlayerPrefs.SetString("SkillTreeSave", JsonUtility.ToJson(saveData));

// หลัง Load Game
SkillTreeManager.Instance.LoadSaveData(activeNodeIds, skillPoints);
```

### Events
```csharp
// Subscribe ไว้ใน UI Script
SkillTreeManager.OnNodeUnlocked += (id) => RefreshNodeUI(id);
SkillTreeManager.OnNodeReset    += (id) => RefreshNodeUI(id);
SkillTreeManager.OnAllReset     += () => RefreshAllUI();
```

---

## 🔗 Database Static Shortcuts

เรียกผ่าน `Database` (Global entry point) ได้เลย:

```csharp
// ดึง Node จาก global Database
SkillTreeNode node  = Database.GetSkillTreeNode("n44_309");
var nodes           = Database.GetSkillTreeNodes();
SkillTreeStats stat = Database.GetSkillTreeStats();
bool canUnlock      = Database.CanUnlockSkillTreeNode("n44_309");
```

---

## 💡 ตัวอย่างการใช้งานจริง

### spawn UI Node อัตโนมัติจากข้อมูล JSON
```csharp
void BuildSkillTreeUI()
{
    foreach (var node in SkillTreeDatabase.GetAllNodes())
    {
        var go = Instantiate(nodeUIPrefab, skillTreeRoot);
        var rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = node.Position;   // x,y จาก JSON ตรงเลย

        var btn = go.GetComponent<SkillNodeButton>();
        btn.Setup(node);
    }
}
```

### apply ค่า stat จาก SkillTree ให้ตัวละคร
```csharp
void ApplySkillTreeBonus(PlayerData player)
{
    var stats = SkillTreeDatabase.GetActiveStats();
    player.STR += (int)stats.STR;
    player.AGI += (int)stats.AGI;
    player.INT += (int)stats.INT;
    player.VIT += (int)stats.VIT;
    player.DEX += (int)stats.DEX;
    player.LUK += (int)stats.LUK;
}
```

### ตรวจสอบก่อน unlock ใน UI
```csharp
void OnNodeButtonClicked(string nodeId)
{
    if (SkillTreeManager.Instance.TryUnlockNode(nodeId))
    {
        // อัปเดต UI สำเร็จ
    }
    else
    {
        ShowToast("ไม่สามารถ unlock ได้: prerequisite หรือ SP ไม่เพียงพอ");
    }
}
```

---

## 📝 หมายเหตุ

- ระบบใช้ **Newtonsoft.Json** เพื่อรองรับ `Dictionary<string, float>` ใน `extras` (JsonUtility ไม่รองรับ Dictionary)
- `SkillTreeDatabase` เป็น **static class** ไม่ต้องมี MonoBehaviour — เรียกได้จากทุกที่
- `isActive` ใน runtime เป็น **in-memory state** ต้อง save/load ผ่าน `GetSaveData()` / `ApplySaveData()` เอง
- ข้อมูล JSON จะถูกโหลดครั้งแรกอัตโนมัติเมื่อ `Database.Initialize()` ถูกเรียก
