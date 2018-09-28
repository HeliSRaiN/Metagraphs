﻿using System.Collections.Generic;
using UnityEngine;

namespace nm
{
    public class PredicateModule : MonoBehaviour
    {
        private static PredicateModule init;
        private StructureModule structureM;

        private void Awake()
        {
            init = this;
        }

        private void Start()
        {
            structureM = StructureModule.GetInit();
        }

        public static PredicateModule GetInit()
        {
            return init;
        }

        //Система даёт индивидуальный индекс имени.
        public static class NameSystem
        {
            public static Dictionary<string, int> nameDict = new Dictionary<string, int>();

            public static string GetName(string type)
            {
                string name = "error";
                if (nameDict.ContainsKey(type))
                    nameDict[type]++;
                else
                    nameDict.Add(type, 1);

                name = type + nameDict[type];
                return name;
            }

            public static void RemoveLastIndex(string type)
            {
                nameDict[type]--;
            }
        }

        public void BuildGraphs()
        {
            foreach(var part in structureM.structure)
            {
                TactBuild(part.Key, part.Value.ObjectType);
            }
        }

        public void TactBuild(string name, string objectType)
        {
            switch (objectType)
            {
                case "Vertex":
                case "Metavertex":
                case "Graph":
                case "Metagraph":
                    new VertexGraph(name, ref structureM.structure);
                    break;
                case "Edge":
                case "Metaedge":
                    new Edge(name, ref structureM.structure);
                    break;
                case "Attribute":
                    new Attribute(name, ref structureM.structure);
                    break;
            }
        }

        /// <summary>
        /// Метаграф/граф. Вершина/Метавершина.
        /// </summary>
        public class VertexGraph
        {
            public string Name { get; set; }
            private Structure m_structure;

            public VertexGraph(string name, ref Dictionary<string, Structure> structure)
            {
                Name = name;
                m_structure = structure[Name];
                Create();
                OutlogModule outlogM = OutlogModule.GetInit();
                outlogM.ConsoleLog(Name, ref structure, "Vertex");
                outlogM.OutTooltip();
            }
            public void Create()
            {
                Vector3 position = m_structure.GetPosition();
                // Если альфа канал 0, то цвет не установлен. TO DO
                if (m_structure.color.a == 0)
                {
                    SetColor(new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255));
                }

                if (m_structure.ChildStructures != null && m_structure.ChildStructures.Count >= 1)
                {
                    // Вершина-связь.
                    foreach (var part in m_structure.ChildStructures)
                    {
                        // Не делаем связь с Edge и MetaEdge
                        if (part.Value.ObjectType == "Edge" || part.Value.ObjectType == "Metaedge") continue;
                        Vector3 childPosition = part.Value.GetPosition();
                        m_structure.gameObject.AddRange(InitObject.Instance.InitLine(true, position, childPosition, m_structure.color, Name));
                    }
                }
                else
                {
                    // Вершина-сфера.
                    m_structure.gameObject.Add(InitObject.Instance.InitGraph(position, m_structure.color, Name));
                }
            }
            public void SetColor(Color32 colorNew)
            {
                m_structure.color = colorNew;
            }
        }

        /// <summary>
        /// Ребро/Метаребро.
        /// Ребро должно содержать в себе две любые вершины. Без них ребро не ребро.
        /// Ребро может быть направленным (eo=true), от указанного StartVertex до EndVertex. 
        /// </summary>
        public class Edge
        {
            public string Name { get; set; }
            private Dictionary<string, Structure> m_structureDict;
            private Structure m_structure;

            public Edge(string name, ref Dictionary<string, Structure> structure)
            {
                Name = name;
                m_structureDict = structure;
                m_structure = structure[Name];
                Create();
                OutlogModule outlogM = OutlogModule.GetInit();
                outlogM.ConsoleLog(Name, ref structure, "Edge");
                outlogM.OutTooltip();
            }
            public void Create()
            {
                // Проверка через одно место. Если альфа канал 0, то цвет не установлен. TO DO
                if (m_structure.color.a == 0)
                {
                    SetColor(new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255));
                }

                Vector3 firstPosition = Vector3.zero;
                Vector3 secondPosition = Vector3.zero;

                if (m_structure.Start != null && m_structure.End != null)
                {
                    firstPosition = m_structureDict[m_structure.Start].GetPosition();
                    secondPosition = m_structureDict[m_structure.End].GetPosition();
                }
                else
                {
                    if (m_structure.ChildStructures.Count == 2)
                    {
                        int k = 0;
                        foreach (var part in m_structure.ChildStructures)
                        {
                            if (k == 0)
                            {
                                firstPosition = part.Value.GetPosition();
                            }
                            if (k == 1)
                            {
                                secondPosition = part.Value.GetPosition();
                            }
                            k++;
                        }
                    }
                }
                m_structure.gameObject.AddRange(InitObject.Instance.InitLine(false, firstPosition, secondPosition, m_structure.color, Name));
            }
            public void SetColor(Color32 colorNew)
            {
                m_structure.color = colorNew;
            }
        }

        /// <summary>
        /// Атрибут.
        /// </summary>
        public class Attribute
        {
            public string Name { get; set; }
            public Attribute(string name, ref Dictionary<string, Structure> structure)
            {
                Name = name;
                OutlogModule outlogM = OutlogModule.GetInit();
                outlogM.ConsoleLog(Name, ref structure, "Attribute");
            }
        }
    }
}