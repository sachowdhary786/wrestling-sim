using System;
using System.Collections.Generic;

[Serializable]
public class SerializableGameData
{
    public List<Company> companies;
    public List<Wrestler> wrestlers;
    public List<Title> titles;
    public List<Feud> feuds;
    public List<TagTeam> teams;
    public List<Referee> referees;
    public List<Trait> traits;
}
