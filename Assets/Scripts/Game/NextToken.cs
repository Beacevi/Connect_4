using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class NextToken : MonoBehaviour
{
    private RectTransform nextTokenRect;
    private RectTransform containerRect;
    private Canvas        canvas;

    private GameObject    nextToken ;
    void Start()
    {
        containerRect = GetComponent<RectTransform>();
        nextTokenRect = GetComponentInChildren<Image>().GetComponent<RectTransform>();
        canvas        = transform.parent.parent.GetComponent<Canvas>();

        nextToken = nextTokenRect.gameObject;
        nextToken.GetComponent<Image>().color = Colors.RED;
    }
    void Update()
    {
        MoveToken();
    }

    private void MoveToken()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
             containerRect,
             mousePos,
             canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
             out Vector2 localPoint
         );

        float halfContainerWidth = containerRect.rect.width / 2f;
        float halfImageWidth = nextTokenRect.rect.width / 2f;

        float clampedX = Mathf.Clamp(localPoint.x, -halfContainerWidth + halfImageWidth, halfContainerWidth - halfImageWidth);

        nextTokenRect.anchoredPosition = new Vector2(clampedX, nextTokenRect.anchoredPosition.y);
    }

    public void ActivateNextToken(bool isActivated)
    {
        nextToken.SetActive(isActivated);
    }

    public void ChangeTurn(bool isPlayerTurn)
    {
        StartCoroutine(ChangingTurn());

        nextToken.GetComponent<Image>().color = isPlayerTurn ? Colors.RED : Colors.YELLOW;
    }

    IEnumerator ChangingTurn()
    {
        nextToken.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        nextToken.SetActive(true);
    }
}

