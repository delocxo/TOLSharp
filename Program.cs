namespace TOLSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: TOLSharp <filepath.tol>");
                Environment.Exit(1);
            }

            Intepreter.Run(args[0]);
        }
    }
}
