using TMPro;
using UnityEngine;

public class Connect4 : MonoBehaviour
{
    [SerializeField] private GameObject _endGamePanel;
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private GameObject _chooseAiPanel;
    [SerializeField] private GameObject _chooseModePanel;
    [SerializeField] private TextMeshProUGUI _resultsGame;

    private int selectedAi1 = -1;
    private int selectedAi2 = -1;
    private bool awaitingSecondAi = false;

    public bool PlayerExist;

    private void Start()
    {
        _endGamePanel.SetActive(false);
        _gamePanel.SetActive(false);
        _chooseModePanel.SetActive(true);
        _chooseAiPanel.SetActive(false);
    }

    public void Results(string result)
    {
        _resultsGame.text = result == "draw" ? "It's a draw!" : "The winner is: " + result;
        _endGamePanel.SetActive(true);
    }

    public void OnClickedModeType(bool playerMode)
    {
        PlayerExist = playerMode;
        _chooseModePanel.SetActive(false);
        _chooseAiPanel.SetActive(true);
    }

    public void OnClickedTypeAi(int type)
    {
        Board board = FindFirstObjectByType<Board>();

        if (PlayerExist)
        {
            // Modo Player vs AI
            board.SetAiTypes(type, -1);
            _chooseAiPanel.SetActive(false);
            _gamePanel.SetActive(true);
        }
        else
        {
            // Modo AI vs AI
            if (!awaitingSecondAi)
            {
                selectedAi1 = type;
                awaitingSecondAi = true;
                Debug.Log("Selecciona la segunda IA");
            }
            else
            {
                selectedAi2 = type;
                board.SetAiTypes(selectedAi1, selectedAi2);
                _chooseAiPanel.SetActive(false);
                _gamePanel.SetActive(true);
            }
        }
    }
}
