Shader "Custom/CloudShader"
{
    Properties
    {
        _CloudColor ("Cloud Color", Color) = (1,1,1,1)
        _SkyColor ("Sky Color", Color) = (0.5,0.7,1,1)
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 0.5
        _NoiseIterations ("Noise Iterations", Range(1, 8)) = 4
        _NoiseScale ("Noise Scale", Float) = 1.0
        _Threshold ("Threshold", Range(0,1)) = 0.5
        _Speed ("Animation Speed", Float) = 0.1
        _SkyTransparency ("Sky Transparency", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Shader properties
            float4 _CloudColor;
            float4 _SkyColor;
            float _EdgeSoftness;
            float _SkyTransparency;
            int _NoiseIterations;
            float _NoiseScale;
            float _Threshold;
            float _Speed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Hash function for noise
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            // 2D Noise function
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Fractal Brownian Motion
            float fbm(float2 p)
            {
                float total = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 5; i++)
                {
                    total += noise(p) * amplitude;
                    p *= 2.0;
                    amplitude *= 0.5;
                }
                return total;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Animate UVs over time
                float2 uv = i.uv * _NoiseScale;
                uv.x += _Time.y * _Speed;

                // Generate fractal noise
                float n = fbm(uv);

                // Apply threshold to create cloud shapes
                float clouds = smoothstep(_Threshold, _Threshold + 0.1, n);

                // Blend between sky and cloud colors
                _SkyColor.a *= _SkyTransparency;
                fixed4 color = lerp(_SkyColor, _CloudColor, clouds);


                // Output the final color
                return color;
            }
            ENDCG
        }
    }
}
