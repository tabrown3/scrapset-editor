using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapset.Editor
{
    public class WorldCursor : MonoBehaviour
    {
        [SerializeField] InputManager inputManager;

        void Start()
        {
            inputManager.OnCursorMove += OnCursorMove;
        }

        void OnCursorMove(InputValue value)
        {
            transform.position = inputManager.CursorPosWorld;
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            // layer 2 is "Ignore Raycast" - if raycast ignores it, I want the hover behavior to ignore it
            if (collision.gameObject.layer != 2)
            {
                Debug.Log("Cursor over world object!!!");
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer != 2)
            {
                Debug.Log("Cursor out of world object!!!");
            }
        }
    }
}
