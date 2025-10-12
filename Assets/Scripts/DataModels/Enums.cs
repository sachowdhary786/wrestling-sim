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

public enum BookingPhilosophy
{
    Balanced, // Mix of everything
    Storyline, // Prioritize feuds and angles
    StarPower, // Feature top stars
    WorkRate, // Feature best in-ring workers
    Development, // Give newer talent opportunities
}

public enum MatchAim
{
    GreatMatch,
    AdvanceStory,
    Squash,
    ProtectTalent,
}

public enum FinishType
{
    Pinfall,
    Submission,
    Clean,
    Dirty,
    Interference,
    Injury,
    DQ,
    CountOut,
    Escape,
    DoubleCountOut,
    Knockout,
    ControversialFinish,
    BotchedFinish,
}

public enum CompanyType
{
    Player,
    AI,
}

public enum CompanyTier
{
    Indie,
    Major,
}

/// <summary>
/// Match position on the card
/// </summary>
public enum MatchSlot
{
    Opener,
    EarlyMidcard,
    Midcard,
    LateMidcard,
    SemiMain,
    MainEvent,
}

public enum StaffRole
{
    CEO,
    HeadBooker,
    RoadAgent,
    Trainer,
    Scout,
    Doctor,
}
