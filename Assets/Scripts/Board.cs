
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

    private GameObject[,] board = new GameObject[rows, cols];

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

    public void PutToken(string Column)
    {
        switch (Column) 
        {
            case "C1":
                break;
            case "C2":
                break;
            case "C3":
                break;
            case "C4":
                break;
            case "C5":
                break;
            case "C6":
                break;
            case "C7":
                break;
            default:
                break;
        }
            
    }
}
