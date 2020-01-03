using System;
using System.IO;

namespace PNGCompressin
{
    class Program
    {
        static void Main(string[] args)
        {

            PNGParecer parecer = new PNGParecer("../../../demo.png");
            parecer.PrintAllBytes();
            Console.ReadKey();
        }
    }
}
