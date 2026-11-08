# แผนระบบ Global Visualizer (ร่างที่ 2 แก้ตามคำตอบ)

สถานะ: ร่างที่ 2 ได้รับอนุมัติแล้ว (ใช้ค่าเริ่มต้นในข้อ 12) **P1 ถึง P7 เสร็จแล้ว package เวอร์ชัน 1.0.0** (2026-10-18) วิธีใช้ล่าสุดอยู่ใน [Global_Visualizer_Walkthrough.md](./Global_Visualizer_Walkthrough.md)

> **ผล P3 (ขนาดไม่เท่ากัน + Flow + placeholder):**
> - **Size Mode:**
>   - Fixed
>   - Provider (`SizeProvider`)
>   - Measure (วัดด้วย `LayoutUtility` หลัง bind)
>   - ช่องที่ยังไม่ได้วัดใช้ค่าเฉลี่ยไปก่อน และช่องแรกที่มองเห็นอยู่กับที่ตอนขนาดถูกแก้ จึงไม่กระโดด
> - **Flow layout (ตัดบรรทัด):** มีในเมนู Hierarchy > Flow (Tags) ซึ่งตั้ง Measure ให้อัตโนมัติ
> - **Placeholder:** `EmptyPrefab` และเมนูจะสร้าง Empty Template ให้ทุกครั้ง
> - **Cross Alignment:** จัดช่องที่แคบกว่า viewport ไว้ชิดซ้าย กลาง หรือขวาได้
>
> **ผล P4 (Input):**
> - **Selection:** None, Single หรือ Multi (Shift เลือกช่วง, Ctrl เพิ่มหรือเอาออก)
> - **Event:** คลิก, ดับเบิลคลิก, คลิกขวา และ Submit
> - **คีย์บอร์ดและจอย:** เลื่อนผ่าน EventSystem ไปช่องที่ยังไม่ถูกสร้างได้ ถ้าเลยขอบจะส่ง `OnNavigateOut`
> - **ลากหรือเลื่อน:** ตัดสินจากมุม และส่งต่อผ่าน `IVisualDragHandler`
> - **อื่น ๆ:** Snap, `EnsureVisible` และ `IVisualItemState`
>
> **ผล P5 (Inventory):**
> - **`PlayerInventory` มี `AddItems`, `BeginBatch` และ `EndBatch`:** event ยิงครั้งเดียวต่อชุด ตรงนี้แก้โค้ดเกมตามที่ข้อ 11 บอกว่าต้องขออนุมัติ แต่ทำไปเลยเพราะคุณสั่งให้ทำ P3 ถึง P7 โดยไม่ต้องถาม
> - **Dev Tool:** "Add All" ใช้ `AddItems` และ "Discard All" ใช้ batch
> - **`InventoryWindow`:** ใช้ visualizer บน Scroll View เดิมผ่าน `VisualizerViewFactory.AttachTo` โดยไม่แก้ prefab และรวม event ไว้ refresh ครั้งเดียวต่อเฟรม
> - **ผลทดสอบ:** test กับ slot prefab จริง 199 ชิ้นได้ event 1 ครั้ง สร้าง slot ไม่เกิน 35 ชิ้น และใช้เวลาไม่ถึง 50 ms
>
> **ผล P6 (หน้าจออื่น):**
> - **Skill list ใน `SkillManageWindow`:** ใช้ visualizer แล้ว (Vertical + Cross Alignment Center ตาม VerticalLayoutGroup เดิม)
> - **`ui.Visualizer(...)` ใน UIBuilder:** มี `UIVisualizerFactory` อยู่เบื้องหลัง และหน้า Inventory ของ Dev Tool ใช้แล้ว
> - **ตัวตรวจ layout:** ไม่เตือน OversizedControl กับช่องของ visualizer แล้ว เหมือนที่ไม่เตือนกับช่องของ Grid
> - **หน้า Database ของ Dev Tool:** เป็นรายการเดียว (หัวกลุ่มกับแถวไอเท็ม) แบบ Measure กดหัวกลุ่มแล้วแทรกหรือเอาแถวออกในที่เดิม กดแถวแล้ว refresh แค่การ์ดรายละเอียด รายการจึงไม่กระโดด
> - **แก้ใน package:** ถ้า LayoutGroup เปลี่ยนขนาด visualizer ระหว่างที่ canvas กำลัง rebuild ระบบจะรอ refresh หลัง rebuild เสร็จ (เจอจาก UILayoutRegressionTests)
> - **ยังไม่ทำ:** ตารางค่าสถานะ (รายการสั้น ยังไม่คุ้ม)
>
> **ผล P7 (ปล่อย):**
> - **package 1.0.0:** มี README.md / README_TH.md / CHANGELOG.md / LICENSE (MIT)
> - **sample 2 ชุด:** Basic Demo และ Interaction Demo
> - **ทดสอบในโปรเจกต์เปล่า:** ติดตั้งแบบ `file:` + `testables` โดยไม่มี Input System แล้ว test ของ package ผ่าน, sample คอมไพล์ได้ และ PlayMode smoke test รันทั้งสอง demo ได้
> - **ยังไม่ได้ทดสอบ:** การติดตั้งจาก git URL เพราะยังไม่มี repository แยกของ package

