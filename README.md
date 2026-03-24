# Bard Dialogue System

A customizable dialogue and quest system with visual node-based editors for Unity, built on top of xNode.

Provides a visual editor that allows you to design dialogue trees with complex branching and quest graphs, along with the data model and backend to support them. \
This package intentionally does not include runtime managers or a plug-and-play dialogue runner — you wire those up yourself to fit your project's architecture.

## Requirements

- [Bard xNode Fork](https://github.com/blahnumbers/xNode) — includes performance improvements, xNodeGroups, and other changes required by this package.
- [UniTask](https://github.com/Cysharp/UniTask)
- [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes)

> **Unity version:** I develop and test using Unity 6000.0+, so earlier versions are not being actively tested. If you'd like to use this in a legacy version and run into issues, feel free to open an issue.

## Installation

### 1. Install Dependencies

Install the following packages first via whichever method you prefer (git URL, OpenUPM, manual):

- [Bard xNode Fork](https://github.com/blahnumbers/xNode)
- [UniTask](https://github.com/Cysharp/UniTask)
- [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes)

### 2. Install Bard Dialogue System

In Unity, open **Window → Package Manager**, click **+** and select **Add package from git URL**, then enter:

```
https://github.com/blahnumbers/BardDialogueSystem.git?path=Assets/Scripts
```

To target a specific version, add `#v0.1.2` (or any other valid version tag) at the end.

## Setup

On first install, the package will automatically create the necessary configuration assets and open the **Project Settings → Bard Dialogue** window. Configuration assets are created under `Assets/Bard/Config` by default.

## What's Included

- xNode-based visual editors for dialogue trees and quest graphs
- Dialogue message and action system with a configurable type registry
- Quest step and condition tracking with auto-generated C# definition classes for compile-time safety
- NPC definition config with grouped ID management
- Project settings window for managing all registries

## Using Exported Data in Your Project

Dialogue trees and quest graphs can be exported to JSON via right-click in the xNode editor. Exports respect the folder structure of your graphs and generate both data and localization files in their respective locations.

If multiple graphs share the same folder — for example, to separate non-intersecting dialogue branches for the same character — they are combined into a single JSON datafile. This lets you fetch all dialogue belonging to a given NPC in one load operation from your game logic.

The intended runtime flow is: load the JSON containing the dialogue or quests data, deserialize it into the provided data classes (either `List<DialogueTree>` or `List<Quest>`), and drive it from your own manager implementation.

For now, the best reference for integration is the source code itself — `DialogueTree`, `DialogueMessage`, and related classes are a good starting point for understanding the data model. Proper integration documentation will be made available in a future release.

## What's Not Included

This package provides the data model and editor tooling. You will need to implement:

- A dialogue runner / manager
- A quest manager
- Localization backend (interface is provided)
