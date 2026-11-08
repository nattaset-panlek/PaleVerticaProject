# UIBuilder Framework Guide (For AI Agents)

**UIBuilder** is a custom, code-first UI framework designed for Unity. It allows developers (and AI agents) to dynamically generate native Unity UI (`GameObject`, `RectTransform`, `TextMeshPro`, etc.) using a declarative, lambda-based syntax similar to HTML/React or Unity's IMGUI (`OnGUI`).

This document is written to give future AI agents a complete understanding of how to use and extend this system to create beautiful, functional modals and UI without needing to read the entire project source code.

---

## 1. Core Architecture
- **`UIBuilder`**: The static entry point for procedural UI. Usually, you call `UIBuilder.BuildGlobalModal(...)`.
- **`UIBuilderContext`**: The main context object passed into your lambda functions. It provides methods like `Div()`, `Horizontal()`, `Vertical()`, `Grid()`, `Text()`, `Button()`, `Custom()`, etc., which construct the actual GameObjects.
- **`UIGlobalModal`**: The runtime modal window. Modals are keyed by name (e.g. `UIGlobalModal.Create("Inventory")`). If the modal already exists at runtime, it retrieves and displays the existing instance; otherwise, it instantiates a new GameObject clone from the template. Multiple modals (A, B, C, D) can coexist at runtime simultaneously!
- **`UIElementRegistrySO`**: A ScriptableObject mapping that acts like CSS classes. It maps `UIElementType` + `styleName` to a specific **Prefab**.
- **`UICustomElement`**: A MonoBehaviour component attached to complex prefabs that exposes internal child GameObjects via string keys.

---

## 2. Fallback Generation (The "Default" Style)
You **do not** need to provide a prefab for basic UI elements. If you use standard elements without mapping them in the registry, the `UIBuilderContext` will automatically generate them via code with sensible default styles (padding, margins, colors, alignments).

**Do NOT add basic elements (like Div, Text, Button, Grid) to the `UIElementRegistrySO` unless you specifically want to OVERRIDE the default styling with a highly customized graphic or animation.**

---

## 3. Syntax & Available Methods

Always build your UI inside the lambda scope provided by `BuildGlobalModal`:

```csharp
// Example of opening a Modal
var modal = UIBuilder.BuildGlobalModal("Title", context => 
{
    // The UI building happens here
}, new Vector2(800, 600)); // Optional: Custom size
```

### Containers (Layouts)
Use containers to group elements. They automatically use `ContentSizeFitter` and `LayoutGroup` components.

```csharp
// Vertical Layout (Default Div)
context.Div("Container", div => {
    div.Text("Label", "Hello World");
});

// Horizontal Layout
context.Horizontal("Row", row => {
    row.Button("Btn1", "Accept", () => { /* action */ });
    row.Button("Btn2", "Decline", () => { /* action */ });
});

// Vertical Layout (Explicit)
context.Vertical("Col", col => { ... });

// Grid Layout
context.Grid("SlotGrid", grid => {
    // Child elements inside grid
})
.SetGridCellSize(new Vector2(100, 100))
.SetGridSpacing(new Vector2(8, 8))
.SetGridConstraint(GridLayoutGroup.Constraint.FixedColumnCount, 4);

// Horizontal ScrollView (Elastic or Clamped)
context.HorizontalScrollView("TabScroll", scroll => {
    // Child items that overflow horizontally
    scroll.Button("TabBtn", "Tab 1", () => { }).SetPreferredSize(120, 40);
    scroll.Button("TabBtn", "Tab 2", () => { }).SetPreferredSize(120, 40);
}, ScrollRect.MovementType.Elastic)
.SetPreferredSize(-1, 50)
.SetSpacing(8);
```

### Interactive Elements
```csharp
// Button
context.Button("SubmitBtn", "Confirm", () => {
    Debug.Log("Clicked!");
});

// InputField
context.InputField("SearchInput", "Initial Text", (newText) => {
    Debug.Log("Typed: " + newText);
});

// Toggle (Checkbox)
context.Toggle("SettingsToggle", true, "Enable Feature", (isOn) => {
    Debug.Log("Toggle is now: " + isOn);
});
```

### Static Elements
```csharp
// Text
context.Text("Header", "System Menu");

// Image
context.Image("Icon", mySprite);

// RawImage (RenderTextures, videos, portraits)
context.RawImage("Portrait", myTexture);

// ProgressBar
context.ProgressBar("HealthBar", 0.75f, Color.green).SetPreferredSize(-1, 20);

// Divider / Separator line
context.Divider("Line", thickness: 2f, Color.gray);
```