> **ผล P2:**
> - **เมนูคลิกขวาใน Hierarchy:** Global Visualizer > Vertical List / Horizontal List / Grid / Create Item Template... (Flow เพิ่มแล้วใน P3)
> - **เมนูใน Project:** Create > Global Visualizer > Item Template...
> - **Wizard สร้าง Template:** สร้างสคริปต์ `IVisualItem<T>` (มี 5 region และไม่มี comment) กับ prefab แล้วติดคอมโพเนนต์และผูกฟิลด์ให้อัตโนมัติหลังคอมไพล์
> - **Inspector:** มีคำเตือน, สถานะ และ Edit Mode Preview ที่ไม่ถูกบันทึกลงฉากและถูกล้างก่อน Play
> - **Gizmo ใน Scene view**
> - **Test:** EditMode ทั้งโปรเจกต์ผ่าน 149/149
วันที่: 2026-10-18 (ร่างที่ 2)

> **ผล P1:**
> - **package:** `Packages/com.globalvisualizer.core` เวอร์ชัน 0.1.0 (namespace `GlobalVisualizer`, MIT, Unity 2022.3 ขึ้นไป)
> - **คอมโพเนนต์หลัก:** `GlobalVisualizerView` ซึ่งในเมนู Add Component แสดงเป็น "UI/Global Visualizer" ไม่ได้ใช้ชื่อ `GlobalVisualizer` ตรงๆ เพราะจะชนกับ namespace
> - **Layout:** Vertical, Horizontal และ Grid (Fit / Fixed Columns / Fixed Rows)
> - **การทำงาน:** virtual scroller, pool, ผูกข้อมูลด้วย callback หรือ `IVisualItem<T>`, `Notify*` และ `ScrollTo`
> - **Test:** EditMode 21 ตัวผ่านครบ รวมถึง "เลื่อน 1,000 ครั้งไม่มี GC alloc"
> - **demo:** อยู่ที่ `Samples~/BasicDemo` (import ผ่าน Package Manager)
> - **วิธีใช้:** [Global_Visualizer_Walkthrough.md](./Global_Visualizer_Walkthrough.md)
ไฟล์ที่ดูประกอบ: `CoreSystem/Data/PlayerInventory.cs`, `UI/Inventory/InventoryWindow.cs`, `UI/Skill Manage Page/SkillManageWindow.cs`, `SkillItemListUI.cs`, `UI/Utility/ItemDragHandler.cs`, `CoreSystem/UI/Manager/UIBuilderContext.cs`, `UIElementFactory.cs`, `CoreSystem/UI/DevTools/GameDevToolUI.cs`

> สิ่งที่เปลี่ยนจากร่างที่ 1
> - **Context:** ร่างที่ 1 เข้าใจว่าหมายถึงเมนูคลิกขวาตอนเล่นเกม จริง ๆ หมายถึง**เมนูคลิกขวาใน Hierarchy ของ Unity Editor** (Create > ...) ส่วนเมนูคลิกขวาตอนเล่นเกมจึงตัดออกจาก Core
> - **ไม่ผูกกับระบบใด:** เน้นเป็น Framework ใส่ prefab อะไรก็ได้ ไม่บังคับให้ prefab สืบทอด class ใด
> - **v1 รองรับเพิ่ม:** ช่องขนาดไม่เท่ากัน และเลื่อนด้วยคีย์บอร์ดหรือจอย
> - **หน้าจอแรก:** Inventory โดยเป้าหมายแรกคือ **กด "Add All" แล้วเกมต้องไม่ค้าง**

