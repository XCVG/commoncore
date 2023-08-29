# CommonCore Framework for Unity
### Version 6.0.0 _Firene_ PREVIEW
### For Unity 2022.3

## Introduction

CommonCore (formerly ARES) is a framework/library for Unity. The original intent was to create a freely usable framework for RPG and FPS-RPG games that allows easy development of everything from quick adventures to epic open-world sagas. Over time the design goals have been shifted to an internal project providing a flexible base for games across many disparate genres.

The full list of features is too long to list here and not well documented (yet). Among other things, CommonCore handles standard RPG mechanics (including levelling, characters with stats/skills, inventory, quests), game state with saving and loading, the player object, NPCs, first-person shooter mechanics, dialogue, input, UI, configuration, pausing/locking, and many miscellaneous utilites. It is meant to be used as a template which a game can be immediately build upon, and has a somewhat modular design so unnecessary components can be excluded (though this is still WIP).

## Documentation

Some limited documentation is available in the Reference folder. The Documentation folder, perhaps confusingly, contains documentation and license information for third-party components.

## Features

**Core Services**

* Module system/modular architecture with well-defined startup sequence and events
* Dynamic resource manager with runtime loading and addon support
* Pause/lock management
* Broadcast tessaging system (QDMS) for loosely coupling components, primarily used for UI
* "Scripting" system for running bits of code by name or by hook
* String substitution, lookup, and macro system
* Command console integration currently set up for DevConsole 2 but adaptable to other implementations if desired
* Config system with persistence, extensibility, and built-in handling for basic settings
* Miscellaneous utilities and helper functions to make coding a little easier

**Mechanical Building Blocks**

* A defined flow of scenes that handles everything up to getting into your game itself
* Abstracted input system that can be extended with custom mapper implementations without changing game logic, and including a keyboard/mouse mapper that allows rebinding
* State management for building save/load systems providing extensible state objects with well-defined lifetimes and means of serializing and deserializing them
* Game world building blocks including bullet controllers and collision utility functions, the concepts of players, actors, and general entities and utility functions for working with them
* Action/Trigger components for building out generic world interactions between the player and other objects and mutating game state, with several premade components including some that integrate with RPG systems
* Theme engine for styling the user interface without changing every single object, plus a few other UI utilities including message modals

**Fully Realized Game Systems** 

* Full menu systems including an in-game menu, all open for customization and extension
* FPS-style player controller handling movement, actions, damage and weapons, with support for 2D and 3D weapon graphics and tight integration with RPG systems, extensible and divided into components
* Actor/NPC logic handling movement, attacks, damage and interactions, with support for navmesh, 2D and 3D world models, extensible and divided into components
* Full set of RPG mechanics including inventory, quests, leveling, stats/skills, open for extension, integrated with other components and ready to integrate into more
* Data-driven WRPG-style dialogue system with plenty of flexibility, integration with RPG systems and campaign state, and built for extensibility if you want to go beyond
* Saving and loading of world state including actors and player, integrated with state objects and saving of RPG state, and extensible to your own data

## Platform Support

CommonCore 6.x targets Unity 2022.3. The last version to support Unity 2021 is 5.0.0 and the last version to support Unity 2020 is 4.1.2.

CommonCore is targeted toward standalone platforms using Mono and the .NET 4.x scripting runtime. Other configurations are not the focus and have varying levels of support. IL2CPP is supported with some limitations, and the WebGL and UWP platforms have been somewhat tested (both with their own limitations). Other platforms have not been tested but might work, maybe requiring minor tweaking.

## Other Repositories

Extra modules are located in a [separate repository](https://github.com/XCVG/commoncore-modules).

Miscellaneous bits and pieces are also located in [their own repository](https://github.com/XCVG/commoncore-misc).

## Usage

With the exceptions listed below, this repository is dual-licensed under the MIT License (a copy of which is included in LICENSE.txt) and the Creative Commons Attribution 3.0 Unported license (a copy of which is included in ALTLICENSE.txt), with later versions permitted.

It is **strongly** recommended that you license derivatives under the MIT License or dual-license them rather than using the Creative Commons license exclusively. The MIT License is a GPL-compatible permissive software license approved by the Free Software Foundation and the Open Source Initiative, while the Creative Commons license is designed for creative works and has several ambiguities when used to license software. The Creative Commons option is provided primarily for compliance with game jams that require entries to be released under such a license.

Some open-licensed third-party assets are included in the repository. These are also listed in CREDITS along with their respective licenses. In general, all may be reused and distributed under the same conditions as the code even if the specific license differs.

**Please do not use the Ascension 3 name or Ascension 3 graphics in your own releases. The permissions granted above to not apply to these.** These should be stripped out by now so you probably won't have to worry about them. The game data in Resources/, Objects/, and Scenes/ falls under the same license as the code and may be used under the same conditions.


