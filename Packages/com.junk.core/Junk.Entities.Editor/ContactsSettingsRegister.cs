using System.Collections.Generic;
using Junk.Entities.Hybrid;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Junk.Entities.Editor
{
    // Register a SettingsProvider using UIElements for the drawing framework:
    static class ContactsSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider ContactsSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
            var provider = new SettingsProvider("Project/Game/Contacts", SettingsScope.Project)
            {
                label = "Contacts Prefabs",
                // activateHandler is called when the user clicks on the Settings item in the Settings window.
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = ContactsSettingsObject.GetSerializedSettings();

                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    //var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/settings_ui.uss");
                    //rootElement.styleSheets.Add(styleSheet);
                    var title = new Label()
                    {
                        text = "Contacts Authoring Settings"
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

                    properties.Add(new PropertyField(settings.FindProperty("bloodsplatTiny")));
                    properties.Add(new PropertyField(settings.FindProperty("bulletholeTiny")));
                    //properties.Add(new Label("Small debris - pebble"));
                    properties.Add(new PropertyField(settings.FindProperty("debrisTiny1")));

                    rootElement.Bind(settings);
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "Number", "Some String" })
            };

            return provider;
        }
    }
}