using KH;
using UnityEngine;
using VInspector;

public class TEST : MonoBehaviour
{
    [KHResetStatic, ShowInInspector]
    public static int testIns { get; private set; } = 0;

    [KHResetStatic, ShowInInspector]
    public static bool testBool { get; } = true;

    [KHResetStatic, ShowInInspector]
    public static Vector2 testVe2 { get; private set; } = new(3, 2);

    [KHResetStatic, ShowInInspector]
    public static TEST testGameObject { get; private set; } = null;

    [Button(color = "green")]
    public void ChangeValues()
    {
        testIns++;
        // testBool = !testBool;
        testVe2.Set((testVe2.x + 1) * 2, (testVe2.y + 1) * 2);
        testGameObject = this;
    }
}