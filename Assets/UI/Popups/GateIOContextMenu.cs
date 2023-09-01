using Scrapset.Editor;
using Scrapset.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public class GateIOContextMenu : MonoBehaviour, IContextMenu
    {
        PopupMenuManager popupMenuManager;

        void Start()
        {
            popupMenuManager = GetComponent<PopupMenuManager>();
        }

        public void Attach(GameObject gameObject, IGate gate, PortDirection portDirection, Anchor anchor = Anchor.Right)
        {
            var listView = new ListView();
            FillPortList(listView, gate, portDirection);

            popupMenuManager.Attach(gameObject, listView, anchor);
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
    }
}
