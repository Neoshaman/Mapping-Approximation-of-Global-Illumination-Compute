using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSetup : MonoBehaviour
{
	public Shader
		UV,
		Transfer, 
		Lighting,
		UpdateGI;

	int Cellsize = 4;
	int AtlasTileSize = 128;//size(2048)/16
	int ProbeNumber = 256;//16x16
	GameObject[] ProbeArray;
	GameObject Capture;
	
	//setting are on teh texture object?
	public CustomRenderTexture
		IndirectionProbeAtlas,
		DirectLight,
		LightAccumulation;
	public RenderTexture SceneCapture;
	public RenderTexture[] LMGB;
	enum LMGBtexture{
		albedo = 0,
		worldNormal = 1,
		worldPosition = 2,
		shadowMasking = 3
	}
	//--------------functions-----------------
	//placeprobe
	void placeProbe(){
		ProbeArray = new GameObject[ProbeNumber];
		int midcell = Cellsize/2;
		Debug.Log("placeprobe");
		
		IndirectionProbeAtlas.ClearUpdateZones();
		CustomRenderTextureUpdateZone[] ProbeTexture = new CustomRenderTextureUpdateZone[ProbeNumber];
		
		for(int i=0;i<ProbeNumber; i++ ){
			int x = i/16;
			int y = i%16;
			//Debug.Log(i);
			
			ProbeArray[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);//new GameObject();
			ProbeArray[i].transform.position = new Vector3((x*Cellsize)+midcell,0,(y*Cellsize)+midcell);
			
			//setup atlas textures
			int halftile = AtlasTileSize/2;
			int xt = (x * AtlasTileSize) + halftile;
			int yt = (y * AtlasTileSize) + halftile;
			ProbeTexture[i].updateZoneSize		= new Vector3(AtlasTileSize,AtlasTileSize,0);
			ProbeTexture[i].updateZoneCenter	= new Vector3(xt,yt,0);
		}
		IndirectionProbeAtlas.SetUpdateZones(ProbeTexture);
	}
	//renderprobes
	//--for each probes position in probe array
	//----for each face of cubemap
	//------render face with Uvshader
	//----project cubemap to atlas
	void renderProbe(){
		Camera UVCapture = Capture.GetComponent<Camera>();
		List<CustomRenderTextureUpdateZone> zones = new List<CustomRenderTextureUpdateZone>();
		IndirectionProbeAtlas.GetUpdateZones(zones);
		//CustomRenderTextureUpdateZone[] Tile = zones.ToArray();

		//UVCapture.depth = cameraDepth;
		//UVCapture.cullingMask = cameraLayerMask;
		//UVCapture.nearClipPlane = cameraNearPlane;
		//UVCapture.farClipPlane = cameraFarPlane;
		//UVCapture.useOcclusionCulling = cameraUseOcclusion;
			
		UVCapture.SetReplacementShader(UV, "RenderType");
		UVCapture.backgroundColor = Color.blue;
		UVCapture.clearFlags = CameraClearFlags.SolidColor;
		//public void SetReplacementShader(Shader shader, string replacementTag);
		int i = 0;
		foreach(GameObject probe in ProbeArray){
			Capture.transform.position = probe.transform.position;
			Capture.transform.rotation = Quaternion.identity;
			UVCapture.RenderToCubemap(SceneCapture);
			//project cubemap to atlas -> in shader:
			//sample cubemap with perpixel octomaping 128 tile in atlas
			
			i++;
		}
		UVCapture.enabled = false;
	}
	//renderFarfield
	void renderFarfield(){
		
	}
	//renderLMGB
	//--for each element in array
	//----foreach object in scene tagg as GIcaster
	//------unwrap to LMGBtexture with LMGBshader
	void renderLMGB(){
		
	}
	//LightLMGB
	//light to Directlight texture with directlight shader
	void lightLMGB(){
		
	}
	//TickAccumulation
	//count cycle
	//pass ray delta
	//if cycle = complete, swap buffer, reset count
	void accumulationTick(){
		
	}
	//--------------Lifecycles-----------------
	
	//start:
	//yield is scene loaded
	//renderLMGB
	//hide this chunk
	//renderfarfield
	//show chunk
	//place probe
	//renderprobe async
	//lightLMGB
	void Start(){
		
		//set up capture camera
		//probably should just add the camera to this object instead of creating another
		Capture = new GameObject("CaptureScene");
		Capture.AddComponent<Camera>();
		
		placeProbe();
		renderProbe();

	}
	//update:
	//TickAccumulation rate
	//if light change then update direct
	//if scene change then update probes
	void Update(){
		
	}
	//--------------------------------------

	
	
    
}
