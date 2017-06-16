using System;

namespace PostSharp.Samples.Simplest
{
    [SimplestTrace]
    public class Speech
    {
        private static void Say(string what)
        {
            Console.WriteLine(what);
        }

        public void SayHello()
        {
            Say("Hello.");
        }

        public void SayBye()
        {
            Say("Bye.");
        }
    }
}