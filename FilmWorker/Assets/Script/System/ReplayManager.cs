using UnityEngine;

public class ReplayManager : MonoBehaviour
{
    public GameObject replayPrefab;

    void Update()
    {
        // テスト用：Lキーで最新リプレイを再生
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadAndSpawnReplay();
        }
    }

    public void LoadAndSpawnReplay()
    {
        int count = JsonController.instance.GetFileCount();
        if (count == 0)
        {
            Debug.Log("No replay files found.");
            return;
        }

        int latestId = count - 1;
        ReplayData data = JsonController.instance.LoadFileId(latestId);

        GameObject obj = Instantiate(replayPrefab);
        ReplayCharacter rc = obj.AddComponent<ReplayCharacter>();
        rc.Initialize(data);
    }
}
