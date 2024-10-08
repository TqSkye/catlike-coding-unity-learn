using UnityEngine;
/*
    ͼ�εķֱ���Խ�ߣ�CPU �� GPU ����λ�ú���Ⱦ������Ĺ�������Խ�󡣵���������ڷֱ��ʵ�ƽ������˷ֱ������һ�����������ӹ�������
    �ڷֱ���Ϊ 100 ������£����ǻ����ܴﵽ 60FPS��������������ʲô�̶��أ��������������ƿ�����Ƿ����ͨ��ʹ�ò�ͬ�ķ�����ͻ�ƣ�

    �� 40,000 ����ı任�����������������Ȼ���͵� GPU ��Ҫ����ʱ�䡣���������� 16 ����������ɣ�ÿ�������� 4 ���ֽڣ�ÿ�������ܼ� 64B��
    ���� 40,000 ������˵��ÿ�λ�����Щ�㶼��Ҫ�� 256 ���ֽڣ�Լ 2.44MiB�����Ƶ� GPU��URP ÿ֡��Ҫ�������Σ�һ��������Ӱ��һ�����ڳ��漸���塣
    BRP(���ù���) ����Ҫ�����Σ���Ϊ���ж���Ĵ���ȴ��ݣ����ҳ����������֮�⣬ÿ�ֹ��߶�Ҫ����һ�Ρ�
    ("MiB":���ڼ����Ӳ��ʹ�ö���������Ѱַ�ڴ棬����ڴ����� 2 ���ݴζ����� 10 ���ݴ������ֵġ�MiB �� mebibyte �ĺ�׺���� 220 = 1,0242 = 1,048,576 �ֽڡ�
    ��ԭ������Ϊ���ֽڣ��� MB ��ʾ��������Ӧ�ñ�ʾ 106 ���ֽڣ���ٷ�����İ����ֽ�һ�¡�������MB��GB ���Ծ�����ʹ�ã������� MiB��GiB �ȡ�)

    һ����˵����þ������� CPU �� GPU ֮���ͨ�ź����ݴ���������������ֻ��Ҫ���λ������ʾ���ǣ������Щ����ֻ������ GPU �ˣ��ǽ���������ġ�
    �������Լ��ٴ��������ݴ��䡣������һ����CPU �Ͳ����ټ���λ���ˣ��������� GPU ����ɡ����˵��ǣ�GPU �ǳ��ʺ���������
    
    �� GPU ����λ����Ҫ���ò�ͬ�ķ�����Ϊ�˱��ڱȽϣ����ǽ�������ǰͼ�Σ�������һ����ͼ�Ρ�����ͼ�� C# �ʲ��ļ�������������Ϊ GPUGraph��
    ɾ�������е� pointPrefab �� points �ֶΡ�Ȼ��ɾ���� Awake��UpdateFunction �� UpdateFunctionTransition ������
    ��ֻ�������������ɾ���Ĵ��룬�����ǽ����д��붼���Ϊ�´��롣
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

    // ��Ϊλ���Ѿ������� GPU �ϣ��������ǲ���Ҫ�� CPU �ϸ������ǡ�������������Ҫ��Ϸ�����෴�����ǽ�ͨ��һ��������ָʾ GPU ���ƴ����ض����ʵ��ض�����
    // Ҫ���û��Ƶ����ݣ����� GPUGraph ����ӿ����л��Ĳ��ʺ������ֶΡ���������ǽ�ʹ�����е� ������� �����ʣ�Point Surface�����ò���������ʹ�� BRP ���Ƶ㡣
    // �����������ǽ�ʹ��Ĭ�������塣
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

    // Ҫ�� GPU �ϴ洢λ�ã�������ҪΪ���Ƿ���ռ䡣Ϊ�ˣ�������Ҫ����һ�� ComputeBuffer ����ͨ������ new ComputeBuffer()�������췽������
    // �� GPUGraph �����һ��λ�û������ֶΣ������µ� Awake �����д����������Ĺ���ԭ�������ڷ���һ�������飬�������ǽṹ�塣
    ComputeBuffer positionsBuffer;

	void OnEnable () {
        // ������Ҫ����������Ԫ��������Ϊ�������ݣ�Ҳ���Ƿֱ��ʵ�ƽ��������ͼ��λ������һ����
        // ���㻺��������������������ݡ����Ǳ���ͨ���ڶ�������ָ��ÿ��Ԫ�ص�ȷ�д�С�����ֽ�Ϊ��λ����������Ҫ�洢��������������ɵ���άλ��ʸ�������Ԫ�ش�СΪ���������ĸ��ֽڡ�
        // ��ˣ�40,000 ��λ����Ҫ 0.48MB ���Լ 0.46MiB �� GPU �ڴ档
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
        // ���ǻ���������λ�û������������Ḵ���κ����ݣ����Ὣ���������ں���������������Ҫ���� SetBuffer ����ɣ����Ĺ���ԭ���������������ƣ�
        // ֻ����Ҫһ������Ĳ��������ĵ�һ���������ں˺�������������Ϊһ��������ɫ�����԰�������ںˣ��������������ӵ��ض����ںˡ�
        // ���ǿ���ͨ�����ü�����ɫ���ϵ� FindKernel ����ȡ�ں������������ǵĵ����ں˵�����ʼ��Ϊ�㣬������ǿ���ֱ��ʹ�ø�ֵ��
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
        // ���úû����������ǾͿ����ڼ�����ɫ���ϵ��� ���ɷ�����Dispatch����ʹ���ĸ����������������ںˡ���һ�����ں�����������������Ҫ���е�������ͬ����ά�Ȼ��֡����������ά���϶�ʹ�� 1������ζ��ֻ�����һ�� 8��8 ��λ�á�
        // �������ǵķ����С�̶�Ϊ 8��8����������� X �� Y ά������Ҫ�ķ����������ڷֱ��ʳ��� 8��Ȼ���������롣���ǿ���ͨ��ִ�и����������������� Mathf.CeilToInt ��ʵ�֡�
        int groups = Mathf.CeilToInt(resolution / 8f);
		computeShader.Dispatch(kernelIndex, groups, groups, 1);

        // ���� GPU �Ͽ��õ�λ�ã���һ�����ǻ��Ƶ㣬������� CPU �� GPU �����κ�ת��������ˣ���ɫ������ӻ�������ȡ��ȷ��λ�ã�������������׼����

        material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);

        // �������ֻ��Ʒ�ʽ��ʹ����Ϸ����Unity �޷�֪�����Ʒ����ڳ������ĸ�λ�á����Ǳ���ͨ���ṩһ���߽����Ϊ���Ӳ�����˵����һ�㡣
        // ����һ�������Ŀ򣬱�ʾ�������ڻ��Ƶ��κ����ݵĿռ�߽硣Unity ��ʹ������ȷ���Ƿ�����������ƣ���Ϊ�������ջᳬ�����������Ұ��Χ��
        // �������ν�� �������޳�����frustum culling������ˣ����ڲ��ǰ��������߽磬����һ������������ͼ�Ρ�������ǵ�ͼ��û�����⣬��Ϊ���ǵ��뷨�ǲ鿴����ͼ�Ρ�
        // ���ǵ�ͼ��λ��ԭ�㣬����Ӧ�����ڴ�СΪ 2 ���������ڡ����ǿ���ʹ�� Vector3.zero �� Vector3.one ��Ϊ������ͨ������ Bounds ���췽������һ���߽�ֵ��
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        // �����ͼ�ķ����ǵ��� Graphics.DrawMeshInstancedProcedural�������������������������Ͳ��ʡ��������������������ɶ��������ɵ�����������ǵ����������ˣ��������ʹ�õ������������� UpdateFunctionOnGPU ����ʱִ�д˲�����
        // ���Ǳ����� DrawMeshInstancedProcedural �ṩ�����һ��������Ӧ�û��ƶ��ٸ�ʵ������Ӧ����λ�û������е�Ԫ��������ƥ�䣬���ǿ���ͨ����������Ի�ȡ��
        Graphics.DrawMeshInstancedProcedural(
			mesh, 0, material, bounds, resolution * resolution
		);
	}
}