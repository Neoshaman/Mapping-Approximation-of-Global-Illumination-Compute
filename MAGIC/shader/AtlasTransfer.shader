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
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            
            samplerCUBE _Cube;

            float3 UnpackNormalFromOct(float2 f)
            {
                float3 n = float3(f.x, f.y, 1.0 - abs(f.x) - abs(f.y));
                float t = max(-n.z, 0.0);
                n.xy += n.xy >= 0.0 ? -t.xx : t.xx;
                return normalize(n);
            }

            fixed4 frag (v2f_customrendertexture i) : COLOR
            {
                float3 normal = UnpackNormalFromOct(i.localTexcoord.uv);
                //float3 normal = i.localTexcoord.xyz;
                return texCUBE(_Cube, normal);
            }
            ENDCG
        }
    }
}
