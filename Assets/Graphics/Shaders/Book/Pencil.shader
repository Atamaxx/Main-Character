Shader "Custom/PostRendering/PencilStatic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradThresh ("Gradient Threshold", Range(0.000001, 0.01)) = 0.01
        _ColorThreshold ("Color Threshold", Range(0.0, 1.0)) = 0.5
        _Scale ("Sampling Scale", Float) = 1.0 // controls how far we step in UV space
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float _GradThresh;
            float _ColorThreshold;
            float _Scale;

            // Vertex data structure with UVs
            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            // Varying data to the fragment shader
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // ----------------------------------------------------------------------------
            // Vertex Shader
            // ----------------------------------------------------------------------------
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord; // pass the mesh UVs straight through
                return o;
            }

            // ----------------------------------------------------------------------------
            // Helpers and constants
            // ----------------------------------------------------------------------------
            #define PI2 6.28318530717959
            #define STEP 0.01       // step size in UV space
            #define RANGE 0.2       // total sample range in UV space
            #define ANGLENUM 4      // how many directions we sample
            #define SENSITIVITY 10.0

            float4 getCol(float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            float getVal(float2 uv)
            {
                // Grayscale (luma) value
                float4 c = getCol(uv);
                return dot(c.rgb, float3(0.2126, 0.7152, 0.0722));
            }

            // Approximates a gradient using a small delta in UV space
            float2 getGrad(float2 uv, float delta)
            {
                float2 d = float2(delta, 0.0);

                float gx = getVal(uv + d.xy) - getVal(uv - d.xy);
                float gy = getVal(uv + d.yx) - getVal(uv - d.yx);

                // Scale by (2 * delta) so that gradient is in [–1, 1]
                return float2(gx, gy) / (2.0 * delta);
            }

            // Simple 2D rotation
            void pR(inout float2 p, float a)
            {
                p = float2(
                    p.x * cos(a) - p.y * sin(a),
                    p.x * sin(a) + p.y * cos(a)
                );
            }

            // ----------------------------------------------------------------------------
            // Fragment Shader
            // ----------------------------------------------------------------------------
            fixed4 frag(v2f i) : SV_Target
            {
                // UV space from the mesh
                float2 uv = i.uv;

                // This "weight" goes from 1.0 down to 0.0 based on detected edges
                float weight = 1.0;

                // Check multiple angles
                for (int j = 0; j < ANGLENUM; j++)
                {
                    // Direction in UV space
                    float2 dir = float2(1.0, 0.0);
                    // Rotate dir by an increment of 2π/(2 * ANGLENUM)
                    pR(dir, j * PI2 / (2.0 * ANGLENUM));

                    // The gradient direction is perpendicular
                    float2 gradDir = float2(-dir.y, dir.x);

                    // Sweep along this direction
                    for (float stepPos = -RANGE; stepPos <= RANGE; stepPos += STEP)
                    {
                        float2 sampleUV = uv + dir * (stepPos * _Scale);

                        // Skip if out of [0,1] range
                        if (sampleUV.x < 0.0 || sampleUV.x > 1.0 ||
                            sampleUV.y < 0.0 || sampleUV.y > 1.0)
                            continue;

                        // Compute gradient magnitude
                        float2 g = getGrad(sampleUV, 0.002 * _Scale);
                        if (length(g) < _GradThresh)
                            continue;

                        // Reduce the weight if there is a strong gradient along gradDir
                        weight -= pow(abs(dot(normalize(gradDir), normalize(g))), SENSITIVITY)
                                  / ( ((RANGE * 2.0) / STEP) + 1.0 ) 
                                  / ANGLENUM;
                    }
                }

                // Base color
                float4 col = getCol(uv);

                // “Paper” color
                float4 paperColor = float4(1.0, 1.0, 1.0, 1.0);

                // Create the background mix
                float4 background = lerp(col, paperColor, _ColorThreshold);

                // Dark pencil vs. background
                float4 finalCol = lerp(float4(0.0, 0.0, 0.0, 1.0), background, weight);

                return finalCol;
            }
            ENDCG
        }
    }
}
