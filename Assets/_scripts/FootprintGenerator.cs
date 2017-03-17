using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class FootprintGenerator : MonoBehaviour {

    public enum Type {
        Corner,
        Straight
    }

    public enum Style {
        Chamfered,
        RightAngled
    }

    public Dictionary<Type, string> materials = new Dictionary<Type, string>();

    //block class (each building block of the building)
    public class BuildingBlock {
        public Type blockType;
        public Vector2 footprintPosition;
        public bool decorated;

        public BuildingBlock() {
            blockType = Type.Straight;
        }

        public BuildingBlock(Type block, Vector2 pos, bool hasProp) {
            blockType = block;
            footprintPosition = pos;
            decorated = hasProp;
        }

        public Vector2 getPosition() { return footprintPosition; }

        public Type getType() { return blockType; }
    }

    //this is the maximum size the building footprint can possibly be
    [Range(1,9)]
    public int maxDepth = 1;
    [Range(1,9)]
    public int maxWidth = 1;

    private BuildingBlock[][] footprintArray;

    private float baseOffset = 0.5f;

    private GameObject root;

    private float offsetX;
    private float offsetZ;

    public enum BuildingStyle {
        Symmetrical,
        Quirky
    }

    public BuildingStyle style;

    private void Awake() {
        materials.Add(Type.Corner, "_materials/Mat_Building_Yellow_Vert_256");
        materials.Add(Type.Straight, "_materials/Mat_Building_White_Vert_256");
        footprintArray = new BuildingBlock[maxDepth][];
        offsetX = 0 - (maxWidth / 2);
        offsetZ = 0 - (maxDepth / 2);
        root = transform.gameObject;
        ConstructBuilding();
    }

    void Start() { 
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //meshFilters[i].gameObject.active = false;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        //transform.gameObject.active = true; 
        transform.gameObject.SetActive(true);
        Material mat = Resources.Load(materials[Type.Straight], typeof(Material)) as Material;
        transform.gameObject.GetComponent<Renderer>().material = mat;



        //only do this to randomly cycle
        //StartCoroutine(GenerateBuilding());
    }

    private void BuildFootprint() {
        InitializeFootprintArray();
        if (style == BuildingStyle.Symmetrical) {
            //do the front of the building
            GenerateFootprintRow(0);
            for(int i = 1; i < maxDepth; i++) {
                GenerateFootprintRow(i);
            }
        }
    }

    private void ConstructBuilding() {
        BuildFootprint();
        for (int height = 0; height < UnityEngine.Random.Range(5, 9); height++) {
            for (int i = 0; i < maxDepth; i++) {
                for (int j = 0; j < maxWidth; j++) {
                    if (footprintArray[i][j] != null) {
                        AddBlock(j, i, height, footprintArray[i][j].getType());
                    }
                }
            }
        }
    }

    IEnumerator GenerateBuilding() {
        while(true) {
            ConstructBuilding();
            yield return new WaitForSeconds(1);
            DestroyChildObjects();
        }
    }

    private void DestroyChildObjects() {
        List<GameObject> children = new List<GameObject>();
        foreach(Transform t in transform) {
            children.Add(t.gameObject);
        }
        children.ForEach(child => Destroy(child));
    }

    private void InitializeFootprintArray() {
        for(int i = 0; i < maxDepth; i++) {
            footprintArray[i] = new BuildingBlock[maxWidth];
        }
    }

    private void GenerateFootprintRow(int row) {
        switch(style) {
            case BuildingStyle.Quirky: {
                    break;
                }
            case BuildingStyle.Symmetrical: {
                    //determine whether we're splitting evenly
                    int midPoint;
                    bool even, value, fill = false;
                    if(maxWidth % 2 == 0) {
                        midPoint = maxWidth / 2;
                        even = true;
                    } else {
                        midPoint = (maxWidth - 1) / 2;
                        even = false;
                    }
                    //for each value on the left, set the corresponding value on the right
                    for (int i = 0; i < midPoint; i++) {
                        value = CheckAndAssign(row, i);
                        if (value && !fill) { fill = true; }
                        if (value) {
                            footprintArray[row][i] = new BuildingBlock(Type.Straight, new Vector2(row, i), false);
                            footprintArray[row][maxWidth - (i + 1)] = new BuildingBlock(Type.Straight, new Vector2(row, (maxWidth - (i+1))), false);
                        }
                    }
                    //handle middle blocks in odd width cases
                    if (!even) {
                        footprintArray[row][midPoint] = footprintArray[row][midPoint - 1];
                    }

                    //handle cases where we don't fill anything in on the first row (in which case, just fill in the first row)
                    if (!fill) {
                        for (int i = 0; i < footprintArray[row].Length; i++) {
                            footprintArray[row][i] = new BuildingBlock(Type.Straight, new Vector2(row, i), false);
                        }
                    }
                    break;
                }
            default: {
                    break;
                }
        }
    }

    private bool CheckAndAssign(int row, int cell) {


        if(cell == 0) {
            if(row == 0) {
                return (Random.value > 0.5f);
            } else if (row == 1) {
                //1 vertical sibling
                if(EntryExists(row-1, cell)) {
                    return true;
                }
                return (Random.value > 0.5f);
            } else {
                if(EntryExists(row-1, cell)) {
                    if(EntryExists(row-2, cell)) {
                        return (Random.value > 0.5f);
                    }
                    return true;
                }
                return (Random.value > 0.5f);
            }
        }

        if(row == 0) {
            //this is the first row, so we don't need to check vs1 or vs2;
            if(cell > 1) {
                //we potentially have 2 pre-existing cells that are filled
                if(EntryExists(row, cell - 1)) {
                    //the immediate horizontal neighbour exists, so check the next
                    if(EntryExists(row, cell - 2)) {
                        //2 generations of neighbour already exist, so set this to whatever we want
                        return (Random.value > 0.5f);
                    }
                    //this will be the second contiguous block, so set to true;
                    return true;
                }
                return (Random.value > 0.5f);
            }
            if(EntryExists(row, cell-1)) {
                return true;
            }
            return (Random.value > 0.5f);
        }

        //don't need to worry about cell = 0 or row == 0, so just move on
        if(row == 1) {
            if(EntryExists(row-1, cell)) {
                //vertically we have a match, so we must return true regardless of whatever else is set
                return true;
            } else {
                //this is the first row, so we don't need to check vs1 or vs2;
                if (cell > 1) {
                    //we potentially have 2 pre-existing cells that are filled
                    if (EntryExists(row, cell - 1)) {
                        //the immediate horizontal neighbour exists, so check the next
                        if (EntryExists(row, cell - 2)) {
                            //2 generations of neighbour already exist, so set this to whatever we want
                            return (Random.value > 0.5f);
                        }
                        //this will be the second contiguous block, so set to true;
                        return true;
                    }
                    return (Random.value > 0.5f);
                }
                if (EntryExists(row, cell - 1)) {
                    return true;
                }
                return (Random.value > 0.5f);
            }
        }

        if(EntryExists(row-1,cell)) {
            if(EntryExists(row-2,cell)) {
                return (Random.value > 0.5f);
            }
            return true;
        }

        return (Random.value > 0.5f);
    }

    private bool EntryExists(int row, int cell) {
        if (footprintArray[row][cell] != null) {
            return true;
        }
        return false;
    }

    private void FillRow(int row) {
        for(int i = 0; i < maxWidth; i++) {

            if (i == 0 || i == maxWidth - 1) {
                //at the moment, this should always be filled in to make the building wall
                footprintArray[row][i] = new BuildingBlock(Type.Straight, new Vector2(row, i), false);
            } else {
                if (footprintArray[row - 1][i] == null) {
                    footprintArray[row][i] = new BuildingBlock(Type.Straight, new Vector2(row,i), false);
                }
            }
        }
    }

    private void AddBlock(int x, int y, int height, Type sectionType) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Material mat = Resources.Load(materials[sectionType], typeof(Material)) as Material;
        cube.gameObject.GetComponent<Renderer>().material = mat;
        cube.transform.SetParent(root.transform);
        cube.transform.localPosition = new Vector3(offsetX + x, height + baseOffset, offsetZ + y);
        cube.transform.localRotation = root.transform.localRotation;
    }

    public void setWidth(int w) {
        maxWidth = w;
    }

    public void setDepth(int d) {
        maxDepth = d;
    }
}
