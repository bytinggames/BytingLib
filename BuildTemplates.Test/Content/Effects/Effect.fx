
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "Include.fxh"

Texture2D SpriteTexture;

sampler TextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct PixelInput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

float4 MainPS(PixelInput input) : COLOR
{
	float4 texColor = tex2D(TextureSampler, input.TexCoord) * input.Color;
	if (input.Position.x < 50)
	{
		texColor.rgb = 1.0 * texColor.a - texColor.rgb;
		texColor *= Alpha;
	}
	return texColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};