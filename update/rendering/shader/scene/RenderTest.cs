using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTest : MonoBehaviour {
    RenderTexture Canvas;
    public Material Painter;
    public GameObject Stamp;

    Mesh MeshStamps;
    Material CanvasFrame;

    //public Texture tex;


    private void Awake()
    {
        Canvas = new RenderTexture(512, 512, 24);
        MeshStamps = Stamp.GetComponent<MeshFilter>().mesh;
        CanvasFrame = this.gameObject.GetComponent<MeshRenderer>().material;

        CanvasFrame.SetTexture("_MainTex", Canvas);
        //Material Wallpaper = Stamp.gameObject.GetComponent<MeshRenderer>().material;
        //Wallpaper.SetTexture("_MainTex", tex);

    }
    void Update () {
        Matrix4x4 Placement = Stamp.transform.localToWorldMatrix;//worldToLocalMatrix;
        Placement.SetTRS(
            new Vector3(0.3f,0.3f,0),
            Quaternion.Euler(new Vector3(-90,0,0)),
            new Vector3(10,10,10));
        //Debug.Log(Camera.current);

        SurfaceCanvas.init(Canvas, Painter);
        Graphics.DrawMeshNow(MeshStamps, Placement);
        //SurfaceCanvas.fullScreen();
        SurfaceCanvas.close();

		
	}
	
	// Update is called once per frame
	//void Update () {	}

    //en.m.wikibooks.org : GLSL programming/Unity/Debugging of Shaders
    //catlikecoding.com : custom render pipeline
    //community.arm.com : shader pixel local storage
    //arm-software.github.io : advence shading technique with pixel local storage
    // arm 404
    //bitbucket.org/catlikecodingunitytutorial... :
    // arm 404
    //what-when-how.com /tutorial/topic-547pjramj8/GPU-Pro-Advanced-Rendering-Techniques-295.HTML /: bandwidth efficiency
    //
}