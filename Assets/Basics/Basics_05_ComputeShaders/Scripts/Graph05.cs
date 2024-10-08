using UnityEngine;

public class Graph05 : MonoBehaviour {

	[SerializeField]
	Transform pointPrefab;

	[SerializeField, Range(10, 200)]
	int resolution = 10;

	[SerializeField]
	FunctionLibrary05.FunctionName function;

	public enum TransitionMode { Cycle, Random }

	[SerializeField]
	TransitionMode transitionMode;

	[SerializeField, Min(0f)]
	float functionDuration = 1f, transitionDuration = 1f;

	Transform[] points;

	float duration;

	bool transitioning;

	FunctionLibrary05.FunctionName transitionFunction;

	void Awake () {
		float step = 2f / resolution;
		var scale = Vector3.one * step;
		points = new Transform[resolution * resolution];
		for (int i = 0; i < points.Length; i++) {
			Transform point = points[i] = Instantiate(pointPrefab);
			point.localScale = scale;
			point.SetParent(transform, false);
		}
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

		if (transitioning) {
			UpdateFunctionTransition();
		}
		else {
			UpdateFunction();
		}
	}

	void PickNextFunction () {
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary05.GetNextFunctionName(function) :
			FunctionLibrary05.GetRandomFunctionNameOtherThan(function);
	}

	void UpdateFunction () {
		FunctionLibrary05.Function f = FunctionLibrary05.GetFunction(function);
		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
			if (x == resolution) {
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			points[i].localPosition = f(u, v, time);
		}
	}

	void UpdateFunctionTransition () {
		FunctionLibrary05.Function
			from = FunctionLibrary05.GetFunction(transitionFunction),
			to = FunctionLibrary05.GetFunction(function);
		float progress = duration / transitionDuration;
		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
			if (x == resolution) {
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			points[i].localPosition = FunctionLibrary05.Morph(
				u, v, time, from, to, progress
			);
		}
	}
}