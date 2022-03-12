using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSetup : MonoBehaviour
{
	#region declaration
	public Shader
		UV,
		Transfer, 
		Lighting,
		UpdateGI;
		
	public Material
		atlasTransfer,
		boxProject,
		DirectLightMat;

	int Cellsize = 4;
	int AtlasTileSize = 128;	//size(2048)/16
	int ProbeNumber = 256;		//16x16
	GameObject[] ProbeArray;
	GameObject Capture;
	Camera UVCapture;
	List<CustomRenderTextureUpdateZone> zones;
	CustomRenderTextureUpdateZone[] updating;
	
	//setting are on teh texture object?
	public CustomRenderTexture
		IndirectionProbeAtlas,
		DirectLight,
		LightAccumulation;
	public RenderTexture SceneCapture, atlastest;
	public RenderTexture[] LMGB;
	public Material[] LMGBmat;
	public GameObject[] GIscene;

	// enum LMGBtexture{
	// 	albedo = 0,
	// 	worldNormal = 1,
	// 	worldPosition = 2,
	// 	shadowMasking = 3
	// }
	#endregion

	//--------------functions-----------------
	void placeProbe(){
		
		//set slots parameters
		ProbeArray = new GameObject[ProbeNumber];
		int midcell = Cellsize/2;
		//texture zones data
		IndirectionProbeAtlas.ClearUpdateZones();
		CustomRenderTextureUpdateZone[] ProbeTexture = new CustomRenderTextureUpdateZone[ProbeNumber];
		
		for(int i=0;i<ProbeNumber; i++ ){
			int x = i/16;
			int y = i%16;
			
			//create slots primitive
			ProbeArray[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);//new GameObject();
			ProbeArray[i].transform.position = new Vector3((x*Cellsize)+midcell,0,(y*Cellsize)+midcell);
			ProbeArray[i].SetActive(false);
			
			//create atlas textures zones
			int halftile = AtlasTileSize/2;
			int xt = (x * AtlasTileSize) + halftile;
			int yt = (y * AtlasTileSize) + halftile;
			ProbeTexture[i].updateZoneSize		= new Vector3(AtlasTileSize,AtlasTileSize,0);
			ProbeTexture[i].updateZoneCenter	= new Vector3(xt,yt,0);
		}
		//initialize the texture zones
		IndirectionProbeAtlas.SetUpdateZones(ProbeTexture);
	}
	
	void setProbeCapture(){
		placeProbe();
		//set camera
		Capture = new GameObject("CaptureScene");
		Capture.AddComponent<Camera>();
		UVCapture = Capture.GetComponent<Camera>();
		UVCapture.SetReplacementShader(UV, "RenderType");
		//Set camera parameters
		UVCapture.backgroundColor = Color.blue;
		UVCapture.clearFlags = CameraClearFlags.SolidColor;
		UVCapture.allowMSAA = false;
		UVCapture.cullingMask = 1 << 8;
		
		//UVCapture.targetTexture = SceneCapture;
		
		//atlas data
		zones = new List<CustomRenderTextureUpdateZone>();
		IndirectionProbeAtlas.GetUpdateZones(zones);
		updating = new CustomRenderTextureUpdateZone[1];
		IndirectionProbeAtlas.Initialize();

	}
	void renderProbe(){

		setProbeCapture();

		GameObject probe;
		UVCapture.enabled = true; // move that outside

		for (int i = 0; i<ProbeArray.Length;i++){
			//render at position on grid
			probe = ProbeArray[i];
			Capture.transform.position = probe.transform.position;
			Capture.transform.rotation = Quaternion.identity;
			UVCapture.RenderToCubemap(SceneCapture);
			//update zone
			updating[0] = zones[i];
			IndirectionProbeAtlas.SetUpdateZones(updating);
			
			float size = updating[0].updateZoneSize.x;
			Vector2 position = updating[0].updateZoneCenter;
			
			updatingAtlas(size, position);
		}

		//move both outside
 		initLMGB();
		UVCapture.enabled = false;
	}
	
	void initLMGB(){
		// albedo = 0,
		// worldNormal = 1,
		// worldPosition = 2,
		// shadowMasking = 3
		UVCapture.backgroundColor = Color.black;

		for (int i = 0; i < LMGB.Length; i++) {
			UVCapture.targetTexture = LMGB[i];
			UVCapture.RenderWithShader(LMGBmat[i].shader,"");
			//updateRT(LMGB[i],LMGBmat[i]);
		}
	}

	void updatingAtlas(float size, Vector2 offset){
		//could probably do teh UV selection in shader
		//at each point
		//hash the offset using the size
		//if offset ! of input uniform, add 0
		//else add transfered color using remap UV
		
		offset		= new Vector2 (offset.x - (size/2),offset.y - (size/2));
		Vector2 s	= new Vector2 (size + offset.x,size + offset.y);
		
		offset	/=IndirectionProbeAtlas.width;
		s		/=IndirectionProbeAtlas.width;
		
		Graphics.SetRenderTarget(atlastest);
		GL.PushMatrix();
		GL.LoadOrtho(); 
		atlasTransfer.SetPass(0);

		GL.Begin(GL.TRIANGLE_STRIP);
		
		 //0,0
		GL.TexCoord(new Vector3(0f,				0f,			0f));
		GL.Vertex3(				offset.x,		offset.y,	1f);
		
		 //0,1
		GL.TexCoord(new Vector3(0f,				1f,			0f));
		GL.Vertex3(				offset.x,		s.y,		1f);
		
		 //1,0
		GL.TexCoord(new Vector3(1f,				0f,			0f));
		GL.Vertex3(				s.x,			offset.y,	1f);
		
		 //1,1
		GL.TexCoord(new Vector3(1f,				1f,			0f));
		GL.Vertex3(				s.x,			s.y,		1f); 
		
		GL.End(); 
		GL.PopMatrix();
	}
	
	void updateRT(RenderTexture RT, Material MT){
		Graphics.SetRenderTarget(RT);
		GL.PushMatrix();
		GL.LoadOrtho(); 
		MT.SetPass(0);

		Mesh m = GIscene[0].GetComponent<Mesh>();
		Debug.Log(GIscene[0]);

		Graphics.DrawMeshNow(m, Vector3.zero, Quaternion.identity);

		GL.PopMatrix();
	}
	
	
	//TickAccumulation
	//count cycle
	//pass ray delta
	//if cycle = complete, swap buffer, reset count
	void accumulationTick(){
		
	}
	
	//--------------Lifecycles-----------------
	
	//hide this chunk	//renderfarfield	//show chunk
	//place probe		//renderprobe async	//lightLMGB
	
	void Start(){
		renderProbe();
	}
	
	//TickAccumulation rate
	//if light change then update direct
	//if scene change then update probes
	void Update(){
		//set light direction for shader
		Light dirlit = FindObjectOfType<Light>();
		Vector3 sunny = -dirlit.transform.forward;
		//boxProject.SetVector("_MainLight",sunny);
		//DirectLightMat.SetVector("_MainLight",sunny);
		Shader.SetGlobalVector("_MainLight",sunny);

		//updates shaders
		DirectLight.Update();
		//LightAccumulation.Update();

		//adjust scene hashing origin to mesh bound
		Mesh m = GIscene[0].GetComponent<MeshFilter>().mesh;
		Renderer r = GIscene[0].GetComponent<Renderer>();
		Bounds b = r.bounds;
		Shader.SetGlobalVector("_Origin",b.min);
		//boxProject.SetVector("_Origin",b.min);

		//Debug.Log(b.min);
		
		//Debug.DrawRay(Vector3.zero,Vector3.one*16,Color.red);
		
		// const int numSamples = 64;
		// const float phi = 1.618033988f;
		// const float gAngle = phi * Mathf.PI * 2.0f;
		// Vector3 worldNormal = Vector3.up; 
        // for (int i = 0; i < numSamples; i++)
        // {
        // 	float fi = (float)i;
        // 	float fiN = fi / numSamples;
        // 	float longitude = gAngle * fi;
        // 	float latitude = Mathf.Asin(fiN * 2.0f - 1.0f);
                     
        // 	Vector3 kernel = new Vector3(
        // 		Mathf.Cos(latitude) * Mathf.Cos(longitude),
        // 		Mathf.Cos(latitude) * Mathf.Sin(longitude),
        // 		Mathf.Sin(latitude)
		// 	);
        // 	kernel = (kernel + worldNormal).normalized;
			
        // 	if (i == 0){
        // 		kernel = Vector3.up;
        // 	}
        // 	Debug.DrawRay(Vector3.zero,kernel*16,Color.red);
		// 	//traceResult += ConeTrace(voxelOrigin.xyz, kernel.xyz, worldNormal.xyz);
        // }
	}
	//--------------------------------------

	
	
    
}
