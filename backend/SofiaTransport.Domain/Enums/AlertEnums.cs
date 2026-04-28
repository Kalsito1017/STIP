namespace SofiaTransport.Domain.Enums;

public enum AlertCause
{
    Unknown = 1,
    Other = 2,
    TechnicalProblem = 3,
    Strike = 4,
    Demonstration = 5,
    Accident = 6,
    Holiday = 7,
    Weather = 8,
    Maintenance = 9,
    Construction = 10,
    PoliceActivity = 11,
    MedicalEmergency = 12,
}

public enum AlertEffect
{
    NoService = 1,
    ReducedService = 2,
    SignificantDelays = 3,
    Detour = 4,
    AdditionalService = 5,
    ModifiedService = 6,
    OtherEffect = 7,
    UnknownEffect = 8,
    StopMoved = 9,
}

public enum AlertSeverity
{
    Info = 1,
    Warning = 2,
    Severe = 3,
}
