using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/GI Scene")]
public class GIScene : ScriptableObject //probably no longer a scriptable, no data is hold, managed by manager
{
	Mesh[] geometry; //need to set culling mask for LPdata
	public GameObject root;//probably don't need the gameobject, just the position //pass by set root functions
	
    GameObject debug;
    //Bounds?
    // Vector3 origine;
	//Vector3 size;
    
	// shaderIndex getshader;
    globalLights lightdata;

    //probably need an id too in case of share texture among scenes
    lightprobeData UVprobe;
    LMGB GIbuffer;
    MAGICAL GI;


    //farfield?
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

        //set farfield

        //alternative to farfield
        //set lmgb in tiles
        //store lmgb tile index in cubemap
        //render all scenes in cubemap as UV + tile index
        //rg = uv, b = sky mask, a = coarse distance?
        //c1.rg = UV, c1.b = tile index, c1.a = skymask //c2.rg = distance
        //when sampling LMGB use tile index to offset

	    UVprobe.initAtlas();//should init as delta of origine!!!
        GIbuffer.initializeLMGB(geometry);
        GI.SetGlobalLights(globalLights);
	    GI.InitMAGICAL(geometry, origine, UVprobe.atlas, GIbuffer.texture);

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
        RenderSurface.show(root, GI.returnDisplay());

        debug = GameObject.CreatePrimitive(PrimitiveType.Quad);
        debug.GetComponent<Renderer>().material =  new Material(shader.debugshader);
        // RenderSurface.show(debug, GI.returnDisplay());
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
	    GI.updateDirectLight(geometry);//only when change happen
    }
    public void updateGI(){
	    GI.updateGIBuffer(geometry);
	    if (GI.rayCounter == 0){ 
            RenderSurface.show(root, GI.returnDisplay());
        }
        RenderSurface.show(debug,UVprobe.atlas);
    }
}