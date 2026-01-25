using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class JsonController : MonoBehaviour
{
    public static JsonController Instance { get; private set; }

    const string FilePrefix = "replay";
    string replayFolder;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        replayFolder = Path.Combine(Application.persistentDataPath, "Replays");
        Directory.CreateDirectory(replayFolder);
    }

    public void SaveFile(ReplayData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        int id = GetNextId();
        string fileName = $"{FilePrefix}{id}.json";
        string path = Path.Combine(replayFolder, fileName);

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);

        Debug.Log($"Saved replay: {fileName}");
    }

    public ReplayData LoadFileId(int id)
    {
        string fileName = $"{FilePrefix}{id}.json";
        string path = Path.Combine(replayFolder, fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Replay not found: {fileName}");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<ReplayData>(json);
    }

    public int GetFileCount()
    {
        if (string.IsNullOrEmpty(replayFolder) || !Directory.Exists(replayFolder)) return 0;
        return Directory.GetFiles(replayFolder, "*.json").Length;
    }

    public void DeleteAllReplays()
    {
        if (string.IsNullOrEmpty(replayFolder) || !Directory.Exists(replayFolder)) return;

        foreach (string file in Directory.GetFiles(replayFolder, "*.json"))
        {
            try
            {
                File.Delete(file);
                Debug.Log($"Deleted replay: {Path.GetFileName(file)}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Delete failed: {file} ({e.Message})");
            }
        }
    }

    int GetNextId()
    {
        if (string.IsNullOrEmpty(replayFolder) || !Directory.Exists(replayFolder)) return 0;

        var ids = Directory.GetFiles(replayFolder, $"{FilePrefix}*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => name.Substring(FilePrefix.Length))
            .Select(s => int.TryParse(s, out var n) ? (int?)n : null)
            .Where(n => n.HasValue)
            .Select(n => n.Value);

        return ids.Any() ? ids.Max() + 1 : 0;
    }
}
