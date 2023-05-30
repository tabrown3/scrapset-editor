using Scrapset.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Editor
{
    public class ScrapsetEditorController : MonoBehaviour
    {
        [SerializeField] SubroutineManager SubroutineManager;

        [SerializeField] GameObject SubroutineDefinitionPrefab;
        [SerializeField] GameObject EntrypointPrefab;
        [SerializeField] GameObject ExpressionPrefab;
        [SerializeField] GameObject SRInputPrefab;
        [SerializeField] GameObject SROutputPrefab;
        [SerializeField] GameObject StatementPrefab;
        [SerializeField] GameObject SubroutinePrefab;
        [SerializeField] GameObject VariablePrefab;

        Dictionary<string, GameObject> srGameObjects = new Dictionary<string, GameObject>();
        SubroutineDefinition activeSRDefinition;

        void Start()
        {
            //Debug.Log($"AddGate qualified assembly: {typeof(AddGate).AssemblyQualifiedName}");
            System.Type.GetType("Scrapset.Engine.AddGate");
            Debug.Log($"AddGate qualified assembly: {System.Type.GetType("Scrapset.Engine.AddGate").AssemblyQualifiedName}");
            var mainName = "Main";
            if (!SubroutineManager.HasDefinition(mainName))
            {
                activeSRDefinition = SubroutineManager.DeclareSubroutine(mainName);
                SetActiveSubroutine(mainName);
                CreateSubroutineRef(mainName);
                CreateGateRef(activeSRDefinition.EntrypointId);
            }
        }

        private void SetActiveSubroutine(string name)
        {
            if (!SubroutineManager.HasDefinition(name))
            {
                throw new System.Exception($"Unable to set active subroutine: '{name}' does not exist");
            }

            activeSRDefinition = SubroutineManager.GetDefinition(name);
        }

        void CreateSubroutineRef(string name)
        {
            var gameObject = Instantiate(SubroutineDefinitionPrefab);
            gameObject.name = name;
            gameObject.transform.SetParent(transform);

            var subroutineRef = gameObject.GetComponent<SubroutineRef>();
            subroutineRef.SubroutineName = name;

            srGameObjects.Add(name, gameObject);
        }

        void CreateGateRef(int id)
        {
            if (activeSRDefinition == null)
            {
                throw new System.Exception($"Could not create GateRef for Gate ID {id}: no subroutine is active");
            }

            var gate = activeSRDefinition.GetGateById(id);

            var gameObject = Instantiate(GetGatePrefab(gate.Category));
            gameObject.name = gate.Name;
            var subroutineRef = srGameObjects[activeSRDefinition.Name];
            gameObject.transform.SetParent(subroutineRef.transform);

            var gateRef = gameObject.GetComponent<GateRef>();
            gateRef.SubroutineName = activeSRDefinition.Name;
            gateRef.GateId = id;
        }

        GameObject GetGatePrefab(LanguageCategory category)
        {
            switch (category)
            {
                case LanguageCategory.Entrypoint: return EntrypointPrefab;
                case LanguageCategory.Expression: return ExpressionPrefab;
                case LanguageCategory.SRInput: return SRInputPrefab;
                case LanguageCategory.SROutput: return SROutputPrefab;
                case LanguageCategory.Statement: return StatementPrefab;
                case LanguageCategory.Subroutine: return SubroutinePrefab;
                case LanguageCategory.Variable: return VariablePrefab;
                default: throw new System.Exception($"Could not find prefab for gate language category: category {category} is not recognized");
            }
        }
    }
}
