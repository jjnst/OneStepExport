Shader "Shadero Customs/EnemyTier3" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_PremadeGradients_Offset_1 ("_PremadeGradients_Offset_1", Range(-1, 1)) = 0
		_PremadeGradients_Fade_1 ("_PremadeGradients_Fade_1", Range(0, 1)) = 1
		_PremadeGradients_Speed_1 ("_PremadeGradients_Speed_1", Range(-2, 2)) = 0
		_TintRGBA_Color_1 ("_TintRGBA_Color_1", Vector) = (1,1,1,0)
		_GrayScale_Fade_1 ("_GrayScale_Fade_1", Range(0, 1)) = 0
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