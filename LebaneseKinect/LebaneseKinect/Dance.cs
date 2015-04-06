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
    class Dance
    {
        List<DanceMove> m_danceMoves;
        List<DanceMove> m_scoreBlock;
        List<DanceMove> f_danceMoves;
        List<DanceMove> f_scoreBlock;

        String moviePath;
        Video movieFile;

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
            //this.movieFile = content.Load<Video>("Video\\" + moviePath);
            SetTimes();
            foreach (DanceMove move in m_danceMoves)
            {
                move.LoadContent(content);
            }
            foreach (DanceMove move in f_danceMoves)
            {
                move.LoadContent(content);
            }
        }

        public void AddDanceMove(char gender, TimeSpan moveSpan, string moveIcon)
        {
            DanceMove newMove = new DanceMove(moveSpan, moveIcon);
            if (moveIcon == "block")
            {
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
                    f_danceMoves.Add(newMove);
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

            foreach (DanceMove move in m_danceMoves)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (Math.Abs(diff) < 2000)
                {
                    int xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                    if (diff > 0)
                    {
                        xlocation = Convert.ToInt32(0 + maleRectDiff);
                    }
                    sb.Draw(move.GetMoveIcon(), new Rectangle(xlocation, WINDOW_HEIGHT - 250, 120, WINDOW_HEIGHT - 350), Color.White);
                }
            }
            foreach (DanceMove move in f_danceMoves)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (Math.Abs(diff) < 2000)
                {
                    int xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                    if (diff > 0)
                    {
                        xlocation = Convert.ToInt32(500 - maleRectDiff);
                    }
                    sb.Draw(move.GetMoveIcon(), new Rectangle(xlocation, WINDOW_HEIGHT - 250, 120, WINDOW_HEIGHT - 350), Color.White);
                }
            }
        }

        public int ScoreMoves(TimeSpan currentTime, string movename)
        {
            int score = 0;

            //first find the current block. This is the next block in the list that is after the current time.
            TimeSpan thisBlock;
            foreach (DanceMove move in m_scoreBlock)
            {
                double diff = (currentTime.Subtract(move.moveSpan).TotalMilliseconds);
                if (diff < 0)
                {
                    thisBlock = move.moveSpan;
                    break;
                }
            }

            //then, go through the moves in the current list and check for the move name, or if they're after the current block, don't bother checking



            return score;
        }

        //load hard-coded timings for dance 1
        public void SetTimes()
        {
            //Score block 1
            //LeftKneeLift1
            AddDanceMove('m', new TimeSpan(0, 0, 0, 6, 562), "LeftKneeLift");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 7, 602), "RightKneeLift");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 8, 574), "LeftKneeLift");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 9, 731), "RightKneeLift");

            AddDanceMove('f', new TimeSpan(0, 0, 0, 6, 562), "FemLeftKneeLift");
            AddDanceMove('f', new TimeSpan(0, 0, 0, 7, 602), "FemRightKneeLift");
            AddDanceMove('f', new TimeSpan(0, 0, 0, 8, 574), "FemLeftKneeLift");
            AddDanceMove('f', new TimeSpan(0, 0, 0, 9, 731), "FemRightKneeLift");
            AddDanceMove('m', new TimeSpan(0, 0, 0, 10, 451), "block");

            /*
            //Second score block
            LeftKneeLift3 AddDanceMove('m', newTimeSpan(0, 0, 0, 10, 731);
            RightKneeLift3 AddDanceMove('m', newTimeSpan(0, 0, 0, 11, 843);
            LeftKneeLiftAndFrontTorso1 AddDanceMove('m', newTimeSpan(0, 0, 0, 12, 951);
            RightKneeLiftAndBackTorso1 AddDanceMove('m', newTimeSpan(0, 0, 0, 14, 172);
            LeftKneeLift4 AddDanceMove('m', newTimeSpan(0, 0, 0, 15, 146);
            RightKneeLift4 AddDanceMove('m', newTimeSpan(0, 0, 0, 16, 169);
            LeftKneeLiftAndFrontTorso2 AddDanceMove('m', newTimeSpan(0, 0, 0, 17, 170);
            RightKneeLiftAndBackTorso2 AddDanceMove('m', newTimeSpan(0, 0, 0, 18, 294);

            FemLeftKneeLift3 AddDanceMove('m', newTimeSpan(0, 0, 0, 10, 731);
            FemRightKneeLift3 AddDanceMove('m', newTimeSpan(0, 0, 0, 11, 843);
            FemLeftFootLiftAndFrontTorso1 AddDanceMove('m', newTimeSpan(0, 0, 0, 12, 951);
            FemRightFootLiftAndBackTorso1 AddDanceMove('m', newTimeSpan(0, 0, 0, 14, 172);
            FemLeftKneeLift4 AddDanceMove('m', newTimeSpan(0, 0, 0, 15, 146);
            FemRightKneeLift4 AddDanceMove('m', newTimeSpan(0, 0, 0, 16, 169);
            FemLeftFootLiftAndFrontTorso2 AddDanceMove('m', newTimeSpan(0, 0, 0, 17, 170);
            FemRightFootLiftAndBackTorso2 AddDanceMove('m', newTimeSpan(0, 0, 0, 18, 294);
            Score2 AddDanceMove('m', newTimeSpan(0, 0, 0, 18, 546);

            //Score block 3
            LeftKneeLiftAndLeftHand1 AddDanceMove('m', newTimeSpan(0, 0, 0, 19, 355);
            RightKneeLiftAndLeftHand1 AddDanceMove('m', newTimeSpan(0, 0, 0, 20, 311);
            LeftKneeLiftAndFrontTorsoAndLeftHand1 AddDanceMove('m', newTimeSpan(0, 0, 0, 21, 427);
            RightKneeLiftAndBackTorsoAndLeftHand1 AddDanceMove('m', newTimeSpan(0, 0, 0, 22, 556);
            LeftKneeLiftAndLeftHand2 AddDanceMove('m', newTimeSpan(0, 0, 0, 23, 512);
            RightKneeLiftAndLeftHand2 AddDanceMove('m', newTimeSpan(0, 0, 0, 24, 595);
            LeftKneeLiftAndFrontTorsoAndLeftHand2 AddDanceMove('m', newTimeSpan(0, 0, 0, 25, 654);
            RightKneeLiftAndBackTorsoAndLeftHand2 AddDanceMove('m', newTimeSpan(0, 0, 0, 26, 814);
            //LeftKneeLift11 and LeftHand1 AddDanceMove('m', newTimeSpan(0,0,0,27,859); We're ignoring this step for now.

            FemHandSwingFront1 AddDanceMove('m', newTimeSpan(0, 0, 0, 19, 492);
            FemHandSwingRight1 AddDanceMove('m', newTimeSpan(0, 0, 0, 20, 615);
            FemHipShakeBack1 AddDanceMove('m', newTimeSpan(0, 0, 0, 21, 151);
            FemHandSwingBack1 AddDanceMove('m', newTimeSpan(0, 0, 0, 23, 834);
            FemHandSwingLeft1 AddDanceMove('m', newTimeSpan(0, 0, 0, 25, 046);
            FemHipShakeFront1 AddDanceMove('m', newTimeSpan(0, 0, 0, 25, 479);
            Score3 AddDanceMove('m', newTimeSpan(0, 0, 0, 28, 343);

            //Score block 4
            KneelDownsAndClap AddDanceMove('m', newTimeSpan(0, 0, 0, 28, 785);

            FemHandSwingFront2 AddDanceMove('m', newTimeSpan(0, 0, 0, 28, 192);
            FemHandSwingRight2 AddDanceMove('m', newTimeSpan(0, 0, 0, 29, 318);
            FemHipShakeBack2 AddDanceMove('m', newTimeSpan(0, 0, 0, 29, 707);
            FemHandSwingBack2 AddDanceMove('m', newTimeSpan(0, 0, 0, 32, 517);
            FemHandSwingLeft2 AddDanceMove('m', newTimeSpan(0, 0, 0, 33, 562);
            FemHipShakeFront2 AddDanceMove('m', newTimeSpan(0, 0, 0, 34, 045);
            Score4 AddDanceMove('m', newTimeSpan(0, 0, 0, 35, 806);

            //Score block 5
            LeftKneeLift5 AddDanceMove('m', newTimeSpan(0, 0, 0, 36, 590);
            RightKneeLift5 AddDanceMove('m', newTimeSpan(0, 0, 0, 37, 460);
            LeftKneeLift6 AddDanceMove('m', newTimeSpan(0, 0, 0, 38, 574);
            RightKneeLift6 AddDanceMove('m', newTimeSpan(0, 0, 0, 39, 558);
            LeftKneeLift7 AddDanceMove('m', newTimeSpan(0, 0, 0, 40, 706);
            RightKneeLift7 AddDanceMove('m', newTimeSpan(0, 0, 0, 41, 809);
            LeftKneeLift8 AddDanceMove('m', newTimeSpan(0, 0, 0, 42, 857);
            RightKneeLift8 AddDanceMove('m', newTimeSpan(0, 0, 0, 43, 916);

            FemMoveToRightAndScrollingHands1 AddDanceMove('m', newTimeSpan(0, 0, 0, 36, 228);
            FemMoveToRightAndScrollingHands2 AddDanceMove('m', newTimeSpan(0, 0, 0, 37, 496);
            FemCrouchAndHipShake1 AddDanceMove('m', newTimeSpan(0, 0, 0, 38, 552);
            FemMoveToLeftAndScrollingHands1 AddDanceMove('m', newTimeSpan(0, 0, 0, 40, 698);
            FemMoveToLeftAndScrollingHands2 AddDanceMove('m', newTimeSpan(0, 0, 0, 41, 670);
            FemCrouchAndHipShake2 AddDanceMove('m', newTimeSpan(0, 0, 0, 42, 728);
            Score5 AddDanceMove('m', newTimeSpan(0, 0, 0, 44, 494);

            //Score block 6
            MoveToRightAndWaiterHand AddDanceMove('m', newTimeSpan(0, 0, 0, 44, 751); //Just detect waiter hand bones?
            ShrugShoulders AddDanceMove('m', newTimeSpan(0, 0, 0, 46, 612);
            LeftKneeLift9 AddDanceMove('m', newTimeSpan(0, 0, 0, 49, 288);
            RightKneeLift9 AddDanceMove('m', newTimeSpan(0, 0, 0, 50, 273);
            LeftKneeLift10 AddDanceMove('m', newTimeSpan(0, 0, 0, 51, 299);
            RightKneeLift10 AddDanceMove('m', newTimeSpan(0, 0, 0, 52, 379);
            LeftKneeBendCrouch0B AddDanceMove('m', newTimeSpan(0, 0, 0, 53, 649);
            LeftKneeBendCrouch0A AddDanceMove('m', newTimeSpan(0, 0, 0, 54, 688);

            FemMoveToRightAndScrollingHands3 AddDanceMove('m', newTimeSpan(0, 0, 0, 44, 890);
            FemMoveToRightAndScrollingHands4 AddDanceMove('m', newTimeSpan(0, 0, 0, 45, 944);
            FemCrouchAndHipShake3 AddDanceMove('m', newTimeSpan(0, 0, 0, 47, 034);
            FemMoveToLeftAndScrollingHands3 AddDanceMove('m', newTimeSpan(0, 0, 0, 49, 112);
            FemMoveToLeftAndScrollingHands4 AddDanceMove('m', newTimeSpan(0, 0, 0, 50, 252);
            FemCrouchAndHipShake4 AddDanceMove('m', newTimeSpan(0, 0, 0, 51, 309);
            FemLeftKneeBendCrouch0B AddDanceMove('m', newTimeSpan(0, 0, 0, 53, 649);
            FemLeftKneeBendCrouch0A AddDanceMove('m', newTimeSpan(0, 0, 0, 54, 688);
            Score6 AddDanceMove('m', newTimeSpan(0, 0, 0, 53, 225);

            //Score block 7
            RightFootCross1 AddDanceMove('m', newTimeSpan(0, 0, 0, 55, 405);
            RightFootSwing1 AddDanceMove('m', newTimeSpan(0, 0, 0, 55, 876);
            RightFootCross2 AddDanceMove('m', newTimeSpan(0, 0, 0, 56, 544);
            RightFootSwing2 AddDanceMove('m', newTimeSpan(0, 0, 0, 57, 210);
            LeftKneeBendCrouch1 AddDanceMove('m', newTimeSpan(0, 0, 0, 57, 801);
            LeftKneeBendCrouch2 AddDanceMove('m', newTimeSpan(0, 0, 0, 58, 857);
            RightFootCross3 AddDanceMove('m', newTimeSpan(0, 0, 0, 59, 795);
            RightFootSwing3 AddDanceMove('m', newTimeSpan(0, 0, 0, 60, 100);
            RightFootCross4 AddDanceMove('m', newTimeSpan(0, 0, 0, 60, 768);
            RightFootSwing4 AddDanceMove('m', newTimeSpan(0, 0, 0, 61, 294);
            LeftKneeBendCrouch3 AddDanceMove('m', newTimeSpan(0, 0, 0, 62, 125);
            LeftKneeBendCrouch4 AddDanceMove('m', newTimeSpan(0, 0, 0, 63, 181);
            RightFootCross5 AddDanceMove('m', newTimeSpan(0, 0, 0, 64, 020);
            RightFootSwing5 AddDanceMove('m', newTimeSpan(0, 0, 0, 64, 510);
            RightFootCross6 AddDanceMove('m', newTimeSpan(0, 0, 0, 65, 058);
            RightFootSwing6 AddDanceMove('m', newTimeSpan(0, 0, 0, 65, 604);
            LeftKneeBendCrouch5 AddDanceMove('m', newTimeSpan(0, 0, 0, 66, 384);
            LeftKneeBendCrouch6 AddDanceMove('m', newTimeSpan(0, 0, 0, 67, 406);
            RightFootCross7 AddDanceMove('m', newTimeSpan(0, 0, 0, 68, 783);
            RightFootSwing7 AddDanceMove('m', newTimeSpan(0, 0, 0, 68, 981);
            RightFootCross8 AddDanceMove('m', newTimeSpan(0, 0, 0, 69, 355);
            RightFootSwing8 AddDanceMove('m', newTimeSpan(0, 0, 0, 69, 896);

            FemRightFootCross1 AddDanceMove('m', newTimeSpan(0, 0, 0, 55, 405);
            FemRightFootSwing1 AddDanceMove('m', newTimeSpan(0, 0, 0, 55, 876);
            FemRightFootCross2 AddDanceMove('m', newTimeSpan(0, 0, 0, 56, 544);
            FemRightFootSwing2 AddDanceMove('m', newTimeSpan(0, 0, 0, 57, 210);
            FemLeftKneeBendCrouch1 AddDanceMove('m', newTimeSpan(0, 0, 0, 57, 801);
            FemLeftKneeBendCrouch2 AddDanceMove('m', newTimeSpan(0, 0, 0, 58, 857);
            FemRightFootCross3 AddDanceMove('m', newTimeSpan(0, 0, 0, 59, 795);
            FemRightFootSwing3 AddDanceMove('m', newTimeSpan(0, 0, 0, 60, 100);
            FemRightFootCross4 AddDanceMove('m', newTimeSpan(0, 0, 0, 60, 768);
            FemRightFootSwing4 AddDanceMove('m', newTimeSpan(0, 0, 0, 61, 294);
            FemLeftKneeBendCrouch3 AddDanceMove('m', newTimeSpan(0, 0, 0, 62, 125);
            FemLeftKneeBendCrouch4 AddDanceMove('m', newTimeSpan(0, 0, 0, 63, 181);
            FemRightFootCross5 AddDanceMove('m', newTimeSpan(0, 0, 0, 64, 020);
            FemRightFootSwing5 AddDanceMove('m', newTimeSpan(0, 0, 0, 64, 510);
            FemRightFootCross6 AddDanceMove('m', newTimeSpan(0, 0, 0, 65, 058);
            FemRightFootSwing6 AddDanceMove('m', newTimeSpan(0, 0, 0, 65, 604);
            FemLeftKneeBendCrouch5 AddDanceMove('m', newTimeSpan(0, 0, 0, 66, 384);
            FemLeftKneeBendCrouch6 AddDanceMove('m', newTimeSpan(0, 0, 0, 67, 406);
            FemRightFootCross7 AddDanceMove('m', newTimeSpan(0, 0, 0, 68, 344);
            FemRightFootSwing7 AddDanceMove('m', newTimeSpan(0, 0, 0, 68, 783);
            FemRightFootCross8 AddDanceMove('m', newTimeSpan(0, 0, 0, 69, 355);
            FemRightFootSwing8 AddDanceMove('m', newTimeSpan(0, 0, 0, 69, 896);
            Score7 AddDanceMove('m', newTimeSpan(0, 0, 0, 70, 787);

            //Score block 8
            LeftKneeLiftAndCross AddDanceMove('m', newTimeSpan(0, 0, 0, 71, 366);
            RightKneeLiftAndCross AddDanceMove('m', newTimeSpan(0, 0, 0, 72, 472);
            LeftKneeLift12 AddDanceMove('m', newTimeSpan(0, 0, 0, 73, 007);
            RightKneeLift11 AddDanceMove('m', newTimeSpan(0, 0, 0, 74, 028);
            LeftKneeLift13 AddDanceMove('m', newTimeSpan(0, 0, 0, 75, 171);
            RightKneeLift12 AddDanceMove('m', newTimeSpan(0, 0, 0, 76, 246);
            LeftHandtoFaceSpinForward AddDanceMove('m', newTimeSpan(0, 0, 0, 76, 896);
            LeftHandtoFaceSpinBack AddDanceMove('m', newTimeSpan(0, 0, 0, 77, 364);
            RightKneeKick2 AddDanceMove('m', newTimeSpan(0, 0, 0, 78, 492);

            FemRightElbowSway1 AddDanceMove('m', newTimeSpan(0, 0, 0, 70, 578);
            FemLeftElbowSway1 AddDanceMove('m', newTimeSpan(0, 0, 0, 71, 617);
            FemRightElbowSway2 AddDanceMove('m', newTimeSpan(0, 0, 0, 72, 623);
            FemLeftElbowSway2 AddDanceMove('m', newTimeSpan(0, 0, 0, 73, 680);
            FemRightElbowSway3 AddDanceMove('m', newTimeSpan(0, 0, 0, 74, 770);
            FemLeftElbowSway3 AddDanceMove('m', newTimeSpan(0, 0, 0, 75, 858);
            FemRightElbowSway4 AddDanceMove('m', newTimeSpan(0, 0, 0, 76, 948);
            FemLeftElbowSway4 AddDanceMove('m', newTimeSpan(0, 0, 0, 78, 055);
            Score8 AddDanceMove('m', newTimeSpan(0, 0, 0, 79, 292);

            //Score block 9
            LeftKneeLift14 AddDanceMove('m', newTimeSpan(0, 0, 0, 79, 527);
            RightKneeKick3 AddDanceMove('m', newTimeSpan(0, 0, 0, 80, 569);
            LeftKneeLift15 AddDanceMove('m', newTimeSpan(0, 0, 0, 81, 707);
            RightKneeKick4 AddDanceMove('m', newTimeSpan(0, 0, 0, 82, 797);
            LeftKneeLift16 AddDanceMove('m', newTimeSpan(0, 0, 0, 83, 687);
            LeftKneeLiftLeft AddDanceMove('m', newTimeSpan(0, 0, 0, 84, 460);
            LeftKneeLiftBack AddDanceMove('m', newTimeSpan(0, 0, 0, 85, 646);
            LeftKneeLiftRight AddDanceMove('m', newTimeSpan(0, 0, 0, 86, 574);

            FemLeftWristArcRaise1 AddDanceMove('m', newTimeSpan(0, 0, 0, 79, 648);
            FemRightWristArcRaise1 AddDanceMove('m', newTimeSpan(0, 0, 0, 80, 603);
            FemHome1 AddDanceMove('m', newTimeSpan(0, 0, 0, 82, 602);
            FemLeftWristArcRaise2 AddDanceMove('m', newTimeSpan(0, 0, 0, 83, 938);
            FemRightWristArcRaise2 AddDanceMove('m', newTimeSpan(0, 0, 0, 85, 011);
            FemHome2 AddDanceMove('m', newTimeSpan(0, 0, 0, 86, 859);
            Score9 AddDanceMove('m', newTimeSpan(0, 0, 0, 87, 345);

            //Score block 10
            LeftKneeLift17 AddDanceMove('m', newTimeSpan(0, 0, 0, 87, 668);
            LeftKneeKick17 AddDanceMove('m', newTimeSpan(0, 0, 0, 88, 099);
            RightKneeLift13 AddDanceMove('m', newTimeSpan(0, 0, 0, 88, 385);
            LeftKneeLift18 AddDanceMove('m', newTimeSpan(0, 0, 0, 88, 686);
            LeftKneeKick18 AddDanceMove('m', newTimeSpan(0, 0, 0, 89, 255);
            RightKneeLift14 AddDanceMove('m', newTimeSpan(0, 0, 0, 89, 508);
            LeftKneeLift19 AddDanceMove('m', newTimeSpan(0, 0, 0, 90, 059);
            RightKneeKick5 AddDanceMove('m', newTimeSpan(0, 0, 0, 91, 250);

            LeftKneeLift20 AddDanceMove('m', newTimeSpan(0, 0, 0, 92, 038);
            LeftKneeKick20 AddDanceMove('m', newTimeSpan(0, 0, 0, 92, 424);
            RightKneeLift15 AddDanceMove('m', newTimeSpan(0, 0, 0, 92, 675);
            LeftKneeLift21 AddDanceMove('m', newTimeSpan(0, 0, 0, 92, 976);
            LeftKneeKick21 AddDanceMove('m', newTimeSpan(0, 0, 0, 93, 529);
            RightKneeLift16 AddDanceMove('m', newTimeSpan(0, 0, 0, 93, 831);
            LeftKneeLift22 AddDanceMove('m', newTimeSpan(0, 0, 0, 94, 569);
            RightKneeKick6 AddDanceMove('m', newTimeSpan(0, 0, 0, 95, 609);

            FemThrillerHandsLeft1 AddDanceMove('m', newTimeSpan(0, 0, 0, 88, 301);
            FemLeftBendHipShake1 AddDanceMove('m', newTimeSpan(0, 0, 0, 90, 413);
            FemThrillerHandsLeft2 AddDanceMove('m', newTimeSpan(0, 0, 0, 92, 628);
            FemLeftBendHipShake2 AddDanceMove('m', newTimeSpan(0, 0, 0, 95, 224);
            Score10 AddDanceMove('m', newTimeSpan(0, 0, 0, 96, 681);

            //Score block 11
            LeftKneeLiftAndFrontTorso3 AddDanceMove('m', newTimeSpan(0, 0, 0, 96, 336);
            RightKneeKick7 AddDanceMove('m', newTimeSpan(0, 0, 0, 97, 508);
            LeftKneeLift23 AddDanceMove('m', newTimeSpan(0, 0, 0, 98, 480);
            RightKneeKick8 AddDanceMove('m', newTimeSpan(0, 0, 0, 99, 621);
            LeftKneeLift24 AddDanceMove('m', newTimeSpan(0, 0, 0, 100, 660);
            RightKneeKick9 AddDanceMove('m', newTimeSpan(0, 0, 0, 101, 734);
            LeftKneeLiftAndFrontTorso4 AddDanceMove('m', newTimeSpan(0, 0, 0, 102, 738);
            RightKneeKick10 AddDanceMove('m', newTimeSpan(0, 0, 0, 103, 833);
            LeftKneeKick AddDanceMove('m', newTimeSpan(0, 0, 0, 104, 887);
            RightKneeKick AddDanceMove('m', newTimeSpan(0, 0, 0, 105, 974);

            FemRightElbowSway5 AddDanceMove('m', newTimeSpan(0, 0, 0, 96, 789);
            FemLeftElbowSway5 AddDanceMove('m', newTimeSpan(0, 0, 0, 97, 862);
            FemRightElbowSway6 AddDanceMove('m', newTimeSpan(0, 0, 0, 99, 035);
            FemLeftElbowSway6 AddDanceMove('m', newTimeSpan(0, 0, 0, 99, 973);
            FemLeftKneeLift5 AddDanceMove('m', newTimeSpan(0, 0, 0, 100, 980);
            FemRightKneeKick2 AddDanceMove('m', newTimeSpan(0, 0, 0, 102, 086);
            FemLeftKneeLiftAndFrontTorso5 AddDanceMove('m', newTimeSpan(0, 0, 0, 103, 112);
            FemRightKneeKick AddDanceMove('m', newTimeSpan(0, 0, 0, 104, 198);
            FemLeftKneeLift6 AddDanceMove('m', newTimeSpan(0, 0, 0, 105, 237);
            FemRightKneeKick3 AddDanceMove('m', newTimeSpan(0, 0, 0, 106, 381);
            Score11 AddDanceMove('m', newTimeSpan(0, 0, 0, 106, 988);

            //Score 12
            LeftKneeBendCrouch7 AddDanceMove('m', newTimeSpan(0, 0, 0, 107, 526);
            LeftKneeBendCrouch8 AddDanceMove('m', newTimeSpan(0, 0, 0, 108, 047);
            RightFootCross9 AddDanceMove('m', newTimeSpan(0, 0, 0, 109, 046);
            RightFootSwing9 AddDanceMove('m', newTimeSpan(0, 0, 0, 109, 501);
            RightFootCross10 AddDanceMove('m', newTimeSpan(0, 0, 0, 110, 034);
            RightFootSwing10 AddDanceMove('m', newTimeSpan(0, 0, 0, 110, 624);
            LeftKneeBendCrouch9 AddDanceMove('m', newTimeSpan(0, 0, 0, 111, 248);
            LeftKneeBendCrouch10 AddDanceMove('m', newTimeSpan(0, 0, 0, 112, 356);
            RightFootCross11 AddDanceMove('m', newTimeSpan(0, 0, 0, 113, 354);
            RightFootSwing11 AddDanceMove('m', newTimeSpan(0, 0, 0, 113, 814);
            RightFootCross12 AddDanceMove('m', newTimeSpan(0, 0, 0, 114, 358);
            RightFootSwing12 AddDanceMove('m', newTimeSpan(0, 0, 0, 114, 938);
            LeftKneeBendCrouch11 AddDanceMove('m', newTimeSpan(0, 0, 0, 115, 572);
            LeftKneeBendCrouch12 AddDanceMove('m', newTimeSpan(0, 0, 0, 116, 611);
            RightFootCross13 AddDanceMove('m', newTimeSpan(0, 0, 0, 117, 662);
            RightFootSwing13 AddDanceMove('m', newTimeSpan(0, 0, 0, 118, 069);
            RightFootCross14 AddDanceMove('m', newTimeSpan(0, 0, 0, 118, 600);
            RightFootSwing14 AddDanceMove('m', newTimeSpan(0, 0, 0, 119, 207);
            LeftKneeBendCrouch13 AddDanceMove('m', newTimeSpan(0, 0, 0, 119, 880);
            LeftKneeBendCrouch14 AddDanceMove('m', newTimeSpan(0, 0, 0, 120, 906);
            RightFootCross15 AddDanceMove('m', newTimeSpan(0, 0, 0, 121, 919);
            RightFootSwing15 AddDanceMove('m', newTimeSpan(0, 0, 0, 122, 409);
            RightFootCross16 AddDanceMove('m', newTimeSpan(0, 0, 0, 122, 960);
            RightFootSwing16 AddDanceMove('m', newTimeSpan(0, 0, 0, 123, 498);


            FemLeftKneeBendCrouch7 AddDanceMove('m', newTimeSpan(0, 0, 0, 107, 526);
            FemLeftKneeBendCrouch8 AddDanceMove('m', newTimeSpan(0, 0, 0, 108, 047);
            FemRightFootCross9 AddDanceMove('m', newTimeSpan(0, 0, 0, 109, 046);
            FemRightFootSwing9 AddDanceMove('m', newTimeSpan(0, 0, 0, 109, 501);
            FemRightFootCross10 AddDanceMove('m', newTimeSpan(0, 0, 0, 110, 034);
            FemRightFootSwing10 AddDanceMove('m', newTimeSpan(0, 0, 0, 110, 624);
            FemLeftKneeBendCrouch9 AddDanceMove('m', newTimeSpan(0, 0, 0, 111, 248);
            FemLeftKneeBendCrouch10 AddDanceMove('m', newTimeSpan(0, 0, 0, 112, 356);
            FemRightFootCross11 AddDanceMove('m', newTimeSpan(0, 0, 0, 113, 354);
            FemRightFootSwing11 AddDanceMove('m', newTimeSpan(0, 0, 0, 113, 814);
            FemRightFootCross12 AddDanceMove('m', newTimeSpan(0, 0, 0, 114, 358);
            FemRightFootSwing12 AddDanceMove('m', newTimeSpan(0, 0, 0, 114, 938);
            FemLeftKneeBendCrouch11 AddDanceMove('m', newTimeSpan(0, 0, 0, 115, 572);
            FemLeftKneeBendCrouch12 AddDanceMove('m', newTimeSpan(0, 0, 0, 116, 611);
            FemRightFootCross13 AddDanceMove('m', newTimeSpan(0, 0, 0, 117, 662);
            FemRightFootSwing13 AddDanceMove('m', newTimeSpan(0, 0, 0, 118, 069);
            FemRightFootCross14 AddDanceMove('m', newTimeSpan(0, 0, 0, 118, 600);
            FemRightFootSwing14 AddDanceMove('m', newTimeSpan(0, 0, 0, 119, 207);
            FemLeftKneeBendCrouch13 AddDanceMove('m', newTimeSpan(0, 0, 0, 119, 880);
            FemLeftKneeBendCrouch14 AddDanceMove('m', newTimeSpan(0, 0, 0, 120, 906);
            FemRightFootCross15 AddDanceMove('m', newTimeSpan(0, 0, 0, 121, 919);
            FemRightFootSwing15 AddDanceMove('m', newTimeSpan(0, 0, 0, 122, 409);
            FemRightFootCross16 AddDanceMove('m', newTimeSpan(0, 0, 0, 122, 960);
            FemRightFootSwing16 AddDanceMove('m', newTimeSpan(0, 0, 0, 123, 498);
            Score12 AddDanceMove('m', newTimeSpan(0, 0, 0, 124, 200);

            //Score 13
            LeftKneeLiftAndFrontTorso5 AddDanceMove('m', newTimeSpan(0, 0, 0, 124, 217);
            RightKneeKickAndUnderArm1 AddDanceMove('m', newTimeSpan(0, 0, 0, 125, 309);
            LeftKneeLiftAndUnderArm AddDanceMove('m', newTimeSpan(0, 0, 0, 126, 413);
            RightKneeKickAndUnderArm2 AddDanceMove('m', newTimeSpan(0, 0, 0, 127, 537);
            RightKneeKneelAndUnderArm AddDanceMove('m', newTimeSpan(0, 0, 0, 128, 576);
            RightKneeKneelAndUnderArmAndLeftHandBehind AddDanceMove('m', newTimeSpan(0, 0, 0, 132, 163);

            //Female set times
            FemLeftKneeLiftAndFrontTorso2 AddDanceMove('m', newTimeSpan(0, 0, 0, 124, 298);
            FemBackSpinRightKneeLift1 AddDanceMove('m', newTimeSpan(0, 0, 0, 125, 765);
            FemForwardSpinFacingRightKneeLift AddDanceMove('m', newTimeSpan(0, 0, 0, 127, 225);
            FemCrouchHipSwivel AddDanceMove('m', newTimeSpan(0, 0, 0, 129, 471);
            FemRightHandHigh AddDanceMove('m', newTimeSpan(0, 0, 0, 131, 988);
            Score13 AddDanceMove('m', newTimeSpan(0, 0, 0, 132, 800);
             * 
             * */
        }

        //load timings from a file instead of from code
        public void SetTimes(string filePath)
        {

        }
    }
}
