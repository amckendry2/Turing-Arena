using UnityEngine;
using System.Collections.Generic;

public class PathNode
{
    public readonly int ID;
    public Vector2 pos;
    public float distance {get; private set;}

    public PathNode(int parent, Vector2 pos){
        this.ID = parent;
        this.pos = pos;// + (Vector2.one * Random.Range(-7, 8) * .1f);
    }

    public void setDistance(float dist){
        this.distance = dist;
    }
}

public class HallNode : PathNode{
    
    List<PathObj> Connections;

    public HallNode(int parent, Vector2 pos, List<PathObj> connections) : base(parent, pos){
        this.Connections = connections;
    }
}

public class ExitNode : PathNode
{
    public int ExitID;

    public ExitNode(int parent, Vector2 pos, int exitID) : base(parent, pos) {
        this.ExitID = exitID;
    }
}
 