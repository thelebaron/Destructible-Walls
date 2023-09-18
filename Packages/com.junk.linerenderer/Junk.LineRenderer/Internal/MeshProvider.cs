using UnityEditor;
using UnityEngine;

namespace Junk.LineRenderer.Internal
{
    public static class MeshProvider
	{
		#if UNITY_EDITOR
		[MenuItem("Assets/Create/E7/ECS/LineRenderer/Line Mesh")]
		public static void CreateMeshAsset()
		{
			//Create a new mesh asset
			var mesh = QuadLineMesh();
			ProjectWindowUtil.CreateAsset(mesh, "LineMesh.asset");
		}
		#endif
		
		public static Mesh lineMesh { get; private set; }

		static MeshProvider ()
		{
			lineMesh = new Mesh();
			lineMesh.hideFlags |= HideFlags.DontSave;
			lineMesh.name = "quad 1x1, pivot at bottom center";
			lineMesh.vertices = new Vector3[4]{ new Vector3{ x=-0.5f } , new Vector3{ x=0.5f } , new Vector3{ x=-0.5f , z=1 } , new Vector3{ x=0.5f , z=1 } };
			lineMesh.triangles = new int[6]{ 0 , 2 , 1 , 2 , 3 , 1 };
			lineMesh.normals = new Vector3[4]{ -Vector3.forward , -Vector3.forward , -Vector3.forward , -Vector3.forward };
			lineMesh.uv = new Vector2[4]{ new Vector2{ x=0 , y=0 } , new Vector2{ x=1 , y=0 } , new Vector2{ x=0 , y=1 } , new Vector2{ x=1 , y=1 } };
		}
		public static Mesh QuadLineMesh()
		{
			var mesh =  new Mesh
			{
				//mesh.hideFlags |= HideFlags.DontSave;
				name      = "quad 1x1, pivot at bottom center",
				vertices  = new Vector3[4]{ new Vector3{ x =-0.5f } , new Vector3{ x =0.5f } , new Vector3{ x =-0.5f , z =1 } , new Vector3{ x =0.5f , z =1 } },
				triangles = new int[6]{ 0 , 2 , 1 , 2 , 3 , 1 },
				normals   = new Vector3[4]{ -Vector3.forward , -Vector3.forward , -Vector3.forward , -Vector3.forward },
				uv        = new Vector2[4]{ new Vector2{ x =0 , y =0 } , new Vector2{ x =1 , y =0 } , new Vector2{ x =0 , y =1 } , new Vector2{ x =1 , y =1 } }
			};

			return mesh;
		}
	}
}
