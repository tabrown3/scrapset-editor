using Scrapset.Editor;
using Scrapset.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Attach(GameObject gameObject, IGate gate, PortDirection portDirection, RenderSide renderSide = RenderSide.Right)
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

            var listView = layout.Q<ListView>("GateIOContextMenu__IOList");
            FillPortList(listView, gate, portDirection);

            root.Add(layout);
        }

        void FillPortList(ListView listView, IGate gate, PortDirection portDirection)
        {
            List<ParameterNameTypePair> parameterNameTypePairs;
            if (portDirection == PortDirection.Input)
            {
                parameterNameTypePairs = gate.InputParameters.Select(u => new ParameterNameTypePair() { Name = u.Key, Type = u.Value.Type }).ToList();
            } else if (portDirection == PortDirection.Output)
            {
                parameterNameTypePairs = gate.OutputParameters.Select(u => new ParameterNameTypePair() { Name = u.Key, Type = u.Value.Type }).ToList();
            } else
            {
                throw new Exception("Could not generate parameter list: portDirection must be Input or Output");
            }

            listView.itemsSource = parameterNameTypePairs;
            listView.makeItem = () => new Button();
            listView.bindItem = (elem, ind) =>
            {
                var button = elem as Button;
                var item = parameterNameTypePairs[ind];
                button.text = $"{item.Name}: {item.Type}";
                button.clicked += () => { Debug.Log("Clicked!"); };
            };
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
