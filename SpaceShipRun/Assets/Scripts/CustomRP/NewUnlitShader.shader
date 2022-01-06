Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#include "UnityCG.cginc"
            
            #pragma target 3.5
			#pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            
            #ifndef MYRP_UNLIT_INCLUDED
            #define MYRP_UNLIT_INCLUDED

            UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)
            
            
            struct VertexInput {
	            float4 pos : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput {
	            float4 clipPos : SV_POSITION;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            VertexOutput UnlitPassVertex (VertexInput input) {
				VertexOutput output;
            	UNITY_SETUP_INSTANCE_ID(input)
            	UNITY_TRANSFER_INSTANCE_ID(input, output);
				float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
            	output.clipPos = mul(unity_MatrixVP, worldPos);
				return output;
			}

            float4 UnlitPassFragment (VertexOutput input) : SV_TARGET {
            	UNITY_SETUP_INSTANCE_ID(input);
            	float4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				return color;
			}
            #endif // MYRP_UNLIT_INCLUDED
            ENDCG
        }
    }
}
