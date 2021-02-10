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

# 3.0.0 Preview 2

* Updated to Unity 2019.4.17
* Updated dependency package versions
* New loading indicator (finally!)
* Fixed fundamentally flawed "cutscene" pause level
* FormID of Player is now set to "Player"
* Fixed theme font being applied inconsistently to toggle labels
* Fixed some panels not using heading font for headings
* Added scoped (different lifetimes) generic stores to MetaState
* Interaction with NPCs can now be blocked by faction relationships
* FacingSprite is now called via camera events (still slow, but handles multiple cameras)
* Magic points added to CharacterModel
* Aid items can now execute a script on use
* Aid items can now restore shields or magic
* Improved result message and result message handling for aid items
* Added ComboAidItemModel that supports multiple aid types/effects
* Dialogue frame options now supports Fixed height type
* TextFrame now supports allowSkip
* Added IReceiveEntityEvents interface for components to receive entity event calls (consider this API unstable)
* Dialogue system now partially supports keyboard/controller option selection
* Added UnityWebRequest Resource Importer for handling ogg and mp3 audio
* Resource Importers are now (effectively) tried last-registered-first
* Fixed some Resource Importers not setting name of imported objects
* Version info is now saved to PersistState, ConfigState, and game saves (setting up for migrations)

# 3.0.0 next version

* VoiceOverride now supports relative (unprefixed) or absolute (starts with /) paths
* DialogueNavigator robustness and use key handling
* Added variant of AsyncUtils.RunWithExceptionHandling that returns Task
* Added LockPause-aware wait method to AsyncUtils
* Implemented VoiceVolume property in dialogue frame Options
* Changed some labels in GameplayConfigPanel to overflow (fix for some themes)
* Fixed DialogueParser throwing an exception when loading dialogue with Exec microscripts with args
* Added editor script to copy files post-build
* Added datetime and guid to finalsave
