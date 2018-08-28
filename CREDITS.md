# Third Party Assets (not included in repository)

You will need to acquire these and place them in the correct directories for the project to work properly.

## Standard Assets

These come with the engine and can be unpacked from within the editor.

Environment/Water (Basic)
	(aesthetic only; can be left out and will still run)
	
Characters
	(you only need Third Person Character but importing the whole thing should be safe)

## Assets/ThirdParty

Unity Technologies - Post Processing Stack v2
	https://github.com/Unity-Technologies/PostProcessing
	
Wanzyee Studio - Json.NET Converters
	https://assetstore.unity.com/packages/tools/input-management/json-net-converters-simple-compatible-solution-58621
	delete or comment out DictionaryConverter
	change line 127? on JsonNetUtility from 
		).Where(!(type) => (null != type.Namespace && type.Namespace.StartsWith("NewtonSoft"))
	to
		).Where((type) => (null != type.Namespace && type.Namespace.StartsWith("WanzyeeStudio"))
		

Jean Moreno (JMO) - War FX
	https://www.assetstore.unity3d.com/en/#!/content/5669
	
## Assets/Plugins

Cobo Antonio - DevConsole 2
	https://assetstore.unity.com/packages/tools/gui/devconsole-2-16833

## Assets/ProCore
	
Unity Technologies - ProBuilder
	https://www.assetstore.unity3d.com/en/?stay#!/content/111418
	(needed for demo scenes)
	
## Assets/Objects/ThirdParty

Kalamona - Free Fantasy Spider
	https://assetstore.unity.com/packages/3d/characters/creatures/free-fantasy-spider-10104

Calvin Weibel - Free Shipping Containers
	https://www.assetstore.unity3d.com/en/?stay#!/content/18315

ice_screen - Ruined Tower Free
	https://www.assetstore.unity3d.com/en/?stay#!/content/66495
	Rename "mesh" to "ruined tower free"

Laxer - Mobile Trees Package
	https://www.assetstore.unity3d.com/en/?stay#!/content/18866

Lylek Games - Medieval Town Exteriors [Free]
	https://www.assetstore.unity3d.com/en/?stay#!/content/27026

Game-Ready - Free Ships
	https://www.assetstore.unity3d.com/en/?stay#!/content/104215

Lylek Games - Island Assets
	https://www.assetstore.unity3d.com/en/?stay#!/content/56989

StarCity Designs - Satellite Dish / Communication Tower
	https://www.assetstore.unity3d.com/en/?stay#!/content/5884

Jaroslav Grafskiy - Mobile Wooden Fences
	https://www.assetstore.unity3d.com/en/?stay#!/content/54772

HONETi - Free Animated Space Man
	https://www.assetstore.unity3d.com/en/?stay#!/content/61548
	Animator probably needs to be modified for state names to match up
	
EnsenaSoft, S.A. de C.V. - Village Buildings 5
	https://www.assetstore.unity3d.com/en/?stay#!/content/99911
	Remove the EnsenaSoft folder, full path should be "Assets/Objects/ThirdParty/Village Buildings 5"
	
3DMondra - Traditional water well
	https://www.assetstore.unity3d.com/en/#!/content/4477
	Put in Assets/Objects/ThirdParty instead of Standard Assets
	
davidbinformatique - Modern People
	https://www.assetstore.unity3d.com/en/?stay#!/content/73497

animations from Firefighter VR, but these will be removed/replaced soon
	(substitute your own animations, instructions to follow)

# Third Party Assets (included in repository)

These are included under permissive or copyleft licenses.

Rakshi Games - Free House [CC-BY 3.0]
	https://opengameart.org/content/free-house
	Assets/Objects/OpenSource/freehouse

JamesWhite - Wooden Bridge [CC0]
	https://opengameart.org/content/wooden-bridge-0
	Assets/Objects/OpenSource/ogabridge
	
djonvincent/opengameart.org - Wall [CC0]
	https://opengameart.org/content/wall
	Assets/Objects/OpenSource/magewall
	
djonvincent/opengameart.org - Mage Tower [CC0]
	https://opengameart.org/content/mage-tower
	Assets/Objects/OpenSource/magewall
	
Blender Foundation | apricot.blender.org - Loy-poly fences [CC-BY 3.0]
	https://opengameart.org/content/low-poly-fences
	Assets/Objects/OpenSource/lowpolyfence

César da Rocha - Fantasy Choir 3 orchestral pieces [CC0]
	https://opengameart.org/content/fantasy-choir-3-orchestral-pieces
	Assets/Resources/DynamicMusic/menu.ogg
	
SeKa - Sad Scene Music (What is Left) [CC0]
	https://opengameart.org/content/sad-scene-music-what-is-left
	Assets/Resources/DynamicMusic/gameover.ogg
	
athile - Seamless Grass Textures (20 pack) [CC0]
	https://opengameart.org/content/seamless-grass-textures-20-pack
	Assets/Shared/Textures/grass13.png

n4 - Seamless Beach Sand [CC0]
	https://opengameart.org/content/seamless-beach-sand
	Assets/Shared/Textures/beach_sand.png
	
LuminousDragonGames - Simple Seamless tiles of dirt and sand [CC0]
	https://opengameart.org/content/simple-seamless-tiles-of-dirt-and-sand
	Assets/Shared/Textures/dirt_10.png
	
para - real asphalt texture pack [CC0]
	https://opengameart.org/content/real-asphalt-texture-pack
	Assets/Shared/Textures/ground_asphalt_old_05a.png
	
Kenney.nl - Crosshair pack (200x) [CC0]
	https://opengameart.org/content/crosshair-pack-200%C3%97
	Assets/UI/Graphics/crosshair.png

pixabella - Red Glossy Valentine Heart [CC0]
	https://openclipart.org/detail/21281/red-glossy-valentine-heart
	Assets/UI/Graphics/heart.png
	
inky2010 - Gold Shield [CC0] [modified]
	https://openclipart.org/detail/75697/gold-shield
	Assets/UI/Graphics/shield.png
	
bpcomp - Lightning Icon [CC0] [modified]
	https://openclipart.org/detail/26221/lightning-icon
	Assets/UI/Graphics/energy.png
	
ckhoo - yellow target [CC0] [modified]
	https://openclipart.org/detail/192457/yellow-target
	Assets/UI/Graphics/ranged.png
	
Designer.io - Sword 2 [CC0] [modified]
	https://openclipart.org/detail/289643/sword-2
	Assets/UI/Graphics/melee.png
	
Luis Zuno (@ansimuz) - Mountain at Dusk Background [CC0] [modified]
	https://opengameart.org/content/mountain-at-dusk-background
	Assets/Scenes/Meta/GameOverScene/gameover_bg.png
	
# Miscelleneous

These are separated out largely for formatting

Aurelia charachter model (Assets/OpenSource/makehuman/aurelia)
	Built with MakeHuman (CC0 export exemption)
	Crude long gloves by Joel Palmius (CC0)
	Shoes Biker Boots female by Mindfront (CC-BY)
	Leather armor by MaciekG (CC-BY)
	
	