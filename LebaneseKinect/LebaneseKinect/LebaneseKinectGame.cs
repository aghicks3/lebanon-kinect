//#define USE_KINECT // Comment out this line to test without a Kinect!!!

using System;
using System.Diagnostics; 
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SkinnedModel;
using System.Timers;

#if USE_KINECT
using Microsoft.Kinect;
#endif


namespace LebaneseKinect
{
    /// <summary>
    /// Lebanese Kinect Game - "Being"
    /// This game teaches the user a simple traditional Arab dance called dabke.
    /// Currently only displays and rotates player model
    /// </summary>
    public class LebaneseKinectGame : Microsoft.Xna.Framework.Game
    {
        // Declaring variables...
        bool bWantsToQuit = false;
#if USE_KINECT
        KinectSensor kinect;
        Skeleton[] skeletonData;
        Skeleton skeleton;
        Boolean debugging = true;

        // Tracking direction changes in head and feet
        float previousHeadY = 0;
        float previousLeftFootZ = 0;
        float previousRightFootZ = 0;
        int headYCounter = 0;
        int leftFootZCounter = 0;
        int rightFootZCounter = 0;

#endif
        const int WINDOW_WIDTH = 640;//720;
        const int WINDOW_HEIGHT = 480;

        // Basic XNA 3d variables
        GraphicsDeviceManager graphics;
        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(-2.25f, 0.5f, -1.35f);
        float cameraArc = 0;
        float cameraRotation = 0;
        float cameraDistance = 18;

        // Dabke dancer model variables
        Model modelMan, modelWoman;
        const int numberOfAnimationPlayers = 6;
        const float spaceBetweenAnimationPlayers = 4.4f;//4.5f;
        const float initialStartingPosition = -11.0f;
        AnimationPlayer[] animationPlayers;
        Matrix[] animationPlayersOffsets;
        AnimationClip[] clips;

        RenderTarget2D backBuffer;
        Effect kinectDepthVisualizer;
        Texture2D depthTexture;
        short[] depthData;
        bool needToRedrawBackBuffer;
        Rectangle shadowRect = new Rectangle(0, 0, 64, 64);

        // FSM variables
        SpriteBatch spriteBatch; //Draws all 2d images and text
        Texture2D jointTexture, shadowTexture;
        Texture2D StepHomeOFF, StepCrossOFF, StepKickOFF;
        Stopwatch videoTime = new Stopwatch();
        Vector2[] buttonPositions = { new Vector2(5, 5), new Vector2(75, 5), new Vector2(145, 5), new Vector2(215, 5), new Vector2(285, 5), new Vector2(355, 5) }; //currently unused
        
        /* Keyboard controls
         *  Up arrow: kick
         *  Down arrow: home
         *  Left/Right Arrow: cross
         */
        
        bool bSpaceKeyPressed = false;
        bool bCrossoverKeyPressed = false;
        bool bHomeKeyPressed = false;
        bool bKickKeyPressed = false;
        bool bDebugKeyPressed = false; //Display text, default is true
        bool bShowDebugText = true;

        /* Calculates the difference between the time at which it detects a user's action, and when the dancing animation does the same action.
         * TimeSpan(0, 0, 0, X, Y), X is seconds, Y is milliseconds.
         */
        //Expected dance move times;
        TimeSpan textFadeOut = new TimeSpan(0, 0, 0); // 2-second fadeout for result text
        

        int stepsDone = 0;
        //
        TimeSpan stepFinished = new TimeSpan(0,0,3,0,0);
        //First Male move
        TimeSpan LeftKneeLift1;
        TimeSpan RightKneeLift1;
        TimeSpan LeftKneeLift2;
        TimeSpan RightKneeLift2;
        TimeSpan introScore;

        //Second score block
        TimeSpan LeftKneeLift3;
        TimeSpan RightKneeLift3;
        TimeSpan LeftFootLiftAndFrontTorso1;
        TimeSpan RightFootLiftAndBackTorso1;
        TimeSpan LeftKneeLift4;
        TimeSpan RightKneeLift4;
        TimeSpan LeftFootLiftandFrontTorso2;
        TimeSpan RightFootLiftAndBackTorso2;
        TimeSpan Score2;

        //Third Score block
        TimeSpan LeftKneeLiftAndLeftHand1;
        TimeSpan RightKneeLiftAndLeftHand1;
        TimeSpan LeftKneeLiftAndFrontTorsoAndLeftHand1;
        TimeSpan RightKneeLiftAndBackTorsoAndLeftHand1;
        TimeSpan LeftKneeLiftAndLeftHand2;
        TimeSpan RightKneeLiftAndLeftHand2;
        TimeSpan LeftKneeLiftFrontTorsoAndLeftHand2;
        TimeSpan RightKneeLiftBackTorsoAndLeftHand2;
        TimeSpan Score3;

       //Fourth Score block
        TimeSpan KneelDownsAndClap;
        TimeSpan Score4;

        //Fifth Score block
        TimeSpan LeftKneeLift5;
        TimeSpan RightKneeLift5;
        TimeSpan LeftKneeLift6;
        TimeSpan RightKneeLift6;
        TimeSpan LeftKneeLift7;
        TimeSpan RightKneeLift7;
        TimeSpan LeftKneeLift8;
        TimeSpan RightKneeLift8;
        TimeSpan Score5;

        //Sixth Score block
        TimeSpan MoveToRightAndWaiterHand;
        TimeSpan ShrugShoulders;
        TimeSpan LeftKneeLift9;
        TimeSpan RightKneeLift9;
        TimeSpan LeftKneeLift10;
        TimeSpan RightKneeLift10;
        TimeSpan LeftKneeLiftAndFrontTorso3;
        TimeSpan RightKneeKick1;
        TimeSpan Score6;

        //Seventh Score block
        TimeSpan RightFootCross1;
        TimeSpan RightFootSwing1;
        TimeSpan RightFootCross2;
        TimeSpan RightFootSwing2;
        TimeSpan LeftKneeBendCrouch1;
        TimeSpan LeftKneeBendCrouch2;
        TimeSpan RightFootCross3;
        TimeSpan RightFootSwing3;
        TimeSpan RightFootCross4;
        TimeSpan RightFootSwing4;
        TimeSpan LeftKneeBendCrouch3;
        TimeSpan LeftKneeBendCrouch4;
        TimeSpan RightFootCross5;
        TimeSpan RightFootSwing5;
        TimeSpan RightFootCross6;
        TimeSpan RightFootSwing6;
        TimeSpan LeftKneeBendCrouch5;
        TimeSpan LeftKneeBendCrouch6;
        TimeSpan RightFootCross7;
        TimeSpan RightFootSwing7;
        TimeSpan RightFootCross8;
        TimeSpan RightFootSwing8;
        TimeSpan Score7;

        TimeSpan LeftKneeLiftAndCross;
        TimeSpan RightKneeLiftAndCross;
        TimeSpan LeftKneeLift12;
        TimeSpan RightKneeLift11;
        TimeSpan LeftKneeLift13;
        TimeSpan RightKneeLift12;
        TimeSpan LeftHandtoFaceSpinForward;
        TimeSpan LeftHandtoFaceSpinBack;
        TimeSpan RightKneeKick2;
        TimeSpan Score8;

        TimeSpan LeftKneeLift14;
        TimeSpan RightKneeKick3;
        TimeSpan LeftKneeLift15;
        TimeSpan RightKneeKick4;
        TimeSpan LeftKneeLift16;
        TimeSpan LeftKneeLiftLeft;
        TimeSpan LeftKneeLiftBack;
        TimeSpan LeftKneeLiftRight;
        TimeSpan Score9;

