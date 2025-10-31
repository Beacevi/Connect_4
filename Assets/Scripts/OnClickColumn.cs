using UnityEngine;

public class OnClickColumn : MonoBehaviour
{
    private NextToken nextToken;
    private Board     board;

    void Start()
    {
        
    }
    public void OnClicked()
    {
        nextToken.ActivateNextToken(false);

        board.PutToken(gameObject.tag);
    }
}
