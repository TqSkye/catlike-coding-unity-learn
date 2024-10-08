public readonly struct SmallXXHash {
    // 在一个单独的 C# 文件中为 SmallXXHash 创建一个结构类型。如下所示，在其中定义五个 uint 常量。这是五个二进制素数，分别命名为 A 至 E，用于操作比特。这些值是 Yann Collet 根据经验选择的。
    const uint primeA = 0b10011110001101110111100110110001;
	const uint primeB = 0b10000101111010111100101001110111;
	const uint primeC = 0b11000010101100101010111000111101;
	const uint primeD = 0b00100111110101001110101100101111;
	const uint primeE = 0b00010110010101100110011110110001;

    // 算法的工作原理是将哈希位存储在累加器中，为此我们需要一个 uint 字段。该值初始化为一个种子数，然后再加上质数 E。
    // 这是创建哈希值的第一步，因此我们通过一个带有种子参数的公共构造方法来完成。我们将种子视为 uint，但代码中通常使用带符号的整数，因此使用 int 参数更方便。
    readonly uint accumulator;

	public SmallXXHash (uint accumulator) {
		this.accumulator = accumulator;
	}

	public static implicit operator SmallXXHash (uint accumulator) =>
		new SmallXXHash(accumulator);

	public static SmallXXHash Seed (int seed) => (uint)seed + primeE;

    // 这只是 “Eat”的第一步。添加数值后，Eat 必须将累加器的位向左旋转。让我们为此添加一个私有静态方法，按给定的步长移动一些数据。首先使用 << 操作符将所有位向左移动。
    static uint RotateLeft (uint data, int steps) =>
		(data << steps) | (data >> 32 - steps);

    // XXHash32 的工作方式是以 32 位为单位，并行消耗输入。我们将添加一个 SmallXXHash.Eat 方法，该方法有一个 int 参数，不返回任何内容。
    // 我们会再次将输入数据视为 uint，与素数 C 相乘，然后将其加到累加器中。这将导致整数溢出，但这没什么，因为我们并不关心数据的数值解释。因此，所有运算实际上都是 modulo 232。
    // 现在以 Eat 为单位将累加器向左旋转 17 位。Burst 也将内联此方法调用，并直接使用 15 进行右移，省去了常数减法。
    // Eat过程的最后一步是将累加器与质数 D 相乘。
    public SmallXXHash Eat (int data) =>
		RotateLeft(accumulator + (uint)data * primeC, 17) * primeD;

    // 虽然结果看起来还不是很好，但 Eat 方法已经完成了。虽然我们不会在本教程中使用它，但让我们也添加一个接受单字节的变种 Eat方法，因为 XXHash32 对该数据大小的处理略有不同：它向左旋转 11 步，而不是 17 步，并与素数 E 和 A 相乘，而不是与素数 C 和 D 相乘。
    public SmallXXHash Eat (byte data) =>
		RotateLeft(accumulator + data * primeE, 11) * primeA;

	public static implicit operator uint (SmallXXHash hash) {
		uint avalanche = hash.accumulator;
		avalanche ^= avalanche >> 15;
		avalanche *= primeB;
		avalanche ^= avalanche >> 13;
		avalanche *= primeC;
		avalanche ^= avalanche >> 16;
		return avalanche;
	}
}