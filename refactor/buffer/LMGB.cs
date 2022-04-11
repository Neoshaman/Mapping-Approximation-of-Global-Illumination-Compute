using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMGB //HOLD and initialize graphic data in a lightmap to compute light in shader
//TODO:
//probably need a parent class
//(GIGB: global illumination graphic buffer)
//to derive other GB like SMGB, VPMGB, SPMGB
{
	int size = 256; //max = 256 : 8bit indexing limits//65536 pixels/splat addressable
	public RenderTexture[] texture;

    public Shader
		bakeAlbedo,
		bakeWorldNormal,
		bakeWorldPosition,
		BakeShadowMasking;
    Shader[] shader;

	//TODO:how about making a LMGBLayer class objects to put in array, encapsulate shader, texture, name

    // public RenderTexture albedo			{ get => texture[0]; protected set => albedo		= value; }
    // public RenderTexture worldNormal	{ get => texture[1]; protected set => worldNormal	= value; }
    // public RenderTexture worldPosition	{ get => texture[2]; protected set => worldPosition	= value; }
    // public RenderTexture shadowMasking	{ get => texture[3]; protected set => shadowMasking	= value; }

	void setShader(){
		shader = new Shader[4];

        shader[0] = bakeAlbedo;
        shader[1] = bakeWorldNormal;
        shader[2] = bakeWorldPosition;
        shader[3] = BakeShadowMasking;
	}
	public void initializeLMGB( Mesh[] mesh){
		setShader();
		texture = new RenderTexture[4];
		for (int lmgbLayer = 0; lmgbLayer < texture.Length; lmgbLayer++)
        {
			RenderSurface.initCanvasFrame(mesh, shader[lmgbLayer], texture[lmgbLayer], size);//null exception
        }
	}
}
