using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using SkinnedModel;
using System.Timers;

namespace LebaneseKinect
{
    class DanceMove
    {
        String moveIconName;
        public String moveGender = "male";
        Texture2D moveIcon;
        public TimeSpan moveSpan;

        public DanceMove(TimeSpan moveSpan, string moveIconName)
        {
            // TODO: Complete member initialization
            this.moveSpan = moveSpan;
            this.moveIconName = moveIconName;
            this.completed = false;
        }

        public void LoadContent(ContentManager content)
        {
            try
            { moveIcon = content.Load<Texture2D>("MoveIcons\\" + moveGender + "_" + moveIconName); }
            catch (Exception e)
            {
                moveIcon = null;
                Console.WriteLine("Couldn't load the icon " + moveGender + "_" + moveIconName);
            }
        }

        public Texture2D GetMoveIcon()
        {
            return moveIcon;
        }

        public String GetName()
        {
            return moveIconName;
        }

        public bool completed { get; set; }
    }
}
