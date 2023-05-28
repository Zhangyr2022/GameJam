using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Player.Instance.gameObject)
        {
            ItemManager.Instance.RemoveItem(gameObject);
            GameObject.Destroy(gameObject);
        }
    }
}
