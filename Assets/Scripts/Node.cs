using System.Collections;
using System.Collections.Generic;


public class Node
{
    public int value;
    public int alpha;
    public int beta;

    public int[,] matrix;

    public Node parent;
    public List<Node> children = new();

    public Node(int[,] matrix, Node parent)
        { this.matrix = matrix; this.parent = parent; }
}
