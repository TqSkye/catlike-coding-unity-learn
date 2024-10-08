using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour {

	[SerializeField]
	TextMeshProUGUI display;

	public enum DisplayMode { FPS, MS }

	[SerializeField]
	DisplayMode displayMode = DisplayMode.FPS;

	[SerializeField, Range(0.1f, 2f)]
	float sampleDuration = 1f;

	int frames;

	float duration, bestDuration = float.MaxValue, worstDuration;

	void Update () {
		float frameDuration = Time.unscaledDeltaTime;
		frames += 1;
		duration += frameDuration;
        /*
            平均帧频会波动，因为我们应用程序的性能并不是恒定不变的。它有时会变慢，要么是因为暂时有更多工作要做，要么是因为同一台机器上运行的其他进程妨碍了它。
            为了了解这些波动有多大，我们还将记录并显示样本期间出现的最佳和最差帧持续时间。将最佳持续时间默认设置为 float.MaxValue，这是可能的最差最佳持续时间。
            每次更新都会检查当前帧的持续时间是否小于迄今为止的最佳持续时间。如果是，则将其作为新的最佳持续时间。同时检查当前帧的持续时间是否大于迄今为止最差的持续时间。如果是，则将其设为新的最差持续时间。
         */
        if (frameDuration < bestDuration) {
			bestDuration = frameDuration;
		}
		if (frameDuration > worstDuration) {
			worstDuration = frameDuration;
		}

		if (duration >= sampleDuration) {
			if (displayMode == DisplayMode.FPS) {
				display.SetText(
					"FPS\n{0:0}\n{1:0}\n{2:0}",
					1f / bestDuration,
					frames / duration,  // 平均帧率
					1f / worstDuration
				);
			}
			else {
				display.SetText(
					"MS\n{0:1}\n{1:1}\n{2:1}",
					1000f * bestDuration,
					1000f * duration / frames,
					1000f * worstDuration
				);
			}
			frames = 0;
			duration = 0f;
			bestDuration = float.MaxValue;
			worstDuration = 0f;
		}
	}
}