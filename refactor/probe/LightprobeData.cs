using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//freeform version and hash version
//make a parent class and derive child class
//(areaprobedata vs hashprobedata) //probably a "shaderless" version too
public class lightprobeData
{
	int cellsize = 4; //hash dimension
    int cubemapSize = 64;
	int atlasSize = 16; //atlasSize on the side
	int probeNumber = 256; //atlasSize² //max addressing with 8bit for freeform
    int tileSize = 128; //2048/16
	int atlasTextureSize = 2048; //atlas size x tilesize //16*128=2048
	
	//Origine!!!!!!!!!
	
	
	//rethink capture for regular cubemap rendering (multi material)

	public Shader capture;//get a view of the scene    //passed externally? // see setreplacementshader in updatecell

	public Shader transfer;//unwrap cubemap to tile
	Material atlasTransfer;//apply unwrap to atlas
	// public RenderTexture atlas{ get => atlas; protected set => atlas = value; } //should have layer for extra data like depth or LPGB
	public RenderTexture atlas;//{ get => atlas; protected set => atlas = value; } //should have layer for extra data like depth or LPGB
    // RenderTexture farfield;//not part of lightprobe?
	
	//external?
	Camera lens; //cubemap capture
	GameObject pivot;//camera position
	RenderTexture sceneCapture;//temp cubemap for transfer to atlas

    //editor write to this? should be extrenal that pass data?
    //--------------------------------------------------------------------
    //for freeform 
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
    }

    private void SetAtlas(){

        //set atlas
	    atlas = new RenderTexture(atlasTextureSize, atlasTextureSize, 24);
	    atlas.antiAliasing = 1;
	    atlas.filterMode = FilterMode.Point;
	    atlas.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
	    atlas.Create();
        //set material
        atlasTransfer = new Material(transfer);
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

        //set rendertexture
        sceneCapture = new RenderTexture(cubemapSize, cubemapSize, 24);
        sceneCapture.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        sceneCapture.antiAliasing = 1;
        sceneCapture.filterMode = FilterMode.Point;
        sceneCapture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        sceneCapture.depth = 16;
        sceneCapture.Create();
    }
    public  void updateAll(){
        for (int i = 0; i < probeNumber; i++)
        {
            int x = i / atlasSize;
            int y = i % atlasSize;
	        updateCell(x, y);//Add the origin!!!!!!!!!
        }
    }
    public  void updateCell(int x, int y){
        //place camera
		int midcell = cellsize / 2;
        //-------------------- hashed position
        pivot.transform.position = new Vector3((x * cellsize) + midcell, 0, (y * cellsize) + midcell);
        //-------------------- if freeform: for each zone get center
        pivot.transform.rotation = Quaternion.identity;

        //pass capture shader/texture data here
        //should probably have an alternative for regular scene rendering
        lens.SetReplacementShader(capture, "RenderType");
        lens.RenderToCubemap(sceneCapture);

        updateTile(x, y);
    }
    private void updateTile(int x, int y){
        //create atlas textures zones
        int halftile = tileSize / 2;
        int xt = (x * tileSize) + halftile;
        int yt = (y * tileSize) + halftile;
        float size = tileSize;
        Vector2 position = new Vector2(xt, yt);
        renderTile(size, position);
    }
    private void renderTile(float size, Vector2 offset){
        //could probably do teh UV selection in shader
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