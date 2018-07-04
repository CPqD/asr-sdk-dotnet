using CPqDASR.Recognizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CPqDASRUnitTest.Recognizer
{
    [TestClass]
    public class BufferAudioSourceUnitTest
    {
        [TestMethod]
        public void TestReadWrightWait()
        {
            //Creates the source that contains buffer which producer will wright and consumer will read
            Queue<byte[]> originSource = ReadFileInChunks(TestsReferences.AudioPizzaVeg, out int originalLength);

            //BufferAudioSource object, that reperesents our circular buffer source.
            BufferAudioSource bufferAudioSource = new BufferAudioSource();

            bool isToContinue = true;

            //Producer thread
            Thread producerThread = new Thread(() =>
            {
                while (originSource.Count > 0)
                {
                    bufferAudioSource.Write(originSource.Dequeue());
                    Thread.Sleep(100);
                }

                isToContinue = false;
                bufferAudioSource.Finish();
            });

            //Counter that validates whether the read method awaits the producer or not
            int iterations = 0;

            //Flag that indicates the thread was released
            bool isReadReleased = false;

            //Consumer thread
            Thread consumerThread = new Thread(() =>
            {
                //Consuming the buffer of audio source
                while (true)
                {
                    if (isToContinue)
                    {
                        byte[] localbytes = bufferAudioSource.Read();

                        if (localbytes != null && localbytes.Length > 0)
                        {
                            iterations++;
                        }
                       
                    }
                    else
                    {
                        break;
                    }
                }
            });

            //Start the producer thread
            producerThread.Start();

            //Start the consumer thread
            consumerThread.Start();

            while (iterations < originalLength)
            {
                Thread.Sleep(1000);
            }

            Thread.Sleep(1000);

            byte[] bytes = bufferAudioSource.Read();

            if (bytes?.Length == 0)
            {
                isReadReleased = true;
            }

            Assert.IsTrue(iterations == originalLength && originSource.Count == 0 && isReadReleased);
        }

        private Queue<byte[]> ReadFileInChunks(String path, out int queueLength)
        {
            Queue<byte[]> source = new Queue<byte[]>();

            byte[] fileByte = File.ReadAllBytes(path);

            int incomingOffset = 0;

            while (incomingOffset < fileByte.Length)
            {
                int length = Math.Min(3200, fileByte.Length - incomingOffset);

                byte[] chunck = new byte[length];

                Buffer.BlockCopy(fileByte, incomingOffset, chunck, 0, length);

                incomingOffset += length;

                // Transmit outbound buffer
                source.Enqueue(chunck);
            }
            queueLength = source.Count;
            return source;
        }
    }
}
