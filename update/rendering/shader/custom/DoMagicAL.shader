Shader "MAGIC/DoMagicAL"
{
    Properties
    {
        //_albedo ("Texture", 2D) = "white" {}
        _w_normal ("World normal", 2D) = "white" {}
        _atlas ("UV atlas", 2D) = "white" {}
        _directLight ("Direct light", 2D) = "white" {}

        //_MainLight ("Main Light", Vector) = (1,1,1,1)    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "../MAGIC.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            sampler2D _w_normal;
            sampler2D _atlas;
            sampler2D _directLight;
            sampler2D _SelfTexture2D;
            
            fixed4 frag (v2f_customrendertexture i) : COLOR
            {
                //get sampling direction from normal texture +1 sample (can we do mesh?)
                float4 normal   = normalize( tex2D(_w_normal, i.uv) );
                //iteration to ray direction

                //sample cubemap ray uv adress +1 sample
                float4 rayhit   = tex2D(_atlas, i.uv); //hash from position texture, sample from ray
                
                //Sample direct light map      +1 sample
                float4 Dlight   = tex2D(_directLight, rayhit.xy);
                
                //--sample farfield either direct or uv to lod lightmap +1 sample (can we do sh?)
                //float4 Far      = tex2D(_far, ray.xy);
                
                //blend with previous data +1 sample
                float4 previous = tex2D(_SelfTexture2D, i.uv);
                //_______________________________________5samples remain 3samples -- 3 remain 5
                //output accumulation
                return 1;//col;
            }
            ENDCG
        }
    }
}
