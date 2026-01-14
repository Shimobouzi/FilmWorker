using UnityEngine;

public class KakoimaDB : MonoBehaviour
{
    public static KakoimaDB Instance;

    private int gameCount = 0;
    public int getGameCount() { return gameCount; }
    public void setGameCount(int n) { gameCount += n; }
    private int missCount = 0;
    public int getMissCount() { return missCount; }
    public void setMissCount(int n) { missCount += n; }
    private int goalCount = 0;
    public int getGoalCount() { return goalCount; }
    public void setGoalCount(int n) { goalCount += n; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ResetCount()
    {
        gameCount = 0;
        missCount = 0;
        goalCount = 0;
    }
}
