using System;
using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StackAR.Runtime
{
    public class UIManager : MonoBehaviour
    {
        public Button StartGameButton;
        public Button GameOverExit;

        [FormerlySerializedAs("gamingScoreText")]
        public Text GamingScoreText;

        [FormerlySerializedAs("gameOverScoreText")]
        public Text GameOverScoreText;

        private Animator animator;
        private static readonly int START = Animator.StringToHash("Start");
        private static readonly int GAME_OVER = Animator.StringToHash("GameOver");
        private static readonly int GAMING = Animator.StringToHash("Gaming");


        private int currentScore;
        private static readonly int UPDATE_SCORE = Animator.StringToHash("UpdateScore");

        private void Awake()
        {
            ActionNotificationCenter.DefaultCenter.AddObserver(GameStateListener, "UpdateGameState");
            ActionNotificationCenter.DefaultCenter.AddObserver(GameScoreListener, "UpdateScore");
        }

        private void Start()
        {
            StartGameButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                ActionNotificationCenter.DefaultCenter.PostNotification("UpdateGameState",
                    new GameStateNotificationData() {GameState = GameState.Gaming});
            });
            GameOverExit.GetComponent<Button>().onClick.AddListener(() => { StackARMainEntry.api.Exit(); });

            animator = GetComponent<Animator>();
            Assert.IsNotNull(animator, "Animator can not empty!");
        }

        private void GameScoreListener(BaseNotificationData _data)
        {
            if (_data is GameScoreNotificationData tmp_Data)
            {
                currentScore += tmp_Data.UpdatedScore;
                animator.SetTrigger(UPDATE_SCORE);
                if (GamingScoreText)
                    GamingScoreText.text = currentScore.ToString();
                if (GameOverScoreText)
                    GameOverScoreText.text = currentScore.ToString();
            }
        }

        private void GameStateListener(BaseNotificationData _data)
        {
            if (!(_data is GameStateNotificationData tmp_Data)) return;
            Debug.Log(tmp_Data.GameState);
            switch (tmp_Data.GameState)
            {
                case GameState.Ready:
                    if (animator)
                        animator.SetTrigger(START);
                    break;
                case GameState.Gaming:
                    if (animator)
                        animator.SetTrigger(GAMING);
                    break;
                case GameState.Over:
                    if (animator)
                        animator.SetTrigger(GAME_OVER);
                    break;
                case GameState.Scan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}