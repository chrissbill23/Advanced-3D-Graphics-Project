using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public enum Zone{
    FootHill,
    Montane,
    Subalpine,
    Alpine,
    Nival,
    Random
}
public class TreeMap<T>{
    private List<Dictionary<int,T>> values = new List<Dictionary<int,T>>();
    public T get(int x,int y){
        return values[x][y];
    }
    public void Add(int x, int y, T v){
        if(x >= values.Count){
            Dictionary<int,T> t = new Dictionary<int,T>();
            t.Add(y,v);
            values.Add(t);

        } else{
            Dictionary<int,T> xtmp = values[x];
            if(y >= xtmp.Count){
                xtmp.Add(y,v);
            } else
            {
                xtmp[y] = v;
            }
            values[x] = xtmp;

        }
    }
}
public class CustomTerrain : MonoBehaviour {

    public Text debug;
    [Header("Max height terrain deformation")]
    [Range(0, 1000)]
    public float max_height = 500.0f; //added

    [Header("Global brushes attributes")]
    [Range(1, 800)]
    public int brush_radius = 10;

    [Header("Instance brush attributes")]
    public GameObject object_prefab = null;
    public float min_scale = 0.8f;
    public float max_scale = 1.2f;

    
    private int[] vegetations; //Added
    private GameObject vegetationstmp0;

    private Brush current_brush;

    private Terrain terrain;
    private Collider terrain_collider;
    private TerrainData terrain_data;
    private Vector3 terrain_size;
    private int heightmap_width;
    private int heightmap_height;
    private float[,] heightmap_data;
    private int amap_width, amap_height;
    private int detail_width, detail_height;
    private Dictionary<int, int[,]> detail_layer = null;//Edited
    private float[,,] alphamaps;
    private int[,] occupance;//Added
    private float prop_width = 1.0f;//Added
    private float prop_height = 1.0f;//Added
    private int totDet = 1;//Added

    private GameObject highlight_go;
    private Projector highlight_proj;
    public static System.Random rnd = new System.Random();

    [SerializeField] Camera cam;

    // Initialization
    void Start () {
        if (!terrain)
            terrain = Terrain.activeTerrain;
        terrain_collider = terrain.GetComponent<Collider>();
        terrain_data = terrain.terrainData;
        terrain_size = terrain_data.size;
        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;
        heightmap_data = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        current_brush = null;
        highlight_go = GameObject.Find("Cursor Highlight");
        highlight_proj = highlight_go.GetComponent<Projector>();

        detail_width = terrain_data.detailWidth;
        detail_height = terrain_data.detailHeight;
        //detail_layer = terrain_data.GetDetailLayer(0, 0, detail_width, detail_height, 0);
        //Edited 
        totDet = terrain_data.detailPrototypes.Length;
        detail_layer = new Dictionary<int, int[,]>();
        for (int y = 0; y < totDet; y++) {
            detail_layer.Add(y,terrain_data.GetDetailLayer(0, 0, detail_width, detail_height, y));
        }
        amap_width = terrain_data.alphamapWidth;
        amap_height = terrain_data.alphamapHeight;
        alphamaps = terrain_data.GetAlphamaps(0, 0, amap_width, amap_height);
        
        occupance = new int[amap_height,amap_width]; //Added
        // Reset and save grass
        for (int l = 0; l < totDet; l++)
        for (int y = 0; y < detail_height; y++) {
            for (int x = 0; x < detail_width; x++) {
                detail_layer[l][x, y] = 0;
            }
        }
        saveDetails();

        prop_width = detail_width / amap_width;
        prop_height = detail_height / amap_height;

        // Reset and save textures
         for (int y = 0; y < amap_height; y++) {
             for (int x = 0; x < amap_width; x++) {
                 occupance[y,x] = 0;
              }
        }
        // saveTextures();

        cam = GameObject.FindGameObjectWithTag("SecondCamera").GetComponent<Camera>();

        //Added
        vegetationstmp0 = new GameObject();
        vegetationstmp0.AddComponent<GrassConstraint>();
        VegetationConstraint[] vegetationstmp1 = vegetationstmp0.GetComponents<VegetationConstraint>();
        vegetations = new int[totDet];
        for(int i = 0; i<totDet; i++){
            for(int j = 0; j <vegetationstmp1.Length; j++){  
                    if(Array.Exists(vegetationstmp1[j].detailsIndexes, el => i == el)){
                        vegetations[i] = j;
                    }
                    else 
                        vegetations[i] = 0;
            }
        }


    }

