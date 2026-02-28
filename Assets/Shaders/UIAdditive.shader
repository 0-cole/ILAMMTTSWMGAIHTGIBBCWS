Shader "UI/CircularVisualizer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Range(0,0.5)) = 0.2
        _OuterRadius ("Outer Radius", Range(0,0.5)) = 0.48
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One One // Additive: black = invisible

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _InnerRadius;
            float _OuterRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert UV to centered coordinates (-0.5 to 0.5)
                float2 centered = i.uv - 0.5;
                float dist = length(centered);
                float angle = atan2(centered.y, centered.x);

                // Map angle to horizontal UV (0-1)
                float u = (angle + 3.14159265) / (2.0 * 3.14159265);

                // Map distance to vertical UV (inner=bottom of video, outer=top)
                float v = saturate((dist - _InnerRadius) / (_OuterRadius - _InnerRadius));

                // Clip outside the ring
                float ringMask = step(_InnerRadius, dist) * step(dist, _OuterRadius);

                // Sample the video with polar coordinates (bars extend outward)
                fixed4 col = tex2D(_MainTex, float2(u, v));
                col *= i.color * ringMask;

                return col;
            }
            ENDCG
        }
    }
}
