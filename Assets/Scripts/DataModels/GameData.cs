using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public List<Wrestler> wrestlers;
    public List<Company> companies;
    public List<Title> titles;
    public List<Show> shows;
    public List<TagTeam> tagTeams = new List<TagTeam>();
    public List<Feud> feuds = new List<Feud>();

    public GameData()
    {
        wrestlers = new List<Wrestler>();
        companies = new List<Company>();
        titles = new List<Title>();
        shows = new List<Show>();
        tagTeams = new List<TagTeam>();
        feuds = new List<Feud>();
    }
}
