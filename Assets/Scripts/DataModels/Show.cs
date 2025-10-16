using System;
using System.Collections.Generic;

[Serializable]
public class Show
{
    public Guid id;
    public string name;
    public Guid companyId;
    public string location;
    public List<Match> matches;
    public float averageRating;

    public Show(string name, Guid companyId)
    {
        this.id = Guid.NewGuid();
        this.name = name;
        this.companyId = companyId;
        this.matches = new List<Match>();
        this.averageRating = 0;
    }
}
