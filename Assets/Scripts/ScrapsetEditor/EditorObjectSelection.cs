using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Editor
{
    // maintains a list of selected game objects and their Bounds
    public class EditorObjectSelection
    {
        public bool HasSelection
        {
            get
            {
                return gameObjects.Count > 0;
            }
        }

        List<GameObject> gameObjects = new List<GameObject>();
        Bounds bounds = new Bounds();

        public void Select(GameObject gameObject)
        {
            if (gameObjects.Contains(gameObject)) { return; }

            var collider = gameObject.transform.GetComponent<Collider2D>();
            if (gameObjects.Count == 0)
            {
                bounds = collider.bounds;
            } else
            {
                bounds.Encapsulate(collider.bounds);
            }

            gameObjects.Add(gameObject);
        }

        public void Deselect(GameObject gameObject)
        {
            gameObjects.Remove(gameObject);
            bounds = new Bounds();

            foreach (var obj in gameObjects)
            {
                var collider = obj.transform.GetComponent<Collider2D>();
                bounds.Encapsulate(collider.bounds);
            }
        }

        public void Clear()
        {
            gameObjects.Clear();
            bounds = new Bounds();
        }
    }
}
