
// This was made by aaro4130 on the Unity forums.  Thanks boss!
// It's been optimized and slimmed down for the purpose of loading Quake 3 TGA textures from memory streams.
// see https://github.com/cmdr2/unity-remote-obj-loader/blob/master/OBJ/src/TGALoader.cs

namespace Junk.Core.Creation
{

using System;
using System.IO;
using UnityEngine;

public static class TGALoader
{
	
	/*public static Texture2D LoadTGA(string fileName)
	{
		using (var imageFile = File.OpenRead(fileName))
		{
			return LoadTGA(imageFile);
		}
	}*/
	/// <summary>
	/// 
	/// </summary>
	public static Texture2D LoadTGA(Stream stream)
	{
		
		using (BinaryReader reader = new BinaryReader(stream))
		{
			// Skip some header info we don't care about.
			// Even if we did care, we have to move the stream seek point to the beginning,
			// as the previous method in the workflow left it at the end.
			reader.BaseStream.Seek(12, SeekOrigin.Begin);
			
			short width = reader.ReadInt16();
			short height = reader.ReadInt16();
			int bitDepth = reader.ReadByte();
			
			// Skip a byte of header information we don't care about.
			reader.BaseStream.Seek(1, SeekOrigin.Current);
			
			Texture2D tex = new Texture2D(width, height);
			Color32[] pulledColors = new Color32[width * height];
			int length = width * height;

			if (bitDepth == 32)
			{
				for (int row = 1; row <= height; row++)
				{
					for (int col = 0; col < width; col++)
					{
						byte red = reader.ReadByte();
						byte green = reader.ReadByte();
						byte blue = reader.ReadByte();
						byte alpha = reader.ReadByte();
						
	//					pulledColors [i] = new Color32(blue, green, red, alpha);
						pulledColors [length - (row * width) + col] = new Color32(blue, green, red, alpha);
					}
				}
			} else if (bitDepth == 24)
			{
				for (int row = 1; row <= height; row++)
				{
					for (int col = 0; col < width; col++)
					{
						byte red = reader.ReadByte();
						byte green = reader.ReadByte();
						byte blue = reader.ReadByte();
						
						pulledColors [length - (row * width) + col] = new Color32(blue, green, red, 1);
					}
				}
			} else
			{
				throw new Exception("TGA texture had non 32/24 bit depth.");
			}

			tex.SetPixels32(pulledColors);
			tex.Apply();
			return tex;
			
		}
	}
}
}