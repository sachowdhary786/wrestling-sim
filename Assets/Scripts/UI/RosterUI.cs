using System;
using System.Collections.Generic;
using UnityEngine;

public class RosterUI : MonoBehaviour
{
    public Transform rosterContainer;
    public GameObject wrestlerCardPrefab;

    void Start()
    {
        foreach (var wrestler in GameManager.Instance.roster)
        {
            var card = Instantiate(wrestlerCardPrefab, rosterContainer);
            card.GetComponent<WrestlerCard>().Setup(wrestler);
        }
    }
}