        TimeSpan LeftKneeLift17 = new TimeSpan(0, 0, 0, 87, 668);
        TimeSpan LeftKneeKick17 = new TimeSpan(0,0,0,88,099);
        TimeSpan RightKneeLift13 = new TimeSpan(0,0,0,88,385);
        TimeSpan LeftKneeLift18 = new TimeSpan(0,0,0,88,686);
        TimeSpan LeftKneeKick18 = new TimeSpan(0,0,0,89,255);
        TimeSpan RightKneeLift14 = new TimeSpan(0,0,0,89,508);
        TimeSpan LeftKneeLift19 = new TimeSpan(0,0,0,90,059);
        TimeSpan RightKneeKick5 = new TimeSpan(0,0,0,91,250);

        TimeSpan LeftKneeLift20 = new TimeSpan(0,0,0,92,038);
        TimeSpan LeftKneeKick20 = new TimeSpan(0,0,0,92,424);
        TimeSpan RightKneeLift15 = new TimeSpan(0,0,0,92,675);
        TimeSpan LeftKneeLift21 = new TimeSpan(0,0,0,92,976);
        TimeSpan LeftKneeeKick21 = new TimeSpan(0,0,0,93,529);
        TimeSpan RightKneeLift16 = new TimeSpan(0,0,0,93,831);
        TimeSpan LeftKneeLift22 = new TimeSpan(0,0,0,94,569);
        TimeSpan RightKneeKick6 = new TimeSpan(0,0,0,95,609);
        TimeSpan Score10 = new TimeSpan(0,0,0,96, 681);

        TimeSpan LeftFootLiftAndFrontTorso2 = new TimeSpan(0,0,0,96,336);
        TimeSpan RightKneeKick7 = new TimeSpan(0,0,0,97,508);
        TimeSpan LeftKneeLift23 = new TimeSpan(0,0,0,98,480);
        TimeSpan RightKneeKick8 = new TimeSpan(0,0,0,99,621);
        TimeSpan LeftKneeLift24 = new TimeSpan(0,0,0,100,660);
        TimeSpan RightKneeKick9 = new TimeSpan(0,0,0,101,734);
        TimeSpan LeftFootLiftAndFrontTorso4 = new TimeSpan(0,0,0,102, 738);
        TimeSpan RightKneeKick10 = new TimeSpan(0,0,0,103,833);
        TimeSpan LeftKneeKick = new TimeSpan(0,0,0,104,887);
        TimeSpan RightKneeKick = new TimeSpan(0, 0, 0, 105, 974);
        TimeSpan Score11 = new TimeSpan(0,0,0,106, 988);
            
        TimeSpan LeftKneeBendCrouch7 = new TimeSpan(0,0,0,107,526);
        TimeSpan LeftKneeBendCrouch8 = new TimeSpan(0,0,0, 108, 047);
        TimeSpan RightFootCross9 = new TimeSpan(0,0,0, 109, 101);
        TimeSpan RightFootSwing9 = new TimeSpan(0,0,0, 109, 620);
        TimeSpan RightFootCross10 = new TimeSpan(0,0,0, 110, 191);
        TimeSpan RightFootSwing10 = new TimeSpan(0,0,0, 110, 711);
        TimeSpan LeftKneeBendCrouch9 = new TimeSpan(0,0,0, 111, 248);
        TimeSpan LeftKneeBendCrouch10 = new TimeSpan(0,0,0, 112, 356);
        TimeSpan RightFootCross11 = new TimeSpan(0,0,0, 113, 393);
        TimeSpan RightFootSwing11 = new TimeSpan(0,0,0, 113, 879);
        TimeSpan RightFootCross12 = new TimeSpan(0,0,0, 114, 449);
        TimeSpan RightFootSwing12 = new TimeSpan(0,0,0, 115, 019);
        TimeSpan LeftKneeBendCrouch11 = new TimeSpan(0,0,0, 115, 572);
        TimeSpan LeftKneeBendCrouch12 = new TimeSpan(0,0,0, 116, 611);
        TimeSpan RightFootCross13 = new TimeSpan(0,0,0, 117, 619);
        TimeSpan RightFootSwing13 = new TimeSpan(0,0,0, 118, 207);
        TimeSpan RightFootCross14 = new TimeSpan(0,0,0, 118, 773);
        TimeSpan RightFootSwing14 = new TimeSpan(0,0,0, 119, 326);
        TimeSpan LeftKneeBendCrouch13 = new TimeSpan(0,0,0, 119, 880);
        TimeSpan LeftKneeBendCrouch14 = new TimeSpan(0,0,0, 120, 906);
        TimeSpan RightFootCross15 = new TimeSpan(0,0,0, 121, 892);
        TimeSpan RightFootSwing15 = new TimeSpan(0,0,0, 122, 462);
        TimeSpan RightFootCross16 = new TimeSpan(0,0,0, 123, 033);
        TimeSpan RightFootSwing16 = new TimeSpan(0,0,0, 123, 551);
        TimeSpan Score12 = new TimeSpan(0,0,0, 124,200);

        TimeSpan LeftFootLiftAndFrontTorso = new TimeSpan(0,0,0, 124,217);
        TimeSpan RightKneeKickAndUnderArm1 = new TimeSpan(0,0,0, 125,309);
        TimeSpan LeftKneeLiftAndUnderArm = new TimeSpan(0,0,0, 126,413);
        TimeSpan RightKneeKickAndUnderArm2 = new TimeSpan(0,0,0, 127,537);
        TimeSpan RightKneeKneelAndUnderArm = new TimeSpan(0,0,0, 128,576);
        TimeSpan RightKneeKneelAndUnderArmAndLeftHandBehind = new TimeSpan(0,0,0,132,163);
        TimeSpan Score13 = new TimeSpan(0, 0, 0, 132, 800);

        /*Female Times //////////////////
         */

        //First Female move
        int femStepsDone = 0;
        TimeSpan FemLeftKneeLift1;
        TimeSpan FemRightKneeLift1;
        TimeSpan FemLeftKneeLift2;
        TimeSpan FemRightKneeLift2;

        //Second score block
        TimeSpan FemLeftKneeLift3;
        TimeSpan FemRightKneeLift3;
        TimeSpan FemLeftFootLiftAndFrontTorso1;
        TimeSpan FemRightFootLiftAndBackTorso1;
        TimeSpan FemLeftKneeLift4;
        TimeSpan FemRightKneeLift4;
        TimeSpan FemLeftFootLiftandFrontTorso2;
        TimeSpan FemRightFootLiftAndBackTorso2;

        //Third Score block
        TimeSpan FemHandSwingFront1;
        TimeSpan FemHandSwingRight1;
        TimeSpan FemHipShakeBack1;
        TimeSpan FemHandSwingBack1;
        TimeSpan FemHandSwingLeft1;
        TimeSpan FemHipShakeFront1;


        //Fourth Score block
        TimeSpan FemHandSwingFront2;
        TimeSpan FemHandSwingRight2;
        TimeSpan FemHipShakeBack2;
        TimeSpan FemHandSwingBack2;
        TimeSpan FemHandSwingLeft2;
        TimeSpan FemHipShakeFront2;

        //Fifth Score block
        TimeSpan FemMoveToRightAndScrollingHands1;
        TimeSpan FemMoveToRightAndScrollingHands2;
        TimeSpan FemCrouchAndHipShake1;
        TimeSpan FemMoveToLeftAndScrollingHands1;
        TimeSpan FemMoveToLeftAndScrollingHands2;
        TimeSpan FemCrouchAndHipShake2;

