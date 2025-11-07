using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Trait
{
    public Guid id;
    public string name;
    public string description;
    public TraitEffect effect;
}

public enum TraitEffect
{
    IronMan,
    TechnicalWizard,
    Brawler,
    HighFlyer,
    HardcoreSpecialist,
    SubmissionExpert,
    BigMatchPerformer,
    LazyWorker,
    ChemistryMaster,
    CrowdFavourite
}
