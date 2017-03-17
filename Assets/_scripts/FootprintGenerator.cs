using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintGenerator : MonoBehaviour {

    public enum Type {
        Corner,
        Straight
    }

    public enum Style {
        Chamfered,
        RightAngled
    }

    //block class (each building block of the building)
    public class BuildingBlock {
        public Type blockType;
        public Vector2 footprintPosition;
        public bool decorated;

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

    void Start() {
        footprintArray = new BuildingBlock[maxDepth][];
        offsetX = 0 - (maxWidth / 2);
        offsetZ = 0 - (maxDepth / 2);
        root = transform.gameObject;
        StartCoroutine(GenerateBuilding());
    }

    private void BuildFootprint() {
        //BuildBasicFootprint();
        BuildBetterFootprint();
    }

    private void ConstructBuilding() {
        BuildFootprint();
        for (int height = 0; height < Random.Range(5, 9); height++) {
            for (int i = 0; i < maxDepth; i++) {
                for (int j = 0; j < maxWidth; j++) {
                    if (footprintArray[i][j] != null) {
                        AddBlock(j, i, height);
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

    private void BuildBasicFootprint() {
        //a wall has to be at least 1 block wide, and attached to another block on one edge
        //we need to iterate through each of the rows of the array, and assign a true/false value to whether there's a block there
        //we don't need to fill in the inner blocks, they're just wasted space, but there must be a contiguous wall

        //we define the array as "depth" and "width" so that we can look at it from the top down:

        /**
         * d [ ][X][X][ ][ ]
         * e [X][X][X][ ][X]
         * p [X][X][X][X][X]
         * t [X][X][X][X][ ]
         * h [ ][ ][ ][X][X]
         *    w  i  d  t  h
         **/

        //build the back row
        footprintArray[0] = new BuildingBlock[maxWidth];
        for (int i = 0; i < maxWidth; i++) {
            bool set = (Random.value > 0.5f);
            if(set) {
                footprintArray[0][i] = new BuildingBlock(Type.Straight, new Vector2(0, i), false);
            }
        }

        //we've built the back row, now we can fill in the middle rows
        //the number of rows to fill in is [1] ... maxDepth - 2
        //if the cell directly above has a value, and doesn't have a partner, then this one must be true
        for (int row = 1; row < maxDepth; row++) {
            //create the next row and drop into it for processing
            footprintArray[row] = new BuildingBlock[maxWidth];
            for (int col = 0; col < maxWidth; col++) {
                //for each cell, check the cell above it first to see if we must be true, otherwise randomly assign
                bool siblingHasConnections = CheckSiblingConnections(row - 1, col);
                if (siblingHasConnections) {
                    //we can set this to a random value
                    if(Random.value > 0.5f) {
                        footprintArray[row][col] = new BuildingBlock(Type.Straight, new Vector2(row, col), false);
                    }
                } else {
                    //this cell must be true so we don't break the rules
                    footprintArray[row][col] = new BuildingBlock(Type.Straight, new Vector2(row, col), false);
                }
            }
        }
    }

    private void BuildBetterFootprint() {
        InitializeFootprintArray();
        if(style == BuildingStyle.Symmetrical) {
            //do the front of the building
            GenerateFootprintRow(0);
        }
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
                        if (i == 0) {
                            value = (Random.value > 0.5f);
                        } else {
                            value = CheckAndAssign(row, i);
                        }
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
        bool lastSibling = false;
        if(row == 0) {
            //row zero, only check horizontally
            if(footprintArray[row][cell-1] != null) {
                lastSibling = true;
            }
            if (cell > 1) {
                //check 2 cells
                if (lastSibling) {
                    //the last cell was set true, if the one before that was also set true, set this to whatever
                    //bool secondSibling = footprintArray[row][cell - 2];
                    if (footprintArray[row][cell-2] != null) {
                        //the second sibling was also set to true, so this satisfies the logic that there must always be 2 contiguous blocks
                        //therefore, we can set this to whatever
                        return (Random.value > 0.5f);
                    } else {
                        //this is the second contiguous block, so we must set it to true
                        return true;
                    }
                }
                //the last cell was set false, so we can set this one to whatever
                return (Random.value > 0.5f);
            } else {
                //cell can only be either 1 or 0, and thus must be whatever the cell next to it is
                if (lastSibling) {
                    return true;
                }
                return (Random.value > 0.5f);
            }
        } else {
            //we have some other logic for different rows (as we have to check vertically as well)

            return false;
        }
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

    private bool CheckSiblingConnections(int x, int y) {

        //get the cell immediately above this one
        bool cellIsSet = false;
        if(footprintArray[x][y] != null) {
            cellIsSet = true;
        }

        if (cellIsSet) {
            //if the cell is on the edge, then it must have a sibling above
            if (y == 0 || y == maxWidth - 1) {
                //if this isn't the top row
                if(x > 0) {
                    if (footprintArray[x - 1][y] != null) {
                        //this cell has an active connection above, so we can set true and don't need to fill the new cell
                        return true;
                    }
                }
            } else {
                //we must set false if there are siblings either side (otherwise we end up with a disconnected wall)
                if ((footprintArray[x][y - 1] != null) || (footprintArray[x][y + 1] != null)) {
                    return false;
                }
            }
        }
        return false;
    }

    private void AddBlock(int x, int y, int height) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Material mat = Resources.Load("_materials/Mat_Building_White_Vert_256", typeof(Material)) as Material;
        cube.gameObject.GetComponent<Renderer>().material = mat;
        cube.transform.SetParent(root.transform);
        cube.transform.localPosition = new Vector3(offsetX + x, height + baseOffset, offsetZ + y);
        cube.transform.localRotation = root.transform.localRotation;
    }
}
