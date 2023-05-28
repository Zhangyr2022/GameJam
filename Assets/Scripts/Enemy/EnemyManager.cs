using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public GameObject EnemyInstance;

    public int WaveCount = 9;

    public float OccupyR = 0.4f;

    // public int MaxEnemyCount = 3;

    // public float GenerationInterval = 1;

    public int CurWave = 0;

    private int _id;

    private int _toGenerateCount = 0;

    private Dictionary<string, GameObject> _enemies = new Dictionary<string, GameObject>();

    private Dictionary<Vector2Int, int> _occupied = new Dictionary<Vector2Int, int>();

    public void DoAOEDamage(Vector3 center, float r, float damage)
    {
        List<GameObject> affected = new List<GameObject>();
        foreach (var kv in _enemies)
        {
            GameObject enemy = kv.Value;
            Vector3 enemyPos = enemy.transform.position;
            if (Vector3.Distance(center, enemyPos) <= r)
                affected.Add(enemy);

        }

        foreach (var enemy in affected)
            enemy.GetComponent<Enemy>().DoDamage(damage);
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (_enemies.ContainsKey(enemy.name))
        {
            _enemies.Remove(enemy.name);
            if (_enemies.Count == 0)
                NextWave();
        }
    }

    public void StopAll()
    {
        Debug.Log("STOP ALL");
        _toGenerateCount = 0;
        foreach (var kv in _enemies)
            GameObject.Destroy(kv.Value);

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach (var bullet in bullets)
            GameObject.Destroy(bullet);
    }

    private void Win()
    {
        StopAll();
        Game.Instance.ChangeGameState(Game.GameState.HappyEnding);
    }

    private void NextWave()
    {
        if (CurWave == WaveCount || (CurWave >= 7 && GridManager.Instance.GetEvilRatio() <= 0.4))
        {
            Win();
            return;
        }

        CurWave++;
        _toGenerateCount = CurWave;
    }

    private void GenerateEnemy()
    {
        List<Vector2Int> positions = GridManager.Instance.GetValidChunks(true, _occupied);

        if (positions.Count == 0)
            return;

        Dictionary<Vector2Int, bool> used = new Dictionary<Vector2Int, bool>();

        int maxTry = _toGenerateCount * 5;
        for (int i = 0; i < maxTry; i++)
        {
            int rand = Random.Range(0, positions.Count);
            Vector2Int pos = positions[rand];
            if (used.ContainsKey(pos))
                continue;
            used.Add(pos, true);

            GameObject enemy = GameObject.Instantiate(EnemyInstance, new Vector3(pos.x + 0.5f, 0.9f, pos.y + 0.5f), Quaternion.identity);
            enemy.layer = LayerMask.NameToLayer("Enemy");
            enemy.name = "Enemy" + _id++;
            _enemies.Add(enemy.name, enemy);
            _toGenerateCount--;

            if (_toGenerateCount == 0)
                break;
        }
    }

    private void MarkAsOccupied(Vector3 pos)
    {
        Vector2Int p1 = new Vector2Int((int)(pos.x - OccupyR), (int)(pos.z - OccupyR));
        Vector2Int p2 = new Vector2Int((int)(pos.x - OccupyR), (int)(pos.z + OccupyR));
        Vector2Int p3 = new Vector2Int((int)(pos.x + OccupyR), (int)(pos.z - OccupyR));
        Vector2Int p4 = new Vector2Int((int)(pos.x + OccupyR), (int)(pos.z + OccupyR));

        _occupied.TryAdd(p1, 1);
        _occupied.TryAdd(p2, 1);
        _occupied.TryAdd(p3, 1);
        _occupied.TryAdd(p4, 1);
    }

    private void GetOccupied()
    {
        _occupied.Clear();
        foreach (var enemy in _enemies)
            MarkAsOccupied(enemy.Value.transform.position);

        MarkAsOccupied(Player.Instance.transform.position);
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        CurWave = 0;
        NextWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (_toGenerateCount > 0)
        {
            GetOccupied();
            GenerateEnemy();
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            // StopAll();
            Debug.Log("BOOM");
            DoAOEDamage(Vector3.zero, 100, 100);
        }
    }
}
