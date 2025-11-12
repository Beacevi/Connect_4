using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connect4 : MonoBehaviour
{
    [SerializeField] private GameObject      _endGamePanel;
    [SerializeField] private GameObject      _gamePanel;
    [SerializeField] private GameObject      _chooseAiPanel;
    [SerializeField] private GameObject      _chooseModePanel;
    [SerializeField] private TextMeshProUGUI _resultsGame;

    public int aiType;
    public bool PlayerExist;
    private void Start()
    {
        _endGamePanel.SetActive(false);
        _gamePanel.SetActive(false);
        _chooseModePanel.SetActive(true);
        _chooseAiPanel.SetActive(false);
    }

    public void Results (string result)
    {
        if (result == "draw")
        {
            _resultsGame.text = "It's a draw!";
        }
        else
        {
            _resultsGame.text = "The winner is: " + result;
        }

        _endGamePanel.SetActive(true);
    }
    public void OnClickedTypeAi(int type)
    {
        aiType = type;
        Board board = FindFirstObjectByType<Board>();
        board.AiType(type);
        _gamePanel.SetActive(true);
        _chooseAiPanel.SetActive(false);
        
    }
    public void OnClickedModeType(bool mode)
    {
        PlayerExist = mode;
        Board board = FindFirstObjectByType<Board>();
        //board.AiType(type);
        _chooseAiPanel.SetActive(true);
        _chooseModePanel.SetActive(false);

    }
}
