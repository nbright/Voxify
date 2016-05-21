Shader "Custom/VertexAdd"
{
	Subshader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend one one
		BindChannels
	{
		Bind "vertex", vertex
		Bind "color", color
	}
		Pass
	{

	}
	}
}