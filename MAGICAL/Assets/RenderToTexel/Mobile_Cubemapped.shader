// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mobile/Cubemapped" {

Properties 
{
	_ReflCol ("Reflection Color", Color ) = (1,1,1,1)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_EnvTex ("Cube env tex", CUBE) = "black" {}

}

SubShader {
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
	LOD 100
	
	
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	sampler2D _MainTex;
	samplerCUBE _EnvTex;

	float3 _SpecColor;
	float _Shininess;
	float4 _ReflCol;
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 refl : TEXCOORD1;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		
		o.pos	= UnityObjectToClipPos(v.vertex);
		o.uv		= v.texcoord;
		
		float3 worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
		o.refl = reflect(-WorldSpaceViewDir(v.vertex), worldNormal);
		o.refl.x = -o.refl.x;
		
		
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		fixed4 frag (v2f i) : COLOR
		{
			fixed4 c = tex2D (_MainTex, i.uv.xy);
			
			c.xyz += texCUBE(_EnvTex,i.refl) * _ReflCol * c.a;
			
			return c;
		}
		ENDCG 
	}	
}
}