---

## 0. สรุปสั้น ๆ

- **Global Visualizer** คือ Framework กลางสำหรับแสดง "รายการของ" ด้วย prefab อะไรก็ได้ ระบบรู้แค่ "มีกี่ชิ้น" กับ "ชิ้นที่ i ต้องแสดงอะไร" ไม่รู้จัก Item, Skill หรือระบบใดในเกม
- **สร้างเฉพาะช่องที่มองเห็น** แล้วนำ GameObject กลับมาใช้ซ้ำ (pool) ข้อมูลมีหมื่นชิ้นก็สร้าง GameObject แค่หลักสิบ
- **Layout:** Vertical, Horizontal, Grid และ Flow (ตัดบรรทัด) รองรับช่องขนาดไม่เท่ากันตั้งแต่ v1
- **สร้างจาก Editor:** คลิกขวาใน Hierarchy เลือก **Global Visualizer > Vertical List / Horizontal List / Grid / Flow** ได้ ScrollRect ที่ตั้งค่าครบ และสร้าง Item Template (prefab ตัวอย่าง) ให้ด้วย
- **แยกเป็น package** ปล่อยเป็น open-source add-on ได้ ส่วนที่ผูกกับเกมอยู่ในชั้น Adapter

---

## 1. สิ่งที่ผมเข้าใจ (อัปเดตแล้ว)

| # | ความต้องการ | สรุป |
|---|---|---|
| 1 | Context | **เมนูคลิกขวาใน Hierarchy ของ Editor** ใช้สร้าง Scroller / Visualizer / Template ได้ทันที เหมือนเมนู UI > Scroll View ของ Unity |
| 2 | ชื่อ | **Global Visualizer** ทำง่าย ๆ แบบ Framework รับ prefab ที่ส่งเข้ามา ตั้งค่าได้ทุกอย่าง ไม่ fix กับระบบใด |
| 3 | ใช้ได้หลายที่ | Inventory, Skill list, Dev Tool, ตารางค่าสถานะ และทุกหน้าจอในอนาคต |
| 4 | Core แยก | ปล่อยเป็น open-source add-on ได้ ตัว Core ไม่อ้างโค้ดเกมเลย |
| 5 | ช่องขนาดไม่เท่ากัน | **ต้องมีใน v1** |
| 6 | คีย์บอร์ด / จอย | **ต้องมีใน v1** |
| 7 | หน้าจอแรก | **Inventory** ตอนกด "Add All" ใน Dev Tool ให้เพิ่มแค่ข้อมูล แล้ว UI สร้างแค่ช่องที่มองเห็น (ตอนนี้กด Add All หรือ Get All แล้วค้างทุกครั้ง) |
| 8 | Context Menu ตอนเล่นเกม | **ไม่อยู่ใน Core** ใครอยากทำ ใช้ event คลิกของช่องต่อเอง |

---

## 2. ทำไมกด Add All แล้วค้าง (ต้นเหตุจริง)

1. Dev Tool วน `Inventory.AddItem(item)` ทีละชิ้น (199 ชิ้น)
2. `PlayerInventory.AddItem` ยิง `OnInventoryChanged` **ทุกครั้ง** ที่เพิ่ม 1 ชิ้น
3. `InventoryWindow` ฟัง event นี้อยู่ 2 รายการ (Inventory + Equipment) และทุกครั้งจะ **ลบช่องทั้งหมดแล้วสร้างใหม่ทั้งหมด**
4. ผลคือเกิด Instantiate/Destroy หลายหมื่นครั้งในเฟรมเดียว จำนวนช่องโตตามกำลังสอง (199 ชิ้นได้ประมาณ 40,000 ครั้ง) **เกมจึงค้าง**

ต้องแก้ 2 ชั้น

