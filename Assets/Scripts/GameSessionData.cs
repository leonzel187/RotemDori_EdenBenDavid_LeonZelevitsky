using UnityEngine;
using System.Collections.Generic; 

// מחלקה סטטית שומרת נתונים גם כשעוברים סצנות (בניגוד למשתנים רגילים שנמחקים)
public static class GameSessionData
{
    
    // משתנה שזוכר באיזה אינדקס של שאלה עצרנו, כדי שלא נתחיל מהתחלה כשחוזרים מהמשוב
    public static int nextQuestionIndex = 0; 
    
    public static List<ItemData> currentSessionItems;// משתנה ששומר את רשימת השאלות אחרי שערבבנו אותה, כדי שהסדר יישמר
    
    public static bool lastAnswerWasCorrect;// האם השחקן צדק?
    
    public static Sprite itemImage; // התמונה שנוצג בתוך ההוגלרמה של השחקן במשוב
    
    public static string itemTextContent;   // הטקסט בתוך ההוגלרמה
    
    public static int originalTotalQuestions = 0;  // כמה שאלות יש סך הכל  
    
    public static int mistakesCount = 0;  // ספירת הטעויות 
    
    public static int questionNumber = 0;  // ספירת התשובות הנכונות 
    
    public static float totalGameTime = 0f; // הזמן הכולל שלקח לשחקן לענות על הכל
    
    public static bool isMuted = false; // האם השחקן לחץ על כפתור ההשתקה? (נשמר כדי שלא יצטרך להשתיק כל סצנה מחדש)
}