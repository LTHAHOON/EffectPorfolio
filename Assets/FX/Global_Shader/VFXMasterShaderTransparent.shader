Shader "VFX/VFXMasterShaderTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradientMap("Gradient map", 2D) = "white" {} 
        [HDR]_Color("Color", Color) = (1,1,1,1)

        //Secondary texture
        [Space(20)]
        [Toggle(SECONDARY_TEX)]
        _SecondTex("Second texture", float) = 0
        _SecondaryTex("Secondary texture", 2D) = "white" {}
        _SecondaryPanningSpeed("Secondary panning speed", Vector) = (0,0,0,0)

        _PanningSpeed("Panning speed (XY main texture - ZW displacement texture)", Vector) = (0,0,0,0)
        _Contrast("Contrast", float) = 1
        _Power("Power", float) = 1

        //Clipping
        [Space(20)]
        _Cutoff("Cutoff", Range(0, 1)) = 0
        _CutoffSoftness("Cutoff softness", Range(0, 1)) = 0
        [HDR]_BurnCol("Burn color", Color) = (1,1,1,1)
        _BurnSize("Burn size", float) = 0

        //Softness
        [Space(20)]
        [Toggle(SOFT_BLEND)]
        _SoftBlend("Soft blending", float) = 0
        _IntersectionThresholdMax("Intersection Threshold Max", float) = 1

        //Vertex offset
        [Space(20)]
        [Toggle(VERTEX_OFFSET)]
        _VertexOffset("Vertex offset", float) = 0
        _VertexOffsetAmount("Vertex offset amount", float) = 0

        //Displacement
        [Space(20)]
        _DisplacementAmount("Displacement", float) = 0
        _DisplacementGuide("DisplacementGuide", 2D) = "white" {}

        //Culling
        [Space(20)]
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2

        //Banding
        [Space(20)]
        [Toggle(BANDING)]
        _Banding("Color banding", float) = 0
        _Bands("Number of bands", float) = 3

        //Polar coordinates
        [Space(20)]
        [Toggle(POLAR)]
        _PolarCoords("Polar coordinates", float) = 0

        //Circle mask
        [Space(20)]
        [Toggle(CIRCLE_MASK)]
        _CircleMask("Circle mask", float) = 0
        _OuterRadius("Outer radius", Range(0,1)) = 0.5
        _InnerRadius("Inner radius", Range(-1,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.2

        //Rect mask
        [Space(20)]
        [Toggle(RECT_MASK)]
        _RectMask("Rectangle mask", float) = 0
        _RectWidth("Rectangle width", float) = 0
        _RectHeight("Rectangle height", float) = 0
        _RectMaskCutoff("Rectangle mask cutoff", Range(0,1)) = 0
        _RectSmoothness("Rectangle mask smoothness", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset -1, -1
        Cull [_Culling]
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local SECONDARY_TEX
            #pragma shader_feature_local VERTEX_OFFSET
            #pragma shader_feature_local SOFT_BLEND
            #pragma shader_feature_local BANDING
            #pragma shader_feature_local POLAR
            #pragma shader_feature_local CIRCLE_MASK
            #pragma shader_feature_local RECT_MASK
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #if defined(SOFT_BLEND)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv        : TEXCOORD0;
                float4 color     : COLOR;
                float3 normalOS  : NORMAL;
            };

            struct Varyings
            {
                float2 uv          : TEXCOORD0;
                float2 displUV     : TEXCOORD1;
                float2 secondaryUV : TEXCOORD2;
                float4 scrPos      : TEXCOORD3;
                float  fogCoord    : TEXCOORD4;
                float4 positionCS  : SV_POSITION;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);          SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecondaryTex);     SAMPLER(sampler_SecondaryTex);
            TEXTURE2D(_GradientMap);      SAMPLER(sampler_GradientMap);
            TEXTURE2D(_DisplacementGuide);SAMPLER(sampler_DisplacementGuide);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SecondaryTex_ST;
                float4 _DisplacementGuide_ST;

                float4 _Color;
                float4 _BurnCol;

                float _Contrast;
                float _Power;

                float _Bands;

                float4 _PanningSpeed;
                float4 _SecondaryPanningSpeed;

                float _Cutoff;
                float _CutoffSoftness;
                float _BurnSize;

                float _IntersectionThresholdMax;

                float _VertexOffsetAmount;

                float _DisplacementAmount;

                float _Smoothness;
                float _OuterRadius;
                float _InnerRadius;

                float _RectSmoothness;
                float _RectHeight;
                float _RectWidth;
                float _RectMaskCutoff;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o = (Varyings)0;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.secondaryUV = TRANSFORM_TEX(v.uv, _SecondaryTex);

                #ifdef VERTEX_OFFSET
                float vertOffset = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, o.uv + _Time.y * _PanningSpeed.xy, 0).x;
                #ifdef SECONDARY_TEX
                float secondTex = SAMPLE_TEXTURE2D_LOD(_SecondaryTex, sampler_SecondaryTex, o.secondaryUV + _Time.y * _SecondaryPanningSpeed.xy, 0).x;
                vertOffset = vertOffset * secondTex * 2;
                #endif
                vertOffset = ((vertOffset * 2) - 1) * _VertexOffsetAmount;
                v.positionOS.xyz += vertOffset * v.normalOS;
                #endif

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = vertexInput.positionCS;
                o.displUV = TRANSFORM_TEX(v.uv, _DisplacementGuide);
                o.scrPos = ComputeScreenPos(o.positionCS);
                o.color = v.color;
                o.fogCoord = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // sample the texture
                float2 uv = i.uv;
                float2 displUV = i.displUV;
                float2 secondaryUV = i.secondaryUV;

                //Polar coords
                #ifdef POLAR
                float2 mappedUV = (i.uv * 2) - 1;
                uv = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.displUV * 2) - 1;
                displUV = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.secondaryUV * 2) - 1;
                secondaryUV = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                #endif

                //UV Panning
                uv += _Time.y * _PanningSpeed.xy;
                displUV += _Time.y * _PanningSpeed.zw;
                secondaryUV += _Time.y * _SecondaryPanningSpeed.xy;

                //Displacement
                float2 displ = SAMPLE_TEXTURE2D(_DisplacementGuide, sampler_DisplacementGuide, displUV).xy;
                displ = ((displ * 2) - 1) * _DisplacementAmount;

                float col = pow(saturate(lerp(0.5, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + displ).x, _Contrast)), _Power);
                #ifdef SECONDARY_TEX
                col = col * pow(saturate(lerp(0.5, SAMPLE_TEXTURE2D(_SecondaryTex, sampler_SecondaryTex, secondaryUV + displ).x, _Contrast)), _Power) * 2;
                #endif

                //Masking
                #ifdef CIRCLE_MASK
                float circle = distance(i.uv, float2(0.5, 0.5));
                col *= 1 - smoothstep(_OuterRadius, _OuterRadius + _Smoothness, circle);
                col *= smoothstep(_InnerRadius, _InnerRadius + _Smoothness, circle);
                #endif

                #ifdef RECT_MASK
                float2 uvMapped = (i.uv * 2) - 1;
                float rect = max(abs(uvMapped.x / _RectWidth), abs(uvMapped.y / _RectHeight));
                col *= 1 - smoothstep(_RectMaskCutoff, _RectMaskCutoff + _RectSmoothness, rect);
                #endif

                float orCol = col;

                //Banding
                #ifdef BANDING
                col = round(col * _Bands) / _Bands;
                #endif

                //Transparency
                float cutoff = saturate(_Cutoff + (1 - i.color.a));
                float alpha = smoothstep(cutoff, cutoff + _CutoffSoftness, orCol);

                //Coloring
                half4 rampCol = SAMPLE_TEXTURE2D(_GradientMap, sampler_GradientMap, float2(col, 0)) + _BurnCol * smoothstep(orCol - cutoff, orCol - cutoff + _CutoffSoftness, _BurnSize) * smoothstep(0.001, 0.5, cutoff);
                half4 finalCol = half4(rampCol.rgb * _Color.rgb * rampCol.a, 1);

                // apply fog
                finalCol.rgb = MixFog(finalCol.rgb, i.fogCoord);
                finalCol.a = alpha * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + displ).a * _Color.a;

                //Soft Blending
                #ifdef SOFT_BLEND
                float2 screenUV = i.scrPos.xy / i.scrPos.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float depth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float diff = saturate(_IntersectionThresholdMax * (depth - i.scrPos.w));
                finalCol.a *= diff;
                #endif

                return finalCol;
            }
            ENDHLSL
        }
    }
}
