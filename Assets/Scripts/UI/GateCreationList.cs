using Scrapset.Editor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public class GateCreationList : MonoBehaviour
    {
        [SerializeField] UIDocument gateMenu;

        public event Action<GateMenuItem> OnClick;

        void OnEnable()
        {
            var root = gateMenu.rootVisualElement;
            var listView = root.Q<ListView>("ListView");

            // TODO: pull this into resource management class
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
