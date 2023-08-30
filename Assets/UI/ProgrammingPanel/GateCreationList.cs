using Scrapset.Editor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public class GateCreationList : MonoBehaviour
    {
        [SerializeField] UIDocument scrapsetEditorUI;

        public event Action<GateMenuItem> OnClick;

        void OnEnable()
        {
            var root = scrapsetEditorUI.rootVisualElement;
            var listView = root.Q<ListView>("GateList");

            // TODO: pull this into resource management class
            // pulls all GateMenuItems (ScriptableObjects) from the directory
            var gateMenuItems = Resources.LoadAll("ScriptableObjects/GateMenu/GateMenuItems");

            listView.itemsSource = gateMenuItems;
            listView.makeItem = () => new Button();
            listView.bindItem = (elem, ind) =>
            {
                var button = elem as Button;
                var gateMenuItem = gateMenuItems[ind] as GateMenuItem;
                button.text = gateMenuItem.Name;
                button.clicked += () => { OnClick.Invoke(gateMenuItem); };
            };
        }
    }
}
