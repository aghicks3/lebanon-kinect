#define USE_KINECT // Comment out this line to test without a Kinect!!!
//irene version

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
//using SkinnedModel;
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
        //Important Drew Hicks Refactor
        enum GameState { ATTRACT, MENU, HOWTOPLAY, DANCE, SCORE, QUIT};
        int gameState = (int)GameState.ATTRACT;

        Dance dance1 = new Dance("Lebanon");
        Dance dance2 = new Dance("Lebanon2");
        Dance selectedDance; 

        // Declaring variables...
        bool bWantsToQuit = false;

        const int SCORING_WINDOW_EASY = 600;
        const int SCORING_WINDOW_MEDIUM = 300;
        const int SCORING_WINDOW_HARD = 150;
        //int GLOBALS.SCORING_WINDOW = SCORING_WINDOW_MEDIUM;

#if USE_KINECT
        KinectSensor kinect;
        Skeleton[] skeletonData;

        int P1skeleton; //keep tracking IDs for players
        int P2skeleton; //keep tracking IDs for players
        Boolean debugging = true;
#endif
        
        const int WINDOW_WIDTH = 640;//720;
        const int WINDOW_HEIGHT = 480;

        // Basic XNA 3d variables
        GraphicsDeviceManager graphics;
        Vector3 modelPosition = Vector3.Zero;
        Vector3 cameraPosition = new Vector3(-2.25f, 0.5f, -1.35f);

        Effect kinectDepthVisualizer;
        bool needToRedrawBackBuffer;
        Rectangle shadowRect = new Rectangle(0, 0, 64, 64);
        RenderTarget2D backBuffer;
        Texture2D depthTexture;
        short[] depthData;


        // FSM variables
        SpriteBatch spriteBatch; //Draws all 2d images and text
        Texture2D jointTexture, shadowTexture;
        Texture2D StepHomeOFF, StepCrossOFF, StepKickOFF;
        Stopwatch videoTime = new Stopwatch();
        Stopwatch introVideoTime = new Stopwatch();
        Vector2[] buttonPositions = { new Vector2(5, 5), new Vector2(75, 5), new Vector2(145, 5), new Vector2(215, 5), new Vector2(285, 5), new Vector2(355, 5) }; //currently unused


        // Dancer Sprite Variables
        Texture2D female_BackSpinRightKneeLift, female_crossover, female_Crouch_HipShake, female_Crouch_HipSwivel;
        Texture2D female_ElbowSway, female_HandSwingBack, female_HandSwingFront, female_HandSwingLeft, female_HandSwingRight;
        Texture2D female_HipShakeBack, female_HipShakeFront, female_Home, female_LeftBendHipShake, female_LeftKneeBendCrouch_Left;
        Texture2D female_LeftKneeBendCrouch_Right, female_LeftKneeKick, female_LeftKneeLift, female_LeftKneeLift_FaceLeft;
        Texture2D female_LeftKneeLift_FrontTorso, female_RightHandHigh, female_rightKneeKick, female_RightKneeLift;
        Texture2D female_RightKneeLift_FaceLeft, female_ScrollingHands_Left, female_ScrollingHands_Right;
        Texture2D female_ThrillerhandsLeft, female_WristArcRaise_Left, female_WristArcRaise_Right;

        Texture2D male_crossover, male_KneelandClap, male_LeftHandToFace_Spin, male_LeftKneeBendCrouch_Left;
        Texture2D male_LeftKneeBendCrouch_Right, male_LeftKneeCross, male_LeftKneeKick, male_LeftKneeLift;
        Texture2D male_LeftKneeLift_FaceBack, male_LeftKneeLift_FaceLeft, male_LeftKneeLift_FaceRight;
        Texture2D male_LeftKneeLift_FrontTorso, male_LeftKneeLift_FrontTorso_LeftHand, male_LeftKneeLift_LeftHand;
        Texture2D male_LeftKneeLift_UnderArm, male_RightKneeCross, male_rightKneeKick, male_RightKneeKick_UnderArm;
        Texture2D male_RightKneeKneel_UnderArm, male_RightKneeKneel_UnderArm_HandBehind, male_rightKneeLift;
        Texture2D male_RightKneeLift_FaceRight, male_RightKneeLift_LeftHand, male_shrug, male_WaiterHand;

        Texture2D n_MoveTarget;
        Texture2D n_P1icon;
        Texture2D n_P2icon;

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

        bool rightFootCrossed = false;
        bool arcedHands = false;

        int stepsDone = 0;
        int tempStepsDone = 0;
        //
        TimeSpan stepFinished = new TimeSpan(0, 0, 3, 0, 0);
        TimeSpan danceVideoLength = new TimeSpan(0, 0, 0, 5, 0);
        TimeSpan femPlayerRecog = new TimeSpan(0, 0, 3, 0, 0);
        TimeSpan malePlayerRecog = new TimeSpan(0, 0, 3, 0, 0);

        bool femPlayingText = false;
        bool malePlayingText = false;

        SpriteFont font;
        SpriteFont resultFont;
        Color resultColor, resultColorF;
        string resultString = " ";
        string resultStringF = " ";
        string displayScoreText = " ";
        string displayScoreTextF = " ";
        string displayRecentScoreText = " ";
        string displayRecentScoreTextF = " ";
        int totalScore = 0;
        int totalScoreF = 0;
        int setScore = 0;
        int setScoreF = 0;
        int displayScore = 0;
        int displayScoreF = 0;
        int tempScore = 0;
        int tempScoreF = 0;
        static int maleRectDiff = WINDOW_WIDTH / 2 - 100;

        //First Male move
        TimeSpan RightKneeLift1;
        TimeSpan LeftKneeLift1;
        TimeSpan RightKneeLift2;
        TimeSpan LeftKneeLift2;
        TimeSpan introScore;

        //Second score block
        TimeSpan LeftKneeLift3;
        TimeSpan RightKneeLift3;
        TimeSpan LeftKneeLiftAndFrontTorso1;
        TimeSpan RightKneeLiftAndBackTorso1;
        TimeSpan LeftKneeLift4;
        TimeSpan RightKneeLift4;
        TimeSpan LeftKneeLiftAndFrontTorso2;
        TimeSpan RightKneeLiftAndBackTorso2;
        TimeSpan Score2;
        
        //Third Score block
        TimeSpan LeftKneeLiftAndLeftHand1;
        TimeSpan RightKneeLiftAndLeftHand1;
        TimeSpan LeftKneeLiftAndFrontTorsoAndLeftHand1;
        TimeSpan RightKneeLiftAndBackTorsoAndLeftHand1;
        TimeSpan LeftKneeLiftAndLeftHand2;
        TimeSpan RightKneeLiftAndLeftHand2;
        TimeSpan LeftKneeLiftAndFrontTorsoAndLeftHand2;
        TimeSpan RightKneeLiftAndBackTorsoAndLeftHand2;
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
        TimeSpan LeftKneeBendCrouch0B;
        TimeSpan LeftKneeBendCrouch0A;
        TimeSpan Score6;

        //Seventh score block
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
        TimeSpan LeftKneeKick17 = new TimeSpan(0, 0, 0, 88, 099);
        TimeSpan RightKneeLift13 = new TimeSpan(0, 0, 0, 88, 385);
        TimeSpan LeftKneeLift18 = new TimeSpan(0, 0, 0, 88, 686);
        TimeSpan LeftKneeKick18 = new TimeSpan(0, 0, 0, 89, 255);
        TimeSpan RightKneeLift14 = new TimeSpan(0, 0, 0, 89, 508);
        TimeSpan LeftKneeLift19 = new TimeSpan(0, 0, 0, 90, 059);
        TimeSpan RightKneeKick5 = new TimeSpan(0, 0, 0, 91, 250);

        TimeSpan LeftKneeLift20 = new TimeSpan(0, 0, 0, 92, 038);
        TimeSpan LeftKneeKick20 = new TimeSpan(0, 0, 0, 92, 424);
        TimeSpan RightKneeLift15 = new TimeSpan(0, 0, 0, 92, 675);
        TimeSpan LeftKneeLift21 = new TimeSpan(0, 0, 0, 92, 976);
        TimeSpan LeftKneeKick21 = new TimeSpan(0, 0, 0, 93, 529);
        TimeSpan RightKneeLift16 = new TimeSpan(0, 0, 0, 93, 831);
        TimeSpan LeftKneeLift22 = new TimeSpan(0, 0, 0, 94, 569);
        TimeSpan RightKneeKick6 = new TimeSpan(0, 0, 0, 95, 609);
        TimeSpan Score10 = new TimeSpan(0, 0, 0, 96, 681);

        TimeSpan LeftKneeLiftAndFrontTorso3 = new TimeSpan(0, 0, 0, 96, 336);
        TimeSpan RightKneeKick7 = new TimeSpan(0, 0, 0, 97, 508);
        TimeSpan LeftKneeLift23 = new TimeSpan(0, 0, 0, 98, 480);
        TimeSpan RightKneeKick8 = new TimeSpan(0, 0, 0, 99, 621);
        TimeSpan LeftKneeLift24 = new TimeSpan(0, 0, 0, 100, 660);
        TimeSpan RightKneeKick9 = new TimeSpan(0, 0, 0, 101, 734);
        TimeSpan LeftKneeLiftAndFrontTorso4 = new TimeSpan(0, 0, 0, 102, 738);
        TimeSpan RightKneeKick10 = new TimeSpan(0, 0, 0, 103, 833);
        TimeSpan LeftKneeKick = new TimeSpan(0, 0, 0, 104, 887);
        TimeSpan RightKneeKick = new TimeSpan(0, 0, 0, 105, 974);
        TimeSpan Score11 = new TimeSpan(0, 0, 0, 106, 988);

        TimeSpan LeftKneeBendCrouch7 = new TimeSpan(0, 0, 0, 107, 526);
        TimeSpan LeftKneeBendCrouch8 = new TimeSpan(0, 0, 0, 108, 047);
        TimeSpan RightFootCross9 = new TimeSpan(0, 0, 0, 109, 101);
        TimeSpan RightFootSwing9 = new TimeSpan(0, 0, 0, 109, 620);
        TimeSpan RightFootCross10 = new TimeSpan(0, 0, 0, 110, 191);
        TimeSpan RightFootSwing10 = new TimeSpan(0, 0, 0, 110, 711);
        TimeSpan LeftKneeBendCrouch9 = new TimeSpan(0, 0, 0, 111, 248);
        TimeSpan LeftKneeBendCrouch10 = new TimeSpan(0, 0, 0, 112, 356);
        TimeSpan RightFootCross11 = new TimeSpan(0, 0, 0, 113, 393);
        TimeSpan RightFootSwing11 = new TimeSpan(0, 0, 0, 113, 879);
        TimeSpan RightFootCross12 = new TimeSpan(0, 0, 0, 114, 449);
        TimeSpan RightFootSwing12 = new TimeSpan(0, 0, 0, 115, 019);
        TimeSpan LeftKneeBendCrouch11 = new TimeSpan(0, 0, 0, 115, 572);
        TimeSpan LeftKneeBendCrouch12 = new TimeSpan(0, 0, 0, 116, 611);
        TimeSpan RightFootCross13 = new TimeSpan(0, 0, 0, 117, 619);
        TimeSpan RightFootSwing13 = new TimeSpan(0, 0, 0, 118, 207);
        TimeSpan RightFootCross14 = new TimeSpan(0, 0, 0, 118, 773);
        TimeSpan RightFootSwing14 = new TimeSpan(0, 0, 0, 119, 326);
        TimeSpan LeftKneeBendCrouch13 = new TimeSpan(0, 0, 0, 119, 880);
        TimeSpan LeftKneeBendCrouch14 = new TimeSpan(0, 0, 0, 120, 906);
        TimeSpan RightFootCross15 = new TimeSpan(0, 0, 0, 121, 892);
        TimeSpan RightFootSwing15 = new TimeSpan(0, 0, 0, 122, 462);
        TimeSpan RightFootCross16 = new TimeSpan(0, 0, 0, 123, 033);
        TimeSpan RightFootSwing16 = new TimeSpan(0, 0, 0, 123, 551);
        TimeSpan Score12 = new TimeSpan(0, 0, 0, 124, 200);

        TimeSpan LeftKneeLiftAndFrontTorso5 = new TimeSpan(0, 0, 0, 124, 217);
        TimeSpan RightKneeKickAndUnderArm1 = new TimeSpan(0, 0, 0, 125, 309);
        TimeSpan LeftKneeLiftAndUnderArm = new TimeSpan(0, 0, 0, 126, 413);
        TimeSpan RightKneeKickAndUnderArm2 = new TimeSpan(0, 0, 0, 127, 537);
        TimeSpan RightKneeKneelAndUnderArm = new TimeSpan(0, 0, 0, 128, 576);
        TimeSpan RightKneeKneelAndUnderArmAndLeftHandBehind = new TimeSpan(0, 0, 0, 132, 163);
        TimeSpan Score13 = new TimeSpan(0, 0, 0, 132, 800);

        TimeSpan gameEnd;
        TimeSpan restartGameLoop;
        TimeSpan showInstructions = new TimeSpan(0, 0, 0, 10, 0);

        TimeSpan marker;

        /*Female Times //////////////////
        */

        //First Female move
        int femStepsDone = 0;
        int tempFemStepsDone = 0;
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
        TimeSpan FemLeftFootLiftAndFrontTorso2;
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
        TimeSpan FemLeftKneeBendCrouch0B;
        TimeSpan FemLeftKneeBendCrouch0A;


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
        TimeSpan FemRightElbowSway1;
        TimeSpan FemLeftElbowSway1;
        TimeSpan FemRightElbowSway2;
        TimeSpan FemLeftElbowSway2;
        TimeSpan FemRightElbowSway3;
        TimeSpan FemLeftElbowSway3;
        TimeSpan FemRightElbowSway4;
        TimeSpan FemLeftElbowSway4;

        //Ninth Score block
        TimeSpan FemLeftWristArcRaise1;
        TimeSpan FemRightWristArcRaise1;
        TimeSpan FemHome1; //Hands at sides detection after fluttering hands down
        TimeSpan FemLeftWristArcRaise2;
        TimeSpan FemRightWristArcRaise2;
        TimeSpan FemHome2; //Hands at sides detection after fluttering hands down

        //Tenth Score block
        TimeSpan FemThrillerHandsLeft1;
        TimeSpan FemLeftBendHipShake1;
        TimeSpan FemThrillerHandsLeft2;
        TimeSpan FemLeftBendHipShake2;

        //Eleventh Score block
        TimeSpan FemRightElbowSway5;
        TimeSpan FemLeftElbowSway5;
        TimeSpan FemRightElbowSway6;
        TimeSpan FemLeftElbowSway6;
        TimeSpan FemLeftKneeLift5;
        TimeSpan FemRightKneeKick2;
        TimeSpan FemLeftKneeLiftAndFrontTorso5;
        TimeSpan FemRightKneeKick;
        TimeSpan FemLeftKneeLift6;
        TimeSpan FemRightKneeKick3;

        //Twelth Score block
        TimeSpan FemLeftKneeBendCrouch7;
        TimeSpan FemLeftKneeBendCrouch8;
        TimeSpan FemRightFootCross9;
        TimeSpan FemRightFootSwing9;
        TimeSpan FemRightFootCross10;
        TimeSpan FemRightFootSwing10;
        TimeSpan FemLeftKneeBendCrouch9;
        TimeSpan FemLeftKneeBendCrouch10;
        TimeSpan FemRightFootCross11;
        TimeSpan FemRightFootSwing11;
        TimeSpan FemRightFootCross12;
        TimeSpan FemRightFootSwing12;
        TimeSpan FemLeftKneeBendCrouch11;
        TimeSpan FemLeftKneeBendCrouch12;
        TimeSpan FemRightFootCross13;
        TimeSpan FemRightFootSwing13;
        TimeSpan FemRightFootCross14;
        TimeSpan FemRightFootSwing14;
        TimeSpan FemLeftKneeBendCrouch13;
        TimeSpan FemLeftKneeBendCrouch14;
        TimeSpan FemRightFootCross15;
        TimeSpan FemRightFootSwing15;
        TimeSpan FemRightFootCross16;
        TimeSpan FemRightFootSwing16;

        //Thirteenth Score block
        TimeSpan FemLeftKneeLiftAndFrontTorso2;
        TimeSpan FemBackSpinRightKneeLift1;
        TimeSpan FemForwardSpinFacingRightKneeLift;
        TimeSpan FemCrouchHipSwivel;
        TimeSpan FemRightHandHigh;

        public void setTime()
        {
            #region Scoring TimeSpans
            //Score block 1
            LeftKneeLift1 = new TimeSpan(0, 0, 0, 6, 562);
            RightKneeLift1 = new TimeSpan(0, 0, 0, 7, 602);
            LeftKneeLift2 = new TimeSpan(0, 0, 0, 8, 574);
            RightKneeLift2 = new TimeSpan(0, 0, 0, 9, 731);

            FemLeftKneeLift1 = new TimeSpan(0, 0, 0, 6, 562);
            FemRightKneeLift1 = new TimeSpan(0, 0, 0, 7, 602);
            FemLeftKneeLift2 = new TimeSpan(0, 0, 0, 8, 574);
            FemRightKneeLift2 = new TimeSpan(0, 0, 0, 9, 731);
            introScore = new TimeSpan(0, 0, 0, 10, 451);

            //Second score block
            LeftKneeLift3 = new TimeSpan(0, 0, 0, 10, 731);
            RightKneeLift3 = new TimeSpan(0, 0, 0, 11, 843);
            LeftKneeLiftAndFrontTorso1 = new TimeSpan(0, 0, 0, 12, 951);
            RightKneeLiftAndBackTorso1 = new TimeSpan(0, 0, 0, 14, 172);
            LeftKneeLift4 = new TimeSpan(0, 0, 0, 15, 146);
            RightKneeLift4 = new TimeSpan(0, 0, 0, 16, 169);
            LeftKneeLiftAndFrontTorso2 = new TimeSpan(0, 0, 0, 17, 170);
            RightKneeLiftAndBackTorso2 = new TimeSpan(0, 0, 0, 18, 294);

            FemLeftKneeLift3 = new TimeSpan(0, 0, 0, 10, 731);
            FemRightKneeLift3 = new TimeSpan(0, 0, 0, 11, 843);
            FemLeftFootLiftAndFrontTorso1 = new TimeSpan(0, 0, 0, 12, 951);
            FemRightFootLiftAndBackTorso1 = new TimeSpan(0, 0, 0, 14, 172);
            FemLeftKneeLift4 = new TimeSpan(0, 0, 0, 15, 146);
            FemRightKneeLift4 = new TimeSpan(0, 0, 0, 16, 169);
            FemLeftFootLiftAndFrontTorso2 = new TimeSpan(0, 0, 0, 17, 170);
            FemRightFootLiftAndBackTorso2 = new TimeSpan(0, 0, 0, 18, 294);
            Score2 = new TimeSpan(0, 0, 0, 18, 546);

            //Score block 3
            LeftKneeLiftAndLeftHand1 = new TimeSpan(0, 0, 0, 19, 355);
            RightKneeLiftAndLeftHand1 = new TimeSpan(0, 0, 0, 20, 311);
            LeftKneeLiftAndFrontTorsoAndLeftHand1 = new TimeSpan(0, 0, 0, 21, 427);
            RightKneeLiftAndBackTorsoAndLeftHand1 = new TimeSpan(0, 0, 0, 22, 556);
            LeftKneeLiftAndLeftHand2 = new TimeSpan(0, 0, 0, 23, 512);
            RightKneeLiftAndLeftHand2 = new TimeSpan(0, 0, 0, 24, 595);
            LeftKneeLiftAndFrontTorsoAndLeftHand2 = new TimeSpan(0, 0, 0, 25, 654);
            RightKneeLiftAndBackTorsoAndLeftHand2 = new TimeSpan(0, 0, 0, 26, 814);
            //LeftKneeLift11 and LeftHand1 = new TimeSpan(0,0,0,27,859); We're ignoring this step for now.

            FemHandSwingFront1 = new TimeSpan(0, 0, 0, 19, 492);
            FemHandSwingRight1 = new TimeSpan(0, 0, 0, 20, 615);
            FemHipShakeBack1 = new TimeSpan(0, 0, 0, 21, 151);
            FemHandSwingBack1 = new TimeSpan(0, 0, 0, 23, 834);
            FemHandSwingLeft1 = new TimeSpan(0, 0, 0, 25, 046);
            FemHipShakeFront1 = new TimeSpan(0, 0, 0, 25, 479);
            Score3 = new TimeSpan(0, 0, 0, 28, 343);

            //Score block 4
            KneelDownsAndClap = new TimeSpan(0, 0, 0, 28, 785);

            FemHandSwingFront2 = new TimeSpan(0, 0, 0, 28, 192);
            FemHandSwingRight2 = new TimeSpan(0, 0, 0, 29, 318);
            FemHipShakeBack2 = new TimeSpan(0, 0, 0, 29, 707);
            FemHandSwingBack2 = new TimeSpan(0, 0, 0, 32, 517);
            FemHandSwingLeft2 = new TimeSpan(0, 0, 0, 33, 562);
            FemHipShakeFront2 = new TimeSpan(0, 0, 0, 34, 045);
            Score4 = new TimeSpan(0, 0, 0, 35, 806);

            //Score block 5
            LeftKneeLift5 = new TimeSpan(0, 0, 0, 36, 590);
            RightKneeLift5 = new TimeSpan(0, 0, 0, 37, 460);
            LeftKneeLift6 = new TimeSpan(0, 0, 0, 38, 574);
            RightKneeLift6 = new TimeSpan(0, 0, 0, 39, 558);
            LeftKneeLift7 = new TimeSpan(0, 0, 0, 40, 706);
            RightKneeLift7 = new TimeSpan(0, 0, 0, 41, 809);
            LeftKneeLift8 = new TimeSpan(0, 0, 0, 42, 857);
            RightKneeLift8 = new TimeSpan(0, 0, 0, 43, 916);

            FemMoveToRightAndScrollingHands1 = new TimeSpan(0, 0, 0, 36, 228);
            FemMoveToRightAndScrollingHands2 = new TimeSpan(0, 0, 0, 37, 496);
            FemCrouchAndHipShake1 = new TimeSpan(0, 0, 0, 38, 552);
            FemMoveToLeftAndScrollingHands1 = new TimeSpan(0, 0, 0, 40, 698);
            FemMoveToLeftAndScrollingHands2 = new TimeSpan(0, 0, 0, 41, 670);
            FemCrouchAndHipShake2 = new TimeSpan(0, 0, 0, 42, 728);
            Score5 = new TimeSpan(0, 0, 0, 44, 494);

            //Score block 6
            MoveToRightAndWaiterHand = new TimeSpan(0, 0, 0, 44, 751); //Just detect waiter hand bones?
            ShrugShoulders = new TimeSpan(0, 0, 0, 46, 612);
            LeftKneeLift9 = new TimeSpan(0, 0, 0, 49, 288);
            RightKneeLift9 = new TimeSpan(0, 0, 0, 50, 273);
            LeftKneeLift10 = new TimeSpan(0, 0, 0, 51, 299);
            RightKneeLift10 = new TimeSpan(0, 0, 0, 52, 379);
            LeftKneeBendCrouch0B = new TimeSpan(0, 0, 0, 53, 649);
            LeftKneeBendCrouch0A = new TimeSpan(0, 0, 0, 54, 688);

            FemMoveToRightAndScrollingHands3 = new TimeSpan(0, 0, 0, 44, 890);
            FemMoveToRightAndScrollingHands4 = new TimeSpan(0, 0, 0, 45, 944);
            FemCrouchAndHipShake3 = new TimeSpan(0, 0, 0, 47, 034);
            FemMoveToLeftAndScrollingHands3 = new TimeSpan(0, 0, 0, 49, 112);
            FemMoveToLeftAndScrollingHands4 = new TimeSpan(0, 0, 0, 50, 252);
            FemCrouchAndHipShake4 = new TimeSpan(0, 0, 0, 51, 309);
            FemLeftKneeBendCrouch0B = new TimeSpan(0,0,0,53,649);
            FemLeftKneeBendCrouch0A = new TimeSpan(0,0,0,54,688);
            Score6 = new TimeSpan(0, 0, 0, 53, 225);

            //Score block 7
            RightFootCross1 = new TimeSpan(0, 0, 0, 55, 405);
            RightFootSwing1 = new TimeSpan(0, 0, 0, 55, 876);
            RightFootCross2 = new TimeSpan(0, 0, 0, 56, 544);
            RightFootSwing2 = new TimeSpan(0, 0, 0, 57, 210);
            LeftKneeBendCrouch1 = new TimeSpan(0, 0, 0, 57, 801);
            LeftKneeBendCrouch2 = new TimeSpan(0, 0, 0, 58, 857);
            RightFootCross3 = new TimeSpan(0, 0, 0, 59, 795);
            RightFootSwing3 = new TimeSpan(0, 0, 0, 60, 100);
            RightFootCross4 = new TimeSpan(0, 0, 0, 60, 768);
            RightFootSwing4 = new TimeSpan(0, 0, 0, 61, 294);
            LeftKneeBendCrouch3 = new TimeSpan(0, 0, 0, 62, 125);
            LeftKneeBendCrouch4 = new TimeSpan(0, 0, 0, 63, 181);
            RightFootCross5 = new TimeSpan(0, 0, 0, 64, 020);
            RightFootSwing5 = new TimeSpan(0, 0, 0, 64, 510);
            RightFootCross6 = new TimeSpan(0, 0, 0, 65, 058);
            RightFootSwing6 = new TimeSpan(0, 0, 0, 65, 604);
            LeftKneeBendCrouch5 = new TimeSpan(0, 0, 0, 66, 384);
            LeftKneeBendCrouch6 = new TimeSpan(0, 0, 0, 67, 406);
            RightFootCross7 = new TimeSpan(0, 0, 0, 68, 783);
            RightFootSwing7 = new TimeSpan(0, 0, 0, 68, 981);
            RightFootCross8 = new TimeSpan(0, 0, 0, 69, 355);
            RightFootSwing8 = new TimeSpan(0, 0, 0, 69, 896);

            FemRightFootCross1 = new TimeSpan(0, 0, 0, 55, 405);
            FemRightFootSwing1 = new TimeSpan(0, 0, 0, 55, 876);
            FemRightFootCross2 = new TimeSpan(0, 0, 0, 56, 544);
            FemRightFootSwing2 = new TimeSpan(0, 0, 0, 57, 210);
            FemLeftKneeBendCrouch1 = new TimeSpan(0, 0, 0, 57, 801);
            FemLeftKneeBendCrouch2 = new TimeSpan(0, 0, 0, 58, 857);
            FemRightFootCross3 = new TimeSpan(0, 0, 0, 59, 795);
            FemRightFootSwing3 = new TimeSpan(0, 0, 0, 60, 100);
            FemRightFootCross4 = new TimeSpan(0, 0, 0, 60, 768);
            FemRightFootSwing4 = new TimeSpan(0, 0, 0, 61, 294);
            FemLeftKneeBendCrouch3 = new TimeSpan(0, 0, 0, 62, 125);
            FemLeftKneeBendCrouch4 = new TimeSpan(0, 0, 0, 63, 181);
            FemRightFootCross5 = new TimeSpan(0, 0, 0, 64, 020);
            FemRightFootSwing5 = new TimeSpan(0, 0, 0, 64, 510);
            FemRightFootCross6 = new TimeSpan(0, 0, 0, 65, 058);
            FemRightFootSwing6 = new TimeSpan(0, 0, 0, 65, 604);
            FemLeftKneeBendCrouch5 = new TimeSpan(0, 0, 0, 66, 384);
            FemLeftKneeBendCrouch6 = new TimeSpan(0, 0, 0, 67, 406);
            FemRightFootCross7 = new TimeSpan(0, 0, 0, 68, 344);
            FemRightFootSwing7 = new TimeSpan(0, 0, 0, 68, 783);
            FemRightFootCross8 = new TimeSpan(0, 0, 0, 69, 355);
            FemRightFootSwing8 = new TimeSpan(0, 0, 0, 69, 896);
            Score7 = new TimeSpan(0, 0, 0, 70, 787);

            //Score block 8
            LeftKneeLiftAndCross = new TimeSpan(0, 0, 0, 71, 366);
            RightKneeLiftAndCross = new TimeSpan(0, 0, 0, 72, 472);
            LeftKneeLift12 = new TimeSpan(0, 0, 0, 73, 007);
            RightKneeLift11 = new TimeSpan(0, 0, 0, 74, 028);
            LeftKneeLift13 = new TimeSpan(0, 0, 0, 75, 171);
            RightKneeLift12 = new TimeSpan(0, 0, 0, 76, 246);
            LeftHandtoFaceSpinForward = new TimeSpan(0, 0, 0, 76, 896);
            LeftHandtoFaceSpinBack = new TimeSpan(0, 0, 0, 77, 364);
            RightKneeKick2 = new TimeSpan(0, 0, 0, 78, 492);

            FemRightElbowSway1 = new TimeSpan(0, 0, 0, 70, 578);
            FemLeftElbowSway1 = new TimeSpan(0, 0, 0, 71, 617);
            FemRightElbowSway2 = new TimeSpan(0, 0, 0, 72, 623);
            FemLeftElbowSway2 = new TimeSpan(0, 0, 0, 73, 680);
            FemRightElbowSway3 = new TimeSpan(0, 0, 0, 74, 770);
            FemLeftElbowSway3 = new TimeSpan(0, 0, 0, 75, 858);
            FemRightElbowSway4 = new TimeSpan(0, 0, 0, 76, 948);
            FemLeftElbowSway4 = new TimeSpan(0, 0, 0, 78, 055);
            Score8 = new TimeSpan(0, 0, 0, 79, 292);

            //Score block 9
            LeftKneeLift14 = new TimeSpan(0, 0, 0, 79, 527);
            RightKneeKick3 = new TimeSpan(0, 0, 0, 80, 569);
            LeftKneeLift15 = new TimeSpan(0, 0, 0, 81, 707);
            RightKneeKick4 = new TimeSpan(0, 0, 0, 82, 797);
            LeftKneeLift16 = new TimeSpan(0, 0, 0, 83, 687);
            LeftKneeLiftLeft = new TimeSpan(0, 0, 0, 84, 460);
            LeftKneeLiftBack = new TimeSpan(0, 0, 0, 85, 646);
            LeftKneeLiftRight = new TimeSpan(0, 0, 0, 86, 574);

            FemLeftWristArcRaise1 = new TimeSpan(0, 0, 0, 79, 648);
            FemRightWristArcRaise1 = new TimeSpan(0, 0, 0, 80, 603);
            FemHome1 = new TimeSpan(0, 0, 0, 82, 602);
            FemLeftWristArcRaise2 = new TimeSpan(0, 0, 0, 83, 938);
            FemRightWristArcRaise2 = new TimeSpan(0, 0, 0, 85, 011);
            FemHome2 = new TimeSpan(0, 0, 0, 86, 859);
            Score9 = new TimeSpan(0, 0, 0, 87, 345);

            //Score block 10
            LeftKneeLift17 = new TimeSpan(0, 0, 0, 87, 668);
            LeftKneeKick17 = new TimeSpan(0, 0, 0, 88, 099);
            RightKneeLift13 = new TimeSpan(0, 0, 0, 88, 385);
            LeftKneeLift18 = new TimeSpan(0, 0, 0, 88, 686);
            LeftKneeKick18 = new TimeSpan(0, 0, 0, 89, 255);
            RightKneeLift14 = new TimeSpan(0, 0, 0, 89, 508);
            LeftKneeLift19 = new TimeSpan(0, 0, 0, 90, 059);
            RightKneeKick5 = new TimeSpan(0, 0, 0, 91, 250);

            LeftKneeLift20 = new TimeSpan(0, 0, 0, 92, 038);
            LeftKneeKick20 = new TimeSpan(0, 0, 0, 92, 424);
            RightKneeLift15 = new TimeSpan(0, 0, 0, 92, 675);
            LeftKneeLift21 = new TimeSpan(0, 0, 0, 92, 976);
            LeftKneeKick21 = new TimeSpan(0, 0, 0, 93, 529);
            RightKneeLift16 = new TimeSpan(0, 0, 0, 93, 831);
            LeftKneeLift22 = new TimeSpan(0, 0, 0, 94, 569);
            RightKneeKick6 = new TimeSpan(0, 0, 0, 95, 609);

            FemThrillerHandsLeft1 = new TimeSpan(0, 0, 0, 88, 301);
            FemLeftBendHipShake1 = new TimeSpan(0, 0, 0, 90, 413);
            FemThrillerHandsLeft2 = new TimeSpan(0, 0, 0, 92, 628);
            FemLeftBendHipShake2 = new TimeSpan(0, 0, 0, 95, 224);
            Score10 = new TimeSpan(0, 0, 0, 96, 681);

            //Score block 11
            LeftKneeLiftAndFrontTorso3 = new TimeSpan(0, 0, 0, 96, 336);
            RightKneeKick7 = new TimeSpan(0, 0, 0, 97, 508);
            LeftKneeLift23 = new TimeSpan(0, 0, 0, 98, 480);
            RightKneeKick8 = new TimeSpan(0, 0, 0, 99, 621);
            LeftKneeLift24 = new TimeSpan(0, 0, 0, 100, 660);
            RightKneeKick9 = new TimeSpan(0, 0, 0, 101, 734);
            LeftKneeLiftAndFrontTorso4 = new TimeSpan(0, 0, 0, 102, 738);
            RightKneeKick10 = new TimeSpan(0, 0, 0, 103, 833);
            LeftKneeKick = new TimeSpan(0, 0, 0, 104, 887);
            RightKneeKick = new TimeSpan(0, 0, 0, 105, 974);

            FemRightElbowSway5 = new TimeSpan(0, 0, 0, 96, 789);
            FemLeftElbowSway5 = new TimeSpan(0, 0, 0, 97, 862);
            FemRightElbowSway6 = new TimeSpan(0, 0, 0, 99, 035);
            FemLeftElbowSway6 = new TimeSpan(0, 0, 0, 99, 973);
            FemLeftKneeLift5 = new TimeSpan(0, 0, 0, 100, 980);
            FemRightKneeKick2 = new TimeSpan(0, 0, 0, 102, 086);
            FemLeftKneeLiftAndFrontTorso5 = new TimeSpan(0, 0, 0, 103, 112);
            FemRightKneeKick = new TimeSpan(0, 0, 0, 104, 198);
            FemLeftKneeLift6 = new TimeSpan(0, 0, 0, 105, 237);
            FemRightKneeKick3 = new TimeSpan(0, 0, 0, 106, 381);
            Score11 = new TimeSpan(0, 0, 0, 106, 988);

            //Score 12
            LeftKneeBendCrouch7 = new TimeSpan(0, 0, 0, 107, 526);
            LeftKneeBendCrouch8 = new TimeSpan(0, 0, 0, 108, 047);
            RightFootCross9 = new TimeSpan(0, 0, 0, 109, 046);
            RightFootSwing9 = new TimeSpan(0, 0, 0, 109, 501);
            RightFootCross10 = new TimeSpan(0, 0, 0, 110, 034);
            RightFootSwing10 = new TimeSpan(0, 0, 0, 110, 624);
            LeftKneeBendCrouch9 = new TimeSpan(0, 0, 0, 111, 248);
            LeftKneeBendCrouch10 = new TimeSpan(0, 0, 0, 112, 356);
            RightFootCross11 = new TimeSpan(0, 0, 0, 113, 354);
            RightFootSwing11 = new TimeSpan(0, 0, 0, 113, 814);
            RightFootCross12 = new TimeSpan(0, 0, 0, 114, 358);
            RightFootSwing12 = new TimeSpan(0, 0, 0, 114, 938);
            LeftKneeBendCrouch11 = new TimeSpan(0, 0, 0, 115, 572);
            LeftKneeBendCrouch12 = new TimeSpan(0, 0, 0, 116, 611);
            RightFootCross13 = new TimeSpan(0, 0, 0, 117, 662);
            RightFootSwing13 = new TimeSpan(0, 0, 0, 118, 069);
            RightFootCross14 = new TimeSpan(0, 0, 0, 118, 600);
            RightFootSwing14 = new TimeSpan(0, 0, 0, 119, 207);
            LeftKneeBendCrouch13 = new TimeSpan(0, 0, 0, 119, 880);
            LeftKneeBendCrouch14 = new TimeSpan(0, 0, 0, 120, 906);
            RightFootCross15 = new TimeSpan(0, 0, 0, 121, 919);
            RightFootSwing15 = new TimeSpan(0, 0, 0, 122, 409);
            RightFootCross16 = new TimeSpan(0, 0, 0, 122, 960);
            RightFootSwing16 = new TimeSpan(0, 0, 0, 123, 498);


            FemLeftKneeBendCrouch7 = new TimeSpan(0, 0, 0, 107, 526);
            FemLeftKneeBendCrouch8 = new TimeSpan(0, 0, 0, 108, 047);
            FemRightFootCross9 = new TimeSpan(0, 0, 0, 109, 046);
            FemRightFootSwing9 = new TimeSpan(0, 0, 0, 109, 501);
            FemRightFootCross10 = new TimeSpan(0, 0, 0, 110, 034);
            FemRightFootSwing10 = new TimeSpan(0, 0, 0, 110, 624);
            FemLeftKneeBendCrouch9 = new TimeSpan(0, 0, 0, 111, 248);
            FemLeftKneeBendCrouch10 = new TimeSpan(0, 0, 0, 112, 356);
            FemRightFootCross11 = new TimeSpan(0, 0, 0, 113, 354);
            FemRightFootSwing11 = new TimeSpan(0, 0, 0, 113, 814);
            FemRightFootCross12 = new TimeSpan(0, 0, 0, 114, 358);
            FemRightFootSwing12 = new TimeSpan(0, 0, 0, 114, 938);
            FemLeftKneeBendCrouch11 = new TimeSpan(0, 0, 0, 115, 572);
            FemLeftKneeBendCrouch12 = new TimeSpan(0, 0, 0, 116, 611);
            FemRightFootCross13 = new TimeSpan(0, 0, 0, 117, 662);
            FemRightFootSwing13 = new TimeSpan(0, 0, 0, 118, 069);
            FemRightFootCross14 = new TimeSpan(0, 0, 0, 118, 600);
            FemRightFootSwing14 = new TimeSpan(0, 0, 0, 119, 207);
            FemLeftKneeBendCrouch13 = new TimeSpan(0, 0, 0, 119, 880);
            FemLeftKneeBendCrouch14 = new TimeSpan(0, 0, 0, 120, 906);
            FemRightFootCross15 = new TimeSpan(0, 0, 0, 121, 919);
            FemRightFootSwing15 = new TimeSpan(0, 0, 0, 122, 409);
            FemRightFootCross16 = new TimeSpan(0, 0, 0, 122, 960);
            FemRightFootSwing16 = new TimeSpan(0, 0, 0, 123, 498);
            Score12 = new TimeSpan(0, 0, 0, 124, 200);

            //Score 13
            LeftKneeLiftAndFrontTorso5 = new TimeSpan(0, 0, 0, 124, 217);
            RightKneeKickAndUnderArm1 = new TimeSpan(0, 0, 0, 125, 309);
            LeftKneeLiftAndUnderArm = new TimeSpan(0, 0, 0, 126, 413);
            RightKneeKickAndUnderArm2 = new TimeSpan(0, 0, 0, 127, 537);
            RightKneeKneelAndUnderArm = new TimeSpan(0, 0, 0, 128, 576);
            RightKneeKneelAndUnderArmAndLeftHandBehind = new TimeSpan(0, 0, 0, 132, 163);

            //Female set times
            FemLeftKneeLiftAndFrontTorso2 = new TimeSpan(0, 0, 0, 124, 298);
            FemBackSpinRightKneeLift1 = new TimeSpan(0, 0, 0, 125, 765);
            FemForwardSpinFacingRightKneeLift = new TimeSpan(0, 0, 0, 127, 225);
            FemCrouchHipSwivel = new TimeSpan(0, 0, 0, 129, 471);
            FemRightHandHigh = new TimeSpan(0, 0, 0, 131, 988);
            Score13 = new TimeSpan(0, 0, 0, 132, 800);

            gameEnd = new TimeSpan(0, 0, 0, 136, 500);
            restartGameLoop = new TimeSpan(0, 0, 0, 146, 500);
            
            #endregion

            stepsDone = 0;
            femStepsDone = 0;
            tempStepsDone = 0;
            tempFemStepsDone = 0;
            rightFootCrossed = false;
            arcedHands = false;
            /*femPlaying = false;
            malePlaying = false;
            femPlayingText = false;
            malePlayingText = false;*/
            totalScore = 0;
            totalScoreF = 0;
            setScore = 0;
            setScoreF = 0;
            displayScore = 0;
            displayScoreF = 0;
            tempScore = 0;
            tempScoreF = 0;
        }


        Color p1JoinColor, p2JoinColor;
        string p1JoinString = "Player 1";
        string p2JoinString = "Player 2";
        TimeSpan p1textFadeOut = new TimeSpan(0, 0, 0);
        TimeSpan p2textFadeOut = new TimeSpan(0, 0, 0);

        Texture2D backgroundDabke, scoreBackground, howtoplay;
        Rectangle backgroundRect = new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT);
        List<Rectangle> shadowRects = new List<Rectangle>();

        List<string> eventsTriggeredList;

        protected Song song;
        VideoPlayer videoPlayer, videoPlayer2;
        Video video;
        Video video1;

        //bool gamePlaying = false;
        //bool scoreScreen = false;

        public LebaneseKinectGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreparingDeviceSettings += this.GraphicsDevicePreparingDeviceSettings;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.IsFullScreen = true;
            Content.RootDirectory = "Content";
            eventsTriggeredList = new List<string>();
            //for (int i = 0; i < numberOfAnimationPlayers; i++)
                //shadowRects.Add(new Rectangle(0, 0, 64, 64));
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
            font = Content.Load<SpriteFont>("myFont");
            resultFont = Content.Load<SpriteFont>("resultFont");

            howtoplay = Content.Load<Texture2D>("Textures\\instructions");

            kinectDepthVisualizer = Content.Load<Effect>("KinectDepthVisualizer");
            // Create and load all the dance step icons
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), spriteBatch);
            jointTexture = Content.Load<Texture2D>("Textures\\joint");
            shadowTexture = Content.Load<Texture2D>("Textures\\shadow");
            backgroundDabke = Content.Load<Texture2D>("Textures\\backgroundDabke");
            scoreBackground = Content.Load<Texture2D>("Textures\\scoreBackground");
            StepHomeOFF = Content.Load<Texture2D>("Textures\\dsteps1OFF");
            StepCrossOFF = Content.Load<Texture2D>("Textures\\dsteps2OFF");
            StepKickOFF = Content.Load<Texture2D>("Textures\\dsteps3OFF");

            //All dance sprites
            /*
            female_BackSpinRightKneeLift = Content.Load<Texture2D>("Sprites\\female_BackSpinRightKneeLift");
            female_crossover = Content.Load<Texture2D>("Sprites\\female_crossover");
            female_Crouch_HipShake = Content.Load<Texture2D>("Sprites\\female_Crouch_HipShake");
            female_Crouch_HipSwivel = Content.Load<Texture2D>("Sprites\\female_Crouch_HipSwivel");
            female_ElbowSway = Content.Load<Texture2D>("Sprites\\female_ElbowSway");
            female_HandSwingBack = Content.Load<Texture2D>("Sprites\\female_HandSwingBack");
            female_HandSwingFront = Content.Load<Texture2D>("Sprites\\female_HandSwingFront");
            female_HandSwingLeft = Content.Load<Texture2D>("Sprites\\female_HandSwingLeft");
            female_HandSwingRight = Content.Load<Texture2D>("Sprites\\female_HandSwingRight");
            female_HipShakeBack = Content.Load<Texture2D>("Sprites\\female_HipShakeBack");
            female_HipShakeFront = Content.Load<Texture2D>("Sprites\\female_HipShakeFront");
            female_Home = Content.Load<Texture2D>("Sprites\\female_Home");
            female_LeftBendHipShake = Content.Load<Texture2D>("Sprites\\female_LeftBendHipShake");
            female_LeftKneeBendCrouch_Left = Content.Load<Texture2D>("Sprites\\female_LeftKneeBendCrouch_Left");
            female_LeftKneeBendCrouch_Right = Content.Load<Texture2D>("Sprites\\female_LeftKneeBendCrouch_Right");
            female_LeftKneeKick = Content.Load<Texture2D>("Sprites\\female_LeftKneeKick");
            female_LeftKneeLift = Content.Load<Texture2D>("Sprites\\female_LeftKneeLift");
            female_LeftKneeLift_FaceLeft = Content.Load<Texture2D>("Sprites\\female_LeftKneeLift_FaceLeft");
            female_LeftKneeLift_FrontTorso = Content.Load<Texture2D>("Sprites\\female_LeftKneeLift_FrontTorso");
            female_RightHandHigh = Content.Load<Texture2D>("Sprites\\female_RightHandHigh");
            female_rightKneeKick = Content.Load<Texture2D>("Sprites\\female_rightKneeKick");
            female_RightKneeLift = Content.Load<Texture2D>("Sprites\\female_RightKneeLift");
            female_RightKneeLift_FaceLeft = Content.Load<Texture2D>("Sprites\\female_RightKneeLift_FaceLeft");
            female_ScrollingHands_Left = Content.Load<Texture2D>("Sprites\\female_ScrollingHands_Left");
            female_ScrollingHands_Right = Content.Load<Texture2D>("Sprites\\female_ScrollingHands_Right");
            female_ThrillerhandsLeft = Content.Load<Texture2D>("Sprites\\female_ThrillerhandsLeft");
            female_WristArcRaise_Left = Content.Load<Texture2D>("Sprites\\female_WristArcRaise_Left");
            female_WristArcRaise_Right = Content.Load<Texture2D>("Sprites\\female_WristArcRaise_Right");

            male_crossover = Content.Load<Texture2D>("Sprites\\male_crossover");
            male_KneelandClap = Content.Load<Texture2D>("Sprites\\male_KneelandClap");
            male_LeftHandToFace_Spin = Content.Load<Texture2D>("Sprites\\male_LeftHandToFace_Spin");
            male_LeftKneeBendCrouch_Left = Content.Load<Texture2D>("Sprites\\male_LeftKneeBendCrouch_Left");
            male_LeftKneeBendCrouch_Right = Content.Load<Texture2D>("Sprites\\male_LeftKneeBendCrouch_Right");
            male_LeftKneeCross = Content.Load<Texture2D>("Sprites\\male_LeftKneeCross");
            male_LeftKneeKick = Content.Load<Texture2D>("Sprites\\male_LeftKneeKick");
            male_LeftKneeLift = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift");
            male_LeftKneeLift_FaceBack = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_FaceBack");
            male_LeftKneeLift_FaceLeft = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_FaceLeft");
            male_LeftKneeLift_FaceRight = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_FaceRight");
            male_LeftKneeLift_FrontTorso = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_FrontTorso");
            male_LeftKneeLift_FrontTorso_LeftHand = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_FrontTorso_LeftHand");
            male_LeftKneeLift_LeftHand = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_LeftHand");
            male_LeftKneeLift_UnderArm = Content.Load<Texture2D>("Sprites\\male_LeftKneeLift_UnderArm");
            male_RightKneeCross = Content.Load<Texture2D>("Sprites\\male_RightKneeCross");
            male_rightKneeKick = Content.Load<Texture2D>("Sprites\\male_rightKneeKick");
            male_RightKneeKick_UnderArm = Content.Load<Texture2D>("Sprites\\male_RightKneeKick_UnderArm");
            male_RightKneeKneel_UnderArm = Content.Load<Texture2D>("Sprites\\male_RightKneeKneel_UnderArm");
            male_RightKneeKneel_UnderArm_HandBehind = Content.Load<Texture2D>("Sprites\\male_RightKneeKneel_UnderArm_HandBehind");
            male_rightKneeLift = Content.Load<Texture2D>("Sprites\\male_rightKneeLift");
            male_RightKneeLift_FaceRight = Content.Load<Texture2D>("Sprites\\male_RightKneeLift_FaceRight");
            male_RightKneeLift_LeftHand = Content.Load<Texture2D>("Sprites\\male_RightKneeLift_LeftHand");
            male_shrug = Content.Load<Texture2D>("Sprites\\male_shrug");
            male_WaiterHand = Content.Load<Texture2D>("Sprites\\male_WaiterHand");
            */

            n_MoveTarget = Content.Load<Texture2D>("Sprites\\n_MoveTarget");
            n_P1icon = Content.Load<Texture2D>("Textures\\lefthandraise");
            n_P2icon = Content.Load<Texture2D>("Textures\\righthandraise");

            song = Content.Load<Song>("Music\\dabke");
            //MediaPlayer.Play(song);

            dance1.LoadContent(Content);
            dance2.LoadContent(Content);

            video = Content.Load<Video>("Video\\tempIntro");
            video1 = Content.Load<Video>("Video\\Lebanon");
            videoPlayer = new VideoPlayer();
            videoPlayer.Play(video1);
            introVideoTime.Start();
            videoPlayer.IsLooped = true;

            selectedDance = dance1; //pick the dance
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
                GLOBALS.writer.Close();
                #endif
            }

            // Press spacebar to advance your current dance steps without actually dancing.
            // Helpful for testing, making videos, ect...
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (!bSpaceKeyPressed)
                {
                    if(gameState == (int)GameState.ATTRACT)
                    {
                        // SPACEBAR stops the intro video, if it is playing...
                        //malePlaying = true; //generic man playing
                        videoPlayer.Dispose();
                        marker = gameTime.TotalGameTime;
                        GLOBALS.PLAYER_ONE_ACTIVE = true;
                        GLOBALS.PLAYER_TWO_ACTIVE = true;
                        gameState = (int)GameState.HOWTOPLAY;                 

                    }
                    else
                    {
                        // Otherwise, SPACEBAR is our developer shortcut for getting the current time
                        int currentTime = (int)videoTime.ElapsedMilliseconds;
                        Debug.WriteLine("\nCurrent time in seconds:" + currentTime + "\n");
                        bSpaceKeyPressed = true;
                    }
                }
            }
            else
                bSpaceKeyPressed = false;

            if (gameState == (int)GameState.HOWTOPLAY)
            {             
                if (gameTime.TotalGameTime.CompareTo(marker.Add(showInstructions)) > 0 || Keyboard.GetState().IsKeyDown(Keys.A)) //when to start the video
                {                     
                    videoPlayer2 = new VideoPlayer();
                    videoPlayer2.Play(selectedDance.GetMovie());
                    gameState = (int)GameState.DANCE;
                    setTime();
                    videoTime.Start();
                }
            }

            if (gameState == (int)GameState.DANCE)
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

                //crappy testing thing
                int newPts = 0;
                newPts = selectedDance.ScoreMovesFake(videoTime.Elapsed);
                if (newPts > 0)
                {
                    tempStepsDone++;
                    tempScore += newPts;
                }

                //score blocks (requires no kinect)
                if (selectedDance.ScoreBlockFake(videoTime.Elapsed))
                {
                    scorePlayer();
                    scorePlayerF();
                }

                /*Check each step to see if it is hit*/
                if (false)
                {
                    #region old step checking code
                    if (LeftKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem scores
                    if (FemLeftKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLift1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeLift1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeLift2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLift2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeLift2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeLift2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (introScore.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        introScore = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    /*Finish checking steps for the intro*/

                    //Score for block 2
                    if (LeftKneeLift3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndFrontTorso1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorso1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndBackTorso1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndBackTorso1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndFrontTorso2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorso2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndBackTorso2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndBackTorso2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem times
                    if (FemLeftKneeLift3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLift3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeLift3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeLift3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftFootLiftAndFrontTorso1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftFootLiftAndFrontTorso1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootLiftAndBackTorso1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootLiftAndBackTorso1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeLift4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLift4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeLift4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeLift4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftFootLiftAndFrontTorso2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftFootLiftAndFrontTorso2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootLiftAndBackTorso2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootLiftAndBackTorso2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score2.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score2 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //Done with score block 2

                    //Start score block 3
                    if (LeftKneeLiftAndLeftHand1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndLeftHand1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndLeftHand1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndLeftHand1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndFrontTorsoAndLeftHand1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorsoAndLeftHand1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndLeftHand1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndLeftHand1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndBackTorsoAndLeftHand1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndBackTorsoAndLeftHand1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndLeftHand2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndLeftHand2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndFrontTorsoAndLeftHand2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorsoAndLeftHand2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndBackTorsoAndLeftHand2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndBackTorsoAndLeftHand2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem scores block 3
                    if (FemHandSwingFront1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingFront1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHandSwingRight1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingRight1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHipShakeBack1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHipShakeBack1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHandSwingBack1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingBack1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHandSwingLeft1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingLeft1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHipShakeFront1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHipShakeFront1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score3.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score3 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //Done with score block 3

                    //Start score block 4
                    if (KneelDownsAndClap.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        KneelDownsAndClap = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    //Fem scores block 4
                    if (FemHandSwingFront2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingFront2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHandSwingRight2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingRight2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHipShakeBack2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHipShakeBack2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHandSwingBack2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingBack2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHandSwingLeft2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHandSwingLeft2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHipShakeFront2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHipShakeFront2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (Score4.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score4 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //End score block 4

                    //Start score block 5
                    if (LeftKneeLift5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift6 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift6 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift7 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift7 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift8 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift8 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem score block 5
                    if (FemMoveToRightAndScrollingHands1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToRightAndScrollingHands1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemMoveToRightAndScrollingHands2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToRightAndScrollingHands2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemCrouchAndHipShake1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemCrouchAndHipShake1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemMoveToLeftAndScrollingHands1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToLeftAndScrollingHands1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemMoveToLeftAndScrollingHands2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToLeftAndScrollingHands2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemCrouchAndHipShake2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemCrouchAndHipShake2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (Score5.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score5 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //End score block 5

                    //Start score block 6
                    if (MoveToRightAndWaiterHand.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        MoveToRightAndWaiterHand = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (ShrugShoulders.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        ShrugShoulders = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift9 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift9 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift10 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift10 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch0B.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch0B = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch0A.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch0A = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    //Fem score block 6
                    if (FemMoveToRightAndScrollingHands3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToRightAndScrollingHands3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemMoveToRightAndScrollingHands4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToRightAndScrollingHands4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemCrouchAndHipShake3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemCrouchAndHipShake3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemMoveToLeftAndScrollingHands3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToLeftAndScrollingHands3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemMoveToLeftAndScrollingHands4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemMoveToLeftAndScrollingHands4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemCrouchAndHipShake4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemCrouchAndHipShake4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch0B.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch0B = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch0A.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch0A = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (Score6.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score6 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 6

                    //Start score block 7
                    if (RightFootCross1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross6 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing6 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch6 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross7 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing7 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross8 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing8 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem scores block 7
                    if (FemRightFootCross1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross6 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing6 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch6 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross7 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing7 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross8 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing8 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score7.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score7 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //End score block 7

                    //Score block 8
                    if (LeftKneeLiftAndCross.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndCross = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLiftAndCross.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLiftAndCross = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift12 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift11 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift13 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift12 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftHandtoFaceSpinForward.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftHandtoFaceSpinForward = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftHandtoFaceSpinBack.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftHandtoFaceSpinBack = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem moves block 8
                    if (FemRightElbowSway1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightElbowSway1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftElbowSway1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftElbowSway1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightElbowSway2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightElbowSway2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftElbowSway2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftElbowSway2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightElbowSway3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightElbowSway3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftElbowSway3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftElbowSway3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightElbowSway4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightElbowSway4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftElbowSway4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftElbowSway4 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score8.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score8 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 8

                    //Begin score block 9
                    if (LeftKneeLift14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift14 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift15.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift15 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift16.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift16 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftLeft.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftLeft = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftBack.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftBack = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftRight.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftRight = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem moves block 9
                    if (FemLeftWristArcRaise1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftWristArcRaise1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightWristArcRaise1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightWristArcRaise1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHome1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHome1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftWristArcRaise2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftWristArcRaise2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightWristArcRaise2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightWristArcRaise2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemHome2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemHome2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score9.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score9 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 9

                    //Start score block 10
                    if (LeftKneeLift17.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift17 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeKick17.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeKick17 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift13 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift18.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift18 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeKick18.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeKick18 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift14 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift19.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift19 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }


                    if (LeftKneeLift20.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift20 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeKick20.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeKick20 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift15.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift15 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift21.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift21 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeKick21.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeKick21 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeLift16.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeLift16 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift22.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift22 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick6 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem score block 10
                    if (FemThrillerHandsLeft1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemThrillerHandsLeft1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftBendHipShake1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftBendHipShake1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemThrillerHandsLeft2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemThrillerHandsLeft2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftBendHipShake2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftBendHipShake2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score10.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score10 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 10

                    //Start score block 11
                    if (LeftKneeLiftAndFrontTorso3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorso3 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick7 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift23.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift23 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick8 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLift24.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLift24 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick9 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndFrontTorso4.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorso4 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick10 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeKick.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeKick = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKick.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKick = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem score block 11
                    if (FemRightElbowSway5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightElbowSway5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftElbowSway5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftElbowSway5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightElbowSway6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightElbowSway6 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftElbowSway6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftElbowSway6 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeLift5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLift5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeKick2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeKick2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeLiftAndFrontTorso5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLiftAndFrontTorso5 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeKick.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeKick = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeLift6.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLift6 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightKneeKick3.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightKneeKick3 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score11.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score11 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 11

                    //Start score block 12
                    if (LeftKneeBendCrouch7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch7 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch8 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross9 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing9 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross10 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing10 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch9 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch10 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross11 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing11 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross12 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing12 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch11 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch12 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross13 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing13 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross14 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing14 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch13 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeBendCrouch14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeBendCrouch14 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross15.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross15 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing15.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing15 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootCross16.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootCross16 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightFootSwing16.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightFootSwing16 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem score block 12
                    if (FemLeftKneeBendCrouch7.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch7 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch8.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch8 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross9 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing9 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross10 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing10 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch9.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch9 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch10.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch10 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross11 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing11 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross12 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing12 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch11.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch11 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch12.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch12 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross13 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing13 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross14 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing14 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch13.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch13 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemLeftKneeBendCrouch14.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeBendCrouch14 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross15.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross15 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing15.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing15 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootCross16.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootCross16 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightFootSwing16.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightFootSwing16 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score12.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score12 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 12

                    //start score block 13
                    if (LeftKneeLiftAndFrontTorso5.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndFrontTorso5 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKickAndUnderArm1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKickAndUnderArm1 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (LeftKneeLiftAndUnderArm.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        LeftKneeLiftAndUnderArm = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKickAndUnderArm2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKickAndUnderArm2 = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKneelAndUnderArm.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKneelAndUnderArm = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }
                    if (RightKneeKneelAndUnderArmAndLeftHandBehind.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        RightKneeKneelAndUnderArmAndLeftHandBehind = stepFinished;
                        stepsDone++;
                        tempStepsDone++;
                    }

                    //Fem score block 13
                    if (FemLeftKneeLiftAndFrontTorso2.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemLeftKneeLiftAndFrontTorso2 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemBackSpinRightKneeLift1.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemBackSpinRightKneeLift1 = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemForwardSpinFacingRightKneeLift.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemForwardSpinFacingRightKneeLift = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemCrouchHipSwivel.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemCrouchHipSwivel = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }
                    if (FemRightHandHigh.Subtract(videoTime.Elapsed).Milliseconds + GLOBALS.SCORING_WINDOW < 0)
                    {
                        FemRightHandHigh = stepFinished;
                        femStepsDone++;
                        tempFemStepsDone++;
                    }

                    if (Score13.Subtract(videoTime.Elapsed).Milliseconds < 0)
                    {
                        Score13 = stepFinished;
                        //scorePlayer();
                        scorePlayerF();
                    }
                    //end score block 13
                    #endregion
                }
                //We are at the end of the dance video, time to display scores
                if (gameEnd.Subtract(videoTime.Elapsed).Milliseconds < 0)
                {
                    gameState = (int)GameState.SCORE;
                    GLOBALS.PLAYER_ONE_ACTIVE = false;
                    GLOBALS.PLAYER_TWO_ACTIVE = false;
                }
            }
           
            //Loop back to the start
            if (gameState == (int)GameState.SCORE && restartGameLoop.Subtract(videoTime.Elapsed).Milliseconds < 0)
            {
                videoPlayer2.Dispose();
                videoPlayer = new VideoPlayer();
                videoPlayer.IsLooped = true;
                videoPlayer.Play(video1);
                gameState = (int)GameState.ATTRACT;
                videoTime.Reset();
                introVideoTime.Reset();
                introVideoTime.Start();
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

            if (totalScoreF > displayScoreF)
                displayScoreF += 11;

            if (totalScoreF < displayScoreF)
                displayScoreF = totalScoreF;

            base.Update(gameTime);  // Base XNA update...
        }

        private void scorePlayer()
        {
            //if (!malePlaying) return;
            setScore = tempScore;
            tempScore = 0;
            if (setScore > (tempStepsDone * 200))
            {
                resultColor = Color.Cyan;
                resultString = "EXCELLENT!";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScore > (tempStepsDone * 100))
            {
                resultColor = Color.Green;
                resultString = "GOOD";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else// if (setScore > (stepsDone*100))
            {
                resultColor = Color.Yellow;
                resultString = "Okay";
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
            tempStepsDone = 0;
        }

        private void scorePlayerF()
        {
            if (!GLOBALS.PLAYER_TWO_ACTIVE) return;
            setScoreF = tempScoreF;
            tempScoreF = 0;
            if (setScoreF > (tempFemStepsDone * 200))
            {
                resultColorF = Color.Cyan;
                resultStringF = "EXCELLENT!";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else if (setScoreF > (tempFemStepsDone * 100))
            {
                resultColorF = Color.Green;
                resultStringF = "GOOD";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }
            else// if (setScore > (stepsDone*100))
            {
                resultColorF = Color.Yellow;
                resultStringF = "Okay";
                textFadeOut = new TimeSpan(0, 0, 2); // 2-second fadeout for result text
            }

            if (setScoreF > 400)
            {
                displayRecentScoreTextF = "+" + setScoreF.ToString();
                totalScoreF += setScoreF;
            }
            else
            {
                displayRecentScoreTextF = " ";
                setScoreF = 0;
            }
            tempFemStepsDone = 0;
        }

        private void scoreDurationMove(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            int modifiedScore;
            if (earnedScore > 400)
            {
                modifiedScore= 400;
            }
            else
            {
                modifiedScore= Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            modifiedScore = Math.Max(modifiedScore, 100);
            tempScore += modifiedScore;
            stepsDone++;
            tempStepsDone++;
        }
        private void scoreDurationMoveF(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            int modifiedScore;
            if (earnedScore > 400)
            {
                modifiedScore = 400;
            }
            else
            {
                modifiedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            modifiedScore = Math.Max(modifiedScore, 100);
            tempScoreF += modifiedScore;
            femStepsDone++;
            tempFemStepsDone++;
        }

        private void scoreMove(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            if (earnedScore > 400)
            {
                tempScore += 400;
            }
            else
            {
                tempScore += Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            stepsDone++;
            tempStepsDone++;
        }
        private void scoreMoveF(double diff)
        {
            int earnedScore = Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            if (earnedScore > 400)
            {
                tempScoreF += 400;
            }
            else
            {
                tempScoreF += Math.Max((int)(GLOBALS.SCORING_WINDOW - diff), 0);
            }
            femStepsDone++;
            tempFemStepsDone++;
        }

        //Left knee lift move, total of 24 for the male
        private void LeftKneeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLift1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeLift2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(LeftKneeLift3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(LeftKneeLift4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(LeftKneeLift5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(LeftKneeLift6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(LeftKneeLift7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(LeftKneeLift8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(LeftKneeLift9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(LeftKneeLift10).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(LeftKneeLift12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(LeftKneeLift13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(LeftKneeLift14).TotalMilliseconds));
            double diff15 = Math.Abs((currentTime.Subtract(LeftKneeLift15).TotalMilliseconds));
            double diff16 = Math.Abs((currentTime.Subtract(LeftKneeLift16).TotalMilliseconds));
            double diff17 = Math.Abs((currentTime.Subtract(LeftKneeLift17).TotalMilliseconds));
            double diff18 = Math.Abs((currentTime.Subtract(LeftKneeLift18).TotalMilliseconds));
            double diff19 = Math.Abs((currentTime.Subtract(LeftKneeLift19).TotalMilliseconds));
            double diff20 = Math.Abs((currentTime.Subtract(LeftKneeLift20).TotalMilliseconds));
            double diff21 = Math.Abs((currentTime.Subtract(LeftKneeLift21).TotalMilliseconds));
            double diff22 = Math.Abs((currentTime.Subtract(LeftKneeLift22).TotalMilliseconds));
            double diff23 = Math.Abs((currentTime.Subtract(LeftKneeLift23).TotalMilliseconds));
            double diff24 = Math.Abs((currentTime.Subtract(LeftKneeLift24).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLift1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                LeftKneeLift2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                LeftKneeLift3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                LeftKneeLift4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                LeftKneeLift5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff6);
                LeftKneeLift6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff7);
                LeftKneeLift7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff8);
                LeftKneeLift8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff9);
                LeftKneeLift9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                LeftKneeLift10 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                LeftKneeLift10 = stepFinished;
            }

            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff12);
                LeftKneeLift12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff13);
                LeftKneeLift13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff14);
                LeftKneeLift14 = stepFinished;
            }
            else if (diff15 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff15);
                LeftKneeLift15 = stepFinished;
            }
            else if (diff16 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff16);
                LeftKneeLift16 = stepFinished;
            }
            else if (diff17 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff17);
                LeftKneeLift17 = stepFinished;
            }
            else if (diff18 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff18);
                LeftKneeLift18 = stepFinished;
            }
            else if (diff19 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff19);
                LeftKneeLift19 = stepFinished;
            }
            else if (diff20 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff20);
                LeftKneeLift20 = stepFinished;
            }
            else if (diff21 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff21);
                LeftKneeLift21 = stepFinished;
            }
            else if (diff22 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff22);
                LeftKneeLift22 = stepFinished;
            }
            else if (diff23 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff23);
                LeftKneeLift23 = stepFinished;
            }
            else if (diff24 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff24);
                LeftKneeLift24 = stepFinished;
            }

        }

        //Right knee lift, total of 16 for the male
        private void RightKneeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLift1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightKneeLift2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(RightKneeLift3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(RightKneeLift4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(RightKneeLift5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(RightKneeLift6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(RightKneeLift7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(RightKneeLift8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(RightKneeLift9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(RightKneeLift10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(RightKneeLift11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(RightKneeLift12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(RightKneeLift13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(RightKneeLift14).TotalMilliseconds));
            double diff15 = Math.Abs((currentTime.Subtract(RightKneeLift15).TotalMilliseconds));
            double diff16 = Math.Abs((currentTime.Subtract(RightKneeLift16).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeLift1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightKneeLift2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                RightKneeLift3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                RightKneeLift4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                RightKneeLift5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff6);
                RightKneeLift6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff7);
                RightKneeLift7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff8);
                RightKneeLift8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff9);
                RightKneeLift9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                RightKneeLift10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff11);
                RightKneeLift11 = stepFinished;
            }

            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff12);
                RightKneeLift12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff13);
                RightKneeLift13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff14);
                RightKneeLift14 = stepFinished;
            }
            else if (diff15 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff15);
                RightKneeLift15 = stepFinished;
            }
            else if (diff16 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff16);
                RightKneeLift16 = stepFinished;
            }
        }

        private void LeftKneeLiftAndFrontTorsoTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorso1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorso2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorso3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorso4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorso5).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftAndFrontTorso1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                LeftKneeLiftAndFrontTorso2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                LeftKneeLiftAndFrontTorso3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                LeftKneeLiftAndFrontTorso4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                LeftKneeLiftAndFrontTorso5 = stepFinished;
            }
        }

        private void RightKneeLiftAndBackTorsoTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLiftAndBackTorso1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightKneeLiftAndBackTorso2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeLiftAndBackTorso1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightKneeLiftAndBackTorso2 = stepFinished;
            }
        }

        private void LeftKneeLiftAndLeftHandTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndLeftHand1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndLeftHand2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftAndLeftHand1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                LeftKneeLiftAndLeftHand2 = stepFinished;
            }
        }

        private void RightKneeLiftAndLeftHandTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLiftAndLeftHand1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightKneeLiftAndLeftHand2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeLiftAndLeftHand1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightKneeLiftAndLeftHand2 = stepFinished;
            }
        }

        public void LeftKneeLiftAndFrontTorsoAndLeftHandTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorsoAndLeftHand1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndFrontTorsoAndLeftHand2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftAndFrontTorsoAndLeftHand1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                LeftKneeLiftAndFrontTorsoAndLeftHand2 = stepFinished;
            }
        }

        public void RightKneeLiftAndBackTorsoAndLeftHandTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLiftAndBackTorsoAndLeftHand1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightKneeLiftAndBackTorsoAndLeftHand2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeLiftAndBackTorsoAndLeftHand1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightKneeLiftAndBackTorsoAndLeftHand2 = stepFinished;
            }
        }

        public void KneelDownsAndClapTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(KneelDownsAndClap).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMove(diff1);
                KneelDownsAndClap = stepFinished;
            }
        }

        public void MoveToRightAndWaiterHandTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(MoveToRightAndWaiterHand).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMove(diff1);
                MoveToRightAndWaiterHand = stepFinished;
            }
        }

        public void ShrugShouldersTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(ShrugShoulders).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMove(diff1);
                ShrugShoulders = stepFinished;
            }
        }

        private void RightFootCrossTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightFootCross1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightFootCross2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(RightFootCross3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(RightFootCross4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(RightFootCross5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(RightFootCross6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(RightFootCross7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(RightFootCross8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(RightFootCross9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(RightFootCross10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(RightFootCross11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(RightFootCross12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(RightFootCross13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(RightFootCross14).TotalMilliseconds));
            double diff15 = Math.Abs((currentTime.Subtract(RightFootCross15).TotalMilliseconds));
            double diff16 = Math.Abs((currentTime.Subtract(RightFootCross16).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightFootCross1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightFootCross2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                RightFootCross3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                RightFootCross4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                RightFootCross5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff6);
                RightFootCross6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff7);
                RightFootCross7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff8);
                RightFootCross8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff9);
                RightFootCross9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                RightFootCross10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff11);
                RightFootCross11 = stepFinished;
            }

            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff12);
                RightFootCross12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff13);
                RightFootCross13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff14);
                RightFootCross14 = stepFinished;
            }
            else if (diff15 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff15);
                RightFootCross15 = stepFinished;
            }
            else if (diff16 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff16);
                RightFootCross16 = stepFinished;
            }
        }

        private void RightFootSwingTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightFootSwing1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(RightFootSwing2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(RightFootSwing3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(RightFootSwing4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(RightFootSwing5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(RightFootSwing6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(RightFootSwing7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(RightFootSwing8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(RightFootSwing9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(RightFootSwing10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(RightFootSwing11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(RightFootSwing12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(RightFootSwing13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(RightFootSwing14).TotalMilliseconds));
            double diff15 = Math.Abs((currentTime.Subtract(RightFootSwing15).TotalMilliseconds));
            double diff16 = Math.Abs((currentTime.Subtract(RightFootSwing16).TotalMilliseconds));


            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightFootSwing1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightFootSwing2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                RightFootSwing3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                RightFootSwing4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                RightFootSwing5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff6);
                RightFootSwing6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff7);
                RightFootSwing7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff8);
                RightFootSwing8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff9);
                RightFootSwing9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                RightFootSwing10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff11);
                RightFootSwing11 = stepFinished;
            }

            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff12);
                RightFootSwing12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff13);
                RightFootSwing13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff14);
                RightFootSwing14 = stepFinished;
            }
            else if (diff15 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff15);
                RightFootSwing15 = stepFinished;
            }
            else if (diff16 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff16);
                RightFootSwing16 = stepFinished;
            }
        }

        private void LeftKneeBendCrouchTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff0A = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch0A).TotalMilliseconds));  
            double diff0B = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch0B).TotalMilliseconds));   
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(LeftKneeBendCrouch14).TotalMilliseconds));

            if (diff0A < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff0A);
                LeftKneeBendCrouch1 = stepFinished;
            }
            if (diff0B < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff0B);
                LeftKneeBendCrouch1 = stepFinished;
            }
            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeBendCrouch1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                LeftKneeBendCrouch2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                LeftKneeBendCrouch3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                LeftKneeBendCrouch4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                LeftKneeBendCrouch5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff6);
                LeftKneeBendCrouch6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff7);
                LeftKneeBendCrouch7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff8);
                LeftKneeBendCrouch8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff9);
                LeftKneeBendCrouch9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                LeftKneeBendCrouch10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff11);
                LeftKneeBendCrouch11 = stepFinished;
            }

            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff12);
                LeftKneeBendCrouch12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff13);
                LeftKneeBendCrouch13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff14);
                LeftKneeBendCrouch14 = stepFinished;
            }
        }

        private void RightKneeKickTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeKick).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(RightKneeKick2).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(RightKneeKick3).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(RightKneeKick4).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(RightKneeKick5).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(RightKneeKick6).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(RightKneeKick7).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(RightKneeKick8).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(RightKneeKick9).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(RightKneeKick10).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeKick = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff3);
                RightKneeKick2 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff4);
                RightKneeKick3 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff5);
                RightKneeKick4 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff6);
                RightKneeKick5 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff7);
                RightKneeKick6 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff8);
                RightKneeKick7 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff9);
                RightKneeKick8 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff10);
                RightKneeKick9 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff11);
                RightKneeKick10 = stepFinished;
            }

        }

        private void LeftKneeKickTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeKick).TotalMilliseconds));
            double diff17 = Math.Abs((currentTime.Subtract(LeftKneeKick17).TotalMilliseconds));
            double diff18 = Math.Abs((currentTime.Subtract(LeftKneeKick18).TotalMilliseconds));
            double diff20 = Math.Abs((currentTime.Subtract(LeftKneeKick20).TotalMilliseconds));
            double diff21 = Math.Abs((currentTime.Subtract(LeftKneeKick21).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeKick = stepFinished;
            }
            else if (diff17 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff17);
                LeftKneeKick17 = stepFinished;
            }
            else if (diff18 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff18);
                LeftKneeKick18 = stepFinished;
            }
            else if (diff20 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff20);
                LeftKneeKick20 = stepFinished;
            }
            else if (diff21 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff21);
                LeftKneeKick21 = stepFinished;
            }
        }

        private void LeftHandtoFaceSpinBackTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftHandtoFaceSpinBack).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftHandtoFaceSpinBack = stepFinished;
            }
        }

        private void LeftKneeLiftAndCrossTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndCross).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftAndCross = stepFinished;
            }
        }

        private void LeftHandtoFaceSpinForwardTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftHandtoFaceSpinForward).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftHandtoFaceSpinForward = stepFinished;
            }
        }

        private void RightKneeLiftAndCrossTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeLiftAndCross).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeLiftAndCross = stepFinished;
            }
        }

        private void LeftKneeLiftLeftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftLeft).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftLeft = stepFinished;
            }
        }

        private void LeftKneeLiftBackTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftBack).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftBack = stepFinished;
            }
        }

        private void LeftKneeLiftRightTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftRight).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftRight = stepFinished;
            }
        }

        private void RightKneeKneelAndUnderArmTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeKneelAndUnderArm).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeKneelAndUnderArm = stepFinished;
            }
        }

        private void LeftKneeLiftAndUnderArmTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(LeftKneeLiftAndUnderArm).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                LeftKneeLiftAndUnderArm = stepFinished;
            }
        }

        private void RightKneeKneelAndUnderArmAndLeftHandBehindTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(RightKneeKneelAndUnderArmAndLeftHandBehind).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
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

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff1);
                RightKneeKickAndUnderArm1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMove(diff2);
                RightKneeKickAndUnderArm2 = stepFinished;
            }
        }

        //FEMALE TRIGGERS

        private void FemLeftKneeLiftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftKneeLift1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemLeftKneeLift2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemLeftKneeLift3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemLeftKneeLift4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(FemLeftKneeLift5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(FemLeftKneeLift6).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftKneeLift1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemLeftKneeLift2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemLeftKneeLift3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemLeftKneeLift4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff5);
                FemLeftKneeLift5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff6);
                FemLeftKneeLift6 = stepFinished;
            }
        }

        private void FemRightKneeLiftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightKneeLift1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemRightKneeLift2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemRightKneeLift3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemRightKneeLift4).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightKneeLift1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemRightKneeLift2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemRightKneeLift3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemRightKneeLift4 = stepFinished;
            }
        }

        private void FemCrouchAndHipShakeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemCrouchAndHipShake1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemCrouchAndHipShake2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemCrouchAndHipShake3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemCrouchAndHipShake4).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff1);
                FemCrouchAndHipShake1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff2);
                FemCrouchAndHipShake2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff3);
                FemCrouchAndHipShake3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff4);
                FemCrouchAndHipShake4 = stepFinished;
            }
        }

        private void FemMoveToRightAndScrollingHandsTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemMoveToRightAndScrollingHands1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemMoveToRightAndScrollingHands2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemMoveToRightAndScrollingHands3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemMoveToRightAndScrollingHands4).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemMoveToRightAndScrollingHands1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemMoveToRightAndScrollingHands2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemMoveToRightAndScrollingHands3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemMoveToRightAndScrollingHands4 = stepFinished;
            }
        }

        private void FemMoveToLeftAndScrollingHandsTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemMoveToLeftAndScrollingHands1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemMoveToLeftAndScrollingHands2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemMoveToLeftAndScrollingHands3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemMoveToLeftAndScrollingHands4).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemMoveToLeftAndScrollingHands1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemMoveToLeftAndScrollingHands2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemMoveToLeftAndScrollingHands3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemMoveToLeftAndScrollingHands4 = stepFinished;
            }
        }

        private void FemLeftFootLiftAndFrontTorsoTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftFootLiftAndFrontTorso1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemLeftFootLiftAndFrontTorso2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftFootLiftAndFrontTorso1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemLeftFootLiftAndFrontTorso2 = stepFinished;
            }
        }

        private void FemRightFootLiftAndBackTorsoTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightFootLiftAndBackTorso1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemRightFootLiftAndBackTorso2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightFootLiftAndBackTorso1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemRightFootLiftAndBackTorso2 = stepFinished;
            }
        }

        private void FemHandSwingFrontTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHandSwingFront1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHandSwingFront2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemHandSwingFront1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemHandSwingFront2 = stepFinished;
            }
        }

        private void FemHandSwingRightTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHandSwingRight1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHandSwingRight2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemHandSwingRight1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemHandSwingRight2 = stepFinished;
            }
        }

        private void FemHipShakeBackTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHipShakeBack1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHipShakeBack2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff1);
                FemHipShakeBack1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff2);
                FemHipShakeBack2 = stepFinished;
            }
        }

        private void FemHandSwingBackTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHandSwingBack1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHandSwingBack2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemHandSwingBack1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemHandSwingBack2 = stepFinished;
            }
        }

        private void FemHandSwingLeftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHandSwingLeft1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHandSwingLeft2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemHandSwingLeft1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemHandSwingLeft2 = stepFinished;
            }
        }

        private void FemHipShakeFrontTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHipShakeFront1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHipShakeFront2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff1);
                FemHipShakeFront1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff2);
                FemHipShakeFront2 = stepFinished;
            }
        }

        //adding these for 2/2
        private void FemRightFootCrossTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightFootCross1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemRightFootCross2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemRightFootCross3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemRightFootCross4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(FemRightFootCross5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(FemRightFootCross6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(FemRightFootCross7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(FemRightFootCross8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(FemRightFootCross9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(FemRightFootCross10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(FemRightFootCross11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(FemRightFootCross12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(FemRightFootCross13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(FemRightFootCross14).TotalMilliseconds));
            double diff15 = Math.Abs((currentTime.Subtract(FemRightFootCross15).TotalMilliseconds));
            double diff16 = Math.Abs((currentTime.Subtract(FemRightFootCross16).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightFootCross1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemRightFootCross2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemRightFootCross3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemRightFootCross4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff5);
                FemRightFootCross5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff6);
                FemRightFootCross6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff7);
                FemRightFootCross7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff8);
                FemRightFootCross8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff9);
                FemRightFootCross9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff10);
                FemRightFootCross10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff11);
                FemRightFootCross11 = stepFinished;
            }
            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff12);
                FemRightFootCross12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff13);
                FemRightFootCross13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff14);
                FemRightFootCross14 = stepFinished;
            }
            else if (diff15 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff15);
                FemRightFootCross15 = stepFinished;
            }
            else if (diff16 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff16);
                FemRightFootCross16 = stepFinished;
            }
        }

        private void FemRightFootSwingTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightFootSwing1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemRightFootSwing2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemRightFootSwing3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemRightFootSwing4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(FemRightFootSwing5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(FemRightFootSwing6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(FemRightFootSwing7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(FemRightFootSwing8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(FemRightFootSwing9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(FemRightFootSwing10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(FemRightFootSwing11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(FemRightFootSwing12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(FemRightFootSwing13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(FemRightFootSwing14).TotalMilliseconds));
            double diff15 = Math.Abs((currentTime.Subtract(FemRightFootSwing15).TotalMilliseconds));
            double diff16 = Math.Abs((currentTime.Subtract(FemRightFootSwing16).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightFootSwing1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemRightFootSwing2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemRightFootSwing3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemRightFootSwing4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff5);
                FemRightFootSwing5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff6);
                FemRightFootSwing6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff7);
                FemRightFootSwing7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff8);
                FemRightFootSwing8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff9);
                FemRightFootSwing9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff10);
                FemRightFootSwing10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff11);
                FemRightFootSwing11 = stepFinished;
            }
            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff12);
                FemRightFootSwing12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff13);
                FemRightFootSwing13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff14);
                FemRightFootSwing14 = stepFinished;
            }
            else if (diff15 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff15);
                FemRightFootSwing15 = stepFinished;
            }
            else if (diff16 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff16);
                FemRightFootSwing16 = stepFinished;
            }
        }

        private void FemLeftKneeBendCrouchTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff0A = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch0A).TotalMilliseconds));
            double diff0B = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch0B).TotalMilliseconds));
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch6).TotalMilliseconds));
            double diff7 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch7).TotalMilliseconds));
            double diff8 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch8).TotalMilliseconds));
            double diff9 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch9).TotalMilliseconds));
            double diff10 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch10).TotalMilliseconds));
            double diff11 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch11).TotalMilliseconds));
            double diff12 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch12).TotalMilliseconds));
            double diff13 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch13).TotalMilliseconds));
            double diff14 = Math.Abs((currentTime.Subtract(FemLeftKneeBendCrouch14).TotalMilliseconds));

            if(diff0A < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff0A);
                FemLeftKneeBendCrouch0A = stepFinished;
            }
            else if (diff0B < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff0B);
                FemLeftKneeBendCrouch0B = stepFinished;
            }
            else if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftKneeBendCrouch1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemLeftKneeBendCrouch2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemLeftKneeBendCrouch3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemLeftKneeBendCrouch4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff5);
                FemLeftKneeBendCrouch5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff6);
                FemLeftKneeBendCrouch6 = stepFinished;
            }
            else if (diff7 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff7);
                FemLeftKneeBendCrouch7 = stepFinished;
            }
            else if (diff8 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff8);
                FemLeftKneeBendCrouch8 = stepFinished;
            }
            else if (diff9 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff9);
                FemLeftKneeBendCrouch9 = stepFinished;
            }
            else if (diff10 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff10);
                FemLeftKneeBendCrouch10 = stepFinished;
            }
            else if (diff11 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff11);
                FemLeftKneeBendCrouch11 = stepFinished;
            }
            else if (diff12 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff12);
                FemLeftKneeBendCrouch12 = stepFinished;
            }
            else if (diff13 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff13);
                FemLeftKneeBendCrouch13 = stepFinished;
            }
            else if (diff14 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff14);
                FemLeftKneeBendCrouch14 = stepFinished;
            }
        }

        private void FemLeftWristArcRaiseTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftWristArcRaise1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemLeftWristArcRaise2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftWristArcRaise1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemLeftWristArcRaise2 = stepFinished;
            }
        }

        private void FemRightWristArcRaiseTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightWristArcRaise1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemRightWristArcRaise2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightWristArcRaise1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemRightWristArcRaise2 = stepFinished;
            }
        }

        private void FemHomeTriggered()//Hands at sides detection after fluttering hands down
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemHome1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemHome2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemHome1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemHome2 = stepFinished;
            }
        }

        private void FemThrillerHandsLeftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemThrillerHandsLeft1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemThrillerHandsLeft2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemThrillerHandsLeft1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemThrillerHandsLeft2 = stepFinished;
            }
        }

        private void FemLeftBendHipShakeTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftBendHipShake1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemLeftBendHipShake2).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftBendHipShake1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemLeftBendHipShake2 = stepFinished;
            }
        }

        private void FemRightHandHighTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightHandHigh).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightHandHigh = stepFinished;
            }
        }

        private void FemCrouchHipSwivelTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemCrouchHipSwivel).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreDurationMoveF(diff1);
                FemCrouchHipSwivel = stepFinished;
            }
        }

        private void FemForwardSpinFacingRightKneeLiftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemForwardSpinFacingRightKneeLift).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemForwardSpinFacingRightKneeLift = stepFinished;
            }
        }

        private void FemRightElbowSwayTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightElbowSway1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemRightElbowSway2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemRightElbowSway3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemRightElbowSway4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(FemRightElbowSway5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(FemRightElbowSway6).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightElbowSway1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemRightElbowSway2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemRightElbowSway3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemRightElbowSway4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff5);
                FemRightElbowSway5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff6);
                FemRightElbowSway6 = stepFinished;
            }

        }

        private void FemLeftElbowSwayTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftElbowSway1).TotalMilliseconds));
            double diff2 = Math.Abs((currentTime.Subtract(FemLeftElbowSway2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemLeftElbowSway3).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemLeftElbowSway4).TotalMilliseconds));
            double diff5 = Math.Abs((currentTime.Subtract(FemLeftElbowSway5).TotalMilliseconds));
            double diff6 = Math.Abs((currentTime.Subtract(FemLeftElbowSway6).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftElbowSway1 = stepFinished;
            }

            else if (diff2 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff2);
                FemLeftElbowSway2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemLeftElbowSway3 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemLeftElbowSway4 = stepFinished;
            }
            else if (diff5 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff5);
                FemLeftElbowSway5 = stepFinished;
            }
            else if (diff6 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff6);
                FemLeftElbowSway6 = stepFinished;
            }
        }

        private void FemRightKneeKickTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemRightKneeKick).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemRightKneeKick2).TotalMilliseconds));
            double diff4 = Math.Abs((currentTime.Subtract(FemRightKneeKick3).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemRightKneeKick = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemRightKneeKick2 = stepFinished;
            }
            else if (diff4 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff4);
                FemRightKneeKick3 = stepFinished;
            }
        }

        private void FemLeftKneeLiftAndFrontTorsoTriggerd()
        {//3, 5, 2
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemLeftKneeLiftAndFrontTorso2).TotalMilliseconds));
            double diff3 = Math.Abs((currentTime.Subtract(FemLeftKneeLiftAndFrontTorso5).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemLeftKneeLiftAndFrontTorso2 = stepFinished;
            }
            else if (diff3 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff3);
                FemLeftKneeLiftAndFrontTorso5 = stepFinished;
            }
        }

        private void FemBackSpinRightKneeLiftTriggered()
        {
            TimeSpan currentTime = videoTime.Elapsed;
            double diff1 = Math.Abs((currentTime.Subtract(FemBackSpinRightKneeLift1).TotalMilliseconds));

            if (diff1 < GLOBALS.SCORING_WINDOW)
            {
                scoreMoveF(diff1);
                FemBackSpinRightKneeLift1 = stepFinished;
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
            if (gameState == (int)GameState.ATTRACT)
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
                DrawText();
            }
            else if (gameState == (int)GameState.HOWTOPLAY)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(howtoplay, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                spriteBatch.End();
            }
            else if (gameState == (int)GameState.SCORE)
            {
                spriteBatch.Begin();
                if (scoreBackground != null)
                {
                    // Draw score screen over top
                    spriteBatch.Draw(scoreBackground, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                }
                //write the score to screen using DisplayScore
                displayScoreTextF = String.Format("{0,5}", displayScoreF);
                DrawDebugString(resultFont, Color.White, (int)(WINDOW_WIDTH - 225), 100, displayScoreTextF);

                displayScoreText = String.Format("{0,5}", displayScore);
                DrawDebugString(resultFont, Color.White, (int)(15), 100, displayScoreText);

                String rating, ratingF;
                Color ratingC, ratingFC;
                //Display rating of how they did
                if (displayScore > stepsDone * 180)
                {
                    rating = "DANCE STAR!";
                    ratingC = Color.Cyan;
                }
                else if (displayScore > stepsDone * 100)
                {
                    rating = "Professional";
                    ratingC = Color.Green;
                }
                else
                {
                    rating = "In Training";
                    ratingC = Color.Yellow;
                }
                //Fem
                if (displayScoreF > femStepsDone * 180)
                {
                    ratingF = "DANCE STAR!";
                    ratingFC = Color.Cyan;
                }
                else if (displayScoreF > femStepsDone * 100)
                {
                    ratingF = "Professional";
                    ratingFC = Color.Green;
                }
                else
                {
                    ratingF = "In Training";
                    ratingFC = Color.Yellow;
                }
                DrawDebugString(resultFont, ratingFC, (int)(WINDOW_WIDTH / 2 - 50), 400, ratingF);
                DrawDebugString(resultFont, ratingC, (int)(5), 300, rating);

                spriteBatch.End();
            }
            else if (gameState == (int)GameState.DANCE) // Intro video is no longer playing...
            {


#if USE_KINECT
                //If we don't have a depth target, exit
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
                    GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
#if USE_KINECT
                    depthTexture.SetData<short>(depthData);
                    SharedSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, null, null, null, kinectDepthVisualizer);
#endif
                    //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                    spriteBatch.Begin();
                    Texture2D texture = videoPlayer2.GetTexture();
                    if (texture != null)
                    {
                        // Draw intro video
                        //spriteBatch.Draw(texture, new Rectangle(0, 0, 720, 480), Color.White);
                        spriteBatch.Draw(texture, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    }

                    //draw sprites
                    TimeSpan currentTime = videoTime.Elapsed;
                    double diff = 0.0;
                    //malestart
                    int xlocation = 0;
                    
                    selectedDance.Draw(currentTime, spriteBatch); //I can draw moves from a move batch
                    if(GLOBALS.PLAYER_ONE_ACTIVE)
                        spriteBatch.Draw(n_MoveTarget, new Rectangle(0 + maleRectDiff, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                    if(GLOBALS.PLAYER_TWO_ACTIVE)
                        spriteBatch.Draw(n_MoveTarget, new Rectangle(500 - maleRectDiff, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);

                    if (false)
                    {
                        #region old dance drawing
                        if (GLOBALS.PLAYER_ONE_ACTIVE)
                        {
                            diff = (currentTime.Subtract(RightKneeLift1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeLift_FaceRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FaceRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeLift_FaceRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FaceRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock2

                            diff = (currentTime.Subtract(LeftKneeLift3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorso1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndBackTorso1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorso2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndBackTorso2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock3

                            diff = (currentTime.Subtract(LeftKneeLiftAndLeftHand1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndLeftHand1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeLift_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorsoAndLeftHand1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndBackTorsoAndLeftHand1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeLift_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndLeftHand2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndLeftHand2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeLift_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorsoAndLeftHand2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndBackTorsoAndLeftHand2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeLift_LeftHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock4

                            diff = (currentTime.Subtract(KneelDownsAndClap).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_KneelandClap, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock5

                            diff = (currentTime.Subtract(LeftKneeLift5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock6

                            diff = (currentTime.Subtract(MoveToRightAndWaiterHand).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_WaiterHand, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(ShrugShoulders).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_shrug, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch0B).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch0A).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock7

                            diff = (currentTime.Subtract(RightFootCross1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock8

                            diff = (currentTime.Subtract(LeftKneeLiftAndCross).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeCross, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLiftAndCross).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeCross, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift12).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift11).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift13).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift12).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftHandtoFaceSpinForward).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftHandToFace_Spin, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock9

                            diff = (currentTime.Subtract(LeftKneeLift14).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift15).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift16).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift16).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftLeft).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FaceLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftBack).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FaceBack, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftRight).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FaceRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock10

                            diff = (currentTime.Subtract(LeftKneeLift17).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeKick17).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift13).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift18).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeKick18).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift14).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift19).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift20).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeKick20).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift15).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift21).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeKick21).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeLift16).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift22).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock11

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorso3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift23).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLift24).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorso4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeKick).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKick).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock 12

                            diff = (currentTime.Subtract(LeftKneeBendCrouch7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross11).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross12).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch11).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch12).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross13).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross14).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch13).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeBendCrouch14).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross15).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightFootCross16).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //scoreblock 13

                            diff = (currentTime.Subtract(LeftKneeLiftAndFrontTorso5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKickAndUnderArm1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeKick_UnderArm, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(LeftKneeLiftAndUnderArm).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_LeftKneeLift_UnderArm, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKickAndUnderArm2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeKick_UnderArm, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKneelAndUnderArm).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeKneel_UnderArm, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(RightKneeKneelAndUnderArmAndLeftHandBehind).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(0 + maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(0 + maleRectDiff);
                                }
                                spriteBatch.Draw(male_RightKneeKneel_UnderArm_HandBehind, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                        }
                        //femalestart 

                        spriteBatch.Draw(n_MoveTarget, new Rectangle(500 - maleRectDiff, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                        //First Female move
                        if (GLOBALS.PLAYER_TWO_ACTIVE)
                        {
                            diff = (currentTime.Subtract(FemLeftKneeLift1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift_FaceLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeLift1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightKneeLift_FaceLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeLift2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift_FaceLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeLift2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightKneeLift_FaceLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Second score block

                            diff = (currentTime.Subtract(FemLeftKneeLift3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeLift3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftFootLiftAndFrontTorso1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootLiftAndBackTorso1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeLift4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeLift4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftFootLiftAndFrontTorso2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootLiftAndBackTorso2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Third Score block

                            diff = (currentTime.Subtract(FemHandSwingFront1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingFront, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHandSwingRight1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHipShakeBack1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HipShakeBack, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHandSwingBack1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingBack, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHandSwingLeft1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHipShakeFront1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HipShakeFront, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Fourth Score block

                            diff = (currentTime.Subtract(FemHandSwingFront2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingFront, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHandSwingRight2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingRight, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHipShakeBack2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HipShakeBack, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHandSwingBack2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingBack, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHandSwingLeft2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HandSwingLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHipShakeFront2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_HipShakeFront, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Fifth Score block

                            diff = (currentTime.Subtract(FemMoveToRightAndScrollingHands1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemMoveToRightAndScrollingHands2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemCrouchAndHipShake1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Crouch_HipShake, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemMoveToLeftAndScrollingHands1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemMoveToLeftAndScrollingHands2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemCrouchAndHipShake2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Crouch_HipShake, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Sixth Score block

                            diff = (currentTime.Subtract(FemMoveToRightAndScrollingHands3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemMoveToRightAndScrollingHands4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemCrouchAndHipShake3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Crouch_HipShake, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemMoveToLeftAndScrollingHands3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemMoveToLeftAndScrollingHands4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ScrollingHands_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemCrouchAndHipShake4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Crouch_HipShake, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch0A).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch0B).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Seventh Score block

                            diff = (currentTime.Subtract(FemRightFootCross1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Eighth Score block

                            diff = (currentTime.Subtract(FemRightElbowSway1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ElbowSway, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightElbowSway2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ElbowSway, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightElbowSway3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ElbowSway, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightElbowSway4).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ElbowSway, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Ninth Score block

                            diff = (currentTime.Subtract(FemLeftWristArcRaise1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_WristArcRaise_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightWristArcRaise1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_WristArcRaise_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHome1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Home, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftWristArcRaise2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_WristArcRaise_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightWristArcRaise2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_WristArcRaise_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemHome2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Home, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Tenth Score block

                            diff = (currentTime.Subtract(FemThrillerHandsLeft1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ThrillerhandsLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftBendHipShake1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftBendHipShake, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemThrillerHandsLeft2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ThrillerhandsLeft, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftBendHipShake2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftBendHipShake, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Eleventh Score block

                            diff = (currentTime.Subtract(FemRightElbowSway5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ElbowSway, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightElbowSway6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_ElbowSway, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeLift5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeKick2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeLiftAndFrontTorso5).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeKick).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeLift6).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightKneeKick3).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_rightKneeKick, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Twelth Score block

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch7).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch8).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch9).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch10).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross11).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross12).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch11).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch12).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Left, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross13).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross14).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch13).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemLeftKneeBendCrouch14).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeBendCrouch_Right, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross15).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightFootCross16).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_crossover, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                            //Thirteenth Score block

                            diff = (currentTime.Subtract(FemLeftKneeLiftAndFrontTorso2).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_LeftKneeLift_FrontTorso, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemBackSpinRightKneeLift1).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_BackSpinRightKneeLift, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemCrouchHipSwivel).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_Crouch_HipSwivel, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }

                            diff = (currentTime.Subtract(FemRightHandHigh).TotalMilliseconds);
                            if (Math.Abs(diff) < 2000)
                            {
                                xlocation = Convert.ToInt32(500 - maleRectDiff * (2000 - Math.Abs(diff)) / 2000);
                                if (diff > 0)
                                {
                                    xlocation = Convert.ToInt32(500 - maleRectDiff);
                                }
                                spriteBatch.Draw(female_RightHandHigh, new Rectangle(xlocation, WINDOW_HEIGHT - 150, 120, WINDOW_HEIGHT - 350), Color.White);
                            }
                        }
                        #endregion
                    }
                     
                    spriteBatch.End();

#if USE_KINECT
                    SharedSpriteBatch.Draw(depthTexture, Vector2.Zero, Color.White);
#endif
                    DrawText();
                    textFadeOut = textFadeOut.Subtract(gameTime.ElapsedGameTime);
                    p1textFadeOut = p1textFadeOut.Subtract(gameTime.ElapsedGameTime);
                    p2textFadeOut = p2textFadeOut.Subtract(gameTime.ElapsedGameTime);

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
                //DrawSkeleton(SharedSpriteBatch, new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT), jointTexture);
                //DrawSkeleton(SharedSpriteBatch, new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT), jointTexture); //draw mocap balls
                SharedSpriteBatch.End();
#endif
            }

            base.Draw(gameTime); // Draw base XNA stuff...
        }

        private void DrawSkeletons(SpriteBatch spriteBatch, Vector2 resolution, Texture2D img, Skeleton skel)
        {
#if USE_KINECT
            // Draw debug skeleton dots dots on the screen if player is being tracked by Kinect
            if (skel != null)
            {
                foreach (Joint joint in skel.Joints)
                {
                    Vector2 position = new Vector2((((0.5f * joint.Position.X) + 0.5f) * (resolution.X)), (((-0.5f * joint.Position.Y) + 0.5f) * (resolution.Y)));
                    spriteBatch.Draw(img, new Rectangle(Convert.ToInt32(position.X), Convert.ToInt32(position.Y), 10, 10), Color.Red);
                }
            }
#endif
        }

        private double distance2d(float x1, float x2, float y1, float y2)
        {
            double ydis = y1 - y2;
            double xdis = x1 - x2;
            double result = Math.Sqrt(((xdis * xdis) + (ydis * ydis)));
            return result;
        }


        private void DrawDebugString(SpriteFont fontVal, Color colorVal, int x, int y, string stringVal)
        {
            Color textShadowColor = Color.Black;
            textShadowColor.A = colorVal.A;
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x + 2, y + 2), textShadowColor);
            spriteBatch.DrawString(fontVal, stringVal, new Vector2(x, y), colorVal);
        }

        private void DrawText()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            //spriteBatch.Begin();
            if (gameState == (int)GameState.ATTRACT)
            {
                String beingTitle = "Being: Lebanese Dance";
                beingTitle = String.Format("{0,5}", beingTitle);
                Vector2 beingSize = font.MeasureString(beingTitle);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH / 2.0 - (beingSize.X / 2.0)), 10, beingTitle);

                String beingTitle2 = "Raise a hand to play";
                beingTitle = String.Format("{0,5}", beingTitle2);
                Vector2 beingSize2 = font.MeasureString(beingTitle2);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH / 2.0 - (beingSize2.X / 2.0)), WINDOW_HEIGHT - 40, beingTitle2);

                spriteBatch.Draw(n_P1icon, new Rectangle((WINDOW_WIDTH / 2) - 270, WINDOW_HEIGHT - 150, 120, 150), Color.White);
                spriteBatch.Draw(n_P2icon, new Rectangle((WINDOW_WIDTH / 2) + 150, WINDOW_HEIGHT - 150, 120, 150), Color.White);
            
            }
            else if (bShowDebugText)
            {
                displayScoreTextF = String.Format("{0,5}", displayScoreF);
                //Vector2 scoreTextSize = resultFont.MeasureString(displayScoreText);
                //DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreTextSize.X), 10, displayScoreText);
                Vector2 scoreSize = font.MeasureString(displayScoreTextF);
                DrawDebugString(font, Color.White, (int)(WINDOW_WIDTH - scoreSize.X - 10), 10, displayScoreTextF);

                displayScoreText = String.Format("{0,5}", displayScore);
                Vector2 scoreSize2 = font.MeasureString(displayScoreText);
                DrawDebugString(font, Color.White, (int)(10), 10, displayScoreText);
            }

            double textFadeOutAmt = textFadeOut.TotalMilliseconds / 2000.0;

            resultColor.A = (byte)Math.Max((255 * textFadeOutAmt), 0);
            Vector2 resultSize = resultFont.MeasureString(resultString);
            DrawDebugString(resultFont, resultColor, (int)(10), 25, resultString);

            resultColorF.A = (byte)Math.Max((255 * textFadeOutAmt), 0);
            Vector2 resultSizeF = resultFont.MeasureString(resultStringF);
            DrawDebugString(resultFont, resultColorF, (int)(WINDOW_WIDTH - resultSizeF.X), 25, resultStringF);


            Color pColor = Color.White;
            double playerFadeOut = p1textFadeOut.TotalMilliseconds / 2000.0;
            pColor.A = (byte)Math.Max((255 * playerFadeOut), 0);
            if (!GLOBALS.PLAYER_ONE_ACTIVE) pColor.A = 0;
            DrawDebugString(resultFont, pColor, (int)(10), 25, p1JoinString);

            double playerFadeOut2 = p2textFadeOut.TotalMilliseconds / 2000.0;
            pColor.A = (byte)Math.Max((255 * playerFadeOut2), 0);
            Vector2 p2size = resultFont.MeasureString(p2JoinString);
            if (!GLOBALS.PLAYER_TWO_ACTIVE) pColor.A = 0;
            DrawDebugString(resultFont, pColor, (int)(WINDOW_WIDTH - p2size.X), 25, p2JoinString);



            /*    Color recentScoreColor = Color.White;
                recentScoreColor.A = (byte)Math.Max(255 * Math.Pow(textFadeOutAmt, 10), 0);
                Vector2 tempScoreSize = font.MeasureString(displayRecentScoreText);
                DrawDebugString(font, recentScoreColor, (int)(WINDOW_WIDTH - tempScoreSize.X - 10), 30, displayRecentScoreText);*/


            spriteBatch.End();
            //GraphicsDevice.BlendState = DefaultBlendState;
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

            int numPeople = 0;

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
            //Get the number of players
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        numPeople++;
                    }
                }
            }
            
            // If there is valid skelton data, examine for dance steps
            if (skeletonData != null)
            {
                foreach (Skeleton skel in skeletonData)
                {

                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        //skeleton = skel;

                        TimeSpan time = videoTime.Elapsed;

                        if (false)
                        {
                            #region old skel code
                            /*
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
                            */
                            #endregion
                        }
                        
                        float lhy = skel.Joints[JointType.HandLeft].Position.Y;
                        float hy = skel.Joints[JointType.Head].Position.Y;
                        //Player Recog- Must hold hand up for a set period of time to register, cannot already have that player playing
                        //left hand raised = play as male
                        if (gameState == (int)GameState.ATTRACT)
                        {
                            selectedDance.mc.UpdateSkeleton(skel);
                            if (!GLOBALS.PLAYER_ONE_ACTIVE && selectedDance.mc.LeftHandRaiseTriggered())
                            {
                                if (!GLOBALS.PLAYER_TWO_ACTIVE || numPeople > 1)
                                {
                                    if (malePlayerRecog == stepFinished) //Start recording
                                    {
                                        if (gameState == (int)GameState.ATTRACT) malePlayerRecog = introVideoTime.Elapsed;
                                        else malePlayerRecog = videoTime.Elapsed;
                                    }
                                    else
                                    { //If their hand has been up for over 5 seconds
                                        if (videoTime.Elapsed.Subtract(malePlayerRecog).Milliseconds > 800 || (gameState == (int)GameState.ATTRACT && introVideoTime.Elapsed.Subtract(malePlayerRecog).Milliseconds > 800))
                                        {
                                            GLOBALS.PLAYER_ONE_ACTIVE = true;
                                            P1skeleton = skel.TrackingId;
                                            malePlayingText = true;
                                            p1textFadeOut = new TimeSpan(0, 0, 3); // 3-second fadeout
                                            //ensure 1 person doesn't count as 2
                                            /*      if ((femPlaying && numPeople < 2))
                                                  {
                                                      malePlaying = false;
                                                      malePlayingText = false;
                                                      p1textFadeOut = new TimeSpan(0, 0, 0); // 3-second fadeout
                                                  }*/
                                            if (gameState == (int)GameState.ATTRACT && GLOBALS.PLAYER_ONE_ACTIVE)
                                            {
                                                videoPlayer.Dispose();
                                                gameState = (int)GameState.HOWTOPLAY;
                                            }
                                        }
                                    }
                                }
                            }
                            //right hand raised = play as female
                            if (!GLOBALS.PLAYER_TWO_ACTIVE && selectedDance.mc.RightHandRaiseTriggered())
                            {
                                if (!GLOBALS.PLAYER_ONE_ACTIVE || numPeople > 1)
                                {
                                    if (femPlayerRecog == stepFinished) //Start recording
                                    {
                                        if (gameState == (int)GameState.ATTRACT) femPlayerRecog = introVideoTime.Elapsed;
                                        else femPlayerRecog = videoTime.Elapsed;
                                    }
                                    else
                                    { //If their hand has been up for over 3 seconds
                                        if (videoTime.Elapsed.Subtract(femPlayerRecog).Milliseconds > 800 || (gameState == (int)GameState.ATTRACT && introVideoTime.Elapsed.Subtract(femPlayerRecog).Milliseconds > 800))
                                        {
                                            GLOBALS.PLAYER_TWO_ACTIVE = true;
                                            P2skeleton = skel.TrackingId;
                                            femPlayingText = true;
                                            p2textFadeOut = new TimeSpan(0, 0, 3); // 3-second fadebout
                                            if (gameState == (int)GameState.ATTRACT && GLOBALS.PLAYER_TWO_ACTIVE)
                                            {
                                                videoPlayer.Dispose();
                                                gameState = (int)GameState.HOWTOPLAY;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //new scoring code
                        if (gameState == (int)GameState.DANCE)
                        {
                            if (skel.TrackingId != P1skeleton && skel.TrackingId != P2skeleton)
                                return;

                            int player = 1;
                            if (skel.TrackingId == P2skeleton)
                                player = 2;

                            selectedDance.mc.UpdateSkeleton(skel);
                            int newPts = 0;
                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                newPts = selectedDance.ScoreMoves(videoTime.Elapsed, skel, player);
                            }

                            if (newPts > 0)
                            {
                                if (player == 1)
                                {
                                    tempScore += newPts;
                                    tempStepsDone++;
                                }
                                else
                                {
                                    tempScoreF += newPts;
                                    tempFemStepsDone++;
                                }
                            }
                        }

                        #if nothing
                        if (false)
                        {
                            #region old move recognition
                            //////////////////////////////////////////////////
                            //MALE MOVE RECOGNITION
                            /////////////////////////////////////////////////
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

                            #endregion
                        }
                        #endif    
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