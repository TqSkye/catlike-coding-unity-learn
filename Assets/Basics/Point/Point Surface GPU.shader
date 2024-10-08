Shader "Graph/Point Surface GPU" {

	Properties {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5
	}
	
	SubShader {
		CGPROGRAM
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        // ���ڣ�����ռ䶥��λ�ý�ͨ�����ǵ����⺯�����ݣ����ǵĴ��뽫���������ɵ���ɫ���С�
        // ��Ҫ���ó�����Ⱦ�����ǻ�������� #pragma instancing_options �� #pragma editor_sync_compilation ������ָ�

        // ��shader graph��:
        // ��Щָ�����ֱ��ע�뵽���ɵ���ɫ��Դ�����У�����ͨ���������ļ����롣��ˣ������һ���Զ��庯���ڵ㣬������������֮ǰ����ͬ������ν�����������Ϊ�ַ�����
        // ������������Ϊ���ʵ����ƣ��� InjectPragmas��Ȼ��ָ����� Body �ı�����(shader graph��)�������Ǻ����Ĵ���飬������ǻ����������ｫ�������������

        // ���⣬����һ�� unity_WorldToObject �������а������任�����ڱ任����������Ӧ�÷Ǿ��ȱ���ʱ����Ҫ��������ȷ�任�����������������������������ǵ�ͼ�Σ�������ǿ��Ժ�����������������Ӧ����ʵ����ѡ�� pragma ����� assumeuniformscaling���Ӷ�������ɫ����һ�㡣
		#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
		#pragma editor_sync_compilation
		#pragma target 4.5

		#include "PointGPU.hlsl"
		
		struct Input {
			float3 worldPos;
		};

		float _Smoothness;
		
		void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
			surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
			surface.Smoothness = _Smoothness;
		}
		ENDCG
	}
						
	FallBack "Diffuse"
}