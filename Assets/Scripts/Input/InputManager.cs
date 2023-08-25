using Scrapset.Examples;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scrapset
{
    public class InputManager : MonoBehaviour
    {
        public Vector2 PanDirection { get; private set; }
        public float ZoomDelta { get; private set; }
        public Vector2 CursorPosScreen { get; private set; }
        public Vector3 CursorPosWorld { get; private set; }
        public bool IsPanningByDrag { get; private set; }
        public Vector3 InitDragPosWorld { get; private set; }

        public event Action<InputValue> OnUIClick;
        public event Action<InputValue> OnWorldClick;

        [SerializeField] Camera cam;

        UIInputManager uiInputManager;
        bool prevIsPanningByDrag;

        void Start()
        {
            uiInputManager = transform.GetComponent<UIInputManager>();
        }

        void OnPrimaryAction(InputValue value)
        {
            if (uiInputManager.IsPointerOverUI)
            {
                OnUIClick?.Invoke(value);
            } else
            {
                OnWorldClick?.Invoke(value);
            }
        }

        void OnPan(InputValue value)
        {
            PanDirection = value.Get<Vector2>();
        }

        void OnZoom(InputValue value)
        {
            ZoomDelta = uiInputManager.IsPointerOverUI ? 0 : value.Get<Vector2>().y;
        }

        void OnCursorPosition(InputValue value)
        {
            var screenPos = value.Get<Vector2>();
            CursorPosScreen = screenPos;
            CursorPosWorld = cam.ScreenToWorldPoint(screenPos);
        }

        void OnActivatePanByDrag(InputValue value)
        {
            IsPanningByDrag = value.Get<float>() != 0f;
            if (IsPanningByDrag && !prevIsPanningByDrag)
            {
                InitDragPosWorld = CursorPosWorld;
            }
            prevIsPanningByDrag = IsPanningByDrag;
        }

        void OnBuild()
        {
            IProgram program = ExamplePrograms.Factorial;
            program.Build();
        }

        void OnRun()
        {
            IProgram program = ExamplePrograms.Factorial;
            var returnValues = program.Run();

            Debug.Log("Below are the subroutine's return values:");
            foreach (var kv in returnValues)
            {
                Debug.Log($"Identifier: '{kv.Key}', Value: {kv.Value.Value}, Type: {kv.Value.Type}");
            }
        }
    }
}
