Shader "MAGIC/LightBoxOcclusionDirectional"
{
    Properties
    {
        _albedo ("Texture", 2D) = "white" {}
        _w_normal ("Texture", 2D) = "white" {}
        _w_position ("Texture", 2D) = "white" {}
        _shadowMask ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #include "../MAGICA.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag

            sampler2D _albedo;      //RGB
            float4 _albedo_ST;
            
            sampler2D _w_normal;    //RG: keep it as octohedron 
            float4 _w_normal_ST;
            
            sampler2D _w_position; //RGB: position, A: shadowMask?
            float4 _w_position_ST;
            
            sampler2D _shadowMask;  //merge with w_position
            float4 _shadowMask_ST;

           //included magic oct to normal

            //cubemap atlas, skybox/farfield, light direction and color, box position and size (infered)

           //hash pixel to cubemap id
           //for each directional light
           //--boxproject direction to pixel position
           //--sample cubemap with direction
           //--if skybox : light directional + sample skybox
           //--else occluded (shadow color)
           //ouput result

            fixed4 frag (v2f_customrendertexture i) : COLOR
            {
                // sample the texture
                float4 col = tex2D(_albedo, i.globalTexcoord.xy);

                // compute normal lighting
                //(probably passing light data in a loop)
                //maybe move light out completely to accumulate over time in script
                float4 oct = tex2D(_w_normal, i.globalTexcoord.xy);
                float3 normal = UnpackNormalFromOct(oct.xy);
                //get light dir 
                //float ndotl = saturate(dot(lightdir, normal))


                //compute shadowmasking
                //(directly or probably sampling another custom)

                //final gathering
                //col = col * (min (ndotl, shadowmask);

                return col;
            }
            ENDCG
        }
    }
}
