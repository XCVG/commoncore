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

# 3.0.0 Preview 7

* Added RpgInventoryModified messages sent when inventory is changed
* Fixed OnFire and OnReload scripts being passed item model instead of item instance
* Melee weapon damage can be delayed no longer WIP
* Moved input lock and player in control checks in weapon controller to a more correct spot
* Fixed some effect fields on SpriteWeaponViewModelScript not serialized
* Added magazine and reload effect handling to SpriteWeaponViewModelScript and RangedWeaponViewModelScript
* Redirect assets now support relative paths (and use these by default!)
* Factions are now entirely case insensitive
* Added multiple hit handling to melee weapons
* Added fractional restore options for aid items
* Demo weapon arm animations now have consistent length
* Added recock handling to weapons
* Added override component for sprite weapon lighting
* Fixed sprite weapon lighting so it actually applies
* Added Bright flag for sprite weapons (ignores reported lighting)
* Added StringUtils with case-insensitive string contains
* Added SetVelocity to PlayerMovementComponent
* Added RepeatFireSound flag to ranged and sprite weapon viewmodels
* ShowDialogueSubtitle can now take a Color for colour argument
* Added some handling for fatal errors during startup
* Added editor script that adds build info to build
* ActorAttackComponent now derives from abstract base class
* ActorAttackComponentBase derivatives can optionally handle target selection and chase destination
* Swinging doors can now open away from the activator
* Added load-scene-directly cheat (WarpDirect)
* Added CheckPositionReachable and IsStuck to ActorMovementComponent (limited implementations)
* Added CheckLineOfSight function to ActorAttackComponentBase
* Target point is now defined on IAmTargetable interface
* Added support for glue-to-bone to ranged weapon viewmodel [Experimental]
* Added contact-hit hack to melee weapons [Experimental]
* Implemented HUD scaling [Experimental]
* Implemented resource manifest generated on build [Experimental]
* Implemented GetFolders* APIs in ResourceManager using resource manifest [Experimental]
* Added optional line of sight check to ActorAttackComponent [Experimental]
* Fixed DialogueNavigator NRE [Untested]

# 3.0.0 Preview 8

* Fixed ExtraData on InventoryItemModels not being loaded
* Fixed swinging door away from activator not working at some angles
* Subtitle time now advances when game is in AllowCutscene pause state
* Cleaned up some use of obsolete APIs and unused variables
* Moved DragonAIController out of RPGGame
* CharacterModel messages now include reference to the specific CharacterModel
* Receivers of CharacterModel messages now check if it refers to the player CharacterModel
* Original colors of health/shield/energy/magic bars in RpgHUDController are now publically visible
* Placeholder text for ammo/weapon in RpgHUDController can now be disabled
* Generalized ADS enter/leave state handling in PlayerWeaponComponent/WeaponViewModelScript
* Fixed SpriteWeaponViewModelScript not being put into ADSRaise/ADSLower state
* Fixed RangedWeaponViewModelScript being put into ADSRaise when it should be put into ADSLower
* Fixed noclip up/down movement not working
* Fixed quest stage text not being cleared when quest selected
* Fixed facing sprites not handling Bright flag and emissive texture properly
* Fixed Tags recursively calling its own getter in BaseController/ActorController
* Removed stray GetGameplayConfig in ConfigState
* HandleRestorableExtraData now defaults to true
* Action Triggers now default to repeatable
* FindObjectByTID and FindEntityByTID now uses CoreUtils.GetWorldRoot
* FindEntityByTID logic now completely ignores non-entity objects (previously could cause problems if name/TID was the same)
* Deprecated SceneUtils.FindAllGameObjects (logic is not safe)
* Added NoShieldRecharge PlayerFlag
* Made several methods of PlayerShieldComponent public
* Fixed IGUI tabs added after the fact not receiving themeing
* Fixed Button text going invisible when element color class is overridden to None
* Exposed a few state fields in PlayerWeaponComponent as read only properties
* Fixed AutosaveOnEnter saving inconsistent state
* Improved a few edge cases in ActorFollowerTetherEx
* PlayMusic no longer restarts songs if they are already playing
* Fixed NRE when reloading save on SpriteActorAnimationComponent
* Added HasVideo API to VideoModule
* Added ConditionalFilter that filters action special triggers by evaluating a Conditional
* Added explicit EndQuest API (SetQuestStage no longer sends "quest finished" message on negative quest stages)
* Added handling for shields, energy, and armor to StatusPanelController
* StatusPanelController will now (generally) try substitution lists for terms
* StatusPanelController now properly supports all builtin gender values
* Added RecoilEffectScale/ADSRecoilEffectScale to ranged weapon item models
* Added generalized "slide show" for scripted sequences
* Dialogue backgrounds are now fit to rather than enveloping screen (intended behaviour, previous setting was an error)
* Fixed dialogue reactivating hidden objects even if they were initially inactive
* Added new FindDeepChildren to SceneUtils and deprecated semi-broken FindAllGameObjects
* Added new FindDeepChildrenIgnorePlaceholders to WorldUtils
* Some WorldUtils utility methods now return concrete List<T> instead of IList<T>
* ActorController now uses FindDeepChildren instead of FindAllGameObjects to unswizzle target after loading a save
* Fixed incorrect target path for resource manifest when building for macOS
* Added NavigationNodeEx script [Experimental]
* Fixed ThemeableElement and NonThemeableElement being ignored on child components of complex elements [Untested]
* Added MaxChargeFraction to ShieldParams [Untested]

