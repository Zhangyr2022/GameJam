using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public GameObject EnemyInstance;

    public int MaxEnemyCount = 3;

    public float GenerationInterval = 1;

    private float _lastGen = 0;

    private Dictionary<string, GameObject> _enemies = new Dictionary<string, GameObject>();

    public void DoAOEDamage(Vector3 center, float r, float damage)
    {
        foreach (var kv in _enemies)
        {
            GameObject enemy = kv.Value;
            Vector3 enemyPos = enemy.transform.position;
            if (Vector3.Distance(center, enemyPos) <= r)
                enemy.GetComponent<Enemy>().DoDamage(damage);
        }
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (_enemies.ContainsKey(enemy.name))
        {
            _enemies.Remove(enemy.name);

        }
    }

    private void GenerateEnemy()
    {
        List<Vector2Int> positions = GridManager.Instance.GetEvilChunks();

        if (positions.Count == 0)
            return;

        int rand = Random.Range(0, positions.Count);

        Vector2Int pos = positions[rand];

        GameObject enemy = GameObject.Instantiate(EnemyInstance, new Vector3(pos.x + 0.5f, 0.9f, pos.y + 0.5f), Quaternion.identity);
        enemy.name = "Enemy" + _enemies.Count.ToString();

        _enemies.Add(enemy.name, enemy);
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        _lastGen = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        float curTime = Time.time;
        if (_enemies.Count < MaxEnemyCount && curTime - _lastGen >= GenerationInterval)
        {
            GenerateEnemy();
            _lastGen = curTime;
        }
    }
}
