using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAGICAL //MAGIC applied by lightprobe
//TODO:
//need a parent class (MAGIC: mapping approximation of global illumination compute)
//alternative -> (MAGIC: mapping approximation of global illumination cache)
//to derive other implementation like:
//MAGIC HAT: by hemisperic atlas tiling
//MAGIC TRICK: by tiled ray indirection as compressed kernel
{
	int size = 256; //texture size, max = 256 //8bit indexing limits //UV using 8bit channel to index //allow to adress 65536 points
	public int rayCounter = 0; //count rays for accumulation before swap
	Vector4 Kernel;//ray direction + counter

    globalLights globalLights;

	//GI BUFFER
    public Shader GI;
    Material GIpass;
	RenderTexture displayBuffer;//used to show GI
	RenderTexture accumulationBuffer;//used to accumulate the rays
    RenderTexture swap;
    
	//Direct light BUFFER
    public Shader direct;
    Material directPass;
	RenderTexture directlight;



	Matrix4x4 positionMatrix = Matrix4x4.identity;
	
	//---------------INIT-----------------------------------------------
	public  void SetGlobalLights (globalLights lights){
        globalLights = lights;
	}
    
    
	private void initDirectLight (Mesh[] mesh, Vector3 origin, RenderTexture probes){
        directPass = new Material(direct);
		directPass.SetTexture("_Atlas", probes, UnityEngine.Rendering.RenderTextureSubElement.Default);
		directPass.SetVector("_MainLight",globalLights.directionalLight);
		directPass.SetVector("_Origin",origin);

		positionMatrix.SetTRS(
			new Vector3(32,32,0),		//position
			// new Vector3(32,0,32),		//position
			Quaternion.Euler(
				0,
				0,
				0
			),						//rotation
			// new Vector3(64,1,64)		//scale
			new Vector3(100,-100,100)		//scale
		);
		directPass.SetMatrix("_PosMat", positionMatrix);
		
		directlight = new RenderTexture(size/2, size/2, 24);//2:128² - 4:64²->light leaks
        RenderSurface.applyShader(mesh,directlight,directPass);
	}
	
	
    private void initGIBuffer (Mesh[] mesh, Vector3 origin, RenderTexture probes, RenderTexture[] LMGB){
        GIpass = new Material(GI);

        setGlobalLightsToGI();
        setSceneDataToGI(origin,probes);
        setGIBufferToGI();
	    setLMGBToGI(LMGB);
		
		positionMatrix.SetTRS(
			new Vector3(32,32,0),		//position
			// new Vector3(32,0,32),		//position
			Quaternion.Euler(
				0,
				0,
				0
			),						//rotation
			// new Vector3(64,1,64)		//scale
			new Vector3(100,-100,100)		//scale
		);
		GIpass.SetMatrix("_PosMat", positionMatrix);

	    displayBuffer = new RenderTexture(size, size, 24);
	    accumulationBuffer = new RenderTexture(size, size, 24);
        // RenderSurface.setCanvas(displayBuffer,size);
        // RenderSurface.setCanvas(accumulationBuffer,size);
        RenderSurface.applyShader(mesh,displayBuffer,GIpass);
        RenderSurface.applyShader(mesh,accumulationBuffer,GIpass);
    }
    public  void InitMAGICAL (Mesh[] mesh, Vector3 origin, RenderTexture probes, RenderTexture[] LMGB){
        Kernel = new Vector4();
	    initDirectLight(mesh,origin,probes);
	    initGIBuffer(mesh,origin,probes,LMGB);
    }
    
    
    
	void setGlobalLightsToGI(){
		GIpass.SetTexture("_Skybox", globalLights.sky, UnityEngine.Rendering.RenderTextureSubElement.Default);
		GIpass.SetVector("_MainLight",globalLights.directionalLight);
		GIpass.SetColor("_Ambientsky",globalLights.ambientSky);
		GIpass.SetColor("_Ambientcolor",globalLights.ambientLight);
	}
	void setSceneDataToGI(Vector3 origin, RenderTexture probes){
		GIpass.SetVector("_Origin",origin);
		GIpass.SetVector("_Kernel",Kernel);
		GIpass.SetTexture("_Atlas", probes, UnityEngine.Rendering.RenderTextureSubElement.Default);
		GIpass.SetTexture("_LMdirect", directlight, UnityEngine.Rendering.RenderTextureSubElement.Default);
	}
	void setGIBufferToGI(){
		GIpass.SetTexture("_Accumulation", accumulationBuffer, UnityEngine.Rendering.RenderTextureSubElement.Default);
		GIpass.SetTexture("_Display", displayBuffer, UnityEngine.Rendering.RenderTextureSubElement.Default);
	}
	void setLMGBToGI(RenderTexture[] LMGB){
		GIpass.SetTexture("_Albedo", LMGB[0], UnityEngine.Rendering.RenderTextureSubElement.Default);
		GIpass.SetTexture("_Wnormal", LMGB[1], UnityEngine.Rendering.RenderTextureSubElement.Default);
		GIpass.SetTexture("_Wposition", LMGB[2], UnityEngine.Rendering.RenderTextureSubElement.Default);
		GIpass.SetTexture("_Shadowmask", LMGB[3], UnityEngine.Rendering.RenderTextureSubElement.Default);
	}
	
	
 //-----------------UPDATE----------------------------------------
	public  void updateDirectLight (Mesh[] mesh){
		//directPass.SetTexture("_Atlas", probes, UnityEngine.Rendering.RenderTextureSubElement.Default);

		directPass.SetVector("_MainLight",globalLights.directionalLight);
        RenderSurface.applyShader(mesh,directlight,directPass);//render direct to texture
    }
    
    public  void updateGIBuffer (Mesh[] mesh){
	    const int numSamples = 64; //(number of rays)
	    const float phi = 1.618033988f;//golden number
	    const float gAngle = phi * Mathf.PI * 2.0f;//golden angle
		Vector3 worldNormal = Vector3.up; 
        
        // for (int rayCounter = 0; rayCounter < numSamples; rayCounter++){
        	float fi = (float)rayCounter;
        	float fiN = fi / numSamples;
        	float longitude = gAngle * fi;
        	float latitude = Mathf.Asin(fiN * 2.0f - 1.0f);
                 
	    //hemisphere sampling
	    Vector3 kernel = new Vector3(
        		Mathf.Cos(latitude) * Mathf.Cos(longitude),
        		Mathf.Cos(latitude) * Mathf.Sin(longitude),
        		Mathf.Sin(latitude)
	    );
			
            //send _Kernel.rgb to shader, pass .a = count 
            Kernel = new Vector4(kernel.x,kernel.y,kernel.z,rayCounter);
            GIpass.SetVector("_Kernel",Kernel);
            RenderSurface.applyShader(mesh,accumulationBuffer,GIpass);

	    rayCounter +=1;//each update, generally each frame
            rayCounter %= numSamples;
	    if (rayCounter == 0){//every modulo
                //swap buffer
                swap = displayBuffer;
                displayBuffer = accumulationBuffer;
                accumulationBuffer = swap;
                setGIBufferToGI();
            }


        	// kernel = (kernel + worldNormal).normalized;
        	// if (i == 0){
        	// 	kernel = Vector3.up;
        	// }
        	// Debug.DrawRay(Vector3.zero,kernel*16,Color.red);
			//traceResult += ConeTrace(voxelOrigin.xyz, kernel.xyz, worldNormal.xyz);
    }
	//RECAP from shader
            // sampler2D _Skybox;
            // float4    _MainLight;
            // float4    _Ambientsky;
            // float4    _Ambientcolor;

            // float4    _Origin;
            // float4    _Kernel;
            // sampler2D _Atlas;
            // sampler2D _LMdirect;

            // sampler2D _Accumulation;
            // sampler2D _Display;
            
            // sampler2D _Albedo;
            // sampler2D _Wnormal;
            // sampler2D _Wposition;
            // sampler2D _Shadowmask;

	public RenderTexture returnDisplay(){return displayBuffer;}
	public RenderTexture returnDirect(){return directlight;}
	public RenderTexture returnAccum(){return accumulationBuffer;}

}