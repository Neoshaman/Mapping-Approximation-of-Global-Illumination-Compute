using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Assets/MAGIC config")]
public class MAGICconfig : ScriptableObject //TODO:probably no longer a scriptable, no data is hold, managed by manager
{
	//global lights
    //shader holders
    public globalLights glight;
   	public shaderIndex shaders;    //probe config --> lightprobe data
	
    // int cellsize = 4; //hash dimension
	// int cubemapSize = 64; //size of the cubemap capturing the scene
	// int atlasSize = 16; //atlas Size, on the side, hold 16x16 tiles
	// int probeNumber = 256; //-> atlasSize² //max number of probes, addressing limit with 8bit for freeform
	// int tileSize = 128; //-> 2048/16  //size of tile in the atlas
	// int atlasTextureSize = 2048; //atlas size x tilesize //16*128=2048
    //free probe or hash probe toggle
    //probe camera?
	
}