        //Sixth Score block
        TimeSpan FemMoveToRightAndScrollingHands3;
        TimeSpan FemMoveToRightAndScrollingHands4;
        TimeSpan FemCrouchAndHipShake3;
        TimeSpan FemMoveToLeftAndScrollingHands3;
        TimeSpan FemMoveToLeftAndScrollingHands4;
        TimeSpan FemCrouchAndHipShake4;
        TimeSpan FemLeftKneeLiftAndFrontTorso3;
        TimeSpan FemRightKneeKick1;


        //Seventh Score block
        TimeSpan FemRightFootCross1;
        TimeSpan FemRightFootSwing1;
        TimeSpan FemRightFootCross2;
        TimeSpan FemRightFootSwing2;
        TimeSpan FemLeftKneeBendCrouch1;
        TimeSpan FemLeftKneeBendCrouch2;
        TimeSpan FemRightFootCross3;
        TimeSpan FemRightFootSwing3;
        TimeSpan FemRightFootCross4;
        TimeSpan FemRightFootSwing4;
        TimeSpan FemLeftKneeBendCrouch3;
        TimeSpan FemLeftKneeBendCrouch4;
        TimeSpan FemRightFootCross5;
        TimeSpan FemRightFootSwing5;
        TimeSpan FemRightFootCross6;
        TimeSpan FemRightFootSwing6;
        TimeSpan FemLeftKneeBendCrouch5;
        TimeSpan FemLeftKneeBendCrouch6;
        TimeSpan FemRightFootCross7;
        TimeSpan FemRightFootSwing7;
        TimeSpan FemRightFootCross8;
        TimeSpan FemRightFootSwing8;


        //Eighth Score block
        TimeSpan FemHome5;
        TimeSpan FemLeftFootGoFowardAndLeftHandUp1;
        TimeSpan FemRightFootGoFowardAndRightHandUp1;
        TimeSpan FemBothHandsScrollingDown1;
        TimeSpan FemLeftFootGoFowardAndLeftHandUp2;
        TimeSpan FemRightFootGoFowardAndLeftHandUp2;
        TimeSpan FemBothHandsScrollingDown2;


        //Ninth Score block
        TimeSpan FemScrollingHandsLeftSide1;
        TimeSpan FemFrontTorsoAndShakeShoulders1;
        TimeSpan FemScrollingHandsLeftSide2;
        TimeSpan FemFrontTorsoAndShakeShoulders2;
        TimeSpan FemRightKneeBackLift1;
        TimeSpan FemLeftKneeBackLift1;
        TimeSpan FemRightKneeFrontLift3;
        TimeSpan FemHome6;
        TimeSpan FemLeftKneeFrontLift4;
        TimeSpan FemRightKneeFrontLift4;
        TimeSpan FemLeftKneeLiftAndFrontTorso4;
        TimeSpan FemRightKneeLiftAndBackTorso3;
        TimeSpan FemLeftKneeFrontLift5;
        TimeSpan FemRightKneeFrontLift5;


        //Tenth Score block
        TimeSpan FemMoveToLeft5;
        TimeSpan FemMoveToLeft6;
        TimeSpan FemCrossOver9;
        TimeSpan FemCrossOver10;
        TimeSpan FemMoveToRight5;
        TimeSpan FemMoveToRight6;
        TimeSpan FemCrossOver11;
        TimeSpan FemCrossOver12;
        TimeSpan FemMoveToLeft7;
        TimeSpan FemMoveToLeft8;
        TimeSpan FemCrossOver13;
        TimeSpan FemCrossOver14;
        TimeSpan FemMoveToRight7;
        TimeSpan FemMoveToRight8;
        TimeSpan FemCrossOver15;
        TimeSpan FemCrossOver16;

        //Eleventh Score block
        TimeSpan FemLeftKneeFrontLift2;
        TimeSpan FemTurnAroundWithRightKneeLift1;
        TimeSpan FemTurnAroundWithLeftKneeLift1;
        TimeSpan FemTurnAroundWithRightKneeLift2;
        TimeSpan FemAllBodyScrollingDown1;

