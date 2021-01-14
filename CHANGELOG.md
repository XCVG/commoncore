# 3.0.0 Preview 1

* Updated to Unity 2018.4.30
* Added OnLoadingSceneOpen script hook
* Fixed InitScene graphic stretched on ultrawide
* Changed LoadWheel graphic to not be colored by default
* Fixed typo of MuzzleTransform in DemoPistol WeaponViewModel
* Implemented auto-assignment of InstanceUID on inventory items
* Implemented GetByUID APIs on Inventory
* Added FillConstrained, Cover, and CharacterBottom image positions in dialogue
* Moved names of critical scenes (LoadingScene, MainMenuScene) to CoreParams
* Added SuppressThemeWarnings option
* Removed resource testing logic from CoreUtils
* Implemented fullscreen ImageFrame for dialogue system
* Implemented timed advance for dialogue system
* Implemented DummyWeaponItemModel (weapon model that can be equipped, but with no logic)
* Bullets are now passed their target when available
* Separated player initial state load logic, allowing reloading player state
* Added "keep existing color" element color to theme engine and ThemeableElement
* Added GetAs extension method for dictionaries
* Reworked Faction state to be part of GameState, not use static methods, and be mutable
* Added ScriptedAction state to Actors
* Last CampaignID is now saved to PersistState
* Added ExtraData to dialogue frames
* Added script hooks (named hooks) to dialogue system
* Implemented GameData module (experimental) for loading arbitrary model objects from JSON
* Improved damage and pain handling in actors, adding more extensibility
* Implemented extreme death flag and animations (FacingSpriteActorAnimationComponent only for now)
* Finished implemented Raise/Lower codepaths for Actors
* Added "current target" option for dialogue camera
* Implemented "hide object" options for dialogue
* Implemented "cutscene" pause level (between menu only and all)