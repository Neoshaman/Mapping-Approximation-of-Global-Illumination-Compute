Shader "MAGIC/BoxTEST"
{
    Properties
    {
        _MainTex ("Cubemap Atlas", 2D) = "white" {}
        _DirectLightMap ("Direct Lighting", 2D) = "white" {}
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
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "MAGIC.cginc"

            struct d
            {
                float4 vertex	: POSITION;
                float2 uv		: TEXCOORD1;
                fixed4 color    : COLOR;
                fixed3 normal   : NORMAL;
            };

            struct v2f
            {
                float4 vertex	: POSITION;
                float4 wpos     : TEXCOORD1;
                fixed4 color    : COLOR;
                fixed3 wnormals : NORMAL;
            };

            v2f vert (d v)
            {
                v2f o;
                o.wpos = mul(unity_ObjectToWorld, v.vertex);    //world position
                o.vertex = UnityObjectToClipPos(v.vertex);      //screen position
                o.wnormals =UnityObjectToWorldNormal(v.normal); //normal to world normal

                o.color = float4(v.uv, 0,1);// v.color;
                return o;
            }
            
            sampler2D _MainTex;
            float4    _MainLight;
            float4    _Origin;

            //lightcolor,
            //mesh position to align with grid potentially in vertex clamping using the bounding box
            //pass the size and grid range, compute cell size
            //pass number of samples over hemisphere
            fixed4 frag (v2f i) : COLOR
            {
                //set size
                const float size    = 4;
                const float2 cuberange = float2(16,16);

                float  epsilon      = 0.000001;
                float3 origin       = _Origin.xyz;
                float3 worldnorm    = normalize(i.wnormals) + epsilon;
                float3 pos          = i.wpos.xyz - origin + 0.001;

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

                //light test
                float3 light        = normalize(_MainLight);
                float  ndotl        = saturate(dot(light.xyz, worldnorm));
                float  skyocclusion = saturate(dot(float3(0,1,0), worldnorm));
                //skyocclusion *= skyocclusion;

                //shadow sampling, box projected and direct
                float3 lightproj    = BoxProjectVector(pos, light, cubecenter, mincube, maxcube);
                float2 lightbox     = (PackNormalToOct(lightproj) + 1) / 2;
                float2 shadowbox    = (lightbox + hash_id) / cuberange;
                
                float2 lightdirect  = (PackNormalToOct(light) + 1) / 2;
                float2 shadowdirect = (lightdirect + hash_id) / cuberange;
                
                //gather loop
                const float PI = 3.14159265359;
		        const float phi = 1.618033988;
		        const float gAngle = phi * PI * 2.0;
                const int numSamples = 64;

                float4 gi;
                float4 traceResult;

                for (int i = 0; i < numSamples; i++)
                {
                    float fi = (float)i;
                    float fiN = fi / numSamples;
                    float longitude = gAngle * fi;
                    float latitude = asin(fiN * 2.0 - 1.0);
                     
                    float3 kernel;
                    kernel.x = cos(latitude) * cos(longitude);
                    kernel.z = cos(latitude) * sin(longitude);
                    kernel.y = sin(latitude);
                    kernel = normalize(kernel + worldnorm);
                    if (i == 0){
                        kernel = float3(0.0, 1.0, 0.0);
                    }
                    traceResult += 1;// ConeTrace(voxelOrigin.xyz, kernel.xyz, worldNormal.xyz);
                }
                traceResult /= numSamples;
                //gi.rgb = traceResult.rgb;
                //gi.rgb *= 4.3;
                //gi.rgb += traceResult.a * 1.0 * SEGISkyColor;
                //float4 result = float4(gi.rgb, 2.0);

                //cubemap result
                float4 cubesample   = tex2D   ( _MainTex, samplepos );
                float4 boxshadow    = tex2Dlod( _MainTex, float4(shadowbox,0,7));//tex2D(_MainTex, shadowtest);
                float4 directlight  = tex2Dlod( _MainTex, float4(shadowdirect,0,4));
                float4 occlufactor  = tex2Dlod( _MainTex, float4(shadowdirect,0,7));
                float4 occlusion    = occlufactor.b * (skyocclusion + 1.0);
                return occlusion;
            }

            float4 traceRays(){
                //box sample cubemap to get uv
                //use uv to sample direct lightmap to get illumination
                //mask sample by skybox mask 
                //sample skybox
                //mask skybox by skybox mask
                //return sample + skybox
            }
            ENDCG
        }
    }
}
