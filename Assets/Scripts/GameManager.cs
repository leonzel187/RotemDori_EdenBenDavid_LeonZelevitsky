using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{    
    // חיבורים לאובייקטים בסצנה
    [Header("Player & UI Refs")]
    [SerializeField] PlayerMove playerScript; // חיבור לסקריפט של השחקן
    [SerializeField] Transform playerStartPosition; // נקודת ההתחלה של השחקן
    [SerializeField] GameObject endScreenPanel; // הפאנל שמופיע בסוף המשחק
    
    [Header("Audio System")]
    [SerializeField] AudioSource ambienceSource; // מקור הסאונד שמנגן את מוזיקת הרקע
    [SerializeField] AudioSource sfxSource;   // מקור סאונד נפרד לאפקטים קצרים
    
    // קבצי השמע עצמם
    [SerializeField] AudioClip backgroundNoiseClip;  // מוזיקת הרקע
    [SerializeField] AudioClip correctSound; // סאונד משוב הצלחה        
    [SerializeField] AudioClip wrongSound;   // סאונד משוב אי הצלחה
    
    // רכיבי UI
    [Header("Game UI")]
    [SerializeField] GameObject timeUpPanel; // הודעה כשנגמר הזמן
    [SerializeField] TextMeshPro playerText;      // הטקסט שמופיע בהולגרמה של השחקן
    [SerializeField] SpriteRenderer playerImage;  // התמונה שמופיעה בהולגרמה של השחקן
    [SerializeField] TextMeshPro timerText; // הטיימר 
    [SerializeField] TextMeshPro progressText; // טקסט מד התקדמות 

    [Header("Game Settings")]
    [SerializeField] float timePerQuestion = 10f; // זמן לכל שאלה
    [SerializeField] float feedbackDelay = 1.0f; // זמן המתנה לפני מעבר למשוב
    [SerializeField] float nextQuestionDelay = 2.0f; // כמה זמן להציג את הודעת נגמר הזמן, לפני שעוברים הלאה

    [Header("Pipes Settings")]
    [SerializeField] PipeScript pipePrefab; // הפריפאב של הצינור
    [SerializeField] float pipeDistance = 3f; // רווח בין צינור לצינור
    [SerializeField] float pipesYPosition = 4f; // גובה הצינורות
    [SerializeField] float globalXOffset = -2f;

    [Header("Pause & Menus")] 
    [SerializeField] GameObject pausePanel; // פאנל השהייה
    [SerializeField] GameObject muteButton; // כפתור סאונד
    [SerializeField] GameObject pauseButton; // כפתור השהייה 
    
    // טקסטים למסך הסיום
    [Header("End Screen")]
    [SerializeField] TextMeshProUGUI endMistakesText; 
    [SerializeField] TextMeshProUGUI endTimeText;       
    
    [Header("Data Source")]
    [SerializeField] List<CategoryData> categories;    // רשימת הקטגוריות והפריטים באינספקטור
    
    private List<ItemData> activeGameItems = new List<ItemData>(); // רשימת השאלות הפעילה
    private List<PipeScript> activePipes = new List<PipeScript>(); // רשימת הצינורות שנוצרו
    
    private int currentItemIndex = 0; // באיזו שאלה אנחנו כרגע
    private ItemData currentItem; // המאפיינים של השאלה הנוכחית
    
    private float currentTimer; // הזמן שנשאר לשאלה הנוכחית
    private bool isTimerRunning = false; // האם הטיימר רץ כרגע?
    private float totalElapsedTime = 0f; // זמן משחק כולל
    public bool canAnswer = true; // מונע לחיצות כפולות על תשובה

    void Start()
    {
        // השתקת והפעלת הסאונד
        if (GameSessionData.isMuted)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        } 
        
        // לוודא שהפאנלים הלא רלוונטים לכרגע כבויים
        endScreenPanel.SetActive(false);
        pausePanel.SetActive(false);
        timeUpPanel.SetActive(false);

        // קריאה לפונקציות של הפעלת מוזיקה והכנת המשחק
        PlayBackgroundAudio();
        PrepareGameItems(); 
        SpawnCategories();  
        
        // שחזור המצב מהזיכרון הסטטי לאחר חזרה ממשוב
        currentItemIndex = GameSessionData.nextQuestionIndex;
        totalElapsedTime = GameSessionData.totalGameTime; 

        // בדיקה האם סיימנו את כל השאלות
        if (currentItemIndex >= activeGameItems.Count)
        {
            GameOver();
        }
        else
        {
            LoadNextItem(); // טעינת הפריט הבא    
        }
    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime; // הורדת זמן
            totalElapsedTime += Time.deltaTime; // צבירת זמן כללי
            
            UpdateTimerUI(); // עדכון הטיימר על המסך

            if (currentTimer <= 0)  // אם נגמר הזמן קוראים לפונקציה שמטפלת בזה
            {
                StartCoroutine(HandleTimeUpSequence()); 
            }
        }
    }

    void PlayBackgroundAudio()
    {
        // הפעלת מוזיקת רקע
        ambienceSource.clip = backgroundNoiseClip;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    void PrepareGameItems()  
    {
        // אם חזרנו מסצנה אחרת והרשימה כבר קיימת, נמשיך איתה
        if (GameSessionData.currentSessionItems != null && GameSessionData.currentSessionItems.Count > 0)
        {
            activeGameItems = new List<ItemData>(GameSessionData.currentSessionItems);
        }
        else
        {
            // משחק חדש, לוקחים את כל הפריטים מכל הקטגוריות לרשימה אחת
            activeGameItems.Clear();
            foreach (var cat in categories)
            {
                foreach (var item in cat.ItemList)
                {
                    item.categoryId = cat.categoryId; 
                    activeGameItems.Add(item);
                }
            }

            ShuffleList(activeGameItems); // מערבבים את הפריטים
            
            // שומרים לזיכרון הסטטי ומאפסים 
            GameSessionData.currentSessionItems = new List<ItemData>(activeGameItems);
            GameSessionData.originalTotalQuestions = activeGameItems.Count;
            GameSessionData.mistakesCount = 0; 
            GameSessionData.questionNumber = 0;
            GameSessionData.totalGameTime = 0; 
        }
    }

    void SpawnCategories()
    {
        // לולאה למחיקת צינורות קודמים למניעת כפילויות
        foreach (var p in activePipes)
        {
            if (p != null) 
            {
                Destroy(p.gameObject);
            }
        }
        activePipes.Clear();

        // חישוב המיקום כדי שהצינורות יהיו במרכז
        float containerSize = (categories.Count - 1) * pipeDistance;
        float offset = containerSize / 2;

        for (int i = 0; i < categories.Count; i++)
        {
            float x = (i * pipeDistance) - offset + globalXOffset;
            Vector3 position = new Vector3(x, pipesYPosition, 0); 

            // יצירת הצינור
            PipeScript newPipe = Instantiate(pipePrefab, position, Quaternion.identity);
            
            // תיקון עברית בטקסט
            RTLfixer.FixRtl(newPipe.textMesh, categories[i].categoryName);
            newPipe.myCategoryId = categories[i].categoryId; // הגדרת ID של הצינור החדש בהתאם לקטגוריה הנוכחית בלולאה
            newPipe.gameManager = this; // חיבור הצינור לסקריפט, כדי שיוכל לדווח כשהשחקן מתנגש בו
            activePipes.Add(newPipe);
        }
    }

    void LoadNextItem() 
    {
        // הכנה לפריט חדש
        canAnswer = true;
        currentTimer = timePerQuestion;
        isTimerRunning = true;
        
        UpdateProgressText(); // קורא לפונקציה של המד התקדמות 
        
        // מחזירים את השחקן לנקודת ההתחלה
        playerScript.ResetPosition(playerStartPosition.position);

        // איפוס הצינור שיחזור לצבע המקורי שלו
        foreach (var pipe in activePipes)
        {
            if (pipe != null)
            {
                pipe.ResetColor();
            }
        }

        // טעינת התוכן (תמונה או טקסט)
        if (currentItemIndex < activeGameItems.Count)
        {
            currentItem = activeGameItems[currentItemIndex];

            if (currentItem.itemImage != null)
            {
                // הצגת תמונה
                playerText.gameObject.SetActive(false);
                playerImage.gameObject.SetActive(true);
                playerImage.sprite = currentItem.itemImage;
            }
            else
            {
                // הצגת טקסט
                playerImage.gameObject.SetActive(false);
                playerText.gameObject.SetActive(true);
                RTLfixer.FixRtl(playerText, currentItem.itemContent);
            }
        }
        else
        {
            GameOver(); 
        }
    }

    // הפונקציה שבודקת האם התשובה נכונה
    public void CheckSorting(int selectedCategoryId, PipeScript selectedPipe)
    {
        if (!canAnswer)
        {
            return;
        }
        canAnswer = false;
        isTimerRunning = false; 

        // האם הקטגוריה של הצינור זהה לקטגוריה של הפריט לפי הID
        bool isMatch = (currentItem.categoryId == selectedCategoryId);
    
        // שמירת הנתונים הרלוונטים למעבר סצנה
        GameSessionData.lastAnswerWasCorrect = isMatch;
        GameSessionData.itemImage = currentItem.itemImage;
        GameSessionData.itemTextContent = currentItem.itemContent;
        GameSessionData.nextQuestionIndex = currentItemIndex + 1; 
        GameSessionData.totalGameTime = totalElapsedTime;// שמירת הזמן שנצבר עד כה, כדי שיישמר גם במעבר לסצנה הבאה 

        if (isMatch)// במיון נכון מעלים את התשובה הנכונה ומפעילים משוב הצלחה 
        {
            GameSessionData.questionNumber++; 
            StartCoroutine(HandleFeedbackSequence(correctSound));
        }
        else
        {
            GameSessionData.mistakesCount++; 
            activeGameItems.Add(currentItem);// באי הצלחה,מחזירים את השאלה לסוף הרשימה
            GameSessionData.currentSessionItems = new List<ItemData>(activeGameItems);
        
            // הפעלת משוב אי הצלחה
            StartCoroutine(HandleFeedbackSequence(wrongSound));
        }
    }

    // טיפול במשוב ומעבר סצנה
    IEnumerator HandleFeedbackSequence(AudioClip clip)
    {
         sfxSource.PlayOneShot(clip); // הפעלת הסאונד
         yield return new WaitForSeconds(feedbackDelay); // עוצרים את הרצת הקוד לזמן קצר, כדי שהשחקן יוכל לשמוע את הסאונד ולהבין את המשוב לפני שעוברים סצנה
         SceneManager.LoadScene("FeedbackScene"); // פקודה שעוברת לסצנת המשוב
    }
    
    // טיפול כשנגמר הזמן
    IEnumerator HandleTimeUpSequence()
    {
        isTimerRunning = false;
        canAnswer = false;
        
        // נחשב כטעות
        GameSessionData.mistakesCount++; 
        activeGameItems.Add(currentItem);
        GameSessionData.currentSessionItems = new List<ItemData>(activeGameItems);
        GameSessionData.nextQuestionIndex = currentItemIndex + 1;
        GameSessionData.totalGameTime = totalElapsedTime;

        // הצגת הודעת נגמר הזמן
        timeUpPanel.SetActive(true);
        
        yield return new WaitForSeconds(nextQuestionDelay); 

        // מעבר לפריט הבא
        timeUpPanel.SetActive(false);
        currentItemIndex++; 
        LoadNextItem(); 
    }
    
    void UpdateTimerUI()
    {
        timerText.text = Mathf.Ceil(currentTimer).ToString();// עדכון הטיימר על המסך
        if (currentTimer <= 3)// בדיקה, האם נשאר פחות מ-3 שניות, צובעים באדום 

        {
            timerText.color = Color.red; 
        }
        else
        {
            timerText.color = Color.white; 
        }
    }
    
    void UpdateProgressText() 
    {
        // עדכון טקסט מד ההתקדמות
        int total = GameSessionData.originalTotalQuestions;
        int current = GameSessionData.questionNumber; 
        progressText.text = current + " / " + total;
    }
    
    void GameOver()
    {
        isTimerRunning = false; 
        GameSessionData.totalGameTime = totalElapsedTime;
        
        // כיבוי כפתורי השהייה וסאונד והצגת מסך סיום
        muteButton.SetActive(false);
        pauseButton.SetActive(false);
        endScreenPanel.SetActive(true);

        CalculateTotalTime(); //קריאה לפונקציה שמחשבת זמן
        
        // איפוס נתונים למשחק הבא
        GameSessionData.nextQuestionIndex = 0;
        GameSessionData.currentSessionItems = null; 
        GameSessionData.originalTotalQuestions = 0; 
    }

    void CalculateTotalTime()
    {
        // חישוב דקות ושניות למסך הסיום
        float totalTime = GameSessionData.totalGameTime;
        int minutes = (int)(totalTime / 60);
        int seconds = (int)(totalTime % 60);
        string timeString = minutes.ToString("00") + ":" + seconds.ToString("00");

        endTimeText.text = timeString;
        endMistakesText.text = GameSessionData.mistakesCount.ToString();
    }
    
    public void RestartGame()
    {
         // איפוס מלא והתחלה מחדש
        GameSessionData.nextQuestionIndex = 0;
        GameSessionData.mistakesCount = 0;
        GameSessionData.questionNumber = 0; 
        GameSessionData.currentSessionItems = null;
        GameSessionData.totalGameTime = 0; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //ערבוב רשימה 
    void ShuffleList(List<ItemData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);
            ItemData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public void MuteMusic() 
    {
        // כפתור השתקה
        GameSessionData.isMuted = !GameSessionData.isMuted;

        if (GameSessionData.isMuted)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = 1;
        }
    }

    public void PauseGame()
    {
        // כפתור השהייה
        isTimerRunning = false;
        canAnswer = false;
        pausePanel.SetActive(true);
        muteButton.SetActive(false);
    }

    public void ResumeAndSkipQuestion()
    {
        pausePanel.SetActive(false);
        muteButton.SetActive(true);
        
         // מעבירים את הפריט לסוף הרשימה כדי שהפריט יתחלף
        activeGameItems.RemoveAt(currentItemIndex);
        activeGameItems.Add(currentItem);
        GameSessionData.currentSessionItems = activeGameItems;
        LoadNextItem(); 
    }
}


