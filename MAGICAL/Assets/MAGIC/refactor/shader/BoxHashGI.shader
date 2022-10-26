Shader "MAGIC/BoxGI"
{
    Properties
    {//no properties, passed by code//commented for documentation
        
        //Globals
        // _Skybox ("Skybox", 2D) = "black" {}                 //might be replace with a color, or cosine dome
        // _MainLight ("Main Light", Vector) = (1,1,1,1)
        // _Ambientsky ("Ambient sky", Color) = (0,0,0,0)      //in place of skybox
        // _Ambientcolor ("Origin", Color) = (0,0,0,0)         //brighter shadow?
        
        //scene
        // _Origin ("Origin", Vector) = (0,0,0,0)
        // _Kernel ("Kernel", Vector) = (0,0,0,0)
        // _Atlas ("Cubemap Atlas", 2d) = "black" {}            //rg = uv, b= skymask, a=distance?-> =ba?
        // _LMdirect ("Direct Lighting", 2D) = "black" {}       //could be online

        //Buffer
        // _Accumulation ("Direct Lighting", 2D) = "black" {}
        // _Display ("Direct Lighting", 2D) = "black" {}

        //LMGB
        // _Albedo ("Direct Lighting", 2D) = "black" {}         //Probably part of Direct
        // _Wnormal ("Direct Lighting", 2D) = "black" {}
        // _Wposition ("Direct Lighting", 2D) = "black" {}      //to test vs cubemap atlas
        // _Shadowmask ("Direct Lighting", 2D) = "black" {}     //Rolled into direct

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
            // #include "MAGICTools.cginc"

            float4x4 _PosMat;
            float4x4 _RotMat;


            struct meshData
            {
                float4 vertex	: POSITION; 
                fixed3 normal   : NORMAL;
                 float2 uv		: TEXCOORD0;
                // fixed4 color    : COLOR;
            };

            struct rasterData
            {
                float4 vertex	: POSITION;
                float4 wpos     : TEXCOORD0;
                fixed3 wnormals : NORMAL;
                // fixed4 color    : COLOR;
            };

            rasterData vertexProgram ( meshData input)//, out rasterData output )
            {
                rasterData output;
                output.vertex = unwrap(input.uv, input.vertex.w);      //screen position
                output.wpos     = mul ( _PosMat, input.vertex );//world position
                output.wnormals = mul(input.normal, (float3x3)unity_WorldToObject);
                return output;
            }
            
            //set size
            // float numRays = 64; // set that externally
            // float  size   = 4;//not const, expose as parameter
            // float2 cuberange = float2 ( 16, 16 );//not const, expose as parameter            
            
            //globals
            sampler2D _Skybox;              //not used yet
            float4    _MainLight;           //not used see LMdirect
            float4    _Ambientsky;
            float4    _Ambientcolor;        //not used yet

            //scene
            float4    _Origin;
            float4    _Kernel;
            sampler2D _Atlas;
            sampler2D _LMdirect;
            
            //buffer
            sampler2D _Accumulation;        //not used yet - this shader accumulate to
            sampler2D _Display;             
            
            //lmgb
            sampler2D _Albedo;              //not used - see lmdirect
            sampler2D _Wnormal;
            sampler2D _Wposition;           //not used yet - possibly attenuation
            sampler2D _Shadowmask;          //not used yet - see lmdirect


            //mesh position to align with grid potentially in vertex clamping using the bounding box
            //pass the size and grid range, compute cell size
            fixed4 fragmentProcessing ( rasterData input ) : COLOR
            {
                //set size
                const float numRays = 64; // set that externally
                const float  size   = 4;//not const, expose as parameter
                const float2 cuberange = float2 ( 16, 16 );//not const, expose as parameter
                const float  epsilon   = 0.000001;

                float3 origin       = _Origin.xyz;
                float3 worldnorm    = normalize ( input.wnormals ) + epsilon;
                float3 pos          = input.wpos.xyz - origin + 0.001;//why the 0.001 again?

                //hash position to read the right cubemap in the atlas
                float3 hashpos      = floor ( pos.xyz / size );
                float3 hash_offset  = hashpos * size;
                float2 hash_id      = max ( float2 ( 0, 0 ), min ( hashpos.xz, cuberange ) );
              
                //current Ray direction, passed as paramater
                float3 ray = normalize ( _Kernel.rgb + worldnorm );
                float cosineterm = ndotl(worldnorm,ray);
         
                //box projection of world normal
                float3 cubecenter   = hash_offset + ( size / 2 );
                float3 mincube      = hash_offset + 0;
                float3 maxcube      = hash_offset + size;
                float3 projected    = BoxProjectVector ( pos, ray, cubecenter, mincube, maxcube );

                //data for sampling the atlas
                float2 octnormal    = ( PackNormalToOct ( projected ) + 1) / 2;
                float2 samplepos    = ( octnormal + hash_id ) / cuberange;


                //cubemap result //.rg contain UV, .b is skymasking
                float4 raycubesample   = tex2D ( _Atlas, samplepos );
                //sample scene indirectlight with ray
                float4 indirectlight   = tex2D ( _Display, raycubesample.rg );
                //use uv to sample direct lightmap to get illumination
                float4 direct = tex2D   ( _LMdirect, raycubesample.rg );
                //sample skybox
                float4 sky = tex2D    ( _Skybox, octnormal ); sky = 1; //use the worldnormal to oct
                
                //mask sample by skybox mask //the sample would try to get uv invalid on LM
                // direct   *= raycubesample.b;
                
                //attenuate by distance //and sample cosine? //and fog (how)?
                //--
                //resolve sample BRDF //++++ take wnormal at sample
                //--                
                //mask skybox by skybox mask //mask part fromdirect light
                sky *= raycubesample.b;//return raycubesample.b;
                //we gonna use the ambient for now
                float wrapsky = wrappedTo1(float3(0,1,0), worldnorm);
                _Ambientsky *= raycubesample.b * wrapsky;
                sky += _Ambientsky;// ?

                //get direct light or skybox/skyo occlusion as RGB
                float4 irradiance   = sky + direct;
                irradiance *= cosineterm;
                //add indirectlight + irradiance
                irradiance += float4(YCoCgToRgb(indirectlight),1) ;
                
                
                //chromaLum
                float4 result = float4(RgbToYCoCg(irradiance),1);
                //divide lum by num ray
                result.x /= numRays;
                //turn the Y (lum) into 16bits
                float4 lum = Float32ToIntrgba(result.x);
                //encode split lum and chroma
                result = float4(lum.x,lum.y, result.y,result.z);

			    //return 16bit encoding //display material must reconstruct RGB from chroma lum
                return result;
            }
            ENDCG
        }
    }
}

//gi.rgb = traceResult.rgb;
                //gi.rgb *= 4.3;
                //gi.rgb += traceResult.a * 1.0 * SEGISkyColor;
                //float4 result = float4(gi.rgb, 2.0);

// float4 irradiance   = traceRay  (_LMdirect, _Skybox, raycubesample, octnormal,worldnorm );
