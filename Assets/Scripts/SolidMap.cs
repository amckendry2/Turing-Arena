using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class SolidMap : MonoBehaviour {

    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;

    private bool[,] map;
    private int cellSize;
    private int mapWidth;
    private int mapHeight;

    public static SolidMap Instance {get; private set;}

    void Awake(){
        if(Instance != null){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start(){
        this.cellSize = (int)tilemap.cellSize.x;
        BoundsInt bounds = new BoundsInt(Vector3Int.zero, new Vector3Int(width, height, 1));
        TileBase[] rawArray = tilemap.GetTilesBlock(bounds);
        map = new bool[width, height];
        for (int i = 0; i < rawArray.Length; i++){
            int x = i % width;
            int y = Mathf.FloorToInt(i / width);
            map[x,y] = rawArray[i] != null;
        }
        mapWidth = map.GetLength(0);
        mapHeight = map.GetLength(1);
    }

    public bool this[int x, int y]{
        get { return map[x,y]; }
    }

    public CollisionResult CheckPointCollision(Vector2 point, Vector2 origin, float resolution){
        int x = point.x % 1 > .5f ? Mathf.CeilToInt(point.x) : Mathf.FloorToInt(point.x);
        int y = point.y % 1 > .5f ? Mathf.CeilToInt(point.y) : Mathf.FloorToInt(point.y);
        if(map[x, y]){
            // float halfCell = (float)cellSize / 2;
            // float boxX1 = x * cellSize - halfCell;
            // float boxY1 = y * cellSize - halfCell;
            // float boxX2 = x + cellSize;
            // float boxY2 = y + cellSize;
            // float closestX = Mathf.Clamp(origin.x, boxX1, boxX2 - .5f);    
            // float closestY = Mathf.Clamp(origin.y, boxY1, boxY2 - .5f);
            // return new CollisionResult(new Vector2(closestX, closestY));
            Vector2 offset = (origin - point).normalized * resolution;
            return new CollisionResult(point + offset);
        }
        return new CollisionResult(false);
    }

    public CollisionResult CheckCollision(Vector2 actorPos, float colliderSize){ //TESTING
        List<Vector2> collisions = new List<Vector2>();

        float halfCollider = colliderSize / 2;
        float halfCell = (float)cellSize / 2;

        float x = actorPos.x;
        float y = actorPos.y;

        float xCoord = x / cellSize;
        float yCoord = y / cellSize;

        int playerBoxX = xCoord % 1 > .5f ? Mathf.CeilToInt(xCoord) : Mathf.FloorToInt(xCoord);
        int playerBoxY = yCoord % 1 > .5f ? Mathf.CeilToInt(yCoord) : Mathf.FloorToInt(yCoord);

        for (int xInc = -1; xInc <= 1; xInc++){
            for (int yInc = -1; yInc <= 1; yInc++){
                
                if(xInc == 0 && yInc == 0) continue;
                
                int boxX = playerBoxX + xInc;
                int boxY = playerBoxY + yInc;

                if(!map[boxX, boxY]) continue;
                if(Mathf.Clamp(boxX, 0, mapWidth - 1) != boxX) continue;
                if(Mathf.Clamp(boxY, 0, mapHeight - 1) != boxY) continue;
                
                float boxX1 = boxX * cellSize - halfCell;
                float boxY1 = boxY * cellSize - halfCell;
                float boxX2 = boxX1 + cellSize;
                float boxY2 = boxY1 + cellSize;

                float closestX = Mathf.Clamp(x, boxX1, boxX2);    
                float closestY = Mathf.Clamp(y, boxY1, boxY2);
                float deltaX = x - closestX;
                float deltaY = y - closestY;
                bool collided = (deltaX * deltaX + deltaY * deltaY) < halfCollider * halfCollider;
                
                if(collided){                    
                    Vector2 collisionPoint = new Vector2(closestX, closestY);
                    collisions.Add(collisionPoint);
                } 
            }
        }

        if(collisions.Count == 0) return new CollisionResult(false);

        Vector2 bestCollPoint = new Vector2();
        float shortestDistance = 0;
        bool firstCheck = true;
        foreach(Vector2 collPoint in collisions){
            float distance = (collPoint - new Vector2(x,y)).sqrMagnitude;
            if ((distance < shortestDistance) || firstCheck) {
                firstCheck = false;
                shortestDistance = distance;
                bestCollPoint = collPoint;
            } 
        }
        Debug.DrawLine(new Vector2(x, y), new Vector2(bestCollPoint.x, bestCollPoint.y), Color.magenta, .1f);
        return new CollisionResult(bestCollPoint);
    }  
}

public class CollisionResult{
    public readonly bool DidCollide = true;
    public readonly Vector2 CollisionPoint;
    public CollisionResult(bool didCollide){
        this.DidCollide = didCollide;
        CollisionPoint = new Vector2();
    }
    public CollisionResult(Vector2 point){
        this.DidCollide = true;
        this.CollisionPoint = point;
    }
}

    // public List<Vector2> GetPath(int startX, int startY, int targetX, int targetY){
        
    //     if(!IsInMap(startX, startY) || !IsInMap(targetX, targetY)) return null;

    //     Location current = null;
    //     var start = new Location { X = startX, Y = startY, G = 0 };
    //     var target = new Location { X = targetX, Y = targetY };
    //     var openList = new List<Location>();
    //     var closedList = new List<Location>();

    //     openList.Add(start);

    //     while (openList.Count > 0) {

    //         var lowest = openList.Min(l => l.F);
    //         current = openList.First(l => l.F == lowest);

    //         closedList.Add(current);
    //         openList.Remove(current);

            
    //         // if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null){
    //             // Debug.Log("found target at " + current.X + ", " + current.Y);
    //         //     break;
    //         // }

    //         List<Location> adjacentSquares = GetAdjacentSquares(current.X, current.Y);
    //             // Debug.Log("checking " + current.X + ", " + current.Y + " adjacents ----------");
    //         foreach(var adjacentSquare in adjacentSquares){

    //             if(adjacentSquare.X == target.X && adjacentSquare.Y == target.Y){
    //             List<Vector2> path = new List<Vector2>();
    //             path.Add(new Vector2(adjacentSquare.X + .5f, adjacentSquare.Y + .5f));
    //             Location here = current;
    //             while(here != null){
    //                 path.Insert(0, new Vector2(here.X + .5f, here.Y + .5f));
    //                 here = here.Parent;
    //             }
    //             return path;
    //         }
                
    //             if(closedList.FirstOrDefault(l => l.X == adjacentSquare.X && l.Y == adjacentSquare.Y) != null){
    //                 continue;
    //             }

    //             if(openList.FirstOrDefault(l => l.X == adjacentSquare.X && l.Y == adjacentSquare.Y) == null){
    //                 adjacentSquare.G = current.G + 1;
    //                 adjacentSquare.H = ComputeHScore(adjacentSquare.X, adjacentSquare.Y, target.X, target.Y);
    //                 adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
    //                 adjacentSquare.Parent = current;

    //                 openList.Insert(0, adjacentSquare);
               
    //             } else {
    //                 if (current.G + 1 + adjacentSquare.H < adjacentSquare.F){
    //                     adjacentSquare.G = current.G + 1;
    //                     adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
    //                     adjacentSquare.Parent = current;
    //                 }
    //             }
    //         }
    //     }
    //     return null;
    // }

    // private List<Location> GetAdjacentSquares(int cenX, int cenY){
    //     var allAdjacent = new List<Location>();
    //     for (var xInc = -1; xInc < 2; xInc ++){
    //         for (var yInc = -1; yInc < 2; yInc ++){
    //             int x = cenX + xInc;
    //             int y = cenY + yInc;
    //             if((xInc == 0 || yInc == 0) && xInc + yInc != 0 && IsInMap(x, y)){
    //                 allAdjacent.Add( new Location { X = x, Y = y });
    //             }
    //         }
    //     }
    //     var map = this.map;
    //     return allAdjacent.Where(l => !map[l.X, l.Y]).ToList();
    // }

    // private int ComputeHScore(int x, int y, int targetX, int targetY){
    //     return Mathf.Abs(targetX - x) + Mathf.Abs(targetY - y);
    // }

    // private bool IsInMap(int x, int y){
    //     return Mathf.Clamp(x, 0, mapWidth -1) == x 
    //         && Mathf.Clamp(y, 0, mapHeight - 1) == y;
    // }

    // private class Location{
    //     public int X;
    //     public int Y;
    //     public int F;
    //     public int G;
    //     public int H;
    //     public Location Parent;
    //     public Location GetClone(){
    //         return (Location) this.MemberwiseClone();
    //     }
    // }
    