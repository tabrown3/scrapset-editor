using Scrapset.UI;
using System;
using UnityEngine;

namespace Scrapset.Editor
{
    public enum PortDirection
    {
        Input,
        Output
    }

    public enum LinkType
    {
        InputOutput,
        ProgramFlow
    }

    public class GatePort : MonoBehaviour
    {
        [SerializeField] PortDirection portDirection;
        [SerializeField] Anchor anchor;
        [SerializeField] LinkType linkType;

        // TODO: create interface for GateIO and ProgramFlowContextMenu to share
        IContextMenu menu;
        WorldCursor worldCursor;
        GateRef gateRef;

        void Start()
        {
            if (linkType == LinkType.InputOutput)
            {
                menu = FindObjectOfType<GateIOContextMenu>();
            } else if (linkType == LinkType.ProgramFlow)
            {
                menu = FindObjectOfType<ProgramFlowContextMenu>();
            } else
            {
                throw new Exception($"Cannot attach context menu: invalid LinkType {linkType}");
            }


            worldCursor = FindObjectOfType<WorldCursor>();
            gateRef = transform.parent.GetComponent<GateRef>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (menu == null || worldCursor == null) return;
            if (collision.gameObject != worldCursor.gameObject) return; // only attach on WorldCursor collision

            menu.Attach(gameObject, gateRef.Gate, portDirection, anchor);
        }
    }
}
