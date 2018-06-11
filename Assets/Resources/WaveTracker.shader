Shader "Custom/WaveTracker" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
//        _Param1 ("Wave Params 1", Vector) = (6.0, 2.0, 0.12, 0.0)
//        _Param2 ("Wave Params 2", Vector) = (12.0, 10.0, 0.03, 0.0)
        _Size ("Size", Float) = 50
        _NumCenters ("NumCenters", Int) = 100
        _RangeRed ("RangeRed", Float) = 2
        _RangeGreen ("RangeRed", Float) = 3
        _RangeBlue ("RangeRed", Float) = 5
        _Sound ("Sound", Float) = 1
        _Thickness ("Thickness", Float) = 0.15
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Cull Off
        
        CGPROGRAM
        #pragma surface surf Lambert alpha vertex:vert

        sampler2D _MainTex;
        half4 _Param1;
        half4 _Param2;
        float _Size;
        int _NumCenters;
        float3 _Centers[100];
        float _RangeRed;
        float _RangeGreen;
        float _RangeBlue;
        float _Sound;
        float _Thickness;
        float _Amplitudes[100];

        void vert (inout appdata_full v) {
            half time = _Time.y;
            //half u = v.texcoord.x;
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
//            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;

            half time = _Time.y;
//            float rad = _Radius / 2 + _Radius / 10 * sin(time);
//            float dN = 1 - saturate(d / rad);
            float sumC = 0;
            float sumDist = 0;
            for (int n = 0; n < _NumCenters; n++) {
                float size = _Size;
                float offset = 1;
                float dist = distance(_Centers[n], IN.worldPos);
                float v = _Amplitudes[n] * 100;
                time = 1; // live
                float c = sin(dist * size * v - time * _Sound);
                c = (c / 2) + 0.5;
                if (c > (1 - _Thickness)) {
                   c = 1;
                } else {
                   c = 0;
                }
                sumC += c;
                sumDist += dist; 
            }
            float cAve = sumC / _NumCenters;
            //
            float r = 0;
            float g = 0;
            float b = 0;
            float aveDist = sumDist / _NumCenters;
            if (aveDist < _RangeRed) {
                r = 0.75 - cAve;
                g = 0;
                b = 0;
            }
            if (aveDist > _RangeRed && aveDist < _RangeGreen) {
                r = 0;
                g = 0.75 - cAve;
                b = 0;
            }
            if (aveDist > _RangeGreen && aveDist < _RangeBlue) {
                r = 0;
                g = 0;
                b = 0.75 - cAve;
            }
            o.Albedo = half3(cAve + r, cAve + g, cAve + b);
            o.Alpha = 0.75;
        }
        ENDCG
    } 
    FallBack "Diffuse"
    CustomEditor "WavyAnimatedMaterialInspector"
}