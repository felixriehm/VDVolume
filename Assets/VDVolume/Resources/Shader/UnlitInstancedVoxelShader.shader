Shader "Custom/InstancedVoxelShader"
{
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
            };
            
            struct VoxelProperties {
                float3 position;
                float color;
            };
            
            StructuredBuffer<VoxelProperties> voxelProperties;
            float4x4 volumeTransform;
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata_t v, uint instanceID : SV_InstanceID)
            {
                float4 worldVoxelPos = mul(volumeTransform, float4(voxelProperties[instanceID].position, 1.0f));
                float4x4 voxelMeshTransform = volumeTransform;
                voxelMeshTransform[0][3] = worldVoxelPos.x;
                voxelMeshTransform[1][3] = worldVoxelPos.y;
                voxelMeshTransform[2][3] = worldVoxelPos.z;
                voxelMeshTransform[3][3] = 1.0f;
                
                float4 pos = mul(voxelMeshTransform, v.vertex);
                float rgbZeroToOne = voxelProperties[instanceID].color/255;

                v2f o;
                o.pos = UnityObjectToClipPos(pos); // mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.color = float4(rgbZeroToOne,rgbZeroToOne,rgbZeroToOne, 1.0f);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(i.color);
            }

            ENDCG
        }
    }
}
