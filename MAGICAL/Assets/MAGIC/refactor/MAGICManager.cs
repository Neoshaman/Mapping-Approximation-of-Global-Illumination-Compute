using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MAGICManager : MonoBehaviour//probably not a monobehaviaviour
{
    public globalLights glight;
   	public shaderIndex shaders;
	public GIScene[] scenes;
	
	public GameObject DebugRoot;
	
	// public Matrix4x4 sceneposition;

	//TODO:
	//update lights
    //scene management // interface to derive? scriptable code?
    //refresh
    //load/unload/create/save?

    // globalLights setLight(){
    //     //set light direction for shader
    //     if (glight == null) glight.initNULL();
	//     glight.main = FindObjectOfType<Light>();// should discriminate light in a function, test scene should only have 1 light
	// 	glight.directionalLight = -glight.main.transform.forward;
    //     return glight;
    // }
    
	// globalLights updateLight(){
    //     //set light direction for shader
	// 	glight.directionalLight = -glight.main.transform.forward;
    //     return glight;
    // }
    
//*************************************************************
//TODO: move out should be done by general scene manager
    void Start(){
    // void Startup(){
	    foreach (var scene in scenes){
	    	
	    	scene.root = Instantiate(DebugRoot, 
		    	new Vector3(32,0,32),
		    	//Vector3.zero,
		    	//Quaternion.Euler(0,0,0)
		    	Quaternion.Euler(-90,0,0)
		    	//Quaternion.identity
	    	);
	    	
	    	//Instantiate(scene.root, 
		    //	new Vector3(32,0,32),
		    //	//Vector3.zero,
		    //	Quaternion.Euler(-90,0,0)
		    //	//Quaternion.identity
	    	//);
		    if (glight == null) glight.initNULL();
		    glight.setMainLight();
	    	
	    	//TODO: set position
	    	scene.init(glight, shaders);
	    	//scene.root = DebugRoot;
       }
    }
    void Update(){

        //TODO: select which scene to refresh/init based on logic
	    foreach (var scene in scenes){
		   glight.updateLight();
	       scene.updateLight();//TODO: when light change, refresh
	       scene.updateGI();
	       //TODO: eventually position, for example circular grid of scene
        }
    }
//*************************************************************

//TODO: 
//set up
	//make sure all mesh in a scene don't overlap on lightmap (currently just loop them, test scene should have 1 mesh anyway)
	//Add mesh into the geometry array (done)
	//add light, skybox and set ambiant (in prtogress)
    //overide start and update for specific management?

}