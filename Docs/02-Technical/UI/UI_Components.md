# UI Components Library

This directory contains reusable, premium UI components that can be easily instantiated from the Unity Context Menu.

## How to use
1. Right-click in the **Hierarchy** window.
2. Select **UI > Custom Components**.
3. Choose the component you want to create (e.g., **Switch Toggle**).

## Structure
- **Scripts**: The runtime logic for each component is located here.
- **Editor**: Menu extensions and setup logic are located in `Assets/Script/Editor/CustomUICreateMenu.cs`.

## Features
- **Switch Toggle**: A modern, animated toggle switch using DOTween.
- (More items can be added here following the same pattern)

## Guidelines for adding new items
1. Create a runtime script in this folder.
2. Add a MenuItem entry in `CustomUICreateMenu.cs`.
3. Ensure the creation logic automatically sets up the required GameObject hierarchy and components.
