using System;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a wrestling company in the league.
/// </summary>
[Serializable]
public class Company
{
    public Guid id;
    public string name;
    public string acronym;
    public CompanyType companyType;
    public CompanyTier tier;
    public int prestige;
    public float finances;
    public BookingPhilosophy bookingPhilosophy;
    public List<Guid> roster;
    public List<Guid> titles;
    public int rosterCap;
    public List<CorporateStaff> corporateStaff;
    public bool delegateBooking;

    public Company(
        string name,
        string acronym,
        CompanyType companyType,
        CompanyTier tier,
        int prestige,
        float finances,
        BookingPhilosophy bookingPhilosophy,
        int rosterCap
    )
    {
        this.id = Guid.NewGuid();
        this.name = name;
        this.acronym = acronym;
        this.companyType = companyType;
        this.tier = tier;
        this.prestige = prestige;
        this.finances = finances;
        this.bookingPhilosophy = bookingPhilosophy;
        this.rosterCap = rosterCap;
        this.roster = new List<Guid>();
        this.titles = new List<Guid>();
        this.corporateStaff = new List<CorporateStaff>();
    }
}
