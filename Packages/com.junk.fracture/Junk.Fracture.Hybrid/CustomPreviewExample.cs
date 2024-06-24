using UnityEditor;
using UnityEngine;

/// <summary>
/// Demonstrates how to use <see cref="UnityEditor.PreviewRenderUtility"/>
/// to render a small interactive scene in a custom editor window.
/// </summary>
public class CustomPreviewExample : EditorWindow
{
	[MenuItem("Tools/Custom Preview")]
	public static void ShowWindow()
	{
		GetWindow<CustomPreviewExample>("Preview");
	}

	private PreviewRenderUtility previewUtility;
	private GameObject targetObject;

	private void OnEnable()
	{
		previewUtility = new PreviewRenderUtility();
		SetupPreviewScene();
	}

	private void OnDisable()
	{
		if (previewUtility != null)
			previewUtility.Cleanup();

		if (targetObject != null)
			Object.DestroyImmediate(targetObject);
	}

	private void SetupPreviewScene()
	{
		targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		targetObject.transform.position = Vector3.zero;
		
		var meshrenderer = targetObject.GetComponent<MeshRenderer>();
		meshrenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
		// set color
		meshrenderer.sharedMaterial.color = Color.green;
		
		// Since we want to manage this instance ourselves, hide it
		// from the current active scene, but remember to also destroy it.
		targetObject.hideFlags = HideFlags.HideAndDontSave;
		previewUtility.AddSingleGO(targetObject);

		// Camera is spawned at origin, so position is in front of the cube.
		previewUtility.camera.transform.position = new Vector3(0f, 0f, -10f);
		
		// This is usually set very small for good performance, but
		// we need to shift the range to something our cube can fit between.
		previewUtility.camera.nearClipPlane = 5f;
		previewUtility.camera.farClipPlane = 20f;
	}

	private void Update()
	{
		// Just do some random modifications here.
		float time = (float)EditorApplication.timeSinceStartup * 15;
		targetObject.transform.rotation = Quaternion.Euler(time * 2f, time * 4f, time * 3f);
		
		// Since this is the most important window in the editor, let's use our
		// resources to make this nice and smooth, even when running in the background.
		Repaint();
	}

	private void OnGUI()
	{
		// Render the preview scene into a texture and stick it
		// onto the current editor window. It'll behave like a custom game view.
		Rect rect = new Rect(0, 0, base.position.width, base.position.height);
		previewUtility.BeginPreview(rect, previewBackground: GUIStyle.none);
		previewUtility.Render();
		var texture = previewUtility.EndPreview();
		GUI.DrawTexture(rect, texture);
	}
}