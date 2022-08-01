using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: 
//freeform version and hash version (currently hash version)
//make a parent class and derive child class
//(areaProbeData vs hashProbeData) //probably a "shaderless" version too, for generic rendering
public class lightprobeData //: MonoBehaviour
{
	//lightprobeData class manage a custom cubemap format,
	//it initialize the atlas by capturing the scene, at set points, using a cubemap camera
	
	int cellsize = 4; //hash dimension
	int cubemapSize = 64; //size of the cubemap capturing the scene
	int atlasSize = 16; //atlas Size, on the side, hold 16x16 tiles
	int probeNumber = 256; //-> atlasSize² //max number of probes, addressing limit with 8bit for freeform
	int tileSize = 128; //-> 2048/16  //size of tile in the atlas
	int atlasTextureSize = 2048; //atlas size x tilesize //16*128=2048
	
	//TODO: Origine!!!!!!!!! for offset of hash!
	
	//TODO: rethink capture for regular cubemap rendering (multi material)
	//TODO:external?
	public Shader capture;//get a view of the scene using this shader   //passed externally? // see setreplacementshader in updatecell
	Camera lens; //cubemap capture
	GameObject pivot;//camera position
	RenderTexture sceneCapture;//temp cubemap for transfer to atlas


	public Shader transfer;//shader that unwrap cubemap to tile
	Material atlasTransfer;//use to apply unwrap shader to atlas
	public RenderTexture atlas;//should have layer for extra data like depth or LPGB
    //TODO: RenderTexture farfield;//not part of lightprobe?


	//TODO:
	//editor write to this? should be external that pass data?
    //--------------------------------------------------------------------
	//for freeform (cubemap defined by hand placed zone instead of hash)
    // class zone{//use unity built in Bounds class instead?
    //     Vector3 start,size, center;//AABB
    //     //compute center
    //     //get set start size with computecenter updated
    // }
    // zone[] cells;//Bounds[] cells;
    //--------------------------------------------------------------------
    public void initAtlas(){
	    setCameraData();
        SetAtlas();
        updateAll();

		atlas.GenerateMips();
    }

    
    private void setCameraData(){

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
	    lens.SetReplacementShader(capture, "RenderType");
	    
        //set rendertexture
	    sceneCapture = new RenderTexture(cubemapSize, cubemapSize, 24);
        sceneCapture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        sceneCapture.antiAliasing = 1;
        sceneCapture.filterMode = FilterMode.Point;
        sceneCapture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        sceneCapture.depth = 16;

	    sceneCapture.Create();
	    
	    //
	    lens.targetTexture = sceneCapture;
	    lens.forceIntoRenderTexture = true;
        
    }
	private void SetAtlas(){

		//set atlas
		atlas = new RenderTexture(atlasTextureSize, atlasTextureSize, 24);
		atlas.antiAliasing = 1;
		atlas.filterMode = FilterMode.Point;
		atlas.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
		
		atlas.useMipMap = true;
		atlas.autoGenerateMips = false;

		atlas.Create();
		//set material
		atlasTransfer = new Material(transfer);
		atlasTransfer.SetTexture("_Cube",sceneCapture); //pass externally

		//Debug.Log(atlasTransfer);
	}
    public  void updateAll(){
        for (int i = 0; i < probeNumber; i++)
        {
            int x = i / atlasSize;
            int y = i % atlasSize;
	        updateCell(x, y);//TODO:Add the origin!!!!!!!!!
        }
    }

	void destroyCamera(){
		//TODO: how about just disabling it?
		sceneCapture.Release();
		//Destroy(pivot);
		pivot.SetActive(false);
		
	}

    
//-----------------------------------------------------
	public  void updateCell(int x, int y){
		//this fonction capture a cubemap of the scene from the point of view of a given cell
        
		//place camera
		int midcell = cellsize / 2;
        //-------------------- hashed position
        pivot.transform.position = new Vector3((x * cellsize) + midcell, 0, (y * cellsize) + midcell);
	    //-------------------- //TODO:if freeform: for each zone get center data
	    pivot.transform.rotation = Quaternion.identity;//TODO: orientation if freeform is OBB instead of AABB?

		pivot.GetComponent<Camera>().RenderToCubemap(sceneCapture);
		//lens.RenderToCubemap(sceneCapture);
		
        updateTile(x, y);
    }
    private void updateTile(int x, int y){
	    //create the atlas textures zones
        int halftile = tileSize / 2;
        int xt = (x * tileSize) + halftile;
        int yt = (y * tileSize) + halftile;
        float size = tileSize;
        Vector2 position = new Vector2(xt, yt);
        renderTile(size, position);
    }
    private void renderTile(float size, Vector2 offset){
    	//this function use low level immediate render mode to draw on a rendertexture
    	
	    // //TODO:
	    //could probably do the UV selection in shader
        //at each point
        //hash the offset using the size
        //if offset ! of input uniform, add 0
        //else add transferred color using remap UV
        
        offset		= new Vector2 (offset.x - (size/2),offset.y - (size/2));
        Vector2 s	= new Vector2 (size + offset.x,size + offset.y);
        
	    offset	/=atlas.width;
	    s		/=atlas.width;
        
	    Graphics.SetRenderTarget(atlas);
	    // GL.Flush();
        GL.PushMatrix();
        GL.LoadOrtho(); 
        atlasTransfer.SetPass(0);

	    //draw with a quad
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
        Graphics.SetRenderTarget(null);
	}
}