# TUX
**Textures Utilities Reloaded** - This will be a tool for artists to test their textures and mess with its values in game. The goal is to allow the artist to know the correct settings for the ksp2 shader so that later he can create a config file that will be read once the material is loaded.

## Shader .cfg
```
SHADER = ShaderName
PROPERTIES
bool _ThisIsABool = true
int _ThisIsAnInt = 1
float _ThisIsAFloat = 1,1
color _ThisIsAColor = 0,123,255,300
texture _ThisIsAModAddedTexture = modid/images/cooltexture.png
texture _ThisIsAKSP2Texture = textureName
texture _ThisIsAWhiteTexture = white
texture _ThisIsANormalTexture = NORMAL
```

##### Floats and Ints
Both floats and Ints can have min and max values, they are set like this:
`defaultValue : minValue : maxValue`
Example:
`0,2 : 0 : 2` - This will tell TUX that your shader's default value is 0,2 and that value cannot be less than 0 or bigger than 2, this is mostly used on the UI

##### Observations
Colors range from 0,255 any number below will be set as 0 and any number above as 255
KSP2 Textures must have the same name as in game (CaseSensitive)

## Overrides .cfg
```
SHADER = ShaderName
OVERRIDE = (PartName, GameObjectName)
_ThisIsABool = true
_ThisIsAnInt = 1
_ThisIsAFloat = 1,1f
_ThisIsAColor = 0,123,255,300
_ThisIsAModAddedTexture = modid/images/cooltexture.png
_ThisIsAKSP2Texture = textureName
```
##### Observations
Any property not disclosed will be set as is on the original shader!
Colors range from 0,255 any number below will be set as 0 and any number above as 255
KSP2 Textures must have the same name as in game (CaseSensitive)

##### Example
```
SHADER = KSP2/Scenery/Standard (Opaque)
OVERRIDE = (engine_1v_methalox_swivel, Engine_Swivel)
ShaderPropertyName = Value
_SmoothnessOverride = true
_Metalic = 0,5f
_EmissionColor = 0,123,255,300
_MetallicMap = tux/images/coolMetallicMap.png
_PaintMaskGlossMap = pod_2v_lander_crew_pm
```

## There are also aliases for textures!
- white texture = white, unitywhite
- black texture = black, unityblack
- normal/bump texture = normal, bump, unitynormalmap

Soon color aliases and more textures will be added!