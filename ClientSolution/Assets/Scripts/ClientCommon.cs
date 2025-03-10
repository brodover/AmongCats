﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ClientCommon
{
    public const string ServerUrl = "https://localhost:7083";
    public const string ServerGameHub = ClientCommon.ServerUrl + "/GameHub";

    public static class File
    {
        public const string HumanPrefab = "Prefabs/Human";
        public const string CatPrefab = "Prefabs/Cat";
        public const string GameTimerPrefab = "Prefabs/GameTimer";
        public const string MessMeterPrefab = "Prefabs/MessMeter";
    }

    public static class Scene
    {
        public const string Start = "StartScene";
        public const string Game = "GameScene";
    }

    public static class Game
    {
        public const float TimeToMaxSpeed = 0.3f;
        public const float HumanMovementSpeed = 7f;
        public const float CatMovementSpeed = 12f;
        public const int InitMess = 2;

        public enum State
        {
            Uninitialized=0,
            Ongoing=5,
            Ended=10,
            EndHandled=11,
            Closed=15,
            CloseHandled=16,
        }
    }
}
