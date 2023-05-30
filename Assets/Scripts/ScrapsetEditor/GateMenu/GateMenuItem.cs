using UnityEngine;

namespace Scrapset.Editor
{
    [CreateAssetMenu(fileName = "GateMenuItem.asset", menuName = "ScriptableObjects/GateMenuItem")]
    public class GateMenuItem : ScriptableObject
    {
        [SerializeField] string Name;
        [SerializeField] string Description;
        [SerializeField] string Category;
        [SerializeField] string AssemblyQualifiedName;
        [SerializeField] string PrefabPath;
    }
}
