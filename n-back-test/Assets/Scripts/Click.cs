using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using System.Text;
#if UNITY_ANDROID
using UnityEngine.Networking;
#endif

public class Click : MonoBehaviour
{
    [SerializeField] TMP_Text Tmp;

    bool clicked;
    bool isPaused;
    bool isWaiting;
    bool select;
    bool test;
    bool typeSelected;

    string folder;
    string letters;
    string line;

    char currentLetter;
    char target;

    float reactionTime;
    float totalReactionTime;
    float waitTimer;
    float waitDuration;

    int charCounter;
    int difficulty;
    int lineCount;
    int misses;
    int n;
    int reactions;
    int score;
    int seconds;

    StreamReader sr;
    StringBuilder sb;

    void Start()
    {
        clicked = false;
        isPaused = false;
        isWaiting = false;
        select = false;
        test = false;
        typeSelected = false;

#if UNITY_ANDROID && !UNITY_EDITOR
        folder = Application.streamingAssetsPath + "/N-Back-Files";
#else
        folder = Path.Combine(Application.streamingAssetsPath, "N-Back-Files");
#endif

        line = "";
        letters = "";
        currentLetter = ' ';
        target = ' ';
        reactionTime = 0;
        totalReactionTime = 0;
        waitTimer = 0;
        waitDuration = 5f;
        charCounter = 0;
        difficulty = 0;
        lineCount = 0;
        misses = 0;
        n = 0;
        reactions = 0;
        score = 0;
        seconds = 0;

        sr = null;
        sb = new StringBuilder();
    }

    void Update()
    {
        if (isWaiting)
        {
            if (waitTimer < waitDuration)
            {
                waitTimer += Time.deltaTime;
                reactionTime += Time.deltaTime;

                if (seconds == 1)
                {
                    waitDuration = 4.9f;
                    Tmp.SetText("The test will resume in:\n" + (5 - (int)waitTimer));
                }
                else if (seconds == 2)
                {
                    waitDuration = 4.9f;
                    Tmp.SetText("The test will begin in:\n" + (5 - (int)waitTimer));
                }
            }
            else
            {
                if (seconds != 0) seconds = 0;

                if (waitDuration == .5f)
                {
                    waitTimer = 0;
                    Tmp.SetText("");
                    waitDuration = 1.5f;
                }
                else
                {
                    waitTimer = 0;
                    reactionTime = 0;
                    Tick();
                    waitDuration = .5f;
                }
            }
        }
    }

