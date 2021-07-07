using UnityEngine;
using System;
using System.Threading.Tasks;
using com.Phantoms.ActionNotification.Runtime;
using com.Phantoms.ARMODAPI.Runtime;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace StackAR
{
    public class StackARMainEntry
    {
        internal static API api = new API();

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
            "C7FFEB",
            "C7FFDA",
            "C4F4C7",
            "9BB291"
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
            await BuildUI();

            cubePrefab = await api.LoadAssetAsync<GameObject>(CONST_PROJECT_NAME, CONST_CUBE_NAME);
            var tmp_WorldPrefab = await api.LoadAssetAsync<GameObject>(CONST_PROJECT_NAME, CONST_WORLD_NAME);
            worldGO = api.InstanceGameObject(tmp_WorldPrefab, string.Empty, null);
            worldGO.SetActive(false);

            SetupScript();

            BlockFallSound = await api.LoadAssetAsync<AudioClip>(CONST_PROJECT_NAME, CONST_BLOCK_FALL_SOUNDS_NAME);
            Assert.IsNotNull(BlockFallSound);

            await Task.Delay(2000);
            api.ChangeARAlgorithmLife(new ARAlgorithmNotificationData()
            {
                ARAlgorithmOperator = ARAlgorithmOperator.StartAlgorithm, ARAlgorithmType = ARAlgorithmType.FocusSLAM
            });
        }

        private async Task BuildUI()
        {
            var tmp_UICanvasPrefab = await api.LoadAssetAsync<GameObject>(CONST_PROJECT_NAME, CONST_UI_CANVAS_NAME);
            stackARCanvasGO = api.InstanceGameObject(tmp_UICanvasPrefab, string.Empty, null);
            stackARCanvasGO.AddComponent<UIManager>();

            var tmp_StartGameButtonGO = api.FindGameObjectByName("Start_Game_Button");
            Assert.IsNotNull(tmp_StartGameButtonGO);
            tmp_StartGameButtonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                ActionNotificationCenter.DefaultCenter.PostNotification("UpdateGameState",
                    new GameStateNotificationData() {GameState = StackARManager.GameState.Gaming});
            });

            var tmp_GameOverExit = api.FindGameObjectByName("Game_Over_Exit");
            Assert.IsNotNull(tmp_GameOverExit);
            tmp_GameOverExit.GetComponent<Button>().onClick.AddListener(() => { api.ExitAR(); });
        }

        private void SetupScript()
        {
            var tmp_MovingCubeScript = cubePrefab.AddComponent<MovingCube>();
            tmp_MovingCubeScript.moveSpeed = 3;

            var tmp_SpawnerX = api.FindGameObjectByName(CONST_CUBE_SPAWNER_X_NAME);
            var tmp_SpawnerZ = api.FindGameObjectByName(CONST_CUBE_SPAWNER_Z_NAME);

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


            stackArManager = api.FindGameObjectByName(CONST_CUBE_MANAGER_NAME).AddComponent<StackARManager>();
            stackArManager.cubeColors = new Color32[cubeColors.Length];
            stackArManager.cubeColors[0] = new Color(199f / 255f, 1f, 235f / 255f);
            stackArManager.cubeColors[1] = new Color(199f / 255f, 1f, 218f / 255f);
            stackArManager.cubeColors[2] = new Color(196f / 255f, 244 / 255f, 199f / 255f);
            stackArManager.cubeColors[3] = new Color(155f / 255f, 178 / 255f, 145f / 255f);

//            int tmp_ForLoopIndex = 0;
//            foreach (string tmp_Color in cubeColors)
//            {
//                if (ColorUtility.TryParseHtmlString(tmp_Color, out Color tmp_OutColor))
//                {
//                    stackArManager.cubeColors[tmp_ForLoopIndex] = tmp_OutColor;
//                }
//
//                tmp_ForLoopIndex++;
//            }

            stackArManager.CubeSpawners = new CubeSpawner[2];
            stackArManager.CubeSpawners[0] = tmp_SpawnerXScript;
            stackArManager.CubeSpawners[1] = tmp_SpawnerZScript;
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
                new GameStateNotificationData() {GameState = StackARManager.GameState.Ready});
            api.ChangeARAlgorithmLife(new ARAlgorithmNotificationData()
            {
                ARAlgorithmOperator = ARAlgorithmOperator.PauseAlgorithm, ARAlgorithmType = ARAlgorithmType.FocusSLAM
            });
            var tmp_AudioSource = Object.FindObjectOfType<AudioSource>();
            Assert.IsNotNull(tmp_AudioSource);
            tmp_AudioSource.clip = await api.LoadAssetAsync<AudioClip>(CONST_PROJECT_NAME, CONST_GAME_LAUNCH_NAME);
            tmp_AudioSource.Play();
        }
    }
}