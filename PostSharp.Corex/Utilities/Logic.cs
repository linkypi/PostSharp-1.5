namespace PostSharp.Utilities
{
    internal sealed class Logic
    {
        public static bool Implies( bool a, bool b )
        {
            return a ? b : true;
        }
    }
}