---

## 4. Complex Custom Prefabs (`UICustomElement`)
When an element is too intricate to assemble with basic primitives (e.g. an equipment card with particle roots, specific animations, complex nested hierarchies), you can use the **`Custom`** type:

1. Create a prefab in Unity with any arbitrary hierarchy.
2. Attach the `UICustomElement` script to the root of that prefab.
3. In the Inspector of `UICustomElement`, map internal child GameObjects to descriptive string keys (e.g. `"Icon"`, `"TitleText"`, `"ActionButton"`).
4. Register the prefab in `UIElementRegistrySO` under `UIElementType.Custom` with a `StyleName` (e.g. `"ItemCard"`).
5. In your code, instantiate it and retrieve internal components directly:

```csharp
UICustomElement card = context.Custom("ItemCard");

var title = card.Get<TextMeshProUGUI>("TitleText");
if (title != null) title.text = "Excalibur";

var btn = card.Get<Button>("ActionButton");
if (btn != null)
{
    btn.onClick.AddListener(() => Debug.Log("Equipped!"));
}

// Or retrieve raw GameObject:
GameObject iconObj = card.GetGameObject("Icon");
```

---

## 5. Layout and Size Management
Every builder method returns a `UIElementRef` which allows you to chain layout modifiers using a fluent API. This is very useful when you want to force specific sizes or flexible layouts (like CSS Flexbox).

```csharp
// Force exact width and height (ignoring layout rules)
context.Button("Btn", "Click Me", () => {}).SetSize(200, 50);

// Set LayoutElement properties (best when inside a Horizontal/Vertical group)
// Automatically unconstrains ContentSizeFitter on the modified axes to avoid layout fights!
context.Button("Btn", "Click Me", () => {})
       .SetPreferredSize(200, 50)
       .SetMinSize(100, 30);

// Flexible Width / Height (flex-grow)
context.Horizontal("Row", row => {
    row.Text("Label", "Name:").SetPreferredSize(100, -1);
    row.InputField("Input", "", (val) => {}).SetFlexibleSize(1, -1); // Fills remaining space
});

// Fit to Parent Container (e.g. width: 100%)
// Removes ContentSizeFitter on selected axes and sets flexibleWidth/Height = 1
context.Vertical("Div", col => { ... })
       .SetFitParent(stretchWidth: true, stretchHeight: false);
```

### Grid Modifiers
```csharp
context.Grid("ItemGrid", grid => { ... })
       .SetGridCellSize(new Vector2(95, 95))
       .SetGridSpacing(new Vector2(8, 8))
       .SetGridConstraint(GridLayoutGroup.Constraint.FixedColumnCount, 4)
       .SetPadding(8, 8, 8, 8);
```

---

## 6. HTML/CSS-like Styling (Text, Color, Spacing, Clicks)
You can chain style modifiers directly onto elements, functioning similarly to inline CSS:

```csharp
// Modifying Text Properties
context.Text("Header", "Main Title")
       .SetFontSize(24)
       .SetTextColor(Color.yellow)
       .SetTextAlignment(TextAlignmentOptions.Center)
       .SetFontStyle(FontStyles.Bold | FontStyles.Underline);

// Modifying Colors & Images
context.Button("Btn", "Danger", () => {})
       .SetColor(Color.red)          // Changes graphic color
       .SetTextColor(Color.white);    // Changes label text color

context.Image("Img", null)
       .SetSprite(mySprite);

// Clickable Cards / Divs (Turns any container into an interactive button)
context.Vertical("Div", slotCard => {
    slotCard.Text("Name", "Iron Sword");
})
.SetBackgroundColor(new Color(0.15f, 0.15f, 0.2f, 1f))
.SetPadding(10, 10, 10, 10)
.SetOnClick(() => {
    Debug.Log("Slot clicked!");
});

// Modifying Container Layout (Padding & Spacing & Alignment)
context.Vertical("Div", col => {
    col.Text("Label", "Inside Div");
})
.SetBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 0.5f))
.SetPadding(20, 20, 10, 10) // left, right, top, bottom
.SetSpacing(15)             // space between children
.SetChildAlignment(TextAnchor.UpperRight); // UpperLeft, UpperRight, MiddleCenter, etc.
```

---

## 7. Modal Creation & Customization (`UIGlobalModal`)

### Named Runtime Modals (`GetOrCreate`)
Modals are managed as persistent runtime instances keyed by their unique names. Multiple modals (e.g. A, B, C, D) can coexist at runtime simultaneously:

