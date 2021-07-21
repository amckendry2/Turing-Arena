using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Navigator : MonoBehaviour
{

    private List<PathObj> pathObjs = new List<PathObj>();

    [SerializeField]
    public List<Room> rooms = new List<Room>();
    
    public static Navigator Instance;

    void Awake(){
        if(Instance != null){
            Debug.Log("Trying to instantiate multiple singletons!");
            Destroy(gameObject);
        } else {
            Instance = this;
            // DontDestroyOnLoad(Instance);
        }
    }

    void Start(){
        int nextID = rooms.Count;
        foreach(PathObj obj in pathObjs){
            obj.ID = nextID;
            nextID++;
            if(obj is HallObj){
                foreach(PathObj connObj in obj.connections){
                    if(connObj is ExitObj){
                        connObj.connections = new List<PathObj>(){obj};
                    }
                }
            }
        }
        foreach(PathObj obj in pathObjs){
            obj.GenerateNode();
        }
    }

    public void AddNode(PathObj node){
        pathObjs.Add(node);
    }

    public Path GeneratePathInHall(Vector2 position, int endID){
        float bestDistance = 0;
        PathObj bestNode = null;
        foreach(PathObj obj in pathObjs){
            PathNode node = obj.node;
            float distance = (position - node.pos).sqrMagnitude;
            if(bestNode == null || distance < bestDistance){
                bestNode = obj;
                bestDistance = distance;
            }
        }
        List<PathNode> path = WalkRoutes(bestNode, endID, new List<int>());
        path.Insert(0, new PathNode(69, position));
        // path.Add(GetRoom(endID).node);
        return ProcessNodes(path);
    }

    public Room GetClosestRoom(Vector2 pos){
        float bestDistance = 0;
        Room bestRoom = null;
        foreach(Room room in rooms){
            Vector2 roomPos = new Vector2(room.transform.position.x, room.transform.position.y);
            float distance = (pos - roomPos).sqrMagnitude;
            if(bestRoom == null || distance < bestDistance){
                bestRoom = room;
                bestDistance = distance;
            }
        }
        return bestRoom;
    }

    public Path GeneratePathFromRoom(int startID, int endID){
        List<int> blockedList = new List<int>();
        List<ExitObj> startCandidates = pathObjs.Where(n => {
            ExitObj exit = n as ExitObj;
            if(exit != null)
                return exit.roomConnection == startID;
            return false;
        }).Select(e => (ExitObj)e).ToList();

        List<List<PathNode>> paths = new List<List<PathNode>>();
        foreach(PathObj node in startCandidates){
            List<PathNode> path = WalkRoutes(node, endID, blockedList);
            if(path != null){
                paths.Add(path);
            }
        }

        List<PathNode> chosenPath = paths[Random.Range(0, paths.Count)];
        return ProcessNodes(chosenPath);
    }

    public List<PathNode> WalkRoutes(PathObj start, int endID, List<int> blockedList){
        
        List<int> myBlockedList = new List<int>(blockedList);
        myBlockedList.Add(start.ID);

        List<PathObj> viableConnections = start.connections.Where(c => {
            ExitObj exit = c as ExitObj;
            if(exit != null)
                return exit.roomConnection == endID;
            return !myBlockedList.Contains(c.ID);
        }).ToList();

        if(viableConnections.Count == 0) 
            return null;

        viableConnections.Shuffle();

        
        List<PathNode> myPath = new List<PathNode>();
        foreach(PathObj connection in viableConnections){
            myPath = new List<PathNode>();
            ExitObj exit = connection as ExitObj;
            if(exit != null && exit.roomConnection == endID){
                myPath.Insert(0, GetRoom(endID).node);
                myPath.Insert(0, connection.node);
                myPath.Insert(0, start.node);
                return myPath;
            }
                
            myPath = WalkRoutes(connection, endID, myBlockedList);
            if(myPath != null)
                break;
        }

        if(myPath == null)
            return null;
        
        myPath.Insert(0, start.node);
        return myPath;
    }


    private Path ProcessNodes(List<PathNode> path){
        PathNode previous = null;
        for(int i = path.Count - 1; i >= 0; i--){
            if(i == path.Count - 1){
                previous = path[i];
                path[i].setDistance(0);
                continue;
            } 
            PathNode current = path[i];
            float distance = (current.pos - previous.pos).magnitude + previous.distance;
            current.setDistance(distance);  
            Debug.DrawLine(current.pos, previous.pos, Color.white, 1f);
            previous = current;
        }
        // Debug.Log(path[path.Count - 2].distance);
        return new Path(path);
    }

    public Room GetRoom(int roomNum){
        return rooms.FirstOrDefault(r => r.ID == roomNum);
    }

    public Room GetRoomFromPoint(Vector2 point){
        foreach(Room room in rooms){
            Vector3 roomPos = room.transform.position;
            float halfWidth = room.Width / 2;
            float roomX1 = roomPos.x - halfWidth;
            float roomX2 = roomPos.x + halfWidth;
            float roomY1 = roomPos.y - halfWidth;
            float roomY2 = roomPos.y + halfWidth;
            if(Mathf.Clamp(point.x, roomX1, roomX2) == point.x 
            && Mathf.Clamp(point.y, roomY1, roomY2) == point.y)
                return room;
        }
        return null;
    }

    public int RoomCount {get {return rooms.Count;}}

    public List<int> GetRoomInts { get {return rooms.Select(r => r.ID).ToList();}}
}


