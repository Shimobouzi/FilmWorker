using UnityEngine;

public class SoundLoader : MonoBehaviour
{
    [SerializeField] private string csvPath = "sound_data";

    private void Awake()
    {
        LoadCsv();
    }

    private void LoadCsv()
    {
        //CSV読み込み
        TextAsset csvFlie = Resources.Load<TextAsset>(csvPath);

        if (csvFlie == null)
        {
            Debug.LogError("CSVが取得できませんでした");
            return;
        }

        string[] lines = csvFlie.text.Split('\n');  //改行で分ける

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();          //空白の除去
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            if (i == 0)                              //ヘッダー除去
            {
                continue;
            }

            string[] parts = line.Split(',');

            if (parts.Length < 3)
            {
                Debug.LogError("CSVが不十分です");
                continue;
            }

            string key = parts[0].Trim();
            string type = parts[1].Trim().ToLower();
            string file = parts[2].Trim();
            string channelStr = parts.Length > 3 ? parts[3].Trim() : "";

            if (!int.TryParse(channelStr, out int channel))
            {
                Debug.LogWarning("channelの数値を正しく取得できません");
                channel = -1;
            }

            Debug.Log($"[{i}] key:{key}, type:{type}, file:{file}, channel:{channel}");

            if (type == "bgm")
            {
                SoundManager.LoadBgm(key, file);
            }
            else if (type == "se")
            {
                SoundManager.LoadSE(key, file, channel);
            }
            else
            {
                Debug.LogWarning($"未定義のtypeです: {type}（行: {i + 1}）");
            }
        }
    }
}