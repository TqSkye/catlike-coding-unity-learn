using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour {

	[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
	struct HashJob : IJobFor {

		[WriteOnly]
		public NativeArray<uint> hashes;

		public int resolution;

		public float invResolution;

		public SmallXXHash hash;
        
		public void Execute(int i) {
            // 网格错位错误是由于浮点精度限制造成的。在某些情况下，在应用下限之前，我们最终会得到比整数小一点点的值，从而导致实例错位。在这种情况下，我们可以在舍弃小数部分之前添加 0.00001 的正偏置来解决这个问题。
            int v = (int)floor(invResolution * i + 0.00001f);
			int u = i - resolution * v - resolution / 2;
			v -= resolution / 2;

			hashes[i] = hash.Eat(u).Eat(v);
		}
	}

	static int
		hashesId = Shader.PropertyToID("_Hashes"),
		configId = Shader.PropertyToID("_Config");

	[SerializeField]
	Mesh instanceMesh;

	[SerializeField]
	Material material;

	[SerializeField, Range(1, 512)]
	int resolution = 16;

	[SerializeField, Range(-2f, 2f)]
	float verticalOffset = 1f;

	[SerializeField]
	int seed;

	NativeArray<uint> hashes;

	ComputeBuffer hashesBuffer;

	MaterialPropertyBlock propertyBlock;

    // 在 OnEnable 中初始化所有内容。因为我们不会将哈希值动画化，所以可以在这里立即运行工作，同时也可以一次性配置属性块，而不是每次更新都做。
	void OnEnable () {
		int length = resolution * resolution;
		hashes = new NativeArray<uint>(length, Allocator.Persistent);
		hashesBuffer = new ComputeBuffer(length, 4);

		new HashJob {
			hashes = hashes,
			resolution = resolution,
			invResolution = 1f / resolution,
			hash = SmallXXHash.Seed(seed)
		}.ScheduleParallel(hashes.Length, resolution, default).Complete();

		hashesBuffer.SetData(hashes);

		propertyBlock ??= new MaterialPropertyBlock();
		propertyBlock.SetBuffer(hashesId, hashesBuffer);
        // 我们需要在着色器中与分辨率相乘并除以分辨率，因此要在配置向量的前两个分量中存储分辨率及其倒数。
        propertyBlock.SetVector(configId, new Vector4(
			resolution, 1f / resolution, verticalOffset / resolution
		));
	}

	void OnDisable () {
		hashes.Dispose();
		hashesBuffer.Release();
		hashesBuffer = null;
	}

	void OnValidate () {
		if (hashesBuffer != null && enabled) {
			OnDisable();
			OnEnable();
		}
	}

	void Update () {
		Graphics.DrawMeshInstancedProcedural(
			instanceMesh, 0, material, new Bounds(Vector3.zero, Vector3.one),
			hashes.Length, propertyBlock
		);
	}
}