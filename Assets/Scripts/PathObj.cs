using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathObj : MonoBehaviour
{
    public int ID;
    public PathNode node;
    
    [SerializeField]
    public List<PathObj> connections;

    public virtual void GenerateNode(){}
    
}
