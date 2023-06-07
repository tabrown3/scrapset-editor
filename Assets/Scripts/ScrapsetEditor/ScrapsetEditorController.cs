using Scrapset.Engine;
using Scrapset.UI;
using System;
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
            var mainName = "Main";
            if (!SubroutineManager.HasDefinition(mainName))
            {
                activeSRDefinition = SubroutineManager.DeclareSubroutine(mainName);
                SetActiveSubroutine(mainName);
                CreateSubroutineRef(mainName);
                CreateGateRef(activeSRDefinition.EntrypointId);
            }

            // TODO: this is so dirty - clean it up
            var gateCreationList = FindObjectOfType<GateCreationList>();
            gateCreationList.OnClick += (gateMenuItem) =>
            {
                var gateTypeFromString = Type.GetType(gateMenuItem.AssemblyQualifiedName);
                var newGate = (IGate)Activator.CreateInstance(gateTypeFromString);
                var id = activeSRDefinition.RegisterGate(newGate);
                CreateGateRef(id);
            };
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

            // TODO: refactor to Strategy Pattern - strategy accepts the gate and returns an object
            //  that has an associated Prefab
            var gameObject = Instantiate(GetGatePrefab(gate));
            gameObject.name = gate.GetType().ToString(); // TODO: replace with ScriptableObject Name
            var subroutineRef = srGameObjects[activeSRDefinition.Identifier];
            gameObject.transform.SetParent(subroutineRef.transform);

            var gateRef = gameObject.GetComponent<GateRef>();
            gateRef.SubroutineName = activeSRDefinition.Identifier;
            gateRef.GateId = id;
        }

        GameObject GetGatePrefab(IGate gate)
        {
            if (gate.Id == activeSRDefinition.EntrypointId) return EntrypointPrefab;

            var variable = gate as IVariable;

            if (variable != null)
            {
                if (activeSRDefinition.InputParameters.ContainsKey(variable.Identifier)) return SRInputPrefab;
                if (activeSRDefinition.OutputParameters.ContainsKey(variable.Identifier)) return SROutputPrefab;
                return VariablePrefab;
            }

            var gateType = gate.GetType();

            if (gateType == typeof(SubroutineExpressionGate)) return SubroutinePrefab;
            if (typeof(IExpression).IsAssignableFrom(gateType)) return ExpressionPrefab;
            if (typeof(IStatement).IsAssignableFrom(gateType)) return StatementPrefab;

            throw new System.Exception($"Could not find prefab for gate: type {gateType} is not recognized");
        }
    }
}
