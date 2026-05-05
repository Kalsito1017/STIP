using Xunit;
using SofiaTransport.Domain.Enums;

namespace SofiaTransport.Domain.Tests.Enums;

public class AlertEnumsTests
{
    [Fact]
    public void AlertCause_HasExpectedValues()
    {
        Assert.Equal(1, (int)AlertCause.Unknown);
        Assert.Equal(2, (int)AlertCause.Other);
        Assert.Equal(3, (int)AlertCause.TechnicalProblem);
        Assert.Equal(4, (int)AlertCause.Strike);
        Assert.Equal(5, (int)AlertCause.Demonstration);
        Assert.Equal(6, (int)AlertCause.Accident);
        Assert.Equal(7, (int)AlertCause.Holiday);
        Assert.Equal(8, (int)AlertCause.Weather);
        Assert.Equal(9, (int)AlertCause.Maintenance);
        Assert.Equal(10, (int)AlertCause.Construction);
        Assert.Equal(11, (int)AlertCause.PoliceActivity);
        Assert.Equal(12, (int)AlertCause.MedicalEmergency);
    }

    [Fact]
    public void AlertEffect_HasExpectedValues()
    {
        Assert.Equal(1, (int)AlertEffect.NoService);
        Assert.Equal(2, (int)AlertEffect.ReducedService);
        Assert.Equal(3, (int)AlertEffect.SignificantDelays);
        Assert.Equal(4, (int)AlertEffect.Detour);
        Assert.Equal(5, (int)AlertEffect.AdditionalService);
        Assert.Equal(6, (int)AlertEffect.ModifiedService);
        Assert.Equal(7, (int)AlertEffect.OtherEffect);
        Assert.Equal(8, (int)AlertEffect.UnknownEffect);
        Assert.Equal(9, (int)AlertEffect.StopMoved);
    }

    [Fact]
    public void AlertSeverity_HasExpectedValues()
    {
        Assert.Equal(1, (int)AlertSeverity.Info);
        Assert.Equal(2, (int)AlertSeverity.Warning);
        Assert.Equal(3, (int)AlertSeverity.Severe);
    }

    [Fact]
    public void AllEnums_HaveCorrectTotalCount()
    {
        Assert.Equal(12, Enum.GetValues<AlertCause>().Length);
        Assert.Equal(9, Enum.GetValues<AlertEffect>().Length);
        Assert.Equal(3, Enum.GetValues<AlertSeverity>().Length);
    }
}
