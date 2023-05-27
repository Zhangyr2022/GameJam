using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int MaxHP = 5;

    public bool Evil = true;

    private int _hp;

    private Material _mat;

    public void DoDamage(int damage)
    {
        if (!Evil)
        {
            _hp = Mathf.Max(_hp - damage, 0);
            Evil = _hp == 0;
            float r = Mathf.Lerp(1, 0, _hp * 1.0f / MaxHP);
            float g = Mathf.Lerp(0, 1, _hp * 1.0f / MaxHP);
            _mat.color = new Color(r, g, 0);
        }
    }

    public void Recover()
    {
        if (Evil)
        {
            Evil = false;
            _hp = MaxHP;
            _mat.color = Color.green;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _hp = MaxHP;
        _mat = gameObject.GetComponent<MeshRenderer>().material;
        _mat.color = Evil ? Color.red : Color.green;

        Vector3 chunkPos = transform.position;
        Vector2Int pos = new Vector2Int((int)(chunkPos.x - 0.5), (int)(chunkPos.z - 0.5));
        GridManager.Instance.AddChunk(pos, this);
    }

    // Update is called once per frame
    void Update() { }
}
