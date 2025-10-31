using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connect4 : MonoBehaviour
{
    [SerializeField] private GameObject      _endGamePanel;
    [SerializeField] private TextMeshProUGUI _resultsGame;

    private void Start()
    {
        _endGamePanel.SetActive(false);
    }

    public void Results (string result)
    {
        _resultsGame.text = "The winner is: " + result;

        _endGamePanel.SetActive(true);
    }
}
