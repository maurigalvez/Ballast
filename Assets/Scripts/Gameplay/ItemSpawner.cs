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
        [SerializeField, Min(0)] private int totalManifestItems = 5;
        [SerializeField, Range(0f, 1f)] private float manifestBias = 0.4f;

        [Header("Wall avoidance")]
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private float wallCheckRadius = 0.8f;
        [SerializeField, Min(1)] private int maxRejectRetries = 4;

        [Header("Despawn")]
        [SerializeField] private float despawnAboveDiver = 30f;

        private float nextSpawnY;
        private int manifestSpawned;
        private readonly List<ItemPickup> live = new();

        private void Start()
        {
            if (diver == null) return;
            nextSpawnY = diver.position.y - 5f;
        }

        private void Update()
        {
            if (diver == null) return;
            if (GameManager.Instance != null && GameManager.Instance.State != RunState.Running) return;

            float frontier = diver.position.y - spawnAheadDistance;
            int safety = 16;
            while (nextSpawnY > frontier && safety-- > 0)
            {
                SpawnOne(nextSpawnY);
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

        private void SpawnOne(float y)
        {
            bool manifest = ShouldSpawnManifest();
            IReadOnlyList<ItemData> pool = manifest
                ? (manifestRegistry != null ? manifestRegistry.Items : null)
                : nonManifestPool;
            ItemPickup prefab = manifest ? manifestPrefab : nonManifestPrefab;
            if (pool == null || pool.Count == 0) return;

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

            var data = pool[Random.Range(0, pool.Count)];
            var chosenPrefab = data.Prefab != null ? data.Prefab : prefab;
            if (chosenPrefab == null) return;
            var instance = Instantiate(chosenPrefab, pos, Quaternion.identity);
            instance.Bind(data);
            live.Add(instance);
            if (manifest) manifestSpawned++;
        }

        private bool ShouldSpawnManifest()
        {
            bool manifestQuotaLeft = manifestSpawned < totalManifestItems;
            bool hasNonManifest = nonManifestPool != null && nonManifestPool.Length > 0 && PoolHasUsablePrefab(nonManifestPool, nonManifestPrefab);
            if (!manifestQuotaLeft) return false;
            if (!hasNonManifest) return true;
            return Random.value < manifestBias;
        }

        private static bool PoolHasUsablePrefab(ItemData[] pool, ItemPickup fallback)
        {
            if (fallback != null) return true;
            for (int i = 0; i < pool.Length; i++)
            {
                if (pool[i] != null && pool[i].Prefab != null) return true;
            }
            return false;
        }
    }
}
