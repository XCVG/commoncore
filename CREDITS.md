# Additional assets not included in repository

While CommonCore makes use of many external third-party assets, the base library and demo/example project no longer require these.

Most dependencies are included. A few of them are pulled in automatically as Unity packages:

- Post-Processing Stack (https://docs.unity3d.com/Packages/com.unity.postprocessing@2.1/manual/index.html)

You can get the extra CommonCore modules at (https://github.com/XCVG/commoncore-modules). Some modules do have third-party dependencies.

There is no dependency on the Unity Standard Assets but you can bring them in if you need to.

# Assets included in repository

These are included under permissive or copyleft licenses. License texts are included inside the Documentation folder.

## Libraries

Kalle Jillheden - Newtonsoft.Json for Unity
	https://github.com/jilleJr/Newtonsoft.Json-for-Unity
	Included as DLL (Assets/Plugins/JsonNet)
	MIT License
	
Microsoft - System.Collections.Immutable
	https://www.nuget.org/packages/System.Collections.Immutable/
	Included as DLL (Assets/Plugins/Microsoft)
	MIT License
	
## Code

Microsoft - Xbox Live Unity Plugin
	https://github.com/Microsoft/xbox-live-unity-plugin
	Adapted for Async module (Assets/CommonCoreModules/Async)
	MIT License
	
Modest Tree Media - Unity3dAsyncAwaitUtil
	https://github.com/modesttree/Unity3dAsyncAwaitUtil
	Adapted for Async module (Assets/CommonCoreModules/Async)
	MIT License

## Fonts

Croscore Fonts - Arimo, Cousine, Tinos (Assets/CommonCore/Fonts, Assets/UI/Fonts)
	Steve Matteson 
	Apache License (https://www.apache.org/licenses/LICENSE-2.0)
	
Space Mono (Assets/UI/Fonts/SpaceMono)
	Colophon
	Open Font License
	Sourced from Google Fonts
	
## Graphics

Crosshair (Assets/UI/Graphics/crosshair)
	Kenney.nl - Crosshair pack (200x) [CC0]
	https://opengameart.org/content/crosshair-pack-200%C3%97
	
Loading Indicator (Assets/CommonCore/Graphics/loadwheel)
	CodeAndReload - Loading Wheel (Transparent) [CC0]
	https://openclipart.org/detail/192512/loading-wheel-transparent

Freedoom assets (Assets/Objects/OpenSource/spriteguy, Assets/Objects/OpenSource/spriterifle, Assets/Objects/OpenSource/spritetorch)
	Contributors to the Freedoom project
	https://github.com/freedoom/freedoom/
	licensed under a modified MIT license

## Models

Aurelia character model (Assets/OpenSource/makehuman/aurelia, Assets/OpenSource/makehuman/aurelia2)
	Built with MakeHuman (CC0 export exemption)
	Crude long gloves by Joel Palmius (CC0)
	Riding Boots by punkduck (CC-BY)
	Riding Breeches by punkduck (CC-BY)
	Leather armor by MaciekG (CC-BY)
	
FPS arms model (Assets/TestObjects/arms)
	Built with MakeHuman (CC0 export exemption)
	Crude long gloves by Joel Palmius (CC0)
	Low poly stylized Rifle (AK-47) by Fernando Ferreira (CC0)
	
FPS legs model (Assets/TestObjects/legs)
	Built with MakeHuman (CC0 export exemption)
	Riding Boots by punkduck (CC-BY)
	Riding Breeches by punkduck (CC-BY)
	
Peasant (female) character model (Assets/OpenSource/makehuman/peasantf)
	Built with MakeHuman (CC0 export exemption)
	F Dress 03 by Mindfront (CC-BY)
	Starship uniform boots by Sculletto (CC0)
	
Peasant (male) character model (Assets/OpenSource/makehuman/peasantm)
	Built with MakeHuman (CC0 export exemption)
	Ruffle Sleeve Peasant Blouse 1 by Mindfront (CC-BY)
	F Trousers 01 by Mindfront (CC-BY)
	Starship uniform boots by Sculletto (CC0)

Sword (Assets/Objects/OpenSource/sword)
	Clint Bellanger - Historical Swords Set [CC-BY 3.0]
	https://opengameart.org/content/historical-swords-set

Crossbow (Assets/Objects/OpenSource/crossbow)
	Lamoot - Low-poly Crossbow [CC-BY 3.0]
	https://opengameart.org/content/low-poly-crossbow
	
Demo Rifle (Assets/Objects/OpenSource/rifle)
	OrbitStudios, Fernando Ferreira :D - Low poly stylized Rifle (AK-47) [CC0/public domain]
	https://opengameart.org/content/low-poly-stylized-rifle-ak-47
	
## Sounds

Footsteps (Assets/Resources/DynamicSound/Steps*)
	TinyWorlds - Different steps on wood, stone, leaves, gravel, and mud [CC0]
	https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud
	
Hits (Assets/Resources/DynamicSound/Hit* except as noted below)
	Iwan 'qubodup' Gabovitch - Impact [CC0]
	https://opengameart.org/content/impact

Dirt Hit (Assets/Resources/DynamicSound/HitDirt)
	Jordan Irwin (AntumDeluge) - Thwack Sounds [CC0]
	https://opengameart.org/content/thwack-sounds

Stone Hit (Assets/Resources/DynamicSound/HitStone, Assets/Resources/DynamicSound/FallImpact, Assets/Resources/DynamicSound/StepsEnterCrouch)
	rubberduck - 75 CC0 breaking/falling/hit sfx [CC0]	
	https://opengameart.org/content/75-cc0-breaking-falling-hit-sfx
	
Sword Swing (Assets/Resources/DynamicSound/SwordSwing)
	Iwan Gabovitch - Swish - bamboo stick weapon swhoshes [CC0]
	https://opengameart.org/content/swish-bamboo-stick-weapon-swhoshes
	
Bow Fire (Assets/Resources/DynamicSound/BowFire)
	Erdie - bow02.wav [CC-BY]
	https://freesound.org/people/Erdie/sounds/65734/
	
Crossbow Fire (Assets/Resources/DynamicSound/CrossbowFire)
	Erdie - bow01.wav [CC-BY]
	https://freesound.org/people/Erdie/sounds/65733/	

Crossbow Draw (Assets/Resources/DynamicSound/CrossbowDraw)
	InspectorJ - Bodyboard, Stretch, A.wav [CC-BY]
	https://freesound.org/people/InspectorJ/sounds/401648/
	
Pistol reload sounds (Assets/Resources/DynamicSound/DemoRifleReload)
	sampled from Glock 19 Handgun Pistol Slide Cocking Sounds
	https://freesound.org/people/jacklmurr27/sounds/393734/
	by jacklmurr27, licensed CC0/Public Domain
	
Pistol fire sound (Assets/Resources/DynamicSound/DemoRifleFire)
	Small pistol gunshot indoors
	https://freesound.org/people/acidsnowflake/sounds/402789/
	by acidsnowflake, licensed CC0/Public Domain
	
Hit Indicator (Assets/Resources/DynamicSound/HitIndicator)
	SFX: The Ultimate 2017 16 bit Mini pack
	https://opengameart.org/content/sfx-the-ultimate-2017-16-bit-mini-pack
	SwissArcadeGameEntertainment, licensed CC0/Public Domain
	
Freedoom assets (Assets/Objects/OpenSource/spriteguy, Assets/Objects/OpenSource/spriterifle, Assets/Objects/OpenSource/spritetorch)
	Contributors to the Freedoom project
	https://github.com/freedoom/freedoom/
	licensed under a modified MIT license
	
Sliding Door (Assets/Resources/DynamicSound/DoorSlide)
	Dodge Caracan sliding door.wav
	https://freesound.org/people/CGEffex/sounds/100771/
	by CGEffex, licensed CC-BY 3.0
	
Door Open, Door Close Set (Assets/Resources/DynamicSound/DoorSwing*)
	https://opengameart.org/content/door-open-door-close-set
	by Iwan 'qubodup' Gabovitch, licensed CC0/public domain

Shield sounds (Assets/Resources/DynamicSound/Shield*)
	50 CC0 Sci-Fi SFX
	https://opengameart.org/content/50-cc0-sci-fi-sfx
	by rubberduck, licensed CC0/public domain

# A note on WebGL Templates

The WebGL templates in Assets/WebGLTemplates are in a bit of an ambiguous licensing situation. They are based on the built-in templates shipped with Unity (as per Unity's own recommendation), with some modifications and with trademarked Unity assets stripped out.