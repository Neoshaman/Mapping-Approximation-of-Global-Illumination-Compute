using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class RenderSurface
{
	//low level graphic rendering utils 
	public  static void initRender(RenderTexture RT, Material mat){
		Graphics.SetRenderTarget(RT);
		// GL.Flush();
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(0);
	}
    public  static void clear(){
        GL.Clear(true, true, Color.black);
    }
    public  static void render(Mesh[] mesh){		
	    //cycle//spin//run//shift//jolt//period//turn//swing//generation//phase//index
		//pass//sweep//loop//order//move//lap//walk//trace//tour//epoch//time//session
		for (int pass = 0; pass < mesh.Length; pass++)
		{
			Graphics.DrawMeshNow(mesh[pass],  Matrix4x4.identity);
		}
	}
	public  static void close(){
		GL.PopMatrix();
		Graphics.SetRenderTarget(null);
	}
	//---------------------------------------------------------------------
	//refactor show into a more generic class that takes texture name
	public  static void show(GameObject frame, RenderTexture canvas){
		Material CanvasFrame = frame.GetComponent<MeshRenderer>().sharedMaterial;
		//Material CanvasFrame = frame.GetComponent<Renderer>().material;
		CanvasFrame.SetTexture("_MainTex", canvas);
    }
	public  static void showTexture(GameObject frame, RenderTexture canvas, string textureName){
		Material CanvasFrame = frame.GetComponent<MeshRenderer>().sharedMaterial;
		//Material CanvasFrame = frame.GetComponent<Renderer>().material;
		CanvasFrame.SetTexture(textureName, canvas);
    }
	
	public  static void showATLAS(GameObject frame, RenderTexture canvas){
		Material CanvasFrame = frame.GetComponent<MeshRenderer>().sharedMaterial;
		//Material CanvasFrame = frame.GetComponent<Renderer>().material;
		CanvasFrame.SetTexture("_Atlas", canvas);
    }
	public  static void initCanvasFrame(Mesh[] mesh, Shader shader, RenderTexture canvas, int size){
		setCanvas(canvas, size);
        draw(mesh, shader, canvas);
	}
	//--------------------------------------------------------------------------------
    private static void draw(Mesh[] mesh, Shader shader, RenderTexture canvas){
	    Material painter = new Material(shader);
	    applyShader(mesh,canvas, painter);
    }
	public static void setCanvas( RenderTexture canvas, int size){
        canvas.antiAliasing = 1;
        canvas.filterMode = FilterMode.Point;
        canvas.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        canvas.depth = 16;
        canvas.Create();
	}
	public static void applyShader(Mesh[] mesh, RenderTexture canvas, Material painter){
        RenderSurface.initRender(canvas, painter);
        RenderSurface.clear();
        RenderSurface.render(mesh);
        RenderSurface.close();
    }
	 private void renderTile(float size, Vector2 offset, RenderTexture canvas, Material painter){
    	
	    // //TODO:
	    //could probably do the UV selection in shader
        //at each point
        //hash the offset using the size
        //if offset ! of input uniform, add 0
        //else add transferred color using remap UV
        
        offset		= new Vector2 (offset.x - (size/2),offset.y - (size/2));
        Vector2 s	= new Vector2 (size + offset.x,size + offset.y);

		//why is that again?
	    offset	/=canvas.width;
	    s		/=canvas.width;
        
	    Graphics.SetRenderTarget(canvas);
	    // GL.Flush();
        GL.PushMatrix();
        GL.LoadOrtho(); 
        painter.SetPass(0);

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
