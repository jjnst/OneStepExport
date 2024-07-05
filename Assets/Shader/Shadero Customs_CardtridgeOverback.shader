Shader "Shadero Customs/CardtridgeOverback" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_ColorHSV_Hue_1 ("_ColorHSV_Hue_1", Range(0, 360)) = 180
		_ColorHSV_Saturation_1 ("_ColorHSV_Saturation_1", Range(0, 2)) = 1
		_ColorHSV_Brightness_1 ("_ColorHSV_Brightness_1", Range(0, 2)) = 1
		_FillColor_Color_1 ("_FillColor_Color_1", Vector) = (1,1,1,1)
		_GrayScale_Fade_1 ("_GrayScale_Fade_1", Range(0, 1)) = 1
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