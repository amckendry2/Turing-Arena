using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public readonly List<PathNode> nodes;
    public readonly float totalDistance;
    public PathNode currentTarget {get { return nodes[currentIndex];}}

    private int currentIndex = 0;

    public Path(List<PathNode> n){
        this.nodes = n;
        this.totalDistance = n[0].distance;
    }

    public void Advance(){
         currentIndex = currentIndex == nodes.Count -1 ? currentIndex : currentIndex + 1;
    }

    public void Backtrack(){
        currentIndex = currentIndex == 0 ? 0 : currentIndex - 1;
    }

    public float GetRemainingPercentage(Vector2 pos){
        float distanceToNext = (pos - currentTarget.pos).magnitude;
        return ((distanceToNext + currentTarget.distance) / totalDistance) * 100;
    }
}
