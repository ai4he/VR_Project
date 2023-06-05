using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    public ScreenManager screenManager;
    public InputField monitorIdInputField;

    public void StartScreen()
    {
        if (monitorIdInputField.text != "")
        {
            int monitorId = int.Parse(monitorIdInputField.text);
            if (!screenManager.IsScreenActive(monitorId))
            {
                screenManager.StartScreen(monitorId);
            }
        }
    }

    public void StopScreen()
    {
        if (monitorIdInputField.text != "")
        {
            int monitorId = int.Parse(monitorIdInputField.text);
            if (screenManager.IsScreenActive(monitorId))
            {
                screenManager.StopScreen(monitorId);
            }
        }
    }
}
