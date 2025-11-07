using UnityEngine;

public static class WeeklyManager
{
    public static void AdvanceWeek(GameData data, Show bookedShow)
    {
        ShowSimulator.SimulateShow(bookedShow, data);

        // Heal wrestlers slightly each week
        foreach (var w in data.wrestlers.Values)
        {
            if (!w.injured)
                w.stamina = Mathf.Min(100, w.stamina + 10);
        }

        Debug.Log("Week advanced. Wrestlers have recovered slightly.");
    }
}
