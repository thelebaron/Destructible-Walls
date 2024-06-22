//#define DEBUG_CUSTOMIZATION
//#define DEBUG_CUSTOMIZATION_DETAILED

#nullable enable
#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Editor;
using Unity.Entities.UI;
using Unity.Transforms;

using UnityEditor;
using UnityEditor.Compilation;

using UnityEngine;
using UnityEngine.UIElements;

using Object = UnityEngine.Object;

namespace YourNamespace
{
  /// <summary>
  /// Customization for the inspection of entities in the Unity Inspector.
  /// Provides the following features:
  /// * Maintaining the foldout states of components/buffers and tags section
  /// * Can keep search filter
  /// * Can keep scrollbar position
  /// * Can always show the entity header at the top of the inspector
  /// * Can categorize the entities by a category key for grouping entities and having seperate states
  /// </summary>
  /// <remarks>
  /// References to the following assemblies are required:
  /// * Unity.Collections
  /// * Unity.Entities
  /// * Unity.Entities.Editor
  /// * Unity.Entities.UI
  /// * Unity.Entities.UI.Editor
  /// Requires Assembly Definition References (asmref) with internals access for:
  /// * Unity.Entities.Editor
  /// * Unity.Entities.UI.Editor
  /// Additionally uses internals of (just informational):
  /// * UnityEditor.CoreModule
  /// * UnityEngine.UIElementsModule
  /// Some more information can be found here:
  /// * <see cref="http://forum.unity.com/threads/entity-inspector-usability-customization.1589481/" />
  /// --- Changelog ---
  /// 1.2
  /// * No more errors when selecting anything while the Unity Inspector is closed or docked being hidden/inactive
  /// * Activating the Unity Inspector when being docked and hidden/inactive will now correctly show the restored states
  /// * Selecting an entity while Inspector is closed and opening the Inspector thereafter will now correctly attach the
  /// customization and restores the states
  /// * Fixed a bug where the entity header was not always shown when category preserved scroll position was disabled
  /// 1.1
  /// * Fixed a behavior which caused the focus to switch to the inspector when selecting a project asset
  /// * Fixed a flickering issue when having entity header always shown (no more use of delayCall)
  /// 1.0
  /// * Initial version
  /// </remarks>
  public static class EntityInspectorCustomization
  {
    private static HashSet<CategoryStateData> _categoryStates = new();

    private static CategoryStateData _currentCategoryState = new();
    private static Entity _currentInspectedEntity;
    private static CategoryStateData _defaultCategoryState = new();
    private static Color _defaultWindowBackgroundColor;
    private static GeneralStateData _generalState = new();
    private static bool _hasUnsavedChanges;
    private static readonly Delegate _inspectorHierarchyChangedEventHandler;
    private static VisualElement? _inspectorRoot;
    private static EditorWindow? _inspectorWindow;
    private static ExpressionInstance? _inspectorTypedPanelInstance;
    private static double _nextAutoSaveTime;
    private static readonly List<Foldout> _registeredComponentFoldouts = new();
    private static EntityEditor? _registeredEntityEditor;

    private static VisualElement? _registeredEntityHeaderElement;
    private static Button? _registeredSearchCancelButton;
    private static TextField? _registeredSearchTextField;

    private static Foldout? _registeredTagsFoldout;
    private static Scroller? _registeredVerticalScroller;
    private static Color _separatorLineColor;
    private static string _unityProjectName = string.Empty;

    /// <summary>
    /// The auto-save interval in seconds.
    /// </summary>
    private const double AutoSaveIntervalSeconds = 60d;

    /// <summary>
    /// The base path for all menu entries.
    /// </summary>
    private const string BaseMenuPath = "Tools/Entity Inspector";

    private static readonly Type BufferElementBaseType;
    private static readonly Func<object, ExpressionInstance> CastToBaseVisualElementPanelCall;
    private const string CategoryPreservedScrollPositionMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Option: Category Preserved Scroll Position";
    private const string CollapseActiveFoldoutsMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Active: Collapse Foldouts";
    private const string CollapseGlobalFoldoutsMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Global: Collapse Foldouts";
    private static readonly Type ComponentElementType;
    private static readonly EditorSetSelection EditorSetSelectionCall;
    private const string EntityHeaderAlwaysShownMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Option: Entity Header Always Shown";

    private const string EntityInspectorCustomizationName = nameof(EntityInspectorCustomization);
    private const string ExpandActiveFoldoutsMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Active: Expand Foldouts";
    private const string ExpandGlobalFoldoutsMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Global: Expand Foldouts";
    private const string FoldoutStatesEditorPrefsName = EntityInspectorCustomization.EntityInspectorCustomizationName + "/FoldoutStates";

    private static readonly (Func<ExpressionBuilderUI.CustomHierarchyEvent, Delegate> CreateHandler, Action<ExpressionInstance, Delegate> Add, Action<ExpressionInstance, Delegate>
      Remove, Func<ExpressionInstance, Delegate, bool> Contains) HierarchyChangedEventCalls;

    private const string PreserveSearchMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Option: Preserve Search String";

    private static readonly Type UnityInspectorWindowType;
    private static readonly Func<EditorWindow, ExpressionInstance> GetEditorWindowParentHostViewCall;
    private static readonly Func<ExpressionInstance, VisualElement> GetGuiViewVisualTreeCall;
    private static readonly Func<IList> GetInspectorWindowAllInspectorsCall;
    private const string UseCategoriesMenuItemName = EntityInspectorCustomization.BaseMenuPath + "/Option: Use categories";

    /// <summary>
    /// Static constructor.
    /// </summary>
    static EntityInspectorCustomization()
    {
      // Setup all lambda calls and do necessary reflection

      const string HostViewTypeName = "UnityEditor.HostView, UnityEditor.CoreModule";
      const string GuiViewTypeName = "UnityEditor.GUIView, UnityEditor.CoreModule"; // HostView inherits from GUIView
      const string InspectorWindowTypeName = "UnityEditor.InspectorWindow, UnityEditor.CoreModule";
      EntityInspectorCustomization.UnityInspectorWindowType = Type.GetType(InspectorWindowTypeName, true)!;

      EntityInspectorCustomization.EditorSetSelectionCall = ExpressionBuilder.BuildStaticMethodCallUsingDelegate<Selection, EditorSetSelection>("SetSelection");
      EntityInspectorCustomization.CastToBaseVisualElementPanelCall = ExpressionBuilder.BuildCastToExpressionInstance(ExpressionBuilderUI.BasicVisualElementPanelTypeName);

      EntityInspectorCustomization.GetInspectorWindowAllInspectorsCall = ExpressionBuilder.BuildStaticGetFieldAsIList(InspectorWindowTypeName, "m_AllInspectors");
      EntityInspectorCustomization.GetEditorWindowParentHostViewCall =
        ExpressionBuilder.BuildInstanceGetFieldReturningExpressionInstance<EditorWindow>(HostViewTypeName, "m_Parent");
      EntityInspectorCustomization.GetGuiViewVisualTreeCall = ExpressionBuilder.BuildInstanceGetProperty<VisualElement>(GuiViewTypeName, "visualTree");

      var resultTuple = ExpressionBuilderUI.BuildAddRemoveContainsHierarchyChangedEventHandler();
      EntityInspectorCustomization.HierarchyChangedEventCalls.Add = resultTuple.Add;
      EntityInspectorCustomization.HierarchyChangedEventCalls.Remove = resultTuple.Remove;
      EntityInspectorCustomization.HierarchyChangedEventCalls.Contains = resultTuple.Contains;
      EntityInspectorCustomization.HierarchyChangedEventCalls.CreateHandler = ExpressionBuilderUI.BuildCreateHierarchyChangedEventHandler();

      EntityInspectorCustomization.ComponentElementType = typeof(ComponentElement<>).GetGenericTypeDefinition();
      EntityInspectorCustomization.BufferElementBaseType = typeof(BufferElement<,>).GetGenericTypeDefinition();

      EntityInspectorCustomization._inspectorHierarchyChangedEventHandler =
        EntityInspectorCustomization.HierarchyChangedEventCalls.CreateHandler(EntityInspectorCustomization.HandleInspectorElementHierarchyChanged);
    }

    /// <summary>
    /// Collapses all currently shown foldouts of the active entity in the entity inspector. The changes are applied to the
    /// category the entity belongs to.
    /// </summary>
    [MenuItem(EntityInspectorCustomization.CollapseActiveFoldoutsMenuItemName)]
    public static void CollapseActiveFoldouts()
    {
      if (EntityInspectorCustomization._currentInspectedEntity != Entity.Null)
      {
        EntityInspectorCustomization.ToggleActiveFoldouts(false);
      }
    }

    /// <summary>
    /// Collapses all foldouts for all categories and all components/buffers. This is a global operation.
    /// </summary>
    [MenuItem(EntityInspectorCustomization.CollapseGlobalFoldoutsMenuItemName)]
    public static void CollapseGlobalFoldouts()
    {
      EntityInspectorCustomization.ToggleGlobalFoldouts(false);
    }

