

public static class Utils
{
    public static void Voronoi(NvFractureTool fractureTool, NvMesh nvMesh, int count = 2)
    {
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(count);
        fractureTool.voronoiFracturing(0, sites);

        //return fractureTool;
    }
    
    public static NvFractureTool Voronoi2(NvFractureTool fractureTool, NvMesh nvMesh, int count = 2)
    {
        
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(count);
        fractureTool.voronoiFracturing(0, sites);

        return fractureTool;
    }
}