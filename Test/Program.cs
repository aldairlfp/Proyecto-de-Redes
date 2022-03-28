using System.IO;

StreamReader stream;

try
{
    stream = new StreamReader("Test/bin/Debug/net6.0/config.txt");
    System.Console.WriteLine(stream.ReadToEnd());
}
catch (System.IO.FileNotFoundException)
{
    System.Console.WriteLine("No encontro el archivo");
}