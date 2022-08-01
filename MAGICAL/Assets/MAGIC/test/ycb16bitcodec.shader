Shader "ycb16bit/ycb16bitcodec"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "../refactor/shader/ShaderTools.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 c = col;

                //covert to yco
                c = RgbToYCoCg(c);

                //show the y ie lum value
                // c = c.x;
                
                //set y to rgba
                float4 yco16 = Float32ToIntrgba(c.x);
                //set 16bits y back to float
                c.x = IntrgbaToFloat32(float4(yco16.xy,0,0));

                //set yco to RGB
                c = YCoCgToRgb(c);

                col = float4(c,1);

                return col;
                // return yco16;
            }
            ENDCG
        }
    }
}
