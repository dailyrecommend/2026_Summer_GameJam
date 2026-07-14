// URP 풀스크린 포스트 이펙트: 픽셀화 + CRT(곡률/스캔라인/비네트/색수차).
// URP의 "Full Screen Pass Renderer Feature"에 이 셰이더로 만든 머티리얼을 연결해서 사용.
// Blit.hlsl에 의존하지 않고(버전별 경로 이슈 회피) 풀스크린 삼각형을 직접 그린다.
Shader "CardBoardWar/CRTPixelFullscreen"
{
    Properties
    {
        _PixelResolution ("Pixel Resolution (vertical, 0=off)", Float) = 180
        _Curvature ("CRT Curvature", Range(0,0.5)) = 0.15
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.3
        _ScanlineCount ("Scanline Count", Float) = 240
        _Vignette ("Vignette", Range(0,2)) = 0.6
        _Aberration ("Chromatic Aberration", Range(0,0.01)) = 0.002
        [HDR] _TintColor ("Tint Color", Color) = (1,1,1,1)
        _TintStrength ("Tint Strength", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "CRTPixel"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Full Screen Pass Renderer Feature가 소스 화면을 _BlitTexture로 바인딩한다.
            // (sampler_LinearClamp는 Core.hlsl에 이미 선언돼 있으므로 다시 선언하지 않는다)
            TEXTURE2D_X(_BlitTexture);

            float _PixelResolution;
            float _Curvature;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _Vignette;
            float _Aberration;
            half4 _TintColor;
            float _TintStrength;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings   { float4 positionCS : SV_POSITION; float2 texcoord : TEXCOORD0; };

            Varyings Vert(Attributes IN)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                o.texcoord   = GetFullScreenTriangleTexCoord(IN.vertexID);
                return o;
            }

            // 배럴 왜곡(CRT 곡률)
            float2 CurveUV(float2 uv)
            {
                float2 dc = uv - 0.5;
                uv += dc * dot(dc, dc) * _Curvature * 4.0;
                return uv;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // 1) CRT 곡률
                uv = CurveUV(uv);

                // 2) 픽셀화: 세로 _PixelResolution 픽셀로 다운스케일(가로는 종횡비 유지)
                if (_PixelResolution > 0.0)
                {
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    float2 res = float2(floor(_PixelResolution * aspect), _PixelResolution);
                    uv = (floor(uv * res) + 0.5) / res;
                }

                // 곡률로 화면 밖으로 밀려난 부분은 검게(CRT 테두리)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0, 0, 0, 1);

                // 3) 색수차(RGB를 살짝 어긋나게 샘플)
                float2 off = float2(_Aberration, 0);
                half3 col;
                col.r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + off).r;
                col.g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                col.b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - off).b;

                // 4) 스캔라인
                float scan = 0.5 + 0.5 * sin(uv.y * _ScanlineCount * 6.2831853);
                col *= lerp(1.0, scan, _ScanlineIntensity);

                // 5) 비네트
                float2 vd = uv - 0.5;
                col *= saturate(1.0 - dot(vd, vd) * _Vignette);

                // 6) 색 틴트(원하는 색을 세기만큼 곱함)
                col *= lerp(half3(1, 1, 1), _TintColor.rgb, _TintStrength);

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
