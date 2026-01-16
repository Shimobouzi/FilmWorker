using System.IO;
using UnityEngine;

public class JsonController : MonoBehaviour
{
    public static JsonController instance;

    private string fileN = "replay";
    private string replayFolder;
    private void Awake()
    {
        if (instance == null) {
            instance = this;
            replayFolder = Path.Combine(Application.persistentDataPath, "Replays");
            Directory.CreateDirectory(replayFolder);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void SaveFile(ReplayData data)
    {
        string fileName = fileN + GetFileCount().ToString() + ".json";
        string jsonFile = JsonUtility.ToJson(data);
        File.WriteAllText(Path.Combine(replayFolder, fileName), jsonFile);
        Debug.Log("Saved: " + fileName);
    }

    public ReplayData LoadFileId(int id)
    {
        string fileName = fileN + id.ToString() + ".json";
        string jsonFile = File.ReadAllText(Path.Combine(replayFolder, fileName));

        return JsonUtility.FromJson<ReplayData>(jsonFile);
    }

    public int GetFileCount()
    {
        // フォルダが無ければ 0 とする
        if (!Directory.Exists(replayFolder)) return 0;
        string[] files = Directory.GetFiles(replayFolder, "*.json");
        return files.Length;
    }

    public void DeleteAllReplays()
    {
        if (string.IsNullOrEmpty(replayFolder))
            replayFolder = Path.Combine(Application.persistentDataPath, "Replays");

        if (!Directory.Exists(replayFolder))
        {
            Directory.CreateDirectory(replayFolder);
            return;
        }

        string[] files = Directory.GetFiles(replayFolder, "*.json");

        foreach (string file in files)
        {
            File.Delete(file);
            Debug.Log("Deleted: " + file);
        }
    }


}