        public void setTime()
        {
            //First Score Block
            LeftKneeLift1 = new TimeSpan(0, 0, 0, 6, 562);
            RightKneeLift1 = new TimeSpan(0, 0, 0, 7, 602);
            LeftKneeLift2 = new TimeSpan(0, 0, 0, 8, 574);
            RightKneeLift2 = new TimeSpan(0, 0, 0, 9, 731);

            FemLeftKneeLift1 = new TimeSpan(0, 0, 0, 6, 562);
            FemRightKneeLift1 = new TimeSpan(0, 0, 0, 7, 602);
            FemLeftKneeLift2 = new TimeSpan(0, 0, 0, 8, 574);
            FemRightKneeLift2 = new TimeSpan(0, 0, 0, 9, 731);
            introScore = new TimeSpan(0,0,0,10,451);

            //2nd Score block
            LeftKneeLift3 = new TimeSpan(0, 0, 0, 10, 731);
            RightKneeLift3 = new TimeSpan(0, 0, 0, 11, 843);
            LeftFootLiftAndFrontTorso1 = new TimeSpan(0,0,0,12,951);
            RightFootLiftAndBackTorso1 = new TimeSpan(0,0,0,14,172);
            LeftKneeLift4 = new TimeSpan(0,0,0,15,146);
            RightKneeLift4 = new TimeSpan(0,0,0,16,169);
            LeftFootLiftandFrontTorso2 = new TimeSpan(0,0,0,17,170);
            RightFootLiftAndBackTorso2 = new TimeSpan(0,0,0,18,294);

            FemLeftKneeLift3 = new TimeSpan(0, 0, 0, 10, 731);
            FemRightKneeLift3 = new TimeSpan(0, 0, 0, 11, 843);
            FemLeftFootLiftAndFrontTorso1 = new TimeSpan(0, 0, 0, 12, 951);
            FemRightFootLiftAndBackTorso1 = new TimeSpan(0,0,0,14,172);
            FemLeftKneeLift4 = new TimeSpan(0,0,0,15,146);
            FemRightKneeLift4 = new TimeSpan(0,0,0,16,169);
            FemLeftFootLiftandFrontTorso2 = new TimeSpan(0, 0, 0, 17, 170);
            FemRightFootLiftAndBackTorso2 = new TimeSpan(0, 0, 0, 18, 294);
            Score2 = new TimeSpan(0,0,0,18,546);

            //Score 3
            LeftKneeLiftAndLeftHand1 = new TimeSpan(0,0,0,19,355);
            RightKneeLiftAndLeftHand1 = new TimeSpan(0,0,0,20,311);
            LeftKneeLiftAndFrontTorsoAndLeftHand1 = new TimeSpan(0,0,0,21,427);
            RightKneeLiftAndBackTorsoAndLeftHand1 = new TimeSpan(0,0,0,22,556);
            LeftKneeLiftAndLeftHand2 = new TimeSpan(0,0,0,23,512);
            RightKneeLiftAndLeftHand2 = new TimeSpan(0,0,0,24,595);
            LeftKneeLiftFrontTorsoAndLeftHand2 = new TimeSpan(0,0,0,25,654);
            RightKneeLiftBackTorsoAndLeftHand2 = new TimeSpan(0,0,0,26,814);
            //LeftKneeLift11 and LeftHand1 = new TimeSpan(0,0,0,27,859); We're ignoring this step for now.

            FemHandSwingFront1 = new TimeSpan(0,0,0,19,492);
            FemHandSwingRight1 = new TimeSpan(0, 0, 0, 20, 615);
            FemHipShakeBack1 = new TimeSpan(0,0,0,21,151);
            FemHandSwingBack1 = new TimeSpan(0,0,0,23,834);
            FemHandSwingLeft1 = new TimeSpan(0,0,0,25,046);
            FemHipShakeFront1 = new TimeSpan(0,0,0,25,479);
            Score3 = new TimeSpan(0,0,0,27,543);

            //Score 4
            KneelDownsAndClap = new TimeSpan(0,0,0,28,785);

            FemHandSwingFront2 = new TimeSpan(0,0,0,28,192);
            FemHandSwingRight2 = new TimeSpan(0,0,0,29,318);
            FemHipShakeBack2 = new TimeSpan(0,0,0,29,707);
            FemHandSwingBack2 = new TimeSpan(0,0,0,32,517);
            FemHandSwingLeft2 = new TimeSpan(0,0,0,33,562);
            FemHipShakeFront2 = new TimeSpan(0,0,0,34,045);
            Score4 = new TimeSpan(0,0,0,35,806);

            //Score section 5
            LeftKneeLift5 = new TimeSpan(0,0,0,36,590);
            RightKneeLift5 = new TimeSpan(0,0,0,37,460);
            LeftKneeLift6 = new TimeSpan(0,0,0,38,574);
            RightKneeLift6 = new TimeSpan(0,0,0,39,558);
            LeftKneeLift7 = new TimeSpan(0,0,0,40,706);
            RightKneeLift7 = new TimeSpan(0,0,0,41,809);
            LeftKneeLift8 = new TimeSpan(0,0,0,42,857);
            RightKneeLift8 = new TimeSpan(0,0,0,43,916);

            FemMoveToRightAndScrollingHands1 = new TimeSpan(0,0,0,36,228);
            FemMoveToRightAndScrollingHands2 = new TimeSpan(0,0,0,37,496);
            FemCrouchAndHipShake1 = new TimeSpan(0,0,0,38,552);
            FemMoveToLeftAndScrollingHands1 = new TimeSpan(0,0,0,40,698);
            FemMoveToLeftAndScrollingHands2 = new TimeSpan(0,0,0,41,670);
            FemCrouchAndHipShake2 = new TimeSpan(0,0,0,42,728);
            Score5 = new TimeSpan(0,0,0,44,494);

            //Score section 6
            MoveToRightAndWaiterHand = new TimeSpan(0,0,0,44,751); //Just detect waiter hand bones?
            ShrugShoulders = new TimeSpan(0,0,0,46,612);
            LeftKneeLift9 = new TimeSpan(0,0,0,49,288);
            RightKneeLift9 = new TimeSpan(0,0,0,50,273);
            LeftKneeLift10 = new TimeSpan(0,0,0,51,299);
            RightKneeLift10 = new TimeSpan(0,0,0,52,379);
            LeftKneeLiftAndFrontTorso3 = new TimeSpan(0, 0, 0, 53, 896);
            RightKneeKick1 = new TimeSpan(0,0,0,54,951);

            FemMoveToRightAndScrollingHands3 = new TimeSpan(0,0,0,44,890);
            FemMoveToRightAndScrollingHands4 = new TimeSpan(0,0,0,45,944);
            FemCrouchAndHipShake3 = new TimeSpan(0,0,0,47,034);
            FemMoveToLeftAndScrollingHands3 = new TimeSpan(0,0,0,49,112);
            FemMoveToLeftAndScrollingHands4 = new TimeSpan(0,0,0,50,252);
            FemCrouchAndHipShake4 = new TimeSpan(0,0,0,51,309);
            Score6 = new TimeSpan(0,0,0,53,225);
            
            RightFootCross1 = new TimeSpan(0,0,0,55,558);
            RightFootSwing1 = new TimeSpan(0,0,0,56,175);
            RightFootCross2 = new TimeSpan(0,0,0,56,678);
            RightFootSwing2 = new TimeSpan(0,0,0,57,210);
            LeftKneeBendCrouch1 = new TimeSpan(0,0,0,57,801);
            LeftKneeBendCrouch2 = new TimeSpan(0,0,0,58,857);
            RightFootCross3 = new TimeSpan(0,0,0,59,903);
            RightFootSwing3 = new TimeSpan(0,0,0,60,483);
            RightFootCross4 = new TimeSpan(0,0,0,60,952);
            RightFootSwing4 = new TimeSpan(0,0,0,61,489);
            LeftKneeBendCrouch3 = new TimeSpan(0,0,0,62,125);
            LeftKneeBendCrouch4 = new TimeSpan(0,0,0,63,181);
            RightFootCross5 = new TimeSpan(0,0,0,64,205);
            RightFootSwing5 = new TimeSpan(0,0,0,64,724);
            RightFootCross6 = new TimeSpan(0,0,0,65,261);
            RightFootSwing6 = new TimeSpan(0,0,0,65,813);
            LeftKneeBendCrouch5 = new TimeSpan(0,0,0,66,384);
            LeftKneeBendCrouch6 = new TimeSpan(0,0,0,67,406);
            RightFootCross7 = new TimeSpan(0,0,0,68,478);
            RightFootSwing7 = new TimeSpan(0,0,0,68,981);
            RightFootCross8 = new TimeSpan(0,0,0,69,537);
            RightFootSwing8 = new TimeSpan(0,0,0,70,087);
            Score7 = new TimeSpan(0,0,0,70,787);
            
            LeftKneeLiftAndCross = new TimeSpan(0,0,0,71,366);
            RightKneeLiftAndCross = new TimeSpan(0,0,0,72,472);
            LeftKneeLift12 = new TimeSpan(0,0,0,73,007);
            RightKneeLift11 = new TimeSpan(0,0,0,74,028);
            LeftKneeLift13 = new TimeSpan(0,0,0,75,171);
            RightKneeLift12 = new TimeSpan(0,0,0,76,246);
            LeftHandtoFaceSpinForward = new TimeSpan(0,0,0,76,896);
            LeftHandtoFaceSpinBack = new TimeSpan(0, 0, 0, 77, 364);
            RightKneeKick2 = new TimeSpan(0,0,0,78,492);
            Score8 = new TimeSpan(0, 0, 0, 79, 292);

            LeftKneeLift14 = new TimeSpan(0,0,0,79,527);
            RightKneeKick3 = new TimeSpan(0,0,0,80,569);
            LeftKneeLift15 = new TimeSpan(0,0,0,81,707);
            RightKneeKick4 = new TimeSpan(0,0,0,82,797);
            LeftKneeLift16 = new TimeSpan(0,0,0,83,687);
            LeftKneeLiftLeft = new TimeSpan(0,0,0,84,460);
            LeftKneeLiftBack = new TimeSpan(0,0,0,85,646);
            LeftKneeLiftRight = new TimeSpan(0,0,0,86,574);
            Score9 = new TimeSpan(0,0,0,87,345);
            
            LeftKneeLift17 = new TimeSpan(0, 0, 0, 87, 668);
            LeftKneeKick17 = new TimeSpan(0,0,0,88,099);
            RightKneeLift13 = new TimeSpan(0,0,0,88,385);
            LeftKneeLift18 = new TimeSpan(0,0,0,88,686);
            LeftKneeKick18 = new TimeSpan(0,0,0,89,255);
            RightKneeLift14 = new TimeSpan(0,0,0,89,508);
            LeftKneeLift19 = new TimeSpan(0,0,0,90,059);
            RightKneeKick5 = new TimeSpan(0,0,0,91,250);

            LeftKneeLift20 = new TimeSpan(0,0,0,92,038);
            LeftKneeKick20 = new TimeSpan(0,0,0,92,424);
            RightKneeLift15 = new TimeSpan(0,0,0,92,675);
            LeftKneeLift21 = new TimeSpan(0,0,0,92,976);
            LeftKneeeKick21 = new TimeSpan(0,0,0,93,529);
            RightKneeLift16 = new TimeSpan(0,0,0,93,831);
            LeftKneeLift22 = new TimeSpan(0,0,0,94,569);
            RightKneeKick6 = new TimeSpan(0,0,0,95,609);
            Score10 = new TimeSpan(0,0,0,96, 681);

            LeftFootLiftAndFrontTorso2 = new TimeSpan(0,0,0,96,336);
            RightKneeKick7 = new TimeSpan(0,0,0,97,508);
            LeftKneeLift23 = new TimeSpan(0,0,0,98,480);
            RightKneeKick8 = new TimeSpan(0,0,0,99,621);
            LeftKneeLift24 = new TimeSpan(0,0,0,100,660);
            RightKneeKick9 = new TimeSpan(0,0,0,101,734);
            LeftFootLiftAndFrontTorso4 = new TimeSpan(0,0,0,102, 738);
            RightKneeKick10 = new TimeSpan(0,0,0,103,833);
            LeftKneeKick = new TimeSpan(0,0,0,104,887);
            RightKneeKick = new TimeSpan(0, 0, 0, 105, 974);
            Score11 = new TimeSpan(0,0,0,106, 988);
            

            LeftKneeBendCrouch7 = new TimeSpan(0,0,0,107,526);
            LeftKneeBendCrouch8 = new TimeSpan(0,0,0, 108, 047);
            RightFootCross9 = new TimeSpan(0,0,0, 109, 101);
            RightFootSwing9 = new TimeSpan(0,0,0, 109, 620);
            RightFootCross10 = new TimeSpan(0,0,0, 110, 191);
            RightFootSwing10 = new TimeSpan(0,0,0, 110, 711);
            LeftKneeBendCrouch9 = new TimeSpan(0,0,0, 111, 248);
            LeftKneeBendCrouch10 = new TimeSpan(0,0,0, 112, 356);
            RightFootCross11 = new TimeSpan(0,0,0, 113, 393);
            RightFootSwing11 = new TimeSpan(0,0,0, 113, 879);
            RightFootCross12 = new TimeSpan(0,0,0, 114, 449);
            RightFootSwing12 = new TimeSpan(0,0,0, 115, 019);
            LeftKneeBendCrouch11 = new TimeSpan(0,0,0, 115, 572);
            LeftKneeBendCrouch12 = new TimeSpan(0,0,0, 116, 611);
            RightFootCross13 = new TimeSpan(0,0,0, 117, 619);
            RightFootSwing13 = new TimeSpan(0,0,0, 118, 207);
            RightFootCross14 = new TimeSpan(0,0,0, 118, 773);
            RightFootSwing14 = new TimeSpan(0,0,0, 119, 326);
            LeftKneeBendCrouch13 = new TimeSpan(0,0,0, 119, 880);
            LeftKneeBendCrouch14 = new TimeSpan(0,0,0, 120, 906);
            RightFootCross15 = new TimeSpan(0,0,0, 121, 892);
            RightFootSwing15 = new TimeSpan(0,0,0, 122, 462);
            RightFootCross16 = new TimeSpan(0,0,0, 123, 033);
            RightFootSwing16 = new TimeSpan(0,0,0, 123, 551);
            Score12 = new TimeSpan(0,0,0, 124,200);

            LeftFootLiftAndFrontTorso = new TimeSpan(0,0,0, 124,217);
            RightKneeKickAndUnderArm1 = new TimeSpan(0,0,0, 125,309);
            LeftKneeLiftAndUnderArm = new TimeSpan(0,0,0, 126,413);
            RightKneeKickAndUnderArm2 = new TimeSpan(0,0,0, 127,537);
            RightKneeKneelAndUnderArm = new TimeSpan(0,0,0, 128,576);
            RightKneeKneelAndUnderArmAndLeftHandBehind = new TimeSpan(0,0,0,132,163);
            Score13 = new TimeSpan(0,0,0,132,800);
        }

