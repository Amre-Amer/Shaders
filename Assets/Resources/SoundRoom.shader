Shader "Custom/SoundRoom" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags {"RenderType"="Opaque" }
        LOD 200
        Cull Off
        
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        fixed4 _Color;
        sampler2D _MainTex;
        float _Size;
        float3 _Center;
        float _Thickness;
        float _Amplitude;
        int _YnLines;
        int _CntFrames;
        float _Yaw;

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
            half3 cValue = half3(0, 0, 0);
            //
            float dist = distance(_Center, IN.worldPos);
            if (_YnLines == 1) {
                float amp = _Amplitude;
                float ang = dist * amp / _Size; 
                float v = sin(ang);
                v = (v / 2) + 0.5;  // saturate?
                if (v > (1 - _Thickness)) {
                    v = 1;
                } else {
                    v = 0;
                }
                cValue  = half3(v, v, v);
            } else {
                float thick = 1;
                float dx =  IN.worldPos.x - _Center.x;
                if (dx >=0 && dx < thick) {
                    cValue  = half3(1, 0, 0);
                }
                float dy = IN.worldPos.y - _Center.y;
                if (dy >=0 && dy < thick) {
                    cValue  = half3(0, 1, 0);
                }
                float dz = IN.worldPos.z - _Center.z;
                if (dz >=0 && dz < thick) {
                    cValue  = half3(0, 0, 1);
                }
                float yawScan = _Yaw;
                if (yawScan < 0) yawScan += 360;
                float yawPix = atan2(dz, dx) * 57.29;
                if (yawPix < 0) yawPix += 360;
            }
            if (dist < 10) {
                cValue  = half3(1, 1, 0);
                cTex  = half3(1, 1, 0);
            }
            float cAve = (cValue + cTex) / 2;
            half3 cResult = (cTex + cValue) / 2;
            o.Albedo = cResult;
            o.Alpha = 1;
        }
        ENDCG
    } 
    FallBack "Diffuse"
    CustomEditor "WavyAnimatedMaterialInspector"
}