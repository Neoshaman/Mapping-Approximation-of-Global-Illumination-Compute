using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SurfaceCanvas
{

    //rendertexture, material

    //init canvas()
    //draw()
    //close canvas()

    //surface: CanvasPosition, CanvasRotation, CanvasScale
    //mesh: camera position, camera settings, perspective

    //render a 
    //- surface/mesh
    //onto a 
    //- rendertexture (canvas)
    //using a 
    //- shader


    // get matrix from the Transform
    //var matrix = transform.localToWorldMatrix;


    public static void init(RenderTexture RT, Material mat)
    {
        Graphics.SetRenderTarget(RT);
        GL.Clear(true, true, Color.black);
        GL.PushMatrix();
        GL.LoadOrtho();
        mat.SetPass(0);
    }

    public static void close()
    {
        GL.PopMatrix();
        Graphics.SetRenderTarget(null);
    }

    public static void fullScreen()
    {
        GL.Begin(GL.TRIANGLE_STRIP);

        GL.TexCoord(new Vector3(0,0,0));
        GL.Vertex3(0, 0, 1);

        GL.TexCoord(new Vector3(0, 1, 0));
        GL.Vertex3(0, 1, 1);

        GL.TexCoord(new Vector3(1, 0, 0));
        GL.Vertex3(1, 0, 1);

        GL.TexCoord(new Vector3(1, 1, 0));
        GL.Vertex3(1, 1, 1);

        GL.End();
    }


    //surface : offset2, size, size2, rotation  --polymorphism
    public static void surface(Vector2 offset, float size)//what's the size?
    {
        offset = new Vector2(offset.x - (size / 2), offset.y - (size / 2));
        Vector2 s = new Vector2(size + offset.x, size + offset.y);

        //offset /= width;
        //s /= width;

        GL.Begin(GL.TRIANGLE_STRIP);

        GL.TexCoord(new Vector3(0, 0, 0));
        GL.Vertex3(offset.x, offset.y, 1);

        GL.TexCoord(new Vector3(0, 1, 0));
        GL.Vertex3(offset.x, s.y, 1);

        GL.TexCoord(new Vector3(1, 0, 0));
        GL.Vertex3(s.x, offset.y, 1);

        GL.TexCoord(new Vector3(1, 1, 0));
        GL.Vertex3(s.x, s.y, 1);

        GL.End();
    }

    public static void stamping(Vector2 offset, Vector3 orientation, Vector3 scaling) //version with normalized size and uniform scale
    {

    }

//    void Draw()
//    {
//        //Graphics.DrawMeshNow(mesh, matrix, MatIndex);
//    }
}