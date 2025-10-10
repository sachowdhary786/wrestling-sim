public enum Alignment
{
    Face,
    Heel,
    Neutral,
}

public enum MatchType
{
    // Standard Matches
    Singles,
    TagTeam,
    TripleThreat,
    FatalFourWay,
    SixManTag,
    Handicap,

    // Multi-Person Matches
    BattleRoyal,
    RoyalRumble,
    Gauntlet,

    // Cage/Enclosure Matches
    SteelCage,
    HellInACell,
    EliminationChamber,
    WarGames,

    // Gimmick Matches
    LadderMatch,
    TablesMatch,
    ChairsMatch,
    TLC,
    StairwayToHell,
    MoneyInTheBank,

    // Hardcore/Extreme
    Hardcore,
    NoDisqualification,
    StreetFight,
    FallsCountAnywhere,
    LastManStanding,
    IQuitMatch,

    // Specialty Matches
    IronMan,
    TwoOutOfThree,
    Submission,
    Inferno,
    BuriedAlive,
    CasketMatch,
    AmbulanceMatch,

    // Team Matches
    Elimination,
    Survivor,
    WarGames5v5,

    // Unique Formats
    Rumble30,
    Rumble40,
    KingOfTheRing,
    Tournament,
}

public enum MatchSimulationMode
{
    Simple,
    Advanced,
}