    public void StartButton()
    {
        if (!isWaiting)
        {
            if (!typeSelected)
            {
                Tmp.SetText("Welcome to the N-Back Test!\n\nChange difficulty with the left button\n\nDifficulty = " + difficulty + "\n\nBegin test with the right button");
                typeSelected = true;
            }
            else if (!clicked)
            {
                n = difficulty;
                string filePath = Path.Combine(folder, "n" + n + ".txt");
#if UNITY_ANDROID && !UNITY_EDITOR
                StartCoroutine(LoadFileAndroid(filePath));
#else
                try
                {
                    sr = new StreamReader(filePath);
                    letters = sr.ReadLine();
                    if (n == 0)
                    {
                        target = sr.ReadLine()[0];
                    }
                    line = sr.ReadLine();
                }
                catch (System.Exception e)
                {
                    Tmp.SetText("Exception: " + e.Message);
                }
                ShowInstructions();
#endif
                File.Delete(Path.Combine(folder, "reactiontimes.csv"));
                clicked = true;
            }
            else if (isPaused)
            {
                isPaused = false;
                waitTimer = 0;
                currentLetter = ' ';
                isWaiting = true;
            }
            else
            {
                seconds = 2;
                waitDuration = 5f;
                isWaiting = true;
            }
        }
        else
        {
            EndButton();
            if (sr != null) sr.Close();
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    IEnumerator LoadFileAndroid(string filePath)
    {
        UnityWebRequest www = UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string[] lines = www.downloadHandler.text.Split('\n');
            letters = lines[0].Replace("\r", "");
            if (n == 0) target = lines[1].Replace("\r", "")[0];
            line = lines[2].Replace("\r", "");
            ShowInstructions();
        }
        else
        {
            Tmp.SetText("Failed to load file: " + filePath);
        }
    }
#endif

    void ShowInstructions()
    {
        if (n == 0)
        {
            Tmp.SetText("Your target letter is: " + target + "\n\nClick the middle button each time you see this letter.\n\nClick the right again button to start.");
        }
        else if (n == 1)
        {
            Tmp.SetText("Your letters are:\n" + letters + "\n\nClick the middle button each time you see two of the same letters in a row.\n\nClick the right button again to start.");
        }
        else
        {
            Tmp.SetText("Your letters are:\n" + letters + "\n\nClick the middle button each time you see the same letter with " + (n - 1) + " other letter(s) between them.\n\nClick the right button again to start.");
        }
    }

    public void EndButton()
    {
        if (isWaiting)
        {
            isWaiting = false;
            clicked = false;

            if (test)
                Tmp.SetText("The test has concluded.\n\nChange difficulty with left button\n\nPlay again with right button");
            else
                Tmp.SetText("Your score was:\n" + score + "\n\nYour number of misses was:\n" + misses + "\n\nChange difficulty with left button\n\nPlay again with right button");

            if (reactions > 0)
                totalReactionTime = totalReactionTime / reactions;

            sb.AppendLine(string.Format("{0}, {1}, {2}", score, misses, totalReactionTime));
            File.WriteAllText(Path.Combine(folder, "reactiontimes.csv"), sb.ToString());

            resetValues();
        }
    }

    public void ScoreButton()
    {
        if (!typeSelected)
        {
            TestSelect();
        }
        else
        {
            if (!select && seconds == 0)
            {
                int correct = 1;
                if (n == 0 && currentLetter == target)
                    score++;
                else if (n > 0 && charCounter > n && currentLetter == line[charCounter - 1 - n])
                    score++;
                else
                {
                    misses++;
                    correct--;
                }

                totalReactionTime += reactionTime;
                reactions++;
                sb.AppendLine(string.Format("{0}, {1}, {2}, {3}", lineCount, charCounter - 1, reactionTime, correct));
            }

            select = true;
        }
    }

    public void TestSelect()
    {
        if (!isWaiting && !clicked && !typeSelected)
        {
            test = !test;
            Tmp.SetText("Select mode with middle button\n\nStart with right button\n\nCurrently selected:\n" + (test ? "Test" : "Game"));
        }
    }

    public void DifficultyButton()
    {
        if (!isWaiting && !clicked && typeSelected)
        {
            difficulty++;
            difficulty %= 4;
            Tmp.SetText("Welcome to the N-Back Test!\n\nChange difficulty with the left button\n\nDifficulty = " + difficulty + "\n\nBegin test with the right button");
        }
    }

    private void Tick()
    {
        if (sr != null)
        {
            select = false;

            if (charCounter >= line.Length)
            {
                line = sr.ReadLine();
                charCounter = 0;
                lineCount++;

                if (lineCount == 4)
                    pauseGame();
                else
                {
                    waitTimer = 0;
                    currentLetter = ' ';
                    seconds = 1;
                }
            }

            if (seconds == 0 && !string.IsNullOrEmpty(line) && charCounter < line.Length)
            {
                currentLetter = line[charCounter];
                charCounter++;
                Tmp.SetText(currentLetter + "");
            }
        }
    }

    private void resetValues()
    {
        clicked = false;
        isWaiting = false;
        select = false;
        line = "";
        letters = "";
        currentLetter = ' ';
        target = ' ';
        waitTimer = 0;
        waitDuration = 5f;
        charCounter = 0;
        lineCount = 0;
        misses = 0;
        score = 0;
        seconds = 0;
        sb.Clear();
    }

    private void pauseGame()
    {
        isWaiting = false;
        seconds = 1;
        isPaused = true;
        Tmp.SetText("The test is halfway through\n\nPress the button to your right when you are ready to resume");
    }
}

