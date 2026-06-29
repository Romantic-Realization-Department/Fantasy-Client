using UnityEngine;

[CreateAssetMenu(fileName = "UIKeyRegistry", menuName = "ScriptableObjects/UIKeyRegistry")]
public class UIKeyRegistry : ScriptableObject
{
    public static implicit operator string(UIKeyRegistry mySelf) =>
        mySelf ? mySelf.name : string.Empty;
}
