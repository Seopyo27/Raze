using MP1.Control;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D basicCursorTexture; 
    public Texture2D attackCursorTexture; 
    public Vector2 hotSpot = Vector2.zero; 
    public PlayerController _player;

    void Update()
    {
        if(_player.Input.Attack)
        {
            Cursor.SetCursor(attackCursorTexture, hotSpot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(basicCursorTexture, hotSpot, CursorMode.Auto);
        }
        
    }
}
