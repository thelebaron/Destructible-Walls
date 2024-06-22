using Nvidia;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    public static class NvFractureUtility
    {
        // extract to separate class
        public static void Voronoi(NvFractureTool fractureTool, NvMesh nvMesh, int totalChunks)
        {
            var sites = new NvVoronoiSitesGenerator(nvMesh);
            sites.uniformlyGenerateSitesInMesh(totalChunks);
            fractureTool.voronoiFracturing(0, sites);
        }
        private static void Clustered(NvFractureTool fractureTool, NvMesh mesh, int clusters, int sitesPerCluster, float clusterRadius)
        {
            NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(mesh);
            sites.clusteredSitesGeneration(clusters, sitesPerCluster, clusterRadius);
            fractureTool.voronoiFracturing(0, sites);
        }
        
        private static void Slicing(NvFractureTool fractureTool, NvMesh mesh, Vector3Int slices, int offset_variations, 
            int angle_variations, float amplitude, float frequency, int octaveNumber, int surfaceResolution)
        {
            SlicingConfiguration conf = new SlicingConfiguration();
            conf.slices            = slices;
            conf.offset_variations = offset_variations;
            conf.angle_variations  = angle_variations;

            conf.noise.amplitude         = amplitude;
            conf.noise.frequency         = frequency;
            conf.noise.octaveNumber      = octaveNumber;
            conf.noise.surfaceResolution = surfaceResolution;

            fractureTool.slicing(0, conf, false);
        }
        
        public static NvFractureTool FractureTool(NvMesh nvMesh)
        {
            var fractureTool = new NvFractureTool();
            fractureTool.setRemoveIslands(false);
            fractureTool.setSourceMesh(nvMesh);

            return fractureTool;
        }

        public static NvMesh Mesh(FractureWorkingData fractureWorking)
        {
            return new NvMesh(
                fractureWorking.mesh.vertices,
                fractureWorking.mesh.normals,
                fractureWorking.mesh.uv,
                fractureWorking.mesh.vertexCount,
                fractureWorking.mesh.GetIndices(0),
                (int) fractureWorking.mesh.GetIndexCount(0)
            );
        }
    }
}