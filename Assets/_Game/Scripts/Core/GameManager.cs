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
            switch (newState)
            {
                case GameState.MainMenu:    SceneManager.LoadScene("MainMenu");    break;
                case GameState.BaseBuilder: SceneManager.LoadScene("BaseBuilder"); break;
                case GameState.Battle:      SceneManager.LoadScene("Battle");      break;
                case GameState.Result:      SceneManager.LoadScene("Result");      break;
            }
        }
    }
}