        SpriteFont font;
        SpriteFont resultFont;
        Color resultColor;
        string resultString = " ";
        string displayScoreText = " ";
        string displayRecentScoreText = " ";
        int totalScore = 0;
        int setScore = 0;
        int displayScore = 0;
        int tempScore = 0;

        Texture2D backgroundDabke;
        Rectangle backgroundRect = new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        List<Rectangle> shadowRects = new List<Rectangle>();

        List<string> eventsTriggeredList;

        // FSM for important keyframes in the dance
        enum DabkeSteps
        {
            Home1,
            Crossover1,
            Home2,
            Crossover2,
            Home3,
            Kick,
            WaitForReset
        };
        DabkeSteps currentDabke = DabkeSteps.Home1;

        protected Song song;
        VideoPlayer videoPlayer;
        Video video;
        Video video1;
        bool introPlaying = true;

        public LebaneseKinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.IsFullScreen = false;
            Content.RootDirectory = "Content";
            eventsTriggeredList = new List<string>();
            for (int i = 0; i < numberOfAnimationPlayers; i++)
                shadowRects.Add(new Rectangle(0,0,64,64));
        }

        private void GraphicsDevicePreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            // This is necessary because we are rendering to back buffer/render targets and we need to preserve the data
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
        }

        public SpriteBatch SharedSpriteBatch
        {
            get
            {
                return (SpriteBatch)Services.GetService(typeof(SpriteBatch));
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
#if USE_KINECT
            kinect = KinectSensor.KinectSensors[0];
            kinect.Start();
#endif

            base.Initialize();  // Base XNA init...

#if USE_KINECT
            try
            {
                // Init Kinect for skeletal tracking
                kinect = KinectSensor.KinectSensors[0];
                kinect.Start();
                Debug.WriteLineIf(debugging, kinect.Status);
                kinect.SkeletonStream.Enable();
                kinect.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinect_DepthFrameReady);
                kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_AllFramesReady);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            kinect.ElevationAngle = 0;
#endif
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Load our 3D dancer model(s)
            modelMan = Content.Load<Model>("Models\\man");
            modelWoman = Content.Load<Model>("Models\\woman");

            // Our 3D models are skinned... notify if there's an error.  
            SkinningData skinningDataMale = modelMan.Tag as SkinningData;
            if (skinningDataMale == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            SkinningData skinningDataFemale = modelWoman.Tag as SkinningData;
            if (skinningDataFemale == null)
                throw new InvalidOperationException
                    ("This model does not contain a SkinningData tag.");

            // "Take_001" is the basic dance animation.  More (better-named) clips will also be loaded here.
            clips = new AnimationClip[6];
            clips[0] = skinningDataMale.AnimationClips["Take_001"];
            clips[1] = skinningDataFemale.AnimationClips["Take_001"];
            clips[2] = skinningDataMale.AnimationClips["Take_001"];
            clips[3] = skinningDataFemale.AnimationClips["Take_001"];
            clips[4] = skinningDataMale.AnimationClips["Take_001"];
            clips[5] = skinningDataFemale.AnimationClips["Take_001"];
            //AnimationClip clip = skinningData.AnimationClips["Take_001"];

            font = Content.Load<SpriteFont>("myFont");
            resultFont = Content.Load<SpriteFont>("resultFont");
            
            animationPlayers = new AnimationPlayer[numberOfAnimationPlayers];
            animationPlayersOffsets = new Matrix[numberOfAnimationPlayers];
            for (int i = 0; i < numberOfAnimationPlayers; i++)
            {
                if (i % 2 == 0)
                {
                    animationPlayers[i] = new AnimationPlayer(skinningDataMale);
                }
                else
                {
                    animationPlayers[i] = new AnimationPlayer(skinningDataFemale);
                }

                animationPlayers[i].StartClip(clips[i]);


                // Create an animation player, and start decoding an animation clip.
                //animationPlayers[i] = (i % 2 == 0) ? new AnimationPlayer(skinningDataMale) : new AnimationPlayer(skinningDataFemale);

                // Animate all three dancers.  TODO: Have slightly varying dances to look more genuine.
                //animationPlayers[i].StartClip(clip);
                //animationPlayers[i].StartClip(clips[i%3]);
                //animationPlayers[i].StartClip(clips[i % 6]);
                //animationPlayers[i].CurrentClip.Keyframes[0].

                //animationPlayersOffsets[i] = new Matrix();
                float offsetX = initialStartingPosition + (spaceBetweenAnimationPlayers * (float)i);
                animationPlayersOffsets[i] = Matrix.CreateTranslation(new Vector3(offsetX, 0.0f, 0.0f));
                
            }

            kinectDepthVisualizer = Content.Load<Effect>("KinectDepthVisualizer");

            // Create and load all the dance step icons
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            jointTexture = Content.Load<Texture2D>("Textures\\joint");
            shadowTexture = Content.Load<Texture2D>("Textures\\shadow");
            backgroundDabke = Content.Load<Texture2D>("Textures\\backgroundDabke");
            StepHomeOFF = Content.Load<Texture2D>("Textures\\dsteps1OFF");
            StepCrossOFF = Content.Load<Texture2D>("Textures\\dsteps2OFF");
            StepKickOFF = Content.Load<Texture2D>("Textures\\dsteps3OFF");

            song = Content.Load<Song>("Music\\dabke");
            //MediaPlayer.Play(song);

            video = Content.Load<Video>("Video\\tempIntro");
            video1 = Content.Load<Video>("Video\\Lebanon"); 
            videoPlayer = new VideoPlayer();
            videoPlayer.Play(video);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            spriteBatch.Dispose();
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (bWantsToQuit)
                Exit();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape) ||
                Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                bWantsToQuit = true;
#if USE_KINECT
                kinect.Dispose();
#endif
            }

            // Press spacebar to advance your current dance steps without actually dancing.
            // Helpful for testing, making videos, ect...
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!bSpaceKeyPressed)
                {
                    if (introPlaying)
                    {
                        // SPACEBAR stops the intro video, if it is playing...
                        videoPlayer.Dispose();
                        videoPlayer = new VideoPlayer();
                        videoPlayer.Play(video1);
                        introPlaying = false;
                        setTime();
                        videoTime.Start();    
                    }
                    else
                    {
                        // Otherwise, SPACEBAR is our developer shortcut for getting the current time
                        int currentTime = (int)videoTime.ElapsedMilliseconds;
                        Debug.WriteLine( "\nCurrent time in milliseconds:" +currentTime+ "\n");
                        bSpaceKeyPressed = true;
                    }
                }
            }
            else
                bSpaceKeyPressed = false;

            if (!introPlaying)
            {

                if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.Right))
                {
                    if (!bCrossoverKeyPressed)
                    {
                        bCrossoverKeyPressed = true;
                    }
                }
                else
                {
                    bCrossoverKeyPressed = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                {
                    if (!bHomeKeyPressed)
                    {
                        bHomeKeyPressed = true;
                    }
                }
                else
                {
                    bHomeKeyPressed = false;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                {
                    if (!bKickKeyPressed)
                    {
                        bKickKeyPressed = true;
                    }
                }
                else
                {
                    bKickKeyPressed = false;
                }


                /*Check each step to see if it is hit*/
                if (LeftKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + 600 < 0)
                {
                    LeftKneeLift1 = stepFinished;
                    stepsDone++;
                }
                if (LeftKneeLift2.Subtract(videoTime.Elapsed).Milliseconds + 600 < 0)
                {
                    LeftKneeLift2 = stepFinished;
                    stepsDone++;
                }
                if (RightKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + 600 < 0)
                {
                    RightKneeLift1 = stepFinished;
                    stepsDone++;
                }
                if (RightKneeLift2.Subtract(videoTime.Elapsed).Milliseconds + 600 < 0)
                {
                    RightKneeLift2 = stepFinished;
                    stepsDone++;
                }
                if (introScore.Subtract(videoTime.Elapsed).Milliseconds < 0)
                {
                    introScore = stepFinished;
                    scorePlayer();
                }
                /*Finish checking steps*/
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (!bDebugKeyPressed)
                {
                    bDebugKeyPressed = true;
                    bShowDebugText = !bShowDebugText;
                }
            }
            else
            {
                bDebugKeyPressed = false;
            }

            if (totalScore > displayScore)
                displayScore += 11;

            if (totalScore < displayScore)
                displayScore = totalScore;

            base.Update(gameTime);  // Base XNA update...
        }

        //formerly danceAnimationEnd
        private void scorePlayer()
        {
            setScore = tempScore;
            tempScore = 0;
            if (setScore > (stepsDone*300))
            {
                resultColor = Color.Green;
                resultString = "EXCELLENT!";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScore > (stepsDone*200))
            {
                resultColor = Color.Yellow;
                resultString = "GOOD";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScore > (stepsDone*100))
            {
                resultColor = Color.Red;
                resultString = "KEEP TRYING";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }

            if (setScore > 400)
            {
                displayRecentScoreText = "+" + setScore.ToString();
                totalScore += setScore;
            }
            else
            {
                displayRecentScoreText = " ";
                setScore = 0;
            }
        }

        private void LeftKneeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLift1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeLift2).TotalMilliseconds));

            if(diff1 < 600)
            {
                int earnedScore = Math.Max((int)(600 - diff1), 0);
                if (earnedScore > 400)
                {
                    tempScore += 400;
                }
                else
                {
                    tempScore += Math.Max((int)(600 - diff1), 0);
                }
                LeftKneeLift1 = stepFinished;
                stepsDone++;
            }

            else if (diff2 < 600)
            {
                int earnedScore = Math.Max((int)(600 - diff2), 0);
                if (earnedScore > 400)
                {
                    tempScore += 400;
                }
                else
                {
                    tempScore += Math.Max((int)(600 - diff2), 0);
                }
                LeftKneeLift2 = stepFinished;
                stepsDone++;
            }
        }
        private void RightKneeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLift1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightKneeLift2).TotalMilliseconds));

            if (diff1 < 600)
            {
                int earnedScore = Math.Max((int)(600 - diff1), 0);
                if (earnedScore > 400)
                {
                    tempScore += 400;
                }
                else
                {
                    tempScore += Math.Max((int)(600 - diff1), 0);
                }
                RightKneeLift1 = stepFinished;
                stepsDone++;
            }

            else if (diff2 < 600)
            {
                int earnedScore = Math.Max((int)(600 - diff2), 0);
                if (earnedScore > 400)
                {
                    tempScore += 400;
                }
                else
                {
                    tempScore += Math.Max((int)(600 - diff2), 0);
                }
                RightKneeLift2 = stepFinished;
                stepsDone++;
            }
        }

        

        private void AddEventTriggeredText(string val)
        {
            eventsTriggeredList.Add(val);
            if (eventsTriggeredList.Count > 10)
                eventsTriggeredList.Remove(eventsTriggeredList[0]);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            // Default XNA draw stuff...
            GraphicsDevice.Clear(Color.White);
            
            //device.Clear(Color.CornflowerBlue);

            // Intro video is playing...
            if (introPlaying)
            {
                spriteBatch.Begin();
                Texture2D texture = videoPlayer.GetTexture();
                if (texture != null)
                {
                    // Draw intro video
                    //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                }
                spriteBatch.End();
            }
            else // Intro video is no longer playing...
            {


                // Draw all the dance step icons.  Yellow if step has been performed.  White, otherwise.
                //spriteBatch.Begin();
                //spriteBatch.Draw(StepCrossOFF, buttonPositions[0], null, (currentDabke > DabkeSteps.Home1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepHomeOFF, buttonPositions[1], null, (currentDabke > DabkeSteps.Crossover1) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepCrossOFF, buttonPositions[2], null, (currentDabke > DabkeSteps.Home2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepHomeOFF, buttonPositions[3], null, (currentDabke > DabkeSteps.Crossover2) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepKickOFF, buttonPositions[4], null, (currentDabke > DabkeSteps.Home3) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.Draw(StepHomeOFF, buttonPositions[5], null, (currentDabke > DabkeSteps.Kick) ? Color.Yellow : Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                //spriteBatch.End();




                // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect 
                //DrawSkeleton(spriteBatch, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), jointTexture);

                //spriteBatch.Begin();

                //spriteBatch.End();


#if USE_KINECT
                // If we don't have a depth target, exit
                if (this.depthTexture == null)
                {
                    return;
                }
#else
                this.needToRedrawBackBuffer = true;
#endif

                //if (depthTexture != null && needToRedrawBackBuffer)
                if (this.needToRedrawBackBuffer)
                {
                    GraphicsDevice.SetRenderTarget(backBuffer);
                    //GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
#if USE_KINECT
                    depthTexture.SetData<short>(depthData);
                    SharedSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, null, kinectDepthVisualizer);
#endif
                    spriteBatch.Begin();
              
                    Texture2D texture = videoPlayer.GetTexture();
                    if (texture != null)
                    {
                        // Draw intro video
                        //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                        spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    }
                    spriteBatch.End();

                    /*spriteBatch.Begin();
                    for(int i=0; i<numberOfAnimationPlayers; i++)
                    {
                        spriteBatch.Draw(shadowTexture, shadowRects[i], Color.White);
                    }
                    spriteBatch.End();
#if USE_KINECT
                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, kinectDepthVisualizer);
#endif
                   DrawModels();*/
#if USE_KINECT
                    SharedSpriteBatch.Draw(depthTexture, Vector2.Zero, Color.White);
#endif
                    DrawText();
                    textFadeOut = textFadeOut.Subtract(gameTime.ElapsedGameTime);
#if USE_KINECT
                    SharedSpriteBatch.End();
#endif
                    GraphicsDevice.SetRenderTarget(null);
                    needToRedrawBackBuffer = false;
                }
#if USE_KINECT
                SharedSpriteBatch.Begin();
                SharedSpriteBatch.Draw(
                    this.backBuffer,
                    new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT),
                    null,
                    Color.White);
                SharedSpriteBatch.End();
#endif
            }

            base.Draw(gameTime); // Draw base XNA stuff...
        }

        private void DrawSkeleton(SpriteBatch spriteBatch, Vector2 resolution, Texture2D img)
        {
#if USE_KINECT
            // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect
            if (skeleton != null)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Red);
                }
            }
