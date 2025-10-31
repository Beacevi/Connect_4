
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public static class Colors
{
    public static readonly Color BLUE   = new Color32(  0,  28, 135, 255);
    public static readonly Color RED    = new Color32(255,   0,   0, 255);
    public static readonly Color YELLOW = new Color32(255, 255,   0, 255);
}

public class Board : MonoBehaviour
{
    private const int rows = 6;
    private const int cols = 7;
    private const int numberToken = rows * cols;

    private GameObject[,] board       = new GameObject[rows, cols];
    private GameObject[,] playerToken = new GameObject[rows, cols];
    private GameObject[,] IAToken = new GameObject[rows, cols];

    private void Start()
    {
        int r = 0;
        int c = 0;

        foreach (Transform child in transform)
        {
            if(child.GetComponent<Image>())
            {
                child.GetComponent<Image>().color = Colors.BLUE;

                board[r, c] = child.gameObject;

                //Debug.Log(child.gameObject.name + " " + r + ", " + c);

                c++;

                if (c >= cols)
                {
                    c = 0;
                    r++;
                }

                if (r >= rows)
                    break;

            }
        }
    }

    private int GetRow(int column)
    {
        int row = 6;

        foreach (GameObject token in board)
        {
            if (board[row, column].GetComponent<Image>().color == Colors.BLUE)
            {
                return row;
            }

            row--;

            if (row <= 0)
            {
                Debug.Log("Column full");
            }

        }

        return -1;
    }

    private void PaintPlayerToken(int row, int column)
    {
        board[row, column].GetComponent<Image>().color = Colors.RED;
    }

    public void PutPlayerToken(string columnTag)
    {
        int row  = -1;
        int colm = -1;

        switch (columnTag) 
        {
            case "C1":
                colm = 0;
                break;
            case "C2":
                colm = 1;
                break;
            case "C3":
                colm = 2;
                break;
            case "C4":
                colm = 3;
                break;
            case "C5":
                colm = 4;
                break;
            case "C6":
                colm = 5;
                break;
            case "C7":
                colm = 6;
                break;
            default:
                break;
        }

        if (row == -1 || colm == -1)
            return;

        row = GetRow(colm);
        PaintPlayerToken(row, colm);
        playerToken[row, colm] = board[row, colm];
    }
}
