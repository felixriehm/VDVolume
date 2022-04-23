Shader "Custom/InstancedVoxelShader"
{
    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"

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
                float3 ambient : TEXCOORD1;
                float3 diffuse : TEXCOORD2;
                SHADOW_COORDS(4)
            };

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            {
                float4 worldVoxelPos = mul(volumeTransform, float4(voxelProperties[instanceID].position, 1.0f));
                float4x4 voxelMeshTransform = volumeTransform;
                voxelMeshTransform[0][3] = worldVoxelPos.x;
                voxelMeshTransform[1][3] = worldVoxelPos.y;
                voxelMeshTransform[2][3] = worldVoxelPos.z;
                voxelMeshTransform[3][3] = 1.0f;
                
                float4 pos = mul(voxelMeshTransform, v.vertex);
                float rgbZeroToOne = voxelProperties[instanceID].color/255;

                // shading
                float3 worldNormal = v.normal;
                float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
                float3 diffuse = (ndotl * _LightColor0.rgb);

                v2f o;
                o.pos = UnityObjectToClipPos(pos); // mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.color = float4(rgbZeroToOne,rgbZeroToOne,rgbZeroToOne, 1.0f);

                // shading
                o.ambient = ambient;
                o.diffuse = diffuse;
                TRANSFER_SHADOW(o)
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed shadow = SHADOW_ATTENUATION(i);
                float3 lighting = i.diffuse * shadow + i.ambient;
                fixed4 output = fixed4(i.color * lighting, 1.0f);
                UNITY_APPLY_FOG(i.fogCoord, output);
                return output;
            }

            ENDCG
        }
    }
}
