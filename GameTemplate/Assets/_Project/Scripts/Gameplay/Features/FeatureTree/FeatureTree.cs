using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Features.FeatureTree
{
    [Serializable]
    public sealed class FeatureTree
    {
        [Serializable]
        private readonly struct SystemMetadata
        {
            [ShowInInspector] public readonly int Order;
            [ShowInInspector] public readonly SystemPauseMode PauseMode;
            [ShowInInspector] public readonly FeatureNode FeatureNode;

            public SystemMetadata(int order, SystemPauseMode pauseMode, FeatureNode featureNode)
            {
                Order = order;
                PauseMode = pauseMode;
                FeatureNode = featureNode;
            }

            public SystemMetadata WithPauseMode(SystemPauseMode pauseMode) => 
                new SystemMetadata(Order, pauseMode, FeatureNode);
        }
        
        [Serializable]
        private class SystemDebugInfo
        {
            [ShowInInspector, HorizontalGroup("Info"), LabelWidth(120)]
            [LabelText("System Type")]
            public string SystemTypeName;

            [ShowInInspector, HorizontalGroup("Info"), LabelWidth(80)]
            [LabelText("Pause Mode")]
            public SystemPauseMode PauseMode;

            [ShowInInspector, HorizontalGroup("Info"), LabelWidth(80)]
            [LabelText("Feature Path")]
            public string FeaturePath;

            [ShowInInspector, HorizontalGroup("Info"), LabelWidth(80)]
            [LabelText("Can Be Paused")]
            [GUIColor("GetPauseStateColor")]
            public bool CanBePaused;

            private Color GetPauseStateColor()
            {
                return CanBePaused ? new Color(0.4f, 0.8f, 0.4f) : new Color(0.8f, 0.4f, 0.4f);
            }
        }
        
        #if UNITY_EDITOR
        [FoldoutGroup("Debug"), ShowInInspector, ReadOnly, ListDrawerSettings(ShowPaging = false, ShowIndexLabels = false)]
        [PropertySpace(10), Title("Pausable Systems Debug View", "Shows which systems can be paused in the current configuration")]
        private List<SystemDebugInfo> _debugSystemsInfo = new List<SystemDebugInfo>();

        [FoldoutGroup("Debug"), Button("Refresh Debug View")]
        private void RefreshDebugView()
        {
            _debugSystemsInfo.Clear();
            
            for (int i = 0; i < _systems.length; i++)
            {
                ISystem system = _systems[i];
                Type systemType = system.GetType();
                
                if (_systemMetadata.TryGetValue(systemType, out SystemMetadata metadata))
                {
                    string featurePath = GetFeaturePath(metadata.FeatureNode);

                    bool canBePaused = metadata.PauseMode switch
                    {
                        SystemPauseMode.ALWAYS_PAUSABLE => true,
                        SystemPauseMode.NEVER_PAUSABLE => false,
                        _ => metadata.FeatureNode.IsFeaturePausable()
                    };

                    _debugSystemsInfo.Add(new SystemDebugInfo
                    {
                        SystemTypeName = systemType.Name,
                        PauseMode = metadata.PauseMode,
                        FeaturePath = featurePath,
                        CanBePaused = canBePaused
                    });
                }
            }
            
            _debugSystemsInfo.Sort(static (a, b) => 
            {
                int pauseComp = b.CanBePaused.CompareTo(a.CanBePaused);
                return pauseComp != 0 ? pauseComp : string.Compare(a.SystemTypeName, b.SystemTypeName, StringComparison.Ordinal);
            });
        }
        
        private string GetFeaturePath(FeatureNode node)
        {
            if (node == null) return "None";
            
            string path = node.Feature.GetType().Name;
            FeatureNode parent = node.Parent;
            
            while (parent != null)
            {
                path = parent.Feature.GetType().Name + " → " + path;
                parent = parent.Parent;
            }
            
            return path;
        }
        [FoldoutGroup("Debug"), Button("Print Feature Tree to Console", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        private void PrintFeatureTreeToConsole()
        {
            string treeStructure = GenerateFeatureTreeStructure();
            Debug.Log(treeStructure);
        }
        
        [FoldoutGroup("Debug"), Button("Copy Feature Tree to Clipboard", ButtonSizes.Medium)]
        [GUIColor(0.8f, 0.4f, 1f)]
        private void CopyFeatureTreeToClipboard()
        {
            string treeStructure = GenerateFeatureTreeStructure();
            GUIUtility.systemCopyBuffer = treeStructure;
            Debug.Log("Feature tree structure copied to clipboard!");
        }
        
        private string GenerateFeatureTreeStructure()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                        FEATURE TREE                         ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            
            HashSet<FeatureNode> rootFeatures = new HashSet<FeatureNode>();
            Dictionary<FeatureNode, List<FeatureNode>> featureHierarchy = new Dictionary<FeatureNode, List<FeatureNode>>();
            
            foreach (KeyValuePair<Type, SystemMetadata> kvp in _systemMetadata)
            {
                SystemMetadata metadata = kvp.Value;
                FeatureNode featureNode = metadata.FeatureNode;
                
                if (featureNode != null)
                {
                    FeatureNode root = featureNode;
                    while (root.Parent != null)
                        root = root.Parent;
                    
                    rootFeatures.Add(root);
                    
                    BuildHierarchy(featureNode, featureHierarchy);
                }
            }
            
            foreach (FeatureNode rootFeature in rootFeatures)
            {
                PrintFeatureNode(sb, rootFeature, "", true, featureHierarchy);
            }
            
            sb.AppendLine();
            sb.AppendLine("╔══════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                         STATISTICS                          ║");
            sb.AppendLine("╚══════════════════════════════════════════════════════════════╝");
            sb.AppendLine($"Total Features: {CountAllFeatures()}");
            sb.AppendLine($"Total Systems: {_systems.length}");
            sb.AppendLine($"Root Features: {rootFeatures.Count}");
            
            return sb.ToString();
        }
        
        private int CountAllFeatures()
        {
            HashSet<FeatureNode> allNodes = new HashSet<FeatureNode>();
            
            foreach (SystemMetadata metadata in _systemMetadata.Values)
            {
                if (metadata.FeatureNode != null)
                {
                    allNodes.Add(metadata.FeatureNode);
                    
                    FeatureNode current = metadata.FeatureNode.Parent;
                    while (current != null)
                    {
                        allNodes.Add(current);
                        current = current.Parent;
                    }
                }
            }
            
            return allNodes.Count;
        }
        
        private void BuildHierarchy(FeatureNode node, Dictionary<FeatureNode, List<FeatureNode>> hierarchy)
        {
            FeatureNode current = node;
            while (current != null)
            {
                if (current.Parent != null)
                {
                    if (!hierarchy.ContainsKey(current.Parent))
                        hierarchy[current.Parent] = new List<FeatureNode>();
                    
                    if (!hierarchy[current.Parent].Contains(current))
                        hierarchy[current.Parent].Add(current);
                }
                current = current.Parent;
            }
        }
        
        private void PrintFeatureNode(StringBuilder sb, FeatureNode node, string prefix, bool isLast, 
            Dictionary<FeatureNode, List<FeatureNode>> hierarchy)
        {
            string connector = isLast ? "└── " : "├── ";
            string featureName = node.Feature.GetType().Name;
            
            List<string> featureSystems = new List<string>();
            foreach (KeyValuePair<Type, SystemMetadata> kvp in _systemMetadata)
            {
                if (kvp.Value.FeatureNode == node)
                {
                    featureSystems.Add(kvp.Key.Name);
                }
            }
            
            sb.Append(prefix);
            sb.Append(connector);
            sb.Append($"🔧 {featureName}");
            
            if (node.IsPausable)
                sb.Append(" [PAUSABLE]");
            
            if (featureSystems.Count > 0)
                sb.Append($" ({featureSystems.Count} systems)");
                
            sb.AppendLine();
            
            for (int i = 0; i < featureSystems.Count; i++)
            {
                string systemPrefix = prefix + (isLast ? "    " : "│   ");
                string systemConnector = i == featureSystems.Count - 1 && 
                    (!hierarchy.ContainsKey(node) || hierarchy[node].Count == 0) ? "└── " : "├── ";
                
                sb.AppendLine($"{systemPrefix}{systemConnector}⚙️ {featureSystems[i]}");
            }
            
            if (hierarchy.TryGetValue(node, out List<FeatureNode> children))
            {
                for (int i = 0; i < children.Count; i++)
                {
                    string childPrefix = prefix + (isLast ? "    " : "│   ");
                    bool isLastChild = i == children.Count - 1;
                    PrintFeatureNode(sb, children[i], childPrefix, isLastChild, hierarchy);
                }
            }
        }
        
        #endif

        [ShowInInspector, Searchable] private readonly Dictionary<Type, SystemMetadata> _systemMetadata;
        [ShowInInspector] private readonly FastList<ISystem> _systems;
        [ShowInInspector] private bool _isPaused;

        [ShowInInspector]
        public SystemsGroup Value { get; }

        public FeatureTree(SystemsGroup systemsGroup, int initialCapacity = 32)
        {
            Value = systemsGroup;
            _systemMetadata = new Dictionary<Type, SystemMetadata>(initialCapacity);
            _systems = new FastList<ISystem>(initialCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FeatureTree AddSystem(ISystem system, FeatureNode featureNode)
        {
            Type systemType = system.GetType();

            _systems.Add(system);
            
            _systemMetadata[systemType] = new SystemMetadata(
                _systems.length - 1,
                SystemPauseMode.DEFAULT,
                featureNode
            );

            Value.AddSystem(system);
            return this;
        }

        public FeatureTree AddFeature(IFeature feature)
        {
            FeatureNode rootNode = new FeatureNode(feature);
            FeatureContext context = new FeatureContext(this, rootNode);

            feature.Configure(context);
            context.Complete();

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FeatureTree SetSystemPauseMode(ISystem system, SystemPauseMode pauseMode)
        {
            if (_systemMetadata.TryGetValue(system.GetType(), out SystemMetadata metadata))
                _systemMetadata[system.GetType()] = metadata.WithPauseMode(pauseMode);

            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldSystemBePaused(ISystem system)
        {
            if (!_systemMetadata.TryGetValue(system.GetType(), out SystemMetadata metadata))
                return false;

            return metadata.PauseMode switch
            {
                SystemPauseMode.ALWAYS_PAUSABLE => true,
                SystemPauseMode.NEVER_PAUSABLE => false,
                _ => metadata.FeatureNode.IsFeaturePausable()
            };
        }

        public void Pause()
        {
            if (_isPaused)
                return;

            for (int i = 0; i < _systems.length; i++)
            {
                ISystem system = _systems[i];
                if (ShouldSystemBePaused(system))
                    Value.DisableSystem(system);
            }

            _isPaused = true;
        }

        public void Unpause()
        {
            if (!_isPaused) 
                return;

            for (int i = 0; i < _systems.length; i++)
            {
                ISystem system = _systems[i];
                
                if (ShouldSystemBePaused(system))
                    Value.EnableSystem(system);
            }

            RestoreSystemsOrder();
            _isPaused = false;
        }

        private void RestoreSystemsOrder()
        {
            SystemOrderComparer orderComparer = new SystemOrderComparer(_systemMetadata);

            FastList<ISystem>[] collections =
            {
                Value.systems,
                Value.fixedSystems,
                Value.lateSystems,
                Value.cleanupSystems
            };

            foreach (FastList<ISystem> collection in collections)
            {
                if (collection.length <= 1)
                    continue;

                collection.Sort(orderComparer);
            }
        }

        private sealed class SystemOrderComparer : IComparer<ISystem>
        {
            private readonly Dictionary<Type, SystemMetadata> _metadata;

            public SystemOrderComparer(Dictionary<Type, SystemMetadata> metadata)
            {
                _metadata = metadata;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(ISystem x, ISystem y)
            {
                _metadata.TryGetValue(x!.GetType(), out SystemMetadata metadataX);
                _metadata.TryGetValue(y!.GetType(), out SystemMetadata metadataY);
                return metadataX.Order.CompareTo(metadataY.Order);
            }
        }

        public FeatureTree Build(int order, World world)
        {
            world.AddSystemsGroup(order, Value);
            return this;
        }
    }
}