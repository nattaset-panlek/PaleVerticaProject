# Unique Node Concepts

Concept ของ Unique Passive สำหรับ Node ใหญ่บน Skill Tree มีแค่ชื่อและแนวคิด ยังไม่มี Script และยังไม่ได้กำหนดตัวเลข ไอเดียชุดเก่าอยู่ที่ [UniquePassiveIdeas.md](./UniquePassiveIdeas.md)

**ตำแหน่งบน Tree:** ทุก Class มีช่อง `C1` ใกล้จุดเริ่ม, `C2` กลางสาย, `C3` ซ้ายและขวาอย่างละช่อง, `C4` ปลายสาย ช่อง `C1` มี Unique แล้วทุก Class ชุดนี้จึงเติมช่องที่เหลือ 46 ช่อง

**แนวออกแบบ:** `C2` เสริมสายหลัก, `C3` แยกแนวเล่นสองทาง, `C4` เปลี่ยนวิธีเล่นและมีข้อแลกเปลี่ยน

| Class | ช่อง | ชื่อ | Concept |
|---|---|---|---|
| Warrior | C2 | Iron Momentum | ยิ่งตีเป้าเดิมต่อเนื่อง ดาเมจยิ่งเพิ่ม เปลี่ยนเป้าแล้วเริ่มนับใหม่ |
| Warrior | C3 ซ้าย | Burden of Kings | ATK เพิ่มตามน้ำหนักที่แบก แต่เดินช้าลงตามน้ำหนัก |
| Warrior | C3 ขวา | Last Banner | เมื่อ HP ลดต่ำกว่าครึ่งครั้งแรกของการต่อสู้ จะได้เกราะชั่วคราว |
| Warrior | C4 | Anvil Heart | ติด Crit ไม่ได้ แต่ทุกการโจมตีแรงเท่าค่าเฉลี่ยของ Crit |
| Inquisitor | C2 | Verdict | ศัตรูที่โดนตีจะถูกตีตรา การโจมตีครั้งถัดไปจะระเบิดตราเป็นดาเมจ Holy |
| Inquisitor | C3 ซ้าย | Penitent Blade | STR ส่วนหนึ่งถูกนับเป็น INT สำหรับสกิล Holy |
| Inquisitor | C3 ขวา | Heretic's Pyre | ศัตรูที่ตายขณะมีตราจะไฟลุกลามไปตัวรอบข้าง |
| Inquisitor | C4 | Oath of Silence | ใช้สกิลที่ต้องร่ายไม่ได้ แต่สกิลร่ายทันทีแรงขึ้นมาก |
| Enchanter | C2 | Spellbound Edge | ทุกการร่ายสำเร็จ ทำให้การโจมตีปกติครั้งถัดไปมีธาตุของสกิลนั้น |
| Enchanter | C3 ซ้าย | Quickened Weave | การโจมตีปกติแต่ละครั้งลดเวลาร่ายของสกิลถัดไป |
| Enchanter | C3 ขวา | Runic Skin | ธาตุของสกิลล่าสุดกลายเป็นธาตุต้านทานชั่วคราว |
| Enchanter | C4 | Twin Conduit | ใช้ ATK และ M.ATK รวมกันเป็นค่าเดียว แต่สวมอาวุธสองมือไม่ได้ |
| Assassin | C2 | Marked Throat | โจมตีจากด้านหลังเป้าหมายจะติด Crit แน่นอน |
| Assassin | C3 ซ้าย | Veil Step | หลังหลบการโจมตีได้ จะล่องหนชั่วครู่ |
| Assassin | C3 ขวา | Venom Ledger | ยิ่งเป้ามีสถานะผิดปกติมาก ดาเมจจากคุณยิ่งแรง |
| Assassin | C4 | One Clean Cut | การโจมตีแรกต่อเป้าแรงมาก แต่ครั้งถัดไปแรงลดลงจนกว่าจะเปลี่ยนเป้า |
| Gunner | C2 | Loaded Chamber | ยืนนิ่งจะบรรจุกระสุนพิเศษ นัดถัดไปแรงขึ้นตามเวลาที่เก็บ |
| Gunner | C3 ซ้าย | Powder Burn | Crit ทำให้เป้าติดไฟ |
| Gunner | C3 ขวา | Ricochet Doctrine | การยิงที่ฆ่าศัตรูจะกระดอนไปตัวถัดไป |
| Gunner | C4 | Six Fates | การโจมตีหมุนผลสุ่มหกแบบ ตั้งแต่พลาดจนถึงแรงมหาศาล |
| Archer | C2 | Windreader | ยิ่งเป้าอยู่ไกล ดาเมจยิ่งเพิ่ม |
| Archer | C3 ซ้าย | Pinning Shot | การยิงโดนเป้าซ้ำจะตรึงเป้าไว้ชั่วครู่ |
| Archer | C3 ขวา | Skirmisher's Pace | ยิงขณะเคลื่อนที่ได้โดยไม่ช้าลง แต่ระยะยิงสั้นลง |
| Archer | C4 | Hollow Quiver | ไม่ใช้ Basic Attack อัตโนมัติ แต่ทุกนัดที่เล็งเองแรงและเจาะทะลุ |
| Mage | C2 | Overflowing Sigil | MP ที่เต็มเกินจากการฟื้นฟูจะสะสมเป็นพลังให้สกิลถัดไป |
| Mage | C3 ซ้าย | Glass Cannon Theorem | ยิ่ง MP เหลือมาก M.ATK ยิ่งสูง |
| Mage | C3 ขวา | Slow Incantation | สกิลร่ายยาวแรงขึ้นตามเวลาร่ายที่ใช้จริง |
| Mage | C4 | Singular Element | เลือกธาตุเดียว ธาตุนั้นแรงมาก ธาตุอื่นใช้ไม่ได้ |
| Sorcerer | C3 ซ้าย | Hexweaver | คำสาปบนเป้าเดียวซ้อนกันได้ และแรงขึ้นตามจำนวน |
| Sorcerer | C3 ขวา | Fickle Stars | LUK กำหนดโอกาสที่สกิลจะไม่ใช้ MP |
| Sorcerer | C4 | Whisper Pact | ยิ่ง Corruption สูงยิ่งแรง แต่ HP สูงสุดลดลง |
| Necromancer | C3 ซ้าย | Grave Tithe | ศัตรูที่ตายรอบตัวคืน HP ให้ทีละน้อย |
| Necromancer | C3 ขวา | Bone Choir | โครงกระดูกที่ตายในระยะจะฟื้นมาเป็นพวกชั่วคราว |
| Necromancer | C4 | Hollow Crown | Max HP ลดเหลือครึ่งเดียว แต่การดูดเลือดทำให้เกิน HP สูงสุดได้ |
| Alchemist | C2 | Volatile Mixture | ยาที่ใช้จะระเบิดเป็นวงรอบตัว |
| Alchemist | C3 ซ้าย | Second Dose | ยามีโอกาสไม่ถูกใช้หมด |
| Alchemist | C3 ขวา | Transmuted Blood | ไอเทมฟื้น HP จะฟื้น MP ด้วย |
| Alchemist | C4 | Living Philosopher | ใช้ยาไม่ได้ แต่ฟื้น HP ต่อเนื่องตาม VIT |
| Priest | C2 | Answered Prayer | การฮีลตัวเองจะฮีลแรงขึ้นเมื่อ HP ยิ่งต่ำ |
| Priest | C3 ซ้าย | Radiant Rebuke | การฮีลปล่อยคลื่น Holy ทำดาเมจศัตรูรอบตัว |
| Priest | C3 ขวา | Vow of Stillness | ยืนนิ่งจะได้ M.DEF และฟื้น MP เร็วขึ้น |
| Priest | C4 | Covenant Keeper | ฟื้นฟูทั้งหมดแรงขึ้นมาก แต่ทำดาเมจเองไม่ได้นอกจากธาตุ Holy |
| Temmer | C2 | Pack Instinct | ยิ่งมีอัญเชิญอยู่มาก ตัวเองยิ่งเดินและตีเร็วขึ้น |
| Temmer | C3 ซ้าย | Shared Wound | อัญเชิญรับดาเมจแทนคุณบางส่วน |
| Temmer | C3 ขวา | Feral Bond | อัญเชิญได้โบนัสจาก DEX และ VIT ของคุณ |
| Temmer | C4 | Alpha's Burden | มีอัญเชิญได้ตัวเดียว แต่ตัวนั้นใหญ่และเก่งขึ้นมาก |