| ชั้น | แก้อย่างไร | อยู่ที่ |
|---|---|---|
| ข้อมูล | เพิ่มของหลายชิ้นแล้วแจ้งเตือน **ครั้งเดียว** (`AddItems(list)` หรือ `BeginBatch()` / `EndBatch()`) | ฝั่งเกม: `PlayerInventory` (ต้องขออนุมัติก่อนแก้) |
| UI | ใช้ Global Visualizer ได้ยินแค่ "จำนวนเปลี่ยน" แล้วสร้างหรือ rebind เฉพาะช่องที่มองเห็น (ราว 20 ถึง 40 ช่อง) | Inventory adapter |

ปัญหาเดียวกันมีใน Skill list (`SkillManageWindow`) และ Grid ของ UIBuilder (Dev Tool database สร้างปุ่มทั้ง 199 อันพร้อมกัน)

---

## 3. หลักการออกแบบ: "ง่ายแบบ Framework"

ระบบรู้จักแค่ 3 อย่าง

| อย่าง | ความหมาย | ใครเป็นคนให้ |
|---|---|---|
| **จำนวน** (`Count`) | มีทั้งหมดกี่ชิ้น | โค้ดผู้ใช้ |
| **Prefab** (Item Template) | หน้าตาของ 1 ช่อง เป็น prefab อะไรก็ได้ | ลากใส่ใน Inspector หรือส่งผ่านโค้ด |
| **วิธีเติมข้อมูล** (Bind) | ช่องที่ i แสดงอะไร | callback หรือ interface บน prefab (เลือกได้) |

การเติมข้อมูลรองรับ 2 แบบ ไม่ต้องสืบทอด class ใด
- **แบบ callback** (ไม่ต้องแก้ prefab เลย):
  ```csharp
  visualizer.SetItems(items, (GameObject view, Item item, int index) => { ... });
  ```
- **แบบ interface บน prefab** (คอมโพเนนต์ใดใน prefab ก็ได้): ใช้ `IVisualItem<T>` ที่มี `Bind(T data, VisualItemInfo info)` / `Unbind()` ระบบเรียกให้อัตโนมัติ

ทุกอย่างอื่นตั้งค่าได้ใน Inspector หรือผ่านโค้ด ได้แก่ Layout, ระยะห่าง, padding, buffer, การเลือก, การนำทาง, ขนาดคงที่หรือไม่คงที่ และ prefab ตอนว่าง (placeholder)

---

## 4. ภาพรวมการทำงาน

```
ข้อมูลของผู้ใช้ (list อะไรก็ได้)
   │  แจ้งระบบ: SetItems / NotifyChanged(i) / NotifyInserted / NotifyRemoved / NotifyReset
   ▼
Layout          คำนวณตำแหน่งและขนาดของช่องที่ i (คงที่ หรือวัดจริงแล้ว cache)
   ▼
Scroller        ดูว่าช่องไหนอยู่ในจอ (+ buffer) แล้วขอ View เท่าที่ต้องใช้
   ▼
Pool            หยิบ GameObject เก่ามาใช้ซ้ำ ถ้าไม่พอค่อย Instantiate จาก Prefab
   ▼
Bind            เรียก callback หรือ IVisualItem.Bind ให้ช่องนั้นแสดงข้อมูลที่ i
   ▼
Input           คลิก / เลือก / ลูกศร / จอย และ event ส่งออกให้โค้ดผู้ใช้
```

- **ไม่มี LayoutGroup บน content และไม่มี `Update()` วนเช็ก:** ขยับเมื่อ `ScrollRect.onValueChanged`, เมื่อมีการแจ้งข้อมูล หรือเมื่อขนาดจอเปลี่ยนเท่านั้น
- **อัปเดตเฉพาะส่วน:** ของเปลี่ยน 1 ชิ้นก็ rebind แค่ 1 ช่อง ถ้าช่องนั้นอยู่นอกจอก็ไม่ทำอะไร

---

## 5. ส่วนประกอบ

