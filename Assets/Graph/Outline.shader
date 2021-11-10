Shader "GraphVR/FresnelShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}
		SubShader{
			Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Lambert alphatest:_Cutoff

			fixed4 _Color;

			struct Input {
				float3 viewDir;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				half factor = dot(normalize(IN.viewDir),o.Normal);
				o.Albedo = _Color;
				o.Emission.rgb = _Color;
				o.Alpha = 1-factor;
			}
			ENDCG
	}
		FallBack "Diffuse"
}