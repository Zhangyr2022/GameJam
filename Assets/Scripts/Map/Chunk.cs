using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int MaxHP = 5;

    public bool Evil = false;

    private Color HealthyColor = new Color(0.5090603f, 0.7264151f, 0.4488696f);

    private Color CorruptedColor = new Color(0.8980392f, 0.8290527f, 0.3843137f);

    private int _hp;

    private Material _mat;

    public void DoDamage(int damage)
    {
        if (!Evil)
        {
            _hp = Mathf.Max(_hp - damage, 0);
            Evil = _hp == 0;
            float r = Mathf.Lerp(CorruptedColor.r, HealthyColor.r, _hp * 1.0f / MaxHP);
            float g = Mathf.Lerp(CorruptedColor.g, HealthyColor.g, _hp * 1.0f / MaxHP);
            float b = Mathf.Lerp(CorruptedColor.b, HealthyColor.b, _hp * 1.0f / MaxHP);
            _mat.color = new Color(r, g, b);
        }
    }

    public void Recover()
    {
        if (Evil)
        {
            Evil = false;
            _hp = MaxHP;
            _mat.color = CorruptedColor;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _hp = MaxHP;
        _mat = gameObject.GetComponent<MeshRenderer>().materials[1];
        _mat.color = Evil ? CorruptedColor : HealthyColor;

        Vector3 chunkPos = transform.position;
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(chunkPos.x - 0.5f), Mathf.RoundToInt(chunkPos.z - 0.5f));
        GridManager.Instance.AddChunk(pos, this);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