#endif
        }

        private void DrawDebugString(SpriteFont fontVal, Color colorVal, int x, int y, string stringVal)
        {
            Color textShadowColor = Color.Black;
            textShadowColor.A = colorVal.A;
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x + 2, y + 2), textShadowColor);
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x, y), colorVal);
        }

        private void DrawModels()
        {
            Matrix[] bones;
            Vector3 partnerRightHand = new Vector3(0, 0, 0);

            for (int i = 0; i < numberOfAnimationPlayers; i++)
            {
                // Grab 3d model skeleton matricies
                bones = animationPlayers[i].GetSkinTransforms();

                // Move the left hand of every dancer to the right hand of the previous
                if (i != 0)
                    bones[12].Translation = partnerRightHand; //11 is left hand
                partnerRightHand = bones[16].Translation + new Vector3(6.0f, -0.8f, 0.0f); // Right hand loc + Magic offset


                // Compute camera matrices.
                Matrix view = Matrix.CreateTranslation(cameraPosition) *
                              Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotation)) *
                              Matrix.CreateRotationX(MathHelper.ToRadians(cameraArc)) *
                              Matrix.CreateLookAt(new Vector3(0, 0, -cameraDistance),
                                                  new Vector3(0, 0, 0), Vector3.Up);

                Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                                                                        GraphicsDevice.Viewport.AspectRatio,
                                                                        1,
                                                                        10000);

                Matrix world = Matrix.Identity;
                Vector3 shadowVect = GraphicsDevice.Viewport.Project(bones[3].Translation, projection, view, world);
                shadowRects[i] = new Rectangle((int)(shadowVect.X*1.6)-120, 435, 200, 34);


                ModelMeshCollection tempMeshes = (i % 2 == 0) ? modelMan.Meshes : modelWoman.Meshes;
                // Render the skinned mesh.
                foreach (ModelMesh mesh in tempMeshes)
                {
                    foreach (SkinnedEffect effect in mesh.Effects)
                    {
                        effect.SetBoneTransforms(bones);

                        effect.View = view;
                        effect.Projection = projection;

                        effect.EnableDefaultLighting();

                        effect.SpecularColor = new Vector3(0.25f);
                        effect.SpecularPower = 16;
                    }

                    mesh.Draw();
                }
            }
        }

        private void DrawText()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            if (bShowDebugText)
            {
                displayScoreText = String.Format("{0,5}", displayScore);
                //Vector2 scoreTextSize = resultFont.MeasureString(displayScoreText);
                //DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreTextSize.X), 10, displayScoreText);
                Vector2 scoreSize = font.MeasureString(displayScoreText);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreSize.X - 10), 10, displayScoreText);
                //DrawDebugString(font, Color.White, 20, 400, "Animation Time: " + animationPlayers[0].CurrentTime.Seconds.ToString() + "." + animationPlayers[0].CurrentTime.Milliseconds.ToString() + "s");

                //for (int i = 0; i < eventsTriggeredList.Count; i++)
                //    DrawDebugString(font, Color.White, 10, 10 + (i * 20), eventsTriggeredList[i]);
            }

            double textFadeOutAmt = textFadeOut.TotalMilliseconds / 2000.0;

            resultColor.A = (byte)Math.Max((255 * textFadeOutAmt), 0);
            Vector2 resultSize = resultFont.MeasureString(resultString);
            DrawDebugString(resultFont, resultColor, (int)(WINDOW_WIDTH-resultSize.X)/2, 10, resultString);

            Color recentScoreColor = Color.White;
            recentScoreColor.A = (byte)Math.Max(255 * Math.Pow(textFadeOutAmt, 10), 0);
            Vector2 tempScoreSize = font.MeasureString(displayRecentScoreText);
            DrawDebugString(font, recentScoreColor, (int)(WINDOW_WIDTH - tempScoreSize.X - 10), 30, displayRecentScoreText);
            

            spriteBatch.End();
            //GraphicsDevice.BlendState = DefaultBlendState;
        }

        // Reset the FSM to start
        void timer_Reset(object sender, EventArgs e)
        {
            currentDabke = DabkeSteps.Home1;
        }