    // Called once per frame
    void Update () {
        Vector3 hit_loc = Vector3.zero;
        bool do_draw_target = false;
        RaycastHit hit;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (terrain_collider.Raycast(ray, out hit, Mathf.Infinity)) {
            hit_loc = hit.point;
            if (current_brush)
                do_draw_target = true;
            if (Input.GetMouseButton(0)) {
                debug.text = "Coords: " + hit_loc.ToString();
                if (current_brush)
                    current_brush.callDraw(hit_loc.x, hit_loc.z);
            }
        }
        drawTarget(hit_loc, do_draw_target);

        int tot_layers = (int) alphamaps.Length/(amap_height*amap_width);
        for (int y = 0; y < amap_height; y++) {//Added
             for (int x = 0; x < amap_width; x++) {
                 int maxIndex = 0;
                 float maxValue = -1.0f;
                 for(int i = 0; i<tot_layers; i++){
                    if (alphamaps[y,x,i] > maxValue)
                    {
                        maxValue = alphamaps[y,x,i];
                        maxIndex = i;
                    }
                 }
                 occupance[y,x] = maxIndex;
              }
        }
        totDet = terrain_data.detailPrototypes.Length;
        VegetationConstraint[] vegetationstmp1 = vegetationstmp0.GetComponents<VegetationConstraint>();
        vegetations = new int[totDet];
        for(int i = 0; i<totDet; i++){
            //terrain_data.detailPrototypes[i].maxHeight = 2.0f;
            for(int j = 0; j <vegetationstmp1.Length; j++){  
                    if(Array.Exists(vegetationstmp1[j].detailsIndexes, el => i == el)){
                        vegetations[i] = j;
                    }
                    else 
                        vegetations[i] = 0;
            }
        }
    }

    // Draw the brush marker on the terrain
    private void drawTarget(Vector3 c, bool show) {
        if (show) {
            c.y += 100;
            highlight_go.transform.position = c;
            highlight_proj.orthographicSize = brush_radius;
        } else {
            highlight_go.transform.position = new Vector3(-10, -10, -10);
            highlight_proj.orthographicSize = 0;
        }
    }

    public int registerPrefab(GameObject go) {
        for (int i = 0; i < terrain_data.treePrototypes.Length; i++) {
            if (terrain_data.treePrototypes[i].prefab.name.Equals(go.name))
                return i;
        }

        TreePrototype proto = new TreePrototype();
        proto.bendFactor = 0.0f;
        proto.prefab = object_prefab;
        List<TreePrototype> protos = new List<TreePrototype>(terrain_data.treePrototypes);
        protos.Add(proto);
        terrain_data.treePrototypes = protos.ToArray();
        return protos.Count - 1;
    }

    public Vector3 get3(int x, int z) {
        return new Vector3(x, get(x, z), z);
    }
    public Vector3 get3(float x, float z) {
        return new Vector3(x, get(x, z), z);
    }
    public Vector3 getInterp3(float x, float z) {
        return new Vector3(x, getInterp(x, z), z);
    }

    // Get grid height for a node
    public float get(int x, int z) {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data[z, x] * terrain_data.heightmapScale.y;
    }
    public float get(float x, float z) {
        return get((int)x, (int)z);
    }

    public float getInterp(float x, float z) {
        return terrain_data.GetInterpolatedHeight(x / heightmap_width,
                                                  z / heightmap_height);
    }
    public float getSteepness(float x, float z) {
        return terrain_data.GetSteepness(x / heightmap_width,
                                         z / heightmap_height);
    }
    public Vector3 getNormal(float x, float z) {
        return terrain_data.GetInterpolatedNormal(x / heightmap_width,
                                                  z / heightmap_height);
    }

    // Set the grid height for a node
    public void set(int x, int z, float val) {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        heightmap_data[z, x] = val / terrain_data.heightmapScale.y;
    }
    public void set(float x, float z, float val) {
        set((int)x, (int)z, val);
    }

    // Spawn a new object (tree)
    public void spawnObject(Vector3 loc, float scale, int proto_idx) {
        TreeInstance obj = new TreeInstance();
        loc = new Vector3(loc.x / heightmap_width,
                          loc.y / terrain_data.heightmapScale.y,
                          loc.z / heightmap_height);
        obj.position = loc;
        obj.prototypeIndex = proto_idx;
        obj.lightmapColor =  Color.white;
        obj.heightScale = scale;
        obj.widthScale = scale;
        obj.rotation = (float)rnd.NextDouble() * 2 * Mathf.PI;
        obj.color = Color.white;
        terrain.AddTreeInstance(obj);
    }
    // Object (tree) manipulation
    public int getObjectCount() {
        return terrain_data.treeInstanceCount;
    }
    public TreeInstance getObject(int index) {
        return terrain_data.GetTreeInstance(index);
    }
    // Returns an object (tree) location in grid space
    public Vector3 getObjectLoc(int index) {
        return getObjectLoc(terrain_data.GetTreeInstance(index));
    }
    public Vector3 getObjectLoc(TreeInstance obj) {
        return new Vector3(obj.position.x * heightmap_width,
                           obj.position.y * terrain_data.heightmapScale.y,
                           obj.position.z * heightmap_height);
    }

