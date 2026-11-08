# Pale Vertica: Action Plan & Implementation Checklist

- วันที่จัดทำ: 2026-11-08
- สถานะ: แผนปฏิบัติการหลัก (ต่อยอดจาก Handoff 2026-11-08)
- เอกสารอ้างอิง: `Docs/03-Project/Handoff_2026-11-08.md`, `Docs/02-Technical/Combat/Combat_Core_Pipeline.md`

---

## Phase A: เคลียร์งานค้างและจัดระเบียบ Git (Handoff Tasks)

ก่อนเริ่มงานใหม่ ต้องสะสางการเปลี่ยนแปลงที่ค้างอยู่ ~920 ไฟล์ และยืนยันความถูกต้องใน Play Mode

- [x] **A1. ทดสอบ Play Mode ยืนยันงานที่เสร็จแล้ว**
  - [x] ตรวจสอบการซูมกล้อง: หมุนล้อเมาส์เหนือ UI ต้องไม่ทำให้กล้องหลักซูมเข้า/ออก
  - [x] ตรวจสอบ Hotbar: โหลดเซฟแล้วตัวเลขไอคอนสกิล/ไอเทมต้องแสดงถูกต้อง (`HotbarSaveMappingTests`)
  - [x] ตรวจสอบ Inspect Modal: แสดงข้อมูล Vitals จาก `Status Vitals.prefab` ถูกต้อง
  - [x] ตรวจสอบระบบร้านค้าและ ItemSlot: ไอเทมประเภท Equipment ในร้านค้าต้องไม่ Stack และแยกการ์ดรายชิ้น
- [x] **A2. ทยอย Commit งานแยกตามหมวดหมู่**
  - [x] Commit งานกล้อง: `CameraController.cs`, `CameraZoomFormula.cs`, `CameraZoomFormulaTests.cs`
  - [x] Commit งาน Hotbar และ Inspect Modal
  - [x] Commit งานระบบร้านค้าและ ItemSlot
  - [x] Commit งาน MapLab Map Editor ใน Repo แยก (`Assets/ThirdParty/MapLab`)
- [x] **A3. ป้องกันข้อมูล NPC สูญหายใน `Character.json`**
  - [x] ปิดหน้าต่าง Game Data Workbench และเปิดใหม่ก่อนกด Save เพื่อโหลด NPC Id 2001-2004 เข้า Memory
- [x] **A4. ตัดสินใจโครงสร้าง Git ของ MapLab**
  - [x] เลือกว่าจะผูก MapLab เป็น Git Submodule หรือคง `.gitignore` ไว้

---

## Phase B: ซ่อมแซมระบบสกิลและการต่อสู้ (Skill & Combat Stabilization)

แก้ปัญหาระบบต่อสู้และสกิลที่พังอยู่ เพื่อให้พร้อมรองรับการต่อสู้จริง

- [x] **B1. ซ่อมแซม Active Skills ที่พัง (6 จาก 24 สกิล)**
  - [x] สำรวจและระบุรายชื่อ 6 Active Skills ที่เรียกใช้ไม่ได้
  - [x] แก้ไขตรรกะและ Payload ให้ทำงานร่วมกับ `SkillCaster` และ `CombatCore` ได้สมบูรณ์
  - [x] กู้คืนสกิล Summon Valkyrie ให้สามารถสร้าง Entity ผู้ช่วยออกมาในฉากได้
- [x] **B2. เชื่อมต่อ Support Skills ที่ยังไม่มีผล (11 จาก 22 สกิล)**
  - [x] ตรวจสอบรายชื่อ 11 Support Modifiers ที่ยังไม่มี Implementation ในเกม
  - [x] เขียนตรรกะปรับแต่ง Payload ผ่าน `IPayloadSplitter`, `ICastRepeater` หรือ Hook ใน `CombatCore`
