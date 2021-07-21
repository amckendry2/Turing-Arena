using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Room : MonoBehaviour
{

    public int ID;
    public PathNode node;
    
    [SerializeField]
    public List<GameObject> spawns;
    [SerializeField]
    public float Width = 12;
    [SerializeField]
    private GameObject bullet;

    public bool hasBullet = false;
    private GameObject myBullet;
 
    public Dictionary<int, Tuple<Vector2, Vector2>> adjacentHallPaths;

    void Awake(){
        node = new PathNode(ID, transform.position);
    }

    public void SpawnBullet(){
        if(!hasBullet){
            hasBullet = true;
            myBullet = Instantiate(bullet, transform);
        }
    }

    public void TakeBullet(){
        hasBullet = false;
        Destroy(myBullet);
    }

    // void OnDrawGizmos(){
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawCube(transform.position, Vector3.one * 2);
    //     Handles.Label(transform.position + Vector3.right*2, ID.ToString());

    // }
}


