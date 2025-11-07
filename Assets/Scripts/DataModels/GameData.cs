using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public Dictionary<Guid, Wrestler> wrestlers;
    public Dictionary<Guid, Company> companies;
    public Dictionary<Guid, Title> titles;
    public List<Show> shows;
    public List<TagTeam> teams = new List<TagTeam>();
    public List<Feud> feuds = new List<Feud>();
    public Dictionary<Guid, Referee> referees = new Dictionary<Guid, Referee>();
    public Dictionary<Guid, Trait> traits = new Dictionary<Guid, Trait>();

    public GameData()
    {
        wrestlers = new Dictionary<Guid, Wrestler>();
        companies = new Dictionary<Guid, Company>();
        titles = new Dictionary<Guid, Title>();
        shows = new List<Show>();
        teams = new List<TagTeam>();
        feuds = new List<Feud>();
        referees = new Dictionary<Guid, Referee>();
        traits = new Dictionary<Guid, Trait>();
    }
}