    /// <summary>
    /// Defines the currently used category.
    /// </summary>
    /// <param name="inspectedEntitySelectionProxy">
    /// The currently inspected entity selection proxy containing entity relevant
    /// data.
    /// </param>
    private static void DefineCurrentCategory(EntitySelectionProxy inspectedEntitySelectionProxy)
    {
      string? categoryKey;

      if (EntityInspectorCustomization._generalState.IsCategorized)
      {
        var inspectorContext = EntityInspectorCustomization.FindEntityEditor()?.m_InspectorContext
          ?? throw new Exception("Could not find the entity editor inspector context.");

        categoryKey = EntityInspectorCustomization.DetermineCategoryKey(inspectedEntitySelectionProxy, inspectorContext);
      }
      else
      {
        categoryKey = null;
      }

      #if DEBUG_CUSTOMIZATION
      if (string.IsNullOrEmpty(categoryKey))
      {
        Debug.Log("New defined category is the DEFAULT");
      }
      else
      {
        Debug.Log($"New defined category key is '{categoryKey}'");
      }
      #endif

      if (string.IsNullOrEmpty(categoryKey))
      {
        EntityInspectorCustomization._currentCategoryState = EntityInspectorCustomization._defaultCategoryState;
      }
      else
      {
        CategoryStateData categoryStateData = new()
        {
          CategoryKey = categoryKey,
        };

        if (EntityInspectorCustomization._categoryStates.TryGetValue(categoryStateData, out var actualCategoryState))
        {
          EntityInspectorCustomization._currentCategoryState = actualCategoryState;
        }
        else
        {
          categoryStateData.SetToDefaults(EntityInspectorCustomization._generalState);
          EntityInspectorCustomization._categoryStates.Add(categoryStateData);
          EntityInspectorCustomization._currentCategoryState = categoryStateData;
        }
      }
    }

    /// <summary>
    /// Determine the category key which is used to define which set of foldout states, search string and scroll bar position
    /// is used.
    /// </summary>
    /// <param name="inspectedEntitySelectionProxy">
    /// The currently inspected entity selection proxy containing entity relevant
    /// data. Never null.
    /// </param>
    /// <param name="inspectorContext">
    /// The inspector context for the entity.
    /// </param>
    /// <returns>
    /// The case-sensitive category key. The exact same string identifies the same category.
    /// Null or empty string means falling back to the default category.
    /// </returns>
    private static string? DetermineCategoryKey(EntitySelectionProxy inspectedEntitySelectionProxy, EntityInspectorContext inspectorContext)
    {
      // NOTE:
      // Feel free to implement a custom category logic here for your needs.
      // This method is called one-time when an entity is selected and loaded in the Unity Inspector

      // Be aware that creating too much categories slows down the
      // serialization and deserialization process of the customization states, so may also prolong
      // recompilation/domain reload times.
      // Having more categories should not affect the performance of the inspector itself,
      // because of relying on hashsets.

      // Can also be used for determination of the category
      // var entity = inspectedEntitySelectionProxy.Entity;
      // var entityManager = inspectedEntitySelectionProxy.Container.EntityManager;
      // // Examples:
      // var entityName = entityManager.GetName(entity);
      // var hasLocalTransform = entityManager.HasComponent<LocalTransform>(entity);
      // var archetypeStableHash = entityManager.GetChunk(entity).Archetype.StableHash;
      // var entityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<LocalTransform>().Build(entityManager);
      // // Do determination
      // entityQuery.Dispose();

      // Currently the prefab is determined and used as category key, so each prefab
      // has its own category and settings. When the entity has no prefab then the default category is used.
      // The default category is the same category as when no categorization is enabled.

      var sourceObject = inspectorContext.GetSourceObject();

      if (sourceObject != null)
      {
        // Prefabs are having the game object set, so this is used as category key
        var prefabAssetType = PrefabUtility.GetPrefabAssetType(sourceObject);

        if (prefabAssetType != PrefabAssetType.NotAPrefab)
        {
          // Use name of the prefab
          return sourceObject.name;
        }
      }

      return null;
    }

    /// <summary>
    /// Tries to determine the main inspector.
    /// </summary>
    /// <param name="inspectorWindow">The editor window of the inspector.</param>
    /// <param name="inspectorRoot">The visual element root of the inspector.</param>
    /// <returns>True when inspector could be determined</returns>
    private static bool TryDetermineMainInspector(out EditorWindow inspectorWindow, out VisualElement inspectorRoot)
    {
      // While this calls does the job, it causes a creation of the Inspector (when not opened) and focuses the inspector
      // inspectorWindow = EditorWindow.GetWindow(EntityInspectorCustomization.UnityInspectorWindowType);
      // inspectorRoot = inspectorWindow?.rootVisualElement;

      // This would also work, but bad for performance
      // var inspectorWindowInstances = UnityEngine.Resources.FindObjectsOfTypeAll(EntityInspectorCustomization.UnityInspectorWindowType);

      // Probably most performant approach
      // Always returns a list instance of all known inspector windows
      var allInspectors = EntityInspectorCustomization.GetInspectorWindowAllInspectorsCall();

      if (allInspectors.Count > 0)
      {
        inspectorWindow = (EditorWindow)allInspectors[0];
        inspectorRoot = inspectorWindow.rootVisualElement;
        return true;
      }
      else
      {
        inspectorWindow = null!;
        inspectorRoot = null!;
        return false;
      }
    }

    /// <summary>
    /// Determine the unity project name.
    /// </summary>
    private static string DetermineUnityProjectName()
    {
      string[] splitDataPathParts = Application.dataPath.Split('/');

      if (splitDataPathParts.Length >= 2)
      {
        return splitDataPathParts[^2];
      }
      else
      {
        throw new Exception("Failed to determine the Unity project name. Entity Inspector Customization is not working properly.");
      }
    }

    /// <summary>
    /// Expands all currently shown foldouts of the active entity in the entity inspector. The changes are applied to the
    /// category the entity belongs to.
    /// </summary>
    [MenuItem(EntityInspectorCustomization.ExpandActiveFoldoutsMenuItemName)]
    public static void ExpandActiveFoldouts()
    {
      if (EntityInspectorCustomization._currentInspectedEntity != Entity.Null)
      {
        EntityInspectorCustomization.ToggleActiveFoldouts(true);
      }
    }

    /// <summary>
    /// Expands all foldouts for all categories and all components/buffers. This is a global operation.
    /// </summary>
    [MenuItem(EntityInspectorCustomization.ExpandGlobalFoldoutsMenuItemName)]
    public static void ExpandGlobalFoldouts()
    {
      EntityInspectorCustomization.ToggleGlobalFoldouts(true);
    }

