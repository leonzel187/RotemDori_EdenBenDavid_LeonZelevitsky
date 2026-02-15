using UnityEngine;
// סקריפט שאחראי על מוזיקת הרקע, דואג שהיא תמשיך לנגן במעבר בין סצנות ושלא יווצרו כפילויות

public class MusicController : MonoBehaviour
{
    void Awake()
    {
        // בדיקה האם קיים כבר אובייקט סאונד בסצנה בעזרת התגית Music
        GameObject[] musicObjs = GameObject.FindGameObjectsWithTag("Music");

        if (musicObjs.Length > 1) 
        {
            // אם נמצא יותר מאובייקט אחד, זה אומר שכבר קיימת מוזיקה מהסצנה הקודמת
            // לכן האובייקט החדש יושמד, כדי למנוע כפילות של סאונד
            Destroy(this.gameObject); 
        }
        else
        {
            // אם זה האובייקט הראשון, נשמור עליו במעבר בין סצנות כדי שהמוזיקה תמשיך להתנגן ברצף
            DontDestroyOnLoad(this.gameObject);
        }
    }
}