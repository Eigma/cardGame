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
    public TextMeshProUGUI scoreText, comboText;
    public Button resetButton;

    [Header("Layout")]
    public float spacing = 8f;

    [Header("Letters")]
    public List<string> letters;

    [Header("Level Progression")]
    public int startSize = 2;
    public int maxSize = 6;

    [Header("Level Display")]
    public GameObject levelPanel;                
    public TextMeshProUGUI levelPanelText;         
    public float levelDisplayDuration = 1f;        

    public bool IsBusy { get; private set; } = false;
    int score, combo;
    int currentSize;
    Card firstRevealed, secondRevealed;

    int rows { get; set; }
    int cols { get; set; }

    void Start()
    {
        resetButton.gameObject.SetActive(false);
        currentSize = startSize;
        StartCoroutine(BeginLevelRoutine());
    }

    IEnumerator BeginLevelRoutine()
    {
        
        levelPanelText.text = $"Level {currentSize} ({currentSize}×{currentSize})";
        levelPanel.SetActive(true);
        yield return new WaitForSeconds(levelDisplayDuration);
        levelPanel.SetActive(false);

        
        score = combo = 0;
        UpdateUI();

        
        rows = cols = currentSize;
        Setup();
    }

    public void Setup()
    {
        StopAllCoroutines();
        foreach (Transform t in gameArea)
            Destroy(t.gameObject);
        StartCoroutine(DealCards());
    }

    IEnumerator DealCards()
    {
        int totalCards = rows * cols;
        int pairCount = totalCards / 2;
        var selected = letters.GetRange(0, pairCount);

        var pool = new List<string>(selected);
        pool.AddRange(selected);
        for (int i = 0; i < pool.Count; i++)
        {
            int r = Random.Range(i, pool.Count);
            (pool[i], pool[r]) = (pool[r], pool[i]);
        }

        var grid = gameArea.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.spacing = new Vector2(spacing, spacing);

        var rt = gameArea.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        float totalW = rt.rect.width - (cols - 1) * spacing;
        float totalH = rt.rect.height - (rows - 1) * spacing;
        grid.cellSize = new Vector2(totalW / cols, totalH / rows);

        foreach (var letter in pool)
        {
            var go = Instantiate(cardPrefab, gameArea);
            var card = go.GetComponent<Card>();
            card.letter = letter;
            go.GetComponent<Button>().onClick.AddListener(card.OnClick);
        }

        yield return null;
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

        bool allMatched = true;
        foreach (Transform t in gameArea)
        {
            Card card = t.GetComponent<Card>();
            if (card != null && !card.IsMatched)  
            {
                allMatched = false;
                break;
            }
        }

        if (allMatched)
        {
            currentSize++;
            if (currentSize > maxSize)
            {
                Debug.Log("All levels complete! Restarting at " + startSize + "×" + startSize);
                currentSize = startSize;
            }
            StartCoroutine(BeginLevelRoutine());
        }
    }

    void UpdateUI()
    {
        scoreText.text = $"Lvl {currentSize}  Score: {score}";
        comboText.text = $"Combo: {combo}";
    }
}
