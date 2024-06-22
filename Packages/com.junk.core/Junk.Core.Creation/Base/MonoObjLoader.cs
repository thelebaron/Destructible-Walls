using UnityEngine;

namespace Junk.Core.Creation
{
    public class MonoObjLoader : MonoBehaviour
    {
        public string path = "Packages/com.thelebaron.foundation/Foundation.Tests/DataAssets/glove.obj";
        public Mesh mesh;
        
        [ContextMenu("Import Mesh Path")]
        private void Start()
        {
            mesh = ObjProcessor.Read(path);
            //var importer = new ObjImporter();
            //mesh = importer.ImportFile(path);
            gameObject.TryGetComponent<MeshFilter>(out var filter);
            filter.mesh = mesh;

        }
    }
}