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

    private bool _oriEvil;

    public void Reset()
    {
        _hp = MaxHP;
        Evil = _oriEvil;
        _mat.color = Evil ? CorruptedColor : HealthyColor;
    }

    public void DoDamage(int damage)
    {
        if (!Evil)
        {
            _hp = Mathf.Max(_hp - damage, 0);
            if (_hp == 0)
            {
                Evil = true;
                GridManager.Instance.UpdateEvilCnt(1);
            }
            _mat.color = Color.Lerp(CorruptedColor, HealthyColor, _hp * 1.0f / MaxHP);
        }
    }

    public void Recover()
    {
        _hp = MaxHP;
        _mat.color = HealthyColor;

        if (Evil)
        {
            Evil = false;
            GridManager.Instance.UpdateEvilCnt(-1);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        _hp = MaxHP;
        _oriEvil = Evil;
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
