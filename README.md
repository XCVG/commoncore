# CommonCore RPG Libraries for Unity
### Version 2.0-pre2
### For Unity 2018.3+

## Important Update

As of May 2019, CommonCore has been downgraded to an internal project. It will still be released as open-source, but the focus will be on implementing features for Ascension III and other games, commits to this repository will be fewer and releases will be less polished. This project was starting to become unmanagable and I need to focus on the game itself.

## Introduction

CommonCore (formerly ARES) is a complete Role-Playing Game library for Unity... or will be someday. The intent is to provide a base that allows easy development of everything from quick adventures to epic open-world sagas, as well as being flexible enough to be adapted for mechanically similar genres such as open-world sandbox, shooters, and more.

CommonCore handles or will handle standard RPG mechanics, game state with saving and loading, the player object, NPCs, dialogue, input, UI, configuration and more. It is (or will be) a complete solution that can be loaded on top of Unity followed immediately by building the actual game.

## Project Philsophy


* Elegance and ease of use take precedence over performance. If it speeds the game up but requires the user of the library to do mental backflips, it's not going in.
* Provide flexibility and transparency to the developer using the library and to the player of the game. No black boxes, and allow multiple approaches when it makes sense to do so.
* Build more than just enough, but not too much. Strike a balance between targeting specific cases and building more general systems. This is hard!
* Build a complete package that provides core functionality without dependencies on other assets (FOSS inclusions are okay). But also provide modularity, extensibility, and optional integrations to go past what's built here.
* Fault tolerance! Handle errors gracefully, attempt to correct common mistakes, and continue execution until no meaningful execution is possible. At the same time, we should also make it clear that errors are occurring rather than swallowing them entirely.
* Game data should be human-readable, which allows easier debugging and manual modification if necessary. JSON was used in most cases.

## Platform Support

CommonCore supports Unity 2018.3 and (probably) later. The plan is to continue to target the latest stable version of 2018.x for the forseeable future. Unity 2017.x is no longer supported, and Unity 2019.x is very much a "we'll see" affair.

CommonCore supports standalone platforms using Mono and the .NET 4.x scripting runtime. The plan is to get the basic core working on mobile and WebGL (IL2CPP) platforms within the Balmora release, but not the full feature set (at least, not yet).

## Modules

CommonCore consists of several modules that encompass various functionality.

* Audio: Handy fire-and-forget audio and music player
* Base: Module loading, lifecycle handling, utility functions
* Config: Graphics, sound, and other configuration state and modification
* Console: Glue that handles interfacing with third-party command console
* DebugLog: Extended debug logging
* Dialogue: Flexible text/graphical dialogue system
* Input: Mapped input system with support for multiple backends
* LockPause: Flexible input locking and pause handling
* QDMS: Simple general-purpose messaging system
* RPG: Role-playing mechanics and state including inventory and stats
* Scripting: Arbitrary method-as-script calling system
* State: Game state storage, save/load
* StringSub: String substitution, lookup, and macros
* TestModule: Dummy module to verify lifecycle handling
* UI: Utility methods and implementation for user interface
* World: Actors, objects, controllers and interaction

Right now the modules are quite interdependent, but separating them is a design goal and work in progress. It's also likely that some will be broken down or combined.

## Current Status

CommonCore is a **work in progress**, heading towards its initial release. You can fork it and work with it, of course, but major changes will be made and I offer **zero** guarantees of API stability or anything like that.

**Implemented Features**

This list is always in flux. As I implement features, I find there are even more I need to add.

* Basic framework, lifecycle handling
* JSON, resource, and scene utility methods
* Quick and Dirty Messaging System
* Mapped Input framework
* UnityInput mapper
* Command console using third-party console system
* Wrapped debug logging
* Robust (ish) input locking
* Robust (ish) pausing
* Complete dialogue system
* Complete inventory system
* Complete levelling and stats system
* String substitution, lookup, and macro engine
* One-line UI utility modals
* Action Special system (quickly create interactivity)
* Game state/objects saving and loading
* Extensible scene controllers
* Extensible actor controllers
* Basic state-machine NPC AI
* Script execution by name
* Save folder creation and fail handling
* Shop/container system (same base logic)
* Basic config options
* Time passage and timed events
* UI "type on" utility function
* Map screen with map markers
* First/third person melee/ranged combat
* Basic faction system
* Basic stealth mechanics (weighted detection)
* custom dev console

**In-Progress Features**

* Loading screen (appears but needs animation and better error handling)
* Graphics, sound, and other configuration
* In-game remappable inputs (ExplicitKBM and XInput backends)
* Gamestate observer and triggered events

**Planned Features**

* Levelled Lists
* improved messaging (MSDMS)
* object pooling (POS/Calandria)
* Extended faction/reputation system
* magic
* proper stealth
* UI theming 
* Mod support! (eventually)

## Roadmap

A rough plan is in place for the future of CommonCore.

* 1.x _Arroyo_ : Basic features complete.
* 2.x _Balmora_ : Separate modules, remove dependencies, quality-of-life, general cleanup and documentation.
* 3.x _Citadel_ : UI theming and initial mod support (data overrides, script hooks).
* 4.x _Downwarren_ : Further mod support. Performance and QoL features. Mechanics improvments?
* 5.x _Endar Spire_ : Vehicles? Even more mod support?
* 6.x and beyond : Keep iterating, improving and adding features as I think of them.

There is no formal timeline. Ultimately, this is a hobby project for me, and is subject to the vagaries of real life. With that being said, I hope to have Balmora complete with a demo game in 2019.

## Usage

With the exceptions listed below, this repository is dual-licensed under the MIT License (a copy of which is included in LICENSE.txt) and the Creative Commons Attribution 3.0 Unported license (a copy of which is included in ALTLICENSE.txt), with later versions permitted.

It is **strongly** recommended that you license derivatives under the MIT License or dual-license them rather than using the Creative Commons license exclusively. The MIT License is a GPL-compatible permissive software license approved by the Free Software Foundation and the Open Source Initiative, while the Creative Commons license is designed for creative works and has several ambiguities when used to license software. The Creative Commons option is provided primarily for compliance with game jams that require entries to be released under such a license.

CommonCore relies on a few third-party packages available from the Asset Store, listed in CREDITS. These will likely be replaced in future versions, but for the time being CommonCore won't work without at least DevConsole 2 and Post-Processing Stack. Some of these require modifications or altered install paths, listed in CREDITS.

Some open-licensed third-party assets are included in the repository. These are also listed in CREDITS along with their respective licenses. In general, all may be reused and distributed with the same conditions as the code even if the specific license differs.

**Please do not use the Ascension 3 name or Ascension 3 graphics in your own releases. The permissions granted above to not apply to these.** If you download a release package, these should be stripped out already. The game data in Resources/, Objects/ and Scenes/, however, falls under the same license as the code and may be used under the same conditions.

## Ascension III Revolution (Demo)

Ascension III: Revolution is the sequel to my previous game, Ascension 2: Galaxy. It is a full 3D role-playing game built on the Unity engine and based on ideas from the failed Ascension Revolution project and earlier Ascension III Awakening plans. 

A demo/prototype of Ascension III serves as the example project for CommonCore. At some point, Ascension III will be forked and development of the library (public) and game (private) will continue in parallel.

