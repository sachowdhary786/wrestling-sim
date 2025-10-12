using System;
using System.Collections.Generic;

[Serializable]
public class Title
{
    public Guid id;
    public string name;
    public Guid companyId;
    public Guid? currentChampionId;
    public List<Guid> previousChampions = new List<Guid>(); // Historicity
}