- [x] **B3. ปรับสมดุล Spell Cascade และความเสียหายต่อเนื่อง**
  - [x] ปรับแก้ `DamageMultiplier` ของ Spell Cascade ให้สเกลตัวคูณความเสียหายถูกต้อง
- [x] **B4. สรุปข้อยุติเรื่องกฎ Fixed Cast Time (FCT)**
  - [x] ข้อยุติ: FCT ลดได้เฉพาะผ่าน Attribute Reduce FCT ซึ่งเป็น Rare Attribute Bonus (`FixedCastTimeReduc`, `FixedCastTimeReducPercent`) โค้ด `CastTimeFormula` ตรงกับกฎนี้แล้ว

---

## Phase C: เติมเต็ม Core Gameplay Loop ที่ยังขาด (Missing Core Systems)

สร้างจิ๊กซอว์ระบบที่จำเป็นสำหรับการเล่นแบบครบวงจร

- [x] **C1. ระบบไอเทมตกบนพื้น (Loot Drop & Ground Pickup)**
  - [x] สร้าง Drop Table Schema เชื่อมโยงกับมอนสเตอร์เมื่อตาย
  - [x] สร้าง World Item Entity (ภาพไอเทมบนพื้น + Collider/Trigger)
  - [x] เพิ่มระบบ Interaction คลิกหรือเดินเหยียบเพื่อเก็บของเข้า `PlayerInventory`
- [x] **C2. ท่อ CombatCore Phase 3 (Heal, Buff & Status Ailments)**
  - [x] สร้าง `HealSpec` และ `HealHandler` ให้อยู่ใน `CombatCore` แบบเดียวกับ `DamageHandler`
  - [x] สร้างระบบจัดการสถานะผิดปกติพื้นฐาน (Stun, Poison, Freeze, Slow)
  - [x] ปรับปรุงไอเทมประเภทยาฟื้นฟู (Potions) ให้ส่งผ่านท่อ CombatCore
- [x] **C3. ระบบกล่องข้อความบทสนทนา (NPC Dialogue System)**
  - [x] ขยาย `InteractionCore` ให้มี Option ประเภทพูดคุย (Talk)
  - [x] ทำหน้าต่าง UI Dialogue Box แสดงข้อความและตัวเลือกตอบคำถาม
- [x] **C4. ระบบเชื่อมต่อฉากและการเกิดของศัตรู (Scene Portal & Spawner)**
  - [x] สร้าง Trigger สำหรับวาร์ปข้ามฉาก (เช่น จากเมืองไปยังฟิลด์ต่อสู้)
  - [x] ปรับปรุง Spawner Zone ควบคุมจำนวนและระยะเวลาการเกิดใหม่ (Respawn) ของมอนสเตอร์

---

## Phase D: ทดสอบ Vertical Slice (1 ลูปเกมเพลย์สมบูรณ์)

นำระบบทั้งหมดมาประกอบและทดสอบการเล่นจริง 1 พื้นที่

- [x] **D1. จัดฉาก Sandbox Prototype (1 เมือง + 1 ฟิลด์)**
  - [x] จัดวาง NPC ร้านค้า, ช่างตีบวก, และ NPC ในโซนเมือง (NPC มอบเควสต์รอระบบเควสต์)
  - [x] จัดวางมอนสเตอร์ 2-3 ชนิด (ระยะประชิด, ระยะไกล, มินิบอส) บนฟิลด์ที่ใช้ Walkable Grid
- [x] **D2. ทดสอบ Full Gameplay Loop (End-to-End)**
  - [x] คุยกับ NPC ในเมือง (รับเควสต์: ยังไม่มีระบบเควสต์)
  - [x] ซื้อเสบียง/ยาฟื้นฟูจากร้านค้า
  - [x] เดินผ่าน Portal ออกสู่ฟิลด์
  - [x] ต่อสู้กับมอนสเตอร์ (รับดาเมจ, กดยาฟื้นฟูเลือด, หลบหลีก)
  - [x] กำจัดมอนสเตอร์ เก็บไอเทมดรอปจากพื้น และรับ EXP เลเวลอัป
  - [x] วาร์ปกลับเมืองเพื่อนำไอเทมไป Refine / Socket
  - [x] บันทึกข้อมูลและโหลดเซฟเกมผ่าน `SaveStorage`
