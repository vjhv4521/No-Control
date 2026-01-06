using UnityEngine;

public class GameApp : MonoBehaviour
{
    private static GameApp _instance;
    public static GameApp Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameApp>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[GameApp]");
                    _instance = go.AddComponent<GameApp>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public enum GameState
    {
        Normal,
        Break,
        Dead,
        Win
    }

    public GameState State { get; set; } = GameState.Normal;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}