namespace CPqDASR.ASR
{
    public interface IAudioSource
    {
        byte[] Read();

        void Close();

        void Finish();
    }
}
