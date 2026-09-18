#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Gameplay.Features.CommonFeature.Services
{
    public sealed class ServicesInspectorWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Services Inspector %#s")]
        private static void OpenWindow()
        {
            ServicesInspectorWindow window = GetWindow<ServicesInspectorWindow>();
            window.titleContent = new GUIContent("Services Inspector", EditorIcons.List.Raw);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        [Title("Services Inspector", titleAlignment: TitleAlignments.Centered)]
        [HorizontalGroup("Header")]
        [ShowInInspector]
        [LabelText("Total Services")]
        private int ServiceCount => AllServices.Instance?.GetAllServices()?.Count ?? 0;

        [PropertySpace(6)]
        [Searchable(FilterOptions = SearchFilterOptions.ISearchFilterableInterface)]
        [ListDrawerSettings(
            HideAddButton = true,
            HideRemoveButton = true,
            DraggableItems = false,
            ShowPaging = false,
            ShowItemCount = false
        )]
        [ShowInInspector]
        [LabelText("Registered Services")]
        [EnableGUI]
        [HideReferenceObjectPicker]
        private List<ServiceInfo> Services
        {
            get => GetAllServices().ToList();
            set => throw new NotImplementedException();
        }

        private IEnumerable<ServiceInfo> GetAllServices()
        {
            if (!Application.isPlaying || AllServices.Instance == null)
            {
                yield break;
            }

            Dictionary<Type, IService> services = AllServices.Instance.GetAllServices();

            foreach (KeyValuePair<Type, IService> kvp in services.OrderBy(static x => x.Key.Name))
            {
                yield return new ServiceInfo(kvp.Key, kvp.Value);
            }
        }

        protected override void OnImGUI()
        {
            if (Application.isPlaying && Event.current.type == EventType.Layout)
            {
                Repaint();
            }

            base.OnImGUI();
        }
    }

    [Serializable]
    public class ServiceInfo : ISearchFilterable
    {
        [field: FoldoutGroup("$ServiceTypeName")]
        [field: OdinSerialize]
        [field: HideReferenceObjectPicker]
        [field: HideLabel]
        [field: InlineProperty]
        public IService ServiceInstance { get; private set; }

        public string ServiceTypeName { get; private set; }

        [FoldoutGroup("$ServiceTypeName")]
        [Button("Select", ButtonSizes.Medium, Stretch = false, ButtonAlignment = 0)]
        [GUIColor(0.9f, 0.9f, 0.6f)]
        [PropertySpace(spaceBefore: 3)]
        private void SelectService()
        {
            if (ServiceInstance is Object unityObj)
            {
                Selection.activeObject = unityObj;
                EditorGUIUtility.PingObject(unityObj);
            }
        }

        public ServiceInfo(Type serviceType, IService serviceInstance)
        {
            ServiceTypeName = GetFriendlyTypeName(serviceType);
            ServiceInstance = serviceInstance;
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                Type[] genericArgs = type.GetGenericArguments();
                string typeName = type.Name.Split('`')[0];
                return $"{typeName}<{string.Join(", ", genericArgs.Select(static t => t.Name))}>";
            }

            return type.Name;
        }

        public bool IsMatch(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return true;

            string search = searchString.ToLower();

            return ServiceTypeName.ToLower().Contains(search) ||
                   ServiceInstance?.GetType().Name.ToLower().Contains(search) == true;
        }
    }
}
#endif