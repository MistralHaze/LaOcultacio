using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class PathFinding : MonoBehaviour
{

    Grid grid;

    void Awake()
    {
        grid = GetComponent<Grid>();
    }
    /*
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            FindPath(seeker.position, target.position);
        }
        
    }*/
    /*
    public List<Vector2> FindPath(Vector3 startPos, Vector3 targetPos, Transform seeker, Transform target)
    {

        List<Vector2> path = new List<Vector2>();

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closeSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closeSet.Add(currentNode);

            if (currentNode == targetNode)      //path found
            {
                sw.Stop();
                //print("Path found " + sw.ElapsedMilliseconds + " ms");
                path = RetracePath(startNode, targetNode);
                return path;
            }
            /* foreach neighbour of the current node
                if neighbour is not traversable or neighbour is in closed
                    skip to the next neighbour
            
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closeSet.Contains(neighbour))
                {
                    continue;           //skips this iteration of the bucle
                }
                //if the new path no the neighbour is shorter OR neighbour is not in open
                /*
                set fcost of neighbour
                set parent of neighbour to current
                if neighbour is not in open
                    add neighbour to open    
                */
                /*
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbour);
                    }

                }
            }
        }

        return null;
    }
    */
    public List<Vector2> FindPathToBlocked(Vector3 startPos, Vector3 targetPos, Transform seeker, Transform target)
    {
        List<Vector2> path = new List<Vector2>();

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        //print(endNode.walkable);
        if (!startNode.walkable)
            foreach (Node node in grid.GetNeighbours(startNode))
            {
                //print(node.walkable);
                if (node.walkable && node != targetNode)
                {
                    startNode = node;
                    break;
                }
            }

        if (!targetNode.walkable)
            foreach (Node node in grid.GetNeighbours(targetNode))
            {
                //print(node.walkable);
                if (node.walkable && node != startNode)
                {
                    targetNode = node;
                    break;
                }
            }


        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closeSet = new HashSet<Node>();

        openSet.Add(startNode);


        while (openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            closeSet.Add(currentNode);

            if (currentNode == targetNode)      //path found
            {
                //print("Path found " + sw.ElapsedMilliseconds + " ms");
                path = RetracePath(startNode, targetNode);
                return path;
            }
            /* foreach neighbour of the current node
                if neighbour is not traversable or neighbour is in closed
                    skip to the next neighbour
            */
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closeSet.Contains(neighbour))
                {
                    continue;           //skips this iteration of the bucle
                }
                //if the new path no the neighbour is shorter OR neighbour is not in open
                /*
                set fcost of neighbour
                set parent of neighbour to current
                if neighbour is not in open
                    add neighbour to open    
                */

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbour);
                    }

                }
            }
        }

        return null;
    }

    List<Vector2> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        List<Vector2> positions = new List<Vector2>();

        int size = path.Count;

        for (int i = 0; i < size; i++)
        {
            //UnityEngine.Debug.Log(size);
            positions.Add(path[i].worldPosition);
            //positions[i] = path[i].worldPosition;
        }

        grid.path = path;

        return positions;
    }
    int GetDistance(Node nodeA, Node nodeB)     //magic
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);

        return 14 * distX + 10 * (distY - distX);
    }
}
