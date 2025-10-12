using System;
using UnityEngine;

public static class StatManager
{
    public static void UpdateAfterMatch(Match match, GameData data)
    {
        Wrestler winner = data.wrestlers.Find(w => w.id == match.winnerId);

        // Adjust stats
        foreach (Guid id in match.participants)
        {
            Wrestler w = data.wrestlers.Find(x => x.id == id);

            if (w.id == winner.id)
                w.popularity = Mathf.Min(100, w.popularity + 2);
            else
                w.popularity = Mathf.Max(0, w.popularity - 1);

            w.stamina = Mathf.Max(0, w.stamina - UnityEngine.Random.Range(5, 15));

            if (w.stamina < 10 && UnityEngine.Random.value < 0.2f)
                w.injured = true;
        }
    }
}
