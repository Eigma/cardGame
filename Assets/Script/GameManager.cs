using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public GameObject cardPrefab;
    public Transform gameArea;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public Button resetButton;

    [Header("Layout")]
    public int rows = 2;
    public int cols = 3;
    public float spacing = 8f;

    [Header("Letters")]
    public List<string> letters; 

    [HideInInspector] public bool IsBusy = false;

    Card firstRevealed, secondRevealed;
    int score = 0, combo = 0;

    void Start()
    {
        resetButton.onClick.AddListener(Setup);
        Setup();
    }

    public void Setup()
    {
        StopAllCoroutines();
        // clear old cards
        foreach (Transform t in gameArea)
            Destroy(t.gameObject);

        score = combo = 0;
        UpdateUI();
        StartCoroutine(DealCards());
    }

    IEnumerator DealCards()
    {
        int total = rows * cols;
        
        List<string> pool = new List<string>();
        foreach (var L in letters) pool.Add(L);
        pool.AddRange(letters);  

        
        for (int i = 0; i < pool.Count; i++)
        {
            int r = Random.Range(i, pool.Count);
            var tmp = pool[i];
            pool[i] = pool[r];
            pool[r] = tmp;
        }

        
        var grid = gameArea.GetComponent<GridLayoutGroup>();
        grid.constraintCount = cols;
        grid.spacing = new Vector2(spacing, spacing);

        
        for (int i = 0; i < pool.Count; i++)
        {
            var go = Instantiate(cardPrefab, gameArea);
            var card = go.GetComponent<Card>();
            card.letter = pool[i];
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(card.OnClick);
            yield return new WaitForSeconds(0.02f);
        }

        
        SaveLoadManager.Instance.LoadProgress(this);
    }

    public void CardRevealed(Card card)
    {
        if (firstRevealed == null)
            firstRevealed = card;
        else
        {
            secondRevealed = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        IsBusy = true;
        yield return new WaitForSeconds(0.5f);

        if (firstRevealed.letter == secondRevealed.letter)
        {
            firstRevealed.HideMatch();
            secondRevealed.HideMatch();
            combo++;
            score += 10 * combo;
        }
        else
        {
            combo = 0;
            yield return firstRevealed.FlipBack();
            yield return secondRevealed.FlipBack();
        }

        firstRevealed = secondRevealed = null;
        IsBusy = false;
        UpdateUI();

        SaveLoadManager.Instance.SaveProgress(this);

        if (gameArea.childCount == 0)
            Debug.Log("Game Over! Final Score: " + score);
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        comboText.text = "Combo: " + combo;
    }
}
