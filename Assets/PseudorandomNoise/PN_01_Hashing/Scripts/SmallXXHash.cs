public readonly struct SmallXXHash {
    // ��һ�������� C# �ļ���Ϊ SmallXXHash ����һ���ṹ���͡�������ʾ�������ж������ uint ��������������������������ֱ�����Ϊ A �� E�����ڲ������ء���Щֵ�� Yann Collet ���ݾ���ѡ��ġ�
    const uint primeA = 0b10011110001101110111100110110001;
	const uint primeB = 0b10000101111010111100101001110111;
	const uint primeC = 0b11000010101100101010111000111101;
	const uint primeD = 0b00100111110101001110101100101111;
	const uint primeE = 0b00010110010101100110011110110001;

    // �㷨�Ĺ���ԭ���ǽ���ϣλ�洢���ۼ����У�Ϊ��������Ҫһ�� uint �ֶΡ���ֵ��ʼ��Ϊһ����������Ȼ���ټ������� E��
    // ���Ǵ�����ϣֵ�ĵ�һ�����������ͨ��һ���������Ӳ����Ĺ������췽������ɡ����ǽ�������Ϊ uint����������ͨ��ʹ�ô����ŵ����������ʹ�� int ���������㡣
    readonly uint accumulator;

	public SmallXXHash (uint accumulator) {
		this.accumulator = accumulator;
	}

	public static implicit operator SmallXXHash (uint accumulator) =>
		new SmallXXHash(accumulator);

	public static SmallXXHash Seed (int seed) => (uint)seed + primeE;

    // ��ֻ�� ��Eat���ĵ�һ���������ֵ��Eat ���뽫�ۼ�����λ������ת��������Ϊ�����һ��˽�о�̬�������������Ĳ����ƶ�һЩ���ݡ�����ʹ�� << ������������λ�����ƶ���
    static uint RotateLeft (uint data, int steps) =>
		(data << steps) | (data >> 32 - steps);

    // XXHash32 �Ĺ�����ʽ���� 32 λΪ��λ�������������롣���ǽ����һ�� SmallXXHash.Eat �������÷�����һ�� int �������������κ����ݡ�
    // ���ǻ��ٴν�����������Ϊ uint�������� C ��ˣ�Ȼ����ӵ��ۼ����С��⽫�����������������ûʲô����Ϊ���ǲ����������ݵ���ֵ���͡���ˣ���������ʵ���϶��� modulo 232��
    // ������ Eat Ϊ��λ���ۼ���������ת 17 λ��Burst Ҳ�������˷������ã���ֱ��ʹ�� 15 �������ƣ�ʡȥ�˳���������
    // Eat���̵����һ���ǽ��ۼ��������� D ��ˡ�
    public SmallXXHash Eat (int data) =>
		RotateLeft(accumulator + (uint)data * primeC, 17) * primeD;

    // ��Ȼ��������������Ǻܺã��� Eat �����Ѿ�����ˡ���Ȼ���ǲ����ڱ��̳���ʹ��������������Ҳ���һ�����ܵ��ֽڵı��� Eat��������Ϊ XXHash32 �Ը����ݴ�С�Ĵ������в�ͬ����������ת 11 ���������� 17 ������������ E �� A ��ˣ������������� C �� D ��ˡ�
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