| ส่วน | ชื่อร่าง | ทำอะไร |
|---|---|---|
| ตัวหลัก | `GlobalVisualizer` (คอมโพเนนต์บน ScrollRect) | ตั้งค่าทั้งหมดใน Inspector และมี API `SetItems`, `SetCount`, `Notify*`, `ScrollTo`, `Select` |
| Template | Item Prefab (prefab อะไรก็ได้) + Empty Prefab (placeholder) + ตัวเลือก template ตามข้อมูล | หลายหน้าตาในรายการเดียวได้ เช่นหัวกลุ่มกับช่องปกติ |
| เติมข้อมูล | callback `Action<GameObject, T, int>` หรือ `IVisualItem<T>` | ไม่บังคับ base class |
| Layout | Vertical, Horizontal, Grid (คอลัมน์คงที่ / พอดีจอ / เลื่อนแนวนอนแบบกำหนดจำนวนแถว), Flow | สลับได้ใน Inspector คำนวณด้วยคณิตศาสตร์ล้วน จึงทดสอบได้ |
| ขนาดช่อง (v1) | **Fixed** (จากขนาด prefab), **Provider** (`Func<int, Vector2>` ผู้ใช้บอกขนาดเอง), **Measure** (วัดจาก layout ของ prefab แล้ว cache) | รองรับช่องขนาดไม่เท่ากัน เช่น chat หรือคำอธิบายยาว |
| Pool | `VisualPool` | pool แยกต่อ prefab มี warm-up และคืนของอัตโนมัติ |
| การเลือก | None / Single / Multi | event `OnSelectionChanged` พร้อมสถานะ selected ที่ส่งให้ช่องตอน Bind |
| นำทาง (v1) | คีย์บอร์ด (ลูกศร, Tab, Enter) และจอย ผ่าน EventSystem / Input System | เลื่อนตาม Layout (กริดไปซ้าย ขวา ขึ้น ลงได้ถูก) และ scroll ตามช่องที่ focus อัตโนมัติ แม้ช่องนั้นยังไม่ถูกสร้าง |
| Event ส่งออก | `OnItemClicked`, `OnItemDoubleClicked`, `OnItemRightClicked`, `OnItemSubmitted`, `OnSelectionChanged`, `OnVisibleRangeChanged` | ฝั่งเกมนำไปทำ tooltip, ลากวาง หรือเมนูตอนเล่นเกมเองได้ |
| ลากหรือเลื่อน | ตัดสินว่าผู้เล่นกำลังลากของหรือเลื่อนรายการไว้ที่เดียว (threshold + มุม) | แทน logic ที่ซ้ำกันใน `SkillItemListUI` และ `ItemDragHandler` |

---

## 6. Context Menu ใน Editor (คลิกขวาใน Hierarchy)

เมนูแบบเดียวกับ **GameObject > UI > Scroll View** ของ Unity

```
คลิกขวาใน Hierarchy
  └ Global Visualizer
      ├ Vertical List
      ├ Horizontal List
      ├ Grid
      ├ Flow
      └ Create Item Template
```

| เมนู | ได้อะไร |
|---|---|
| Vertical / Horizontal / Grid / Flow | GameObject ที่มี ScrollRect, Viewport + Mask, Content, Scrollbar และ `GlobalVisualizer` ตั้งค่า Layout ตามที่เลือก วางใต้ Canvas ที่เลือกอยู่ (ถ้าไม่มี Canvas จะสร้างให้พร้อม EventSystem) และ Undo ได้ |
| Create Item Template | สร้าง prefab ตัวอย่าง (พื้นหลัง, ไอคอน, ข้อความ) และสคริปต์ที่ทำ `IVisualItem<T>` ไว้ให้แล้ว (มี `#region` ครบ) จากนั้นผูกเป็น Item Prefab ของ Visualizer ที่เลือกอยู่ |

เพิ่มเติมใน Inspector ของ `GlobalVisualizer`
- **Preview:** ใส่จำนวนช่องตัวอย่างแล้วดูผลใน Edit mode ได้เลย
- **Gizmo:** แสดงขอบเขตที่มองเห็น, buffer และจำนวนใน pool
- **Warning:** แจ้งเมื่อ prefab มี LayoutGroup ที่จะชนกับระบบ หรือขาด RectTransform

---

## 7. หน้าตา API (ร่าง)

แบบ callback ล้วน ไม่ต้องแก้ prefab
```csharp
_visualizer.SetItems(GameData.Player.Inventory.GetAllItems(), (view, item, index) =>
{
    view.GetComponentInChildren<Image>().sprite = item.Item.Icon;
});
_visualizer.OnItemClicked += index => ShowDetail(index);
```

