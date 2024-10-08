using UnityEngine;
/*
    图形的分辨率越高，CPU 和 GPU 计算位置和渲染立方体的工作量就越大。点的数量等于分辨率的平方，因此分辨率提高一倍会显著增加工作量。
    在分辨率为 100 的情况下，我们或许能达到 60FPS，但我们能做到什么程度呢？如果我们遇到了瓶颈，是否可以通过使用不同的方法来突破？

    对 40,000 个点的变换矩阵进行排序、批处理，然后发送到 GPU 需要大量时间。单个矩阵由 16 个浮点数组成，每个浮点数 4 个字节，每个矩阵总计 64B。
    对于 40,000 个点来说，每次绘制这些点都需要将 256 万字节（约 2.44MiB）复制到 GPU。URP 每帧需要复制两次，一次用于阴影，一次用于常规几何体。
    BRP(内置管线) 至少要做三次，因为它有额外的纯深度传递，而且除了主方向光之外，每种光线都要多做一次。
    ("MiB":由于计算机硬件使用二进制数来寻址内存，因此内存是以 2 的幂次而不是 10 的幂次来划分的。MiB 是 mebibyte 的后缀，即 220 = 1,0242 = 1,048,576 字节。
    这原本被称为兆字节，用 MB 表示，但现在应该表示 106 个字节，与官方定义的百万字节一致。不过，MB、GB 等仍经常被使用，而不是 MiB、GiB 等。)

    一般来说，最好尽量减少 CPU 和 GPU 之间的通信和数据传输量。由于我们只需要点的位置来显示它们，如果这些数据只存在于 GPU 端，那将是最理想的。
    这样可以减少大量的数据传输。但这样一来，CPU 就不能再计算位置了，而必须由 GPU 来完成。幸运的是，GPU 非常适合这项任务。
    
    让 GPU 计算位置需要采用不同的方法。为了便于比较，我们将保留当前图形，并创建一个新图形。复制图形 C# 资产文件并将其重命名为 GPUGraph。
    删除新类中的 pointPrefab 和 points 字段。然后删除其 Awake、UpdateFunction 和 UpdateFunctionTransition 方法。
    我只标记了新类中已删除的代码，而不是将所有代码都标记为新代码。
 */
public class GPUGraph : MonoBehaviour {

	const int maxResolution = 1000;

	static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
		resolutionId = Shader.PropertyToID("_Resolution"),
		stepId = Shader.PropertyToID("_Step"),
		timeId = Shader.PropertyToID("_Time"),
		transitionProgressId = Shader.PropertyToID("_TransitionProgress");

	[SerializeField]
	ComputeShader computeShader;

