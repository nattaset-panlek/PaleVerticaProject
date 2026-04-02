# Costume System Walkthrough
 
This guide explains how to use the **Costume System** within the Project Mobility Assistant to create and customize character cosmetics, including sprite layers and visual attachments.
 
## 1. Creating a Costume Item
1. Open the **Project Mobility Assistant** editor window.
2. Navigate to the **Items** tab.
3. Click **Add New** and select **Costume** from the **Item Type** dropdown.
4. Set the **Costume Slot** (Head, Body, Arms, Legs). This defines where the costume is equipped and which default nodes it targets.
 
## 2. Setting up Sprite Layers
Sprite layers are used to replace the default character textures (like armor designs or skin markers).
- Under **Costume Sprite Layers**, click **Add Sprite Layer**.
- Drag and drop your sprite into the slot.
- For **Arms** and **Legs** slots, you should ideally provide two sprites (Index 0 for Left, Index 1 for Right).
- **Layer Priority**: Controls the rendering depth order of the costume sprites.
 
## 3. Adding Visual Prefabs (Attachments)
This feature allows you to attach 3D/2D GameObjects (like hair, hats, shoulder pads, or special effects) directly to the character model.
- Under **Costume Visual Prefabs**, click **Add Visual Prefab**.
- Drag a **Prefab** from your Project folder into the slot.
- **Target Node**: Select the model part this prefab should follow (Head, Body, Arms, Legs).
- **Transform Controls**:
    - **Offset**: Relative position to the target node's pivot.
    - **Rotation**: Local Euler rotation.
    - **Scale**: Multiplier for the prefab's size (default is 1,1,1).
 
## 4. Runtime Behavior
- **CharacterVisualController.cs** manages the visual updates.
- When a costume is equipped, the system:
    1. Swaps the `SpriteRenderer` sprites based on the Sprite Layers list.
    2. Instantiates all items in the Visual Prefabs list as children of the designated body parts.
- When unequipped, prefabs are automatically destroyed to keep the scene clean.
 
## 5. Requirements
- **Resources Folder**: All Sprites and Prefabs **MUST** be located inside a folder named `Resources` (or its subfolders) for the Item Database to load them correctly at runtime via JSON.
- **Unique IDs**: Each costume item must have a unique Item ID.
