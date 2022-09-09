using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/GI Scene")]
public class GIScene : ScriptableObject //TODO:probably no longer a scriptable, no data is hold, managed by manager
{
	Mesh[] geometry;
	public GameObject root;

	// GameObject debug;//visualizing the output textures from the utils class, quad generated with basic material
    
    globalLights lightdata;

	lightprobeData UVprobe;
	LMGB GIbuffer;//lightmap Graphic buffer, texture data to compute lighting operation
	MAGICAL GI;// manage the update of light and global illumination using data from probes and LMGB


	public void init(globalLights globalLights, shaderIndex shader){
        UVprobe = new lightprobeData();
        GIbuffer = new LMGB();
	    GI = new MAGICAL();

	    Vector3 origine = root.gameObject.GetComponent<Renderer>().bounds.min;
        geometry = new Mesh[1];
	    geometry[0] = root.GetComponent<MeshFilter>().sharedMesh;
	    root.layer = LayerMask.NameToLayer("capture");
	    
	    shaderSetup(shader);

	    UVprobe.initAtlas();
        GIbuffer.initializeLMGB(geometry);
        GI.SetGlobalLights(globalLights);
	    GI.InitMAGICAL(geometry, origine, UVprobe.atlas, GIbuffer.texture);
        //apply light map material to scene

        // Material dmat  = new Material(shader.debugshader);
		Material dmat  = new Material(shader.GILMLit);
        // debug = GameObject.CreatePrimitive(PrimitiveType.Quad);
    	// debug.GetComponent<Renderer>().material = dmat;
		root.GetComponent<Renderer>().material = dmat;
    }

    public void shaderSetup (shaderIndex getshader)
	{
        GI.GI = getshader.GI;
        GI.direct = getshader.DirectLight;

	    GIbuffer.bakeAlbedo = getshader.bakeAlbedo;
        GIbuffer.bakeWorldNormal = getshader.bakeWorldNormal;
        GIbuffer.bakeWorldPosition = getshader.bakeWorldPosition;
        GIbuffer.BakeShadowMasking = getshader.bakeShadowMask;

        UVprobe.capture = getshader.cubemapCapture;
        UVprobe.transfer = getshader.cubemapTransfer;
    }

    public void updateLight(){
	    GI.updateDirectLight(geometry);//TODO:only when change happen, this render and cache direct lighting on a texture
	}
	public void updateGI(){
    	GI.updateGIBuffer(geometry);


		// Material CanvasFrame = root.GetComponent<MeshRenderer>().material;
		// CanvasFrame.SetTexture("_GI", GI.returnDisplay());
		
		root.GetComponent<MeshRenderer>().material.SetTexture("_GI", GI.returnDisplay());
    	// RenderSurface.show(root, GI.returnDisplay());

		// dmat.SetTexture("_GI", GI.returnDisplay(), UnityEngine.Rendering.RenderTextureSubElement.Default);

		//if (GI.rayCounter == 0){//we accumulate one ray per frame, when all ray are accumulated one bounce of GI is done
		//    RenderSurface.show(root, GI.returnDisplay());// update the double buffering of the GI compute
		//    //Debug.Log(GI.returnDisplay());
	    //}
	    
	    //DEBUG: try to show the raw texture within a utils class into the debug quad,
	    //to see if its accessible and properly rendered
	    //RenderSurface.show(debug,UVprobe.atlas);
	    
	    //RenderSurface.show(debug,GI.returnDisplay());
	    //RenderSurface.show(debug,GI.returnAccum());
	    //RenderSurface.show(debug,GI.returnDirect());
	    
	    //RenderSurface.show(debug,GIbuffer.texture[3]);
	    //Debug.Log(GIbuffer.texture[0]);
    }
}