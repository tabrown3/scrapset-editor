using UnityEngine;

namespace Scrapset.Editor
{
    [CreateAssetMenu(fileName = "GateMenuItem.asset", menuName = "ScriptableObjects/GateMenuItem")]
    public class GateMenuItem : ScriptableObject
    {
        [SerializeField] public string Name;
        [SerializeField] public string Description;
        [SerializeField] public string Category;
        [SerializeField] public string AssemblyQualifiedName;
        [SerializeField] public string PrefabPath;
    }
}
