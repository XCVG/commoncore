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

# 3.0.0 Preview 3

* VoiceOverride now supports relative (unprefixed) or absolute (starts with /) paths
* DialogueNavigator robustness and use key handling
* Added variant of AsyncUtils.RunWithExceptionHandling that returns Task
* Added LockPause-aware wait method to AsyncUtils
* Implemented VoiceVolume property in dialogue frame Options
* Changed some labels in GameplayConfigPanel to overflow (fix for some themes)
* Fixed DialogueParser throwing an exception when loading dialogue with Exec microscripts with args
* Added editor script to copy files post-build
* Added datetime and guid to finalsave
* Debug save methods now log full filename to console
* Added ReservedUIDs option to CoreParams to reserve low UID range for preassignment
* Fixed IGUI_Menu breaking in non-16:9 aspect ratios
* Fixed PersistState.IsFirstRun not JsonIgnore'd
* Fixed forcing crosshair on/off not immediately applying to HUD
* Fixed autoaim never fully disabling
* Implemented game-specific input maps in ExplicitKBMInput
* Fixed shield recharging after player is dead
* Fixed exception in PlayerShieldComponent when reloading save
* Kill console command now works on player and ITakeDamage, not just actors
* Fixed actor FeelPain toggle having no effect in some cases
* Added configurable threshold for actor deciding to flee
* Added InitiateDialogueSpecial (action special that does exactly what it says on the tin)
* Made some members of bullet explosion script public (experimental)
* Added TransformCopyScript (experimental)
* Added deferred effect and a few extra options to BulletScript
* Fixed Dialogue HideObjects not parsing correctly
* Added EquipItem console command
* Fixed doubled submit when using Use key in dialogue
* Added a brief delay before dialogue exits (effectively debounces Use)
* Updated Unity Input Mapper config modal message to reflect the new reality
* Added AnimateQuadScript and TextureAssignScript (hacky/experimental dynamic texture assignment)
* Enabled incremental GC
* Added CollectGarbage API to CoreUtils
* Added teleport and list entities console commands
* Added config flag for generating keymap on startup (InputDumpKeycodes)
* Implemented infrastructure for informing player of pending changes or needed restart in options panel
* Fixed MusicFader restarting music that should be stopped
* Implemented utility interfaces for thunking values from classes in Assembly-CSharp to Core
* Implemented timescale setting in GameState
* Implemented dialogue trace (WIP)
* Implemented lighting for sprite weapon viewmodels (WIP)

# 3.0.0 Preview 4

* Added args field to ScriptExecutionContext
* Added raw data, references to scene and base frame to dialogue frame objects
* Fixed ChaseOptimalDistance sanity check being backwards
* Added flag for returning to dialogue after shop (experimental)
* Implemented fake physics for actors, allowing them to be pushed (WIP)
* Implemented option for dialogue controller to hide HUD
* Added support for magic indicator to RPG HUD controller
* Added Terminated flag to CCBase, indicating that CommonCore has shut down
* Added clamp angle function to MathUtils
* Fixed scene override on new game not working
* Custom difficulty can no longer be selected from the ingame UI
* Door open/closed state can now be persisted across scene change
* Revamped player spawn logic with explicit PlayerSpawnPoint script
* Items are now equipped in CharacterModel by item ID
* FlyingActorMovementComponent thresholds are no longer hardcoded
* Minor fix in selection logic for FacingSpriteActorAnimationComponent (will now definitely pick first option)
* Messaging system no longer locks up or lags when messages are sent from message receivers (fixed handle-before-enqueue logic in QdmsMessageInterface)
* ResourceManager is no longer case sensitive (behaviour now consistent with Unity)

# 3.0.0 Preview 5

* Added ability for items to execute scripts on pickup, use, drop etc
* Fixed actor FleeHealthThreshold not disabled when set to 0
* FacingSpriteActorAnimationComponent now uses walking animation if running animation is not available
* Added console command to set faction relationships
* Preemptive fix to EntityPlaceholder (could double-activate in a theoretical edge case)
* Fixed DialogueController placing Character-position portrait at wrong height 
* Added startup metadata to PersistState
* Added consistent support for both `av` and `actorvalue` in DialogueParser

# 3.0.0 Preview 6

* Fixed editor file copy script not creating root dir
* Added Reset functionality to SkippableTimerScript
* Added option for ApplyThemeScript to apply when theme policy is ExplicitOnly or not
* Cleaned up handling of base frame in dialogue scene (now available via DialogueScene.BaseFrame)
* Suppressed warnings from ExplicitKBMInputMapper when noclipping (and other "missing axis" warnings as well)
* Changed ammo type backing type from AmmoType enum to string
* GetActiveCamera now explicitly rejects ViewModel and LightReporter cameras
* Autoaim now works properly when cast includes the ground/non-hittable objects
* Implemented different difficulty handling for actors considered followers vs normal actors
* Added caller and dynamics data to ActionInvokerData
* Assemblies are now loaded from StreamingAssets/managed
* Cleaned up data types and methods of QdmsKeyValueMessage
* Added lock time to ranged weapons
* Added burst fire to ranged weapons
* Implemented projectils-per-shot and ammo-per-shot for ranged weapons
* BulletExplosionComponent (no longer experimental) optionally detonates on world hit and despawn
* Moved GameData module out of experimental status and into Core
* Theme engine can now apply themes to scrollbars
* ResourceHandle now has ResourceType property
* Added OnFire and OnReload to item scripts (for weapons only)
* FacingSprite and BillboardSprite now has a "Bright" flag that draws at full brightness
* Fixed heading font not applying to modal headings
* Fixed BlankSceneController not added to script execution order
* Implemented Generic Translate Script
* Added flag to pre-activate entity placeholders in world scenes
* Added UI sound type (PlayUISound now uses this sound type)
* Removed obsolete methods from AddonBase
* Added VideoModule to provide addon-aware video management
* Added VideoUtils to video module, containing video-related convenience methods
* Weapon ViewModel can be forced to always or never wait for lock time to do effect [Experimental]
* Added subtitle convenience methods from Shattered 2 [Experimental]
* Added fade canvasgroup convenience methods [Experimental]
* Extended projectile and explosion handling for weapons [WIP]
* Extended effects handling for sprite weapon view models [WIP]
* Melee weapon damage can be delayed [WIP]
* Themes are now registered automatically upon loading from addons [Untested]

# 3.0.0 pNext

* Added RpgInventoryModified messages sent when inventory is changed
* Fixed OnFire and OnReload scripts being passed item model instead of item instance
* Melee weapon damage can be delayed no longer WIP
* Added recock handling to weapons [WIP]