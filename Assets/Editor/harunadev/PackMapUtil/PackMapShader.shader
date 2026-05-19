Shader "harunadev/PackMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SeparateMetallicMap ("Separate Metallic Map", 2D) = "white" {}
        _SeparateMetallicMapChannel ("Separate Metallic Map Channel", Range(0, 3)) = 0
        _SeparateSmoothnessMap ("Separate Smoothness Map", 2D) = "white" {}
        _SeparateSmoothnessMapChannel ("Separate Smoothness Map Channel", Range(0, 3)) = 0
        _SeparateSmoothnessMapIsRoughness ("Separate Smoothness Map Is Roughness", Float) = 0
        _SeparateOcclusionMap ("Separate Occlusion Map", 2D) = "white" {}
        _SeparateOcclusionMapChannel ("Separate Occlusion Map Channel", Range(0, 3)) = 0
        _SourceMetallicChannel ("Source Metallic Channel", Range(0, 4)) = 0
        _SourceSmoothnessChannel ("Source Smoothness Channel", Range(0, 4)) = 0
        _SourceOcclusionChannel ("Source Occlusion Channel", Range(0, 4)) = 0
        _SourceIsRoughness ("Source Is Roughness", Float) = 0
        _SourceIsSpecular ("Source Is Specular", Float) = 0
        _TargetBaseColor ("Target Base Color", Color) = (1,1,1,1)
        _TargetMetallicChannel ("Target Metallic Channel", Range(0, 3)) = 0
        _TargetSmoothnessChannel ("Target Smoothness Channel", Range(0, 3)) = 0
        _TargetOcclusionChannel ("Target Occlusion Channel", Range(0, 3)) = 0
        _TargetIsRoughness ("Target Is Roughness", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SeparateMetallicMap;
            float4 _SeparateMetallicMap_ST;
            int _SeparateMetallicMapChannel;

            sampler2D _SeparateSmoothnessMap;
            float4 _SeparateSmoothnessMap_ST;
            int _SeparateSmoothnessMapChannel;
            float _SeparateSmoothnessMapIsRoughness;

            sampler2D _SeparateOcclusionMap;
            float4 _SeparateOcclusionMap_ST;
            int _SeparateOcclusionMapChannel;

            int _SourceMetallicChannel;
            int _SourceSmoothnessChannel;
            int _SourceOcclusionChannel;
            float _SourceIsRoughness;
            float _SourceIsSpecular;
            fixed4 _TargetBaseColor;
            int _TargetMetallicChannel;
            int _TargetSmoothnessChannel;
            int _TargetOcclusionChannel;
            float _TargetIsRoughness;
            
            float metallic;
            float smoothness;
            float occlusion;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv, _SeparateMetallicMap);
                o.uv2 = TRANSFORM_TEX(v.uv, _SeparateSmoothnessMap);
                o.uv3 = TRANSFORM_TEX(v.uv, _SeparateOcclusionMap);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 metallicCol = tex2D(_SeparateMetallicMap, i.uv1);
                fixed4 smoothnessCol = tex2D(_SeparateSmoothnessMap, i.uv2);
                fixed4 occlusionCol = tex2D(_SeparateOcclusionMap, i.uv3);

                if (_SourceIsSpecular > 0.5) {
                    float specLum = dot(col.rgb, float3(0.2126, 0.7152, 0.0722));
                    metallic = saturate((specLum - 0.04) / 0.96);
                }
                else {
                    switch (_SourceMetallicChannel)
                    {
                        case 0: metallic = col.r; break;
                        case 1: metallic = col.g; break;
                        case 2: metallic = col.b; break;
                        case 3: metallic = col.a; break;
                        case 4: 
                        {
                            switch (_SeparateMetallicMapChannel) 
                            {
                                case 0: metallic = metallicCol.r; break;
                                case 1: metallic = metallicCol.g; break;
                                case 2: metallic = metallicCol.b; break;
                                case 3: metallic = metallicCol.a; break;
                                case 4: metallic = 0; break;
                                case 5: metallic = 1; break;
                            }
                        }
                        break;
                    }
                }
                switch (_SourceSmoothnessChannel)
                {
                    case 0: smoothness = col.r; break;
                    case 1: smoothness = col.g; break;
                    case 2: smoothness = col.b; break;
                    case 3: smoothness = col.a; break;
                    case 4: 
                    {
                        switch (_SeparateSmoothnessMapChannel) 
                        {
                            case 0: smoothness = smoothnessCol.r; break;
                            case 1: smoothness = smoothnessCol.g; break;
                            case 2: smoothness = smoothnessCol.b; break;
                            case 3: smoothness = smoothnessCol.a; break;
                            case 4: smoothness = 0; break;
                            case 5: smoothness = 1; break;
                        }
                    }
                    break;
                }
                switch (_SourceOcclusionChannel)
                {
                    case 0: occlusion = col.r; break;
                    case 1: occlusion = col.g; break;
                    case 2: occlusion = col.b; break;
                    case 3: occlusion = col.a; break;
                    case 4: 
                    {
                        switch (_SeparateOcclusionMapChannel) 
                        {
                            case 0: occlusion = occlusionCol.r; break;
                            case 1: occlusion = occlusionCol.g; break;
                            case 2: occlusion = occlusionCol.b; break;
                            case 3: occlusion = occlusionCol.a; break;
                            case 4: occlusion = 0; break;
                            case 5: occlusion = 1; break;
                        }
                    }
                    break;
                }

                if (_SourceIsRoughness > 0.5)
                {
                    smoothness = 1.0 - smoothness;
                }
                if (_TargetIsRoughness > 0.5)
                {
                    smoothness = 1.0 - smoothness;
                }

                fixed4 output = fixed4(1, 1, 1, 1);
                switch (_TargetMetallicChannel)
                {
                    case 0: output.r = metallic; break;
                    case 1: output.g = metallic; break;
                    case 2: output.b = metallic; break;
                    case 3: output.a = metallic; break;
                }
                switch (_TargetSmoothnessChannel)
                {
                    case 0: output.r = smoothness; break;
                    case 1: output.g = smoothness; break;
                    case 2: output.b = smoothness; break;
                    case 3: output.a = smoothness; break;
                }
                switch (_TargetOcclusionChannel)
                {
                    case 0: output.r = occlusion; break;
                    case 1: output.g = occlusion; break;
                    case 2: output.b = occlusion; break;
                    case 3: output.a = occlusion; break;
                }

                return output;
            }
            ENDCG
        }
    }
}
