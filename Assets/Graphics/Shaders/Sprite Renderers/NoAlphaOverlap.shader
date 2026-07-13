Shader "Custom/NoDarkeningMaxAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 200

        // Use a custom blend operation that takes the maximum alpha (and color).
        // Overlapping objects won’t darken or brighten;
        // alpha becomes max of all objects at that pixel.
        BlendOp Max         // Use Max operation for both color and alpha channels
        Blend One One       // Each channel is multiplied by 1 before taking Max

        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;  // If passing vertex colors
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the sprite texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Multiply by overall color tint and any vertex color
                fixed4 finalColor = texColor * _Color * i.color;
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}