แบบ interface บน prefab
```csharp
public sealed class InventorySlotView : MonoBehaviour, IVisualItem<ItemInstance>
{
    #region [Variables/Fields]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _count;
    #endregion

    #region [Public Methods]
    public void Bind(ItemInstance data, VisualItemInfo info)
    {
        _icon.sprite = data.Item.Icon;
        _count.text = data.Quantity > 1 ? data.Quantity.ToString() : string.Empty;
    }

    public void Unbind() { }
    #endregion
}
```

ข้อมูลเปลี่ยน
```csharp
_visualizer.NotifyChanged(index);
_visualizer.NotifyInserted(index, count);
_visualizer.NotifyRemoved(index, count);
_visualizer.NotifyReset();
```

ช่องขนาดไม่เท่ากัน
```csharp
_visualizer.SizeMode = VisualSizeMode.Measure;
_visualizer.SizeProvider = index => new Vector2(0, MeasureMessageHeight(index));
```

---

## 8. แยกเป็น package สำหรับ open source

```
Packages/com.<studio>.global-visualizer/
  package.json  README.md  README_TH.md  LICENSE  CHANGELOG.md
  Runtime/   GlobalVisualizer.Runtime.asmdef    (อ้างแค่ UnityEngine.UI)
  Editor/    GlobalVisualizer.Editor.asmdef     (เมนู Hierarchy, Create Item Template, Inspector, Preview, Gizmo)
  Tests/     GlobalVisualizer.Tests.asmdef      (layout math, visible range, ขนาดไม่เท่ากัน, pool, selection, navigation)
  Samples~/  InventoryGrid, ChatLog (ขนาดไม่เท่ากัน), Leaderboard, GamepadMenu

Assets/Script/UI/Visualizer/   (Adapter ฝั่งเกม ไม่อยู่ใน package)
  InventoryVisualizerAdapter   ต่อ PlayerInventory เข้ากับ Visualizer (รวม event เป็นครั้งเดียวต่อเฟรม)
  GameDragBridge               ส่ง event ลากให้ GlobalDragManager
  UIBuilderVisualizerExtensions  ui.Visualizer(...) สำหรับ Dev Tool
```

- **Namespace:** `GlobalVisualizer` ใน package ส่วนฝั่งเกมอยู่ใต้ `CoreSystem.UI.Visualizer`
- **ไม่ใช้ TextMeshPro ใน Runtime:** ใช้เฉพาะใน sample และ template ที่สร้าง (ตรวจว่ามี TMP ไหมก่อนสร้าง)
- **package ต้องไม่อ้าง `Assembly-CSharp`:** ลบ Adapter ทิ้งแล้ว package ยังคอมไพล์และรัน sample ได้
- **มาตรฐานโค้ด:** ใช้ `#region` ห้าส่วน, ห้ามใช้ runtime Find และไม่ใส่ comment ตามกฎโปรเจกต์ ส่วน README ของ package เป็นที่อธิบายการใช้งาน

---

## 9. ลำดับงาน

| เฟส | งาน | ถือว่าเสร็จเมื่อ |
|---|---|---|
| P0 | ยืนยันร่างนี้ | คุณตอบข้อ 12 ที่เหลือ |
| P1 Core | `GlobalVisualizer`, Layout Vertical / Horizontal / Grid, ขนาดคงที่, Scroller, Pool, callback Bind และ `IVisualItem` | ฉาก demo 10,000 ช่องเลื่อนลื่นที่ 60 fps ไม่มี GC ระหว่างเลื่อน และ EditMode test ผ่าน |
| P2 Editor | เมนูคลิกขวาใน Hierarchy (4 แบบ + Create Item Template), Inspector, Preview, Gizmo | สร้างรายการใช้งานได้จากเมนูภายใน 1 นาที |
| P3 ขนาดไม่เท่ากัน + Flow | Size Provider, Measure + cache, Flow layout, Empty prefab | sample ChatLog ข้อความยาวไม่เท่ากันเลื่อนลื่น และ scroll ไม่กระโดด |
| P4 Input | Selection, คีย์บอร์ด, จอย, ScrollTo, ตัดสินลากหรือเลื่อน, event ส่งออก | sample GamepadMenu เดินครบทุกช่องด้วยจอย |
| P5 Inventory (ปัญหาจริง) | Inventory adapter + แจ้งเตือนแบบรวมครั้งเดียว + ย้าย `InventoryWindow` | **กด Add All (199 ชิ้น) ใน Dev Tool แล้วไม่ค้าง**: ไม่มีเฟรมเกิน 50 ms และสร้าง GameObject ไม่เกิน buffer |
| P6 หน้าจออื่น | Skill list, Dev Tool (`ui.Visualizer`) และตารางค่าสถานะ | หน้าจอเดิมทำงานเหมือนเดิม และ UI snapshot ไม่มี error |
| P7 ปล่อย | แยก package, samples, README TH/EN, license, 1.0 | โปรเจกต์เปล่าติดตั้งจาก git URL แล้ว sample รันได้ |

