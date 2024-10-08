Shader "Graph/Point Surface GPU" {

	Properties {
		_Smoothness ("Smoothness", Range(0,1)) = 0.5
	}
	
	SubShader {
		CGPROGRAM
		#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        // 现在，对象空间顶点位置将通过我们的虚拟函数传递，我们的代码将包含在生成的着色器中。
        // 但要启用程序化渲染，我们还必须包含 #pragma instancing_options 和 #pragma editor_sync_compilation 编译器指令。

        // 若shader graph中:
        // 这些指令必须直接注入到生成的着色器源代码中，不能通过单独的文件加入。因此，添加另一个自定义函数节点，其输入和输出与之前的相同，但这次将其类型设置为字符串。
        // 将其名称设置为合适的名称，如 InjectPragmas，然后将指令放在 Body 文本块中(shader graph中)。正文是函数的代码块，因此我们还必须在这里将输入分配给输出。

        // 此外，还有一个 unity_WorldToObject 矩阵，其中包含反变换，用于变换法向量。当应用非均匀变形时，需要用它来正确变换方向向量。但由于它不适用于我们的图形，因此我们可以忽略它。不过，我们应该在实例化选项 pragma 中添加 assumeuniformscaling，从而告诉着色器这一点。
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