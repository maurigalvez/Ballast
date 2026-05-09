using System.Collections.Generic;
using UnityEngine;

namespace Ballast.Gameplay
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private Transform diver;
        [SerializeField] private GameObject[] obstaclePrefabs;

        [Header("Spawn cadence")]
        [SerializeField] private float spawnAheadDistance = 25f;
        [SerializeField] private float verticalSpacingMin = 8f;
        [SerializeField] private float verticalSpacingMax = 16f;
        [SerializeField] private float lateralRange = 3.5f;
        [SerializeField] private float tunnelZ = 0f;

        [Header("Avoidance")]
        [SerializeField] private LayerMask blockingLayer;
        [SerializeField] private float clearanceRadius = 1.2f;
        [SerializeField, Min(1)] private int maxRejectRetries = 4;

        [Header("Despawn")]
        [SerializeField] private float despawnAboveDiver = 30f;

        private float nextSpawnY;
        private readonly List<GameObject> live = new();

        private void Start()
        {
            if (diver == null) return;
            nextSpawnY = diver.position.y - 8f;
        }

        private void Update()
        {
            if (diver == null) return;

            float frontier = diver.position.y - spawnAheadDistance;
            int safety = 16;
            while (nextSpawnY > frontier && safety-- > 0)
            {
                SpawnOne(nextSpawnY);
                nextSpawnY -= Random.Range(verticalSpacingMin, verticalSpacingMax);
            }

            for (int i = live.Count - 1; i >= 0; i--)
            {
                var go = live[i];
                if (go == null) { live.RemoveAt(i); continue; }
                if (go.transform.position.y > diver.position.y + despawnAboveDiver)
                {
                    Destroy(go);
                    live.RemoveAt(i);
                }
            }
        }

        private void SpawnOne(float y)
        {
            if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

            var prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            if (prefab == null) return;

            bool wallMounted = prefab.GetComponent<WallMounted>() != null;
            float inset = wallMounted ? prefab.GetComponent<WallMounted>().WallInset : 0f;

            Vector3 pos = default;
            Quaternion rot = Quaternion.identity;
            bool placed = false;
            for (int i = 0; i < maxRejectRetries; i++)
            {
                float x;
                if (wallMounted)
                {
                    bool rightSide = Random.value < 0.5f;
                    x = rightSide ? (lateralRange - inset) : -(lateralRange - inset);
                    rot = Quaternion.Euler(0f, rightSide ? -90f : 90f, 0f);
                }
                else
                {
                    x = Random.Range(-lateralRange, lateralRange);
                }
                pos = new Vector3(x, y, tunnelZ);
                if (blockingLayer == 0 || !Physics.CheckSphere(pos, clearanceRadius, blockingLayer))
                {
                    placed = true;
                    break;
                }
            }
            if (!placed) return;

            var instance = Instantiate(prefab, pos, rot);
            live.Add(instance);
        }
    }
}
