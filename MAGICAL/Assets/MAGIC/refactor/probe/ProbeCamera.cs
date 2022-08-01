public class ProbeCamera //: MonoBehaviour
{
   	public Shader capture;//get a view of the scene using this shader   //passed externally? // see setreplacementshader in updatecell
	Camera lens; //cubemap capture
	GameObject pivot;//camera position
	RenderTexture sceneCapture;//temp cubemap for transfer to atlas

    private void setCameraData(){
        //set rendertexture
	    sceneCapture = new RenderTexture(cubemapSize, cubemapSize, 24);
        sceneCapture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        sceneCapture.antiAliasing = 1;
        sceneCapture.filterMode = FilterMode.Point;
        sceneCapture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        sceneCapture.depth = 16;
	    sceneCapture.Create();
	    
        //set camera
        pivot = new GameObject("CaptureScene");
        pivot.AddComponent<Camera>();

        //Set camera parameters
        lens = pivot.GetComponent<Camera>();
	    lens.backgroundColor = Color.blue;
        lens.clearFlags = CameraClearFlags.SolidColor;
        lens.allowMSAA = false;
        lens.cullingMask = 1 << 8;//what mask?

	    //TODO: pass capture shader/texture data here
	    //should probably have an alternative for regular scene rendering
        //if (replacement == true)
	    lens.SetReplacementShader(capture, "RenderType");

	    lens.targetTexture = sceneCapture;
	    lens.forceIntoRenderTexture = true;  
    }

    void destroyCamera(){
		//TODO: how about just disabling it?
		sceneCapture.Release();
		//Destroy(pivot);
		pivot.SetActive(false);
		
	}

    void render(){
		pivot.GetComponent<Camera>().RenderToCubemap(sceneCapture);
		//lens.RenderToCubemap(sceneCapture);
    }
}