
using Scrapset.Editor;
using Scrapset.Engine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scrapset.UI
{
    public class ProgramFlowContextMenu : MonoBehaviour, IContextMenu
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
            IStatement statement = gate as IStatement;
            if (statement == null)
            {
                throw new Exception("Cannot attach Program Flow context menu to a non-statement gate: this is probably an editor bug");
            }

            List<string> programFlowNames;
            if (portDirection == PortDirection.Input)
            {
                programFlowNames = new List<string>() { "In" };
            } else if (portDirection == PortDirection.Output)
            {
                programFlowNames = new List<string>(statement.OutwardPaths);
            } else
            {
                throw new Exception($"Could not determine port direction: PortDirection {portDirection} is invalid");
            }

            listView.itemsSource = programFlowNames;
            listView.makeItem = () => new Button();
            listView.bindItem = (elem, ind) =>
            {
                var button = elem as Button;
                var item = programFlowNames[ind];
                button.text = item;
                button.clicked += () => { Debug.Log("Clicked!"); };
            };
        }
    }
}
