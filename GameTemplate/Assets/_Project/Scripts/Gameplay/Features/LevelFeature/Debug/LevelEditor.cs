#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using _Project.Scripts.Gameplay.Features.LevelFeature.Configs;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Linq;
using _Project.Scripts.Gameplay.Features.CommonFeature;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Behaviours;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature.Configs;
using Sirenix.Serialization;
using _Project.Scripts.Gameplay.Features.LevelFeature.Behaviours;
using System.IO;
using _Project.Scripts.Gameplay.Features.InteractiveObjectsFeature;
using JetBrains.Annotations;
using Kamgam.ExcludeFromBuild;

namespace _Project.Scripts.Gameplay.Features.LevelFeature.Debug
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LevelView))]
    public sealed class LevelEditor : SerializedMonoBehaviour
    {
        [TabGroup("Level Info")]
        [SerializeField] private string _levelName = "[Level] Default";

        [TabGroup("Level Info")]
        [SerializeField] private LevelConfig _levelConfig;

        [TabGroup("Generation")]
        [SerializeField] private GenerationSettings _generationSettings = new GenerationSettings();

        [TabGroup("Roads"), ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [SerializeField] private List<RoadSegment> _roads = new List<RoadSegment>();

        [TabGroup("Objects"), ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        [NonSerialized, OdinSerialize]
        [HideReferenceObjectPicker]
        [Searchable]
        private List<LevelObjectData> _levelObjects = new List<LevelObjectData>();

        [TabGroup("Save Settings")]
        [FolderPath(RequireExistingPath = true)]
        [SerializeField] private string _prefabSavePath = "Assets/_Project/Prefabs/Levels";

        [TabGroup("Save Settings")]
        [FolderPath(RequireExistingPath = true)]
        [SerializeField] private string _configSavePath = "Assets/_Project/Resources/Configs/Levels";

        private readonly ConfigsService _configs = new ConfigsService();
        private GameObject _roadPrefab;

        public LevelConfig LevelConfig => _levelConfig;

        public List<LevelObjectData> LevelObjects
        {
            get => _levelObjects;
            set => _levelObjects = value;
        }

        private void OnEnable()
        {
            _roadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Environment/Road.prefab");
            _configs.Initialize();
        }

        private void Reset()
        {
            ExcludeFromBuildComponent exclude = GetComponent<ExcludeFromBuildComponent>();

            if (exclude == null)
                exclude = gameObject.AddComponent<ExcludeFromBuildComponent>();

            exclude.GameObject = false;
            exclude.AllGroups = true;

            exclude.Components ??= new List<Component>();

            if (!exclude.Components.Contains(this))
            {
                exclude.Components.Add(this);
                EditorUtility.SetDirty(exclude);
            }

            hideFlags = HideFlags.DontSaveInBuild;
        }

        private void AddSystemObjects()
        {
            float totalLength = _roads
                .Skip(_generationSettings.EmptyRoadsAtStart)
                .Take(_roads.Count - _generationSettings.EmptyRoadsAtEnd - _generationSettings.EmptyRoadsAtStart)
                .Sum(static r => r.RoadLength);

            float startOffset = _roads
                .Take(_generationSettings.EmptyRoadsAtStart)
                .Sum(static r => r.RoadLength);

            LevelObjectData finishTrigger = AddFinishTrigger();
            PostFinishFarmingArea farmingArea = AddFarmingArea();
            AddBonusChest();

            return;

            LevelObjectData AddFinishTrigger()
            {
                LevelObjectData trigger = ObjectGenerator.GenerateObject<LevelObjectData>(
                    InteractiveObjectId.FINISH_TRIGGER,
                    position: new Vector3(0, 0.01f, startOffset + totalLength + 5f),
                    GetLastContentRoadIndex());

                _levelObjects.Add(trigger);
                CreateObjectView(trigger);
                return trigger;
            }

            PostFinishFarmingArea AddFarmingArea()
            {
                PostFinishFarmingArea area = ObjectGenerator.GenerateObject<PostFinishFarmingArea>(
                    InteractiveObjectId.POST_FINISH_FARMING_AREA,
                    new Vector3(0, 0, finishTrigger.Position.z + 5f),
                    GetLastContentRoadIndex());

                InteractiveObjectView areaView = CreateObjectView(area);

                area.View = areaView;
                area.GenerateObjects();

                foreach (LevelObjectData stoneData in area.CashStones)
                    CreateObjectView(stoneData, areaView.transform);

                _levelObjects.Add(area);
                return area;
            }

            void AddBonusChest()
            {
                LevelObjectData bonusChest = ObjectGenerator.GenerateObject<LevelObjectData>(
                    InteractiveObjectId.BONUS_CHEST,
                    new Vector3(0, 0, farmingArea.Position.z + farmingArea.CalculateAreaSize().z + 7f),
                    GetLastContentRoadIndex());

                _levelObjects.Add(bonusChest);
                CreateObjectView(bonusChest);
            }
        }

        [TabGroup("Generation")]
        [Button("Generate Complete Level", ButtonSizes.Large)]
        private void GenerateCompleteLevel()
        {
            ClearLevel();

            for (int i = 0; i < _generationSettings.EmptyRoadsAtStart; i++)
                AddNewRoad(isEmptyRoad: true);

            for (int i = 0; i < _generationSettings.RoadCount; i++)
                AddNewRoad();

            for (int i = 0; i < _generationSettings.EmptyRoadsAtEnd; i++)
                AddNewRoad(isEmptyRoad: true);

            for (int i = _generationSettings.EmptyRoadsAtStart;
                 i < _roads.Count - _generationSettings.EmptyRoadsAtEnd;
                 i++)
                RegenerateRoadObjects(i);
        }

        [TabGroup("Roads")]
        [Button("Add New Road")]
        private void AddNewRoad(bool isEmptyRoad = false)
        {
            RoadSegment newRoad = new RoadSegment
            {
                RoadIndex = _roads.Count,
                RoadLength = _generationSettings.DefaultRoadLength,
                ObjectCount = isEmptyRoad ? 0 : _generationSettings.DefaultObjectCount,
                ObjectSpacing = isEmptyRoad ? 0 : _generationSettings.DefaultObjectSpacing,
                ObjectPresets = isEmptyRoad
                    ? Array.Empty<ObjectGenerationPreset>()
                    : _generationSettings.ObjectPresets.ToArray(),
                IsEmptyRoad = isEmptyRoad
            };

            _roads.Add(newRoad);
            UpdateRoadPositions();
            GameObject roadView = CreateRoadView(newRoad);
            newRoad.View = roadView;
        }

        [TabGroup("Roads")]
        [Button("Remove Last Road")]
        private void RemoveLastRoad()
        {
            if (_roads.Count == 0)
                return;

            int lastRoadIndex = _roads.Count - 1;

            foreach (LevelObjectData editorObj in _levelObjects)
            {
                if (editorObj.RoadIndex == lastRoadIndex)
                    DestroyImmediate(editorObj.View.gameObject);
            }

            RoadSegment lastRoad = _roads[lastRoadIndex];
            DestroyImmediate(lastRoad.View.gameObject);

            _levelObjects.RemoveAll(obj => obj.RoadIndex == lastRoadIndex);

            _roads.RemoveAt(lastRoadIndex);
        }

        [TabGroup("Generation")]
        [Button("Clear Level")]
        private void ClearLevel() =>
            ClearAllVisuals();

        [TabGroup("Generation")]
        [Button("Apply Global Presets to All Roads")]
        private void ApplyGlobalPresetsToAllRoads()
        {
            foreach (RoadSegment road in _roads)
            {
                road.ObjectPresets = _generationSettings.ObjectPresets.ToArray();
            }

            UnityEngine.Debug.Log($"Applied global presets to {_roads.Count} roads");
        }

        [TabGroup("Level Info")]
        [Button("Sync to Config", ButtonSizes.Large)]
        private void SyncToConfig()
        {
            if (_levelConfig == null)
            {
                UnityEngine.Debug.LogError("Level config is not assigned!");
                return;
            }

            _levelConfig.LevelObjects = _levelObjects.ToList();

            EditorUtility.SetDirty(_levelConfig);
            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log(
                $"Synced {_levelObjects.Count} objects from {_roads.Count} roads to level config!");
        }

        [TabGroup("Level Info")]
        [Button("Load from Config")]
        private void LoadFromConfig()
        {
            if (_levelConfig == null)
            {
                UnityEngine.Debug.LogError("Level config is not assigned!");
                return;
            }

            ClearLevel();

            _levelObjects = new List<LevelObjectData>(_levelConfig.LevelObjects.Where(static obj => obj.IsActive));

            IOrderedEnumerable<IGrouping<int, LevelObjectData>> groupedObjects = _levelObjects
                .GroupBy(static obj => obj.RoadIndex)
                .OrderBy(static g => g.Key);

            for (int i = 0; i < _generationSettings.EmptyRoadsAtStart; i++)
                AddNewRoad(isEmptyRoad: true);

            for (int i = 0; i < _generationSettings.RoadCount; i++)
                AddNewRoad();

            for (int i = 0; i < _generationSettings.EmptyRoadsAtEnd; i++)
                AddNewRoad(isEmptyRoad: true);

            foreach (IGrouping<int, LevelObjectData> group in groupedObjects)
            {
                foreach (LevelObjectData objData in group)
                {
                    CreateObjectView(objData);
                }
            }

            UnityEngine.Debug.Log($"Loaded {_levelObjects.Count} objects from config to {_roads.Count} roads!");
        }

        [TabGroup("Save Settings")]
        [Button("Create Level Prefab & Config", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        private void CreateLevelPrefabAndConfig()
        {
            if (string.IsNullOrEmpty(_levelName))
            {
                UnityEngine.Debug.LogError("Level name cannot be empty!");
                return;
            }

            if (string.IsNullOrEmpty(_prefabSavePath) || string.IsNullOrEmpty(_configSavePath))
            {
                UnityEngine.Debug.LogError("Save paths must be specified!");
                return;
            }

            if (!Directory.Exists(_prefabSavePath))
                Directory.CreateDirectory(_prefabSavePath);

            if (!Directory.Exists(_configSavePath))
                Directory.CreateDirectory(_configSavePath);

            LevelConfig config = CreateLevelConfig();
            LevelEditor prefab = CreateLevelPrefab(config);
            config.LevelPrefab = prefab.GetComponent<LevelView>();

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        [TabGroup("Objects")]
        [Button("Add Capture Zone", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.7f, 1f)]
        [ShowIf(nameof(IsCaptureZoneMode))]
        private void AddCaptureZone()
        {
            int lastRoadIndex = GetLastContentRoadIndex();

            if (lastRoadIndex < 0 || lastRoadIndex >= _roads.Count)
                return;

            RoadSegment road = _roads[lastRoadIndex];
            Vector3 position = new Vector3(
                0f,
                0.1f,
                road.StartZ + road.RoadLength * 0.5f
            );

            LevelObjectData zoneData = ObjectGenerator.GenerateObject<LevelObjectData>(
                InteractiveObjectId.CAPTURE_ZONE,
                position,
                lastRoadIndex,
                customData: new InteractiveObjectData
                {
                    CaptureZoneConfig = new CaptureZoneConfig()
                }
            );

            CreateObjectView(zoneData);
            _levelObjects.Add(zoneData);
            
            Selection.activeObject = zoneData.View.gameObject;
        }

        [TabGroup("Objects")]
        [Button("Add Collectible", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.7f, 1f)]
        [ShowIf(nameof(IsCollectorMode))]
        private void AddCollectible(CollectibleType collectibleType = CollectibleType.RADIO)
        {
            int lastRoadIndex = GetLastContentRoadIndex();

            if (lastRoadIndex < 0 || lastRoadIndex >= _roads.Count)
                return;

            RoadSegment road = _roads[lastRoadIndex];
            Vector3 position = new Vector3(
                0f,
                1f,
                road.StartZ + road.RoadLength * 0.5f
            );

            LevelObjectData collectibleData = ObjectGenerator.GenerateObject<LevelObjectData>(
                InteractiveObjectId.COLLECTIBLE,
                position,
                lastRoadIndex,
                customData: new InteractiveObjectData
                {
                    CollectibleConfig = new CollectibleConfig
                    {
                        Type = collectibleType
                    }
                }
            );

            CreateObjectView(collectibleData);
            _levelObjects.Add(collectibleData);
            
            Selection.activeObject = collectibleData.View.gameObject;
        }

        [TabGroup("Objects")]
        [Button("Add  Explosive Barrel", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.7f, 1f)]
        [HideIf(nameof(IsStealthMode))]
        private void AddExplosiveBarrel()
        {
            int lastRoadIndex = GetLastContentRoadIndex();

            if (lastRoadIndex < 0 || lastRoadIndex >= _roads.Count)
                return;

            RoadSegment road = _roads[lastRoadIndex];
            Vector3 position = new Vector3(
                0f,
                0f,
                road.StartZ + road.RoadLength * 0.5f
            );

            LevelObjectData explosiveBarrelData = ObjectGenerator.GenerateObject<LevelObjectData>(
                InteractiveObjectId.EXPLOSIVE_BARREL,
                position,
                lastRoadIndex,
                customData: new InteractiveObjectData
                {
                    ExplosiveBarrelConfig = new ExplosiveBarrelConfig()
                }
            );

            CreateObjectView(explosiveBarrelData);
            _levelObjects.Add(explosiveBarrelData);
        
            Selection.activeObject = explosiveBarrelData.View.gameObject;
        }

        [TabGroup("Objects")]
        [Button("Validate Level Objects", ButtonSizes.Medium)]
        [GUIColor(1f, 0.8f, 0.3f)]
        private void ValidateLevelObjects()
        {
            int fixedCount = 0;

            List<LevelObjectData> toRemove = new List<LevelObjectData>();
            foreach (LevelObjectData obj in _levelObjects)
            {
                if (obj.View == null || obj.View.gameObject == null)
                {
                    toRemove.Add(obj);
                    fixedCount++;
                }
            }

            foreach (LevelObjectData obj in toRemove)
                _levelObjects.Remove(obj);

            InteractiveObjectView[] allViews = FindObjectsOfType<InteractiveObjectView>();

            foreach (InteractiveObjectView view in allViews)
            {
                if (!view.CompareTag("EditorOnly"))
                    continue;

                bool foundInList = _levelObjects.Any(obj => obj.View == view);

                if (!foundInList)
                {
                    DestroyImmediate(view.gameObject);
                    fixedCount++;
                }
            }

            if (fixedCount != 0)
                EditorUtility.SetDirty(this);
        }

        private bool IsCaptureZoneMode() =>
            _levelConfig != null && _levelConfig.LevelMode == LevelMode.CAPTURE_ZONES;

        private bool IsStealthMode() => 
            _levelConfig != null && _levelConfig.LevelMode == LevelMode.STEALTH;
        
        private bool IsCollectorMode() =>
            _levelConfig != null && _levelConfig.LevelMode == LevelMode.COLLECTOR;

        public void RegenerateRoadObjects(int roadIndex, InteractiveObjectId[] specificObjects = null)
        {
            if (roadIndex <= 0 || roadIndex >= _roads.Count)
                return;

            RoadSegment road = _roads[roadIndex];

            foreach (LevelObjectData editorObj in _levelObjects)
            {
                if (editorObj.RoadIndex == roadIndex)
                    DestroyImmediate(editorObj.View.gameObject);
            }

            _levelObjects.RemoveAll(obj => obj.RoadIndex == roadIndex);
            road.OccupiedPositions.Clear();

            GenerateRoadObjects(roadIndex, specificObjects);
        }

        private void GenerateRoadObjects(int roadIndex, InteractiveObjectId[] specificObjects = null)
        {
            if (roadIndex >= _roads.Count)
                return;

            RoadSegment road = _roads[roadIndex];

            if (road.IsEmptyRoad)
                return;

            if (roadIndex % 2 == 0)
            {
                if (road.UsePositionalPlacement)
                    GenerateObjectsWithPositionalPlacement(road, specificObjects);
                else
                    GenerateRoadObjectsWithSpace(road);
            }

            if (roadIndex == GetLastContentRoadIndex())
                AddSystemObjects();
        }

        private void GenerateObjectsWithPositionalPlacement(RoadSegment road,
            InteractiveObjectId[] specificObjects = null)
        {
            int objectsToGenerate = specificObjects != null
                ? specificObjects.Length
                : UnityEngine.Random.Range(minInclusive: 1, maxExclusive: Mathf.Min(road.ObjectCount + 1, b: 4));

            for (int i = 0; i < objectsToGenerate; i++)
            {
                SpawnPosition? availablePos = road.GetRandomAvailablePosition();

                if (!availablePos.HasValue)
                    break;

                SpawnPosition spawnPos = availablePos.Value;
                Vector3 position = road.GetSpawnPosition(spawnPos);

                ObjectGenerationPreset preset;

                if (specificObjects != null)
                {
                    preset = _generationSettings.GetPresetForType(specificObjects[i]);
                    if (preset == null)
                    {
                        UnityEngine.Debug.LogWarning($"No preset found for {specificObjects[i]}");
                        continue;
                    }
                }
                else
                {
                    preset = road.GetRandomPreset() ?? _generationSettings.GetRandomPreset();
                    if (preset == null)
                    {
                        UnityEngine.Debug.LogWarning($"No generation presets available for road {road.RoadIndex}");
                        continue;
                    }
                }

                LevelObjectData editorObj = ObjectGenerator.GenerateObjectFromPreset(preset, position, road.RoadIndex);

                {
                    if (editorObj is EnemyGroup enemyGroup)
                    {
                        switch (spawnPos)
                        {
                            case SpawnPosition.LEFT:
                                enemyGroup.Position.x += RoadSegment.LANE_WIDTH * 0.46f;
                                break;
                            case SpawnPosition.CENTER:
                                break;
                            case SpawnPosition.RIGHT:
                                enemyGroup.Position.x -= RoadSegment.LANE_WIDTH * 0.46f;
                                break;
                        }
                    }
                    else if (editorObj.ObjectType == InteractiveObjectId.WATCH_TOWER)
                    {
                        switch (spawnPos)
                        {
                            case SpawnPosition.LEFT:
                                editorObj.Position.x -= RoadSegment.LANE_WIDTH * 2f;
                                break;
                            case SpawnPosition.CENTER:
                                break;
                            case SpawnPosition.RIGHT:
                                editorObj.Position.x += RoadSegment.LANE_WIDTH * 2f;
                                break;
                        }
                    }
                    else if (editorObj.ObjectType == InteractiveObjectId.PATROL_GUARD)
                    {
                        editorObj.Position.y = 1.6f;
                    }
                }

                road.OccupyPosition(spawnPos);

                CreateObjectView(editorObj);
                _levelObjects.Add(editorObj);

                switch (editorObj)
                {
                    case EnemyGroup enemyGroup:
                    {
                        enemyGroup.GenerateEnemies(this);

                        break;
                    }
                }
            }
        }

        private void GenerateRoadObjectsWithSpace(RoadSegment road)
        {
            if (road.RoadIndex >= _roads.Count)
                return;

            float roadCenterZ = road.StartZ + road.RoadLength * 0.5f;

            int objectCount = road.ObjectCount;
            float totalWidth = (objectCount - 1) * road.ObjectSpacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < objectCount; i++)
            {
                Vector3 position = new Vector3(startX + i * road.ObjectSpacing, 0.1f, roadCenterZ);

                ObjectGenerationPreset preset = road.GetRandomPreset() ?? _generationSettings.GetRandomPreset();

                if (preset == null)
                {
                    UnityEngine.Debug.LogWarning($"No generation presets available for road {road.RoadIndex}");
                    continue;
                }

                LevelObjectData editorObj = ObjectGenerator.GenerateObjectFromPreset(preset, position, road.RoadIndex);

                CreateObjectView(editorObj);
                _levelObjects.Add(editorObj);
            }
        }

        private LevelConfig CreateLevelConfig()
        {
            string configFileName = $"{_levelName}.asset";
            string configPath = Path.Combine(_configSavePath, configFileName);

            if (File.Exists(configPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Config Exists",
                    $"Level config '{configFileName}' already exists. Overwrite?",
                    "Overwrite", "Cancel");

                if (!overwrite)
                    throw new OperationCanceledException("User canceled overwrite operation.");
            }

            LevelConfig config = ScriptableObject.CreateInstance<LevelConfig>();

            config.LevelObjects = new List<LevelObjectData>(_levelObjects);

            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log($"Created LevelConfig at: {configPath}");
            return config;
        }

        private LevelEditor CreateLevelPrefab(LevelConfig config)
        {
            string prefabFileName = $"{_levelName}.prefab";
            string prefabPath = Path.Combine(_prefabSavePath, prefabFileName);

            if (File.Exists(prefabPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Prefab Exists",
                    $"Level prefab '{prefabFileName}' already exists. Overwrite?",
                    "Overwrite", "Cancel");

                if (!overwrite)
                    throw new OperationCanceledException("User canceled overwrite operation.");
            }

            _levelConfig = config;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);

            if (gameObject.scene.IsValid())
                DestroyImmediate(gameObject, allowDestroyingAssets: true);

            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log($"Created Level prefab at: {prefabPath}");

            return prefab.GetComponent<LevelEditor>();
        }

        private void UpdateRoadPositions()
        {
            float currentZ = 0f;

            for (int i = 0; i < _roads.Count; i++)
            {
                _roads[i].RoadIndex = i;
                _roads[i].StartZ = currentZ;
                _roads[i].EndZ = currentZ + _roads[i].RoadLength;

                currentZ = _roads[i].EndZ;
            }
        }

        private GameObject CreateRoadView(RoadSegment road)
        {
            GameObject roadView = (GameObject)PrefabUtility.InstantiatePrefab(_roadPrefab, transform);
            roadView.transform.position = new Vector3(0f, 0f, road.StartZ);
            roadView.transform.rotation = _roadPrefab.transform.rotation;
            roadView.name = $"Road_{road.RoadIndex}";
            return roadView;
        }

        public InteractiveObjectView CreateObjectView(LevelObjectData editorObj, [CanBeNull] Transform parent = null)
        {
            InteractiveObjectConfig baseConfig = _configs.GetInteractiveObjectConfig(editorObj.ObjectType);
            editorObj.CustomData.InitializeFromBase(baseConfig);

            InteractiveObjectView prefab;

            if (editorObj.ObjectType == InteractiveObjectId.COLLECTIBLE)
            {
                prefab = editorObj.CustomData.CollectibleConfig.CollectibleByType[
                    editorObj.CustomData.CollectibleConfig.Type];
            }
            else
            {
                prefab = editorObj.CustomData.InteractiveObjectPrefab;
            }

            InteractiveObjectView objectView = (InteractiveObjectView)PrefabUtility.InstantiatePrefab(
                prefab,
                parent ?? transform);

            objectView.transform.position = editorObj.Position;
            objectView.transform.rotation = prefab.transform.rotation;

            objectView.name = $"{editorObj.ObjectType}";

            LevelObjectTransformSyncer transformSyncer =
                objectView.gameObject.AddComponent<LevelObjectTransformSyncer>();
            transformSyncer.LinkedData = editorObj;

            objectView.gameObject.AddComponent<ExcludeFromBuildComponent>();

            switch (editorObj.ObjectType)
            {
                case InteractiveObjectId.EVOLUTIONARY_INCUBATOR:
                    objectView.name += $"_{editorObj.CustomData.EvolutionaryIncubatorConfig.SpawnCount}";
                    break;
                case InteractiveObjectId.CASH_STONE:
                    objectView.HealthBar.UpdateDisplay(
                        editorObj.CustomData.MaxHealth,
                        editorObj.CustomData.MaxHealth);
                    break;
                case InteractiveObjectId.GATE:
                {
                    GateView gateView = (GateView)objectView;

                    gateView.Construct(
                        editorObj.CustomData.GateConfig.EffectType,
                        editorObj.CustomData.GateConfig.StatConfig.TargetStat);

                    break;
                }
                case InteractiveObjectId.ENEMY_GROUP:
                    EnemyGroup enemyGroup = (EnemyGroup)editorObj;
                    objectView.name += $"_{enemyGroup.CustomData.EnemyGroupConfig.FormationType}";
                    break;
                case InteractiveObjectId.COLLECTIBLE:
                    objectView.name += $"_{editorObj.CustomData.CollectibleConfig.Type}";
                    break;
            }

            objectView.tag = "EditorOnly";
            editorObj.View = objectView;

            return objectView;
        }

        private void ClearAllVisuals()
        {
            foreach (LevelObjectData editorObj in _levelObjects)
            {
                if (editorObj.View != null)
                    DestroyImmediate(editorObj.View.gameObject);
            }

            _levelObjects.Clear();

            foreach (RoadSegment road in _roads)
            {
                if (road.View != null)
                    DestroyImmediate(road.View);
            }

            _roads.Clear();
        }

        private int GetLastContentRoadIndex() =>
            _roads.Count - _generationSettings.EmptyRoadsAtEnd - 1;

        private void OnDrawGizmos()
        {
            foreach (LevelObjectData editorObj in _levelObjects)
            {
                Vector3 labelPos = editorObj.Position + Vector3.up * 2f;
                Handles.Label(labelPos, editorObj.ObjectType.ToString());
            }
        }
    }
}
#endif