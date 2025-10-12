using System;

[Serializable]
public class CorporateStaff
{
    public Guid staffId; // Links to the Wrestler's ID
    public StaffRole role;

    // Role-specific skills (0-100)
    public int bookingAcuity;       // For HeadBooker
    public int psychologyInfluence;   // For RoadAgent
    public int skillImprovement;      // For Trainer
    public int talentDiscovery;       // For Scout
    public int injuryRecoveryBonus;   // For Doctor

    public CorporateStaff(Guid staffId, StaffRole role)
    {
        this.staffId = staffId;
        this.role = role;

        // Assign default randomized skills based on role
        switch (role)
        {
            case StaffRole.HeadBooker:
                bookingAcuity = new Random().Next(40, 80);
                break;
            case StaffRole.RoadAgent:
                psychologyInfluence = new Random().Next(40, 80);
                break;
            case StaffRole.Trainer:
                skillImprovement = new Random().Next(40, 80);
                break;
            case StaffRole.Scout:
                talentDiscovery = new Random().Next(40, 80);
                break;
            case StaffRole.Doctor:
                injuryRecoveryBonus = new Random().Next(10, 30); // Represents a percentage bonus
                break;
        }
    }
}
