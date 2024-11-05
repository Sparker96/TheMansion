Shader "Custom/DarknessShader"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)
        _PlayerPos ("Player Position", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Float) = 0.2
        _Smoothness ("Edge Smoothness", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Enable transparency
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            float4 _Color;
            float4 _PlayerPos;
            float _Radius;
            float _Smoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the distance between each pixel and the player position
                float2 uv = i.uv;
                float dist = distance(uv, _PlayerPos.xy);

                // Calculate alpha based on distance, making the circle transparent
                float alpha = smoothstep(_Radius, _Radius + _Smoothness, dist);

                // Set the color to black with the calculated alpha transparency
                return fixed4(_Color.rgb, alpha);
            }
            ENDCG
        }
    }
}
