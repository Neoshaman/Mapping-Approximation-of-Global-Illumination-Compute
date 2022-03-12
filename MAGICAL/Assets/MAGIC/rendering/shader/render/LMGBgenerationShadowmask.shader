Shader "MAGIC/LMGBgenerationShadowmask"
{
     // Properties
    // {
    //     _MainTex ("Texture", 2D) = "white" {}
    // }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "../MAGIC.cginc"

             // half4 unity_LightmapST;
            //float4 unity_Lightmap;

            struct d
            {
                float4 vertex    : POSITION;
                float2 uv        : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex    : POSITION;
                float2 uv       : TEXCOORD1;
                LIGHTING_COORDS(2,3)
            };
            //------------------------------------
            v2f vert (d v)
            {
                v2f o;
                float2 uv = float2(v.uv.x, 1-v.uv.y);//* unity_LightmapST.xy + unity_LightmapST.zw;//TRANSFORM_TEX(v.uv, unity_Lightmap );;
                o.vertex = float4((uv.xy*2)-1,1,v.vertex.w);

                //
               // o.vertex = float4((v.uv.xy*2)-1,1,v.vertex.w);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                float atten = LIGHT_ATTENUATION(i); // This is a float for your shadow/attenuation value, multiply your lighting value by this to get shadows. Replace i with whatever you've defined your input struct to be called (e.g. frag(v2f [b]i[/b]) : COLOR { ... ).
                float4 f1 = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv[1]);
                float3 f2 = DecodeLightmap(f1);
                return half4(f2,1)*atten;
                //return (DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv[1])));
            }
            ENDCG
        }
    }
}
