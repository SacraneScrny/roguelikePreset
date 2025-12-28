namespace Sackrany.CustomRandom.Interfaces
{
    public interface IRandom
    {
        public uint GetState { get; }
        public void Init(uint seed);

        public uint Next();
        public int Next(int max);
        public int Next(int min, int max);

        public float NextFloat();
        public float NextFloat(float max);
        public float NextFloat(float min, float max);

        public void NextArray(ref int[] array, int min, int max);
        public void NextArrayFloat(ref float[] array, float min, float max);
    }
}