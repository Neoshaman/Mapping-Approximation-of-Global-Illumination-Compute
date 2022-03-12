using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class RenderSurface
{
	public  static void initRender(RenderTexture RT, Material mat){
		Graphics.SetRenderTarget(RT);
		GL.Flush();
		GL.PushMatrix();
		GL.LoadOrtho();
		mat.SetPass(0);
	}
    public  static void clear(){
        GL.Clear(true, true, Color.black);
    }
    public  static void render(Mesh[] mesh){		
		//cycle//spin//run//shift//jolt//period//turn//swing//generation//phase//
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
	public  static void show(GameObject frame, RenderTexture canvas){
		Material CanvasFrame = frame.GetComponent<MeshRenderer>().sharedMaterial;
		// Material CanvasFrame = frame.GetComponent<Renderer>().Material;
		CanvasFrame.SetTexture("_MainTex", canvas);
    }
	public  static void initCanvasFrame(Mesh[] mesh, Shader shader, RenderTexture canvas, int size){
        // canvas = new RenderTexture(size, size, 24);
        // canvas.antiAliasing = 0;
        // canvas.filterMode = FilterMode.Point;
        // canvas.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;
        // canvas.depth = 16;
        // canvas.Create();
		setCanvas(canvas, size);
        draw(mesh, shader, canvas);
    }
    private static void draw(Mesh[] mesh, Shader shader, RenderTexture canvas){
        Material painter = new Material(shader);
        RenderSurface.initRender(canvas, painter);
        RenderSurface.clear();
        RenderSurface.render(mesh);
        RenderSurface.close();
    }
	public static void setCanvas( RenderTexture canvas, int size){
		canvas = new RenderTexture(size, size, 24);
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
}
