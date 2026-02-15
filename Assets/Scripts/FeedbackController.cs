using UnityEngine;
using TMPro;
using System.Collections; 

//סקריפט שמנהל את סצנת המשוב, מציג את הפריט שנבחר ובודק אם התשובה הייתה נכונה או לא
public class FeedbackController : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] PlayerMove playerScript; // חיבור לשחקן  
    [SerializeField] TextMeshPro playerHologramText; // הטקסט בהוגלרמה מעל השחקן
    [SerializeField] SpriteRenderer playerHologramImage; // התמונה בהולגרמה (אם הפריט הוא תמונה)

    [Header("Environment")]
    [SerializeField] GameObject redLasers;    //לייזרים אי הצלחה- אדומים
    [SerializeField] GameObject greenLasers;  // לייזרים הצלחה- ירוקים
    [SerializeField] GameObject forwardBlocker; 
    [SerializeField] GameObject sidePipeObject; 

    [Header("Timing")]
    [SerializeField] float successDuration = 2.0f;// משתנה שאפשר לשנות ידנית באינספקטור ומשפיע על הזמן עד שהלייזרים הירוקים נפתחים 
    private Sprite defaultBoxSprite; // משתנה לשמירת הרקע המקורי של ההולגרמה

    void Start()
    {
        // בדיקה, האם המשתנה של התמונה לא ריק, אז נשמור את הספרייט המקורי
        if (playerHologramImage != null)
        {
            defaultBoxSprite = playerHologramImage.sprite; 
        }

        UpdatePlayerHologram(); // קריאה לפוקנציה,הצגת הפריט (תמונה או טקסט) בתוך ההוגלרמה

        // בדיקה בזיכרון הסטטי, האם השחקן צדק בשאלה הקודמת?
        if (GameSessionData.lastAnswerWasCorrect)
        {
            if (playerScript != null)
            {
                playerScript.enabled = false;// מונע מהשחקן לזוז ומפעיל את אנימציית המעבר
            }
            StartCoroutine(SuccessSequence());
        }
        else
        {
            // אם טעה, קוראים לפוקנציה שחוסמת לו את הדרך
            SetupFailure();
        }
    }

    void UpdatePlayerHologram()
    {
        // איפוס ההוגלרמה למצב ברירת מחדל
        if (playerHologramImage != null && defaultBoxSprite != null)
        {
            playerHologramImage.sprite = defaultBoxSprite; 
            playerHologramImage.gameObject.SetActive(true);
        }

        // בדיקה מה להציג בהוגלרמה
        // אם הפריט הוא תמונה
        if (GameSessionData.itemImage != null)
        {
            if (playerHologramText != null) 
            {
                playerHologramText.gameObject.SetActive(false); // מכבים את הטקסט
            }
            
            if (playerHologramImage != null) 
            {
                playerHologramImage.sprite = GameSessionData.itemImage;  // שמים את התמונה של הפריט בתוך ההוגלרמה
            }
        }
        // אם, הפריט הוא טקסט
        else if (!string.IsNullOrEmpty(GameSessionData.itemTextContent))
        {
            if (playerHologramText != null)
            {
                // משתמשים בRTLFIXER כדי שהטקסט לא יהיה הפוך
                RTLfixer.FixRtl(playerHologramText, GameSessionData.itemTextContent);                
                playerHologramText.gameObject.SetActive(true);
            }
        }
    }

     //משוב הצלחה
    IEnumerator SuccessSequence()
    {
        // מעלימים את כל המכשולים והחסימות האדומות
        if (redLasers != null)
        {
            redLasers.SetActive(false);
        }

        if (sidePipeObject != null)
        {
            sidePipeObject.SetActive(false);
        }

        if (forwardBlocker != null)
        {
            forwardBlocker.SetActive(false);
        } 
        
        if (greenLasers != null)        // מדליקים לייזרים ירוקים
        {
            greenLasers.SetActive(true);
        }
        
        yield return new WaitForSeconds(successDuration);         // מחכים כמה שניות 

        if (greenLasers != null)
        { 
            greenLasers.SetActive(false);        // כיבוי לייזרים ירוקים
        } 
        
        // משחררים את השחקן כדי שהשחקן ימשיך לזוז
        if (playerScript != null) 
        {
            // משנים את הגבול העליון למספר גבוה, כדי שהוא יוכל לעבור לשלב הבא
            playerScript.maxY = 20f; 
            playerScript.enabled = true; // מאפשרים לשחקן לנוע
        }
    }

    //משוב אי הצלחה
    void SetupFailure()
    {
        if (playerScript != null) 
        {
            playerScript.enabled = true;
            // מגבילים את התנועה למעלה, כך שהוא לא יוכל להתקדם וחייב לחזור אחורה
            playerScript.maxY = 0f; 
        }

        // מדליקים את כל המכשולים והלייזרים האדומים
        if (redLasers != null)
        {
            redLasers.SetActive(true);
        }

        if (greenLasers != null)
        {
            greenLasers.SetActive(false);
        }

        if (forwardBlocker != null)
        {
            forwardBlocker.SetActive(true);
        }

        if (sidePipeObject != null)
        {
            sidePipeObject.SetActive(true);
        } 
    }
}