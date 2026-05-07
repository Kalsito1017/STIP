using Google.Protobuf;

namespace TransitRealtime;

public sealed class ParseErrorInfo
{
    public int EntityIndex { get; init; }
    public string ErrorType { get; init; } = string.Empty;
    public long ByteOffset { get; init; }
    public string Message { get; init; } = string.Empty;
    public string FirstBytesHex { get; init; } = string.Empty;

    public override string ToString() =>
        $"Entity #{EntityIndex} [{ErrorType}] at offset {ByteOffset}: {Message} (first bytes: {FirstBytesHex})";
}

public sealed class FeedMessage
{
    public List<FeedEntity> Entity { get; } = new();
    public List<ParseErrorInfo> ParseErrors { get; } = new();

    public static FeedMessage ParseFrom(byte[] data)
    {
        var msg = new FeedMessage();
        var input = new CodedInputStream(data);
        var entityIndex = 0;

        while (input.ReadTag() is uint tag)
        {
            if (WireFormat.GetTagFieldNumber(tag) == 2)
            {
                entityIndex++;
                var byteOffset = input.Position;
                var bytes = input.ReadBytes();
                var rawBytes = bytes.ToByteArray();
                var sub = new CodedInputStream(rawBytes);
                try
                {
                    msg.Entity.Add(FeedEntity.Parse(sub));
                }
                catch (InvalidProtocolBufferException ex)
                {
                    var errorType = ex.Message.Contains("ended unexpectedly")
                        ? "Truncated"
                        : ex.Message.Contains("invalid tag")
                            ? "InvalidTag"
                            : "ProtobufError";
                    var previewLen = Math.Min(64, rawBytes.Length);
                    var firstBytes = previewLen > 0
                        ? Convert.ToHexString(rawBytes, 0, previewLen)
                        : "(empty)";
                    msg.ParseErrors.Add(new ParseErrorInfo
                    {
                        EntityIndex = entityIndex,
                        ErrorType = errorType,
                        ByteOffset = byteOffset,
                        Message = ex.Message,
                        FirstBytesHex = firstBytes
                    });
                }
            }
            else if (!ProtobufHelpers.TrySkipField(input)) break;
        }

        return msg;
    }
}

public sealed class FeedEntity
{
    public string Id { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
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
                case 2: entity.IsDeleted = input.ReadBool(); break;
                case 3:
                    var b3 = input.ReadBytes();
                    entity.TripUpdate = TransitRealtime.TripUpdate.Parse(new CodedInputStream(b3.ToByteArray()));
                    break;
                case 4:
                    var b4 = input.ReadBytes();
                    entity.Vehicle = VehiclePosition.Parse(new CodedInputStream(b4.ToByteArray()));
                    break;
                case 5:
                    var b5 = input.ReadBytes();
                    entity.Alert = TransitRealtime.Alert.Parse(new CodedInputStream(b5.ToByteArray()));
                    break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return entity;
                    break;
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
    public uint? CurrentStopSequence { get; set; }
    public string? StopId { get; set; }
    public int CurrentStatus { get; set; } = 2; // default IN_TRANSIT_TO
    public long? Timestamp { get; set; }
    public int CongestionLevel { get; set; }
    public int OccupancyStatus { get; set; }
    public uint? OccupancyPercentage { get; set; }

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
                case 3: vp.CurrentStopSequence = input.ReadUInt32(); break;
                case 4: vp.CurrentStatus = (int)input.ReadUInt64(); break;
                case 5: vp.Timestamp = (long)input.ReadUInt64(); break;
                case 6: vp.CongestionLevel = (int)input.ReadUInt64(); break;
                case 7: vp.StopId = input.ReadString(); break;
                case 8:
                    var b8 = input.ReadBytes();
                    vp.Vehicle = VehicleDescriptor.Parse(new CodedInputStream(b8.ToByteArray()));
                    break;
                case 9: vp.OccupancyStatus = (int)input.ReadUInt64(); break;
                case 10: vp.OccupancyPercentage = input.ReadUInt32(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return vp;
                    break;
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
    public uint? DirectionId { get; set; }

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
                case 6: td.DirectionId = input.ReadUInt32(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return td;
                    break;
            }
        }
        return td;
    }
}

