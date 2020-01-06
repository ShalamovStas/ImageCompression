using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using zlib;

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

        public void PrintByte(byte[] bytes)
        {
            if (bytes == null)
                return;

            int lineCount = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                Console.Write("{0:x2} ", bytes[i]);
                lineCount++;
                if (lineCount == 16)
                {
                    Console.WriteLine();
                    lineCount = 0;
                }
            }

            Console.WriteLine("\n=====");
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

            var firstChunk = ReadChunk(bytes, headerIndex);
            var index = headerIndex;
            while (true)
            {
                if (index == bytes.Length)
                    break;
                Chunk chunk = new Chunk();
                chunk = ReadChunk(bytes, index);
                list.Add(chunk);
                index = chunk.LastIndex;

            }

            

            PNG png = new PNG();
            png.Chunks = list;

            for (int i = 0; i < firstChunk.Data.Length; i++)
            {
                if (i < 4)
                {
                    png.Width += firstChunk.Data[i];
                    continue;
                }
                if (i >= 4 && i < 8)
                {
                    png.Height += firstChunk.Data[i];
                    continue;
                }
                if (i == 8)
                    png.BitDepth = firstChunk.Data[i];
                if (i == 9)
                    png.ColorType = firstChunk.Data[i];
                if (i == 10)
                    png.CompressionMethod = firstChunk.Data[i];
                if (i == 11)
                    png.FilterMethod = firstChunk.Data[i];
                if (i == 12)
                    png.InterlaceMethod = firstChunk.Data[i];
            }

            Console.WriteLine("===IDAT===");
            var IDAT = list.Where(ch => ch.CType.Contains("IDAT")).FirstOrDefault();
            PrintByte(IDAT.Data);


            Console.WriteLine("=====Decompressed====");
            byte[] decompreseeedData;
            DecompressData(IDAT.Data, out decompreseeedData);

            PrintByte(decompreseeedData);


        }

        private Chunk ReadChunk(byte[] bytes, int startIndex)
        {
            var headerLength = 8;
            var chunklengthLength = 4;
            var chunkTypeLength = 4;
            byte chunkDataLength = 0;
            var crcLength = 4;


            if (bytes.Length < startIndex + chunklengthLength + chunkTypeLength)
                throw new Exception();

            for (int i = startIndex; i < startIndex + chunklengthLength; i++)
                chunkDataLength += bytes[i];

            if (bytes.Length < startIndex + chunklengthLength + chunkTypeLength + chunkDataLength + crcLength)
                throw new Exception();

            string CType;
            byte[] type = new byte[4];
            type = bytes.Skip(startIndex + 4).Take(4).ToArray();
            CType = System.Text.Encoding.UTF8.GetString(type);


            byte[] Data = new byte[chunkDataLength];
            var index = 0;
            for (int i = startIndex + chunklengthLength + chunkTypeLength; i < startIndex + chunklengthLength + chunkTypeLength + chunkDataLength; i++)
            {
                Data[index] = bytes[i];
                index++;
            }

            byte[] Crc = new byte[crcLength];
            index = 0;
            for (int i = startIndex + chunklengthLength + chunkTypeLength + chunkDataLength; i < startIndex + chunklengthLength + chunkTypeLength + chunkDataLength + crcLength; i++)
            {
                Crc[index] = bytes[i];
                index++;
            }


            Chunk chunk = new Chunk();
            chunk.Length = chunkDataLength;
            chunk.Data = Data;
            chunk.Crc32 = Crc;
            chunk.CType = CType;
            chunk.LastIndex = startIndex + chunklengthLength + chunkTypeLength + chunkDataLength + crcLength;
            return chunk;
        }

        public static void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_DEFAULT_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
    }

    class Chunk
    {
        public byte Length { get; set; }
        public string CType { get; set; }
        public byte[] Data { get; set; }
        public byte[] Crc32 { get; set; }
        public int LastIndex { get; set; }
    }

    class PNG
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitDepth { get; set; }
        public int ColorType { get; set; }
        public int CompressionMethod { get; set; }
        public int FilterMethod { get; set; }
        public int InterlaceMethod { get; set; }
        public List<Chunk> Chunks { get; set; }
        public int NumberOfChunks { get; set; }
    }
}