# 3.0.0

* Addon loading now ignores all files starting with . and all .meta files
* Fixed incorrect logic for chase destination range in ActorController and ActorAttackComponent
* Fixed unstable/incorrect closest hit detection in ActorAttackComponentBase.CheckLineOfSight

# 3.1.0

* DialogueNavigator now fails gracefully (no error spam) when a dialogue does not exist
* SlideshowControllerEx no longer throws if a slide cannot be found
* More logical handling of shield recharge after item equip (behind a GameParams flag)
* Save game messages now use ephemeral modals
* Added guard against target going null in ActorAttackComponent.DoAttack
* Integral values are now compared as integers in Conditionals
* Fixed shields recharging past limit when MaxChargeFraction < 1
* Added attack repeat functionality to ActorAttackComponent [Experimental]
* Added AudioClipAssignScript [Experimental]

# 4.0.0 Preview 1

* Updated CREDITS
* Options panel now displays game name and versions of game, core, and engine
* Options subpanels can be added with a builder function instead of prefab (and always use a build func internally)
* RpgChangeWeapon message is now RpgEquipmentChanged, more accurately reflecting its actual function
* PlayerShieldComponent now properly handles separate cases of stats changed and shield equipment changed
* Updated jilleJr/Newtonsoft.Json-for-Unity to 12.0.302
* Added Humanizer library
* Split WorldUtils.IsAlive into separately named methods
* Removed obsolete GameParams
* Added exception handling for dialogue microscripts and conditionals (behind GameParam)
* SlideshowControllerEx now tries to use dialogue characters and backgrounds as slides
* OnFire and OnReload scripts are now passed item model in addition to instance
* Added PlaySound overloads with more params (like PlaySoundEx, but does not throw)
* Added options to not receive messages on inactive components (QdmsMessageInterface and QdmsMessageComponent)
* Deprecated PlayerController PlayerInControl and AttackEnabled fields
* Revised player spawn logic
* CollectionUtils dictionary methods can now handle IReadOnlyDictionary<T> as well
* Added script hooks to config panel open/render
* DestroyableThingFacingSpriteComponent no longer needs to be on same object as DestroyableThing
* Added IReceiveDamageableEntityEvents with DamageTaken and Killed called by damageable entities
* IGUI panels can be added with a builder function instead of prefab
* Config panels now inform user about pending changes
* Added Coroutine methods to AsyncUtils
* Fixed GameOverScene using incorrect handler for Reload button
* Dialogue controller no longer resets music if it does not actually change music
* Improved error handling in addon/resource loading
* Added interim ModuleParams to CoreParams [Experimental]
* Added RunOnMainThread methods to AsyncUtils [Experimental]
* QdmsMessageComponent can now translate and rebroadcast as Unity messages [Experimental]

# 4.0.0 Preview 2