public sealed class VehicleDescriptor
{
    public string Id { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? LicensePlate { get; set; }
    public int WheelchairAccessible { get; set; }

    internal static VehicleDescriptor Parse(CodedInputStream input)
    {
        var vd = new VehicleDescriptor();
        while (input.ReadTag() is uint tag)
        {
            switch (WireFormat.GetTagFieldNumber(tag))
            {
                case 1: vd.Id = input.ReadString(); break;
                case 2: vd.Label = input.ReadString(); break;
                case 3: vd.LicensePlate = input.ReadString(); break;
                case 4: vd.WheelchairAccessible = (int)input.ReadUInt64(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return vd;
                    break;
            }
        }
        return vd;
    }
}

public sealed class Position
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Bearing { get; set; }
    public double Odometer { get; set; }
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
                case 4: pos.Odometer = input.ReadDouble(); break;
                case 5: pos.Speed = input.ReadFloat(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return pos;
                    break;
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
    public long? Timestamp { get; set; }
    public int? Delay { get; set; }

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
                        tu.StopTimeUpdates.Add(StopTimeEventUpdate.Parse(new CodedInputStream(b2.ToByteArray())));
                        break;
                    }
                case 3:
                    {
                        var b3 = input.ReadBytes();
                        tu.Vehicle = VehicleDescriptor.Parse(new CodedInputStream(b3.ToByteArray()));
                        break;
                    }
                case 4: tu.Timestamp = (long)input.ReadUInt64(); break;
                case 5: tu.Delay = (int)input.ReadInt64(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return tu;
                    break;
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
                case 1: stu.StopSequence = (int)input.ReadUInt32(); break;
                case 2:
                    {
                        var b2 = input.ReadBytes();
                        stu.Arrival = StopTimeEvent.Parse(new CodedInputStream(b2.ToByteArray()));
                        break;
                    }
                case 3:
                    {
                        var b3 = input.ReadBytes();
                        stu.Departure = StopTimeEvent.Parse(new CodedInputStream(b3.ToByteArray()));
                        break;
                    }
                case 4: stu.StopId = input.ReadString(); break;
                case 5: stu.ScheduleRelationship = (int)input.ReadUInt64(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return stu;
                    break;
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
    public long? ScheduledTime { get; set; }

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
                case 4: ste.ScheduledTime = input.ReadInt64(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return ste;
                    break;
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
    public TranslatedText? TtsHeaderText { get; set; }
    public TranslatedText? TtsDescriptionText { get; set; }
    public int? Severity { get; set; }
    public TranslatedText? CauseDetail { get; set; }
    public TranslatedText? EffectDetail { get; set; }

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
                case 12:
                    var b12 = input.ReadBytes();
                    alert.TtsHeaderText = TranslatedText.Parse(new CodedInputStream(b12.ToByteArray()));
                    break;
                case 13:
                    var b13 = input.ReadBytes();
                    alert.TtsDescriptionText = TranslatedText.Parse(new CodedInputStream(b13.ToByteArray()));
                    break;
                case 14: alert.Severity = (int)input.ReadUInt64(); break;
                case 17:
                    var b17 = input.ReadBytes();
                    alert.CauseDetail = TranslatedText.Parse(new CodedInputStream(b17.ToByteArray()));
                    break;
                case 18:
                    var b18 = input.ReadBytes();
                    alert.EffectDetail = TranslatedText.Parse(new CodedInputStream(b18.ToByteArray()));
                    break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return alert;
                    break;
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
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return tr;
                    break;
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
    public uint? DirectionId { get; set; }

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
                case 6: es.DirectionId = input.ReadUInt32(); break;
                default:
                    if (!ProtobufHelpers.TrySkipField(input))
                        return es;
                    break;
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
                    else if (!ProtobufHelpers.TrySkipField(sub))
                        break;
                    }
                break;
            default:
                if (!ProtobufHelpers.TrySkipField(input))
                    return tt;
                break;
        }
    }
    return tt;
    }
}

internal static class ProtobufHelpers
{
    public static bool TrySkipField(CodedInputStream input)
    {
        try
        {
            input.SkipLastField();
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (InvalidProtocolBufferException)
        {
            return false;
        }
    }
}
