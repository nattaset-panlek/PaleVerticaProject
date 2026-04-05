# การนำไฟล์ JSON สกิลทรี (Skill Tree) ไปใช้งานต่อ

เมื่อคุณสร้างและออกแบบสกิลทรีเสร็จเรียบร้อยผ่าน **Skill Tree Editor** และทำการ Export ข้อมูลออกมาเป็นไฟล์ `.json` แล้ว คุณสามารถนำไฟล์ข้อมูลนี้ไปใช้งานต่อยอดในโปรเจกต์ต่าง ๆ ได้ทันที บทความนี้นำเสนอแนวทางการนำไฟล์ JSON ดังกล่าวไปวิเคราะห์และใช้ประโยชน์

---

## 1. วิเคราะห์และทำความเข้าใจโครงสร้าง JSON ที่ได้จากการ Export

ไฟล์ JSON จะประกอบไปด้วยอ็อบเจ็กต์หลัก (ตามตัวอย่างที่มี) เช่น:

- **`nodes`**: รายการของจุดสกิลทั้งหมด ประกอบด้วยข้อมูลเช่น รหัส (`id`), ชื่อสกิล (`label`), ตำแหน่งการจัดวางบนหน้าจอ (`x`, `y`), ไฟล์ไอคอน (`iconPath`) และค่าพลังต่างๆ ที่ซ่อนอยู่ด้านใน (`extras`)
- **`links`** (ถ้ามีในระบบ): รายการของการเชื่อมต่อ (Edges) ระหว่างโหนด ว่าสกิลไหนต้องอัปเป็นทางผ่านเพื่อไปสู่อีกสกิลหนึ่ง

**ตัวอย่างข้อมูลของหนึ่งโหนด (`node`):**
```json
{
  "id": "n44_309",
  "label": "Attribute Lucky",
  "kind": "active",
  "role": "normal",
  "x": 621,
  "y": 854,
  "isActive": false,
  "iconPath": "SkillIcon/Icon_Attribute.png",
  "extras": {
    "LUK": 1
  }
}
```

---

## 2. แนวทางการนำไฟล์ไปใช้งานต่อ

เมื่อเราเข้าใจโครงสร้างแล้ว นี่คือวิธีที่คุณสามารถนำเอาฐานข้อมูล JSON นี้ไปใช้งาน:

### 2.1 นำไปประยุกต์ใช้กับ Game Engine (Unity, Unreal Engine, Godot) 🎮
นี่คือท่ามาตรฐานที่สุด คุณสามารถเขียนโค้ดในเอนจินเกมเพื่อดึงไฟล์ JSON ตัวนี้เข้าไปอ่านแบบ Runtime
- **การสร้างหน้าต่าง UI ข้อมูลสกิล (Skill Tree UI):** ใช้ค่า `x` และ `y` สร้างปุ่มหรือไอคอนสกิลลงบนหน้าจอให้อัตโนมัติในตำแหน่งที่ถูกต้องเป๊ะ ไม่ต้องไปนั่งจัด UI เองในเอนจินเกมใหม่ทั้งหมด
- **การจัดการ Logic และเงื่อนไขการอัปสกิล:** ตรวจสอบจากโครงสร้างการเชื่อมต่อ หากผู้เล่นต้องการอัปสกิลที่ 2 ตัวเกมสามารถอ่าน JSON มาตรวจสอบย้อนหลังได้ว่า สกิลที่ 1 หรือเงื่อนไขก่อนหน้าถูกอัปไปแล้วหรือยัง
- **การนำค่าไปประยุกต์ใช้จริง:** ทุกครั้งที่ผู้เล่นอัปสกิล ตัวเกมให้อ่านค่าใน `extras` (เช่น `{"LUK": 1, "STR": 2}`) แล้วนำไปบวกเพิ่มลงในค่าสถานะ (Stats) หลักของตัวละครแบบไดนามิก

### 2.2 นำไปสร้างเว็บไซต์ Build Planner / คาแคลคูเลเตอร์สกิลทรี 🌐
เอาใจสายวิเคราะห์เกมเพลย์ด้วยการทำเว็บแอปพลิเคชัน
- สร้างหน้าเว็บให้ผู้เล่นเข้ามากดจำลองลองจัดสายสกิลล่วงหน้า
- ระบบสคริปต์บนเว็บ (เช่น JavaScript/React/Vue) จะทำการอ่าน JSON ขึ้นมา วาดรูปสกิลทรี และรวมผลการคำนวณทั้งหมดว่าได้ Status / Buff รวมทั้งหมดเท่าไร
- ทำให้ทุกครั้งที่มีการปรับอัปเดตบาลานซ์สกิลในเกมจาก Editor ตัวเว็บเหล่านี้นำไฟล์ JSON อัปเดตใหม่ไปวาง ระบบทุกอย่างก็อัปเดตตามทันที