- [x] **D3. สรุปผลและตรวจสอบความเสถียร (Zero Errors)**
  - [x] รัน EditMode Test Suite ทั้งหมดให้ผ่าน 100%
  - [x] ตรวจสอบว่าไม่มี NullReferenceException หรือ Memory Leak ตกค้างในคอนโซล

---

## Phase Q: ระบบเควสต์ (ต้องทำก่อน Phase E)

ข้อสรุปจากผู้ใช้ 2026-11-08: รับและส่งในบทสนทนา NPC, มี Quest Log และตัวติดตามบนจอ, รางวัล EXP + ไอเทม + เงิน, รับได้ครั้งเดียว, ต่อเนื่องกันได้ด้วยเงื่อนไขเควสต์ก่อนหน้า

- [x] **Q1. ข้อมูลและสถานะเควสต์**
  - [x] สร้าง `Quest.json`, Database และสถานะเควสต์ที่แยกจาก Unity และมีเทสต์
  - [x] บันทึกสถานะเควสต์ลง `PlayerDataModel` ผ่าน `PlayerSaveStorage`
- [x] **Q2. เป้าหมาย 4 แบบ**
  - [x] ฆ่ามอนสเตอร์ (`CombatEvents.OnKilled`)
  - [x] เก็บไอเทม (`PlayerInventory.OnInventoryChanged`)
  - [x] คุยกับ NPC (ผ่านระบบ Dialogue)
  - [x] ไปถึงแผนที่ (Scene ที่โหลด)
- [x] **Q3. รับ ส่ง และรางวัล**
  - [x] เพิ่มตัวเลือกรับ/ส่งเควสต์ในบทสนทนา NPC
  - [x] มอบ EXP, ไอเทม และเงินเมื่อส่งเควสต์
- [x] **Q4. หน้าต่าง**
  - [x] Quest Log ด้วย Global Modal และตัวติดตามบนจอ
- [x] **Q5. ทดสอบและเอกสาร**
  - [x] ทดสอบลูปรับเควสต์ใน Town และ Field แล้วเขียน walkthrough

---

## Phase E: การผลิตคอนเทนต์เต็มรูปแบบ (Content Mass Production)

เมื่อระบบแกนกลางทั้งหมดได้รับการยืนยันความเสถียรแล้ว จึงเปิดรับการสร้าง Content จำนวนมาก

ลำดับเริ่มงาน: E3 มอนสเตอร์ก่อน เพราะเควสต์และ Drop ต้องใช้

- [ ] **E1. คลาสอาชีพและผังสกิล (Archetypes & Passive Trees)**
  - [ ] ทยอยบรรจุสกิลและโหนดของ 12 คลาสอาชีพลงใน `Skill.json` และ `SkillTree.json`
- [ ] **E2. คลังไอเทมและอุปกรณ์ (Items, Sets, Costumes, Sockets)**
  - [ ] เพิ่มไอเทมสวมใส่, ชุดแฟชั่น, อัญมณี Fragment, และสูตรคราฟต์ลงใน `Item.json`
- [x] **E3. มอนสเตอร์และบอส (Bestiary & Encounters)**
  - [x] กำหนดค่าสเตตัส, Drop Tables, และพฤติกรรม AI มอนสเตอร์ใน `Character.json`
- [ ] **E4. ออกแบบแผนที่และดันเจี้ยน (Level Design with MapLab)**
  - [ ] ขึ้นรูปแผนที่โลกด้วย `ExtrudedShape` และ Bake เลเยอร์ Walkable พร้อม Elevation
