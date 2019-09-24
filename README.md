# CommonCore RPG Libraries for Unity
### Version 2.0.0pre
### For Unity 2018.3+

## Important Update

As of May 2019, CommonCore has been downgraded to an internal project. It will still be released as open-source, but the focus will be on implementing features for Ascension III and other games, commits to this repository will be fewer and releases will be less polished. This project was starting to become unmanagable and I need to focus on the game itself.

## Introduction

CommonCore (formerly ARES) is a complete Role-Playing Game library for Unity... or will be someday. The intent is to provide a base that allows easy development of everything from quick adventures to epic open-world sagas, as well as being flexible enough to be adapted for mechanically similar genres such as open-world sandbox, shooters, and more.

CommonCore handles or will handle standard RPG mechanics, game state with saving and loading, the player object, NPCs, dialogue, input, UI, configuration and more. It is (or will be) a complete solution that can be loaded on top of Unity followed immediately by building the actual game.

## Platform Support

CommonCore supports Unity 2018.3 and (probably) later. The plan is to continue to target the latest stable version of 2018.x for the forseeable future. Unity 2017.x is no longer supported, and Unity 2019.x will probably work but hasn't been tested yet.

CommonCore supports standalone platforms using Mono and the .NET 4.x scripting runtime. The plan is to get the basic core working on mobile and WebGL (IL2CPP) platforms within the Balmora release, but not the full feature set (at least, not yet).

## Extra Modules

Extra modules are located in a [separate repository](https://github.com/XCVG/commoncore-modules).

## Usage

With the exceptions listed below, this repository is dual-licensed under the MIT License (a copy of which is included in LICENSE.txt) and the Creative Commons Attribution 3.0 Unported license (a copy of which is included in ALTLICENSE.txt), with later versions permitted.

It is **strongly** recommended that you license derivatives under the MIT License or dual-license them rather than using the Creative Commons license exclusively. The MIT License is a GPL-compatible permissive software license approved by the Free Software Foundation and the Open Source Initiative, while the Creative Commons license is designed for creative works and has several ambiguities when used to license software. The Creative Commons option is provided primarily for compliance with game jams that require entries to be released under such a license.

Some open-licensed third-party assets are included in the repository. These are also listed in CREDITS along with their respective licenses. In general, all may be reused and distributed under the same conditions as the code even if the specific license differs.

**Please do not use the Ascension 3 name or Ascension 3 graphics in your own releases. The permissions granted above to not apply to these.** If you download a release package or recent source, these should be stripped out already. The game data in Resources/, Objects/ and Scenes/, however, falls under the same license as the code and may be used under the same conditions.

## Ascension III Revolution (Demo)

Ascension III: Revolution is the sequel to my previous game, Ascension 2: Galaxy. It is a full 3D role-playing game built on the Unity engine and based on ideas from the failed Ascension Revolution project and earlier Ascension III Awakening plans. 

A partial demo/prototype of Ascension III serves as the example project for CommonCore.

