using UnityEngine;

namespace Scrapset.Editor
{
    [CreateAssetMenu(fileName = "GateMenuCategory.asset", menuName = "ScriptableObjects/GateMenuCategory")]
    public class GateMenuCategory : ScriptableObject
    {
        [SerializeField] string Category;
    }
}
