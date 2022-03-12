using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

//[MenuItem("Assets/Create/Inventory Item List")]
[CreateAssetMenu(menuName = "Assets/Shader Inventory")]
public class shaderIndex : ScriptableObject
{
    public Shader cubemapCapture;
    public Shader cubemapTransfer;

    public Shader bakeAlbedo;
    public Shader bakeWorldNormal;
    public Shader bakeWorldPosition;
    public Shader bakeShadowMask;

    public Shader GI;
    public Shader DirectLight;

	public Shader GILMLit;
    
	public Shader debugshader;
}