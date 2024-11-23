Shader "Unlit/ScreenspaceTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Enable fog support
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;   // Object space position
                float2 uv : TEXCOORD0;     // UV coordinates
            };

            struct v2f
            {
                float3 screenPos : TEXCOORD0; // Screen space position (xy/w for UV, w for depth)
                UNITY_FOG_COORDS(1)         // Fog coordinates
                float4 vertex : SV_POSITION; // Clip space position
            };

            sampler2D _MainTex;  // Texture sampler
            float4 _MainTex_ST; // Texture transform (scale, offset)

            v2f vert (appdata v)
            {
                v2f o;

                // Transform to clip space
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Compute screen-space position (xy for UV mapping, w for depth)
                o.screenPos = o.vertex.xyw;

                // Adjust for OpenGL platforms (inverted Y axis in screen space)
                #ifdef UNITY_UV_STARTS_AT_TOP
                o.screenPos.y *= -1.0f;
                #endif

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate screen-space UV coordinates
                float2 screenUV = (i.screenPos.xy / i.screenPos.z) * 0.5f + 0.5f;

                // Sample the texture
                fixed4 col = tex2D(_MainTex, screenUV);

                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}