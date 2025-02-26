Shader "Sprite (Unlit)" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_ZWrite ("Depth Write", Float) = 0
		_Cutoff ("Depth alpha cutoff", Range(0, 1)) = 0
		_ShadowAlphaCutoff ("Shadow alpha cutoff", Range(0, 1)) = 0.1
		_CustomRenderQueue ("Custom Render Queue", Float) = 0
		_OverlayColor ("Overlay Color", Vector) = (0,0,0,0)
		_Hue ("Hue", Range(-0.5, 0.5)) = 0
		_Saturation ("Saturation", Range(0, 2)) = 1
		_Brightness ("Brightness", Range(0, 2)) = 1
		_BlendTex ("Blend Texture", 2D) = "white" {}
		_BlendAmount ("Blend", Range(0, 1)) = 0
		[HideInInspector] _SrcBlend ("__src", Float) = 1
		[HideInInspector] _DstBlend ("__dst", Float) = 0
		[HideInInspector] _RenderQueue ("__queue", Float) = 0
		[HideInInspector] _Cull ("__cull", Float) = 0
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		struct Input
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	//CustomEditor "SpriteShaderGUI"
}