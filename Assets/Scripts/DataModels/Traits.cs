[System.Serializable]
public class Trait
{
    public string id;
    public string name;
    public string description;
    public TraitEffect effect;
}

public enum TraitEffect
{
    None,
    CrowdFavourite, // +5% hometown or big crowd boost
    HardcoreSpecialist, // +10 rating in hardcore matches
    SubmissionExpert, // +10 rating if opponent has low toughness
    BigMatchPerformer, // +5% in title/PPV matches
    LazyWorker, // -5% random penalty
    ChemistryMaster, // Always gets +5 chemistry with everyone
}
