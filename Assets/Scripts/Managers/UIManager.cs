using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject rosterPanel,
        bookingPanel,
        resultsPanel,
        titlesPanel;

    public void ShowPanel(GameObject panel)
    {
        rosterPanel.SetActive(false);
        bookingPanel.SetActive(false);
        resultsPanel.SetActive(false);
        titlesPanel.SetActive(false);
        panel.SetActive(true);
    }
}
