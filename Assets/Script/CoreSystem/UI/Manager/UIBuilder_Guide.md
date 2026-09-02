# UIBuilder Framework Guide (For AI Agents)

**UIBuilder** is a custom, code-first UI framework designed for Unity. It allows developers (and AI agents) to dynamically generate native Unity UI (`GameObject`, `RectTransform`, `TextMeshPro`, etc.) using a declarative, lambda-based syntax similar to HTML/React or Unity's IMGUI (`OnGUI`).

This document is written to give future AI agents a complete understanding of how to use and extend this system to create beautiful, functional modals and UI without needing to read the entire project source code.

## 1. Core Architecture
- **`UIBuilder`**: The static entry point for creating UI. Usually, you call `UIBuilder.BuildGlobalModal(...)`.
- **`UIBuilderContext`**: The main context object passed into your lambda functions. It provides methods like `Div()`, `Horizontal()`, `Text()`, `Button()`, etc., which construct the actual GameObjects.
- **`UIGlobalModal`**: The default window/modal prefab instantiated by the UIBuilder. It handles its own scroll rect, title, and close button.
- **`UIElementRegistrySO`**: A ScriptableObject mapping that acts like CSS classes. It maps `UIElementType` + `styleName` to a specific **Prefab**.

## 2. Fallback Generation (The "Default" Style)
You **do not** need to provide a prefab for basic UI elements. If you use standard elements without mapping them in the registry, the `UIBuilderContext` will automatically generate them via code with sensible default styles (padding, margins, colors, alignments).

**Do NOT add basic elements (like Div, Text, Button) to the `UIElementRegistrySO` unless you specifically want to OVERRIDE the default styling with a highly customized graphic or animation.**

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
```

### Layout and Size Management
Every builder method returns a `UIElementRef` which allows you to chain layout modifiers using a fluent API. This is very useful when you want to force specific sizes or flexible layouts (like CSS Flexbox).

```csharp
// Force exact width and height (ignoring layout rules)
context.Button("Btn", "Click Me", () => {}).SetSize(200, 50);

// Set LayoutElement properties (best when inside a Horizontal/Vertical group)
context.Button("Btn", "Click Me", () => {})
       .SetPreferredSize(200, 50)
       .SetMinSize(100, 30);

// Flexible Width / Height (flex-grow)
context.Horizontal("Row", row => {
    row.Text("Label", "Name:").SetPreferredSize(100, -1);
    row.InputField("Input", "", (val) => {}).SetFlexibleSize(1, -1); // Fills remaining space
});
```

### HTML/CSS-like Styling (Text, Color, Spacing)
You can also chain style modifiers directly onto elements, eliminating the need for creating dozens of variations in the `UIElementRegistrySO`. This makes the system function similarly to inline CSS:

```csharp
// Modifying Text Properties
context.Text("Header", "Main Title")
       .SetFontSize(24)
       .SetTextColor(Color.yellow)
       .SetTextAlignment(TextAlignmentOptions.Center)
       .SetFontStyle(FontStyles.Bold | FontStyles.Underline);

// Modifying Colors
context.Button("Btn", "Danger", () => {})
       .SetColor(Color.red)       // Changes the button's image color
       .SetTextColor(Color.white); // Changes the button's text color

// Modifying Container Layout (Padding & Spacing)
context.Vertical("Div", col => {
    col.Text("Label", "Inside Div");
})
.SetBackgroundColor(new Color(0.1f, 0.1f, 0.1f, 0.5f)) // Adds a background color
.SetPadding(20, 20, 10, 10) // left, right, top, bottom
.SetSpacing(15)             // space between children
.SetChildAlignment(TextAnchor.UpperCenter);
```

## 4. State Management & Reactivity
`UIBuilder` is **imperative**, not intrinsically reactive. If the underlying data changes, the UI will not automatically update.

**To handle state changes:**
You must manually clear and rebuild the modal content. A common pattern is to wrap the building logic in a method and call a `RequestRebuild()` function when state changes.

```csharp
// Pattern Example
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
        RequestRebuild(); // Trigger a UI update
    });
}

private void RequestRebuild()
{
    if (_modal != null)
    {
        _modal.ClearContent();
        BuildUI(new UIBuilderContext(_modal.ContentContainer, UIManager.Instance.ElementRegistry));
    }
}
```

## 5. Scrolling & Overflow
The `UIGlobalModal` template handles scrolling internally using a `ScrollRect`. 
As long as you place your content inside the `UIBuilderContext` normally, if the generated UI exceeds the bounds of the modal window, a scrollbar will seamlessly handle it.

## 6. Using Custom Styles (UIElementRegistrySO)
If you need a highly unique element (e.g., a "Legendary Item Button" with particle effects):
1. Create a Unity Prefab for it.
2. In the `UIElementRegistrySO` asset, add an entry:
   - **Type**: `Button`
   - **StyleName**: `"LegendaryButton"`
   - **Prefab**: [Link your prefab]
   - **TargetComponentPath**: (If the actual `Button` component is on a child object, specify the path here. E.g., `"ClickArea"`. If it's on the root, leave blank).
3. In code, simply call: `context.Button("LegendaryButton", "Equip", () => { ... });`

*End of Guide.*
