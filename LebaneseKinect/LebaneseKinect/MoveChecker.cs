using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace LebaneseKinect
{
    class MoveChecker
    {
        //scoring window
        int scoring_window = 600;
        
        //bools
        bool rightFootCrossed = false;

        //Knees and ankles
        float lky, lkx, lkz, rky, rkx, rkz, lax, lay, laz, rax, ray, raz;

        //Hands
        float lhy, lhx, lhz, rhx, rhy, rhz;

        //Spine is the cnter of the torso
        float spineX, spineY;

        //Shoulders
        float rsx, rsy, rsz, lsx, lsy, lsz, csx, csy, csz;

        //Elbows
        float rex, rey, rez, lex, ley, lez;

        //Waist/Hips
        float wstrx, wstry, wstrz, wstlx, wstly, wstlz;

        //Head pos
        float hx, hy;

        public int CheckMove(DanceMove move, TimeSpan currentTime)
        {
            //return -1 if move not triggered
            //otherwise, return the score for that move

            //TODO: call this somewhere else
            int score = -1;
            switch (move.GetName())
            {
                //TODO: There MUST be a better way of doing this than a huge switch statement
                #region "m/f left knee moves"
                case "LeftKneeLift":
                    if (LeftKneeTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndFrontTorsoAndLeftHand":
                    if (LeftKneeLiftAndFrontTorsoAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndLeftHand":
                    if (LeftKneeLiftAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndFrontTorso":
                    if (LeftKneeLiftAndFrontTorsoTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndCross":
                    if (LeftKneeLiftAndCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftFaceLeft":
                    if (LeftKneeLiftFaceLeftTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftFaceRight":
                    if (LeftKneeLiftFaceRightTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftFaceBack":
                    if (LeftKneeLiftFaceBackTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeLiftAndUnderArm":
                    if (LeftKneeLiftAndUnderArmTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeKick":
                    if (LeftKneeKickTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                #region "m/f right knee moves"
                case "RightKneeLift":
                    if (RightKneeTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndCross":
                    if (RightKneeLiftAndCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndLeftHand":
                    if (RightKneeLiftAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndBackTorso": //this might not be real
                    if (RightKneeLiftAndBackTorsoTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeLiftAndBackTorsoAndLeftHand": //this might not be real either
                    if (RightKneeLiftAndBackTorsoAndLeftHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKick":
                    if (RightKneeKickTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKickUnderArm":
                    if (RightKneeKickAndUnderArmTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                #region "male special moves for Dance1"
                case "KneelAndClap":
                    if (KneelingTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKneelAndUnderArm":
                    if (RightKneeKneelAndUnderArmTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightKneeKneelAndUnderArmAndLeftHandBehind":
                    if (RightKneeKneelAndUnderArmAndLeftHandBehindTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "WaiterHand":
                    if (MoveToRightAndWaiterHandTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "ShrugShoulders":
                    if (ShrugShouldersTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightFootSwing":
                    if (RightFootSwingTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "RightFootCross":
                    if (RightFootCrossTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftKneeBendCrouchLeft":
                case "LeftKneeBendCrouchRight":
                    if (LeftKneeBendCrouchTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftHandToFaceSpinForward":
                    if (LeftHandToFaceSpinForwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                case "LeftHandToFaceSpinBackward":
                    if (LeftHandToFaceSpinBackwardTriggered()) score = ScoreMove(move, currentTime);
                    break;
                #endregion
                default:
                    //do stuff
                    break;
            }

            return score;
        }

        public int ScoreMove(DanceMove move, TimeSpan currentTime)
        {
            double diff = Math.Abs((currentTime.Subtract(move.moveSpan).TotalMilliseconds));
            return Math.Min(400, Math.Max((int)(scoring_window - diff), 0));
        }

        #region Individual Move Check Functions
        //Left knee lift move, total of 24 for the male
        private bool LeftKneeTriggered()
        {
            //same for male and female
            return (lky > rky && lay > ray && distance2d(lkx, rkx, lky, rky) > .2);
        }
        private bool RightShoulderForwardTriggered()
        {
            return (rsz < lsz) || (rsz - lsz) < -.05; //?? TODO: Are these the same idea?
        }
        private bool LeftShoulderForwardTriggered()
        {
            return (lsz - rsz) < -.05;
        }
        private bool LeftKneeLiftAndFrontTorsoTriggered()
        {
            return LeftKneeTriggered() && RightShoulderForwardTriggered();
        }
        private bool LeftHandRaisedTriggered()
        {
            return (Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25);
        }
        private bool LeftKneeLiftAndFrontTorsoAndLeftHandTriggered()
        {
            return LeftHandRaisedTriggered() && LeftKneeLiftAndFrontTorsoTriggered();
        }
        private bool LeftKneeLiftAndLeftHandTriggered()
        {
            return LeftHandRaisedTriggered() && LeftKneeTriggered();
        }
        private bool LeftKneeCrossTriggered()
        {
            return (lkz - rkz) < -.05;
        }
        private bool RightKneeCrossTriggered()
        {
            return (rkz - lkz) < -.05;
        }
        private bool LeftKneeLiftAndCrossTriggered()
        {
            return LeftKneeTriggered() && LeftKneeCrossTriggered();
        }
        private bool LeftKneeLiftFaceLeftTriggered() //left knee lifted, facing left
        {
            return LeftKneeTriggered() && RightShoulderForwardTriggered();
        }
        private bool LeftKneeLiftFaceRightTriggered() //left knee lifted, facing right
        {
            return LeftKneeTriggered() && LeftShoulderForwardTriggered();
        }
        private bool LeftKneeLiftFaceBackTriggered() //left knee lifted, facing backwards
        {
            //Spinning back if left is on pos x
            return LeftKneeTriggered() && ((lsx - rsx) > .01);
        }
        private bool RightArmRaisedTriggered()//right arm and elbow above head
        {
            return (rhy > hy) && (rey > hy);
        }
        private bool LeftKneeLiftAndUnderArmTriggered()
        {
            return RightArmRaisedTriggered() && LeftKneeTriggered();
        }
        private bool RightKneeTriggered() //right knee lifted
        {
            return (rky > lky && ray > lay && distance2d(rkx, lkx, rky, lky) > .2);
        }
        private bool RightKneeLiftAndBackTorsoTriggered()
        {
            return !RightShoulderForwardTriggered() && RightKneeTriggered();
        }
        private bool RightKneeLiftAndBackTorsoAndLeftHandTriggered()
        {
            return LeftHandRaisedTriggered() && RightKneeLiftAndBackTorsoTriggered();
        }
        private bool RightKneeLiftAndLeftHandTriggered()
        {
            return RightKneeTriggered() && LeftHandRaisedTriggered();
        }
        private bool RightKneeLiftAndCrossTriggered()
        {
            return RightKneeTriggered() && LeftKneeCrossTriggered();
        }
        private bool KneelingTriggered()
        {
            return distance2d(rkx, wstrx, rky, wstry) < .2 && (rky - lky) > .01;
        }
        private bool RightKneeKneelAndUnderArmTriggered()
        {
            return KneelingTriggered() && RightArmRaisedTriggered();
        }
        private bool LeftHandBehindWaistTriggered()
        {
            return (wstrz - lhz) < -.05;
        }
        private bool RightKneeKneelAndUnderArmAndLeftHandBehindTriggered()
        {
            return RightKneeKneelAndUnderArmTriggered() && LeftHandBehindWaistTriggered();
        }
        private bool MoveToRightAndWaiterHandTriggered()
        {
            return !KneelingTriggered() && (rhy - rey) > 0.01;
        }
        private bool ShrugShouldersTriggered()
        {
            //TODO: Implement this step
            return true; //code to do this didn't work, cs is interpolated from ls and rs
        }
        private bool RightFootOverTriggered()
        {
            return distance2d(rax, lax, ray, lay) > .1;
        }
        private bool RightFootSwingTriggered() //RightFootSwing, if we've seen a cross, look for a swing back
        {
            if (rightFootCrossed && RightFootOverTriggered())
            {
                rightFootCrossed = false;
                return true;
            }
            return false;
        }
        private bool RightFootCrossTriggered() //RightFootSwing, if we've seen a cross, look for a swing back
        {
            if (!rightFootCrossed && RightFootOverTriggered())
            {
                rightFootCrossed = true;
                return true;
            }
            return false;
        }
        private bool LeftKneeBendCrouchTriggered()
        {
            return (lkz - wstrz) < -.05; //TODO: Test this step.
        }
        private bool FaceForwardTriggered()
        {
            return (rsx - lsx) > .01;
        }
        private bool FaceBackwardTriggered()
        {
            return (lsx - rsx) > .01;
        }
        private bool LeftHandToFaceSpinForwardTriggered()
        {
            return LeftHandRaisedTriggered() && FaceForwardTriggered();
        }
        private bool LeftHandToFaceSpinBackwardTriggered()
        {
            return LeftHandRaisedTriggered() && FaceBackwardTriggered();
        }
        private bool RightKneeKickTriggered()
        {
            return ((rkz - wstrz) < -.05) && ((raz - rkz) < -.05);
        }
        private bool LeftKneeKickTriggered()
        {
            return ((lkz - wstrz) < -.05) && ((laz - lkz) < -.05);
        }
        private bool RightKneeKickAndUnderArmTriggered()
        {
            return RightArmRaisedTriggered() && RightKneeKickTriggered();
        }
        #endregion

        private double distance2d(float x1, float x2, float y1, float y2)
        {
            double ydis = y1 - y2;
            double xdis = x1 - x2;
            double result = Math.Sqrt(((xdis * xdis) + (ydis * ydis)));
            return result;
        }

        public void UpdateSkeleton(Skeleton skeleton)
        {
            //if time has not passed since last skeleton update? return

            //otherwise set up all the values
            //Knees and ankles
            float lky = skeleton.Joints[JointType.KneeLeft].Position.Y;
            float lkx = skeleton.Joints[JointType.KneeLeft].Position.X;
            float lkz = skeleton.Joints[JointType.KneeLeft].Position.Z;
            float rkx = skeleton.Joints[JointType.KneeRight].Position.X;
            float rky = skeleton.Joints[JointType.KneeRight].Position.Y;
            float rkz = skeleton.Joints[JointType.KneeRight].Position.Z;
            float lax = skeleton.Joints[JointType.AnkleLeft].Position.X;
            float lay = skeleton.Joints[JointType.AnkleLeft].Position.Y;
            float laz = skeleton.Joints[JointType.AnkleLeft].Position.Z;
            float rax = skeleton.Joints[JointType.AnkleRight].Position.X;
            float ray = skeleton.Joints[JointType.AnkleRight].Position.Y;
            float raz = skeleton.Joints[JointType.AnkleRight].Position.Z;

            //Hands
            float lhy = skeleton.Joints[JointType.HandLeft].Position.Y;
            float lhx = skeleton.Joints[JointType.HandLeft].Position.X;
            float lhz = skeleton.Joints[JointType.HandLeft].Position.Z;
            float rhy = skeleton.Joints[JointType.HandRight].Position.Y;
            float rhx = skeleton.Joints[JointType.HandRight].Position.X;

            //Spine is the cnter of the torso
            float spineX = skeleton.Joints[JointType.Spine].Position.X;
            float spineY = skeleton.Joints[JointType.Spine].Position.Y;

            //Shoulders
            float rsx = skeleton.Joints[JointType.ShoulderRight].Position.X;
            float rsy = skeleton.Joints[JointType.ShoulderRight].Position.Y;
            float rsz = skeleton.Joints[JointType.ShoulderRight].Position.Z;
            float lsx = skeleton.Joints[JointType.ShoulderLeft].Position.X;
            float lsy = skeleton.Joints[JointType.ShoulderLeft].Position.Y;
            float lsz = skeleton.Joints[JointType.ShoulderLeft].Position.Z;
            float csx = skeleton.Joints[JointType.ShoulderCenter].Position.X;
            float csy = skeleton.Joints[JointType.ShoulderCenter].Position.Y;

            //Elbows
            float rex = skeleton.Joints[JointType.ElbowRight].Position.X;
            float rey = skeleton.Joints[JointType.ElbowRight].Position.Y;
            float rez = skeleton.Joints[JointType.ElbowRight].Position.Z;
            float lex = skeleton.Joints[JointType.ElbowLeft].Position.X;
            float ley = skeleton.Joints[JointType.ElbowLeft].Position.Y;
            float lez = skeleton.Joints[JointType.ElbowLeft].Position.Z;

            //Waist
            float wstrx = skeleton.Joints[JointType.HipRight].Position.X;
            float wstry = skeleton.Joints[JointType.HipRight].Position.Y;
            float wstrz = skeleton.Joints[JointType.HipRight].Position.Z;
            float wstlx = skeleton.Joints[JointType.HipLeft].Position.X;
            float wstly = skeleton.Joints[JointType.HipLeft].Position.Y;
            float wstlz = skeleton.Joints[JointType.HipLeft].Position.Z;

            //Head pos
            float hx = skeleton.Joints[JointType.Head].Position.X;
            float hy = skeleton.Joints[JointType.Head].Position.Y;
        }
    }
}

        /*
        private void RightKneeLiftAndBackTorsoTriggered()
        {

        }

        private void LeftKneeLiftAndLeftHandTriggered()
        {

        }

        private void RightKneeLiftAndLeftHandTriggered()
        {
           
        }

        public void LeftKneeLiftAndFrontTorsoAndLeftHandTriggered()
        {
            
        }

        public void RightKneeLiftAndBackTorsoAndLeftHandTriggered()
        {
            
        }

        public void KneelDownsAndClapTriggered()
        {
           
        }

        public void MoveToRightAndWaiterHandTriggered()
        {
            
        }

        public void ShrugShouldersTriggered()
        {

        }

        private void RightFootCrossTriggered()
        {
           
        }

        private void RightFootSwingTriggered()
        {
        }

        private void LeftKneeBendCrouchTriggered()
        {
           
        }

        private void RightKneeKickTriggered()
        {

        }

        private void LeftKneeKickTriggered()
        {
           
        }

        private void LeftHandtoFaceSpinBackTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftHandtoFaceSpinBack).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftHandtoFaceSpinBack = stepFinished;
            }
        }

        private void LeftKneeLiftAndCrossTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndCross).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftKneeLiftAndCross = stepFinished;
            }
        }

        private void LeftHandtoFaceSpinForwardTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftHandtoFaceSpinForward).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftHandtoFaceSpinForward = stepFinished;
            }
        }

        private void RightKneeLiftAndCrossTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLiftAndCross).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                RightKneeLiftAndCross = stepFinished;
            }
        }

        private void LeftKneeLiftLeftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftLeft).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftKneeLiftLeft = stepFinished;
            }
        }

        private void LeftKneeLiftBackTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftBack).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftKneeLiftBack = stepFinished;
            }
        }

        private void LeftKneeLiftRightTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftRight).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftKneeLiftRight = stepFinished;
            }
        }

        private void RightKneeKneelAndUnderArmTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeKneelAndUnderArm).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                RightKneeKneelAndUnderArm = stepFinished;
            }
        }

        private void LeftKneeLiftAndUnderArmTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndUnderArm).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                LeftKneeLiftAndUnderArm = stepFinished;
            }
        }

        private void RightKneeKneelAndUnderArmAndLeftHandBehindTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeKneelAndUnderArmAndLeftHandBehind).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreDurationMove(diff1);
                RightKneeKneelAndUnderArmAndLeftHandBehind = stepFinished;
            }
        }

        private void RightKneeKickAndUnderArmTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeKickAndUnderArm1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightKneeKickAndUnderArm2).TotalMilliseconds));

            if (diff1 < scoring_window)
            {
                scoreMove(diff1);
                RightKneeKickAndUnderArm1 = stepFinished;
            }

            else if (diff2 < scoring_window)
            {
                scoreMove(diff2);
                RightKneeKickAndUnderArm2 = stepFinished;
            }
        }
    }
}

/* old move rec code
if (malePlaying)
        {
            //Checking for left knee being raised and related moves
            if (lky > rky && lay > ray && distance2d(lkx, rkx, lky, rky) > .2)
            {
                //Check for torso trigger- front torso means right sholder leaning fwd
                if (rsz < lsz)
                {
                    //Check if the left hand is near the face for the left hand trigger
                    if ((Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25))
                    {
                        LeftKneeLiftAndFrontTorsoAndLeftHandTriggered();
                    }
                    //Otherwise we just trigger the torso and knee lift

                    LeftKneeLiftAndFrontTorsoTriggered();
                }
                //Check if the left hand is near the face for the left hand trigger
                if ((Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25))
                {
                    LeftKneeLiftAndLeftHandTriggered();
                }

                //If knee is overtop (crossed)
                if ((lkz - rkz) < -.05)
                {
                    LeftKneeLiftAndCrossTriggered();
                }

                //Holding knee out whil facing left; right shoulder facing camera
                if ((rsz - lsz) < -.05)
                {
                    LeftKneeLiftLeftTriggered();
                }

                //Spinning back if left is on pos x
                if ((lsx - rsx) > .01)
                {
                    LeftKneeLiftBackTriggered();
                }

                //Holding knee out while facing right, left shoulder facing cam
                if ((lsz - rsz) < -.05)
                {
                    LeftKneeLiftRightTriggered();
                }

                //holding hand up above head - hand and elbow will be above head
                if ((rhy > hy) && (rey > hy))
                {
                    LeftKneeLiftAndUnderArmTriggered();
                }

                //Else it's quite simply
                LeftKneeTriggered();
            }

            //Check for right knee raised and related moves
            if (rky > lky && ray > lay && distance2d(lkx, rkx, lky, rky) > .2)
            {
                //Torso trigger- ensure they aren't leaning forward
                if (!(rsz < lsz))
                {
                    //Check if the left hand is near the face for the left hand trigger
                    if ((Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25))
                    {
                        RightKneeLiftAndBackTorsoAndLeftHandTriggered();

                    }
                    //Otherwise we just trigger the torso and knee lift
                    RightKneeLiftAndBackTorsoTriggered();
                }

                //Check to see if left hand is lifted
                if ((Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25))
                {
                    RightKneeLiftAndLeftHandTriggered();
                }

                //If knee is overtop (crossed)
                if ((rkz - lkz) < -.05)
                {
                    RightKneeLiftAndCrossTriggered();
                }

                //Else it's simply
                RightKneeTriggered();
            }

            //For kneeling, see if their right knee is near their waist, and their left knee is below right knee
            if (distance2d(rkx, wstrx, rky, wstry) < .2 && (rky - lky) > .01)
            {
                //holding hand up above head - hand and elbow will be above head
                if ((rhy > hy) && (rey > hy))
                {
                    //Left hand z behind waist
                    if ((wstrz - lhz) < -.05)
                    {
                        RightKneeKneelAndUnderArmAndLeftHandBehindTriggered();
                    }
                    RightKneeKneelAndUnderArmTriggered();
                }
                //if hands are in front of face
                KneelDownsAndClapTriggered();
            }

            //Sense if he's holding out his hand for her, see if his hand is above his elbow and near his shoulder
            //Don't want this triggered if clap is triggered... hence the else
            else if ((rhy - rey) > 0.01)//&& distance2d(rhx, rsx, rhy, rsy) <.15)
            {
                MoveToRightAndWaiterHandTriggered();
            }

            //If both shoulders are above center shoulder
            //This currently doesn't trigger since center shoulder is interpolated...                    
            if (rsy >= csy && lsy >= csy)
            {
                // resultColor = Color.Red;
                // resultString = "Shrug!";
                //  textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
                ShrugShouldersTriggered();
            }

            //RightFootSwing, if we've seen a cross, look for a swing back
            if (rightFootCrossed && distance2d(rax, lax, ray, lay) > .1)
            {
                rightFootCrossed = false;
                RightFootSwingTriggered();
            }
            //Right foot cross,set rightFootCross to true
            //prob want to go on the x value of the joint
            if (distance2d(rax, lax, ray, lay) < .1)
            {
                rightFootCrossed = true;
                RightFootCrossTriggered();
            }
            //LKneeBend crouch- crouching, but leading with left knee
            //see if z value relative to hip will work
            if ((lkz - wstrz) < -.05)
            {
                LeftKneeBendCrouchTriggered();
            }

            //if hand to face..
            if ((Math.Abs(hx - lhx) < .25) && (Math.Abs(hy - lhy) < .25))
            {
                //Spinning fwd if r shoulder is on pos x
                if ((rsx - lsx) > .01)
                {
                    LeftHandtoFaceSpinForwardTriggered();
                }
                //Spinning back if left is on pos x
                if ((lsx - rsx) > .01)
                {
                    LeftHandtoFaceSpinBackTriggered();
                }
            }

            //Right knee kick- z val of ankle greater than knee & knee out from waist

            if (((rkz - wstrz) < -.05) && ((raz - rkz) < -.05))
            {
                //holding hand up above head - hand and elbow will be above head
                if ((rhy > hy) && (rey > hy))
                {
                    RightKneeKickAndUnderArmTriggered();
                }
                RightKneeKickTriggered();
            }

            //Left knee kick- z val of ankle greater than knee & knee out from waist
            if (((lkz - wstrz) < -.05) && ((laz - lkz) < -.05))
            {
                LeftKneeKickTriggered();
            }
        }
////////////////////////////////////////////////////////////////////////////////////////
//FEMALE MOVE RECOGNITION
///////////////////////////////////////////////////////////////////////////////////////
                 //Right now using the 1 skeleton to test triggers
                if (femPlaying)
                {
                    //Checking for left knee lift- all tested
                    if (lky > rky && lay > ray && distance2d(lkx, rkx, lky, rky) > .2)
                    {
                        //Torso- see if leaning fwd (z opposite
                        if ((rsz - wstrz) < -.05)
                        {
                            FemLeftKneeLiftAndFrontTorsoTriggerd();
                        }
                        FemLeftKneeLiftTriggered();
                    }

                    //Check for right knee lift- tested
                    if (rky > lky && ray > lay && distance2d(lkx, rkx, lky, rky) > .2)
                    {
                        //Test for spinning fwd
                        if ((rsx - lsx) > .01)
                        {
                            FemForwardSpinFacingRightKneeLiftTriggered();
                        }
                        //Spinning back if left is on pos x
                        if ((lsx - rsx) > .01)
                        {
                            FemBackSpinRightKneeLiftTriggered();
                        }
                        FemRightKneeLiftTriggered();
                    }

                    //Left foot lift & torso leaning fwd- tested
                    if ((lay - ray) > .05 && (rsz - wstrz) < -.05)
                    {
                        FemLeftFootLiftAndFrontTorsoTriggered();
                    }
                    //Right foot lift & torso leaning back- tested
                    if ((ray - lay) > .05 && (rsz - wstrz) > .05)
                    {
                        FemRightFootLiftAndBackTorsoTriggered();
                    }

                    //Hip shakes- sense hip movement by hip x change from knee x- necessitates a little crouching- tested
                    if (((wstrx - rkx) > .01 && (wstlx - lkx) > .01)
                        || ((rkx - wstrx) > .01 && (lkx - wstlx) > .01))
                    {

                        //Spinning fwd if r shoulder is on pos x- tested
                        if ((rsx - lsx) > .01)
                        {
                            FemHipShakeFrontTriggered();
                        }
                        //else could be crouching- see if hands are above waist- tested
                        if ((rhy - wstry) > .01 && (lhy - wstly) > 0.01)
                        {
                            FemCrouchAndHipShakeTriggered();
                        }
                    }
                    if (lkx > rkx && lkx > wstrx && rkx < wstrx)// tested
                    {
                        //Spinning back if left is on pos x

                        FemHipShakeBackTriggered();
                    }

                    //Hand swings- using the left hand while right is up near face- tested
                    if ((Math.Abs(hx - rhx) < .25) && (Math.Abs(hy - rhy) < .25))
                    {

                        //Spinning right if r shoulder is on pos z (behind)
                        if (rsz > lsz)
                        {
                            FemHandSwingRightTriggered();
                        }

                        //Spinning left if left is on pos z (behind)
                        if (lsz > rsz)
                        {
                            FemHandSwingLeftTriggered();
                        }

                        //Forward if right hand is on pos x
                        if ((rhx - lhx) > .25)
                        {
                            FemHandSwingFrontTriggered();
                        }

                        //back if left hand on pos x
                        if ((lhx - rhx) > .25)
                        {
                            FemHandSwingBackTriggered();
                        }
                    }

                    //Scrolling hands right- left hand near face, right hand is at a right angle (in line with elbow more or less)- tested
                    if (distance2d(lhx, hx, lhy, hy) < .25 && (lhy - rhy) > .01 && Math.Abs(rhy - rey) < .05)
                    {
                        FemMoveToRightAndScrollingHandsTriggered();

                    }
                    //Scrolling hands left- right hand near face, lefthand is at a right angle (in line with elbow more or less)- tested
                    if (distance2d(rhx, hx, rhy, hy) < .25 && (rhy - lhy) > .01 && Math.Abs(lhy - ley) < .05)
                    {
                        FemMoveToLeftAndScrollingHandsTriggered();
                    }

                    //RightFootSwing, if we've seen a cross, look for a swing back- tested
                    if (rightFootCrossed && distance2d(rax, lax, ray, lay) > .1)
                    {
                        rightFootCrossed = false;
                        FemRightFootSwingTriggered();
                    }
                    //Right foot cross,set rightFootCross to true
                    //prob want to go on the x value of the joint- tested
                    if (distance2d(rax, lax, ray, lay) < .1)
                    {
                        rightFootCrossed = true;
                        FemRightFootCrossTriggered();
                    }
                    //LKneeBend crouch- crouching, but leading with left knee- tested
                    if ((lkz - wstrz) < -.1)
                    {
                        FemLeftKneeBendCrouchTriggered();
                    }

                    //Right knee kick- z val of ankle greater than knee & knee out from waist- tesetd
                    if (((rkz - wstrz) < -.1) && ((raz - rkz) < -.1))
                    {
                        FemRightKneeKickTriggered();
                    }
                    //Elbow sway- hands at hip, so hands below "spine" and elbows above hips- tested
                    if (rhy < spineY && lhy < spineY && ley > wstly && rey > wstry)
                    {
                        //then  leading with that shoulder/elbow
                        if ((rez + .1) < lez)
                        {
                            FemRightElbowSwayTriggered();
                        }
                        else if ((lez + .1) < rez)
                        {
                            FemLeftElbowSwayTriggered();
                        }
                    }

                    //Wrist arc raises- hands held above head after being swung in an arc- tested
                    if ((lhy - hy) > .15)
                    {
                        FemLeftWristArcRaiseTriggered();
                    }
                    if ((rhy - hy) > .15) //tested
                    {
                        FemRightWristArcRaiseTriggered();
                    }

                    //Fem home is after hands are arced, hands returned to side- tested
                    if (arcedHands && (hy - lhy) > .1 && (hy - rhy) > .1)
                    {
                        FemHomeTriggered();
                    }

                    //Facing the left- tested
                    if (rsz < (lsz - .05) && wstrz < (wstlz - .05))
                    {
                        //Thriller hands- from the side facing left, hands held out in front
                        if (rhy > spineY && lhy > spineY)
                        {
                            FemThrillerHandsLeftTriggered();
                        }
                        //LeftBendHipShake- bum sticking out on the R while head is on L, crouching
                        if (Math.Abs(rhx - rkx) > .1)
                        {
                            FemLeftBendHipShakeTriggered();
                        }
                    }

                    //Hand swings- right hand above head, left near face, elbow at 90- tested
                    if ((rhy - hy) > .05 && (Math.Abs(hy - lhy) < .1))
                    {
                        FemRightHandHighTriggered();
                    }
                    //Crouch hip swivel- left hand up, knees slightly bent (knee z out), hips swinging - tested
                    if (((lkz - wstrz) < -.1) && ((rkz - wstrz) < -.1) && distance2d(lhx, hx, lhy, hy) < .75)
                    {
                        FemCrouchHipSwivelTriggered();
                    }
                }   

*/