using UnityEngine;

public class OnClickColumn : MonoBehaviour
{
    [SerializeField] private NextToken nextToken;
    [SerializeField] private Board     board;

    void Start()
    {
        board     = FindFirstObjectByType<Board>();
        nextToken = FindFirstObjectByType<NextToken>();

    }
    public void OnClicked()
    {
        nextToken.ActivateNextToken(false);

        Debug.Log("Hola");
        board.PutPlayerToken(gameObject.tag);
    }
}
