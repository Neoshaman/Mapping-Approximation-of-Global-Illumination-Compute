Shader "MAGIC/LightLMGB"
{
    Properties
    {
        _albedo ("Albedo", 2D) = "white" {}
        _w_normal ("normal", 2D) = "white" {}
        _w_position ("position", 2D) = "white" {}
        _shadowMask ("shadow", 2D) = "white" {}
        _lightAccu ("light accumulation", 2D) = "white" {}
        _MainLight ("Main Light", Vector) = (1,1,1,1)
    }
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

            sampler2D _albedo;      //RGB
            float4 _albedo_ST;
            
            sampler2D _w_normal;    //RG: keep it as octohedron 
            float4 _w_normal_ST;
            
            sampler2D _w_position;  //RGB: position, A: shadowMask?
            float4 _w_position_ST;
            
            sampler2D _shadowMask;  //merge with w_position
            float4 _shadowMask_ST;
            
            float4    _MainLight;
            //sampler2d _SelfTexture2D;

            fixed4 frag (v2f_customrendertexture i) : COLOR
            {
                // sample the albedo texture
                float4 col = tex2D(_albedo, i.globalTexcoord.xy);

                // compute normal lighting
                float4 oct = tex2D(_w_normal, i.globalTexcoord.xy);
                float3 normal = oct;// UnpackNormalFromOct(oct.xy);               
                _MainLight = normalize(_MainLight);//another shader for each light type
                float ndotl = saturate(dot(_MainLight.xyz, normal));


                //compute shadowmasking
                //(directly or probably sampling another custom)

                //final gathering
                //col =  col * (min (ndotl, shadowmask);
                //col += _lightAccu;

                return ndotl;
            }
            ENDCG
        }
    }
}
