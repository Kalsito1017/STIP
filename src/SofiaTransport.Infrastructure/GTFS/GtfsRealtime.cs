using Google.Protobuf;

namespace TransitRealtime;

public sealed class FeedMessage
{
    public List<FeedEntity> Entity { get; } = new();

    public static FeedMessage ParseFrom(byte[] data)
    {
        var msg = new FeedMessage();
        var input = new CodedInputStream(data);

        while (input.ReadTag() is uint tag)
        {
            if (WireFormat.GetTagFieldNumber(tag) == 1)
            {
                var bytes = input.ReadBytes();
                var sub = new CodedInputStream(bytes.ToByteArray());
                msg.Entity.Add(FeedEntity.Parse(sub));
            }
            else input.SkipLastField();
        }

        return msg;
    }
}

public sealed class FeedEntity
{
    public string Id { get; set; } = string.Empty;
    public TripUpdate? TripUpdate { get; set; }
    public Alert? Alert { get; set; }
    public VehiclePosition? Vehicle { get; set; }

    internal static FeedEntity Parse(CodedInputStream input)
    {
        var entity = new FeedEntity();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: entity.Id = input.ReadString(); break;
                case 4:
                    var b4 = input.ReadBytes();
                    entity.TripUpdate = TransitRealtime.TripUpdate.Parse(new CodedInputStream(b4.ToByteArray()));
                    break;
                case 5:
                    var b5 = input.ReadBytes();
                    entity.Alert = TransitRealtime.Alert.Parse(new CodedInputStream(b5.ToByteArray()));
                    break;
                case 8:
                    var b8 = input.ReadBytes();
                    entity.Vehicle = VehiclePosition.Parse(new CodedInputStream(b8.ToByteArray()));
                    break;
                default: input.SkipLastField(); break;
            }
        }
        return entity;
    }
}

public sealed class VehiclePosition
{
    public TripDescriptor? Trip { get; set; }
    public VehicleDescriptor? Vehicle { get; set; }
    public Position Position { get; set; } = new();

    internal static VehiclePosition Parse(CodedInputStream input)
    {
        var vp = new VehiclePosition();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1:
                    var b1 = input.ReadBytes();
                    vp.Trip = TripDescriptor.Parse(new CodedInputStream(b1.ToByteArray()));
                    break;
                case 2:
                    var b2 = input.ReadBytes();
                    vp.Position = Position.Parse(new CodedInputStream(b2.ToByteArray()));
                    break;
                case 8:
                    var b8 = input.ReadBytes();
                    vp.Vehicle = VehicleDescriptor.Parse(new CodedInputStream(b8.ToByteArray()));
                    break;
                default: input.SkipLastField(); break;
            }
        }
        return vp;
    }
}

public sealed class TripDescriptor
{
    public string TripId { get; set; } = string.Empty;
    public string? RouteId { get; set; }
    public string? StartTime { get; set; }
    public string? StartDate { get; set; }
    public int ScheduleRelationship { get; set; }

    internal static TripDescriptor Parse(CodedInputStream input)
    {
        var td = new TripDescriptor();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: td.TripId = input.ReadString(); break;
                case 2: td.StartTime = input.ReadString(); break;
                case 3: td.StartDate = input.ReadString(); break;
                case 4: td.ScheduleRelationship = (int)input.ReadUInt64(); break;
                case 5: td.RouteId = input.ReadString(); break;
                default: input.SkipLastField(); break;
            }
        }
        return td;
    }
}

public sealed class VehicleDescriptor
{
    public string Id { get; set; } = string.Empty;

    internal static VehicleDescriptor Parse(CodedInputStream input)
    {
        var vd = new VehicleDescriptor();
        while (input.ReadTag() is uint tag)
        {
            if (WireFormat.GetTagFieldNumber(tag) == 1) vd.Id = input.ReadString();
            else input.SkipLastField();
        }
        return vd;
    }
}

public sealed class Position
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Bearing { get; set; }
    public float Speed { get; set; }

    internal static Position Parse(CodedInputStream input)
    {
        var pos = new Position();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: pos.Latitude = input.ReadFloat(); break;
                case 2: pos.Longitude = input.ReadFloat(); break;
                case 3: pos.Bearing = input.ReadFloat(); break;
                case 6: pos.Speed = input.ReadFloat(); break;
                default: input.SkipLastField(); break;
            }
        }
        return pos;
    }
}

public sealed class TripUpdate
{
    public TripDescriptor? Trip { get; set; }
    public VehicleDescriptor? Vehicle { get; set; }
    public List<StopTimeEventUpdate> StopTimeUpdates { get; } = [];

    internal static TripUpdate Parse(CodedInputStream input)
    {
        var tu = new TripUpdate();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1:
                    var b1 = input.ReadBytes();
                    tu.Trip = TripDescriptor.Parse(new CodedInputStream(b1.ToByteArray()));
                    break;
                case 2:
                    {
                        var b2 = input.ReadBytes();
                        tu.Vehicle = VehicleDescriptor.Parse(new CodedInputStream(b2.ToByteArray()));
                        break;
                    }
                case 3:
                    var b3 = input.ReadBytes();
                    tu.StopTimeUpdates.Add(StopTimeEventUpdate.Parse(new CodedInputStream(b3.ToByteArray())));
                    break;
                default: input.SkipLastField(); break;
            }
        }
        return tu;
    }
}

