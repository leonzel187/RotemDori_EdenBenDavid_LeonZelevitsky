using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // פקודה שמאפשרת לנו לראות ולערוך את המחלקה הזו בחלון הInspector ביוניטי
public class ItemData
{
    public string itemContent;   //התוכן הטקסטואלי של הפריט
    public Sprite itemImage;     // התמונה של הפריט, אם קיימת
    public int categoryId;     // המזהה של הקטגוריה אליה הפריט שייך

}
[System.Serializable] 
public class CategoryData
{
    public string categoryName; // השם שיופיע על הצינור(הקטגוריה) 
    public int categoryId;      // הID של הקטגוריה
    public List<ItemData> ItemList; // רשימת הפריטים ששייכים לקטגוריה הזו
}