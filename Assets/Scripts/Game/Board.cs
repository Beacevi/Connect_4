using UnityEngine;
using UnityEngine.UI;

public static class Colors
{
    public static readonly Color BLUE = new Color32(0, 28, 135, 255);
    public static readonly Color RED = new Color32(255, 0, 0, 255);
    public static readonly Color YELLOW = new Color32(255, 255, 0, 255);
}

public class Board : MonoBehaviour
{
    private const int rows = 6;
    private const int cols = 7;

    private GameObject[,] board = new GameObject[rows, cols];
    private bool isPlayerTurn = true; // true = rojo, false = amarillo

    [SerializeField] private NextToken nextToken; // asigna en el inspector

    private void Start()
    {
        int r = 0;
        int c = 0;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<Image>())
            {
                child.GetComponent<Image>().color = Colors.BLUE;
                board[r, c] = child.gameObject;

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

    public void PutToken(string columnTag)
    {
        int columnIndex = TagToColumn(columnTag);
        if (columnIndex == -1)
        {
            Debug.LogWarning("Columna no válida: " + columnTag);
            return;
        }

        // Busca la primera fila vacía desde abajo hacia arriba
        for (int r = rows - 1; r >= 0; r--)
        {
            Image cellImage = board[r, columnIndex].GetComponent<Image>();
            if (cellImage.color == Colors.BLUE)
            {
                // Coloca la ficha
                cellImage.color = isPlayerTurn ? Colors.RED : Colors.YELLOW;

                // Cambia el turno
                isPlayerTurn = !isPlayerTurn;
                nextToken.ChangeTurn(isPlayerTurn);
                nextToken.ActivateNextToken(true);

                return;
            }
        }

        // Si la columna está llena
        Debug.Log("Columna llena: " + columnTag);
        nextToken.ActivateNextToken(true);
    }

    private int TagToColumn(string tag)
    {
        switch (tag)
        {
            case "C1": return 0;
            case "C2": return 1;
            case "C3": return 2;
            case "C4": return 3;
            case "C5": return 4;
            case "C6": return 5;
            case "C7": return 6;
            default: return -1;
        }
    }
}