ทุกเฟสมี Walkthrough ตามกฎโปรเจกต์: `Docs/02-Technical/UI/Global_Visualizer_Walkthrough.md`

---

## 10. เป้าหมายคุณภาพ

- **10,000 ช่องที่ 60 fps:** ไม่มี GC alloc ต่อเฟรมระหว่างเลื่อน
- **Add All 199 ชิ้น:** ไม่มีเฟรมเกิน 50 ms และ UI สร้าง GameObject แค่เท่าที่มองเห็นบวก buffer
- **ช่องขนาดไม่เท่ากัน:** ตำแหน่ง scroll ไม่กระโดดเมื่อขนาดจริงต่างจากที่ประมาณไว้
- **จอยหรือคีย์บอร์ด:** ไปถึงทุกช่องได้ รวมถึงช่องที่ยังไม่ถูกสร้าง
- **Test:** EditMode test ครอบคลุม layout math, visible range, ขนาดไม่เท่ากัน, pool, selection และ navigation

---

## 11. ความเสี่ยงและทางเลือก

| เรื่อง | ทางที่เลือก |
|---|---|
| ขนาดไม่เท่ากันทำให้ scroll กระโดด | ประมาณขนาดก่อน แล้วแก้ทีละช่องเมื่อวัดจริง โดยตรึงตำแหน่งช่องบนสุดที่มองเห็นไว้ (anchor) |
| จอยต้องไปช่องที่ยังไม่ถูกสร้าง | นำทางด้วย index ไม่ใช่ GameObject: scroll ไปก่อนแล้วค่อยตั้ง focus |
| prefab ของผู้ใช้มี LayoutGroup หรือ ContentSizeFitter | อนุญาตให้มีได้ภายใน prefab แต่ไม่ให้อยู่บน Content และ Inspector จะเตือน |
| แก้ `PlayerInventory` (โค้ดเกม) | เป็นการเพิ่ม API แบบ batch โดยไม่เปลี่ยนสูตรหรือพฤติกรรมเดิม ขออนุมัติก่อนใน P5 |
| ย้ายโค้ดเดิมแล้วพัง | ย้ายทีละหน้าจอ และเก็บของเดิมไว้จนของใหม่ผ่าน |
| UI Toolkit | v1 ทำเฉพาะ uGUI ซึ่งเป็นสิ่งที่เกมใช้อยู่ |

---

## 12. คำถามที่ยังเหลือ

| ข้อ | คำถาม | ถ้าไม่ตอบจะใช้ |
|---|---|---|
| 2 | namespace และชื่อ package (`com.<studio>.global-visualizer`): จะใช้ชื่อ studio อะไร | namespace `GlobalVisualizer` และใส่ชื่อ studio ภายหลัง |
| 3 | Unity ขั้นต่ำของ open source | **2022.3 LTS ขึ้นไป** (ทดสอบจริงบน Unity 6000.4) |
| 4 | License | **MIT** |

---

## 13. สิ่งที่ไม่อยู่ใน v1

- เมนูคลิกขวาตอนเล่นเกม (ทำเองได้จาก `OnItemRightClicked`)
- UI Toolkit
- แอนิเมชันตอนเพิ่มหรือลบช่อง (เผื่อ hook ไว้)
- รายการซ้อนรายการแบบ virtualize ทั้งสองชั้น
- บันทึกตำแหน่ง scroll ลง save
