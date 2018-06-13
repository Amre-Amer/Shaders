Shader "Custom/ShaderPhysicsTransparent" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200
        Cull Off
        
        CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert

        fixed4 _Color;
        sampler2D _MainTex;
        float3 _Centers[100];
        int _NumCenters;
        float _Range;

        void vert (inout appdata_full v) {
            half time = _Time.y;
            half u = v.texcoord.x;
        }

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 cTex4 = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            half3 cTex = cTex4.rgb;
            float distMin = -1;
            for (int n = 0; n < _NumCenters; n++) {
                half3 cValue = half3(0, 0, 0);
                float dist = distance(_Centers[n], IN.worldPos);
                if (n == 0 || dist < distMin) {
                    distMin = dist;
                }
            }
            if (distMin < _Range * .6) {
                float v = 1 - distMin / _Range; 
                o.Albedo  = half3(1, 1, 1) * v;
                o.Alpha = 1;
                o.Emission = o.Albedo;
            } else {
                o.Albedo = cTex;
                o.Alpha = cTex4.a;
            }
        }
        ENDCG
    } 
    FallBack "Diffuse"
}