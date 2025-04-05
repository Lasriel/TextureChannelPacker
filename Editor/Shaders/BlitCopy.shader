Shader "Hidden/BlitCopy" {
    Properties {
        // Initialize to black textures and color
        [HideInInspector] _ToRedTexture ("Red", 2D) = "black" {}
        [HideInInspector] _ToGreenTexture ("Green", 2D) = "black" {}
        [HideInInspector] _ToBlueTexture ("Blue", 2D) = "black" {}
        [HideInInspector] _ToAlphaTexture ("Alpha", 2D) = "black" {}
        [HideInInspector] _OutputColor ("Color", Color) = (0,0,0,0)
    }
    SubShader {
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct Attributes {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Input texture samplers
            sampler2D _ToRedTexture;
            sampler2D _ToGreenTexture;
            sampler2D _ToBlueTexture;
            sampler2D _ToAlphaTexture;
            
            float4 _OutputColor;
            float4 _InputChannels;
            float4 _Inverts;

            Varyings vert(Attributes attributes) {
                Varyings o;
                o.positionCS = UnityObjectToClipPos(attributes.positionOS);
                o.uv = attributes.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target {
                // Sample input textures to colors
                float4 texColorR = tex2D(_ToRedTexture, i.uv);
                float4 texColorG = tex2D(_ToGreenTexture, i.uv);
                float4 texColorB = tex2D(_ToBlueTexture, i.uv);
                float4 texColorA = tex2D(_ToAlphaTexture, i.uv);

                // Get color from texture by channel
                float inputColorR = texColorR[_InputChannels[0]];
                float inputColorG = texColorG[_InputChannels[1]];
                float inputColorB = texColorB[_InputChannels[2]];
                float inputColorA = texColorA[_InputChannels[3]];
                
                // Swap color channels, invert color if invert == 1
                _OutputColor[0] = _Inverts[0] ? 1 - inputColorR : inputColorR;
                _OutputColor[1] = _Inverts[1] ? 1 - inputColorG : inputColorG;
                _OutputColor[2] = _Inverts[2] ? 1 - inputColorB : inputColorB;
                _OutputColor[3] = _Inverts[3] ? 1 - inputColorA : inputColorA;
                return _OutputColor;
            }
            ENDCG
        }
    }
}