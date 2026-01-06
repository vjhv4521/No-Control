using UnityEngine;
using UnityEngine.SceneManagement;

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
        EnsureMinimap();
    }

    void EnsureMinimap()
    {
        var existing = FindObjectOfType<MinimapController>();
        if (existing != null) return;
        var root = new GameObject("MinimapRoot");
        root.transform.SetParent(transform, false);
        root.AddComponent<MinimapController>();
        DontDestroyOnLoad(root);
    }
}
