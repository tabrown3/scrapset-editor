using Scrapset.Engine;
using Scrapset.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scrapset.Editor
{
    public class ScrapsetEditorController : MonoBehaviour
    {
        [SerializeField] SubroutineManager subroutineManager;
        [SerializeField] Camera cam;

        [SerializeField] GameObject subroutineDefinitionPrefab;
        [SerializeField] GameObject entrypointPrefab;
        [SerializeField] GameObject expressionPrefab;
        [SerializeField] GameObject srInputPrefab;
        [SerializeField] GameObject srOutputPrefab;
        [SerializeField] GameObject statementPrefab;
        [SerializeField] GameObject subroutinePrefab;
        [SerializeField] GameObject variablePrefab;

        [SerializeField] GameObject programFlowInPrefab;
        [SerializeField] GameObject programFlowOutPrefab;
        [SerializeField] GameObject gateInputPrefab;
        [SerializeField] GameObject gateOutputPrefab;

        Dictionary<string, GameObject> srGameObjects = new Dictionary<string, GameObject>();
        SubroutineDefinition activeSRDefinition;

        void Start()
        {
            InitMainSubroutine();
            BindGateCreationHandler();
        }

        void InitMainSubroutine()
        {
            var mainName = "Main";
            if (!subroutineManager.HasDefinition(mainName))
            {
                activeSRDefinition = subroutineManager.DeclareSubroutine(mainName);
                SetActiveSubroutine(mainName);
                CreateSubroutineRef(mainName);
                CreateGateRef(activeSRDefinition.EntrypointId);
            }
        }

        void BindGateCreationHandler()
        {
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

        void SetActiveSubroutine(string name)
        {
            if (!subroutineManager.HasDefinition(name))
            {
                throw new System.Exception($"Unable to set active subroutine: '{name}' does not exist");
            }

            activeSRDefinition = subroutineManager.GetDefinition(name);
        }

        void CreateSubroutineRef(string name)
        {
            var gameObject = Instantiate(subroutineDefinitionPrefab);
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
            gameObject.transform.SetParent(subroutineRef.transform); // make the gate a child of the subroutine
            gameObject.transform.position = new Vector2(cam.transform.position.x, cam.transform.position.y); // set the new gate to the cam's position

            // if it has at least one input
            if (gate.InputParameters.Count > 0)
            {
                var gateInputRef = Instantiate(gateInputPrefab);
                gateInputRef.transform.SetParent(gameObject.transform);
                gateInputRef.transform.position = gameObject.transform.position + new Vector3(-0.7f, 0, 0);
            }

            // if it has at least one output
            if (gate.OutputParameters.Count > 0)
            {
                var gateOutputRef = Instantiate(gateOutputPrefab);
                gateOutputRef.transform.SetParent(gameObject.transform);
                gateOutputRef.transform.position = gameObject.transform.position + new Vector3(0.7f, 0, 0);
            }

            var statement = gate as IStatement;
            if (statement != null)
            {
                // the only gate without a program flow in is Entrypoint
                if (!(statement is Entrypoint))
                {
                    var programFlowInRef = Instantiate(programFlowInPrefab);
                    programFlowInRef.transform.SetParent(gameObject.transform);
                    programFlowInRef.transform.position = gameObject.transform.position + new Vector3(0, 0.75f, 0);
                }

                // A statement should always have at least one outward path, unless it's a subroutine return or endpoint or something,
                //  neither of which currently exist in the standard library
                if (statement.OutwardPaths.Count > 0)
                {
                    var programFlowOutRef = Instantiate(programFlowOutPrefab);
                    programFlowOutRef.transform.SetParent(gameObject.transform);
                    programFlowOutRef.transform.position = gameObject.transform.position + new Vector3(0, -0.75f, 0);
                }
            }

            var gateRef = gameObject.GetComponent<GateRef>();
            gateRef.SubroutineName = activeSRDefinition.Identifier;
            gateRef.GateId = id;
        }

        GameObject GetGatePrefab(IGate gate)
        {
            if (gate.Id == activeSRDefinition.EntrypointId) return entrypointPrefab;

            var variable = gate as IVariable;

            if (variable != null)
            {
                if (activeSRDefinition.InputParameters.ContainsKey(variable.Identifier)) return srInputPrefab;
                if (activeSRDefinition.OutputParameters.ContainsKey(variable.Identifier)) return srOutputPrefab;
                return variablePrefab;
            }

            var gateType = gate.GetType();

            if (gateType == typeof(SubroutineExpressionGate)) return subroutinePrefab;
            if (typeof(IExpression).IsAssignableFrom(gateType)) return expressionPrefab;
            if (typeof(IStatement).IsAssignableFrom(gateType)) return statementPrefab;

            throw new System.Exception($"Could not find prefab for gate: type {gateType} is not recognized");
        }
    }
}
