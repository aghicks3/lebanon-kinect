using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LebaneseKinect
{
    static class GLOBALS
    {
        internal static int WINDOW_HEIGHT = 480;
        internal static int WINDOW_WIDTH = 640;

        internal static int EXCELLENT_WINDOW = 150;
        internal static int GREAT_WINDOW = 300;
        internal static int GOOD_WINDOW = 600;

        internal static int SCORING_WINDOW = 600;

        internal static bool PLAYER_ONE_ACTIVE = false;
        internal static bool PLAYER_TWO_ACTIVE = false;

        internal static StreamWriter writer;

        static GLOBALS()
        {
            writer = new StreamWriter("MCDebug.txt", false);
            writer.WriteLine("File created using StreamWriter class.");
        }
    }
}
