Shader "Shadero Customs/Upgrade" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		DistortionUV_WaveX_1 ("DistortionUV_WaveX_1", Range(0, 128)) = 17.806
		DistortionUV_WaveY_1 ("DistortionUV_WaveY_1", Range(0, 128)) = 10
		DistortionUV_DistanceX_1 ("DistortionUV_DistanceX_1", Range(0, 1)) = 0.3
		DistortionUV_DistanceY_1 ("DistortionUV_DistanceY_1", Range(0, 1)) = 0.3
		DistortionUV_Speed_1 ("DistortionUV_Speed_1", Range(-2, 2)) = 1
		_GenerateLightning_PosX_1 ("_GenerateLightning_PosX_1", Range(-2, 2)) = 0.412
		_GenerateLightning_PosY_1 ("_GenerateLightning_PosY_1", Range(-2, 2)) = 0.5
		_GenerateLightning_Size_1 ("_GenerateLightning_Size_1", Range(1, 8)) = 4
		_GenerateLightning_Number_1 ("_GenerateLightning_Number_1", Range(2, 16)) = 7.895
		_GenerateLightning_Speed_1 ("_GenerateLightning_Speed_1", Range(0, 8)) = 1.117
		_Destroyer_Value_1 ("_Destroyer_Value_1", Range(0, 1)) = 0
		_Destroyer_Speed_1 ("_Destroyer_Speed_1", Range(0, 1)) = 0.5
		_SpriteFade ("SpriteFade", Range(0, 1)) = 1
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback "Sprites/Default"
}