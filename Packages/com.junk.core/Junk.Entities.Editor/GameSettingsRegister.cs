using System.Collections.Generic;
using Junk.Entities.Hybrid;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// Register a SettingsProvider using UIElements for the drawing framework:
    static class GameSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/Game", SettingsScope.Project)
            {
                label = "Game Settings",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = GameSettingsObject.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uss");
                    //rootElement.styleSheets.Add(styleSheet);
                    var title = new Label()
                    {
                        text = "Custom UI Elements"
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);

                    var properties = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = FlexDirection.Column
                        }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    properties.Add(new PropertyField(settings.FindProperty("someString")));
                    properties.Add(new PropertyField(settings.FindProperty("number")));
                    properties.Add(new PropertyField(settings.FindProperty("volume")));
                    properties.Add(new PropertyField(settings.FindProperty("prefab")));
                    
                    // create a button to select the asset
                    var button = new Button(() => { Selection.activeObject = settings.targetObject; })
                    {
                        text = "Select Settings Asset",
                    };
                    properties.Add(button);
                    
                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }
    }