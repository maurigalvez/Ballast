using System.Collections.Generic;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private Transform diver;
        [SerializeField] private ItemPickup manifestPrefab;
        [SerializeField] private ItemPickup nonManifestPrefab;
        [SerializeField] private ManifestRegistry manifestRegistry;
        [SerializeField] private ItemData[] nonManifestPool;

        [Header("Spawn cadence")]
        [SerializeField] private float spawnAheadDistance = 25f;
        [SerializeField] private float verticalSpacingMin = 6f;
        [SerializeField] private float verticalSpacingMax = 12f;
        [SerializeField] private float lateralRange = 4f;
        [SerializeField] private float tunnelZ = 0f;

        [Header("Manifest schedule")]
        [Tooltip("One transform per manifest item, paired by index. Each manifest prefab spawns once at its transform when the diver descends near it.")]
        [SerializeField] private Transform[] manifestSpawnPoints;

        [Header("Wall avoidance")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float wallCheckRadius = 0.8f;
        [SerializeField, Min(1)] private int maxRejectRetries = 4;

        [Header("Despawn")]
        [SerializeField] private float despawnAboveDiver = 30f;

        private float nextSpawnY;
        private readonly List<ItemPickup> live = new();
        private bool[] manifestSpawned;

        private void Start()
        {
            if (diver == null) return;
            nextSpawnY = diver.position.y - 5f;
            int count = manifestSpawnPoints != null ? manifestSpawnPoints.Length : 0;
            manifestSpawned = new bool[count];
        }

        private void Update()
        {
            if (diver == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != RunState.Running) return;

            float frontier = diver.position.y - spawnAheadDistance;

            SpawnManifestNearFrontier(frontier);

            int safety = 16;
            while (nextSpawnY > frontier && safety-- > 0)
            {
                SpawnNonManifest(nextSpawnY);
                nextSpawnY -= Random.Range(verticalSpacingMin, verticalSpacingMax);
            }

            for (int i = live.Count - 1; i >= 0; i--)
            {
                var p = live[i];
                if (p == null) { live.RemoveAt(i); continue; }
                if (p.transform.parent != null) { live.RemoveAt(i); continue; }
                if (p.transform.position.y > diver.position.y + despawnAboveDiver)
                {
                    Destroy(p.gameObject);
                    live.RemoveAt(i);
                }
            }
        }

        private void SpawnManifestNearFrontier(float frontier)
        {
            if (manifestSpawnPoints == null || manifestSpawned == null) return;
            if (manifestRegistry == null) return;
            var items = manifestRegistry.Items;
            if (items == null || items.Count == 0) return;

            int max = Mathf.Min(manifestSpawnPoints.Length, items.Count);
            for (int i = 0; i < max; i++)
            {
                if (manifestSpawned[i]) continue;
                var point = manifestSpawnPoints[i];
                if (point == null) continue;
                if (point.position.y > frontier) continue;

                var data = items[i];
                if (data == null) { manifestSpawned[i] = true; continue; }
                var chosenPrefab = data.Prefab != null ? data.Prefab : manifestPrefab;
                if (chosenPrefab == null) { manifestSpawned[i] = true; continue; }

                var instance = Instantiate(chosenPrefab, point.position, Quaternion.identity);
                instance.Bind(data);
                live.Add(instance);
                manifestSpawned[i] = true;
            }
        }

        private void SpawnNonManifest(float y)
        {
            if (nonManifestPool == null || nonManifestPool.Length == 0) return;

            Vector3 pos = default;
            bool placed = false;
            for (int i = 0; i < maxRejectRetries; i++)
            {
                float x = Random.Range(-lateralRange, lateralRange);
                pos = new Vector3(x, y, tunnelZ);
                if (wallLayer == 0 || !Physics.CheckSphere(pos, wallCheckRadius, wallLayer))
                {
                    placed = true;
                    break;
                }
            }
            if (!placed) return;

            var data = nonManifestPool[Random.Range(0, nonManifestPool.Length)];
            if (data == null) return;
            var chosenPrefab = data.Prefab != null ? data.Prefab : nonManifestPrefab;
            if (chosenPrefab == null) return;
            var instance = Instantiate(chosenPrefab, pos, Quaternion.identity);
            instance.Bind(data);
            live.Add(instance);
        }
    }
}
