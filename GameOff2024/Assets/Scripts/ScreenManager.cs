using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField]
    public int currentScreen = 0;

    [SerializeField]
    private GameObject[] screens;

    private void Start()
    {
        SwitchScreens(0);
    }

    public void SwitchScreens(int newScreen)
    {

        foreach (GameObject s in screens)
        {
            s.SetActive(false);
        }

        screens[newScreen].SetActive(true);
        currentScreen = newScreen;
    }
}
