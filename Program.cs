using System.Runtime.InteropServices;

namespace HeimdallUpdate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Heimdall Updater v1.0");

            // Get the current path
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }
}