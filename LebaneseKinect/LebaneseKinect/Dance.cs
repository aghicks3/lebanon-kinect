using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using SkinnedModel;
using System.Timers;
using Microsoft.Kinect;

namespace LebaneseKinect
{
    class Dance
    {
        List<DanceMove> m_danceMoves;
        List<DanceMove> m_scoreBlock;
        List<DanceMove> f_danceMoves;
        List<DanceMove> f_scoreBlock;

        String moviePath;
        Video movieFile;

        MoveChecker mc = new MoveChecker();

        public Dance(string p_moviePath)
        {
            m_danceMoves = new List<DanceMove>();
            m_scoreBlock = new List<DanceMove>();
            f_danceMoves = new List<DanceMove>();
            f_scoreBlock = new List<DanceMove>();
            this.moviePath = p_moviePath;

        }

        public Dance()
        {
            // TODO: Complete member initialization
        }

        public void LoadContent(ContentManager content)
        {
            this.movieFile = content.Load<Video>("Video\\" + moviePath);
            SetTimes();
            foreach (DanceMove move in m_danceMoves)
            {
                move.LoadContent(content);
            }
            foreach (DanceMove move in f_danceMoves)
            {
                move.LoadContent(content);
            }
            foreach (DanceMove move in m_scoreBlock)
            {
                move.LoadContent(content);
            }
            foreach (DanceMove move in f_scoreBlock)
            {
                move.LoadContent(content);
            }
        }

        public void AddDanceMove(char gender, TimeSpan moveSpan, string moveIcon)
        {
            DanceMove newMove = new DanceMove(moveSpan, moveIcon);
            if (moveIcon == "block")
            {
                newMove.moveGender = "neutral";

                if (gender == 'm')
                    m_scoreBlock.Add(newMove);
                else if (gender == 'f')
                    f_scoreBlock.Add(newMove);
            }
            else
            {
                if (gender == 'm')
                    m_danceMoves.Add(newMove);
                else if (gender == 'f')
                {
                    newMove.moveGender = "female";
                    f_danceMoves.Add(newMove);
                }
            }

        }

        public void Draw(TimeSpan currentTime, SpriteBatch sb)
        {
            Console.WriteLine("I am running this!");

            //go through dance moves (added in order) until we are past the scoring threshhold
            //draw them
            int WINDOW_WIDTH = 640;//720;
            int WINDOW_HEIGHT = 480;
            int maleRectDiff = WINDOW_WIDTH / 2 - 100; //(probably don't do this here... need some globals)
            TimeSpan thisBlock = new TimeSpan();
            foreach (DanceMove move in m_scoreBlock)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (Math.Abs(diff) < 2000)
                {
                    int xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                    if (diff > 0)
                    {
                        xlocation = Convert.ToInt32(0 + maleRectDiff);
                    }
                    if (move.GetMoveIcon() != null)
                        sb.Draw(move.GetMoveIcon(), new Rectangle(xlocation, WINDOW_HEIGHT - 250, 120, WINDOW_HEIGHT - 350), Color.White);
                }
            }
            foreach (DanceMove move in f_scoreBlock)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (Math.Abs(diff) < 2000)
                {
                    int xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                    if (diff > 0)
                    {
                        xlocation = Convert.ToInt32(500 - maleRectDiff);
                    }
                    if (move.GetMoveIcon() != null)
                        sb.Draw(move.GetMoveIcon(), new Rectangle(xlocation, WINDOW_HEIGHT - 250, 120, WINDOW_HEIGHT - 350), Color.White);
                }
            }

