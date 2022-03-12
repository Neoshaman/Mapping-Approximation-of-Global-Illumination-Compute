Shader "MAGIC/DirectLitBoxShadow"
{
    Properties
    {
        _MainTex ("Cubemap Atlas", 2D) = "black" {}
        // _DirectLightMap ("Direct Lighting", 2D) = "black" {}
        //_MainLight ("Main Light", Vector) = (1,1,1,1)
        //_Origin ("Origin", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		//Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vertexProgram
            #pragma fragment fragmentProcessing
            #include "UnityCG.cginc"
            #include "ShaderTools.cginc"

            struct meshData
            {
                float4 vertex	: POSITION;
                float2 uv		: TEXCOORD1;
                fixed4 color    : COLOR;
                fixed3 normal   : NORMAL;
            };

            struct rasterData
            {
                float4 vertex	: POSITION;
                float4 wpos     : TEXCOORD1;
                fixed4 color    : COLOR;
                fixed3 wnormals : NORMAL;
            };

            rasterData vertexProgram (meshData input)
            {
                rasterData output;
                output.wpos = mul(unity_ObjectToWorld, input.vertex);    //world position
                output.vertex = UnityObjectToClipPos(input.vertex);      //screen position
                output.wnormals =UnityObjectToWorldNormal(input.normal); //normal to world normal

                output.color = float4(input.uv, 0,1);// v.color;
                return output;
            }
            
            sampler2D _MainTex;
            float4    _MainLight;
            float4    _Origin;
            float3    _AmbientColor;

            //lightcolor,
            //mesh position to align with grid potentially in vertex clamping using the bounding box
            //pass the size and grid range, compute cell size
            fixed4 fragmentProcessing (rasterData input) : COLOR
            {
                //set size
                const float size    = 4;
                const float2 cuberange = float2(16,16);

                float  epsilon      = 0.000001;
                float3 origin       = _Origin.xyz;
                float3 worldnorm    = normalize(input.wnormals) + epsilon;
                float3 pos          = input.wpos.xyz - origin + 0.001;

                //hash position to read the right cubemap in the atlas
                float3 hashpos      = floor(pos / size); 
                float3 hash_offset  = hashpos * size;
                float2 hash_id      = max(float2(0,0), min(hashpos.xz, cuberange)); 

                //box projection
                float3 cubecenter   = hash_offset + (size / 2);
                float3 mincube      = hash_offset + 0;
                float3 maxcube      = hash_offset + size;
                float3 projected    = BoxProjectVector(pos, worldnorm, cubecenter, mincube, maxcube);
                
                //sampling the atlas
                float2 octnormal    = (PackNormalToOct(projected) + 1) / 2;
                float2 samplepos    = (octnormal + hash_id) / cuberange;

                //light
                float3 light        = normalize(_MainLight);
                float  ndotl        = saturate(dot(light.xyz, worldnorm));
                // float  skyocclusion = saturate(dot(float3(0,1,0), worldnorm));//should be wrap lighting
                float  skyocclusion = wrappedTo1(worldnorm,float3(0,1,0));
                //skyocclusion *= skyocclusion;

                //shadow sampling, box projected and direct
                float3 lightproj    = BoxProjectVector(pos, light, cubecenter, mincube, maxcube);
                float2 lightbox     = (PackNormalToOct(lightproj) + 1) / 2;
                float2 shadowbox    = (lightbox + hash_id) / cuberange;
                
                float2 lightdirect  = (PackNormalToOct(light) + 1) / 2;
                float2 shadowdirect = (lightdirect + hash_id) / cuberange;
                
                //cubemap result
                float4 boxshadow    = tex2Dlod( _MainTex, float4(shadowbox,0,7));//tex2D(_MainTex, shadowtest);
                float4 directlight  = tex2Dlod( _MainTex, float4(shadowdirect,0,4));
                float4 occlufactor  = tex2Dlod( _MainTex, float4(shadowdirect,0,7));
                float4 occlusion    = occlufactor.b * (skyocclusion + 1.0);
                return occlusion;
            }
            ENDCG
        }
    }
}
