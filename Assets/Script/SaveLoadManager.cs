using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    string savePath;

    void Awake()
    {
        Debug.Log("SaveLoadManager.Awake");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
        savePath = Path.Combine(Application.persistentDataPath, "save.dat");
    }

    [System.Serializable]
    class SaveData { public int score, combo; }

    public void SaveProgress(GameManager gm)
    {
        var data = new SaveData
        {
            score = int.Parse(gm.scoreText.text.Split(':')[1]),
            combo = int.Parse(gm.comboText.text.Split(':')[1])
        };
        using (var fs = File.Create(savePath))
            new BinaryFormatter().Serialize(fs, data);
    }

    public void LoadProgress(GameManager gm)
    {
        if (!File.Exists(savePath)) return;
        using (var fs = File.OpenRead(savePath))
        {
            var data = (SaveData)new BinaryFormatter().Deserialize(fs);
            gm.scoreText.text = "Score: " + data.score;
            gm.comboText.text = "Combo: " + data.combo;
        }
    }
}
