Shader "VFX/VFXMasterShaderTransparent"
{
    Properties
    {
        //Vertex color (particle Start Color)
        [Space(20)]
        [Toggle(VERTEX_COLOR)]
        _UseVertexColor("Use particle vertex color", float) = 0

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
        [Toggle(DIRECTIONAL_DISPLACEMENT)]
        _UseDirectionalDisplacement("Directional displacement (use R channel)", float) = 0
        _DisplacementDirection("Displacement direction (XY)", Vector) = (1,0,0,0)
        [Toggle(DISTORTION_FLOW)]
        _UseDistortionFlow("Animated distortion flow", float) = 0
        _DistortionFlowAmount("Flow bend amount", Range(0, 0.25)) = 0.02
        _DistortionFlowFrequency("Flow bend frequency (XY)", Vector) = (4,6,0,0)
        _DistortionFlowSpeed("Flow bend speed (XY)", Vector) = (1,1.3,0,0)

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

        //Mask texture
        [Space(20)]
        [Toggle(MASK_TEX)]
        _UseMaskTex("Use mask texture", float) = 0
        _MaskTex("Mask texture (R channel)", 2D) = "white" {}
        _MaskPanningSpeed("Mask panning speed", Vector) = (0,0,0,0)
        [Toggle(MASK_INVERT)]
        _MaskInvert("Invert mask", float) = 0
        _MaskStrength("Mask strength", Range(0,1)) = 1

        //Second mask texture
        [Space(20)]
        [Toggle(MASK_TEX_2)]
        _UseMaskTex2("Use mask texture 2", float) = 0
        _MaskTex2("Mask texture 2 (R channel)", 2D) = "white" {}
        _MaskPanningSpeed2("Mask 2 panning speed", Vector) = (0,0,0,0)
        [Toggle(MASK_INVERT_2)]
        _MaskInvert2("Invert mask 2", float) = 0
        _MaskStrength2("Mask 2 strength", Range(0,1)) = 1

        //Third mask texture
        [Space(20)]
        [Toggle(MASK_TEX_3)]
        _UseMaskTex3("Use mask texture 3", float) = 0
        _MaskTex3("Mask texture 3 (R channel)", 2D) = "white" {}
        _MaskPanningSpeed3("Mask 3 panning speed", Vector) = (0,0,0,0)
        [Toggle(MASK_INVERT_3)]
        _MaskInvert3("Invert mask 3", float) = 0
        _MaskStrength3("Mask 3 strength", Range(0,1)) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Offset -1, -1
        Cull [_Culling]
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local MASK_INVERT
            #pragma shader_feature_local VERTEX_COLOR   // 추가
            #pragma multi_compile_fog
            #pragma shader_feature_local SECONDARY_TEX
            #pragma shader_feature_local VERTEX_OFFSET
            #pragma shader_feature_local DIRECTIONAL_DISPLACEMENT
            #pragma shader_feature_local DISTORTION_FLOW
            #pragma shader_feature_local SOFT_BLEND
            #pragma shader_feature_local BANDING
            #pragma shader_feature_local POLAR
            #pragma shader_feature_local CIRCLE_MASK
            #pragma shader_feature_local RECT_MASK
            #pragma shader_feature_local MASK_TEX
            #pragma shader_feature_local MASK_INVERT
            #pragma shader_feature_local MASK_TEX_2
            #pragma shader_feature_local MASK_INVERT_2
            #pragma shader_feature_local MASK_TEX_3
            #pragma shader_feature_local MASK_INVERT_3
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #if defined(SOFT_BLEND)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float2 displUV : TEXCOORD1;
                float2 secondaryUV : TEXCOORD2;
                float4 scrPos : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                float2 maskUV : TEXCOORD5;
                float2 maskUV2 : TEXCOORD6;
                float2 maskUV3 : TEXCOORD7;
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecondaryTex);
            SAMPLER(sampler_SecondaryTex);
            TEXTURE2D(_GradientMap);
            SAMPLER(sampler_GradientMap);
            TEXTURE2D(_DisplacementGuide);
            SAMPLER(sampler_DisplacementGuide);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_MaskTex2);
            SAMPLER(sampler_MaskTex2);
            TEXTURE2D(_MaskTex3);
            SAMPLER(sampler_MaskTex3);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SecondaryTex_ST;
                float4 _DisplacementGuide_ST;
                float4 _MaskTex_ST;
                float4 _MaskTex2_ST;
                float4 _MaskTex3_ST;

                float4 _Color;
                float4 _BurnCol;

                float _Contrast;
                float _Power;

                float _Bands;

                float4 _PanningSpeed;
                float4 _SecondaryPanningSpeed;
                float4 _MaskPanningSpeed;
                float4 _MaskPanningSpeed2;
                float4 _MaskPanningSpeed3;

                float _Cutoff;
                float _CutoffSoftness;
                float _BurnSize;

                float _IntersectionThresholdMax;

                float _VertexOffsetAmount;

                float _DisplacementAmount;
                float4 _DisplacementDirection;
                float _DistortionFlowAmount;
                float4 _DistortionFlowFrequency;
                float4 _DistortionFlowSpeed;

                float _Smoothness;
                float _OuterRadius;
                float _InnerRadius;

                float _RectSmoothness;
                float _RectHeight;
                float _RectWidth;
                float _RectMaskCutoff;

                float _MaskStrength;
                float _MaskStrength2;
                float _MaskStrength3;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o = (Varyings)0;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.secondaryUV = TRANSFORM_TEX(v.uv, _SecondaryTex);
                o.maskUV = TRANSFORM_TEX(v.uv, _MaskTex);
                o.maskUV2 = TRANSFORM_TEX(v.uv, _MaskTex2);
                o.maskUV3 = TRANSFORM_TEX(v.uv, _MaskTex3);

                #ifdef VERTEX_OFFSET
                float vertOffset = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, o.uv + _Time.y * _PanningSpeed.xy, 0)
                    .x;
                #ifdef SECONDARY_TEX
                float secondTex = SAMPLE_TEXTURE2D_LOD(_SecondaryTex, sampler_SecondaryTex,
                                                       o.secondaryUV + _Time.y * _SecondaryPanningSpeed.xy,
                                                       0).x;
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

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.uv;
                float2 displUV = i.displUV;
                float2 secondaryUV = i.secondaryUV;
                float2 maskUV = i.maskUV;
                float2 maskUV2 = i.maskUV2;
                float2 maskUV3 = i.maskUV3;

                //Polar coords
                #ifdef POLAR
                float2 mappedUV = (i.uv * 2) - 1;
                uv = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.displUV * 2) - 1;
                displUV = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.secondaryUV * 2) - 1;
                secondaryUV = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.maskUV * 2) - 1;
                maskUV = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.maskUV2 * 2) - 1;
                maskUV2 = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                mappedUV = (i.maskUV3 * 2) - 1;
                maskUV3 = float2(atan2(mappedUV.y, mappedUV.x) / PI / 2.0 + 0.5, length(mappedUV));
                #endif

                //UV Panning
                uv += _Time.y * _PanningSpeed.xy;
                displUV += _Time.y * _PanningSpeed.zw;
                secondaryUV += _Time.y * _SecondaryPanningSpeed.xy;
                maskUV += _Time.y * _MaskPanningSpeed.xy;
                maskUV2 += _Time.y * _MaskPanningSpeed2.xy;
                maskUV3 += _Time.y * _MaskPanningSpeed3.xy;

                //Displacement
                #ifdef DISTORTION_FLOW
                //Cross-axis waves continuously reshape the guide UV instead of merely scrolling it.
                //Using separate frequency/speed values keeps the motion from looking repetitive.
                float2 flowOffset;
                flowOffset.x = sin(displUV.y * _DistortionFlowFrequency.x * TWO_PI
                                   + _Time.y * _DistortionFlowSpeed.x);
                flowOffset.y = sin(displUV.x * _DistortionFlowFrequency.y * TWO_PI
                                   + _Time.y * _DistortionFlowSpeed.y);
                displUV += flowOffset * _DistortionFlowAmount;
                #endif

                float4 displacementGuide = SAMPLE_TEXTURE2D(_DisplacementGuide, sampler_DisplacementGuide, displUV);

                #ifdef DIRECTIONAL_DISPLACEMENT
                //A single grayscale channel can be aimed in any UV direction.
                float2 displacementDirection = _DisplacementDirection.xy;
                displacementDirection *= rsqrt(max(dot(displacementDirection, displacementDirection), 0.00001));
                float2 displ = ((displacementGuide.r * 2) - 1) * displacementDirection * _DisplacementAmount;
                #else
                //Legacy mode: R offsets U and G offsets V.
                float2 displ = ((displacementGuide.xy * 2) - 1) * _DisplacementAmount;
                #endif

                //Single main texture sample, reused for color, alpha and ramp coordinate
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + displ);

                float col = pow(saturate(lerp(0.5, mainTex.r, _Contrast)), _Power);

                #ifdef SECONDARY_TEX
                float4 secondTexSample = SAMPLE_TEXTURE2D(_SecondaryTex, sampler_SecondaryTex, secondaryUV + displ);
                col = col * pow(saturate(lerp(0.5, secondTexSample.r, _Contrast)), _Power) * 2;
                #endif

                //Masking (procedural)
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

                //Masking (texture)
                #ifdef MASK_TEX
                float maskSample = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, maskUV).r;
                #ifdef MASK_INVERT
                maskSample = 1 - maskSample;
                #endif
                maskSample = lerp(1, maskSample, _MaskStrength);
                col *= maskSample;
                #endif

                #ifdef MASK_TEX_2
                float maskSample2 = SAMPLE_TEXTURE2D(_MaskTex2, sampler_MaskTex2, maskUV2).r;
                #ifdef MASK_INVERT_2
                maskSample2 = 1 - maskSample2;
                #endif
                maskSample2 = lerp(1, maskSample2, _MaskStrength2);
                col *= maskSample2;
                #endif

                #ifdef MASK_TEX_3
                float maskSample3 = SAMPLE_TEXTURE2D(_MaskTex3, sampler_MaskTex3, maskUV3).r;
                #ifdef MASK_INVERT_3
                maskSample3 = 1 - maskSample3;
                #endif
                maskSample3 = lerp(1, maskSample3, _MaskStrength3);
                col *= maskSample3;
                #endif

                //Value used for alpha cutoff, taken before banding so banding never affects transparency shape
                float orCol = col;

                //Banding (now actually feeds the color ramp instead of being discarded)
                #ifdef BANDING
                col = round(col * _Bands) / _Bands;
                #endif

                //Transparency
                float cutoff = saturate(_Cutoff + (1 - i.color.a));
                float alpha = smoothstep(cutoff, cutoff + _CutoffSoftness, orCol);

                //Coloring
                half4 rampCol = SAMPLE_TEXTURE2D(_GradientMap, sampler_GradientMap, float2(col, 0));

                half4 finalCol = half4(rampCol.rgb * mainTex.rgb * _Color.rgb, 1);

                #ifdef VERTEX_COLOR
                finalCol.rgb *= i.color.rgb;
                #endif

                // apply fog
                finalCol.rgb = MixFog(finalCol.rgb, i.fogCoord);
                finalCol.a = alpha * mainTex.a * _Color.a;

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
