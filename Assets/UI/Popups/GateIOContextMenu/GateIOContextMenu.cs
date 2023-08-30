using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public enum RenderSide
    {
        Right,
        Left
    }

    public class GateIOContextMenu : MonoBehaviour
    {
        [SerializeField] UIDocument scrapsetEditorUI;
        [SerializeField] Camera cam;

        VisualElement layout;

        public void Attach(GameObject gameObject, RenderSide renderSide = RenderSide.Right)
        {
            if (layout != null) return;

            var root = scrapsetEditorUI.rootVisualElement;

            // LAYOUT
            Resources.Load<VisualTreeAsset>("GateIOContextMenuLayout").CloneTree(root); // clones asset and attaches to root
            layout = root.Q("GateIOContextMenu__Container");

            // STYLES
            var styles = Resources.Load<StyleSheet>("GateIOContextMenuStyles");
            layout.styleSheets.Add(styles);
            var screenPos = cam.WorldToScreenPoint(gameObject.transform.position);

            // (0,0) in screen space is the bottom left corner. But we want to set top, not bottom,
            //  so subtracting screenPos.y from height.
            layout.style.top = root.resolvedStyle.height - screenPos.y;

            if (renderSide == RenderSide.Right)
            {
                layout.style.left = screenPos.x;
            } else if (renderSide == RenderSide.Left)
            {
                layout.style.right = root.resolvedStyle.width - screenPos.x;
            }

            root.Add(layout);
        }

        public void Detach()
        {
            if (layout == null) return;

            var root = scrapsetEditorUI.rootVisualElement;
            root.Remove(layout);
            layout = null;
        }
    }
}
