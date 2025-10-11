using UnityEngine;

public static class RandomEventManager
{
    public static void CheckForEvents(GameData data)
    {
        int roll = Random.Range(0, 100);
        if (roll < 5)
        {
            Wrestler w = data.wrestlers[Random.Range(0, data.wrestlers.Count)];
            Debug.Log($"{w.name} cut a viral promo online and gained popularity!");
            w.popularity = Mathf.Min(100, w.popularity + 5);
        }
    }
}
