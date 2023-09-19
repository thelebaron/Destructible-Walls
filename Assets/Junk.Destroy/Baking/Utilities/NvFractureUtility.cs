namespace Junk.Destroy.Authoring
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