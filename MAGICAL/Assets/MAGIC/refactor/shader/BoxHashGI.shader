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

            struct meshData
            {
                float4 vertex	: POSITION; 
                fixed3 normal   : NORMAL;
                // float2 uv		: TEXCOORD1;
                // fixed4 color    : COLOR;
            };

            struct rasterData
            {
                float4 vertex	: POSITION;
                float4 wpos     : TEXCOORD1;
                fixed3 wnormals : NORMAL;
                // fixed4 color    : COLOR;
            };

            rasterData vertexProgram ( meshData input)//, out rasterData output )
            {
                rasterData output;
                output.vertex   = UnityObjectToClipPos ( input.vertex );    //screen position
                output.wpos     = mul ( unity_ObjectToWorld, input.vertex );//world position
                output.wnormals = UnityObjectToWorldNormal ( input.normal );//normal to world normal
                // output.color    = float4 ( input.uv, 0, 1 );
                return output;
            }
            
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
            sampler2D _Display;             //not used yet - used to read old value
            
            //lmgb
            sampler2D _Albedo;              //not used - see lmdirect
            sampler2D _Wnormal;
            sampler2D _Wposition;           //not used yet - possibly attenuation
            sampler2D _Shadowmask;          //not used yet - see lmdirect


            float4 traceRay ( float4 sample, float2 octdirection, float3 normalDirection ){
                //box sample cubemap to get uv
                //use uv to sample direct lightmap to get illumination
                float4 direct    = tex2D   ( _LMdirect, sample.rg );
                //mask sample by skybox mask //the sample would try to get uv invalid on LM
                       direct   *= 1 - sample.b;
                
                //attenuate by distance //and sample cosine? //and fog (how)?
                //--
                //resolve sample BRDF //++++ take wnormal at sample
                //--

                //sample skybox
                float4 sky       = tex2D    ( _Skybox, octdirection ); //use the worldnormal to oct
                //mask skybox by skybox mask //mask part fromdirect light
                       sky      *= sample.b;

                //we gonna use the ambient for now
                float wrapsky = wrappedTo1(float3(0,1,0), normalDirection);//invoke actual wrap normal
                _Ambientsky *= sample.b * wrapsky;
                sky = sky + _Ambientsky;// ?

                //return sample + skybox
                return sky + direct;
            }

            //mesh position to align with grid potentially in vertex clamping using the bounding box
            //pass the size and grid range, compute cell size
            fixed4 fragmentProcessing ( rasterData input ) : COLOR
            {
                //set size
                const float  size      = 4;//not const, expose as parameter
                const float2 cuberange = float2 ( 16, 16 );//not const, expose as parameter
                const float  epsilon   = 0.000001;

                float3 origin       = _Origin.xyz;
                float3 worldnorm    = normalize ( input.wnormals ) + epsilon;
                float3 pos          = input.wpos.xyz - origin + 0.001;//why the 0.001 again?

                //hash position to read the right cubemap in the atlas
                float3 hashpos      = floor ( pos / size ); 
                float3 hash_offset  = hashpos * size;
                float2 hash_id      = max ( float2 ( 0, 0 ), min ( hashpos.xz, cuberange ) );

                //current Ray direction, passed as paramater
                float3 ray = normalize ( _Kernel.rgb + worldnorm );
                if ( _Kernel.a == 0 )  { ray = float3( 0.0, 1.0, 0.0 ); }

                //box projection of world normal-> ray (why again?)
                float3 cubecenter   = hash_offset + ( size / 2 );
                float3 mincube      = hash_offset + 0;
                float3 maxcube      = hash_offset + size;
                float3 projected    = BoxProjectVector ( pos, ray, cubecenter, mincube, maxcube );
                
                //sampling the atlas
                float2 octnormal    = ( PackNormalToOct ( projected ) + 1) / 2;
                float2 samplepos    = ( octnormal + hash_id ) / cuberange;

                //cubemap result //.rg contain UV, .b is skymasking
                float4 cubesample   = tex2D     ( _Atlas, samplepos );
                float4 irradiance   = traceRay  ( cubesample, octnormal,worldnorm );

                //gi.rgb = traceResult.rgb;
                //gi.rgb *= 4.3;
                //gi.rgb += traceResult.a * 1.0 * SEGISkyColor;
                //float4 result = float4(gi.rgb, 2.0);

                //chromaLum
                irradiance = float4(RgbToYCbCr(irradiance),1);
                //divide by num ray
                irradiance /= 64;
                //turn the Y (lum) into 16bits
                float4 lum = Float32ToIntrgba(irradiance.x);
                //encode split lum and chroma
                irradiance = float4(lum.x,lum.y, irradiance.y,irradiance.z);
			    //sample accum
                float4 accum    = tex2D   ( _Display, cubesample.rg );
			    //accumulation (accum + sample accum) //display materal must reconstruct RGB from chroma lum
                irradiance += accum;
			    //return 16bit encoding
                return irradiance;
            }
            ENDCG
        }
    }
}