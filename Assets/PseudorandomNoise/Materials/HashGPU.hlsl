#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<uint> _Hashes;
#endif

float4 _Config;

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float v = floor(_Config.y * unity_InstanceID + 0.00001);
		float u = unity_InstanceID - _Config.x * v;
		
		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(
			_Config.y * (u + 0.5) - 0.5,
			_Config.z * ((1.0 / 255.0) * (_Hashes[unity_InstanceID] >> 24) - 0.5),
			_Config.y * (v + 0.5) - 0.5,
			1.0
		);
		unity_ObjectToWorld._m00_m11_m22 = _Config.y;
	#endif
}

// 在实现真正的哈希函数之前，我们先来简单了解一下简单的数学函数。第一步，我们要让当前的灰度渐变每 256 个点重复一次。
// 为此，我们只需考虑 GetHashColor 中哈希值的八个最小有效位。具体做法是通过 & 位和运算符将哈希值与二进制 11111111（即十进制 255）相结合。
// 这样可以屏蔽数值，只保留最小有效位的 8 位，将其限制在 0-255 范围内。然后，通过除以 255，可将该范围缩小为 0-1。
float3 GetHashColor () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		uint hash = _Hashes[unity_InstanceID];
		return (1.0 / 255.0) * float3(
			hash & 255,
			(hash >> 8) & 255,
			(hash >> 16) & 255
		);
	#else
		return 1.0;
	#endif
}

void ShaderGraphFunction_float (float3 In, out float3 Out, out float3 Color) {
	Out = In;
	Color = GetHashColor();
}

void ShaderGraphFunction_half (half3 In, out half3 Out, out half3 Color) {
	Out = In;
	Color = GetHashColor();
}