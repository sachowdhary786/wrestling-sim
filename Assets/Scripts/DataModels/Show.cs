using System;
using System.Collections.Generic;

[Serializable]
public class Show
{
    public string id;
    public string showName;
    public string date;
    public List<Match> matches;
    public int averageRating;
    public string companyId;
}
