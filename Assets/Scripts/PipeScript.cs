using UnityEngine;
using TMPro;

public class PipeScript : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshPro textMesh; // רכיב הטקסט שמציג את שם הקטגוריה על הצינור
    
    [Header("Data")]
    public int myCategoryId; // ID של הקטגוריה 
    public GameManager gameManager; // חיבור לגיים מנג'ר כדי שנוכל לדווח לו על בחירה

    [Header("Visuals")]
    [SerializeField] SpriteRenderer pipeRenderer; 
    [SerializeField] Color highlightColor = Color.cyan; // צבע החיווי כשהשחקן עומד ליד הצינור

    private bool isPlayerInZone = false; // משתנה בוליאני שבודק האם השחקן נמצא ליד הצינור

    void Start()
    {
        // מציאת הרכיב באופן ישיר בתחילת המשחק
        pipeRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // התנאי של בחירת השחקן, גם חייב להיות באזור הצינור וגם צריך ללחוץ על רווח
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.Space))
        {
            SelectPipe();
        }
    }

    private void SelectPipe()
    {
        // קריאה ישירה לפונקציה בגיים מנג'ר (בלי בדיקה אם הוא קיים)
        gameManager.CheckSorting(myCategoryId, this);
        isPlayerInZone = false; // איפוס המצב כדי למנוע לחיצות כפולות
    }

    // פונקציית עזר לשינוי צבע הצינור
    public void SetColor(Color color)
    {
        // שינוי הצבע ישירות
        pipeRenderer.color = color;
    }

    // זיהוי כניסת שחקן לאזור הצינור 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // וידוא שמי שנכנס הוא השחקן
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = true; // השחקן באזור
            SetColor(highlightColor); // סימון ויזואלי לשחקן
        }
    }

    // זיהוי יציאת שחקן מהאזור
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = false; //השחקן יצא מאזור הצינור
            ResetColor(); // החזרת הצבע המקורי
        }
    }

    // פונקציה להחזרת הצבע המקורי
    public void ResetColor()
    {
        pipeRenderer.color = Color.white;
    }
}