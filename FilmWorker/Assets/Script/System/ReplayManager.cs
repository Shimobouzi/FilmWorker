using UnityEngine;
using System.Collections.Generic;

public class ReplayManager : MonoBehaviour
{
    public GameObject replayPrefab;

    readonly List<ReplayCharacter> spawned = new();
    int nextSpawnIndex;

    void CleanupDestroyed()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null)
                spawned.RemoveAt(i);
        }
    }

    void Update()
    {
        // テスト用：Lキーで最新リプレイを生成
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadAndSpawnReplay();
        }
    }

    public IReadOnlyList<ReplayCharacter> GetSpawnedReplays() => spawned;

    public void ClearSpawnedReplays()
    {
        CleanupDestroyed();

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            var rc = spawned[i];
            if (rc == null) continue;
            Destroy(rc.gameObject);
        }

        spawned.Clear();
        nextSpawnIndex = 0;
    }

    public void RestartAllReplays(Vector3 startPosition)
    {
        CleanupDestroyed();

        for (int i = 0; i < spawned.Count; i++)
        {
            var rc = spawned[i];
            if (rc == null) continue;

            rc.transform.position = startPosition;
            rc.ResetToStart();
            rc.SetPaused(false);
        }

        Physics2D.SyncTransforms();
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

        SpawnReplay(data);
    }

    public ReplayCharacter SpawnReplay(ReplayData data)
    {
        return SpawnReplayInternal(data, null);
    }

    public ReplayCharacter SpawnReplay(ReplayData data, Vector3 spawnPosition)
    {
        return SpawnReplayInternal(data, spawnPosition);
    }

    ReplayCharacter SpawnReplayInternal(ReplayData data, Vector3? spawnPosition)
    {
        if (replayPrefab == null)
        {
            Debug.LogWarning("ReplayManager: replayPrefab is null.");
            return null;
        }

        GameObject obj;
        if (spawnPosition.HasValue)
            obj = Instantiate(replayPrefab, spawnPosition.Value, Quaternion.identity);
        else
            obj = Instantiate(replayPrefab);

        var rc = obj.GetComponent<ReplayCharacter>();
        if (rc == null)
            rc = obj.AddComponent<ReplayCharacter>();

        rc.Initialize(data, nextSpawnIndex++);
        spawned.Add(rc);
        return rc;
    }

    public ReplayCharacter FindNearestReplay(Vector3 position, float radius)
    {
        CleanupDestroyed();

        ReplayCharacter best = null;
        float bestDistSq = float.PositiveInfinity;
        int bestSpawnIndex = int.MaxValue;
        float radiusSq = radius * radius;

        for (int i = 0; i < spawned.Count; i++)
        {
            var rc = spawned[i];
            if (rc == null) continue;

            var d = rc.transform.position - position;
            float distSq = d.sqrMagnitude;
            if (distSq > radiusSq) continue;

            if (distSq < bestDistSq)
            {
                best = rc;
                bestDistSq = distSq;
                bestSpawnIndex = rc.SpawnIndex;
                continue;
            }

            if (Mathf.Approximately(distSq, bestDistSq))
            {
                // 同距離なら生成順が早いものを優先
                if (rc.SpawnIndex < bestSpawnIndex)
                {
                    best = rc;
                    bestSpawnIndex = rc.SpawnIndex;
                }
            }
        }

        return best;
    }
}
