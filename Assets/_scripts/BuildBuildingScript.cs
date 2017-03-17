using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildBuildingScript : MonoBehaviour {

    public GameObject prefab;
    public Vector3 position;
    public int maxDepth;
    public int maxWidth;

    public void BuildObject() {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity) as GameObject;
        instance.GetComponent<FootprintGenerator>().setWidth(maxWidth);
        instance.GetComponent<FootprintGenerator>().setDepth(maxDepth);
    }
}
