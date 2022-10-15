Shader "MAGIC/debugtest"
{
   
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vertexProgram
            #pragma fragment fragmentProcessing
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            
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
                //output.vertex = unwrap(input.uv, input.vertex.w);      //screen position
                output.vertex   = UnityObjectToClipPos ( input.vertex );    //screen position

                 output.wpos     = mul ( unity_ObjectToWorld, input.vertex );//world position
                //output.wpos     = mul ( _PosMat, input.vertex );//world position

                output.wnormals = UnityObjectToWorldNormal ( input.normal );//normal to world normal
                // output.color    = float4 ( input.uv, 0, 1 );
                return output;
            }

           

            fixed4 fragmentProcessing ( rasterData input ) : COLOR
            {
                
                float3 worldnorm    = normalize ( input.wnormals );
                //+ epsilon; worldnorm = worldnorm.xzy; worldnorm.x = - worldnorm.x;
                //float3 pos          = input.wpos.xyz;// - origin + 0.001;//why the 0.001 again?
                float4 result = float4(worldnorm,1);
              
                return result;
            }
            ENDCG
        }
    }
}
