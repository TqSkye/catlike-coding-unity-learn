#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3> _Positions;
#endif

float _Step;

// ת���������ڽ�����Ӷ���ռ�ת��������ռ䡣��ͨ�� unity_ObjectToWorld ȫ���ṩ�����������ǳ��򻯻��ƣ�����һ����ݾ���������Ǳ����滻���������������������Ϊ�㡣
// ���ǿ���ͨ�� float4(position, 1.0) Ϊλ��ƫ�ƹ���һ�������������ǿ��Խ��丳ֵ�� unity_ObjectToWorld._m03_m13_m23_m33����������Ϊ�����С�
// Ȼ������ɫ������Ӹ��� _Step ��ɫ�����ԣ������丳ֵ�� unity_ObjectToWorld._m00_m11_m22������������ȷ�������ǵĵ㡣
void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		float3 position = _Positions[unity_InstanceID];

		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0);
		unity_ObjectToWorld._m00_m11_m22 = _Step;
	#endif
}

// ���ǽ�ʹ���Զ��庯���ڵ㽫 HLSL �ļ���������ɫ��ͼ���С���������Ŀ�����ýڵ�����ļ��еĺ�������Ȼ���ǲ�����Ҫ��һ���ܣ���������������ӵ�ͼ���У�
// ����Ͳ��ᱻ�������ڡ���ˣ����ǽ�Ϊ PointGPU ���һ����ʽ��ȷ���麯�����ú���ֻ���� float3 ֵ�����ı�����
// �������� _float ��׺�Ǳ���ģ���Ϊ����ʾ�����ľ��ȡ���ɫ��ͼ���ṩ���־���ģʽ�������뾫�ȡ����ߵĴ�С��ǰ�ߵ�һ�룬����������ֽڶ������ĸ��ֽڡ��ڵ�ʹ�õľ��ȿ�����ȷѡ��Ҳ����Ĭ������Ϊ�̳С�
// Ϊ��ȷ�����ǵ�ͼ�������־���ģʽ�¶����������������ǻ������һ��ʹ�ð뾫�ȵı��庯����
void ShaderGraphFunction_float (float3 In, out float3 Out) {
	Out = In;
}

void ShaderGraphFunction_half (half3 In, out half3 Out) {
	Out = In;
}