using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class HallObj : PathObj
{
    void Awake(){
        // node = new HallNode(ID, transform.position, connections);
        Navigator nav = (Navigator)FindObjectOfType(typeof(Navigator));
        nav.AddNode(this);
    }

    public override void GenerateNode(){
        node = new HallNode(ID, transform.position, connections);
    }

    // void OnDrawGizmos(){
    //     Gizmos.color = Color.cyan;
    //     Gizmos.DrawCube(transform.position, Vector3.one * 1);
    //     Handles.Label(transform.position + Vector3.right*2, ID.ToString());
    // }
}
