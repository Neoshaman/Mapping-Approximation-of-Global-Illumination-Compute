using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/GI Scene")]
public class GIScene : ScriptableObject //TODO:probably no longer a scriptable, no data is hold, managed by manager
{
	Mesh[] geometry; //TODO:need to set culling mask for LPdata
	public GameObject root;//TODO:probably don't need the gameobject, just the position //pass by set root functions
	
	GameObject debug;//visualizing the output textures from the utils class, quad generated with basic material
	
	//TODO:
    //Bounds?
	//Vector3 origine;?
	//Vector3 size;?
    
    globalLights lightdata;

    //TODO:probably need an id too in case of share texture among scenes
	lightprobeData UVprobe;
	LMGB GIbuffer;//lightmap Graphic buffer, texture data to compute lighting operation
	MAGICAL GI;// manage the update of light and global illumination using data from probes and LMGB


	//TODO: 
	//farfield? (distant scene rendering not within the Lightmap chunk
	//void initOrigin(Vector3 origin){
        // HOW TO HANDLE MULTIPLE SCENE ie multiple shader origin ref per scene Mat per chunks?
       //adjust scene hashing origin to mesh bound 
	//	origine = geometry[0].bounds.min;
	//	Bounds b = geometry[0].bounds;//object spaces NOT WORLDSPACE
       // size = b.max-b.min;
		//Shader.SetGlobalVector("_Origin",b.min);
    //}    //init scene bound
    //init farfield?

	public void init(globalLights globalLights, shaderIndex shader)
    {
        UVprobe = new lightprobeData();
        GIbuffer = new LMGB();
	    GI = new MAGICAL();
	    
	    Vector3 origine = root.gameObject.GetComponent<Renderer>().bounds.min;
        geometry = new Mesh[1];
	    geometry[0] = root.GetComponent<MeshFilter>().sharedMesh;
        root.layer = 8;
	    
        shaderSetup(shader);

        //TODO: set farfield
        //alternative to farfield
        //set lmgb in tiles
        //store lmgb tile index in cubemap
        //render all scenes in cubemap as UV + tile index
        //rg = uv, b = sky mask, a = coarse distance?
        //c1.rg = UV, c1.b = tile index, c1.a = skymask //c2.rg = distance
        //when sampling LMGB use tile index to offset

	    UVprobe.initAtlas();//TODO:should init as delta of origine!!!
        GIbuffer.initializeLMGB(geometry);
        GI.SetGlobalLights(globalLights);
	    GI.InitMAGICAL(geometry, origine, UVprobe.atlas, GIbuffer.texture);

	    //DEBUG boogaloo:
	        //apply light map material to scene
	        //root.gameObject.material();
	        // Material CanvasFrame = root.GetComponent<MeshRenderer>().sharedMaterial;
	        Material dmat  = new Material(shader.debugshader);
	        // Material dmat  = new Material(shader.GILMLit);
	        root.GetComponent<Renderer>().material = dmat;
	        // root.GetComponent<Renderer>().material = new Material(shader.GILMLit);
	        // root.GetComponent<MeshRenderer>().material = new Material(shader.GILMLit);
	        // dmat = root.GetComponent<Renderer>().material;// = new Material(shader.GILMLit);
	        // Material dmat = root.GetComponent<MeshRenderer>().material;// = new Material(shader.GILMLit);
	        // CanvasFrame = new Material(getshader.GILMLit);
	        // CanvasFrame.shader = getshader.GILMLit;
	        Debug.Log(root);
	        debug = GameObject.CreatePrimitive(PrimitiveType.Quad);
	        debug.GetComponent<Renderer>().material =  new Material(shader.debugshader);
	    	// RenderSurface.show(debug, GI.returnDisplay());

	    //in theory show the GI texture buffer on the main mesh, which is to test by passing debug data such as bright red
	    RenderSurface.show(root, GI.returnDisplay());
    }

    public void shaderSetup (shaderIndex getshader)
	{
		//use a scriptable objects to conviniently manage version of shader depending on projects
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
	    if (GI.rayCounter == 0){//we accumulate one ray per frame, when all ray are accumulated one bounce of GI is done
		    RenderSurface.show(root, GI.returnDisplay());// update the double buffering of the GI compute
	    }
	    //DEBUG: try to show the raw texture within a utils class into the debug quad,
	    //to see if its accessible and properly rendered
        RenderSurface.show(debug,UVprobe.atlas);
    }
}