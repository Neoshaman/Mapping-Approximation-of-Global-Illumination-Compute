using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MAGICManager : MonoBehaviour//probably not a monobehaviaviour
{
    public globalLights glight;
   	public shaderIndex shaders;
	public GIScene[] scenes;

   //update lights

   //scene management // interface to derive? scriptable code?
    //refresh
    //load/unload/create/save?

    globalLights setLight(){
        //set light direction for shader
        if (glight == null) glight.initNULL();
        glight.main = FindObjectOfType<Light>();// should discriminate light in a function
		glight.directionalLight = -glight.main.transform.forward;

        //default creation if null
        return glight;
    }
    
//*************************************************************
//move out should be done by general scene manager
    void Start(){
       foreach (var scene in scenes){
	       //set position
	       scene.init(setLight(), shaders);
       }
    }

    void Update(){
        //select which scene to refresh/init based on logic
        foreach (var scene in scenes){
            scene.updateLight();//on light change refresh
	        scene.updateGI();
	        //eventually position
        }
    }
//*************************************************************


//set up
    //make sure all mesh in a scene don't overlap on lightmap
    //Add mesh into the geometry array
    //add light, skybox and set ambiant
    //overide start and update for specific management?

}