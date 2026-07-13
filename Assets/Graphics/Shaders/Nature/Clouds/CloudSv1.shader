Shader "Custom/DeepSkySpriteShaderEnhanced"
{
    Properties
    {
        // Sky Gradient
        _SkyTopColor ("Sky Top Color", Color) = (0.0, 0.5, 1.0, 1)
        _SkyBottomColor ("Sky Bottom Color", Color) = (0.5, 0.7, 1.0, 1)
        _HorizonOffset ("Horizon Offset", Range(-1, 1)) = 0.0

        // Cloud Layers
        _TileSize1 ("Cloud Tile Size Layer 1", Float) = 5.0
        _TileSize2 ("Cloud Tile Size Layer 2", Float) = 10.0
        _TileSize3 ("Cloud Tile Size Layer 3", Float) = 20.0

        _Speed1 ("Cloud Speed Layer 1", Float) = 0.1
        _Speed2 ("Cloud Speed Layer 2", Float) = 0.05
        _Speed3 ("Cloud Speed Layer 3", Float) = 0.02

        _CloudColor1 ("Cloud Color Layer 1", Color) = (1,1,1,0.5)
        _CloudColor2 ("Cloud Color Layer 2", Color) = (1,1,1,0.3)
        _CloudColor3 ("Cloud Color Layer 3", Color) = (1,1,1,0.1)

        _CloudDensity ("Cloud Density", Range(0.0, 1.0)) = 0.5
        _CloudRandomness ("Cloud Randomness", Float) = 1.0

        // Stars
        _StarsIntensity ("Stars Intensity", Range(0, 1)) = 1.0
        _StarsSpeed ("Stars Speed", Float) = 0.0
        _StarsDensity ("Stars Density", Range(0.0, 1.0)) = 0.5
        _StarSize ("Star Size", Float) = 0.001
        _StarColor ("Star Color", Color) = (1,1,1,1)

        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            // Shader properties
            fixed4 _SkyTopColor;
            fixed4 _SkyBottomColor;
            float _HorizonOffset;

            float _TileSize1;
            float _TileSize2;
            float _TileSize3;

            float _Speed1;
            float _Speed2;
            float _Speed3;

            fixed4 _CloudColor1;
            fixed4 _CloudColor2;
            fixed4 _CloudColor3;

            float _CloudDensity;
            float _CloudRandomness;

            float _StarsIntensity;
            float _StarsSpeed;
            float _StarsDensity;
            float _StarSize;
            fixed4 _StarColor;

            float _PixelSnap;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv      : TEXCOORD0;
                float4 vertex  : SV_POSITION;
            };

            // Custom hash function
            float hash(float2 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * (p.x + p.y));
            }

            // Noise function
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // Fractal Brownian Motion
            float fbm(float2 p)
            {
                float total = 0.0;
                float amplitude = 0.5;

                for (int i = 0; i < 5; i++)
                {
                    total += noise(p) * amplitude;
                    p = p * 2.0 + float2(10.0 * _CloudRandomness, 10.0 * _CloudRandomness);
                    amplitude *= 0.5;
                }
                return total;
            }

            // Improved Stars Function
            float starField(float2 uv)
            {
                // Scale UVs for density
                uv *= 1000.0 * _StarsDensity;

                // Calculate grid cell
                float2 cell = floor(uv);

                // Get random value per cell
                float rnd = hash(cell);

                // Determine if star is present in this cell
                float starPresence = step(1.0 - _StarsDensity, rnd);

                // Calculate star position within cell
                float2 starPos = cell + float2(hash(cell + 1.0), hash(cell + 2.0));

                // Calculate distance from current UV to star position
                float2 offset = uv - starPos;
                float dist = length(offset);

                // Calculate star brightness based on distance
                float star = smoothstep(_StarSize * 1.0, 0.0, dist);

                // Apply star presence and intensity
                return star * starPresence * _StarsIntensity;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Compute vertical gradient for sky color
                float gradient = saturate(uv.y + _HorizonOffset);
                fixed4 skyColor = lerp(_SkyBottomColor, _SkyTopColor, gradient);

                // Adjust cloud density
                float cloudThreshold = lerp(0.5, 0.8, _CloudDensity);

                // Layer 1 Clouds
                float2 uv1 = uv * _TileSize1 + float2(_Time.y * _Speed1, 0);
                float clouds1 = fbm(uv1);
                clouds1 = smoothstep(cloudThreshold, cloudThreshold + 0.1, clouds1);
                fixed4 cloudColor1 = _CloudColor1 * clouds1;

                // Layer 2 Clouds
                float2 uv2 = uv * _TileSize2 + float2(_Time.y * _Speed2, 0);
                float clouds2 = fbm(uv2);
                clouds2 = smoothstep(cloudThreshold, cloudThreshold + 0.1, clouds2);
                fixed4 cloudColor2 = _CloudColor2 * clouds2;

                // Layer 3 Clouds
                float2 uv3 = uv * _TileSize3 + float2(_Time.y * _Speed3, 0);
                float clouds3 = fbm(uv3);
                clouds3 = smoothstep(cloudThreshold, cloudThreshold + 0.1, clouds3);
                fixed4 cloudColor3 = _CloudColor3 * clouds3;

                // Combine cloud layers
                fixed4 combinedClouds = cloudColor1 + cloudColor2 + cloudColor3;
                combinedClouds.a = saturate(combinedClouds.a);

                // Improved Stars Layer
                float starValue = starField(uv + _Time.y * _StarsSpeed);
                fixed4 starsColor = _StarColor * starValue;

                // Final Color
                fixed4 finalColor = skyColor + combinedClouds;
                finalColor.rgb = lerp(finalColor.rgb, starsColor.rgb + finalColor.rgb, starsColor.a);
                finalColor.a = saturate(finalColor.a);

                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