#if USE_KINECT
        void kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            if (bWantsToQuit)
                return;

            //throw new NotImplementedException();
            //DepthImageFrame frame = e.OpenDepthImageFrame();
            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame == null)
                    return;
                if (frame != null)
                {
                    if (null == depthData || depthData.Length != frame.PixelDataLength)
                    {
                        depthData = new short[frame.PixelDataLength];

                        depthTexture = new Texture2D(
                            GraphicsDevice,
                            frame.Width,
                            frame.Height,
                            false,
                            SurfaceFormat.Bgra4444);

                        backBuffer = new RenderTarget2D(
                            GraphicsDevice,
                            frame.Width,
                            frame.Height,
                            false,
                            SurfaceFormat.Color,
                            DepthFormat.None,
                            GraphicsDevice.PresentationParameters.MultiSampleCount,
                            RenderTargetUsage.PreserveContents);

                        spriteBatch = new SpriteBatch(GraphicsDevice);
                    }


                    frame.CopyPixelDataTo(depthData);
                    needToRedrawBackBuffer = true;
                }
            }
        }

        // This runs every time Kinect has an updated image
        void kinect_AllFramesReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if (bWantsToQuit)
                return;

            //throw new NotImplementedException();

            // Store all current Kinect skeletal info...
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    //Copy the skeleton data to our array
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                }
            }

            // If there is valid skelton data, examine for dance steps
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        skeleton = skel;
                        int headCounterTrigger = 0;
                        int leftFootCounterTrigger = 0;
                        int rightFootCounterTrigger = 0;

                        #region HeadAndFootDirection
                        // Kinda cheating here... Keeping track of the head movement instead of relying on 
                        // the Kinect tracking crossed legs.  So, yes, you can cheat by bobbing up and down...
                        if (previousHeadY > skeleton.Joints[JointType.Head].Position.Y)
                        {
                            // Head was moving down, but now is moving up.
                            if (headYCounter > 0)
                            {
                                headCounterTrigger = headYCounter; // Store this number and see if it triggers a move
                                headYCounter = -1;
                            }
                            else
                                headYCounter--;  // Head is still moving down.
                        }
                        else
                        {
                            // Head was moving up, but now is moving down.
                            if (headYCounter < 0)
                            {
                                headCounterTrigger = headYCounter; // Store this number and see if it triggers a move
                                headYCounter = 1;
                            }
                            else
                                headYCounter++; // Head is still moving up.
                        }

                        // Do the same thing for Left Foot
                        if (previousLeftFootZ > skeleton.Joints[JointType.FootLeft].Position.Z)
                        {
                            // Left foot was moving away from Kinect, but is now mocing towards Kinect.
                            if (leftFootZCounter > 0)
                            {
                                leftFootCounterTrigger = leftFootZCounter; // Store this number and see if it triggers a move
                                leftFootZCounter = -1;
                            }
                            else
                                leftFootZCounter--;  // Left foot still moving away from Kinect
                        }
                        else
                        {
                            // Left foot was moving towards Kinect, now is moving away from it.
                            if (leftFootZCounter < 0)
                            {
                                leftFootCounterTrigger = leftFootZCounter; // Store this number and see if it triggers a move
                                leftFootZCounter = 1;
                            }
                            else
                                leftFootZCounter++; // Left foot is still away.
                        }

                        if (previousRightFootZ > skeleton.Joints[JointType.FootRight].Position.Z)
                        {
                            // Head was moving down, but now is moving up.
                            if (rightFootZCounter > 0)
                            {
                                rightFootCounterTrigger = rightFootZCounter; // Store this number and see if it triggers a move
                                rightFootZCounter = -1;
                            }
                            else
                                rightFootZCounter--;  // Head is still moving down.
                        }
                        else
                        {
                            // Head was moving up, but now is moving down.
                            if (rightFootZCounter < 0)
                            {
                                rightFootCounterTrigger = rightFootZCounter; // Store this number and see if it triggers a move
                                rightFootZCounter = 1;
                            }
                            else
                                rightFootZCounter++; // Head is still moving up.
                        }
                        #endregion
                        float lky = skeleton.Joints[JointType.KneeLeft].Position.Y;
                        float lkx = skeleton.Joints[JointType.KneeLeft].Position.X;
                        float rky = skeleton.Joints[JointType.KneeRight].Position.Y;
                        float rkx = skeleton.Joints[JointType.KneeRight].Position.X;
                        TimeSpan time = videoTime.Elapsed;

                        if( (skeleton.Joints[JointType.KneeLeft].Position.Y - skeleton.Joints[JointType.KneeRight].Position.Y) > .21
                            && Math.Abs(skeleton.Joints[JointType.KneeLeft].Position.X - skeleton.Joints[JointType.KneeRight].Position.X) > .25)
                            LeftKneeTriggered();
                        if ((skeleton.Joints[JointType.KneeLeft].Position.Y - skeleton.Joints[JointType.KneeRight].Position.Y) < -.21
                            && Math.Abs(skeleton.Joints[JointType.KneeLeft].Position.X - skeleton.Joints[JointType.KneeRight].Position.X) > .25)
                            RightKneeTriggered();

                        // Old, nasty code to advance horrible dance FSM.
                        switch (currentDabke)
                        {
                            case DabkeSteps.Home1:
                                //if (Math.Abs(skeleton.Joints[JointType.FootRight].Position.X - skeleton.Joints[JointType.FootLeft].Position.X) < .025)
                                //if (Math.Abs(skeleton.Joints[JointType.FootLeft].Position.Z - skeleton.Joints[JointType.KneeLeft].Position.Z) > .075)
                                if (headYCounter < -10)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Crossover1:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) < .05)
                                if (headYCounter > 10)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Home2:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X + .05f) < skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) > 0.075)
                                if (headYCounter < -5)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Crossover2:
                                //if ((skeleton.Joints[JointType.FootRight].Position.X - .05f) > skeleton.Joints[JointType.FootLeft].Position.X)
                                //if (Math.Abs(skeleton.Joints[JointType.KneeRight].Position.Z - skeleton.Joints[JointType.Head].Position.Z) < .05)
                                if (headYCounter > 5)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Home3:
                                if ((skeleton.Joints[JointType.FootLeft].Position.Z + .2f) < skeleton.Joints[JointType.KneeLeft].Position.Z)
                                    currentDabke++;
                                break;

                            case DabkeSteps.Kick:
                                if (Math.Abs(skeleton.Joints[JointType.FootLeft].Position.Z - skeleton.Joints[JointType.KneeLeft].Position.Z) < .05)
                                    currentDabke++;
                                break;

                            case DabkeSteps.WaitForReset:

                                currentDabke = DabkeSteps.Home1;
                                //videoTime.Tick += new EventHandler(timer_Reset); // Everytime timer ticks, timer_Tick will be called
                                //videoTime.Interval = (1000) * (1);             // Timer will tick every 10 seconds
                                //videoTime.Enabled = true;                       // Enable the timer
                                //videoTime.Start();                              // Start the timer

                                break;
                        }

                        previousHeadY = skeleton.Joints[JointType.Head].Position.Y;
                        previousLeftFootZ = skeleton.Joints[JointType.FootLeft].Position.Z;
                        previousRightFootZ = skeleton.Joints[JointType.FootRight].Position.Z;

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This method maps a SkeletonPoint to the depth frame.
        /// </summary>
        /// <param name="point">The SkeletonPoint to map.</param>
        /// <returns>A Vector2 of the location on the depth frame.</returns>
        private Vector2 SkeletonToDepthMap(SkeletonPoint point)
        {
            if ((null != kinect) && (null != kinect.DepthStream))
            {
                // This is used to map a skeleton point to the depth image location
                var depthPt = kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(point, kinect.DepthStream.Format);
                return new Vector2(depthPt.X, depthPt.Y);
            }

            return Vector2.Zero;
        }
#endif
    }
}