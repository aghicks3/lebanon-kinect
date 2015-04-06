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
        Texture2D moveIcon;
        public TimeSpan moveSpan;

        private static ContentManager myContent;

        public DanceMove(TimeSpan moveSpan, string moveIconName)
        {
            // TODO: Complete member initialization
            this.moveSpan = moveSpan;
            this.moveIconName = moveIconName;
        }

        public void LoadContent(ContentManager content)
        {      
            moveIcon = content.Load<Texture2D>("Sprites\\female_crossover");
        }

        public Texture2D GetMoveIcon()
        {
            return moveIcon;
        }
    }
}
