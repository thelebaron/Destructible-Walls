

public static class Utils
{
    public static void Voronoi(NvFractureTool fractureTool, NvMesh nvMesh, int count = 5)
    {
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(count);
        fractureTool.voronoiFracturing(0, sites);
    }
    
    public static void Clustered(NvFractureTool fractureTool, NvMesh mesh, int clusters, int sitesPerCluster, float clusterRadius)
    {
        var sites = new NvVoronoiSitesGenerator(mesh);
        sites.clusteredSitesGeneration(clusters, sitesPerCluster, clusterRadius);
        fractureTool.voronoiFracturing(0, sites);
    }
    
    public static NvFractureTool Voronoi2(NvFractureTool fractureTool, NvMesh nvMesh, int count = 2)
    {
        
        var sites = new NvVoronoiSitesGenerator(nvMesh);
        sites.uniformlyGenerateSitesInMesh(count);
        fractureTool.voronoiFracturing(0, sites);

        return fractureTool;
    }
    
    /*
    private void _Skinned(NvFractureTool fractureTool, NvMesh mesh)
    {
        SkinnedMeshRenderer     smr   = source.GetComponent<SkinnedMeshRenderer>();
        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(mesh);
        sites.boneSiteGeneration(smr);
        fractureTool.voronoiFracturing(0, sites);
    }

    private void _Slicing(NvFractureTool fractureTool, NvMesh mesh)
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
    }*/


}