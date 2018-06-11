Shader "Custom/SoundRoom" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Size ("Size", Float) = 50
        _Thickness ("Thickness", Float) = 0.15
        _Amplitude ("Amplitude", Float) = 0.5
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Cull Off
        
        CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert

        fixed4 _Color;
        sampler2D _MainTex;
        float _Size;
        float3 _Center;
        float _Thickness;
        float _Amplitude;

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
            //
                float dist = distance(_Center, IN.worldPos);
                float amp = _Amplitude;
                //_Size = 1;
                float ang = dist * amp / _Size; //dist; // * amp / _Size;
                float v = sin(ang);
                v = (v / 2) + 0.5;  // saturate?
                if (v > (1 - _Thickness)) {
                   v = 1;
                } else {
                   v = 0;
                }
            //
            half3 cValue  = half3(v, v, v);
            //
            float cAve = (cValue + cTex) / 2;
            half3 cResult = cAve;
            //cResult = cValue;
//            if (sumValue == 1 || _Thickness >= 0.5) {
//                cResult = (cTex + cRange) / 2;
//            }
            o.Albedo = cResult;
            o.Alpha = 1;
        }
        ENDCG
    } 
    FallBack "Diffuse"
    CustomEditor "WavyAnimatedMaterialInspector"
}