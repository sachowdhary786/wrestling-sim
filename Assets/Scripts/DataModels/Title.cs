using System;
using System.Collections.Generic;

[Serializable]
public class Title
{
    public string id;
    public string name;
    public string companyId;
    public string currentChampionId;
    public List<string> previousChampions; // Historicity
}
