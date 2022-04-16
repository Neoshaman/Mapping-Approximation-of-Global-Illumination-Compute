using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/Global Light")]
public class globalLights : ScriptableObject
{
    //specific struct to hold light data //to allow swapping
    public Light main;
    public Vector3 directionalLight;
    public Color ambientLight;// shadow colors
    public Color ambientSky;// ambient sky colors
	public RenderTexture sky;//skybox

    public void initNULL(){
        if (main == null) main = new Light();
        Debug.Log(main);
        directionalLight = new Vector3();
        ambientLight = new Color();
        ambientSky = new Color();
	    sky = new RenderTexture(256,256,24);
    }
}