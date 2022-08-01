Shader "MAGIC/BoxDirectionalOcclusionTEST"
{
    Properties
    {
        _MainTex ("Cubemap Atlas", 2D) = "white" {}
    }
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
            #include "MAGIC.cginc"

            struct d
            {
                float4 vertex	: POSITION;
                //float2 uv		: TEXCOORD1;
                fixed4 color    : COLOR;
                fixed4 normal   : NORMAL;
            };

            struct v2f
            {
                float4 vertex	: POSITION;
                float4 wpos     : TEXCOORD0;
                fixed4 color    : COLOR;
                fixed3 wnormals : NORMAL;
            };
			//------------------------------------
            //set camera relative position ???
            //hash cubemap ID
            //set cubemap pos and dimension
            //take light dir
            //box project light dir ???
            //boxproject normal
            //sample light dir occlusion on cubemap atlas as shade
            //sample cubemap UV color as color
            
            sampler2D _MainTex;
            float4 _LIGHT;
            

            v2f vert (d v)
            {
                v2f o;
              
                //vertex world position
                o.wpos = mul(unity_ObjectToWorld, v.vertex);

                //vertex screen position
                o.vertex = UnityObjectToClipPos(v.vertex);

                //normal to world normal
                o.wnormals =UnityObjectToWorldNormal(v.normal);

                //o.vertex = UnWrapToScreenSpace(float2 v.uv, float4 v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                //fake test light dir
                float3 fakelight = normalize(float3(0.5,0.5,0.5));
                float ndotl = saturate(dot(fakelight, i.wnormals));
                
                //set size
                const float size = 4;
                const float2 cuberange = float2(16,16);
                //hash position to read the right cubemap in the atlas
                float4 hashpos = floor(i.wpos/size);
                float2 hash_id = min(float2(0,0), max(hashpos.xy, cuberange));

                //TODO<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                float3 cubecenter = (float3(hash_id.xy,hashpos.z) * 4) + (size/2) ;
                float3 mincube;
                float3 maxcube;

                //boxproject(wpos,wnormal, cubecenter, cubemin,cubemax)
                float3 projected = BoxProjectVector(i.wpos,i.wnormals, cubecenter, mincube, maxcube);
                //get the oct position on teh cubemap
                float2 octnormal = hash_id*size + (PackNormalToOct(i.wnormals)/cuberange);//TODO<<<<<<<<<<<<<<<<<<<
                //transform oct to hashed cubemap
                float2 samplepos = octnormal;



                //sample the cubemap in the direction (world to oct)
                float4 cubesample = tex2D(_MainTex, samplepos);
                //if sample b is bigger than 0 then ndotl else set color to shadow, color is uv
                float4 result = float4(ndotl,ndotl,ndotl,1)*cubesample;
                return (cubesample.z > 0) ? result : float4(0,0,0,1);

                //final color
                //return fixed4(1,0,0,1)*i.color;
            }
            ENDCG
        }
    }
}
