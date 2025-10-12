using System;

[Serializable]
public class Contract
{
    public Guid companyId;
    public int monthlySalary;
    public int durationInMonths;
    public DateTime startDate;
    public DateTime expiryDate;

    public Contract(Guid companyId, int salary, int duration)
    {
        this.companyId = companyId;
        this.monthlySalary = salary;
        this.durationInMonths = duration;
        this.startDate = DateTime.Now;
        this.expiryDate = startDate.AddMonths(duration);
    }
}
