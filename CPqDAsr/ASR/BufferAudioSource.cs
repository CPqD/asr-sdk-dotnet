using CPqDASR.ASR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPqDAsr.ASR
{
    /// <summary>
    /// 
    /// </summary>
    public class BufferAudioSource : IAudioSource
    {
        private Queue<byte[]> buffer;

        private bool finished;

        public BufferAudioSource()
        {
            buffer = new Queue<byte[]>(); 
        }

        public BufferAudioSource(byte[] bytes) : this()
        {
            buffer.Enqueue(bytes);
        }

        public bool Write(byte[] bytes)
        {
            if (!finished)
            {
                buffer.Enqueue(bytes);
                return true;
            }
            else
            {
                return false;
            }
        }

        public byte[] Read()
        {
            if (buffer.Count > 0)
            {
                return buffer.Dequeue();
            }

            return null;
        }

        public void Close()
        {
            buffer = null;
        }

        public void Finish()
        {
            finished = true;
            Close();
        }
    }
}
