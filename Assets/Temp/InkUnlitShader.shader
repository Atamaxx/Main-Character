Shader "Custom/InkUnlitShader" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}            // Your sprite texture.
        _PaperTex ("Paper Texture", 2D) = "white" {}           // An optional paper overlay texture.
        _Ramp ("Color Ramp", 2D) = "white" {}                  // A horizontal gradient texture that defines your toon colors.
        _Steps ("Quantization Steps", Range(2, 8)) = 4         // How many discrete brightness bands.
        _PaperIntensity ("Paper Overlay Intensity", Range(0,1)) = 0.5  // How strongly to blend the paper texture.
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass {
            Tags { "LightMode"="Always" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            // Define target shader model and entry points.
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Property declarations: textures and parameters.
            sampler2D _MainTex;
            sampler2D _PaperTex;
            sampler2D _Ramp;
            float _Steps;
            float _PaperIntensity;

            // Structure for input vertex data.
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            // Structure for passing data from vertex to fragment shader.
            struct v2f {
                float2 uv : TEXCOORD0;
                float2 uvPaper : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            // Vertex function: compute clip space position and pass through UV coordinates.
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uvPaper = v.uv;  // Using the same UV; you can offset/scaling as desired.
                return o;
            }

            // Helper function to compute brightness (luminance) from a color.
            float luminance(float3 color) {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            // Fragment function: computes the final stylized color.
            fixed4 frag (v2f i) : SV_Target {
                // Sample the main texture.
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Compute the luminance (brightness) of the sampled color.
                float lum = luminance(col.rgb);

                // Quantize the brightness into discrete bands.
                float q = floor(lum * _Steps) / _Steps;

                // Use the quantized value to sample the color ramp texture.
                // (Assumes the ramp is a horizontal gradient; we use "q" as the x-coordinate and 0.5 for y.)
                fixed4 rampCol = tex2D(_Ramp, float2(q, 0.5));

                // Optionally, you can mix the ramp color with the original color.
                // For a pure toon effect, we use the ramp color directly.
                fixed4 stylized = rampCol;

                // Sample the paper texture.
                fixed4 paper = tex2D(_PaperTex, i.uvPaper);

                // Blend the paper texture with the toon-stylized color.
                // Here, lerp() interpolates from the stylized color to (stylized * paper)
                // based on the intensity parameter.
                fixed4 finalColor = lerp(stylized, stylized * paper, _PaperIntensity);

                // Carry over the original alpha (transparency) from the main texture.
                finalColor.a = col.a;
                return finalColor;
            }
            ENDCG
        }
    }
}
