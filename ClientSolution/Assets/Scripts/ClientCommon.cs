using System;
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
    }

    public static class Scene
    {
        public const string Start = "StartScene";
        public const string Game = "GameScene";
    }

    public static class Game
    {
        public const float TimeToMaxSpeed = 0.3f;
        public const float CatMovementSpeed = 8f; //18f
    }
}
