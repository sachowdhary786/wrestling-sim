using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public List<Wrestler> wrestlers;
    public List<Company> companies;
    public List<Title> titles;
    public List<Show> shows;
    public List<TagTeam> teams = new List<TagTeam>();
    public List<Feud> feuds = new List<Feud>();
    public List<Referee> referees = new List<Referee>();
    public List<Trait> traits = new List<Trait>();

    public GameData()
    {
        wrestlers = new List<Wrestler>();
        companies = new List<Company>();
        titles = new List<Title>();
        shows = new List<Show>();
        teams = new List<TagTeam>();
        feuds = new List<Feud>();
        referees = new List<Referee>();
        traits = new List<Trait>();
    }
}
