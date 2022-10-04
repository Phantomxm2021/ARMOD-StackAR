using UnityEngine;
using System;
using System.Threading.Tasks;
using com.Phantoms.ActionNotification.Runtime;
using com.Phantoms.ARMODAPI.Runtime;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace StackAR.Runtime
{
    public class StackARMainEntry
    {
        internal static API api = new API(CONST_PROJECT_NAME);

        private const string CONST_PROJECT_NAME = "StackAR";
        private const string CONST_CUBE_NAME = "Cube";
        private const string CONST_WORLD_NAME = "World";
        private const string CONST_CUBE_SPAWNER_X_NAME = "CubeSpawner X";
        private const string CONST_CUBE_SPAWNER_Z_NAME = "CubeSpawner Z";
        public const string CONST_CUBE_MANAGER_NAME = "CubeManager";
        public const string CONST_UI_CANVAS_NAME = "StackARCanvas";
        public const string CONST_BLOCK_FALL_SOUNDS_NAME = "BlockFall";
        private const string CONST_GAME_LAUNCH_NAME = "GameLaunch";

        private string[] cubeColors = new[]
        {
            "#FC7A57",
            "#FCD757",
            "#EEFC57",
            "#5E5B52"
        };

        private bool placed;

        private GameObject cubePrefab;
        private GameObject worldGO;
        private GameObject stackARCanvasGO;
        internal static AudioClip BlockFallSound;

        public StackARManager stackArManager;

        //Please delete the function if it is not used
        public async void OnLoad()
        {
            //Use this for initialization
            var tmp_UICanvasPrefab = await api.LoadAssetAsync<GameObject>(CONST_UI_CANVAS_NAME);
            stackARCanvasGO = Object.Instantiate(tmp_UICanvasPrefab);

            cubePrefab = await api.LoadAssetAsync<GameObject>(CONST_CUBE_NAME);
            var tmp_WorldPrefab = await api.LoadAssetAsync<GameObject>(CONST_WORLD_NAME);
            worldGO = UnityEngine.Object.Instantiate(tmp_WorldPrefab);


            SetupScript();

            BlockFallSound = await api.LoadAssetAsync<AudioClip>(CONST_BLOCK_FALL_SOUNDS_NAME);
            Assert.IsNotNull(BlockFallSound);

            await Task.Delay(2000);
            api.ChangeARAlgorithmLife(new ARAlgorithmNotificationData()
            {
                ARAlgorithmOperator = ARAlgorithmOperator.StartAlgorithm, ARAlgorithmType = ARAlgorithmType.FocusSLAM
            });


            if (Application.isEditor)
            {
                ARObjectToHitPoint(Vector3.forward);
            }
        }


        private void SetupScript()
        {
            // var tmp_MovingCubeScript = cubePrefab.GetComponent<MovingCube>();
            // tmp_MovingCubeScript.moveSpeed = 3;

            var tmp_SpawnerX = GameObject.Find(CONST_CUBE_SPAWNER_X_NAME);
            var tmp_SpawnerZ = GameObject.Find(CONST_CUBE_SPAWNER_Z_NAME);

            CubeSpawner tmp_SpawnerXScript = null, tmp_SpawnerZScript = null;

            if (tmp_SpawnerX)
            {
                tmp_SpawnerXScript = tmp_SpawnerX.AddComponent<CubeSpawner>();
                tmp_SpawnerXScript.cube = cubePrefab;
                tmp_SpawnerXScript.root = tmp_SpawnerX.transform;
                tmp_SpawnerXScript.CubeDirection = CubeDirection.XAxis;
            }

            if (tmp_SpawnerZ)
            {
                tmp_SpawnerZScript = tmp_SpawnerZ.AddComponent<CubeSpawner>();
                tmp_SpawnerZScript.cube = cubePrefab;
                tmp_SpawnerZScript.root = tmp_SpawnerZ.transform;
                tmp_SpawnerZScript.CubeDirection = CubeDirection.ZAxis;
            }

            stackArManager = GameObject.Find(CONST_CUBE_MANAGER_NAME).AddComponent<StackARManager>();
            stackArManager.cubeColors = new Color32[cubeColors.Length];
            int tmp_ForLoopIndex = 0;
            foreach (string tmp_Color in cubeColors)
            {
                if (ColorUtility.TryParseHtmlString(tmp_Color, out Color tmp_OutColor))
                {
                    stackArManager.cubeColors[tmp_ForLoopIndex] = tmp_OutColor;
                }
            
                tmp_ForLoopIndex++;
            }

            stackArManager.CubeSpawners = new CubeSpawner[2];
            stackArManager.CubeSpawners[0] = tmp_SpawnerXScript;
            stackArManager.CubeSpawners[1] = tmp_SpawnerZScript;

            //disblae
            worldGO.SetActive(false);
        }

        public void OnEvent(BaseNotificationData _data)
        {
            if (placed) return;
            if (!worldGO) return;
            if (!(_data is FocusResultNotificationData tmp_Data)) return;


            switch (tmp_Data.FocusState)
            {
                case FindingType.Finding:
                    break;
                case FindingType.Found:
                    ARObjectToHitPoint(tmp_Data.FocusPos);
                    break;
                case FindingType.Limit:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void ARObjectToHitPoint(Vector3 _pos)
        {
            var tmp_ARGOTrans = worldGO.transform;
            tmp_ARGOTrans.position = _pos;
            tmp_ARGOTrans.rotation = Quaternion.identity;
            worldGO.SetActive(true);
            placed = true;
            Handheld.Vibrate();
            ActionNotificationCenter.DefaultCenter.PostNotification("UpdateGameState",
                new GameStateNotificationData() {GameState = GameState.Ready});
            api.ChangeARAlgorithmLife(new ARAlgorithmNotificationData()
            {
                ARAlgorithmOperator = ARAlgorithmOperator.PauseAlgorithm, ARAlgorithmType = ARAlgorithmType.FocusSLAM
            });
            var tmp_AudioSource = Object.FindObjectOfType<AudioSource>();
            Assert.IsNotNull(tmp_AudioSource);
            tmp_AudioSource.clip = await api.LoadAssetAsync<AudioClip>(CONST_GAME_LAUNCH_NAME);
            tmp_AudioSource.Play();
        }
    }

    public enum GameState
    {
        Scan,
        Ready,
        Gaming,
        Over
    }


    public class GameStateNotificationData : BaseNotificationData
    {
        public GameState GameState;
    }


    public class GameScoreNotificationData : BaseNotificationData
    {
        public int UpdatedScore = 0;
    }
}