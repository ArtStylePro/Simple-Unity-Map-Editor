Shader "Custom/GridShader"
{
	Properties
	{
      _GridThickness ("Grid Thickness", Float) = 0.01
      _GridSpacing ("Grid Spacing", Float) = 10.0
      _GridColour ("Grid Colour", Color) = (0.5, 1.0, 1.0, 1.0)
      _BaseColour ("Base Colour", Color) = (0.0, 0.0, 0.0, 0.0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" }

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			uniform float _GridThickness;
	        uniform float _GridSpacing;
	        uniform float4 _GridColour;
	        uniform float4 _BaseColour;

			struct vertexInput
			{
				float4 vertex : POSITION;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float2 worldPos : TEXCOORD0;
			};
			
			vertexOutput vert (vertexInput v)
			{
				vertexOutput o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				// Calculate the world position coordinates to pass to the fragment shader
				o.worldPos = mul(_Object2World, v.vertex);
				return o;
			}
			
			float4 frag (vertexOutput i) : COLOR
			{
				if (frac(i.worldPos.x/_GridSpacing) < _GridThickness || frac(i.worldPos.y/_GridSpacing) < _GridThickness)
				{
	            	return _GridColour;
	            }
	            else 
	            {
	            	return _BaseColour;
	            }
			}
			ENDCG
		}
	}
}
