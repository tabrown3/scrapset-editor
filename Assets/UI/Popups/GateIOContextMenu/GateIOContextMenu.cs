using Scrapset.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public class GateIOContextMenu : MonoBehaviour
    {
        [SerializeField] UIDocument scrapsetEditorUI;
        [SerializeField] WorldCursor worldCursor;
        [SerializeField] Camera cam;

        void Start()
        {
            worldCursor.OnEnter += OnWorldCursorEnter;
            //worldCursor.OnExit += OnWorldCursorExit;
        }

        void OnWorldCursorEnter(GameObject gameObject)
        {
            var root = scrapsetEditorUI.rootVisualElement;

            // LAYOUT
            Resources.Load<VisualTreeAsset>("GateIOContextMenuLayout").CloneTree(root);
            var layout = root.Q("GateIOContextMenu__Container");

            // STYLES
            var styles = Resources.Load<StyleSheet>("GateIOContextMenuStyles");
            layout.styleSheets.Add(styles);
            var bob = cam.WorldToScreenPoint(gameObject.transform.position);

            layout.style.bottom = bob.y;
            layout.style.left = bob.x;

            root.Q("Overlay").Add(layout);
        }

        void OnWorldCursorExit(GameObject gameObject)
        {
            var root = scrapsetEditorUI.rootVisualElement;
            var layout = root.Q("GateIOContextMenu__Container");
            root.Q("Overlay").Remove(layout);
        }
    }
}
