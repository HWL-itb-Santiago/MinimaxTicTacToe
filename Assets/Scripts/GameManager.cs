using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public enum States
{
    CanMove,
    CantMove
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public BoxCollider2D Collider;
    public GameObject token1, token2;
    public int Size = 3;
    public int[,] Matrix;
    [SerializeField] private States state = States.CanMove;
    public Camera Camera;


    void Start()
    {
        Instance = this;
        Matrix = new int[Size, Size];
        Calculs.CalculateDistances(Collider, Size);
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Matrix[i, j] = 0; // 0: desocupat, 1: fitxa jugador 1, -1: fitxa IA;
            }
        }
    }
    private void Update()
    {
        if (state == States.CanMove)
        {
            Vector3 m = Input.mousePosition;
            m.z = 10f;
            Vector3 mousepos = Camera.ScreenToWorldPoint(m);
            if (Input.GetMouseButtonDown(0))
            {
                if (Calculs.CheckIfValidClick((Vector2)mousepos, Matrix))
                {
                    state = States.CantMove;
                    if(Calculs.EvaluateWin(Matrix)==2)
                        StartCoroutine(WaitingABit());
                }
            }
        }
    }
    private IEnumerator WaitingABit()
    {
        yield return new WaitForSeconds(1f);
        PredicteMovement();
    }
    public void RandomAI()
    {
        int x;
        int y;
        do
        {
            x = UnityEngine.Random.Range(0, Size);
            y = UnityEngine.Random.Range(0, Size);
        } while (Matrix[x, y] != 0);
        DoMove(x, y, -1);
        state = States.CanMove;
    }
    public void DoMove(int x, int y, int team)
    {
        Matrix[x, y] = team;
        if (team == 1)
            Instantiate(token1, Calculs.CalculatePoint(x, y), Quaternion.identity);
        else
            Instantiate(token2, Calculs.CalculatePoint(x, y), Quaternion.identity);
        int result = Calculs.EvaluateWin(Matrix);
        switch (result)
        {
            case 0:
                Debug.Log("Draw");
                break;
            case 1:
                Debug.Log("You Win");
                break;
            case -1:
                Debug.Log("You Lose");
                break;
            case 2:
                if(state == States.CantMove)
                    state = States.CanMove;
                break;
        }
    }

    public int Minimax(int[,] board, bool isMax, int alpha, int betha)
    {
        int result = Calculs.EvaluateWin(board);

        if (result != 2)
            return result;

        if (isMax)
        {
            int maxAlpha = int.MinValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] != 0)
                        continue;
                    board[i, j] = 1;
                    int value = Minimax(board, !isMax, alpha, betha);
                    board[i, j] = 0;
                    alpha = Math.Max(alpha, value);
                    maxAlpha = Math.Max(maxAlpha, alpha);
                    if (value >= betha)
                        return maxAlpha;
                }
            }
            return maxAlpha;
        }
        else
        {
            int minBetha = int.MaxValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] != 0)
                        continue;
                    board[i, j] = -1;
                    int value = Minimax(board, !isMax, alpha, betha);
                    board[i, j] = 0;
                    betha = Math.Min(betha, value);
                    minBetha = Math.Min(minBetha, betha);
                    if (value <= alpha)
                        return minBetha;
                }
            }
            return minBetha;
        }
    }
    public void PredicteMovement()
    {
        int bestScore = int.MaxValue;
        int bestX = -1, bestY = -1;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Matrix[i, j] != 0)
                    continue;

                Matrix[i, j] = -1;
                int score = Minimax(Matrix, true, int.MinValue, int.MaxValue);
                Matrix[i, j] = 0;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestX = i;
                    bestY = j;
                }
            }
        }
        if (bestX != -1 && bestY != -1)
        {
            Debug.Log(bestScore);
            DoMove(bestX, bestY, -1);
        }
    }

    public void DebugMatrix(int[,] matrix)
    {
        string text = "";
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                text += matrix[i, j] + " ";
            }
            text += "\n";
        }
        Debug.Log(text);
    }
}