    // 因为位置已经存在于 GPU 上，所以我们不需要在 CPU 上跟踪它们。我们甚至不需要游戏对象。相反，我们将通过一条命令多次指示 GPU 绘制带有特定材质的特定网格。
    // 要配置绘制的内容，请在 GPUGraph 中添加可序列化的材质和网格字段。最初，我们将使用现有的 “点表面 ”材质（Point Surface），该材质已用于使用 BRP 绘制点。
    // 对于网格，我们将使用默认立方体。
    [SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

	[SerializeField, Range(10, maxResolution)]
	int resolution = 10;

	[SerializeField]
    FunctionLibrary05.FunctionName function;

	public enum TransitionMode { Cycle, Random }

	[SerializeField]
	TransitionMode transitionMode;

	[SerializeField, Min(0f)]
	float functionDuration = 1f, transitionDuration = 1f;

	float duration;

	bool transitioning;

    FunctionLibrary05.FunctionName transitionFunction;

    // 要在 GPU 上存储位置，我们需要为它们分配空间。为此，我们需要创建一个 ComputeBuffer 对象。通过调用 new ComputeBuffer()（即构造方法），
    // 在 GPUGraph 中添加一个位置缓冲区字段，并在新的 Awake 方法中创建对象。它的工作原理类似于分配一个新数组，但对象是结构体。
    ComputeBuffer positionsBuffer;

	void OnEnable () {
        // 我们需要将缓冲区的元素数量作为参数传递，也就是分辨率的平方，就像图的位置数组一样。
        // 计算缓冲区包含任意非类型数据。我们必须通过第二个参数指定每个元素的确切大小（以字节为单位）。我们需要存储由三个浮点数组成的三维位置矢量，因此元素大小为三个乘以四个字节。
        // 因此，40,000 个位置需要 0.48MB 或大约 0.46MiB 的 GPU 内存。
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
	}

	void OnDisable () {
		positionsBuffer.Release();
		positionsBuffer = null;
	}

	void Update () {
		duration += Time.deltaTime;
		if (transitioning) {
			if (duration >= transitionDuration) {
				duration -= transitionDuration;
				transitioning = false;
			}
		}
		else if (duration >= functionDuration) {
			duration -= functionDuration;
			transitioning = true;
			transitionFunction = function;
			PickNextFunction();
		}

		UpdateFunctionOnGPU();
	}

	void PickNextFunction () {
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary05.GetNextFunctionName(function) :
            FunctionLibrary05.GetRandomFunctionNameOtherThan(function);
	}

	void UpdateFunctionOnGPU () {
		float step = 2f / resolution;
		computeShader.SetInt(resolutionId, resolution);
		computeShader.SetFloat(stepId, step);
		computeShader.SetFloat(timeId, Time.time);
		if (transitioning) {
			computeShader.SetFloat(
				transitionProgressId,
				Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
			);
		}

		var kernelIndex =
			(int)function +
			(int)(transitioning ? transitionFunction : function) *
            FunctionLibrary05.FunctionCount;
        // 我们还必须设置位置缓冲区，它不会复制任何数据，但会将缓冲区与内核连接起来。这需要调用 SetBuffer 来完成，它的工作原理与其他方法类似，
        // 只是需要一个额外的参数。它的第一个参数是内核函数的索引，因为一个计算着色器可以包含多个内核，缓冲区可以链接到特定的内核。
        // 我们可以通过调用计算着色器上的 FindKernel 来获取内核索引，但我们的单个内核的索引始终为零，因此我们可以直接使用该值。
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
        // 设置好缓冲区后，我们就可以在计算着色器上调用 “派发”（Dispatch），使用四个整数参数来运行内核。第一个是内核索引，另外三个是要运行的组数，同样按维度划分。如果在所有维度上都使用 1，就意味着只计算第一组 8×8 的位置。
        // 由于我们的分组大小固定为 8×8，因此我们在 X 和 Y 维度上需要的分组数量等于分辨率除以 8，然后四舍五入。我们可以通过执行浮点除法并将结果传给 Mathf.CeilToInt 来实现。
        int groups = Mathf.CeilToInt(resolution / 8f);
		computeShader.Dispatch(kernelIndex, groups, groups, 1);

        // 有了 GPU 上可用的位置，下一步就是绘制点，而无需从 CPU 向 GPU 发送任何转换矩阵。因此，着色器必须从缓冲区获取正确的位置，而不是依赖标准矩阵。

        material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);

        // 由于这种绘制方式不使用游戏对象，Unity 无法知道绘制发生在场景的哪个位置。我们必须通过提供一个边界框作为附加参数来说明这一点。
        // 这是一个轴对齐的框，表示我们正在绘制的任何内容的空间边界。Unity 会使用它来确定是否可以跳过绘制，因为绘制最终会超出摄像机的视野范围。
        // 这就是所谓的 “挫面剔除”（frustum culling）。因此，现在不是按点评估边界，而是一次性评估整个图形。这对我们的图形没有问题，因为我们的想法是查看整个图形。
        // 我们的图形位于原点，各点应保持在大小为 2 的立方体内。我们可以使用 Vector3.zero 和 Vector3.one 作为参数，通过调用 Bounds 构造方法创建一个边界值。
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        // 程序绘图的方法是调用 Graphics.DrawMeshInstancedProcedural，参数包括网格、子网格索引和材质。子网格索引用于网格由多个部分组成的情况，而我们的情况并非如此，因此我们使用的是零索引。在 UpdateFunctionOnGPU 结束时执行此操作。
        // 我们必须向 DrawMeshInstancedProcedural 提供的最后一个参数是应该绘制多少个实例。这应该与位置缓冲区中的元素数量相匹配，我们可以通过其计数属性获取。
        Graphics.DrawMeshInstancedProcedural(
			mesh, 0, material, bounds, resolution * resolution
		);
	}
}