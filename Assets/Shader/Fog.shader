Shader "Custom/GradientCubeFogZ"
{
    Properties
    {
        _FrontColor("Front Color", Color) = (1,1,1,0.3) // Putih berkabut (agak transparan)
        _BackColor("Back Color", Color) = (0,0,0,1)     // Hitam pekat
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 localPos : TEXCOORD0;
            };

            fixed4 _FrontColor;
            fixed4 _BackColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = saturate(i.localPos.z); // gradasi dari depan ke belakang
                fixed4 col = lerp(_FrontColor, _BackColor, t);
                return col;
            }
            ENDCG
        }
    }
}
