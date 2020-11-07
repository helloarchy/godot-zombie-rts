﻿﻿Shader "Endless/Transparent Color" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

	CGPROGRAM
	#pragma surface surf Lambert alpha
	
	sampler2D _MainTex;
	fixed4 _Color;
	
	struct Input {
		float2 uv_MainTex;
	};
	
	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 c = _Color;
		//o.Albedo = (c.r/10) + (c.b/10) + (c.g/10);
		o.Albedo = c.rgb;
		o.Alpha = texColor.a * c.a;
	}
	ENDCG
}

Fallback "Transparent/Diffuse"
}