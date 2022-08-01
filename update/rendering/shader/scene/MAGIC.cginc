float4 UNWRAP(float4 vertex, float2 Coord) {
	vertex.zw = float2(0,1);
	return float4(float2(2,-2) * Coord.xy + float2(-1,1),vertex.zw);//mul(UNITY_MATRIX_P, vertex);
}

float3 CONVERTpositionTOtexture(float4 vertex) {
	return vertex.xyz;
}

float3 CONVERTnormalTOtexture(float3 normal) {
	return (normal+1)*.5;
}

float4 GETdepth(float3 position, float3 camera) {
	float3 distance = position - camera;
	//get magnitude
	//encode to float4
	return float4(distance, 0);//float encoded into float4
}