using System;
using com.Phantoms.ActionNotification.Runtime;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace StackAR
{
    public class UIManager : MonoBehaviour
    {
        private Animator animator;
        private static readonly int START = Animator.StringToHash("Start");
        private static readonly int GAME_OVER = Animator.StringToHash("GameOver");
        private static readonly int GAMING = Animator.StringToHash("Gaming");

        private Text gamingScoreText;
        private Text gameOverScoreText;

        private int currentScore;
        private static readonly int UPDATE_SCORE = Animator.StringToHash("UpdateScore");

        private void Start()
        {
            animator = GetComponent<Animator>();
            Assert.IsNotNull(animator, "Animator can not empty!");

            ActionNotificationCenter.DefaultCenter.AddObserver(GameStateListener, "UpdateGameState");
            ActionNotificationCenter.DefaultCenter.AddObserver(GameScoreListener, "UpdateScore");

            gamingScoreText = GetTextComponent("Gaming_Score_Text");
            Assert.IsNotNull(gamingScoreText, "GamingScoreText is empty!");

            gameOverScoreText = GetTextComponent("Gam_Over_Score_Text");
            Assert.IsNotNull(gameOverScoreText, "GamingScoreText is empty!");
        }

        private void GameScoreListener(BaseNotificationData _data)
        {
            if (_data is GameScoreNotificationData tmp_Data)
            {
                currentScore += tmp_Data.UpdatedScore;
                animator.SetTrigger(UPDATE_SCORE);
                if (gamingScoreText)
                    gamingScoreText.text = currentScore.ToString();
                if (gameOverScoreText)
                    gameOverScoreText.text = currentScore.ToString();
            }
        }

        private void GameStateListener(BaseNotificationData _data)
        {
            if (!(_data is GameStateNotificationData tmp_Data)) return;
            switch (tmp_Data.GameState)
            {
                case StackARManager.GameState.Ready:
                    if (animator)
                        animator.SetTrigger(START);
                    break;
                case StackARManager.GameState.Gaming:
                    if (animator)
                        animator.SetTrigger(GAMING);
                    break;
                case StackARManager.GameState.Over:
                    if (animator)
                        animator.SetTrigger(GAME_OVER);
                    break;
                case StackARManager.GameState.Scan:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Text GetTextComponent(string _gameObjectName)
        {
            var tmp_GameObject = StackARMainEntry.api.FindGameObjectByName(_gameObjectName);
            Assert.IsNotNull(tmp_GameObject);
            return tmp_GameObject.GetComponent<Text>();
        }
    }

    public class GameStateNotificationData : BaseNotificationData
    {
        public StackARManager.GameState GameState;
    }


    public class GameScoreNotificationData : BaseNotificationData
    {
        public int UpdatedScore = 0;
    }
}