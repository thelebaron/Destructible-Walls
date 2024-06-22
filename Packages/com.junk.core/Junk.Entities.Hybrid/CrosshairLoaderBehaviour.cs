using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
// Gamecore contains shared menu and managed monobehaviour scripts for menu and splashscreen management
namespace Junk.GameCore
{
    /// <summary>
    /// Some idea to load crosshairs from a folder in the StreamingAssets folder.
    /// Maybe dumb?
    /// </summary>
    public class CrosshairLoaderBehaviour : MonoBehaviour
    {
        // Load a .jpg or .png file by adding .bytes extensions to the file
        // and dragging it on the imageAsset variable.
        public TextAsset imageAsset;
        public Texture2D crosshairTexture2D;
        public void Start()
        {
            //Debug.Log(Application.streamingAssetsPath);
            var path     = Application.streamingAssetsPath+ "/Game/Textures/Crosshair2.png";
            //var path     = "Assets/StreamingAssets/Game/Textures/Crosshair2.png";
            var pngBytes = System.IO.File.ReadAllBytes(path);
            // Create a texture. Texture size does not matter, since
            // LoadImage will replace with the size of the incoming image.
            var tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            tex.LoadImage(pngBytes);
            tex.PremultiplyAlpha();
            crosshairTexture2D = tex;
            
            //Texture2D texture2D = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            //texture2D.SetPixels(tex.GetPixels());
            //crosshairTexture2D = texture2D;
        }
    }
    
    public static class TextureUtil
    {
        public static Texture2D PremultiplyAlpha(this Texture2D texture)
        {
            Color[] pixels                                    = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Premultiply(pixels[i]);
            texture.SetPixels(pixels);
            return texture;
        }
    
        private static Color Premultiply(Color color)
        {
            return new Color(color.r * color.a, color.g * color.a, color.b * color.a, color.a);
        }
    
    }
}
