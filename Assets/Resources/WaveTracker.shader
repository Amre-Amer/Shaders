Shader "Custom/WaveTracker" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
//        _Param1 ("Wave Params 1", Vector) = (6.0, 2.0, 0.12, 0.0)
//        _Param2 ("Wave Params 2", Vector) = (12.0, 10.0, 0.03, 0.0)
        _Size ("Size", Float) = 50
        _NumCenters ("NumCenters", Int) = 100
        _RangeRed ("RangeRed", Float) = 2
        _RangeGreen ("RangeRed", Float) = 3
        _RangeBlue ("RangeRed", Float) = 5
        _Thickness ("Thickness", Float) = 0.15
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Cull Off
        
        CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert

        fixed4 _Color;
        sampler2D _MainTex;
        half4 _Param1;
        half4 _Param2;
        float _Size;
        int _NumCenters;
        float3 _Centers[100];
        float _RangeRed;
        float _RangeGreen;
        float _RangeBlue;
        float _Thickness;
        float _Amplitudes[100];

        void vert (inout appdata_full v) {
            half time = _Time.y;
            half u = v.texcoord.x;
            //half w1 = sin(time * _Param1.x - u * _Param1.y) * _Param1.z;
            //half w2 = sin(time * _Param2.x - u * _Param2.y) * _Param2.z;
            //v.vertex.xyz += v.normal * (w1 + w2) * u;
            //v.vertex.xyz += v.normal * _Radius;
        }

        struct Input {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 cTex4 = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            half3 cTex = cTex4.rgb;
//            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;

//            half time = _Time.y;
//            float rad = _Radius / 2 + _Radius / 10 * sin(time);
//            float dN = 1 - saturate(d / rad);
            float sumValue = 0;
            float sumDist = 0;
            float minDist = -1; 
            for (int n = 0; n < _NumCenters; n++) {
                float dist = distance(_Centers[n], IN.worldPos);
                if (n == 0 || dist < minDist) {
                    minDist = dist;
                }
                float amp = _Amplitudes[n];
                float ang = dist * amp / _Size;
                float v = sin(ang);
                v = (v / 2) + 0.5;  // saturate?
                if (v > (1 - _Thickness)) {
                   v = 1;
                } else {
                   v = 0;
                }
                sumValue += v;
                sumDist += dist; 
            }
            //
            float aveValue = sumValue / _NumCenters;
            half3 cValue  = half3(aveValue, aveValue, aveValue);
            //
            float r = 0;
            float g = 0;
            float b = 0;
            float dist = sumDist / _NumCenters;
            dist = minDist;
            if (dist < _RangeRed) {
                r = 1;
                g = 0;
                b = 0;
            }
            if (dist > _RangeRed && dist < _RangeGreen) {
                r = 0;
                g = 1;
                b = 0;
            }
            if (dist > _RangeGreen && dist < _RangeBlue) {
                r = 0;
                g = 0;
                b = 1;
            }
            half3 cRange = half3(r, g, b);
            float cAve = (cValue + cTex + cRange) / 3;
//            cAve = sumC;
            half3 cResult = cTex; // cRange;
            if (sumValue == 1 || _Thickness >= 0.5) {
                cResult = (cTex + cRange) / 2;
            }
//            o.Albedo = half3(cAve, cAve, cAve);
            o.Albedo = cResult;
            o.Alpha = 1;
        }
        ENDCG
    } 
    FallBack "Diffuse"
    CustomEditor "WavyAnimatedMaterialInspector"
}