### 2.3 สร้างหน้าวิกิเกม (Wiki / Guide / Knowledge Base) 📚
นำข้อมูลไปเขียนเป็นหน้าเว็บวิกิแบบอัตโนมัติ
- อ่านข้อมูลชื่อ (`label`) และผลประโยชน์ที่ได้ (`extras`) วนลูปและพ่นออกมาเป็นรูปแบบตาราง
- ช่วยให้ผู้เล่นเห็นข้อมูลสกิลทั้งหมดหรือเงื่อนไขต่างๆ อย่างละเอียดครบถ้วน โดยที่ไม่ต้องเข้าไปดูทีละอันในเกมให้เสียเวลา

### 2.4 สร้างสคริปต์ทดสอบสมดุล (Balancing Tooling) ⚖️
เขียนโปรแกรมง่ายๆ (เช่น Node.js หรือ Python) เพื่อใช้อ่านไฟล์ช่วยเช็คบาลานซ์เกมแทนคน
- สมมติให้สคริปต์ลองสมมติว่าอัปทุกสกิลในสายอาชีพ Mage ดูซิ แล้วดูว่าค่าสถานะ INT รวมแล้วทะลุเพดานหรือไม่ มีทักษะไหนให้โบนัสมากเกินเหตุไหม
- เช็คว่าโครงสร้างสกิลมีปัญหาไหม เช่น มี Node ไหนลอยๆ ที่ผู้เล่นไม่สามารถเชื่อมเข้ามาได้ (Unreachable Nodes) หรือไม่

---

## 3. ตัวอย่างโค้ดเบื้องต้นสำหรับการนำไปใช้ (Code Snippet)

### ตัวอย่าง: การอ่าน JSON ด้วย JavaScript หาผลรวม Status (ทำเว็บ Build Planner)
```javascript
// สมมติเพื่อจำลองว่าโหลดข้อมูลมาจาก SkillTree.json แล้วมีการเลือก Active
const skillData = require('./SkillTree.json');

function calculateTotalActiveStats(nodes) {
    let totalStats = { STR: 0, AGI: 0, INT: 0, VIT: 0, DEX: 0, LUK: 0 };

    nodes.forEach(node => {
        // จำกัดสิทธิไว้ให้คำนวณเฉพาะโหนดที่ผู้เล่นอัปไว้ (isActive: true)
        if (node.isActive && node.extras) {
            // ดึง Key/Value มาทำการบวกค่า
            for (const [statName, value] of Object.entries(node.extras)) {
                if (totalStats[statName] !== undefined) {
                    totalStats[statName] += value;
                } else {
                    totalStats[statName] = value;
                }
            }
        }
    });

    return totalStats;
}

const finalStats = calculateTotalActiveStats(skillData.nodes);
console.log("พลังตัวละครรวม ณ ปัจจุบันจากสกิลทรีทั้งหมด:", finalStats);
```

### ตัวอย่าง: การตั้ง Data Model เตรียมพร้อมเชื่อมใน Unity (C#)
ใช้ Library ทั่วไปเช่น `Newtonsoft.Json` เข้ามาช่วยแปลงสตริงเป็น Object ของตัวเกม:

```csharp
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

// Model ในส่วนของการแสดงข้อมูล Node
[System.Serializable]
public class SkillNode 
{
    public string id;
    public string label;
    public string kind;
    public string role;
    public float x;
    public float y;
    public bool isActive;
    public string iconPath;
    // ใช้ Dictionary ในการเก็บ extras เพื่อความยืดหยุ่นของจำนวน Status
    public Dictionary<string, float> extras;
}

// Model หลักสำหรับดึงข้อมูล Array 
[System.Serializable]
public class SkillTreeData 
{
    public List<SkillNode> nodes;
    // หากมีการ export Links เพิ่มมาก็สามารถเพิ่มเข้าไปได้ 
    // public List<SkillLink> links; 
}

public class SkillTreeLoader : MonoBehaviour
{
    // ใส่ไฟล์ SkillTree.json ที่ได้เข้าจาก Editor ลากใส่มาที่นี่ผ่าน Inspector
    public TextAsset jsonFile; 

    void Start()
    {
        // ทำการ Deserialize แจกแจงเข้าหา Model 
        SkillTreeData data = JsonConvert.DeserializeObject<SkillTreeData>(jsonFile.text);
        
        foreach(var node in data.nodes)
        {
            Debug.Log($"พบสกิล: {node.label} ลอยอยู่ที่ตำแหน่งพิกัดระดับ UI ที่แกน X:{node.x}, Y:{node.y}");
            // สามารถเขียนโค้ดให้ Spawn UI Prefab ขึ้นมาตามพิกัดนี้ได้เลย
        }
    }
}
```

---

**สรุป**: มองได้ว่าไฟล์ **SkillTree.json** ที่ Export ออกมาคือ **"ฐานข้อมูลของระบบอัปเกรดตัวละคร"** แทนที่เราจะกำหนดทุกอย่างลงในโค้ดแบบ Hard-code เราสามารถออกแบบผ่านหน้าเว็บไซต์แล้วนำจุดพิกัด, ชื่อ, ค่าพลัง, เงื่อนไข มาคำนวณป้อนค่าผ่านการเขียน "สคริปต์ตัวอ่าน (Parser)" ของเรานั่นเอง!
