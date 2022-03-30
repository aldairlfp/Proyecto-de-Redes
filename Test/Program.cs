using System;
using System.IO;

StreamReader stream;

System.Console.WriteLine(Environment.CurrentDirectory);

try
{
    stream = new StreamReader("script.txt");
    Console.WriteLine(stream.ReadToEnd());
}
catch (System.IO.FileNotFoundException)
{
    Console.WriteLine("No encontro el archivo");
}