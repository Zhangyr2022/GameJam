using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{

    public static ItemManager Instance;

    public GameObject ItemInstance;

    public int MaxItemCount;

    public float GenerationInterval;

    private int _id;

    private float _lastGen = 0;

    private Dictionary<string, GameObject> _items = new Dictionary<string, GameObject>();

    // public void GenerateItems() { }

    public void RemoveItem(GameObject item)
    {
        if (_items.ContainsKey(item.name))
            _items.Remove(item.name);

        _lastGen = Time.time;
    }

    public void Reset()
    {
        _lastGen = Time.time;
        foreach (var kv in _items)
            GameObject.Destroy(kv.Value);

        _items.Clear();
    }

    private void Generate()
    {
        List<Vector2Int> positions = GridManager.Instance.GetValidChunks(false, new Dictionary<Vector2Int, int>());
        if (positions.Count == 0)
            return;
        int rand = Random.Range(0, positions.Count);
        Vector2Int pos = positions[rand];

        GameObject item = GameObject.Instantiate(ItemInstance, new Vector3(pos.x + 0.5f, 0.7f, pos.y + 0.5f), Quaternion.identity);
        item.name = "Item" + _id++;

        _items.Add(item.name, item);
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
        if (_items.Count < MaxItemCount && curTime - _lastGen >= GenerationInterval)
        {
            Generate();
            _lastGen = curTime;
        }
    }
}
