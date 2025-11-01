using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connect4 : MonoBehaviour
{
    [SerializeField] private GameObject      _endGamePanel;
    [SerializeField] private GameObject      _gamePanel;
    [SerializeField] private GameObject      _chooseAiPanel;
    [SerializeField] private TextMeshProUGUI _resultsGame;

    public int aiType;

    private void Start()
    {
        _endGamePanel.SetActive(false);
        _gamePanel.SetActive(false);
        _chooseAiPanel.SetActive(true);
    }

    public void Results (string result)
    {
        _resultsGame.text = "The winner is: " + result;

        _endGamePanel.SetActive(true);
    }
    public void OnClicked(int type)
    {
        aiType = type;

        _gamePanel.SetActive(true);
        _chooseAiPanel.SetActive(false);
    }
}