```csharp
// 1. Create or retrieve named modals
var modalA = UIGlobalModal.Create("Inventory");
var modalB = UIGlobalModal.Create("Stats");
var modalC = UIGlobalModal.Create("Death Screen");

// 2. Fluent chaining directly on the modal instance
modalA.SetTitle("Inventory")
      .SetWindowSize(new Vector2(800, 600))
      .Build(ctx => {
          ctx.Text("Label", "Inventory Content");
      });

modalB.SetTitle("Character Attributes")
      .SetWindowSize(new Vector2(400, 500))
      .Build(ctx => {
          ctx.Text("Label", "STR / AGI / INT");
      });
```

### Runtime Lifecycle
- **If the modal exists**: `UIGlobalModal.Create("Name")` returns the existing GameObject, opens it (`SetActive(true)`), and brings it to front.
- **If the modal does not exist**: It instantiates a new GameObject clone from the template and registers it under `"Name"`.
- **Closing**: Calling `UIManager.Instance.Close("Name")` or `modal.CloseSelf()` hides the modal (`SetActive(false)`). The GameObject remains in memory and can be reopened instantly without re-instantiating.
- **Destroying**: If you want to permanently delete the modal GameObject from the scene, call `modal.DestroyModal()` or `UIManager.Instance.DestroyModal("Name")`.

### Modal Window Controls
```csharp
var modal = UIGlobalModal.Create("Death Screen");

// Scroll Rect Movement Type
modal.SetScrollMovementType(ScrollRect.MovementType.Clamped); // Prevent elastic bouncing

// Window Auto-Fitting
modal.AutoFitWindow(fitWidth: false, fitHeight: true, paddingX: 0f, paddingY: 80f);

// Window Resizing (Corner Drag Handle)
modal.SetResizable(true); // Enable or disable drag resizing via bottom-right handle
modal.SetMinSize(new Vector2(300, 200)); // Minimum dimensions
modal.SetMaxSize(new Vector2(1920, 1080)); // Maximum dimensions
modal.SetResizeHandleVisible(false); // Can toggle handle visibility anytime

// Close Button Visibility
modal.SetCloseButtonVisible(false); // Hide the 'X' button

// Override Close Button Action
modal.OverrideCloseAction(() => {
    Debug.Log("Intercepted close button click");
});

// OnClosed Event
modal.OnClosed += () => {
    Debug.Log("Modal closed event fired");
};

// OnResized Event
modal.OnResized += (newSize) => {
    Debug.Log($"Modal resized to: {newSize}");
};
```

---

## 8. State Management & Reactivity
`UIBuilder` is **imperative**, not intrinsically reactive. If the underlying data changes, the UI will not automatically update.

**To handle state changes:**
Wrap your building logic in a method and call `RequestRebuild()` when state changes:

```csharp
private UIGlobalModal _modal;

public void Open()
{
    _modal = UIBuilder.BuildGlobalModal("My Window", BuildUI);
}

private void BuildUI(UIBuilderContext ctx)
{
    ctx.Text("Stat", $"Health: {PlayerData.Health}");
    
    ctx.Button("HealBtn", "Heal", () => {
        PlayerData.Health += 10;
        RequestRebuild(); // Trigger a UI refresh
    });
}

private void RequestRebuild()
{
    if (_modal != null)
    {
        _modal.Build(BuildUI);
    }
}
```

---

## 9. Scrolling & Overflow
The `UIGlobalModal` template handles scrolling internally using a `ScrollRect` (horizontal = false, vertical = true). 
- Always ensure root sections inside the modal call `.SetFitParent(stretchWidth: true, stretchHeight: false)` to fill the horizontal viewport cleanly.
- `Grid` containers automatically unconstrain horizontal width and enforce vertical sizing via `FixedColumnCount`, ensuring smooth vertical scrolling without horizontal clipping.

---

## 10. Using Custom Styles (UIElementRegistrySO)
If you need a specialized element (e.g. a specialized button style with custom sprites):
1. Create a Unity Prefab for it.
2. In the `UIElementRegistrySO` asset, add an entry:
   - **Type**: `Button` (or `Text`, `Image`, `Container`, `Custom`)
   - **StyleName**: `"SpecialButton"`
   - **Prefab**: [Link your prefab]
   - **TargetComponentPath**: (If the actual component is on a child object, specify the child path. E.g. `"HitBox/Button"`. If on the root, leave blank).
3. In code, simply call: `context.Button("SpecialButton", "Equip", () => { ... });`
