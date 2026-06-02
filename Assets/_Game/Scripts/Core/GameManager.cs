using UnityEngine;
using UnityEngine.SceneManagement;

namespace MedievalRTS.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
            EventBus.Publish(new GameStateChangedEvent(newState));
            SceneManager.LoadScene(SceneNameForState(newState));
        }

        public static string SceneNameForState(GameState state)
        {
            return state switch
            {
                GameState.MainMenu => "01_MainMenu",
                GameState.BaseBuilder => "02_BaseBuilder",
                GameState.Battle => "05_TestBattle",
                GameState.Result => "04_Result",
                _ => "05_TestBattle"
            };
        }
    }
}