    /// <summary>
    /// Finds the first entity editor instance.
    /// </summary>
    /// <returns>The editor instance or null when not found.</returns>
    private static EntityEditor? FindEntityEditor()
    {
      var tracker = ActiveEditorTracker.sharedTracker;

      for (var i = 0; i < tracker.activeEditors.Length; i++)
      {
        var editor = tracker.activeEditors[i];

        if (editor != null)
        {
          if (editor is EntityEditor entityEditor)
          {
            return entityEditor;
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Gets the name of the component of a foldout element.
    /// </summary>
    /// <param name="foldout">The foldout element of a component.</param>
    /// <returns>The name of the component without spaces.</returns>
    private static string? GetComponentName(Foldout? foldout)
    {
      var componentNameLabel = foldout?.Q<Label>("ComponentName", "component-name");
      return componentNameLabel?.text.Replace(" ", string.Empty);
    }

    /// <summary>
    /// Handles the change of the foldout value of a foldout element.
    /// </summary>
    private static void HandleComponentFoldoutValueChanged(ChangeEvent<bool> changeEvent)
    {
      var foldout = changeEvent.currentTarget as Foldout;
      var componentName = EntityInspectorCustomization.GetComponentName(foldout);

      if (!string.IsNullOrWhiteSpace(componentName))
      {
        EntityInspectorCustomization.ModifyComponentFoldoutState(componentName, changeEvent.newValue);
      }
    }

    /// <summary>
    /// Handles the delay call for updating the menu items checked states.
    /// </summary>
    private static void HandleEditorDelayCallForUpdateMenuItemsChecked()
    {
      // Delay call is only called once according to the docs
      //EditorApplication.delayCall -= EntityInspectorCustomization.HandleEditorDelayCallForUpdateMenuItemsChecked;

      EntityInspectorCustomization.UpdateMenuItemsChecked();
    }

    /// <summary>
    /// Handles the change of the visual element hierarchy of the inspector.
    /// </summary>
    private static void HandleInspectorElementHierarchyChanged(VisualElement visualElement, ExpressionBuilderUI.CustomHierarchyChangeType changeType)
    {
      const string EntityInspectorElementName = "Entity Inspector";

      switch (changeType)
      {
        case ExpressionBuilderUI.CustomHierarchyChangeType.Remove:
        {
          var childEntityInspectorElement = visualElement.Q<BindableElement>(EntityInspectorElementName);

          // Check for removal of the Entity Inspector (happens on changing the selection)
          if (childEntityInspectorElement != null)
          {
            EntityInspectorCustomization.UnregisterEntitySpecificInspectorEvents();
            EntityInspectorCustomization._currentInspectedEntity = Entity.Null;
          }

          break;
        }
        case ExpressionBuilderUI.CustomHierarchyChangeType.Add:
        {
          // Get entity selection proxy
          var entitySelectionProxy = Selection.activeObject as EntitySelectionProxy ?? Selection.activeContext as EntitySelectionProxy;

          // DO NOT handle classic game objects, assets and the like.
          // Also only when the entity has not already been considered inspected by the customization.
          if (entitySelectionProxy == null || EntityInspectorCustomization._currentInspectedEntity == entitySelectionProxy.Entity)
          {
            return;
          }

          // Find the relevant bindable element containing the Entity Inspector element
          var entityInspectorElement = visualElement.FindParent<BindableElement>(EntityInspectorElementName)
            // When using a docked Inspector with tabs the Entity Inspector element is to be found as child instead as parent  
            ?? visualElement.Q<BindableElement>(EntityInspectorElementName);

          if (entityInspectorElement != null)
          {
            var componentElementQuery = entityInspectorElement.Query<ComponentElementBase>().Build();

            if (componentElementQuery.Any())
            {
              #if DEBUG_CUSTOMIZATION_DETAILED
              Debug.LogWarning($"Registering events for entity '{entitySelectionProxy.Entity}'");
              #endif

              // Mark the entity as inspected
              EntityInspectorCustomization._currentInspectedEntity = entitySelectionProxy.Entity;

              // Define the current category
              EntityInspectorCustomization.DefineCurrentCategory(entitySelectionProxy);

              // --- Foldout adjustments ---

              // Adjust the tags foldout
              const string TagsFoldoutClassName = "component-header";
              var tagComponentContainer = entityInspectorElement.Q<TagComponentContainer>(className: TagsFoldoutClassName);
              var tagComponentContainerFoldout = tagComponentContainer?.Q<Foldout>(className: TagsFoldoutClassName);

              if (tagComponentContainerFoldout != null)
              {
                tagComponentContainerFoldout.value = EntityInspectorCustomization._currentCategoryState.IsTagsFoldoutExpanded;
                tagComponentContainerFoldout.RegisterCallback<ChangeEvent<bool>>(EntityInspectorCustomization.HandleTagsFoldoutValueChanged);
                EntityInspectorCustomization._registeredTagsFoldout = tagComponentContainerFoldout;
              }

              // Adjust component and buffer foldouts
              foreach (var componentElement in componentElementQuery)
              {
                var componentType = componentElement.GetType();

                if (componentType.IsGenericType)
                {
                  var componentTypeGenericDefinition = componentType.GetGenericTypeDefinition();

                  if (componentTypeGenericDefinition == EntityInspectorCustomization.ComponentElementType
                      || componentTypeGenericDefinition == EntityInspectorCustomization.BufferElementBaseType)
                  {
                    var foldoutElement = componentElement.Q<Foldout>();

                    var componentName = EntityInspectorCustomization.GetComponentName(foldoutElement);

                    if (!string.IsNullOrWhiteSpace(componentName))
                    {
                      foldoutElement.value = EntityInspectorCustomization.IsComponentFoldout(componentName);

                      // Register callback for change of the foldout value
                      foldoutElement.RegisterCallback<ChangeEvent<bool>>(EntityInspectorCustomization.HandleComponentFoldoutValueChanged, TrickleDown.TrickleDown);

                      // Remember the foldout element for unregistering the callback later
                      EntityInspectorCustomization._registeredComponentFoldouts.Add(foldoutElement);
                    }
                  }
                }
              }

              // --- Additional options ---
              // (To minimize performance impact, a reload of the Inspector is used in case an option is enabled/disabled while an entity is inspected)

              // Adjust for preserve search option when enabled
              if (EntityInspectorCustomization._generalState.IsSearchPreserved)
              {
                var searchElement = entityInspectorElement.Q<SearchElement>("Components Search");
                var searchTextField = searchElement?.Q<TextField>("search-element-text-field-search-string");
                var searchElementCancelButton = searchElement?.Q<Button>("search-element-cancel-button");

                if (searchTextField != null && searchElementCancelButton != null)
                {
                  searchTextField.value = EntityInspectorCustomization._currentCategoryState.SearchString;

                  searchTextField.RegisterCallback<InputEvent>(EntityInspectorCustomization.HandleSearchStringChanged);

                  // Cancel button click does not trigger the InputEvent, so we need to register to the clicked event
                  searchElementCancelButton.clicked += EntityInspectorCustomization.HandleSearchCancelledClicked;

                  EntityInspectorCustomization._registeredSearchTextField = searchTextField;
                  EntityInspectorCustomization._registeredSearchCancelButton = searchElementCancelButton;
                }
                #if DEBUG_CUSTOMIZATION
                else
                {
                  Debug.LogError("Could not find search elements in the entity inspector.");
                }
                #endif
              }

              // Adjust scrollbar position when enabled and not the default category
              var isApplyingScrollPositionPreservation = EntityInspectorCustomization._generalState.IsCategoryScrollPositionPreserved
                && !string.IsNullOrEmpty(EntityInspectorCustomization._currentCategoryState.CategoryKey);
              var isApplyingAlwaysShowEntityHeader = EntityInspectorCustomization._generalState.IsEntityHeaderAlwaysShown;

              if (isApplyingScrollPositionPreservation || isApplyingAlwaysShowEntityHeader)
              {
                var scrollViewElement = EntityInspectorCustomization._inspectorRoot.Q<ScrollView>(className: "unity-inspector-root-scrollview");
                var verticalScrollerElement = scrollViewElement?.Q<Scroller>(className: "unity-scroller--vertical");
                var scrollContainer = scrollViewElement?.Q<VisualElement>("unity-content-container");

                // This has to be done in both cases, because the vertical scroller is always present
                if (verticalScrollerElement != null && scrollContainer != null)
                {
                  if (isApplyingScrollPositionPreservation)
                  {
                    EntityInspectorCustomization._registeredVerticalScroller = verticalScrollerElement;
                  }

                  if (isApplyingAlwaysShowEntityHeader)
                  {
                    var entityHeaderElement = entityInspectorElement.Q<PropertyElement>("EntityHeader");
                    EntityInspectorCustomization._registeredEntityHeaderElement = entityHeaderElement;

                    #if DEBUG_CUSTOMIZATION
                    if (entityHeaderElement == null)
                    {
                      Debug.LogError("Could not find the entity header in the entity inspector.");
                    }
                    #endif
                  }

                  // The event is used to determine the moment before redraw of the Inspector to adjust the scroll position as well as the entity header.
                  // Unregistering is done when the event is triggered.
                  scrollContainer.RegisterCallback<GeometryChangedEvent>(EntityInspectorCustomization.HandleScrollContainerGeometryChanged);

                  // This handler also in both cases to remember the scroll position and to adjust the entity header
                  verticalScrollerElement.RegisterCallback<ChangeEvent<float>>(EntityInspectorCustomization.HandleVerticalScrollerPositionChanged);
                }
                #if DEBUG_CUSTOMIZATION
                else
                {
                  if (scrollContainer == null)
                  {
                    Debug.LogError("Could not find the vertical scroller in the entity inspector.");
                  }

                  if (scrollContainer == null)
                  {
                    Debug.LogError("Could not find the scroll container in the entity inspector.");
                  }
                }
                #endif
              }
            }
          }

          break;
        }
      }
    }

    /// <summary>
    /// Handles the selection change of the Unity Inspector.
    /// </summary>
    private static void HandleInspectorSelectionChanged()
    {
      // Find the inspector root element of the main inspector window
      if (EntityInspectorCustomization.TryDetermineMainInspector(out var inspectorWindow, out var inspectorRoot))
      {
        EntityInspectorCustomization.AttachToInspectorWhenNotAttached(inspectorWindow, inspectorRoot);
      }
      else
      {
        EntityInspectorCustomization.DetachFromInspector();
      }
    }

    /// <summary>
    /// Attaches to the inspector window allowing for adjusting the entity related visual elements.
    /// </summary>
    private static void AttachToInspectorWhenNotAttached(EditorWindow inspectorWindow, VisualElement inspectorRoot)
    {
      ExpressionInstance? typedPanelInstance;

      if (inspectorWindow.docked)
      {
        // When docked the typed panel instance has to be determined from the hosting window
        var hostViewParent = EntityInspectorCustomization.GetEditorWindowParentHostViewCall(inspectorWindow);
        var visualTree = EntityInspectorCustomization.GetGuiViewVisualTreeCall(hostViewParent);
        typedPanelInstance = EntityInspectorCustomization.CastToBaseVisualElementPanelCall(visualTree.panel);
      }
      else
      {
        // When not docked the panel is directly found in the inspector root
        typedPanelInstance = EntityInspectorCustomization.CastToBaseVisualElementPanelCall(inspectorRoot.panel);
      }

      // Determine the panel instance of the inspector and check whether the event handler is already added to the inspector
      var isEventHandlerAdded =
        EntityInspectorCustomization.HierarchyChangedEventCalls.Contains(typedPanelInstance, EntityInspectorCustomization._inspectorHierarchyChangedEventHandler);

      if (!isEventHandlerAdded)
      {
        // This may happen in case of docking of the inspector window
        EntityInspectorCustomization.DetachFromInspector();

        #if DEBUG_CUSTOMIZATION_DETAILED
        Debug.LogWarning($"Attaching to inspector window with instance ID '{inspectorWindow.GetInstanceID()}'");
        #endif

        // Add event handler to inspector and remember the panel instance
        EntityInspectorCustomization._inspectorWindow = inspectorWindow;
        EntityInspectorCustomization._inspectorRoot = inspectorRoot;
        EntityInspectorCustomization._inspectorTypedPanelInstance = typedPanelInstance;
        EntityInspectorCustomization.HierarchyChangedEventCalls.Add(typedPanelInstance, EntityInspectorCustomization._inspectorHierarchyChangedEventHandler);
      }
    }

    /// <summary>
    /// Detaches all events from the inspector to which has been attached.
    /// </summary>
    private static void DetachFromInspector()
    {
      // To be clean unregister events and remove the event handler from the outdated panel instance
      if (EntityInspectorCustomization._inspectorTypedPanelInstance != null)
      {
        #if DEBUG_CUSTOMIZATION_DETAILED
        Debug.LogWarning($"Detaching from inspector window with instance ID '{EntityInspectorCustomization._inspectorWindow?.GetInstanceID()}'");
        #endif

        EntityInspectorCustomization.UnregisterEntitySpecificInspectorEvents();
        EntityInspectorCustomization._currentInspectedEntity = Entity.Null;

        EntityInspectorCustomization.HierarchyChangedEventCalls.Remove(EntityInspectorCustomization._inspectorTypedPanelInstance,
          EntityInspectorCustomization._inspectorHierarchyChangedEventHandler);

        EntityInspectorCustomization._inspectorTypedPanelInstance = null;
        EntityInspectorCustomization._inspectorWindow = null;
        EntityInspectorCustomization._inspectorRoot = null;
      }
    }

    /// <summary>
    /// Handles the change of the geometry of the scroll container having all the entity data.
    /// </summary>
    private static void HandleScrollContainerGeometryChanged(GeometryChangedEvent changeEvent)
    {
      const string EntityHeaderPlaceholderName = "EntityHeaderPlaceholder";

      ((CallbackEventHandler)changeEvent.target).UnregisterCallback<GeometryChangedEvent>(EntityInspectorCustomization.HandleScrollContainerGeometryChanged);

      if (EntityInspectorCustomization._registeredEntityHeaderElement != null
          // Because when in docked mode, the event can be called multiple times, so query the placeholder to determine whether the entity header is already in place 
          && EntityInspectorCustomization._registeredEntityHeaderElement.parent.Q<VisualElement>(EntityHeaderPlaceholderName) == null)
      {
        var placeholderElement = new VisualElement
        {
          name = EntityHeaderPlaceholderName,
          style =
          {
            height = EntityInspectorCustomization._registeredEntityHeaderElement.layout.height + 4 /* cause of separator */,
          },
        };

        EntityInspectorCustomization._registeredEntityHeaderElement.parent.Add(placeholderElement);
        placeholderElement.SendToBack();

        EntityInspectorCustomization._registeredEntityHeaderElement.BringToFront();
        EntityInspectorCustomization._registeredEntityHeaderElement.style.position = Position.Absolute;
        EntityInspectorCustomization._registeredEntityHeaderElement.style.left = 0f;
        EntityInspectorCustomization._registeredEntityHeaderElement.style.right = 0f;
        EntityInspectorCustomization._registeredEntityHeaderElement.style.backgroundColor = EntityInspectorCustomization._defaultWindowBackgroundColor;

        // Add a separator line for better distinguishing the entity header from the components                 
        EntityInspectorCustomization._registeredEntityHeaderElement.Add(new VisualElement
        {
          style =
          {
            height = 2,
            backgroundColor = EntityInspectorCustomization._separatorLineColor,
          },
        });
      }

      if (EntityInspectorCustomization._registeredVerticalScroller != null)
      {
        EntityInspectorCustomization._registeredVerticalScroller.value = EntityInspectorCustomization._currentCategoryState.VerticalScrollPosition;
      }
    }

    /// <summary>
    /// Handles the search cancelled button.
    /// </summary>
    private static void HandleSearchCancelledClicked()
    {
      if (!string.IsNullOrEmpty(EntityInspectorCustomization._currentCategoryState.SearchString))
      {
        EntityInspectorCustomization._currentCategoryState.SearchString = string.Empty;
        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Handles the change of the search string.
    /// </summary>
    private static void HandleSearchStringChanged(InputEvent inputEvent)
    {
      if (EntityInspectorCustomization._currentCategoryState.SearchString != inputEvent.newData)
      {
        EntityInspectorCustomization._currentCategoryState.SearchString = inputEvent.newData;
        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Handles the change of the tags foldout value.
    /// </summary>
    private static void HandleTagsFoldoutValueChanged(ChangeEvent<bool> changeEvent)
    {
      if (EntityInspectorCustomization._currentCategoryState.IsTagsFoldoutExpanded != changeEvent.newValue)
      {
        EntityInspectorCustomization._currentCategoryState.IsTagsFoldoutExpanded = changeEvent.newValue;
        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Handles the change of the vertical scrollbar position of the inspector when the
    /// <see cref="GeneralStateData.IsCategoryScrollPositionPreserved" /> is enabled.
    /// </summary>
    private static void HandleVerticalScrollerPositionChanged(ChangeEvent<float> changeEvent)
    {
      if (EntityInspectorCustomization._registeredEntityHeaderElement != null)
      {
        EntityInspectorCustomization._registeredEntityHeaderElement.style.top = changeEvent.newValue;
      }

      // Only detect changes when the value is a bit different
      if (EntityInspectorCustomization._registeredVerticalScroller != null
          // Ignore a change when the high value is below zero, indicating that the inspector is clearing its visual element content,
          // otherwise a reset to zero would be detected as a change then
          && EntityInspectorCustomization._registeredVerticalScroller.highValue >= 0
          && !Mathf.Approximately(Mathf.Abs(EntityInspectorCustomization._currentCategoryState.VerticalScrollPosition - changeEvent.newValue), 0f)
         )
      {
        EntityInspectorCustomization._currentCategoryState.VerticalScrollPosition = changeEvent.newValue;
        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Initializes the entity inspector customization, only subscribing to the inspector selection change event.
    /// </summary>
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
      EntityInspectorCustomization._unityProjectName = EntityInspectorCustomization.DetermineUnityProjectName();
      EntityInspectorCustomization.LoadStates();

      EntityInspectorCustomization._nextAutoSaveTime = EditorApplication.timeSinceStartup + EntityInspectorCustomization.AutoSaveIntervalSeconds;

      if (EditorGUIUtility.isProSkin)
      {
        // Dark theme
        EntityInspectorCustomization._defaultWindowBackgroundColor = new Color32(56, 56, 56, 255);
        EntityInspectorCustomization._separatorLineColor = new Color32(21, 21, 21, 255);
      }
      else
      {
        // Light theme
        EntityInspectorCustomization._defaultWindowBackgroundColor = new Color32(194, 194, 194, 255);
        EntityInspectorCustomization._separatorLineColor = new Color32(102, 102, 102, 255);
      }

      // This is a workaround, because the code below does not work when the Unity Project is initially loaded, only after recompile/domain reload
      EditorApplication.delayCall += EntityInspectorCustomization.HandleEditorDelayCallForUpdateMenuItemsChecked;
      //EntityInspectorCustomization.UpdateMenuItemsChecked();

      // Subscribe to certain event for all the time
      EditorApplication.update += EntityInspectorCustomization.UpdateEditor;
      EditorApplication.quitting += EntityInspectorCustomization.SaveStatesWhenChanged;
      CompilationPipeline.compilationStarted += _ => { EntityInspectorCustomization.SaveStatesWhenChanged(); };
      Selection.selectionChanged += EntityInspectorCustomization.HandleInspectorSelectionChanged;
    }

    /// <summary>
    /// Gets the component state.
    /// </summary>
    /// <param name="componentName">The name of the component.</param>
    /// <returns>True when the component is foldout, so fully shown.</returns>
    private static bool IsComponentFoldout(string componentName)
    {
      return EntityInspectorCustomization._currentCategoryState.ToggledFoldoutComponentNames.TryGetValue(componentName, out _)
        != EntityInspectorCustomization._generalState.IsGlobalFoldoutExpanded;
    }

    /// <summary>
    /// Loads the states of the entity inspector.
    /// </summary>
    private static void LoadStates()
    {
      try
      {
        var json = EditorPrefs.GetString($"{EntityInspectorCustomization._unityProjectName}/{EntityInspectorCustomization.FoldoutStatesEditorPrefsName}", "");
        var serializationContainer = JsonUtility.FromJson<SerializationContainer>(json);
        serializationContainer?.PostSerialize();

        EntityInspectorCustomization._generalState = serializationContainer?.GeneralState ?? new();
        EntityInspectorCustomization._defaultCategoryState = serializationContainer?.DefaultCategoryState ?? new();
        EntityInspectorCustomization._categoryStates = new HashSet<CategoryStateData>(serializationContainer?.CategoryStates ?? Array.Empty<CategoryStateData>());
      }
      catch (Exception exception)
      {
        Debug.LogError($"Failed to load states for entity inspector customization. Resetting the states! Exception: {exception.Message}");

        EntityInspectorCustomization._generalState = new();
        EntityInspectorCustomization._generalState.SetToDefaults();
        EntityInspectorCustomization._defaultCategoryState = new();
        EntityInspectorCustomization._defaultCategoryState.SetToDefaults(EntityInspectorCustomization._generalState);
        EntityInspectorCustomization._categoryStates.Clear();

        EditorPrefs.DeleteKey(EntityInspectorCustomization.FoldoutStatesEditorPrefsName);
      }

      EntityInspectorCustomization._currentCategoryState = EntityInspectorCustomization._defaultCategoryState;
    }

    /// <summary>
    /// Modifies the component state.
    /// </summary>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="isFoldoutExpanded">The new foldout expanded value.</param>
    private static void ModifyComponentFoldoutState(string componentName, bool isFoldoutExpanded)
    {
      if (isFoldoutExpanded != EntityInspectorCustomization._generalState.IsGlobalFoldoutExpanded)
      {
        // It is expected that the component state is not yet added to the category state
        var hasAdded = EntityInspectorCustomization._currentCategoryState.ToggledFoldoutComponentNames.Add(componentName);

        if (hasAdded)
        {
          EntityInspectorCustomization._hasUnsavedChanges = true;
        }
      }
      else
      {
        // When matching the global state, remove the foldout state
        EntityInspectorCustomization._hasUnsavedChanges |= EntityInspectorCustomization._currentCategoryState.ToggledFoldoutComponentNames.Remove(componentName);
      }
    }

    /// <summary>
    /// Reloads the currently shown entity in the inspector when an entity is selected
    /// </summary>
    private static void ReloadInspector()
    {
      var entitySelectionProxy = Selection.activeObject as EntitySelectionProxy ?? Selection.activeContext as EntitySelectionProxy;

      // Skip when no entity is selected
      if (entitySelectionProxy != null)
      {
        // This call triggers a reload of the inspector
        EntityInspectorCustomization.EditorSetSelectionCall(Selection.activeObject, EntitySelectionProxy.CreateInstance(entitySelectionProxy.World, entitySelectionProxy.Entity),
          DataMode.Runtime);
      }
    }

    #if DEBUG_CUSTOMIZATION
    /// <summary>
    /// Resets the customization and deletes all settings.
    /// </summary>
    /// <remarks>
    /// Used for debugging and testing purposes.
    /// </remarks>
    [MenuItem(EntityInspectorCustomization.BaseMenuPath + "/Debug: Reset And Delete Settings")]
    public static void ResetAndDeleteSettings()
    {
      if (!EntityInspectorCustomization.ShowUserConfirmationDialog("Reset And Delete Settings"))
      {
        return;
      }

      EntityInspectorCustomization._generalState = new();
      EntityInspectorCustomization._generalState.SetToDefaults();
      EntityInspectorCustomization._defaultCategoryState.SetToDefaults(EntityInspectorCustomization._generalState);
      EntityInspectorCustomization._categoryStates.Clear();

      EntityInspectorCustomization.SaveStates();
      EntityInspectorCustomization.UpdateMenuItemsChecked();
      EntityInspectorCustomization.ReloadInspector();
    }
    #endif

    #if DEBUG_CUSTOMIZATION
    /// <summary>
    /// Saves the settings and copies the JSON to the clipboard.
    /// </summary>
    /// <remarks>
    /// Used for debugging and testing purposes.
    /// </remarks>
    [MenuItem(EntityInspectorCustomization.BaseMenuPath + "/Debug: Save And Copy Settings")]
    public static void SaveAndCopySettings()
    {
      EntityInspectorCustomization.SaveStates();

      var json = EditorPrefs.GetString($"{EntityInspectorCustomization._unityProjectName}/{EntityInspectorCustomization.FoldoutStatesEditorPrefsName}", "");
      EditorGUIUtility.systemCopyBuffer = json;
    }
    #endif

    /// <summary>
    /// Saves the states of the entity inspector.
    /// </summary>
    private static void SaveStates()
    {
      #if DEBUG_CUSTOMIZATION_DETAILED
      Debug.LogWarning("Saving entity inspector customization states.");
      #endif

      var serializationContainer = new SerializationContainer()
      {
        GeneralState = EntityInspectorCustomization._generalState,
        DefaultCategoryState = EntityInspectorCustomization._defaultCategoryState,
        CategoryStates = EntityInspectorCustomization._categoryStates.ToArray(),
      };

      serializationContainer.PreSerialize();
      var json = JsonUtility.ToJson(serializationContainer, false);

      EditorPrefs.SetString($"{EntityInspectorCustomization._unityProjectName}/{EntityInspectorCustomization.FoldoutStatesEditorPrefsName}", json);
      EntityInspectorCustomization._hasUnsavedChanges = false;
    }

    /// <summary>
    /// Saves the states of the entity inspector when changes happened.
    /// </summary>
    private static void SaveStatesWhenChanged()
    {
      if (EntityInspectorCustomization._hasUnsavedChanges)
      {
        EntityInspectorCustomization.SaveStates();
      }
    }

    /// <summary>
    /// Shows a confirmation dialog to the user.
    /// </summary>
    /// <param name="actionName">Name of the action to be executed.</param>
    /// <returns>True when the user confirmed the action.</returns>
    private static bool ShowUserConfirmationDialog(string actionName)
    {
      return !EditorUtility.DisplayDialog("Confirmation",
        $"Are you sure you want to perform '{actionName}'? This will affect all data of the Entity Inspector Customization. Undo not possible.", "Cancel", "Proceed");
    }

    /// <summary>
    /// Toggles only the currently shown foldouts of the entity inspector of the active entity as well as in the maintained
    /// states.
    /// </summary>
    /// <param name="foldoutValue">True when all foldout should be expanded, otherwise false.</param>
    private static void ToggleActiveFoldouts(bool foldoutValue)
    {
      // Adjust all known component foldouts
      EntityInspectorCustomization._currentCategoryState.IsTagsFoldoutExpanded = foldoutValue;

      // Adjust currently displayed foldouts in the Entity Inspector
      if (EntityInspectorCustomization._registeredTagsFoldout != null)
      {
        EntityInspectorCustomization._registeredTagsFoldout.value = foldoutValue;
      }

      foreach (var foldout in EntityInspectorCustomization._registeredComponentFoldouts)
      {
        var componentName = EntityInspectorCustomization.GetComponentName(foldout);

        if (!string.IsNullOrWhiteSpace(componentName))
        {
          EntityInspectorCustomization.ModifyComponentFoldoutState(componentName, foldoutValue);
        }

        foldout.value = foldoutValue;
      }

      EntityInspectorCustomization._hasUnsavedChanges = true;
    }

    /// <summary>
    /// Specifies whether to keep the vertical scrollbar position in the entity inspector for the next selected entity when a
    /// concrete category other than default is applied.
    /// When not enabled the vertical scrollbar position resets every time on changing the entity.
    /// </summary>
    [MenuItem(EntityInspectorCustomization.CategoryPreservedScrollPositionMenuItemName)]
    public static void ToggleCategoryScrollPositionPreservation()
    {
      EntityInspectorCustomization.ToggleCategoryScrollPositionPreservation(!EntityInspectorCustomization._generalState.IsCategoryScrollPositionPreserved);

      if (EntityInspectorCustomization._generalState.IsCategoryScrollPositionPreserved)
      {
        EntityInspectorCustomization.ReloadInspector();
      }
    }

    /// <summary>
    /// Toggles the category scroll position preserval option in the entity inspector customization when a concrete category
    /// other than default is applied.
    /// </summary>
    /// <param name="isEnabled">True when the vertical scroll position should be kept for the next selected entity.</param>
    private static void ToggleCategoryScrollPositionPreservation(bool isEnabled)
    {
      if (EntityInspectorCustomization._generalState.IsCategoryScrollPositionPreserved != isEnabled)
      {
        EntityInspectorCustomization._generalState.IsCategoryScrollPositionPreserved = isEnabled;
        EntityInspectorCustomization.UpdateMenuItemsChecked();

        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Specifies whether to always show the entity header in the inspector at the top area.
    /// When not enabled the entity will not be seen anymore when scrolling down (default behavior).
    /// </summary>
    [MenuItem(EntityInspectorCustomization.EntityHeaderAlwaysShownMenuItemName)]
    public static void ToggleEntityHeaderAlwaysShow()
    {
      EntityInspectorCustomization.ToggleEntityHeaderAlwaysShow(!EntityInspectorCustomization._generalState.IsEntityHeaderAlwaysShown);
      EntityInspectorCustomization.ReloadInspector();
    }

    /// <summary>
    /// Toggles the entity header always shown option in the entity inspector customization.
    /// </summary>
    /// <param name="isEnabled">
    /// True when the entity header should also be seen when scrolling down in the inspector.
    /// </param>
    private static void ToggleEntityHeaderAlwaysShow(bool isEnabled)
    {
      if (EntityInspectorCustomization._generalState.IsEntityHeaderAlwaysShown != isEnabled)
      {
        EntityInspectorCustomization._generalState.IsEntityHeaderAlwaysShown = isEnabled;
        EntityInspectorCustomization.UpdateMenuItemsChecked();

        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Toggles all foldouts of the entity inspector as well as in the maintained states.
    /// </summary>
    /// <param name="foldoutValue">True when all foldout should be expanded, otherwise false.</param>
    private static void ToggleGlobalFoldouts(bool foldoutValue)
    {
      // Because of the potential impact on all foldouts, show a confirmation dialog
      if (!EntityInspectorCustomization.ShowUserConfirmationDialog(foldoutValue ? "Global: Expand Foldouts" : "Global: Collapse Foldouts"))
      {
        return;
      }

      // Adjust general state to affect components not known yet
      EntityInspectorCustomization._generalState.IsGlobalFoldoutExpanded = foldoutValue;

      // Reset all foldout states of all categories (they are not needed when defining a new global foldout value)
      EntityInspectorCustomization._defaultCategoryState.IsTagsFoldoutExpanded = foldoutValue;
      EntityInspectorCustomization._defaultCategoryState.ToggledFoldoutComponentNames.Clear();

      foreach (var categoryState in EntityInspectorCustomization._categoryStates.ToList())
      {
        categoryState.IsTagsFoldoutExpanded = foldoutValue;
        categoryState.ToggledFoldoutComponentNames.Clear();

        // When category state matches the defaults, then the category state is not needed and removed
        if (categoryState.HasDefaults(EntityInspectorCustomization._generalState))
        {
          EntityInspectorCustomization._categoryStates.Remove(categoryState);
        }
      }

      EntityInspectorCustomization._hasUnsavedChanges = true;

      // Adjust currently displayed foldouts in the Entity Inspector
      if (EntityInspectorCustomization._registeredTagsFoldout != null)
      {
        EntityInspectorCustomization._registeredTagsFoldout.value = foldoutValue;
      }

      foreach (var foldout in EntityInspectorCustomization._registeredComponentFoldouts)
      {
        foldout.value = foldoutValue;
      }
    }

    /// <summary>
    /// Specifies whether to keep the search text in the entity inspector for the next selected entity.
    /// When not enabled the search text is cleared every time a new entity is selected (default behavior).
    /// </summary>
    [MenuItem(EntityInspectorCustomization.PreserveSearchMenuItemName)]
    public static void ToggleSearchStringPreservation()
    {
      EntityInspectorCustomization.ToggleSearchStringPreservation(!EntityInspectorCustomization._generalState.IsSearchPreserved);

      if (EntityInspectorCustomization._generalState.IsSearchPreserved)
      {
        EntityInspectorCustomization.ReloadInspector();
      }
    }

    /// <summary>
    /// Toggles the preserve search option in the entity inspector customization.
    /// </summary>
    /// <param name="isEnabled">True when the search text should be kept for the next selected entity.</param>
    private static void ToggleSearchStringPreservation(bool isEnabled)
    {
      if (EntityInspectorCustomization._generalState.IsSearchPreserved != isEnabled)
      {
        EntityInspectorCustomization._generalState.IsSearchPreserved = isEnabled;
        EntityInspectorCustomization.UpdateMenuItemsChecked();

        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Specifies whether to keep the search text in the entity inspector for the next selected entity.
    /// When not enabled the search text is cleared every time a new entity is selected (default behavior).
    /// </summary>
    [MenuItem(EntityInspectorCustomization.UseCategoriesMenuItemName)]
    public static void ToggleUseCategories()
    {
      EntityInspectorCustomization.ToggleUseCategories(!EntityInspectorCustomization._generalState.IsCategorized);
      EntityInspectorCustomization.ReloadInspector();
    }

    /// <summary>
    /// Toggles the use categories option in the entity inspector customization.
    /// </summary>
    /// <param name="isEnabled">
    /// True when categories should be used to get fine-control over foldout states in each category
    /// instead for all entities.
    /// </param>
    private static void ToggleUseCategories(bool isEnabled)
    {
      if (EntityInspectorCustomization._generalState.IsCategorized != isEnabled)
      {
        EntityInspectorCustomization._generalState.IsCategorized = isEnabled;
        EntityInspectorCustomization.UpdateMenuItemsChecked();

        EntityInspectorCustomization._hasUnsavedChanges = true;
      }
    }

    /// <summary>
    /// Unregisters all inspector events which are related to the current selected entity.
    /// </summary>
    private static void UnregisterEntitySpecificInspectorEvents()
    {
      #if DEBUG_CUSTOMIZATION_DETAILED
      Debug.LogWarning($"Unregistering events for current entity '{EntityInspectorCustomization._currentInspectedEntity}'");
      #endif

      // Unregister change events for tags foldout
      EntityInspectorCustomization._registeredTagsFoldout?.UnregisterCallback<ChangeEvent<bool>>(EntityInspectorCustomization.HandleTagsFoldoutValueChanged);
      EntityInspectorCustomization._registeredTagsFoldout = null;

      // Unregister all change events for component/buffer foldouts
      foreach (var registeredFoldout in EntityInspectorCustomization._registeredComponentFoldouts)
      {
        registeredFoldout.UnregisterCallback<ChangeEvent<bool>>(EntityInspectorCustomization.HandleComponentFoldoutValueChanged);
      }

      EntityInspectorCustomization._registeredComponentFoldouts.Clear();

      // Unregister change event for search text element and search cancellation
      EntityInspectorCustomization._registeredSearchTextField?.UnregisterCallback<InputEvent>(EntityInspectorCustomization.HandleSearchStringChanged);
      EntityInspectorCustomization._registeredSearchTextField = null;

      if (EntityInspectorCustomization._registeredSearchCancelButton != null)
      {
        EntityInspectorCustomization._registeredSearchCancelButton.clicked -= EntityInspectorCustomization.HandleSearchCancelledClicked;
        EntityInspectorCustomization._registeredSearchCancelButton = null;
      }

      // Unregister scroll change event
      EntityInspectorCustomization._registeredVerticalScroller?.UnregisterCallback<ChangeEvent<float>>(EntityInspectorCustomization.HandleVerticalScrollerPositionChanged);
      EntityInspectorCustomization._registeredVerticalScroller = null;

      //  Unregister entity header element
      EntityInspectorCustomization._registeredEntityHeaderElement = null;
    }

    /// <summary>
    /// Called regularly for editor update.
    /// </summary>
    private static void UpdateEditor()
    {
      // Only save on changes and periodically
      if (EditorApplication.timeSinceStartup > EntityInspectorCustomization._nextAutoSaveTime)
      {
        EntityInspectorCustomization._nextAutoSaveTime = EditorApplication.timeSinceStartup + EntityInspectorCustomization.AutoSaveIntervalSeconds;
        EntityInspectorCustomization.SaveStatesWhenChanged();
      }

      // Unfortunately this kind of polling is necessary
      // There is no event in Unity for informing about an opened EditorWindow/Inspector
      // so the customization can only be attached to an opened Inspector when it has been detected here
      if ((EntityInspectorCustomization._inspectorWindow == null || !EntityInspectorCustomization._inspectorWindow)
          && EntityInspectorCustomization.TryDetermineMainInspector(out var inspectorWindow, out var inspectorRoot))
      {
        EntityInspectorCustomization.AttachToInspectorWhenNotAttached(inspectorWindow, inspectorRoot);
      }
    }

    /// <summary>
    /// Updates the checked states of all relevant menu options.
    /// </summary>
    private static void UpdateMenuItemsChecked()
    {
      Menu.SetChecked(EntityInspectorCustomization.CategoryPreservedScrollPositionMenuItemName, EntityInspectorCustomization._generalState.IsCategoryScrollPositionPreserved);
      Menu.SetChecked(EntityInspectorCustomization.EntityHeaderAlwaysShownMenuItemName, EntityInspectorCustomization._generalState.IsEntityHeaderAlwaysShown);
      Menu.SetChecked(EntityInspectorCustomization.PreserveSearchMenuItemName, EntityInspectorCustomization._generalState.IsSearchPreserved);
      Menu.SetChecked(EntityInspectorCustomization.UseCategoriesMenuItemName, EntityInspectorCustomization._generalState.IsCategorized);
    }

    /// <summary>
    /// Category state data of the entity inspector customization.
    /// </summary>
    [Serializable]
    private class CategoryStateData : IEquatable<CategoryStateData>
    {
      /// <summary>
      /// The key of the category. Has to be unique.
      /// </summary>
      public string CategoryKey = string.Empty;

      /// <summary>
      /// Specifies whether the tags foldout is expanded for that category.
      /// </summary>
      public bool IsTagsFoldoutExpanded;

      /// <summary>
      /// The last search string used for the category. Only maintained when <see cref="GeneralStateData.IsSearchPreserved" /> is
      /// true.
      /// </summary>
      public string SearchString = string.Empty;

      /// <summary>
      /// The last known vertical scroll position of the inspector. Only maintained when
      /// <see cref="GeneralStateData.IsCategorized" /> and <see cref="GeneralStateData.IsCategoryScrollPositionPreserved" />
      /// is true as well as categorized entity is active in the inspector.
      /// </summary>
      public float VerticalScrollPosition;

      /// <summary>
      /// Only for serialization.
      /// </summary>
      [SerializeField]
      private List<string> ToggledFoldoutComponentNamesList = new();

      /// <summary>
      /// The names of all components/buffers which are toggled, so having a different state than the
      /// <see cref="GeneralStateData.IsGlobalFoldoutExpanded" />.
      /// </summary>
      [NonSerialized]
      public HashSet<string> ToggledFoldoutComponentNames = new();

      /// <inheritdoc />
      public bool Equals(CategoryStateData? other)
      {
        if (ReferenceEquals(null, other))
        {
          return false;
        }

        if (ReferenceEquals(this, other))
        {
          return true;
        }

        return this.CategoryKey == other.CategoryKey;
      }

      /// <inheritdoc />
      public override bool Equals(object? obj)
      {
        if (ReferenceEquals(null, obj))
        {
          return false;
        }

        if (ReferenceEquals(this, obj))
        {
          return true;
        }

        if (obj.GetType() != this.GetType())
        {
          return false;
        }

        return this.Equals((CategoryStateData)obj);
      }

      /// <inheritdoc />
      public override int GetHashCode()
      {
        // ReSharper disable once NonReadonlyMemberInGetHashCode (field has to be read/write for serialization)
        return this.CategoryKey.GetHashCode();
      }

      /// <summary>
      /// Returns whether the category state has the default values.
      /// </summary>
      public bool HasDefaults(GeneralStateData generalState)
      {
        return this.IsTagsFoldoutExpanded == generalState.IsGlobalFoldoutExpanded
          && (string.IsNullOrEmpty(this.SearchString) || !generalState.IsSearchPreserved)
          && (Mathf.Approximately(this.VerticalScrollPosition, default) || !generalState.IsCategoryScrollPositionPreserved)
          && this.ToggledFoldoutComponentNames.Count == 0;
      }

      public static bool operator ==(CategoryStateData? left, CategoryStateData? right)
      {
        return Equals(left, right);
      }

      public static bool operator !=(CategoryStateData? left, CategoryStateData? right)
      {
        return !Equals(left, right);
      }

      /// <summary>
      /// Post-serialization actions.
      /// </summary>
      public void PostSerialize()
      {
        this.ToggledFoldoutComponentNames.Clear();
        this.ToggledFoldoutComponentNames.UnionWith(this.ToggledFoldoutComponentNamesList);
        this.ToggledFoldoutComponentNamesList.Clear();
      }

      /// <summary>
      /// Pre-serialization actions.
      /// </summary>
      public void PreSerialize()
      {
        this.ToggledFoldoutComponentNamesList.Clear();

        foreach (var foldoutState in this.ToggledFoldoutComponentNames)
        {
          this.ToggledFoldoutComponentNamesList.Add(foldoutState);
        }
      }

      /// <summary>
      /// Set the defaults.
      /// </summary>
      public void SetToDefaults(GeneralStateData generalState)
      {
        this.IsTagsFoldoutExpanded = generalState.IsGlobalFoldoutExpanded;
        this.SearchString = string.Empty;
        this.VerticalScrollPosition = default;
        this.ToggledFoldoutComponentNames.Clear();
      }
    }

    private delegate void EditorSetSelection(Object? newActiveObject, Object? newActiveContext, DataMode newDataModeHint);

    /// <summary>
    /// General state of the entity inspector customization.
    /// </summary>
    [Serializable]
    private class GeneralStateData
    {
      /// <summary>
      /// Specifies whether the categorization is used which enables a foldout states for component/buffer types per category
      /// instead of having foldout states over all components/buffer types. Also the search string is maintained per category
      /// then.
      /// </summary>
      public bool IsCategorized;

      /// <summary>
      /// Specifies whether the vertical scroll position is preserved and applied again when another entity is selected.
      /// </summary>
      public bool IsCategoryScrollPositionPreserved;

      /// <summary>
      /// Specifies whether the entity header is always shown at the top of the inspector.
      /// </summary>
      public bool IsEntityHeaderAlwaysShown;

      /// <summary>
      /// Specifies the global default foldout value for all entity inspector foldouts including tags and components/buffers.
      /// </summary>
      public bool IsGlobalFoldoutExpanded;

      /// <summary>
      /// Specifies whether the search string should be preserved and applied again when another entity is selected.
      /// </summary>
      public bool IsSearchPreserved;

      /// <summary>
      /// Set the defaults.
      /// </summary>
      public void SetToDefaults()
      {
        this.IsGlobalFoldoutExpanded = true;
        this.IsSearchPreserved = false;
        this.IsCategorized = false;
        this.IsCategoryScrollPositionPreserved = false;
        this.IsEntityHeaderAlwaysShown = false;
      }
    }

    /// <summary>
    /// The serialization container for the entity inspector customization.
    /// </summary>
    [Serializable]
    private class SerializationContainer
    {
      /// <summary>
      /// Category data for using category keys.
      /// </summary>
      public CategoryStateData[] CategoryStates = Array.Empty<CategoryStateData>();

      /// <summary>
      /// This is the default category data which is applied to all entities without a category or when no categories are used.
      /// </summary>
      public CategoryStateData DefaultCategoryState = new();

      /// <summary>
      /// General state data.
      /// </summary>
      public GeneralStateData GeneralState = new();

      /// <summary>
      /// Post-serialization actions.
      /// </summary>
      public void PostSerialize()
      {
        this.DefaultCategoryState.PostSerialize();

        foreach (var categoryState in this.CategoryStates)
        {
          categoryState.PostSerialize();
        }
      }

      /// <summary>
      /// Preparations for serialization.
      /// </summary>
      public void PreSerialize()
      {
        this.DefaultCategoryState.PreSerialize();

        foreach (var categoryState in this.CategoryStates)
        {
          categoryState.PreSerialize();
        }
      }
    }
  }
  
  /// <summary>
  /// A instance accessor for a type which is internal/private.
  /// </summary>
  public class ExpressionInstance
  {
    public readonly object Instance;
    // public readonly Type InstanceType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionInstance" /> class.
    /// </summary>
    /// <param name="instance">The instance to be hold.</param>
    public ExpressionInstance(object instance)
    {
      this.Instance = instance;
      // this.InstanceType = instance.GetType();
    }
  }

  /// <summary>
  /// An instance accessor for a type which is internal/private using the concrete type.
  /// </summary>
  public class ExpressionInstance<TType> : ExpressionInstance
  {
    public readonly TType TypedInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionInstance" /> class.
    /// </summary>
    /// <param name="instance">The instance to be hold.</param>
    public ExpressionInstance(TType instance) : base(instance!)
    {
      this.TypedInstance = instance;
    }
  }
  
  /// <summary>
  /// A builder for compiled lambda expression especially developed for accessing internal members in a way which enables
  /// best performance.
  /// </summary>
  public static class ExpressionBuilder
  {
    /// <summary>
    /// Builds a typed delegate for casting an object to a expression instance of the specified type name.
    /// </summary>
    /// <param name="fullTypeName">The full type name to cast the object to.</param>
    /// <returns>The delegate for casting the object to the expression instance.</returns>
    public static Func<object, ExpressionInstance> BuildCastToExpressionInstance(string fullTypeName)
    {
      var instanceType = Type.GetType(fullTypeName, true)!;

      var instanceParam = Expression.Parameter(typeof(object), "instance");
      var castExpression = Expression.Convert(instanceParam, instanceType);
      var newExpressionInstance = Expression.New(typeof(ExpressionInstance<>).MakeGenericType(instanceType).GetConstructors().First(), castExpression);

      return Expression.Lambda<Func<object, ExpressionInstance>>(newExpressionInstance, instanceParam).Compile();
    }

    /// <summary>
    /// Builds a typed delegate for getting the value of a field as <see cref="IList"/>.
    /// </summary>
    /// <param name="fullTypeName">Full name of the type wiht the static field.</param>
    /// <param name="fieldName">Name of the property.</param>
    /// <returns>The delegate for calling the method.</returns>
    public static Func<IList> BuildStaticGetFieldAsIList(string fullTypeName, string fieldName)
    {
      var type = Type.GetType(fullTypeName, true)!;

      var fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new Exception($"Field '{fieldName}' not found in type '{type}'");
      var getExpression = Expression.Field(null, fieldInfo);
      var castedListExpression = Expression.Convert(getExpression, typeof(IList));

      return Expression.Lambda<Func<IList>>(castedListExpression).Compile();
    }
    
    /// <summary>
    /// Builds a typed delegate for getting the value of a field.
    /// </summary>
    /// <param name="fullFieldTypeName">The type of the field.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <typeparam name="TType">The type of the instance to be called.</typeparam>
    /// <returns>The delegate for calling the method.</returns>
    public static Func<TType, ExpressionInstance> BuildInstanceGetFieldReturningExpressionInstance<TType>(string fullFieldTypeName,  string fieldName)
    {
      var instanceType = typeof(TType);
      var fieldType = Type.GetType(fullFieldTypeName, true)!;

      var instanceParam = Expression.Parameter(instanceType);

      var getField = Expression.Field(instanceParam, fieldName);
      var newExpressionInstance = Expression.New(typeof(ExpressionInstance<>).MakeGenericType(fieldType).GetConstructors().First(), getField);
      
      return Expression.Lambda<Func<TType, ExpressionInstance>>(newExpressionInstance, instanceParam).Compile();
    }
    
    /// <summary>
    /// Builds a typed delegate for getting the value of a field of an expression instance.
    /// </summary>
    /// <param name="fullTypeName">The full type name of type within the expression instance.</param>
    /// <param name="fieldName">Name of the field.</param>
    /// <typeparam name="TField">The type of field.</typeparam>
    /// <returns>The delegate for calling the method.</returns>
    public static Func<ExpressionInstance, TField> BuildInstanceGetField<TField>(string fullTypeName, string fieldName)
    {
      var instanceType = Type.GetType(fullTypeName, true)!;

      var accessorParam = Expression.Parameter(typeof(ExpressionInstance)); 
      var accessorCastedParam = Expression.Convert(accessorParam, typeof(ExpressionInstance<>).MakeGenericType(instanceType));
      var instanceCastedParam = Expression.Field(accessorCastedParam, "TypedInstance");

      var getExpression = Expression.Field(instanceCastedParam, fieldName);
      return Expression.Lambda<Func<ExpressionInstance, TField>>(getExpression, accessorParam).Compile();
    }
    
    /// <summary>
    /// Builds a typed delegate for getting the value of a property.
    /// </summary>
    /// <param name="fullTypeName">The full type name of type within the expression instance.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <typeparam name="TProperty">The type of property.</typeparam>
    /// <returns>The delegate for calling the property.</returns>
    public static Func<ExpressionInstance, TProperty> BuildInstanceGetProperty<TProperty>(string fullTypeName, string propertyName)
    {
      var instanceType = Type.GetType(fullTypeName, true)!;

      var accessorParam = Expression.Parameter(typeof(ExpressionInstance)); 
      var instanceParam = Expression.Field(accessorParam, nameof(ExpressionInstance.Instance));
      var instanceCastedParam = Expression.Convert(instanceParam, instanceType);
      
      var getExpression = Expression.Property(instanceCastedParam, propertyName);
      return Expression.Lambda<Func<ExpressionInstance, TProperty>>(getExpression, accessorParam).Compile();
    }

    /// <summary>
    /// Builds a typed delegate for calling the specified instance method.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <typeparam name="TDelegate">The type of the delegate to be build.</typeparam>
    /// <typeparam name="TType">The type of the instance to be called.</typeparam>
    /// <returns>The delegate for calling the method.</returns>
    public static TDelegate BuildStaticMethodCallUsingDelegate<TType, TDelegate>(string methodName)
      where TDelegate : Delegate
    {
      var type = typeof(TType);
      return ExpressionBuilder.BuildStaticMethodCallUsingDelegateCore<TDelegate>(type, methodName);
    }

    /// <summary>
    /// Builds a typed delegate for calling the specified instance method.
    /// </summary>
    /// <param name="type">The type to call the method from.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <typeparam name="TDelegate">The type of the delegate to be build.</typeparam>
    /// <returns>The delegate for calling the method.</returns>
    private static TDelegate BuildStaticMethodCallUsingDelegateCore<TDelegate>(Type type, string methodName)
      where TDelegate : Delegate
    {
      var invokeMethod = typeof(TDelegate).GetMethod("Invoke");

      var parameters = invokeMethod.GetParameters();
      Type[] parameterTypes = new Type[parameters.Length];
      ParameterExpression[] parameterExpressions = new ParameterExpression[parameters.Length];

      for (var i = 0; i < parameters.Length; i++)
      {
        parameterTypes[i] = parameters[i].ParameterType;
        parameterExpressions[i] = Expression.Parameter(parameters[i].ParameterType);
      }

      var callExpression = Expression.Call(type, methodName, Type.EmptyTypes, parameterExpressions);
      return Expression.Lambda<TDelegate>(callExpression, parameterExpressions).Compile();
    }
  }
  
  /// <summary>
  /// A specialized class for building expressions for UIToolkit.
  /// </summary>
  public static class ExpressionBuilderUI
  {
    /// <summary>
    /// The delegate for handling a hierarchy changed event on <see cref="VisualElement" />.
    /// A custom variant of <see cref="UnityEditor.UIElements.HierarchyEvent" />.
    /// </summary>
    public delegate void CustomHierarchyEvent(VisualElement visualElement, CustomHierarchyChangeType changeType);

    /// <summary>
    /// The custom hierarchy change type.
    /// A custom variant of <see cref="UnityEditor.UIElements.HierarchyChangeType" />.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: Has to have the same underlying type, enum values and the same enum values (order) as the original type.
    /// </remarks>
    public enum CustomHierarchyChangeType
    {
      Add,
      Remove,
      Move,
    }

    public const string BasicVisualElementPanelTypeName = "UnityEngine.UIElements.BaseVisualElementPanel, UnityEngine.UIElementsModule";
    private const string HierarchChangeTypeTypeName = "UnityEngine.UIElements.HierarchyChangeType, UnityEngine.UIElementsModule";

    /// <summary>
    /// Builds thres typed delegates for adding, removing an event handler for a hierarchy changed event as well as checking
    /// whether a delegate is contained.
    /// </summary>
    /// <returns>A tuple with three delegates for adding, removing event handlers as wells as contains check.</returns>
    public static (Action<ExpressionInstance, Delegate> Add, Action<ExpressionInstance, Delegate> Remove, Func<ExpressionInstance, Delegate, bool> Contains)
      BuildAddRemoveContainsHierarchyChangedEventHandler()
    {
      var instanceType = Type.GetType(ExpressionBuilderUI.BasicVisualElementPanelTypeName, true)!;
      var internalEventInfo = instanceType.GetEvent("hierarchyChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        ?? throw new Exception($"Event 'hierarchyChanged' not found in type '{ExpressionBuilderUI.BasicVisualElementPanelTypeName}'");

      // Define the parameters for the add and remove action
      var accessorParam = Expression.Parameter(typeof(ExpressionInstance), "instance");
      var delegateParam = Expression.Parameter(typeof(Delegate), "delegate");
      var delegateCastedParam = Expression.Convert(delegateParam, internalEventInfo.EventHandlerType);

      var accessorCastedParam = Expression.Convert(accessorParam, typeof(ExpressionInstance<>).MakeGenericType(instanceType));
      var instanceCastedParam = Expression.Field(accessorCastedParam, nameof(ExpressionInstance<Missing>.TypedInstance));

      var addEventHandlerMethod = internalEventInfo.GetAddMethod(true);
      var removeEventHandlerMethod = internalEventInfo.GetRemoveMethod(true);
      var eventField = Expression.Field(instanceCastedParam, "hierarchyChanged");

      var addAction = Expression.Lambda<Action<ExpressionInstance, Delegate>>(
        Expression.Call(instanceCastedParam, addEventHandlerMethod, delegateCastedParam),
        accessorParam, delegateParam).Compile();

      var removeAction = Expression.Lambda<Action<ExpressionInstance, Delegate>>(
        Expression.Call(instanceCastedParam, removeEventHandlerMethod, delegateCastedParam),
        accessorParam, delegateParam).Compile();

      var getInvocationListCall = Expression.Call(eventField, nameof(MulticastDelegate.GetInvocationList), null);

      // Convert the invocation list to an array 
      var invocationListArray = Expression.Convert(getInvocationListCall, typeof(Delegate[]));

      // Create a loop to iterate over the invocation list and check whether the supplied delegate is already part of the list
      var indexVariable = Expression.Variable(typeof(int), "i");
      //var resultVariable = Expression.Variable(typeof(bool), "result");
      var breakLabel = Expression.Label(typeof(bool)); // Return type

      var loopBody = Expression.Block(
        Expression.IfThen(
          Expression.Equal(Expression.ArrayIndex(invocationListArray, indexVariable), delegateCastedParam),
          Expression.Block(
            Expression.Return(breakLabel, Expression.Constant(true))
          )
        ),
        Expression.PostIncrementAssign(indexVariable)
      );

      // Define a label target for the break statement
      var loop = Expression.Loop(
        Expression.IfThenElse(
          Expression.LessThan(indexVariable, Expression.ArrayLength(invocationListArray)),
          loopBody,
          Expression.Return(breakLabel, Expression.Constant(false))
        ),
        breakLabel
      );
      var loopBlock = Expression.Block(
        new[] { indexVariable }, // Add 'i' to the variables list of the block
        loop
      );

      // Create the contains action
      var containsAction = Expression.Lambda<Func<ExpressionInstance, Delegate, bool>>(loopBlock, accessorParam, delegateParam).Compile();

      return (addAction, removeAction, containsAction);
    }

    /// <summary>
    /// Builds the even handler delegate for the <see cref="CustomHierarchyEvent" />.
    /// </summary>
    /// <returns>The event handler for being called by the hierarchy event. The handler has still to be added</returns>
    public static Func<CustomHierarchyEvent, Delegate> BuildCreateHierarchyChangedEventHandler()
    {
      var instanceType = Type.GetType(ExpressionBuilderUI.BasicVisualElementPanelTypeName, true)!;
      var changeTypeType = Type.GetType(ExpressionBuilderUI.HierarchChangeTypeTypeName, true)!;
      var internalEventInfo = instanceType.GetEvent("hierarchyChanged", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

      if (internalEventInfo == null)
      {
        throw new ArgumentException($"Event 'hierarchyChanged' not found in type '{instanceType}'");
      }

      // Define the parameters for the add action
      var hierarchyEventParam = Expression.Parameter(typeof(CustomHierarchyEvent), "hierarchyEventHandler");

      // Define the parameters for the add event handler method
      var visualElementParam = Expression.Parameter(typeof(VisualElement), "visualElement");
      var changeTypeParam = Expression.Parameter(changeTypeType, "changeType");

      // Convert changeType from HierarchyChangeType to int
      var changeTypeToIntConvert = Expression.Convert(changeTypeParam, typeof(int));

      // Convert int to CustomHierarchyChangeTyp
      var intToCustomChangeTypeConvert = Expression.Convert(changeTypeToIntConvert, typeof(CustomHierarchyChangeType));

      // Call the supplied HierarchyEvent in the internalHandlerMethod with the supplied visualElementParam and the converted changeType
      var callHierarchyEvent = Expression.Invoke(hierarchyEventParam, visualElementParam, intToCustomChangeTypeConvert);

      // Create the lambda which is returned
      var lambda = Expression.Lambda(internalEventInfo.EventHandlerType, callHierarchyEvent, visualElementParam, changeTypeParam);

      // Return the lambda
      return Expression.Lambda<Func<CustomHierarchyEvent, Delegate>>(lambda, hierarchyEventParam).Compile();
    }
  }
  
  /// <summary>
  /// Extensions for <see cref="VisualElement" />.
  /// </summary>
  public static class VisualElementExtensions
  {
    /// <summary>
    /// Gets the first parent of the specified type parameter <typeparamref name="T" />.
    /// </summary>
    /// <param name="element">The element to inspect</param>
    /// <param name="elementName">Optional name of the to find.</param>
    /// <typeparam name="T">The type to be found.</typeparam>
    /// <returns>The first found element in the parent elements matching the specified <typeparamref name="T" />.</returns>
    public static T? FindParent<T>(this VisualElement? element, string? elementName = null)
    {
      if (element == null)
      {
        return default;
      }

      if (element is T typedElement && (string.IsNullOrWhiteSpace(elementName) || string.Compare(elementName, element.name, StringComparison.InvariantCultureIgnoreCase) == 0))
      {
        return typedElement;
      }

      return element.parent.FindParent<T>(elementName);
    }

    /// <summary>
    /// Gets the root of the specified element.
    /// </summary>
    /// <param name="element">The element to inspect.</param>
    /// <returns>The root element.</returns>
    public static VisualElement FindRoot(this VisualElement element)
    {
      if (element == null)
      {
        throw new ArgumentNullException(nameof(element));
      }

      if (element.parent != null)
      {
        return element.parent.FindRoot();
      }

      return element;
    }
  }  
}
#endif