            foreach (DanceMove move in m_danceMoves)
            {
                if (move.completed) continue;
                
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (Math.Abs(diff) < 2000)
                {
                    int xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                    if (diff > 0)
                    {
                        xlocation = Convert.ToInt32(0 + maleRectDiff);
                    }
                    if(move.GetMoveIcon() != null)
                        sb.Draw(move.GetMoveIcon(), new Rectangle(xlocation, WINDOW_HEIGHT - 250, 120, WINDOW_HEIGHT - 350), Color.White);
                }
            }
            foreach (DanceMove move in f_danceMoves)
            {
                if (move.completed) continue;

                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (Math.Abs(diff) < 2000)
                {
                    int xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                    if (diff > 0)
                    {
                        xlocation = Convert.ToInt32(500 - maleRectDiff);
                    }
                    if (move.GetMoveIcon() != null)
                        sb.Draw(move.GetMoveIcon(), new Rectangle(xlocation, WINDOW_HEIGHT - 250, 120, WINDOW_HEIGHT - 350), Color.White);
                }
            }
        }

        public int ScoreMovesFake(TimeSpan currentTime)
        {
            int score = 0;
            //first find the current block. This is the next block in the list that is after the current time.
            /*
            TimeSpan thisBlock = new TimeSpan();
            foreach (DanceMove move in m_scoreBlock)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (diff < 0)
                {
                    move.completed = true;
                }
                else
                {
                    thisBlock = move.moveSpan;
                    break;
                }
            }*/

            //then, go through the moves in the current list and check for the move name, or if they're after the current block, don't bother checking
            foreach (DanceMove move in m_danceMoves)
            {
                //moves are in the list sorted by time, if we're past "thisblock" then we don't need to check any more moves
                if (move.completed)
                    continue;

                //if (move.moveSpan.CompareTo(thisBlock) >= 0)
                    //break;

                  if (Keyboard.GetState().IsKeyDown(Keys.Space))
                 {
                    double diff = Math.Abs(currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                    if (diff < 600)
                    {
                        move.completed = true;
                        score += (int)Math.Min(600 - diff, 400);
                        break;
                    }
                }
            }


            return score;
        }

        public int ScoreMoves(TimeSpan currentTime, Skeleton skeleton)
        {
            int score = 0;

            //first find the current block. This is the next block in the list that is after the current time.
            TimeSpan thisBlock = new TimeSpan();
            foreach (DanceMove move in m_scoreBlock)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (diff < -600)
                {
                    move.completed = true;
                }
                else
                {
                    thisBlock = move.moveSpan;
                    break;
                }
            }

            //then, go through the moves in the current list and check for the move name, or if they're after the current block, don't bother checking
            mc.UpdateSkeleton(skeleton);
            foreach (DanceMove move in m_scoreBlock)
            {
                //moves are in the list sorted by time, if we're past "thisblock" then we don't need to check any more moves
                if (move.completed)
                    continue;

                if (move.moveSpan.CompareTo(thisBlock) >= 0)
                    break;

                if (mc.CheckMove(move, currentTime) > -1)
                {
                    move.completed = true;
                }
            }


            return score;
        }

        //load hard-coded timings for dance 1
        public void SetTimes()
        {
            string dancename = "Dance1";
            StreamReader inp_stm = new StreamReader("DanceFiles//" + dancename + ".csv");

            while(!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine( );
                string[] inp_ar = inp_ln.Split(',');

                switch (inp_ar[0])
                {
                    case "M":
                        try
                        {
                            AddDanceMove('m', new TimeSpan(
                                Int32.Parse(inp_ar[2]), 
                                Int32.Parse(inp_ar[3]), 
                                Int32.Parse(inp_ar[4]), 
                                Int32.Parse(inp_ar[5]), 
                                Int32.Parse(inp_ar[6])), inp_ar[1].Trim());
                        }
                        catch (Exception e)
                        {

                        }
                        break;
                    case "N":
                        try
                        {
                            AddDanceMove('m', new TimeSpan(
                                Int32.Parse(inp_ar[2]),
                                Int32.Parse(inp_ar[3]),
                                Int32.Parse(inp_ar[4]),
                                Int32.Parse(inp_ar[5]),
                                Int32.Parse(inp_ar[6])), "block");

                            AddDanceMove('f', new TimeSpan(
                                Int32.Parse(inp_ar[2]),
                                Int32.Parse(inp_ar[3]),
                                Int32.Parse(inp_ar[4]),
                                Int32.Parse(inp_ar[5]),
                                Int32.Parse(inp_ar[6])), "block");
                        }
                        catch (Exception e)
                        {

                        }
                        break;
                }
            }

             inp_stm.Close( );  
        }

        public void SetTimesBeta()
        {

            AddDanceMove('m', new TimeSpan(0, 0, 0, 2, 731), "block");

            AddDanceMove('m', new TimeSpan(0, 0, 0, 6, 562), "LeftKneeLiftFaceRight");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 7, 602), "RightKneeLiftFaceRight");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 8, 574), "LeftKneeLiftFaceRight");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 9, 731), "RightKneeLift");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 9, 731), "block");

            AddDanceMove('f', new TimeSpan(0, 0, 0, 6, 562), "LeftKneeLift");
            AddDanceMove('f', new TimeSpan(0, 0, 0, 7, 602), "RightKneeLift");
            AddDanceMove('f', new TimeSpan(0, 0, 0, 8, 574), "LeftKneeLift");
            AddDanceMove('f', new TimeSpan(0, 0, 0, 9, 731), "RightKneeLift");

            AddDanceMove('m', new TimeSpan(0, 0, 0, 10, 451), "block");

        }

        //load timings from a file instead of from code
        public void SetTimes(string filePath)
        {

        }
    }
}
