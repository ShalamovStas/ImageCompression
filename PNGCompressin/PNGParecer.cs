using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace PNGCompressin
{
    class PNGParecer
    {
        private string filePath;

        private int headerIndex = 8;
        public PNGParecer(string fileName)
        {
            this.filePath = fileName;
        }

        public void PrintAllBytes()
        {
            var bytes = File.ReadAllBytes(filePath);
            int lineCount = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                SetColor(i);
                Console.Write("{0:x2} ", bytes[i]);
                lineCount++;
                if (lineCount == 16)
                {
                    Console.WriteLine();
                    lineCount = 0;
                }
            }

            Console.WriteLine("=====");
            PrintChunks(bytes);
        }

        private void SetColor(int index)
        {
            if (index < headerIndex)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.White;

        }

        private void PrintChunks(byte[] bytes)
        {
            List<Chunk> list = new List<Chunk>();

            ReadChunk(bytes, headerIndex);

            //for (int i = headerIndex; i < bytes.Length; i++)
            //{
            //    Chunk chunk = new Chunk();
            //    if(bytes.Length < headerIndex)
            //    chunk.Length = bytes[i] + bytes[i + 1] + bytes[i + 2] + bytes[i+3];
            //
            //    i += 3;
            //
            //
            //    Console.Write("{0:x2} ", chunk.Length);
            //    break;
            //}

            //byte byte1 = 0x00;
            //byte byte2 = 0x00;
            //byte byte3 = 0x00;
            //byte byte4 = 0x0D;
            //
            //var res = byte1 + byte2 + byte3 + byte4;
            //Console.WriteLine($"res = {res}");
        }

        private Chunk ReadChunk(byte[] bytes, int startIndex)
        {
            var chunklengthLength = 4;
            var chunkTypeLength = 4;
            byte chunkDataLength = 0;
            var crcLength = 4;

            
            if (bytes.Length < startIndex + chunklengthLength + chunkTypeLength)
                throw new Exception();

            Console.WriteLine("Read chunk");
            for (int i = startIndex; i < startIndex + chunklengthLength; i++)
            {
                chunkDataLength += bytes[i];
                Console.Write("{0:x2} ", bytes[i]);
            }
            Console.WriteLine($"\nLength:{chunkDataLength}");

            if (bytes.Length < startIndex + chunklengthLength + chunkTypeLength + chunkDataLength + crcLength)
                throw new Exception();

            byte CType = 0;
            for (int i = startIndex + chunklengthLength; i < startIndex + chunklengthLength + chunkTypeLength; i++)
                CType += bytes[i];

            Console.WriteLine($"\nCType:{CType}");

            Console.WriteLine("Data");
            byte[] Data = new byte[chunkDataLength];
            var index = 0;
            for (int i = startIndex + chunklengthLength + chunkTypeLength; i < startIndex + chunklengthLength + chunkTypeLength + chunkDataLength; i++) {
                Data[index] = bytes[i];
                Console.Write("{0:x2} ", bytes[i]);
                index++;
            }
            Console.WriteLine();

            Console.WriteLine("Crc");
            byte[] Crc = new byte[crcLength];
            index = 0;
            for (int i = startIndex + chunklengthLength + chunkTypeLength + chunkDataLength; i < startIndex + chunklengthLength + chunkTypeLength + chunkDataLength + crcLength; i++)
            {
                Crc[index] = bytes[i];
                Console.Write("{0:x2} ", bytes[i]);
                index++;
            }


            Chunk chunk = new Chunk();
            chunk.Length = chunkDataLength;
            chunk.Data = Data;
            chunk.Crc32 = Crc;
            chunk.LastIndex = startIndex + chunklengthLength + chunkTypeLength + chunkDataLength + crcLength;
            return chunk;
        }
    }

    struct Chunk
    {
        public byte Length { get; set; }
        public string CType { get; set; }
        public byte[] Data { get; set; }
        public byte[] Crc32 { get; set; }
        public int LastIndex { get; set; }
    }
}
