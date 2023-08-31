using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapset.Editor
{
    // WorldCursor is a game object that follows the UI cursor. Its job is to fire off events
    //  when the cursor is "over" a world object. Because it's always underneath the cursor,
    //  when the cursor hovers over something, the WorldCursor collides with it and triggers.
    public class WorldCursor : MonoBehaviour
    {
        public event Action<GameObject> OnEnter;
        public event Action<GameObject> OnExit;

        [SerializeField] InputManager inputManager;

        void Start()
        {
            // subscribe to InputManager's cursor movement event
            inputManager.OnCursorMove += OnCursorMove;
        }

        void OnCursorMove(InputValue value)
        {
            // force the WorldCursor to stick with the UI cursor
            transform.position = inputManager.CursorPosWorld;
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            // layer 2 is "Ignore Raycast" - if raycast ignores it, I want the hover behavior to ignore it
            if (collision.gameObject.layer != 2)
            {
                Debug.Log("Cursor over world object!!!");
                OnEnter?.Invoke(collision.gameObject);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer != 2)
            {
                Debug.Log("Cursor out of world object!!!");
                OnExit?.Invoke(collision.gameObject);
            }
        }
    }
}
