Shader "GenericCrossSection/GenericShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_CrossColor("Cross Section Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_PlaneNormal("PlaneNormal",Vector) = (0,1,0,0)
		_PlanePosition("PlanePosition",Vector) = (0,1000,0,1)
		_StencilMask("Stencil Mask", Range(0, 255)) = 255
		[Enum(None,0,Plane,1,Sphere,2,Box,3)]
		_CutType("Type of cross section", Int) = 0
		//[HideInInspector]
		_SphereRadius("Sphere's Radius", Float) = 1
		[Toggle(STOP)]
		_Stop("Stop", Float) = 1
		[Toggle(REVERSED)]
		_Reversed("Reversed", Float) = 0
}
SubShader{
	Tags { "RenderType" = "Opaque" }
	//LOD 200
	Stencil
	{
		Ref[_StencilMask]
		CompBack Always
		PassBack Replace

		CompFront Always
		PassFront Zero
	}
	Cull Back
		CGPROGRAM
				// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows

				// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

				sampler2D _MainTex;

			struct Input {
				float2 uv_MainTex;

				float3 worldPos;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			fixed4 _CrossColor;
			fixed3 _PlaneNormal;
			fixed3 _PlanePosition;
			float _Stop;
			int _CutType;
			float _SphereRadius;

			float _Reversed;

			fixed3 _BoxPlanesNormal0;
			fixed3 _BoxPlanesPos0;

			fixed3 _BoxPlanesNormal1;
			fixed3 _BoxPlanesPos1;

			fixed3 _BoxPlanesNormal2;
			fixed3 _BoxPlanesPos2;

			fixed3 _BoxPlanesNormal3;
			fixed3 _BoxPlanesPos3;

			fixed3 _BoxPlanesNormal4;
			fixed3 _BoxPlanesPos4;

			fixed3 _BoxPlanesNormal5;
			fixed3 _BoxPlanesPos5;

			bool checkVisability(fixed3 worldPos)
			{
				bool reversed = _Reversed == 1;

				if (_Stop == 1)
					return false;

				if (_CutType == 1) {
					float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
					return reversed ^ (dotProd1 > 0);
				}
				else if (_CutType == 2) {
					return reversed ^ (distance(worldPos, _PlanePosition) > _SphereRadius);
				}
				else if (_CutType == 3) {
					bool b0 = dot(worldPos - _BoxPlanesPos0, _BoxPlanesNormal0) > 0;
					bool b1 = dot(worldPos - _BoxPlanesPos1, _BoxPlanesNormal1) > 0;
					bool b2 = dot(worldPos - _BoxPlanesPos2, _BoxPlanesNormal2) > 0;
					bool b3 = dot(worldPos - _BoxPlanesPos3, _BoxPlanesNormal3) > 0;
					bool b4 = dot(worldPos - _BoxPlanesPos4, _BoxPlanesNormal4) > 0;
					bool b5 = dot(worldPos - _BoxPlanesPos5, _BoxPlanesNormal5) > 0;
					return (b0
						|| b1
						|| b2
						|| b3
						|| b4
						|| b5) ^ reversed;
				}
				else {
					return false;
				}
			}
			void surf(Input IN, inout SurfaceOutputStandard o) {
				if (checkVisability(IN.worldPos))discard;
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG

				Cull Front
				CGPROGRAM
	#pragma surface surf NoLighting  noambient

			struct Input {
				half2 uv_MainTex;
				float3 worldPos;

			};
			sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _CrossColor;
			fixed3 _PlaneNormal;
			fixed3 _PlanePosition;
			float _Stop;
			int _CutType;
			float _SphereRadius;

			float _Reversed;

			fixed3 _BoxPlanesNormal0;
			fixed3 _BoxPlanesPos0;

			fixed3 _BoxPlanesNormal1;
			fixed3 _BoxPlanesPos1;

			fixed3 _BoxPlanesNormal2;
			fixed3 _BoxPlanesPos2;

			fixed3 _BoxPlanesNormal3;
			fixed3 _BoxPlanesPos3;

			fixed3 _BoxPlanesNormal4;
			fixed3 _BoxPlanesPos4;

			fixed3 _BoxPlanesNormal5;
			fixed3 _BoxPlanesPos5;

			bool checkVisability(fixed3 worldPos)
			{
				bool reversed = _Reversed == 1;

				if (_Stop == 1)
					return false;

				if (_CutType == 1) {
					float dotProd1 = dot(worldPos - _PlanePosition, _PlaneNormal);
					return reversed ^ (dotProd1 > 0);
				}
				else if (_CutType == 2) {
					return reversed ^ (distance(worldPos, _PlanePosition) > _SphereRadius);
				}
				else if (_CutType == 3) {
					bool b0 = dot(worldPos - _BoxPlanesPos0, _BoxPlanesNormal0) > 0;
					bool b1 = dot(worldPos - _BoxPlanesPos1, _BoxPlanesNormal1) > 0;
					bool b2 = dot(worldPos - _BoxPlanesPos2, _BoxPlanesNormal2) > 0;
					bool b3 = dot(worldPos - _BoxPlanesPos3, _BoxPlanesNormal3) > 0;
					bool b4 = dot(worldPos - _BoxPlanesPos4, _BoxPlanesNormal4) > 0;
					bool b5 = dot(worldPos - _BoxPlanesPos5, _BoxPlanesNormal5) > 0;
					return (b0
						|| b1
						|| b2
						|| b3
						|| b4
						|| b5) ^ reversed;
				}
				else {
					return false;
				}
			}
			fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
			{
				fixed4 c;
				c.rgb = s.Albedo;
				c.a = s.Alpha;
				return c;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				if (checkVisability(IN.worldPos))discard;
				o.Albedo = _CrossColor;

			}
				ENDCG

			}
				//FallBack "Diffuse"
}
