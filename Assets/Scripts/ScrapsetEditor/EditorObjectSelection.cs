using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapset.Editor
{
    // maintains a list of selected game objects and their Bounds
    public class EditorObjectSelection : MonoBehaviour
    {
        [SerializeField] InputManager inputManager;
        [SerializeField] Camera cam;
        [SerializeField] GameObject selectionFill;
        [SerializeField] GameObject selectionOutline;

        public bool HasSelection
        {
            get
            {
                return gameObjects.Count > 0;
            }
        }

        List<SelectedEditorObject> gameObjects = new List<SelectedEditorObject>();
        Bounds bounds = new Bounds();
        bool isSelecting = false;
        Vector2 selectionStartPos;
        Vector2 selectionEndPos;
        SpriteRenderer selectionFillSpriteRenderer;
        SelectionDetection selectionFillSelectionDetection;

        public void Select(GameObject gameObject)
        {
            if (gameObjects.Find(u => u.SelectedObject == gameObject) != null) { return; }

            var collider = gameObject.transform.GetComponent<Collider2D>();
            if (gameObjects.Count == 0)
            {
                bounds = collider.bounds;
            } else
            {
                bounds.Encapsulate(collider.bounds);
            }

            var outline = Instantiate(selectionOutline, gameObject.transform, false);
            gameObjects.Add(new SelectedEditorObject() { SelectedObject = gameObject, SelectionOutline = outline });
        }

        public void Deselect(GameObject gameObject)
        {
            var index = gameObjects.FindIndex(u => u.SelectedObject == gameObject);
            Destroy(gameObjects[index].SelectionOutline);
            gameObjects.RemoveAt(index);

            bounds = new Bounds();
            foreach (var obj in gameObjects)
            {
                var editorObject = obj.SelectedObject;
                var collider = editorObject.transform.GetComponent<Collider2D>();
                bounds.Encapsulate(collider.bounds);
            }
        }

        public void Clear()
        {
            foreach (var obj in gameObjects)
            {
                // destroy all selection indicator game objects
                Destroy(obj.SelectionOutline);
            }

            gameObjects.Clear();
            bounds = new Bounds();
        }

        void Start()
        {
            // TODO: combine OnUIClick and OnWorldClick and pass in an object with flag that indicates UI or not
            inputManager.OnUIClick += OnUIClick;
            inputManager.OnWorldClick += OnWorldClick;
            inputManager.OnCursorMove += OnCursorMove;

            selectionFillSpriteRenderer = selectionFill.GetComponent<SpriteRenderer>();
            selectionFillSelectionDetection = selectionFill.GetComponent<SelectionDetection>();

            selectionFillSelectionDetection.OnGateTouched += OnGateTouched;
            selectionFillSelectionDetection.OnGateUntouched += OnGateUntouched;
        }

        // executes when a PrimaryAction event (left mouse click, etc) occurs with the cursor over the UI
        //  (as opposed to the world)
        void OnUIClick(InputValue value)
        {
            Debug.Log("Clicked UI!!!");
            if (!value.isPressed)
            {
                ReleaseSelectionFill();
            }
        }

        // executes when a PrimaryAction event (left mouse click, etc) occurs with the cursor over the WORLD
        //  (as opposed to the UI)
        void OnWorldClick(InputValue value)
        {
            if (value.isPressed)
            {
                Debug.Log("Clicked WORLD!!!");
                // raycast to see what objects are under the cursor
                RaycastHit2D raycastHit = Physics2D.Raycast(
                    cam.ScreenToWorldPoint(new Vector3(inputManager.CursorPosScreen.x, inputManager.CursorPosScreen.y, 10)), Vector2.zero
                );

                // clicked an object (currently just GateRefs)
                if (raycastHit)
                {
                    var gameObject = raycastHit.transform.gameObject;
                    var gateRef = raycastHit.transform.GetComponent<GateRef>();

                    // if a gate was clicked, select it
                    if (gateRef != null)
                    {
                        Debug.Log("Selecting GateRef");
                        Select(gameObject);
                    }
                } else // clicked empty space
                {
                    // if player is clicking world, clear all current selections
                    Clear();

                    isSelecting = true;
                    selectionStartPos = inputManager.CursorPosWorld;
                    selectionEndPos = inputManager.CursorPosWorld;
                    selectionFill.transform.localScale = new Vector3(0, 0, 1);
                    selectionFillSpriteRenderer.enabled = true;
                }
            } else
            {
                ReleaseSelectionFill();
            }
        }

        void ReleaseSelectionFill()
        {
            isSelecting = false;
            selectionStartPos = new Vector2();
            selectionEndPos = new Vector2();
            selectionFillSpriteRenderer.enabled = false;
        }

        void OnCursorMove(InputValue value)
        {
            if (isSelecting)
            {
                // set blue selection fill position and scale
                selectionEndPos = inputManager.CursorPosWorld;
                var deltaX = selectionEndPos.x - selectionStartPos.x;
                var deltaY = selectionEndPos.y - selectionStartPos.y;
                selectionFill.transform.localScale = new Vector3(deltaX, deltaY, 1);
                selectionFill.transform.position = selectionStartPos + new Vector2(deltaX / 2, deltaY / 2);
            }
        }

        void OnGateTouched(GameObject gameObject)
        {
            if (isSelecting)
            {
                Select(gameObject);
            }
        }

        void OnGateUntouched(GameObject gameObject)
        {
            if (isSelecting)
            {
                Deselect(gameObject);
            }
        }
    }

    public class SelectedEditorObject
    {
        public GameObject SelectedObject { get; set; }
        public GameObject SelectionOutline { get; set; }
    }
}
