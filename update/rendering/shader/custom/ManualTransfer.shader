Shader "MAGIC/ManualTransfer"
{
    Properties
    {
        _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader
    {
        Pass
        {
        	//Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
             #include "UnityCG.cginc"
             #include "../MAGIC.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            v2f vert (appdata v)
            {
            	v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = v.vertex;
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            
            
            
            samplerCUBE _Cube;

           
            fixed4 frag (v2f i) : COLOR//(v2f_customrendertexture i) : COLOR
            {
            	float2 g = i.uv - 0.5;
                float3 normal = UnpackNormalFromOct(2*g);
                //float3 normal = i.localTexcoord.xyz;
                //return half4(normal,1);//texCUBE(_Cube, normal);
                return texCUBE(_Cube, normal);
                //return half4(g,0,1);//texCUBE(_Cube, normal);
                //return fixed4(1,0,0,1);//texCUBE(_Cube, normal);

            }
            ENDCG
        }
    }
}
