using System;
using System.Collections.Generic;
using UnityEngine;

public static class TitleManager
{
    public static void CheckTitleChange(Match match, GameData data)
    {
        if (match.titleId == Guid.Empty)
            return;

        Title title = data.titles.Find(t => t.id == match.titleId);
        if (title == null)
            return;

        Wrestler winner = data.wrestlers.Find(w => w.id == match.winnerId);

        if (title.currentChampionId.HasValue && winner.id != title.currentChampionId.Value)
        {
            title.previousChampions.Add(title.currentChampionId.Value);
            title.currentChampionId = winner.id;
            Debug.Log($"{winner.name} wins the {title.name}!");
        }
        else if (!title.currentChampionId.HasValue)
        {
            // The title was vacant
            title.currentChampionId = winner.id;
            Debug.Log($"{winner.name} has won the vacant {title.name}!");
        }
    }
}
