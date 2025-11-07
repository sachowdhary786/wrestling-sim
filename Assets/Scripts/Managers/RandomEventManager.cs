using UnityEngine;

public static class RandomEventManager
{
    public static void CheckForEvents(GameData data)
    {
        int roll = Random.Range(0, 100);
        if (roll < 5)
        {
            var wrestlers = new System.Collections.Generic.List<Wrestler>(data.wrestlers.Values);
            Wrestler w = wrestlers[Random.Range(0, wrestlers.Count)];
            Debug.Log($"{w.name} cut a viral promo online and gained popularity!");
            w.popularity = Mathf.Min(100, w.popularity + 5);
        }
    }
}
