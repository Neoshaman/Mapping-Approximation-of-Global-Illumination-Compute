Shader "MAGIC/AtlasTransfer"
{
    Properties
    {
        _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader
    {
        Pass
        {
        	Cull Off
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "../MAGIC.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            
            samplerCUBE _Cube;

            fixed4 frag (v2f_customrendertexture i) : COLOR
            {
                float2 g = i.localTexcoord.xy;
                float3 normal = UnpackNormalFromOct(g);
                //float3 normal = i.localTexcoord.xyz;
                //return fixed4(normal,1);//texCUBE(_Cube, normal);
                return fixed4(1,0,0,1);//texCUBE(_Cube, normal);
            }
            
            ENDCG
        }
    }
}
