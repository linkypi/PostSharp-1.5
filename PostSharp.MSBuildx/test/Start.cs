using System;

namespace PostSharpTest
{
    class Start
    {
        static void Main()
        {
            Order("lynch");

            TextException();

            Console.ReadKey();
        }

        [Log]
        private static void Order(string name)
        {
            Console.WriteLine("{0} order some books....", name);
        }

        [Exception("myexception")]
        private static void TextException()
        {
            Console.WriteLine("this will throw an exception ...");
            throw new Exception("stack overflow !!!");
        }
    }
}
