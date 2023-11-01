using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VPSMap : MonoBehaviour
{
    [SerializeField] public List<GameObject> meshRoots = new List<GameObject>();
    [SerializeField] public List<GameObject> groundRoots = new List<GameObject>();
    [SerializeField] public GameObject minimapSpriteObject;
}
