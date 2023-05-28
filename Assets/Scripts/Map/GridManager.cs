using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    private Dictionary<Vector2Int, Chunk> _grid = new Dictionary<Vector2Int, Chunk>();

    private int _evilCnt = 0;

    private Material _skybox;

    public void AddChunk(Vector2Int pos, Chunk chunk)
    {
        _grid.Add(pos, chunk);
        if (chunk.Evil)
            UpdateEvilCnt(1);
    }

    public void CorruptChunk(Vector2Int pos, int damage)
    {
        if (_grid.ContainsKey(pos))
            _grid[pos].DoDamage(damage);
    }

    public void CleanChunk(Vector2Int pos)
    {
        if (_grid.ContainsKey(pos))
            _grid[pos].Recover();
    }

    public void UpdateEvilCnt(int cnt)
    {
        _evilCnt += cnt;
        float frac = 1.0f * _evilCnt / _grid.Count;

        if (_evilCnt == 0 || (frac <= 0.4 && EnemyManager.Instance.CurWave >= 8))
            Win();

        ChangeSkybox(frac);
    }

    public float GetEvilRatio()
    {
        return _evilCnt * 1.0f / _grid.Count;
    }

    public List<Vector2Int> GetValidChunks(bool evil, Dictionary<Vector2Int, int> occupied)
    {
        var ret = new List<Vector2Int>();

        foreach (var kv in _grid)
        {
            if ((kv.Value.Evil == evil) && !occupied.ContainsKey(kv.Key))
                ret.Add(kv.Key);
        }

        return ret;
    }

    private void Win()
    {
        EnemyManager.Instance.StopAll();
        Game.Instance.ChangeGameState(Game.GameState.HappyEnding);
    }

    private void ChangeSkybox(float factor)
    {
        _skybox.SetFloat("_Factor", factor);
    }


    private void Awake()
    {
        Instance = this;
        _skybox = RenderSettings.skybox;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
