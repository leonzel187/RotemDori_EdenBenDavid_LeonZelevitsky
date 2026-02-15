using UnityEngine;
using UnityEngine.SceneManagement; 

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 8f; // משתנה של מהירות השחקן, שאפשר לשנות ידנית באינספקטור


    [Header("Boundaries")]
    [SerializeField] float minX = -8f;
    [SerializeField] float maxX = 8f;
    [SerializeField] float minY = -4.5f;
    public float maxY = 4.5f; // מוגדר כPublic כדי שסקריפט הפידבק יוכל לשנות את הגבול ולחוסם את השחקן במידת הצורך
    
    [Header("Perspective Settings")]
    [SerializeField] float horizonY = 4.5f; // הגובה שבו השחקן מגיע לגודל המינימלי
    [SerializeField] float startScale = 1.0f; 
    [SerializeField] float endScale = 0.5f;   

    void Update()
    {
        // קבלת קלט מהחצים וחישוב תנועה חלקה עם DeltaTime
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * (moveSpeed * Time.deltaTime);
        transform.position += movement;

        // שימוש ב-Clamp כדי לוודא שהשחקן נשאר בתוך גבולות המסך שהגדרנו
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);

        // עדכון הגודל של השחקן כדי ליצור אשליה של עומק
        AdjustPerspective();
    }

    void AdjustPerspective()
    {
        // חישוב המרחק הכולל בין תחתית המסך לקו האופק
        float totalDistance = horizonY - minY;
        if (totalDistance == 0) return; 

        // חישוב המיקום היחסי של השחקן (בין 0 ל-1)
        float currentY = transform.position.y - minY;
        float percent = currentY / totalDistance; 

        // חישוב הגודל החדש, ככל שעולים למעלה, השחקן נהיה קטן יותר
        float scaleDiff = startScale - endScale;
        float currentScale = startScale - (scaleDiff * percent);
        
        transform.localScale = new Vector3(currentScale, currentScale, 1);
    }

    public void ResetPosition(Vector3 startPos)
    {
        transform.position = startPos;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // זיהוי הגעה לסוף המשוב
        if (collision.CompareTag("Finish")) 
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}