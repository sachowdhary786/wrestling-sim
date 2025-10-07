using System;
using Sysyem.Collections.Generic;

[Serializable]
public class Company
{
    public string id;
    public string name;
    public string region;
    public int popularity;
    public List<string> rosterIds;
    public List<string> titleIds;
    public List<string> showIds;
}
