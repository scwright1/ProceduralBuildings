using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintGenerator : MonoBehaviour {

    //this is the maximum size the building footprint can possibly be
    [Range(1,9)]
    public int maxDepth = 1;
    [Range(1,9)]
    public int maxWidth = 1;

    private bool[][] footprintArray;

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
        footprintArray = new bool[maxDepth][];
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
                    if (footprintArray[i][j]) {
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
        footprintArray[0] = new bool[maxWidth];
        for (int i = 0; i < maxWidth; i++) {
            footprintArray[0][i] = (Random.value > 0.5f);
        }

        //we've built the back row, now we can fill in the middle rows
        //the number of rows to fill in is [1] ... maxDepth - 2
        //if the cell directly above has a value, and doesn't have a partner, then this one must be true
        for (int row = 1; row < maxDepth; row++) {
            //create the next row and drop into it for processing
            footprintArray[row] = new bool[maxWidth];
            for (int col = 0; col < maxWidth; col++) {
                //for each cell, check the cell above it first to see if we must be true, otherwise randomly assign
                bool siblingHasConnections = CheckSiblingConnections(row - 1, col);
                if (siblingHasConnections) {
                    //we can set this to a random value
                    footprintArray[row][col] = (Random.value > 0.5f);
                } else {
                    //this cell must be true so we don't break the rules
                    footprintArray[row][col] = true;
                }
            }
        }
    }

    private void BuildBetterFootprint() {
        InitializeFootprintArray();
        if(style == BuildingStyle.Symmetrical) {
            //do the front of the building
            GenerateFootprintFront();
            FillRow(1);
        }
    }

    private void InitializeFootprintArray() {
        for(int i = 0; i < maxDepth; i++) {
            footprintArray[i] = new bool[maxWidth];
        }
    }

    private void GenerateFootprintFront() {
        switch(style) {
            case BuildingStyle.Quirky: {
                    break;
                }
            case BuildingStyle.Symmetrical: {
                    //determine whether we're splitting evenly
                    int midPoint;
                    bool even;
                    bool value;
                    if(maxWidth % 2 == 0) {
                        midPoint = maxWidth / 2;
                        even = true;
                    } else {
                        midPoint = (maxWidth - 1) / 2;
                        even = false;
                    }

                    //for each value on the left, set the corresponding value on the right
                    for(int i = 0; i < midPoint; i++) {
                        if(i == 0) {
                            value = (Random.value > 0.5f);
                        } else {
                            value = CheckAndAssign(0, i);
                        }
                        footprintArray[0][i] = value;
                        footprintArray[0][maxWidth - (i + 1)] = value;
                    }
                    //handle middle blocks in odd width cases
                    if(!even) {
                        footprintArray[0][midPoint] = footprintArray[0][midPoint-1];
                    }
                    break;
                }
            default: {
                    break;
                }
        }
    }

    private bool CheckAndAssign(int row, int cell) {
        if(row == 0) {
            //row zero, only check horizontally
            bool lastSibling = footprintArray[row][cell - 1];
            if (cell > 1) {
                //check 2 cells
                if (lastSibling) {
                    //the last cell was set true, if the one before that was also set true, set this to whatever
                    bool secondSibling = footprintArray[row][cell - 2];
                    if (secondSibling) {
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
            //we have some other logic for different rows (as we have to check vertically as well
            return false;
        }
    }

    private void FillRow(int row) {
        for(int i = 0; i < maxWidth; i++) {
            if(!footprintArray[row-1][i]) {
                footprintArray[row][i] = true;
            }
        }
    }

    private bool CheckSiblingConnections(int x, int y) {

        //get the cell immediately above this one
        bool cellIsSet = footprintArray[x][y];

        if (cellIsSet) {
            //if the cell is on the edge, then it must have a sibling above
            if (y == 0 || y == maxWidth - 1) {
                //if this isn't the top row
                if(x > 0) {
                    if (footprintArray[x - 1][y]) {
                        //this cell has an active connection above, so we can set true and don't need to fill the new cell
                        return true;
                    }
                }
            } else {
                //we must set false if there are siblings either side (otherwise we end up with a disconnected wall)
                if (footprintArray[x][y - 1] || footprintArray[x][y + 1]) {
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
