using System;

namespace LebaneseKinect
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (LebaneseKinectGame game = new LebaneseKinectGame())
            {
                game.Run();
            }
        }
    }
#endif
}

