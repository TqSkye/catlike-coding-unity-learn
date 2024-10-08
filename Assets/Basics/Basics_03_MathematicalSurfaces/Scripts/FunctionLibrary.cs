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
    /// 在波函数中使用 Z 的最简单方法是同时使用 X 和 Z 的和，而不是只使用 X。
    /// f(x,t) = sin(π*(x + t))
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
    /// 要增加正弦波的复杂性，最简单的方法就是增加一个频率加倍的正弦波。这意味着它的变化速度是原来的两倍，方法是将正弦函数的参数乘以 2。
    /// 这样，新正弦波的形状与旧正弦波相同，但大小减半。
    /// f(x,t) = sin(π*(x + t)) + (sin(2π*(x + t)) / 2)
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="t"></param>
    /// <returns></returns>
	public static Vector3 MultiWave (float u, float v, float t) {
		Vector3 p;
		p.x = u;
        // 对 MultiWave 最直接的修改就是让每个波形使用一个单独的维度。让较小的波使用 Z 维。
        // 我们还可以添加一个沿 XZ 对角线行进的第三波。让我们使用与 “波浪 ”相同的波浪，只是将时间减慢到四分之一。然后将结果除以 2.5，使其保持在 -1-1 的范围内。
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
        // 我们将距离作为正弦函数的输入，并将其作为结果。具体来说，我们将使用 y=sin(4πd)，d=|x|，这样波纹就会在图形的域内多次上下波动。
        // 由于 Y 的变化太大，因此在视觉上很难解读结果。我们可以通过减小波的振幅来减少这种情况。但波纹的振幅并不固定，它会随着距离的增加而减小。
        // 因此，我们将函数转化为 y=sin(4πd) / (1+10d)。
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