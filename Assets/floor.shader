Shader "Custom/floor"
{
	Properties
	{
		[HideInInspector] _ReflectionTex ("Texture", 2D) = "green" {}
	}
	SubShader
	{
		Pass
		{
			Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#pragma multi_compile_fwdbase
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 ref : TEXCOORD0;
				LIGHTING_COORDS(1,2)
				float3 viewDir : TEXCOORD3;
			};

			sampler2D _ReflectionTex;
			
			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.ref = ComputeNonStereoScreenPos(o.pos);
				o.viewDir.xyz = WorldSpaceViewDir(v.vertex);

				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 white = fixed4(1,1,1,1);
				float3 up = float3(0, 1, 0);
				i.viewDir = normalize(i.viewDir);
				half fresnel = dot(i.viewDir, up);

				float4 uv = i.ref;
				fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(uv));
				fixed4 col = lerp(refl, white, fresnel);
				float attenuation = LIGHT_ATTENUATION(i);
				return col * attenuation;
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}
