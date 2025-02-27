// compile this with
// "C:\Projects\MonoGame.BytingGames\Artifacts\MonoGame.Content.Builder.Editor.Launcher\Windows\Debug\mgcb-editor-windows-data\mgfxc.exe" "C:\Projects\SE\BytingLib\BytingLib\Content\Effects\TextureMsaa.fx" "C:\Projects\SE\BytingLib\BytingLib\Content\Effects\TextureMsaa.mgfx"
// make sure to set the TextureMsaa.mgfx to "Copy if newer"

#if OPENGL
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 MatrixTransform;
Texture2D Texture : register(s0);
sampler2D Sampler : register(s0) = sampler_state
{
	Texture = <Texture>;
};

struct VertexIn
{
	float3 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

VertexOut VS(in VertexIn input)
{
	VertexOut output;

	output.Position = mul(float4(input.Position, 1), MatrixTransform);
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;

	return output;
}

float4 PS(in VertexOut input) : COLOR0
{
	float2 toX = ddx(input.TexCoord);
	float2 toY = ddy(input.TexCoord);
	return (0.25 * tex2D(Sampler, input.TexCoord - toX * 0.2 - toY * 0.5)
		+ 0.25 * tex2D(Sampler, input.TexCoord + toX * 0.5 - toY * 0.2)
		+ 0.25 * tex2D(Sampler, input.TexCoord + toX * 0.2 + toY * 0.5)
		+ 0.25 * tex2D(Sampler, input.TexCoord - toX * 0.5 + toY * 0.2)
		) * input.Color;
}

technique Technique
{
	pass Pass
	{
		VertexShader = compile VS_SHADERMODEL VS();
		PixelShader = compile PS_SHADERMODEL PS();
	}
}