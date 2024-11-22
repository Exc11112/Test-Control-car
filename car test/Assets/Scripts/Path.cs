using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Path;

public class Path : MonoBehaviour
{
    public Color lineColor;

    [System.Serializable]
    public class PathNode
    {
        public Transform nodeTransform;
        public List<Transform> nextNodes = new List<Transform>(); // Supports branching paths
    }
    public List<PathNode> pathNodes = new List<PathNode>();
    private List<Transform> nodes = new List<Transform>();

    void OnDrawGizmos()
    {
        Gizmos.color = lineColor;

        Transform[] pathTransforms = GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for(int i =0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != transform) 
            {
                nodes.Add(pathTransforms[i]);
            }
        }

        for(int i = 0; i < nodes.Count; i++)
        {
            Vector3 currentNode = nodes[i].position;
            Vector3 previousNode = Vector3.zero;

            if (i>0)
            {
                previousNode = nodes[i - 1].position;
            }
            else if (i == 0 && nodes.Count > 1) 
            {
                previousNode = nodes[nodes.Count - 1].position;
            }

            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawWireSphere(currentNode, 0.3f);
        }

        foreach (var pathNode in pathNodes)
        {
            if (pathNode.nodeTransform == null) continue;

            foreach (var nextNode in pathNode.nextNodes)
            {
                if (nextNode != null)
                {
                    Gizmos.DrawLine(pathNode.nodeTransform.position, nextNode.position);
                }
            }

            Gizmos.DrawWireSphere(pathNode.nodeTransform.position, 0.3f);
        }
    }
}