    // Get dimensions of the heightmap grid
    public Vector3 gridSize() {
        return new Vector3(heightmap_width, 0.0f, heightmap_height);
    }
    // Get real dimensions of the terrain (world space)
    public Vector3 terrainSize() {
        return terrain_size;
    }
    // Get texture (alphamap) size
    public Vector2 textureSize() {
        return new Vector2(amap_width, amap_height);
    }
    public float[,,] getTextures() {
        return alphamaps;
    }
    // Get detail size
    public Vector2 detailSize() {
        return new Vector2(detail_width, detail_height);
    }
    public int[,] getDetails(int layer = 0) {
        return detail_layer[layer];
    }

    // Convert from grid space to world space
    public Vector3 grid2world(Vector3 grid) {
        return new Vector3(grid.x * terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z * terrain_data.heightmapScale.z);
    }
    public Vector3 grid2world(float x, float y, float z) {
        return grid2world(new Vector3(x, y, z));
    }
    public Vector3 grid2world(float x, float z) {
        return grid2world(x, 0.0f, z);
    }

    // Convert from world space to grid space
    public Vector3 world2grid(Vector3 grid) {
        return new Vector3(grid.x / terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z / terrain_data.heightmapScale.z);
    }
    public Vector3 world2grid(float x, float y, float z) {
        return world2grid(new Vector3(x, y, z));
    }
    public Vector3 world2grid(float x, float z) {
        return world2grid(x, 0.0f, z);
    }

    // Reset to flat terrain
    public void reset() {
        for (int z = 0; z < heightmap_height; z++) {
            for (int x = 0; x < heightmap_width; x++) {
                heightmap_data[z, x] = 0;
            }
        }
        save();
    }

    // Register changes made to the terrain
    public void save() {
        terrain_data.SetHeights(0, 0, heightmap_data);
    }
    public void saveTextures() {
        terrain_data.SetAlphamaps(0, 0, alphamaps);
    }
    public void saveDetails(int layer=0) {
        terrain_data.SetDetailLayer(0, 0, layer, detail_layer[layer]);
    }
    public int getDetails(int layer, int x, int y) {
        return detail_layer[layer][x,y];
    }
    public void saveDetails(int layer,int x, int y, int val) {
        detail_layer[layer][x,y] = val;
    }

    // Get and set active brushes
    public void setBrush(Brush brush) {
        current_brush = brush;
    }
    public Brush getBrush() {
        return current_brush;
    }
    public TerrainData getData(){//Added
        return terrain_data;
    }
    public void setTextures(float[,,] a) {//Added
        alphamaps = a;
    }
    public void setOccupance(int x, int y,  Layers l = Layers.Rocky) {//Added
        occupance[(int)(x),(int)(y)] = (int)l;
    }
    public Layers getOccupance(int x, int y) {//Added
        return (Layers)occupance[(int)x,(int)y] ;
    }
    public Vector2 getPropDetAlpha() {//Added
        return new Vector2(prop_width, prop_height);
    }
    public int getDetLayers(){
        return totDet;
    }
    
    public VegetationConstraint getVegetationFromZone(Zone z){
        switch (z)
        {
            case Zone.FootHill: return GetComponent<FootHillsTrees>();
            case Zone.Montane: return GetComponent<MontaneTrees>();
            case Zone.Subalpine: return GetComponent<SubalpineTrees>();
            case Zone.Alpine: return GetComponent<AlpineZone>();
            case Zone.Nival: return GetComponent<AlpineZone>();
            default: return null;
        }
    }
    public static float[] randomWeights(int tot){
        float[] w = new float[tot];
        float sum = 0.0f;
        System.Random rand = new System.Random();
        for(int t = 0; t < tot; t++){
            w[t] = (float)rand.NextDouble();
            sum += w[t];
        }
        for(int t = 0; t < tot; t++){
            w[t] = w[t] / sum;
        }
        return w;
    }
}
