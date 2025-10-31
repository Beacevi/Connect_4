using UnityEngine;

public class OnClickColumn : MonoBehaviour
{
    private Board board;

    void Start()
    {
        board = FindFirstObjectByType<Board>();
    }

    public void OnClicked()
    {
        board.PutToken(gameObject.tag);
    }
}
