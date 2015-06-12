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

        public TimeSpan compSpan;
        private Color scoreColor = Color.White;

        public DanceMove(TimeSpan moveSpan, string moveIconName)
        {
            // TODO: Complete member initialization
            this.moveSpan = moveSpan;
            this.moveIconName = moveIconName;
            this.completed = false;
            this.visible = true;
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

        public void Draw(SpriteBatch sb, int xlocation, TimeSpan currentTime)
        {
            if (!visible) return;

            int stretch = 0;
            float fadeOutAmt = 1.0f;
            if (completed)
            {
                stretch = (int)(currentTime.Subtract(compSpan).TotalMilliseconds / 5); //magic numbers: 500 milliseconds to grow 50 pixels

                fadeOutAmt = Math.Max((50.0f - stretch) / 50.0f , 0.0f);
                //scoreColor.A = (byte)Math.Max((125 * fadeOutAmt), 0);
                //scoreColor = scoreColor * Math.Max(fadeOutAmt, 0);
                if (stretch > 50)
                {
                    visible = false;
                    return;
                }
            }
            double fadeIn = currentTime.Subtract(moveSpan).TotalMilliseconds + 1200.0f;
            if (fadeIn < 0)
                fadeOutAmt = (float)(800.0f + fadeIn) / 800.0f; //magic numbers!
            
            if (GetMoveIcon() != null)
                sb.Draw(GetMoveIcon(), new Rectangle(xlocation - stretch, GLOBALS.WINDOW_HEIGHT - (101 + stretch), 100 + (2 * stretch), GLOBALS.WINDOW_HEIGHT - (380 - (2 * stretch))), scoreColor * fadeOutAmt);

                //sb.Draw(GetMoveIcon(), new Rectangle(xlocation - stretch, GLOBALS.WINDOW_HEIGHT - (131 + stretch), 120 + (2 * stretch), GLOBALS.WINDOW_HEIGHT - (350 - (2 * stretch))), scoreColor * fadeOutAmt);

                //sb.Draw(GetMoveIcon(), new Rectangle(xlocation - stretch, GLOBALS.WINDOW_HEIGHT - (160 + stretch), 120 + (2 * stretch), GLOBALS.WINDOW_HEIGHT - (350 - (2 * stretch))), scoreColor * fadeOutAmt);
        }

        public void ScoreMove(TimeSpan currentTime)
        {
            ScoreMove(currentTime, true);
        }

        public void ScoreMove(TimeSpan currentTime, Boolean changecolor)
        {
            if (completed) return;
            if (changecolor)
            {
                double diff = Math.Abs((currentTime.Subtract(moveSpan).TotalMilliseconds));
                if (diff < 150)
                    scoreColor = Color.CornflowerBlue;
                else if (diff < 300)
                    scoreColor = Color.Green;
                else if (diff < 600)
                    scoreColor = Color.Gold;
                else
                    scoreColor = Color.Red;
            }

            compSpan = currentTime;
            completed = true;
            return;
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
        public bool visible { get; set; }
    }
}
