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
            // �����λ���������ڸ��㾫��������ɵġ���ĳЩ����£���Ӧ������֮ǰ���������ջ�õ�������Сһ����ֵ���Ӷ�����ʵ����λ������������£����ǿ���������С������֮ǰ��� 0.00001 ����ƫ�������������⡣
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

    // �� OnEnable �г�ʼ���������ݡ���Ϊ���ǲ��Ὣ��ϣֵ�����������Կ����������������й�����ͬʱҲ����һ�����������Կ飬������ÿ�θ��¶�����
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
        // ������Ҫ����ɫ������ֱ�����˲����Էֱ��ʣ����Ҫ������������ǰ���������д洢�ֱ��ʼ��䵹����
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