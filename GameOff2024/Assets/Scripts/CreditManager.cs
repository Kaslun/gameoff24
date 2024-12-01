using UnityEngine;
using TMPro;
using System;
using System.Collections;
public class CreditManager : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI output;
    [SerializeField]
    private TextManager textManager;
    [SerializeField]
    private string credits;
    [SerializeField]
    private float shutdownTimer;

    private void OnEnable()
    {
        StartCoroutine(textManager.TypeText(output, credits, true));
        StartCoroutine(WaitForShutdown());
    }

    private IEnumerator WaitForShutdown()
    {
        yield return new WaitForSeconds(shutdownTimer);
    }
}
