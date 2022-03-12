Render To Texel v1.0
-------------------------

This tool renders lighting to texture pixels (texels) instead of screen pixels. It is useful for scaling down graphics on mobile platforms where rendering normal maps every frame is too slow. 

It can create textures similar to the one found on the statue in the shadowgun example project found here: http://blogs.unity3d.com/2012/03/23/shadowgun-optimizing-for-mobile-sample-level/


USAGE
-------------------------

Spawn a Render to Texel window by choosing Window -> Render to Texel.

Drag your model into the scene, then assign it's renderer object to the "To Bake:" field in the RenderToTexel window.
	- Your model must not have overlapping UVs, similar to lightmap UVs.

Drag your normal map onto the "Normal Map:" slot.
	- Your normal map must have the following import settings:
		* Texture Type: Advanced
		* Read/Write Enabled: Yes
		* Import Type: Normal Map
		---
		* Format: Automatic Truecolor.

By default there is only one Bake Target. This means the system will output only one texture. 

Each bake target requires a material with a Render to Texel shader, and at least one point light and one vantage point.
	- The texture is baked from every vantage point with every light and the results are averaged into the final texture.
	- Say you have a space marine. One bake target could simulate moonlight glinting off his armor, while the other simulates the bright muzzle flare from his gun.
	  At runtime you would rapidly switch between the two resulting textures as he fires.

After you press bake, you will be prompted to provide a location and name for the baked texture(s). If you provide "Test" as a name, they will be named Test0.png, Test1.png, etc.


TIPS FOR SUCCESS
-------------------------

Use a lot of lights to make sure your model has good detail and illumination all around. 

Use multiple vantage points to soften the look and produce something that is "generally acceptable".

Touch up the resulting texture in Photoshop.

Experiement with blending between bake targets and manipulating colors in the shader you use at runtime.


KNOWN ISSUES 
---------------------

Overlapping UV polygons cause ugly seams. 

Shadows not supported.

Only Bumped Specular shader is included.

Only point lights are supported.

Certain models may take a long time to bake.

Resulting texture may have seams.




CHANGELOG
---------------------
1.0
______
Initial version.


