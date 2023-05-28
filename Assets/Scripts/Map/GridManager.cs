using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    private Dictionary<Vector2Int, Chunk> _grid = new Dictionary<Vector2Int, Chunk>();

    public void AddChunk(Vector2Int pos, Chunk chunk)
    {
        _grid.Add(pos, chunk);
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


    private void Awake()
    {
        Instance = this;
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
