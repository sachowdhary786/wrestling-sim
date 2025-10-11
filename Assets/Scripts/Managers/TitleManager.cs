using UnityEngine;

public static class TitleManager
{
    public static void CheckTitleChange(Match match, GameData data)
    {
        if (string.IsNullOrEmpty(match.titleId))
            return;

        Title title = data.titles.Find(t => t.id == match.titleId);
        if (title == null)
            return;

        Wrestler winner = data.wrestlers.Find(w => w.id == match.winnerId);

        if (winner.id != title.currentChampionId)
        {
            title.previousChampions.Add(title.currentChampionId);
            title.currentChampionId = winner.id;
            Debug.Log($"{winner.name} wins the {title.name}!");
        }
    }
}
