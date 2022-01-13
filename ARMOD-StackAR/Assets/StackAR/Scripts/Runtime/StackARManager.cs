using System;
using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace StackAR
{

    public class StackARManager : MonoBehaviour
    {
        public static StackARManager INSTANCE { get; private set; }

        private MovingCube MovingCube;

        public Color32[] cubeColors = new Color32[4];
        public CubeSpawner[] CubeSpawners;

        private int spawnerIdex = 0;
        private int stackCount = 0;

        private Transform selfTrans;
        private GameState gameState;

        internal Transform LastCube;
        internal GameScoreNotificationData gameScoreNotificationData ;

        private Transform startCube;
        private AudioSource audioSource;


        private void Awake()
        {
            INSTANCE = this;
        }

        private void Start()
        {
            audioSource = FindObjectOfType<AudioSource>();
            Assert.IsNotNull(audioSource);
            LastCube = GameObject.Find("Start").transform;
            startCube = LastCube;
            selfTrans = transform;
            ActionNotificationCenter.DefaultCenter.AddObserver(GameStateListener, "UpdateGameState");
            gameScoreNotificationData = new GameScoreNotificationData();
        }


        private void Update()
        {
            if (gameState == GameState.Ready) return;

            if (Input.GetMouseButtonDown(0) && !StackARMainEntry.api.IsPointerOverUi())
            {
                if (MovingCube && gameState == GameState.Gaming)
                {
                    MovingCube.Stop();
                    stackCount++;

                    audioSource.clip = StackARMainEntry.BlockFallSound;
                    audioSource.Play();
                }

                if (gameState == GameState.Gaming)
                    SpawnCube();
            }

            if (LastCube != startCube && gameState == GameState.Gaming)
                selfTrans.localPosition = Vector3.Lerp(selfTrans.localPosition,
                    stackCount * LastCube.transform.localScale.y / 2 * Vector3.down, Time.deltaTime * 5f);
            else if (gameState == GameState.Over)
            {
                selfTrans.localPosition = Vector3.Lerp(selfTrans.localPosition, 0 * Vector3.up, Time.deltaTime * 3f);
            }
        }

        private void SpawnCube()
        {
            MovingCube = CubeSpawners[spawnerIdex].SpawnCube();
            spawnerIdex = spawnerIdex == 0 ? 1 : 0;
            var tmp_Mesh = MovingCube.GetComponent<MeshFilter>().mesh;
            Utility.ApplyColorToMesh(tmp_Mesh, GetTime(), cubeColors);
        }

        public float GetTime()
        {
            return Mathf.Sin(stackCount * 0.25f);
        }


        private void OnDestroy()
        {
            INSTANCE = null;
            //Remove all notification action
            ActionNotificationCenter.DefaultCenter.RemoveObserver("UpdateGameState");
            ActionNotificationCenter.DefaultCenter.RemoveObserver("UpdateScore");
            ActionNotificationCenter.DefaultCenter.RemoveObserver("UpdateGameState");
        }


        private void GameStateListener(BaseNotificationData _data)
        {
            if (_data is GameStateNotificationData tmp_Data)
            {
                gameState = tmp_Data.GameState;
                if (gameState == GameState.Gaming)
                    SpawnCube();
            }
        }
    }
}