namespace thelebaron.Destruction.Authoring
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

        public static NvMesh Mesh(BakeData bake)
        {
            return new NvMesh(
                bake.mesh.vertices,
                bake.mesh.normals,
                bake.mesh.uv,
                bake.mesh.vertexCount,
                bake.mesh.GetIndices(0),
                (int) bake.mesh.GetIndexCount(0)
            );
        }
    }
}