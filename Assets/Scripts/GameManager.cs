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
    public int Minimax(int[,] board, int depth, bool isMaximizing, int alpha, int beta)
    {
        int result = Calculs.EvaluateWin(board);
        if (result != 2) // 2 significa que aún no ha terminado
            return result;

        if (isMaximizing)
        {
            int maxEval = int.MinValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = -1;
                        int eval = Minimax(board, depth + 1, !isMaximizing, alpha, beta);
                        board[i, j] = 0;
                        maxEval = Math.Max(maxEval, eval);
                        alpha = Math.Max(alpha, eval);
                        if (beta <= alpha) // Poda alfa-beta
                            return maxEval;
                    }
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 1;
                        int eval = Minimax(board, depth + 1, !isMaximizing, alpha, beta);
                        board[i, j] = 0;
                        minEval = Math.Min(minEval, eval);
                        beta = Math.Min(beta, eval);
                        if (beta <= alpha) // Poda alfa-beta
                            return minEval;
                    }
                }
            }
            return minEval;
        }
    }
    public void PredicteMovement()
    {
        List<(int, int)> bestMoves = new List<(int, int)> ();
        int bestScore = int.MinValue;
        int bestX = -1, bestY = -1;

        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (Matrix[i, j] != 0)
                    continue;

                Matrix[i, j] = -1;
                int score = Minimax(Matrix, 0, false, int.MinValue, int.MaxValue);
                Matrix[i, j] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestX = i;
                    bestY = j;
                }

                if (score == bestScore)
                {
                    bestMoves.Add((i, j));
                }
            }
        }

        //if (bestMoves.Count > 0)
        //{
        //    var (bestX_, bestY_) = bestMoves[UnityEngine.Random.Range(0, bestMoves.Count)];
        //    DoMove(bestX_, bestY_, -1);
        //}
        if (bestX != -1 && bestY != -1)
        {
            DoMove(bestX, bestY, -1);
        }
    }
    public void MoveAI(int[,] current, int[,] next)
    {
        int x = 0;
        int y = 0;
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                if (current[i, j] != next[i, j])
                {
                    x = i;
                    y = j;
                }
            }
        }
        DoMove(x, y, -1);
        state = States.CanMove;
    }
    public void CloneMatrix(int[,] src, int[,] dst)
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                dst[i, j] = src[i, j];
            }
        }
    }
}
