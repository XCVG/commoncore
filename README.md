# CommonCore Framework for Unity
### Version 4.x.x _Downwarren_
### For Unity 2020.3+

## Introduction

CommonCore (formerly ARES) is a framework/library for Unity. The original intent was to create a freely usable framework for RPG and FPS-RPG games that allows easy development of everything from quick adventures to epic open-world sagas. Over time the design goals have been shifted to an internal project providing a flexible base for games across many disparate genres.

The full list of features is too long to list here and not well documented (yet). Among other things, CommonCore handles standard RPG mechanics (including levelling, characters with stats/skills, inventory, quests), game state with saving and loading, the player object, NPCs, first-person shooter mechanics, dialogue, input, UI, configuration, pausing/locking, and many miscellaneous utilites. It is meant to be used as a template which a game can be immediately build upon, and has a somewhat modular design so unnecessary components can be excluded (though this is still WIP).

## Documentation

Some limited documentation is available in the Reference folder. The Documentation folder, perhaps confusingly, contains documentation and license information for third-party components.

## Platform Support

CommonCore supports Unity 2020.3 and will _probably_ work on Unity 2021. The last stable version to support Unity 2019 was 3.1.0, and the last preview version was 4.0.0 Preview 2.

CommonCore is targeted toward standalone platforms using Mono and the .NET 4.x scripting runtime. Other configurations are not the focus and have varying levels of support. IL2CPP is supported with some limitations, and the WebGL and UWP platforms have been somewhat tested (both with their own limitations). Other platforms have not been tested but might work, maybe requiring minor tweaking.

## Other Repositories

Extra modules are located in a [separate repository](https://github.com/XCVG/commoncore-modules).

Miscellaneous bits and pieces are also located in [their own repository](https://github.com/XCVG/commoncore-misc).

## Usage

With the exceptions listed below, this repository is dual-licensed under the MIT License (a copy of which is included in LICENSE.txt) and the Creative Commons Attribution 3.0 Unported license (a copy of which is included in ALTLICENSE.txt), with later versions permitted.

It is **strongly** recommended that you license derivatives under the MIT License or dual-license them rather than using the Creative Commons license exclusively. The MIT License is a GPL-compatible permissive software license approved by the Free Software Foundation and the Open Source Initiative, while the Creative Commons license is designed for creative works and has several ambiguities when used to license software. The Creative Commons option is provided primarily for compliance with game jams that require entries to be released under such a license.

Some open-licensed third-party assets are included in the repository. These are also listed in CREDITS along with their respective licenses. In general, all may be reused and distributed under the same conditions as the code even if the specific license differs.

**Please do not use the Ascension 3 name or Ascension 3 graphics in your own releases. The permissions granted above to not apply to these.** These should be stripped out by now so you probably won't have to worry about them. The game data in Resources/, Objects/, and Scenes/ falls under the same license as the code and may be used under the same conditions.


