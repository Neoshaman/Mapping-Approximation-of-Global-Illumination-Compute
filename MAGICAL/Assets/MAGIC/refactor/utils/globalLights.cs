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
        directionalLight = new Vector3();
        ambientLight = new Color();
        ambientSky = new Color();
	    sky = new RenderTexture(256,256,24);
    }
    
    public void init(Color ambient, Color skyColor, RenderTexture skyTexture){
        if (main == null) setMainLight();
        ambientLight = ambient;
        ambientSky = skyColor;
	    sky = skyTexture;
    }

    public void setMainLight(){
        //set light direction for shader
	    main = FindObjectOfType<Light>();// should discriminate light in a function, test scene should only have 1 light
		directionalLight = -main.transform.forward;
        if (main == null) initNULL();
    }
    
	public void updateLight(){
        //set light direction for shader
		directionalLight = -main.transform.forward;
    }

}