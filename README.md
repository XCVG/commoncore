# CommonCore RPG Libraries for Unity
### Version 2.0.1
### For Unity 2018.4+

## Introduction

CommonCore (formerly ARES) is a complete Role-Playing Game library for Unity... or will be someday. The intent is to provide a base that allows easy development of everything from quick adventures to epic open-world sagas, as well as being flexible enough to be adapted for mechanically similar genres such as open-world sandbox, shooters, and more.

CommonCore handles or will handle standard RPG mechanics, game state with saving and loading, the player object, NPCs, dialogue, input, UI, configuration and more. It is (or will be) a complete solution that can be loaded as a template followed immediately by building the actual game. For cases where you don't need all the functionality, it is divided into a separate Core, some modules, and RPGGame so you don't have to use it all (more modularity is planned).

## Documentation

Some limited documentation is available in the Reference folder. The Documentation folder, confusingly, contains documentation and license information for third-party components.

## Platform Support

CommonCore supports Unity 2018.4 and Unity 2019.3. The plan is to continue to target the latest stable version of 2018.x at least until the next release. Unity 2017.x is no longer supported.

CommonCore is targeted toward standalone platforms using Mono and the .NET 4.x scripting runtime. As of 2.0.0pre9 (18f00d1), it works with IL2CPP and has been tested on UWP, though this is still experimental and some features will not work.

## Extra Modules

Extra modules are located in a [separate repository](https://github.com/XCVG/commoncore-modules).

## Usage

With the exceptions listed below, this repository is dual-licensed under the MIT License (a copy of which is included in LICENSE.txt) and the Creative Commons Attribution 3.0 Unported license (a copy of which is included in ALTLICENSE.txt), with later versions permitted.

It is **strongly** recommended that you license derivatives under the MIT License or dual-license them rather than using the Creative Commons license exclusively. The MIT License is a GPL-compatible permissive software license approved by the Free Software Foundation and the Open Source Initiative, while the Creative Commons license is designed for creative works and has several ambiguities when used to license software. The Creative Commons option is provided primarily for compliance with game jams that require entries to be released under such a license.

Some open-licensed third-party assets are included in the repository. These are also listed in CREDITS along with their respective licenses. In general, all may be reused and distributed under the same conditions as the code even if the specific license differs.

**Please do not use the Ascension 3 name or Ascension 3 graphics in your own releases. The permissions granted above to not apply to these.** These should be stripped out already. The game data in Resources/, Objects/ and Scenes/, however, falls under the same license as the code and may be used under the same conditions.

## Ascension III (Demo)

[Ascension III](https://www.xcvgsystems.com/ascension-iii-2/) is the sequel to my previous game, Ascension 2: Galaxy. It is a full 3D role-playing game built on the Unity engine and based on ideas from the failed Ascension Revolution project and earlier Ascension III Awakening plans. 

Ascension III is being developed in parallel with CommonCore and it serves as a demo/example for the library. You can get the latest test build [here](https://xcvg.itch.io/ascension-iii-revolution).

