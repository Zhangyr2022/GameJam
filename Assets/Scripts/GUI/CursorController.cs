using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    // Û±Í—˘ Ω
    [SerializeField]
    Texture2D _mouseStyle;

    // Start is called before the first frame update
    void Start()
    {
        _mouseStyle = Resources.Load<Texture2D>("GUI/Cursor");
    }

    // Update is called once per frame
    void Update()
    {
        //if (SceneManager.GetActiveScene().name == "MainScene")

        Cursor.SetCursor(_mouseStyle, Vector2.zero, CursorMode.Auto);

    }

}
