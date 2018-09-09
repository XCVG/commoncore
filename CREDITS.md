# Third Party Assets (not included in repository)

You will need to acquire these and place them in the correct directories for the project to work properly.

It's possible and even likely that many dependencies will be removed as time goes on. Nearly all assets will be removed once the demo and library are split, leaving some to form a generic quick-start base. Most assets in Objects/ in particular are resources used in the demo scene, not critical to the functionality of the library.

## Standard Assets

These come with the engine and can be unpacked from within the editor.

Environment/TerrainAssets
	(aesthetic only; can be left out and will still run)

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

mojo-structure - Pipes Kit
	https://assetstore.unity.com/packages/3d/props/industrial/pipes-kit-64170

Coolworks Studio - Sci-Fi Gravity Generator
	https://assetstore.unity.com/packages/3d/props/electronics/sci-fi-gravity-generator-82847

Ebal Studios - Space Debris and Crates
	https://assetstore.unity.com/packages/3d/characters/space-debris-99699

Studio Krokidana - Ruins Creation Kit
	https://assetstore.unity.com/packages/3d/environments/ruins-creation-kit-2235

Szymon Łukaszuk - Sci-fi Crate
	https://assetstore.unity.com/packages/3d/props/sci-fi-crate-70278

South Studios - Washing Machine A01
	https://assetstore.unity.com/packages/3d/props/furniture/washing-machine-a01-70644

Rakshi Games - Wooden Box
	https://assetstore.unity.com/packages/3d/props/wooden-box-670

Hit Jones - Realistic Cardboard Boxes
	https://assetstore.unity.com/packages/3d/realistic-cardboard-boxes-pbr-hq-58749

Arvis Magone - Space Shuttle
	https://assetstore.unity.com/packages/3d/vehicles/space/space-shuttle-34972

Vertex Studio - Free Laptop
	https://assetstore.unity.com/packages/3d/props/electronics/free-laptop-90315

Super Icon Ltd - Mobile Furniture Pack
	https://assetstore.unity.com/packages/3d/props/mobile-furniture-pack-62164

3DAVEGA - Machine tools for game of survival
	https://assetstore.unity.com/packages/3d/props/tools/machine-tools-for-game-of-survival-53029

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

CDmir - Chest (Animated) [CC0]
	https://opengameart.org/content/chest-animated
	Assets/Objects/OpenSource/chest

JeremyWoods - Drink [CC-BY 3.0]
	https://opengameart.org/content/drink-0
	Assets/Objects/OpenSource/drink

Brian MacIntosh - Door Textures [CC0]
	https://opengameart.org/content/door-textures
	Assets/Objects/OpenSource/fakedoors/door_wood.png

rubberduck - seamless industrial textures [CC-BY 3.0]
	https://opengameart.org/content/seamless-industrial-textures?page=1
	Assets/Objects/OpenSource/fakedoors/door_scifi.png
	Assets/Objects/OpenSource/fakedoors/door_industrial.png

wobba89 - wood door [CC0]
	https://opengameart.org/content/wood-door
	Assets/Objects/OpenSource/door

giddster - Fan (heating) [CC0]
	https://freesound.org/people/giddster/sounds/437343/
	Assets/Shared/Sounds/Ambient/fan

yughues - Free Hi-Tech Sci-Fi IT thing [CC0]
	https://opengameart.org/content/free-hi-tech-sci-fi-it-thing
	Assets/Objects/OpenSource/it_thing

Scouser - Free Urban Textures [CC0]
	https://opengameart.org/users/scouser
	Assets/Shared/Textures/steel_ribbed_low.png
	Assets/Shared/Textures/metal_panel_1.png
	Assets/Shared/Textures/metal_diamond.png

TinyWorlds - Different steps on wood, stone, leaves, gravel, and mud
	https://opengameart.org/content/different-steps-on-wood-stone-leaves-gravel-and-mud
	Assets/Shared/Sounds/Footsteps

AderuMoro - Fighting Game grunts - young female.wav [CC-BY]
	https://freesound.org/people/AderuMoro/sounds/213295/
	Assets/Shared/Sounds/Bolt

Iwan 'qubodup' Gabovitch - Impact [CC0]
	https://opengameart.org/content/impact
	Assets/Resources/DynamicSound/HitGeneric
	Assets/Resources/DynamicSound/HitMetal
	Assets/Resources/DynamicSound/HitWood
	Assets/Resources/DynamicSound/HitMeat
	
Iwan Gabovitch - Swish - bamboo stick weapon swhoshes [CC0]
	https://opengameart.org/content/swish-bamboo-stick-weapon-swhoshes
	Assets/Resources/DynamicSound/SwordSwing

Clint Bellanger - Historical Swords Set [CC-BY 3.0]
	https://opengameart.org/content/historical-swords-set
	Assets/Objects/OpenSource/sword

Lamoot - Low-poly Crossbow [CC-BY 3.0]
	https://opengameart.org/content/low-poly-crossbow
	Assets/Objects/OpenSource/crossbow

Erdie - bow02.wav [CC-BY]
	https://freesound.org/people/Erdie/sounds/65734/
	Assets/Resources/DynamicSound/BowFire

Erdie - bow01.wav [CC-BY]
	https://freesound.org/people/Erdie/sounds/65733/
	Assets/Resources/DynamicSound/CrossbowFire

InspectorJ - Bodyboard, Stretch, A.wav [CC-BY]
	https://freesound.org/people/InspectorJ/sounds/401648/
	Assets/Resources/DynamicSound/CrossbowDraw

arikel - Low poly furniture [CC0]
	https://opengameart.org/content/low-poly-furniture
	Assets/Objects/OpenSource/lowpolyshelf

Lucian Pavel - Low poly "table and chair" [CC0]
	https://opengameart.org/content/low-poly-table-and-chair
	Assets/Objects/OpenSource/lowpolytable

Clint Bellanger - Bed (low poly) [CC0]
	https://opengameart.org/content/bed-low-poly
	Assets/Objects/OpenSource/lowpolybed

DeadKir - Torch [CC0]
	https://opengameart.org/content/low-poly-torchwithout-flame
	Assets/Objects/OpenSource/lowpolytorch

JCW - Wood Texture Tiles [CC0]
	https://opengameart.org/content/wood-texture-tiles
	Assets/Shared/Textures/wood1

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
	Riding Boots by punkduck (CC-BY)
	Riding Breeches by punkduck (CC-BY)
	Leather armor by MaciekG (CC-BY)
	
Peasant (female) character model (Assets/OpenSource/makehuman/peasantf)
	Built with MakeHuman (CC0 export exemption)
	F Dress 03 by Mindfront (CC-BY)
	Starship uniform boots by Sculletto (CC0)
	
Peasant (male) character model (Assets/OpenSource/makehuman/peasantm)
	Built with MakeHuman (CC0 export exemption)
	Ruffle Sleeve Peasant Blouse 1 by Mindfront (CC-BY)
	F Trousers 01 by Mindfront (CC-BY)
	Starship uniform boots by Sculletto (CC0)

Background ambient music (Assets/ThirdParty/compo.ogg)
	Eric Taylor - Royalty Free Music
	https://opengameart.org/content/eric-taylor-royalty-free-music-pack-2-30-songs-cc-by
	https://opengameart.org/content/eric-taylor-royalty-free-music-pack-50-songs-cc-by
	