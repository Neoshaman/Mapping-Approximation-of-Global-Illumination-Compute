using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAGICAL //MAGIC applied by lightprobe
//need a parent class (MAGIC: mapping approximation of global illumination compute)
//alternative -> (MAGIC: mapping approximation of global illumination cache)
//to derive other implementation like:
//MAGIC HAT: by hemisperic atlas tiling
//MAGIC TRICK: by tiled ray indirection as compressed kernel
{
	int size = 256; //8bit indexing limits //UV using 8bit channel to index
    public int rayCounter = 0;
    Vector4 Kernel;

    globalLights globalLights;

    public Shader GI;
    Material GIpass;

    //BUFFER
	RenderTexture displayBuffer;
	RenderTexture accumulationBuffer;
    RenderTexture swap;
    
    public Shader direct;
    Material directPass;
	RenderTexture directlight;

    public  void SetGlobalLights(globalLights lights){
        globalLights = lights;
    }
	private void initDirectLight(Mesh[] mesh, RenderTexture probes){
        directPass = new Material(direct);
		directPass.SetTexture("_Atlas", probes, UnityEngine.Rendering.RenderTextureSubElement.Default);
        RenderSurface.setCanvas(directlight,size);
        RenderSurface.applyShader(mesh,directlight,directPass);
	}
    private void initGIBuffer(Mesh[] mesh, Vector3 origin, RenderTexture probes, RenderTexture[] LMGB){
        GIpass = new Material(GI);
        setGlobalLightsToGI();
        setSceneDataToGI(origin,probes);
        setGIBufferToGI();
        setLMGBToGI(LMGB);
        RenderSurface.setCanvas(displayBuffer,size);
        RenderSurface.setCanvas(accumulationBuffer,size);
        RenderSurface.applyShader(mesh,displayBuffer,directPass);
        RenderSurface.applyShader(mesh,accumulationBuffer,directPass);
    }
    public  void InitMAGICAL(Mesh[] mesh, Vector3 origin, RenderTexture probes, RenderTexture[] LMGB){
        Kernel = new Vector4();
	    initDirectLight(mesh,probes);
	    initGIBuffer(mesh,origin,probes,LMGB);
    }
 
    public  void updateDirectLight(Mesh[] mesh){
        RenderSurface.applyShader(mesh,directlight,directPass);
    }
    public  void updateGIBuffer(Mesh[] mesh){
        const int numSamples = 64;
		const float phi = 1.618033988f;
		const float gAngle = phi * Mathf.PI * 2.0f;
		Vector3 worldNormal = Vector3.up; 
        
        // for (int rayCounter = 0; rayCounter < numSamples; rayCounter++){
        	float fi = (float)rayCounter;
        	float fiN = fi / numSamples;
        	float longitude = gAngle * fi;
        	float latitude = Mathf.Asin(fiN * 2.0f - 1.0f);
                     
        	Vector3 kernel = new Vector3(
        		Mathf.Cos(latitude) * Mathf.Cos(longitude),
        		Mathf.Cos(latitude) * Mathf.Sin(longitude),
        		Mathf.Sin(latitude)
			);
            //send _Kernel.rgb to shader, pass .a = count 
            Kernel = new Vector4(kernel.x,kernel.y,kernel.z,rayCounter);
            GIpass.SetVector("_Kernel",Kernel);
            RenderSurface.applyShader(mesh,accumulationBuffer,GIpass);

            rayCounter +=1;
            rayCounter %= numSamples;
            if (rayCounter == 0){
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

            public RenderTexture returnDisplay(){
                return displayBuffer;
            }

    void setGlobalLightsToGI(){
        GIpass.SetTexture("_Skybox", globalLights.sky, UnityEngine.Rendering.RenderTextureSubElement.Default);
        GIpass.SetVector("_MainLight",globalLights.directionalLight);
        GIpass.SetColor("_Ambientsky",globalLights.ambientSky);
        GIpass.SetColor("_Ambientcolor",globalLights.ambientLight);
    }
    void setSceneDataToGI(Vector3 Origin, RenderTexture probes){
        GIpass.SetVector("_Origin",Origin);
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
}