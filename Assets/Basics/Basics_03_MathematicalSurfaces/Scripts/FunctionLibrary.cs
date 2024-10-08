using UnityEngine;

using static UnityEngine.Mathf;

public static class FunctionLibrary {

	public delegate Vector3 Function (float u, float v, float t);

	public enum FunctionName { Wave, MultiWave, Ripple, Sphere, Torus }

	static Function[] functions = { Wave, MultiWave, Ripple, Sphere, Torus };

	public static Function GetFunction (FunctionName name) {
		return functions[(int)name];
	}

    /// <summary>
    /// �ڲ�������ʹ�� Z ����򵥷�����ͬʱʹ�� X �� Z �ĺͣ�������ֻʹ�� X��
    /// f(x,t) = sin(��*(x + t))
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
	public static Vector3 Wave (float u, float v, float t) {
		Vector3 p;
		p.x = u;
		p.y = Sin(PI * (u + v + t));
		p.z = v;
		return p;
	}

    /// <summary>
    /// Ҫ�������Ҳ��ĸ����ԣ���򵥵ķ�����������һ��Ƶ�ʼӱ������Ҳ�������ζ�����ı仯�ٶ���ԭ���������������ǽ����Һ����Ĳ������� 2��
    /// �����������Ҳ�����״������Ҳ���ͬ������С���롣
    /// f(x,t) = sin(��*(x + t)) + (sin(2��*(x + t)) / 2)
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
	public static Vector3 MultiWave (float u, float v, float t) {
		Vector3 p;
		p.x = u;
        // �� MultiWave ��ֱ�ӵ��޸ľ�����ÿ������ʹ��һ��������ά�ȡ��ý�С�Ĳ�ʹ�� Z ά��
        // ���ǻ��������һ���� XZ �Խ����н��ĵ�������������ʹ���� ������ ����ͬ�Ĳ��ˣ�ֻ�ǽ�ʱ��������ķ�֮һ��Ȼ�󽫽������ 2.5��ʹ�䱣���� -1-1 �ķ�Χ�ڡ�
        p.y = Sin(PI * (u + 0.5f * t));
		p.y += 0.5f * Sin(2f * PI * (v + t));
		p.y += Sin(PI * (u + v + 0.25f * t));
		p.y *= 1f / 2.5f;
		p.z = v;
		return p;
	}

	public static Vector3 Ripple (float u, float v, float t) {
		float d = Sqrt(u * u + v * v);
		Vector3 p;
		p.x = u;
        // ���ǽ�������Ϊ���Һ��������룬��������Ϊ�����������˵�����ǽ�ʹ�� y=sin(4��d)��d=|x|���������ƾͻ���ͼ�ε����ڶ�����²�����
        // ���� Y �ı仯̫��������Ӿ��Ϻ��ѽ����������ǿ���ͨ����С���������������������������Ƶ���������̶����������ž�������Ӷ���С��
        // ��ˣ����ǽ�����ת��Ϊ y=sin(4��d) / (1+10d)��
        p.y = Sin(PI * (4f * d - t));
		p.y /= 1f + 10f * d;
		p.z = v;
		return p;
	}

	public static Vector3 Sphere (float u, float v, float t) {
		float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t));
		float s = r * Cos(0.5f * PI * v);
		Vector3 p;
		p.x = s * Sin(PI * u);
		p.y = r * Sin(0.5f * PI * v);
		p.z = s * Cos(PI * u);
		return p;
	}

	public static Vector3 Torus (float u, float v, float t) {
		float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
		float r2 = 0.15f + 0.05f * Sin(PI * (8f * u + 4f * v + 2f * t));
		float s = r1 + r2 * Cos(PI * v);
		Vector3 p;
		p.x = s * Sin(PI * u);
		p.y = r2 * Sin(PI * v);
		p.z = s * Cos(PI * u);
		return p;
	}
}