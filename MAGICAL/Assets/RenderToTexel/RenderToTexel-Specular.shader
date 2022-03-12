// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RenderToTexel-Specular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	
	_WorldNormal ("_WorldNormal assigned by script", 2D) = "bump" {}
	_WorldPos ("_WorldPos assigned by script", 2D) = "bump" {}
	
	_WorldSpaceMin ("_WorldSpaceMin assigned by script", Vector) = (1,1,1,1)
	_WorldSpaceSize ("_WorldSpaceSize assigned by script", Vector) = (1,1,1,1)
	
	_LightPos ("_LightPos assigned by script", Vector) = (1,1,1,1)
	_LightColor ("_LightColor assigned by script", Vector) = (1,1,1,1)
	
	_ViewPos ("_ViewPos assigned by script", Vector) = (1,1,1,1)
	
}
SubShader {
	Tags { "RenderType"="Opaque" }
	
	Pass {
		Name "BASE"
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;

struct v2f {
  float4 pos : SV_POSITION;
  float2 uv : TEXCOORD0;
};

v2f vert (appdata_full v) {
  v2f o;
  o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pos = UnityObjectToClipPos (v.vertex);
  return o;
}

half4 frag (v2f i) : COLOR
{
	float4 tex = tex2D(_MainTex, i.uv);
	float Albedo = tex.rgb * _Color.rgb;

	tex.rgb = Albedo * UNITY_LIGHTMODEL_AMBIENT.xyz;

 	return tex;
}
ENDCG
    }
    
	Pass {
		Name "PPL"
		Blend One One
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

sampler2D _MainTex;
sampler2D _WorldNormal;
sampler2D _WorldPos;
float4 _MainTex_ST;
float4 _Color;
float4 _SpecColor;
float4 _WorldSpaceMin;
float4 _WorldSpaceSize;
float4 _LightPos;
float4 _LightColor;
float4 _ViewPos;
float _Shininess;

struct v2f {
  float4 pos : SV_POSITION;
  float2 uv : TEXCOORD0;
};

v2f vert (appdata_full v) {
  v2f o;
  o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
  o.pos = UnityObjectToClipPos (v.vertex);
  return o;
}

half4 frag (v2f i) : COLOR
{
	float4 tex = tex2D(_MainTex, i.uv);
	float3 Albedo = tex.rgb * _Color.rgb;
	float Gloss = tex.a;
	float Alpha = tex.a * _Color.a;
	float Specular = _Shininess;
	float3 Normal = tex2D(_WorldNormal, i.uv).xyz * 2 - 1;
	
	tex = tex2D(_WorldPos, i.uv);
	float3 WorldPos = _WorldSpaceMin.xyz + tex.xyz * _WorldSpaceSize.xyz;
	float3 ViewDir = normalize(_ViewPos-WorldPos);
	float3 LightDir = _LightPos-WorldPos;
	
	float atten = (_LightPos.w - length(LightDir)) / _LightPos.w;
	
	LightDir = normalize(LightDir);
	
  	float3 h = normalize (LightDir + ViewDir);
	
	float diff = max (0, dot (Normal, LightDir));
	
	float nh = max (0, dot (Normal, h));
	float spec = pow (nh, Specular*128.0) * Gloss;
	
	float4 c;
	c.rgb = (Albedo * _LightColor.rgb * diff + _LightColor.rgb * _SpecColor.rgb * spec) * atten;
	c.a = Alpha + _LightColor.a * _SpecColor.a * spec;

 	return c;
}
ENDCG
    }
}
Fallback off
} 