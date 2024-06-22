using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Junk.Entities.Hybrid
{
    [RequireComponent(typeof(UIDocument))]
    public class LoadingScreenBehaviour : MonoBehaviour
    {
        private UIDocument    uiDocument;
        private                  VisualElement root;
        // the label that shows the loading text
        private                  Label         loadingLabel;
        // the black background
        private                  VisualElement background;

        public float fadeDuration = 1.5f;  // Duration of the fade in seconds
        
        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            Assert.IsNotNull(uiDocument);
            root = uiDocument.rootVisualElement;
            
            background = root.Q<VisualElement>("LoadingBackground");
            loadingLabel             = root.Q<Label>("LoadingLabel");
            HideLoadingScreen();
        }

        public void FadeOut()
        {
            SetBlack();
            StartCoroutine(FadeText(0));
        }
        
        public void SetBlack()
        {
            root.style.display       = DisplayStyle.Flex;
            background.style.opacity = 1;
        }
        
        public void HideLoadingScreen()
        {
            root.style.display = DisplayStyle.None;
        }
        
        private IEnumerator FadeText(float targetOpacity)
        {
            float elapsedTime   = 0f;
            var startOpacity  = background.style.opacity;

            while (elapsedTime < fadeDuration)
            {
                float progress = elapsedTime / fadeDuration;
                background.style.opacity = Mathf.Lerp(startOpacity.value, targetOpacity, progress);
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            // Ensure the final opacity is exactly the target value
            background.style.opacity = targetOpacity;
            HideLoadingScreen();
        }
    }

}