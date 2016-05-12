struct VOut
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

VOut vsMain(float4 position : POSITION, float4 color : COLOR)
{
	VOut output;

	output.position = position;
	output.color = color;

	return output;
}

float4 psMain(VOut pInput) : SV_TARGET
{
	return pInput.color;
}