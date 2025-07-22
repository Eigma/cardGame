using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Card : MonoBehaviour
{
    [HideInInspector] public string letter;
    public string backCharacter = "?";
    public TextMeshProUGUI textComponent;       
    public float flipDuration = 0.25f;

    bool isFlipped = false;
    bool isMatched = false;
    public bool IsMatched => isMatched;
    GameManager gm;

    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        textComponent.text = backCharacter;
    }

    
    public void OnClick()
    {
        if (isFlipped || isMatched || gm.IsBusy) return;
        StartCoroutine(Flip(letter, true));
        gm.CardRevealed(this);
    }

    IEnumerator Flip(string toChar, bool revealing)
    {
        float half = flipDuration / 2f;
      
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(1, 0, t / half);
            transform.localScale = new Vector3(s, 1, 1);
            yield return null;
        }
        
        textComponent.text = toChar;
        
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            float s = Mathf.Lerp(0, 1, t / half);
            transform.localScale = new Vector3(s, 1, 1);
            yield return null;
        }
        transform.localScale = Vector3.one;
        isFlipped = revealing;
    }

    public void HideMatch()
    {
        isMatched = true;
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        Color c = textComponent.color;
        for (float a = 1f; a > 0f; a -= Time.deltaTime)
        {
            c.a = a;
            textComponent.color = c;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public IEnumerator FlipBack()
    {
        yield return Flip(backCharacter, false);
    }
}
