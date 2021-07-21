using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExitObj : PathObj
{
    public int roomConnection;

    void Awake(){
        roomConnection = transform.parent.GetComponent<Room>().ID;
        // node = new ExitNode(ID, transform.position, roomConnection);
        Navigator nav = (Navigator)FindObjectOfType(typeof(Navigator));
        nav.AddNode(this);
    }

    public override void GenerateNode(){
        node = new ExitNode(ID, transform.position, roomConnection);
    }

    // void OnDrawGizmos(){
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawCube(transform.position, Vector3.one * 1);
    //     Handles.Label(transform.position + Vector3.right*2, ID.ToString());
    // }
}
