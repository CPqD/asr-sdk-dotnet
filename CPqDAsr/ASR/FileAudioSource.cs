using System;
using System.Collections.Generic;
using System.IO;

namespace CPqDASR.ASR
{
    public class FileAudioSource : IAudioSource
    {
        private Queue<byte[]> Buffer { get; set; }

        internal FileAudioSource()
        {
            Buffer = new Queue<byte[]>();
        }

        public FileAudioSource(string Path)
            : this()
        {
            byte[] fileByte = File.ReadAllBytes(Path);

            int incomingOffset = 0;

            while (incomingOffset < fileByte.Length)
            {
                byte[] chunck = new byte[4096];

                int length = Math.Min(chunck.Length, fileByte.Length - incomingOffset);

                System.Buffer.BlockCopy(fileByte, incomingOffset, chunck, 0, length);

                incomingOffset += length;

                // Transmit outbound buffer
                Buffer.Enqueue(chunck);
            }
        }

        public FileAudioSource(byte[] Buffer)
            : this()
        {
            this.Buffer.Enqueue(Buffer);
        }

        public byte[] Read()
        {
            if (Buffer.Count > 0)
                return Buffer.Dequeue();
            return new byte[0];
        }

        public void Close()
        {
            Buffer = null;
        }

        public void Finish()
        {
            Close();
        }
    }
}
