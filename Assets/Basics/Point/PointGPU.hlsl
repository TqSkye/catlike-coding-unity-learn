#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3> _Positions;
#endif

float _Step;

// 转换矩阵用于将顶点从对象空间转换到世界空间。它通过 unity_ObjectToWorld 全局提供。由于我们是程序化绘制，它是一个身份矩阵，因此我们必须替换它。最初将整个矩阵设置为零。
// 我们可以通过 float4(position, 1.0) 为位置偏移构建一个列向量。我们可以将其赋值给 unity_ObjectToWorld._m03_m13_m23_m33，将其设置为第四列。
// 然后在着色器中添加浮点 _Step 着色器属性，并将其赋值给 unity_ObjectToWorld._m00_m11_m22。这样就能正确缩放我们的点。
void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float3 position = _Positions[unity_InstanceID];

		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
		unity_ObjectToWorld._m00_m11_m22 = _Step;
	#endif
}

// 我们将使用自定义函数节点将 HLSL 文件包含在着色器图形中。这样做的目的是让节点调用文件中的函数。虽然我们并不需要这一功能，但如果不将其连接到图形中，
// 代码就不会被包含在内。因此，我们将为 PointGPU 添加一个格式正确的虚函数，该函数只传递 float3 值而不改变它。
// 函数名的 _float 后缀是必需的，因为它表示函数的精度。着色器图形提供两种精度模式：浮点或半精度。后者的大小是前者的一半，因此是两个字节而不是四个字节。节点使用的精度可以明确选择，也可以默认设置为继承。
// 为了确保我们的图形在两种精度模式下都能正常工作，我们还添加了一个使用半精度的变体函数。
void ShaderGraphFunction_float (float3 In, out float3 Out) {
	Out = In;
}

void ShaderGraphFunction_half (half3 In, out half3 Out) {
	Out = In;
}