* Added custom VersionConverter JsonConverter to CCJsonConverters
* Added more JSON handling methods to CoreUtils
* Implemented generalized migration handling
* Implemented migration for ConfigState
* Implemented migration for PersistState
* Added init function run only on first create to ConfigState
* Added AddonData LazyLooseDictionary to GameState
* Implemented previously unimplemented methods in LazyLooseDictionary
* Implemented save metadata including thumbnails
* Attempting to save a new save with the same filename as an existing save now warns
* Base difficulty is now saved into base ConfigState instead of GameplayConfig
* Initial, current, highest, and lowest difficulty level is now saved to game saves
* Registering a script method with a duplicate callable name now replaces existing with warning instead of throwing
* Removed obsolete methods from ScriptingModule
* Added tags to HUD push messages
* Implemented handling for generic custom Modal
* Added generic text entry Modal
* Moved Modal prefabs to UI/Modals/*
* Renamed FXAAToggleTackon to PostProcessingV2ConfigTackon
* Added Brightness value to config
* Implemented Brightness implementation via postprocessing in PostProcessingV2ConfigTackon
* Added Brightness slider to options panel
* Quality level is now stored in ConfigState and applied at runtime
* CoreParams.DefaultJsonSerializerSettings is now used in a few more places
* Added script hooks before and after save read/write and serialize/deserialize
* Added script hooks after entity or effect spawn (behind CoreParam)
* Added Saved Games folder as an option for persistent data path on Windows
* Conditional and MicroscriptNode are now extensible via ConditionalResolver and MicroscriptResolver respectively
* Resource manifest is now only loaded if TryLoadResourceManifest CoreParam is set to true
* Fixed postbuild buildinfo and file copy scripts using incorrect paths for UWP
* Fixed attempting to use Windows desktop file paths on UWP
* Fixed attempting to create root data folder (not allowed) on UWP
* Fixed compile error on UWP because AudioSpeakerMode.Raw is no longer defined
* Switched to recommended setup for jilleJr/Newtonsoft.Json-for-Unity (modified assemblies for all platforms)
* Added CoreAotTypeEnforcer to resolve issues with IL2CPP stripping
* Resource manifest is now created before build and saved to StreamingAssets (PreBuildGenerateResourceManifest)
* Added GetDescriptorForAxis and GetDescriptorForButton APIs to input mappers for getting names of mappings etc [Experimental]
* Implemented migration for save games on load [Untested]

# 4.0.0 Preview 3

* Removed UI folder (remaining contents moved to Shared)
* Added speaker name and better color handling to SubtitleUtilsEx
* Fixed PostProcessVolume layer colliding with other layers
* Allow and attempt impossible skill checks can now be overridden with dialogue scene/frame options
* Fixed FacingSpriteActorAnimationComponent sequence not found error when target animation exists in ExtraAnimations but is not first
* Implemented monitor selection in settings (desktop only)
* Added ActivatorFilter to ObjectActions
* Added ContainsSpecific API to main playerflags source to check if it is in that source specifically
* ViewModel now collides with ViewModel
* Moved NavigationNodeEx out of experimental into World (as NavigationNode)
* Fixed ResourceFolder ExploreForType ignoring redirects (fixes GetResources and a few other things)
* Added GetHandlesAll API to resource management
* Updated to Unity 2020.3 LTS
* Updated package versions
* Fixed UnityWebRequestAssetImporter using obsolete error checks
* ActionSpecialSplitter now catches exceptions in called action specials by default
* ActorController now catches exceptions thrown by OnDeathSpecial
* GetParamsForModule now checks if there's actually a delimiter after the module name
* GetActiveCamera getting player camera is now delegated to PlayerController via IControlPlayerCamera interface
* IControlPlayerCamera now provides getter for player audio listener
* Fixed 2D collision matrix not matching 3D collision matrix
* Added PathFollower entity that pushes actors along a path of NavigationNodes
* Added ThingPusher entity that pushes other things along a path of NavigationNodes
* Updated ForwardedUtils in AddonSupport and added a few missing methods
* Removed now-redundant BulletExplosionComponentEx
* Added IAmPushable interface implemented by pushable entity types, allowing them to be pushed
* Added HitPhysicsInfo describing push physics for attack hits
* BulletScript now handles push physics (ie bullets can now push things)
* ActorAttackComponent can use push physics for ranged and melee attacks
* Player offhand kick can now push around actors
* DestroyableThingController can be pushed (thunks to attached rigidbody)
* RPG weapons can now specify push physics
* Added literal string printing string subber (basically passes through string)
* Added conditional/microscript resolvers for PlayerFlags and SessionFlags (conditional only) [Experimental]
* Added ScriptStringSubber that calls scripts and substitutes the result [Experimental]
* DelayScaled can now be used from threads other than the main thread [Untested]
* Added NavigationNodeReachedTrigger action trigger [Untested]
* Added audio listener switching methods to WorldUtils [Untested]
* Added physics handling to BulletExplosionComponent [Untested]

# 4.0.0 Preview 4

* Added ChangeSceneSpecial action special to RPGGame
* Fixed ToggleObjectSpecial not actually toggling state
* Fixed starting quest through microscript not setting initial stage
* Fixed ConfigState and PersistState not setting LastMigratedVersion when created new
* Added graceful handling of null LastMigratedVersion in MigrationsManager (treated as version 0)
* Fixed attempting to load resource manifest on WebGL (not supported on WebGL)
* Fixed attempting to manipulate GC on WebGL (not supported on WebGL)
* No longer attempts to set resolution/fullscreen on WebGL (nop on WebGL)
* Resolution and fullscreen options now hidden on platforms where they are not applicable
* Fixed NRE in GameplayOptionsPanelController on initial create
* Changed default Quicksave key to F6 on WebGL (F5 refreshes the page)
* Disabled screenshot functionality in WebGL (doesn't really work)
* Implemented fake-exit with post-exit scene for platforms that don't quit normally (currently just WebGL)
* Async, Debug, Console, and Audio modules now properly clean up their GameObjects and components on unload
* Fixed some IGUI prefabs not using sliced sprites
* Split EditorConditional and EditorMicroscript to separate files
* Parsing of Conditionals and Microscripts is now done in their respective classes (instead of DialogueParser)
* Inventory item condition can now count and compare item quantity using standard options/operators
* Added "add this item to its quantity limit" API to inventory model (AddItemsToQuantityLimit overload)
* SpecialInteractableComponent now defaults to repeatable=true
* Added a SetColor method to ScreenFader to allow setting the color directly
* Added ExtraData to Quest Defs and Inventory Item Defs
* Cleaned up visibility and JSON handling for Inventory Item Defs
* Moved Kill console command to WorldConsoleCommands
* Added Kill method to ITakeDamage (already implemented on ActorController, now also implemented on other entity types)
* OnGameEnd is now called before application quit if quit from within a game
* ChangeSceneScript is now called ChangeSceneSpecial
* Removed duplicate ChangeSceneSpecial
* DoorInteractableComponent and ChangeSceneSpecial can now spawn a transfer effect when used
* Volume control is now pseudo-logarithmic (exponential)
* Weapon viewmodels are now explicitly initialized (WeaponViewModelScript.Init)
* Sprite weapon viewmodels can be (pseudo) attached to ViewModel camera
* ScreenFader can now be used when game is paused by specifying a lowest allowed pause level
* Fixed inventory item change messages not being pushed if items did not have scripts attached
* Fixed AddItem enforceQuantityLimit not applying correctly
* Fixed DialogueParser not actually being capable of parsing music=null and not setting music to null by default

# 4.0.0 Preview 5

* Added core_resources and VS code workspace files to default gitignore
* Handling of boss health by RPGHudController is now optional
* BossComponent no longer sends RpgBossAwake message if actor is dead on start
* BossComponent now sends health in RpgBossAwake message
* BossComponent now sends RpgBossDead message when deactivated
* MovingDoorSpecial open state now has a public accessor (IsOpen)
* WorldUtils.IsObjectAlive variant that takes a transform will no longer throw if transform is null
* WorldUtils Is*Alive methods now use TryGetComponent instead of GetComponent
* RpgWorldUtils TargetIsAlive now just thunks to WorldUtils.IsObjectAlive instead of having a redundant implementation
* Added OnIGUIPaint script hook called after IGUI panels are painted
* Removed obsolete ScreenFader methods
* Removed obsolete SubtitleUtilsEx methods
* Added console command to reset/reload faction relationships (Factions.Reset)
* Added utility methods for cross-calling between JS and C# in WebGL (JSCrossCall)
* Added modified WebGL templates needed for JSCrossCall
* Added PlatformMaySuddenlyExit flag to CoreParams
* PersistState is now saved on every scene unload on platforms that may suddenly exit
* WebGL now displays warning when trying to leave page before CommonCore has quit
* Videos utilities now work in WebGL (although paths are not really checked)
* Added video test scene to project
* Upgraded WaveLoader to 1.1.2
* Split up CoreParams into two files; things that aren't game-specific settings are now in CoreParamsBase
* Updated Unity to 2020.3.36f1
* Updated Visual Studio Editor package to 2.0.16
* Implemented backtick option for string sub literal strings
* Initial resolution can now auto-set to native instead of fixed (behind CoreParams.SetNativeResolutionOnFirstRun flag)
* Implemented PseudoExtensibleEnum groundwork, with tests and custom property drawer (this is one line, but it's huge!)
* Added missing graphics to DefaultTheme
* DefaultTheme is now applied by default
* Themes now define a Highlight Color
* Removed a lot of unused assets and prefabs left over from Ascension III
* Moved Aurelia full model, arm and leg viewmodels to Objects/OpenSource/aurelia

# 4.0.0 Release Candidate 1

* Added DialogueOnAdvance named hook called when dialogue is going to next frame
* Fixed SlideshowControllerEx not using sprites from Dialogue/char or Dialogue/bg
* InitSceneController animation can now be disabled
* InitSceneController can now detect if loading has failed and display "Fatal Error" text
* Long system text (printed initially into console) now available via CoreParams.GetLongSystemText()
* Added console command PrintSystemText that prints long system text to console
* Modules are now torn down in reverse order on application quit
* Default init_player.json now uses EquippedIDs and item UID references instead of deprecated direct refs
* Inventory item models now use attributes to control Json.NET serialization
* Weapon and armor inventory models now leverage pseudo-extensible enums for skills, damage type, etc
* Equipment slots are now defined by a pseudo-extensible enum and stored as int
* Removed unused Ascension III specific LevelUpModal
* RPG skills and stats are now defined by pseudo-extensible enums and stored as int
* CharacterPanelController no longer breaks if an expected stat, still, etc has no value on the character
* WeaponSkillType is no longer used (use SkillType instead) and is obsolete
* All equipped armor items are now factored into player damage protection/threshold calculations
* All equipped items will be considered for shields after ShieldGenerator and Body are tried
* Added ICustomLevelUpModal interface for custom level up modals
* Default ammo and money types are now located in json files instead of being created from enums
* Type field of ammo and money item models have been removed (redundant with model type and unused for any purpose)
* Renamed default keys item file to _default_keys.json for consistency with ammo and money
* Default money type is now defined in GameParams
* DefaultHitLocations and DefaultHitMaterials are now pseudoextensible
* DefaultDamageEffectors defined in WorldTypes is pseudoextensible and is used instead of RpgMiscTypes version
* Added DefaultDamageTypes to WorldTypes, replacing RpgMiscTypes DamageTypes
* RpgMiscTypes DamageEffector and DamageTypes declared obsolete, use or pseudoextend WorldTypes versions instead
* ActorBodyPart is now obsolete. HitLocation should be used instead 
* DamageResistanceNode (actor damage resistance) now uses custom property drawer
* HitboxComponent now uses custom property drawer for hit location and material
* Hit puffs and entity controllers now use custom property drawer for hit material
* RpgDefaultValues is now a non-static class and implementes IRpgDefaultValues interface
* Difficulty multipliers for player movement, attack, etc is now handled in RpgValues calculations
* RpgDefaultValues can be overridden in its entirety by providing an IRpgDefaultValues implementation with RpgDefaultValuesOverrideAttribute (this is used for legacy back compat where we need the old calculations)
* Added new placeholder texture/sprite (NULLA0) and material (DebugPattern)
* Greatly simplified RpgDefaultValues, mostly fixed with no skill or stat definitions
* Removed most stat and skill definitions from builtin StatType and SkillType enums
* GameParams will now be loaded from textasset at Data/RPGDefs/rpg_params in addition to external override file
* Scroll speed in the UI is now multiplied by UIScrollSpeed in config
* Removed unused/obsolete DragonAIController, RampScript, and ScreenFadeHackScript scripts
* Moved AdditionalConditionalResolvers into RPGGame and changed namespace to CommonCore.RpgGame.State
* Moved AnimateQuadScript, AudioClipAssignScript, TextureAssignScript, and TransformCopyScript to CoreShared/Util folder and CommonCore.Util namespace
* Moved FacingSpriteAssignScript to CommonCore.World module and namespace
* Recreated SlideshowControllerEx as SlideshowController in CoreShared/Util (CommonCore.Util), this script has a new GUID
* DialogueController now exposes CurrentScene, CurrentFrameName, and CurrentSceneName properties
* Fix PendingChanges not being cleared when config panel is closed
* Updated Unity to 2020.3.40f1

# 4.0.0
* Removed redundant crosshair handling from RpgHUDController
* Implemented crosshair handling support into SpriteWeaponViewModelScript