public sealed class StopTimeEventUpdate
{
    public int? StopSequence { get; set; }
    public string? StopId { get; set; }
    public StopTimeEvent? Arrival { get; set; }
    public StopTimeEvent? Departure { get; set; }
    public int ScheduleRelationship { get; set; }

    internal static StopTimeEventUpdate Parse(CodedInputStream input)
    {
        var stu = new StopTimeEventUpdate();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 2: stu.StopSequence = (int)input.ReadUInt32(); break;
                case 4: stu.StopId = input.ReadString(); break;
                case 12:
                    var b12 = input.ReadBytes();
                    stu.Arrival = StopTimeEvent.Parse(new CodedInputStream(b12.ToByteArray()));
                    break;
                case 13:
                    var b13 = input.ReadBytes();
                    stu.Departure = StopTimeEvent.Parse(new CodedInputStream(b13.ToByteArray()));
                    break;
                case 21: stu.ScheduleRelationship = (int)input.ReadUInt64(); break;
                default: input.SkipLastField(); break;
            }
        }
        return stu;
    }
}

public sealed class StopTimeEvent
{
    public int? Delay { get; set; }
    public long? Time { get; set; }
    public int? Uncertainty { get; set; }

    internal static StopTimeEvent Parse(CodedInputStream input)
    {
        var ste = new StopTimeEvent();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: ste.Delay = (int)input.ReadInt64(); break;
                case 2: ste.Time = input.ReadInt64(); break;
                case 3: ste.Uncertainty = (int)input.ReadUInt32(); break;
                default: input.SkipLastField(); break;
            }
        }
        return ste;
    }
}

public sealed class Alert
{
    public List<TimeRange> ActivePeriods { get; } = [];
    public List<EntitySelector> InformedEntities { get; } = [];
    public int Cause { get; set; }
    public int Effect { get; set; }
    public TranslatedText? Url { get; set; }
    public TranslatedText? HeaderText { get; set; }
    public TranslatedText? DescriptionText { get; set; }
    public int? Severity { get; set; }

    internal static Alert Parse(CodedInputStream input)
    {
        var alert = new Alert();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1:
                    var b1 = input.ReadBytes();
                    alert.ActivePeriods.Add(TimeRange.Parse(new CodedInputStream(b1.ToByteArray())));
                    break;
                case 5:
                    var b5 = input.ReadBytes();
                    alert.InformedEntities.Add(EntitySelector.Parse(new CodedInputStream(b5.ToByteArray())));
                    break;
                case 6: alert.Cause = (int)input.ReadUInt64(); break;
                case 7: alert.Effect = (int)input.ReadUInt64(); break;
                case 8:
                    var b8 = input.ReadBytes();
                    alert.Url = TranslatedText.Parse(new CodedInputStream(b8.ToByteArray()));
                    break;
                case 10:
                    var b10 = input.ReadBytes();
                    alert.HeaderText = TranslatedText.Parse(new CodedInputStream(b10.ToByteArray()));
                    break;
                case 11:
                    var b11 = input.ReadBytes();
                    alert.DescriptionText = TranslatedText.Parse(new CodedInputStream(b11.ToByteArray()));
                    break;
                case 22:
                    alert.Severity = (int)input.ReadUInt64();
                    break;
                default: input.SkipLastField(); break;
            }
        }
        return alert;
    }
}

public sealed class TimeRange
{
    public long? Start { get; set; }
    public long? End { get; set; }

    internal static TimeRange Parse(CodedInputStream input)
    {
        var tr = new TimeRange();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: tr.Start = (long)input.ReadUInt64(); break;
                case 2: tr.End = (long)input.ReadUInt64(); break;
                default: input.SkipLastField(); break;
            }
        }
        return tr;
    }
}

public sealed class EntitySelector
{
    public string? AgencyId { get; set; }
    public string? RouteId { get; set; }
    public int? RouteType { get; set; }
    public TripDescriptor? Trip { get; set; }
    public string? StopId { get; set; }

    internal static EntitySelector Parse(CodedInputStream input)
    {
        var es = new EntitySelector();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: es.AgencyId = input.ReadString(); break;
                case 2: es.RouteId = input.ReadString(); break;
                case 3: es.RouteType = (int)input.ReadUInt32(); break;
                case 4:
                    var b4 = input.ReadBytes();
                    es.Trip = TripDescriptor.Parse(new CodedInputStream(b4.ToByteArray()));
                    break;
                case 5: es.StopId = input.ReadString(); break;
                default: input.SkipLastField(); break;
            }
        }
        return es;
    }
}

public sealed class TranslatedText
{
    public string Text { get; set; } = string.Empty;
    public string? Language { get; set; }

    internal static TranslatedText Parse(CodedInputStream input)
    {
        var tt = new TranslatedText();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1:
                    var b1 = input.ReadBytes();
                    var sub = new CodedInputStream(b1.ToByteArray());
                    while (sub.ReadTag() is uint subTag)
                    {
                        if (WireFormat.GetTagFieldNumber(subTag) == 2)
                            tt.Text = sub.ReadString();
                        else if (WireFormat.GetTagFieldNumber(subTag) == 1)
                            tt.Language = sub.ReadString();
                        else sub.SkipLastField();
                    }
                    break;
                default: input.SkipLastField(); break;
            }
        }
        return tt;
    }
}