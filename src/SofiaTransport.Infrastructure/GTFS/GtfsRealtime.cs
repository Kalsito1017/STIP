using Google.Protobuf;

namespace TransitRealtime
{
    public sealed class FeedMessage
    {
        public List<FeedEntity> Entity { get; } = new();

        public static FeedMessage ParseFrom(byte[] data)
        {
            var msg = new FeedMessage();
            var input = new CodedInputStream(data);

            while (input.ReadTag() is uint tag)
            {
                if (WireFormat.GetTagFieldNumber(tag) == 1) // entity
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
        public VehiclePosition? Vehicle { get; set; }

        internal static FeedEntity Parse(CodedInputStream input)
        {
            var entity = new FeedEntity();
            while (input.ReadTag() is uint tag)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1: entity.Id = input.ReadString(); break;
                    case 8:
                        var bytes = input.ReadBytes();
                        entity.Vehicle = VehiclePosition.Parse(new CodedInputStream(bytes.ToByteArray()));
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
        public string RouteId { get; set; } = string.Empty;

        internal static TripDescriptor Parse(CodedInputStream input)
        {
            var td = new TripDescriptor();
            while (input.ReadTag() is uint tag)
            {
                switch (WireFormat.GetTagFieldNumber(tag))
                {
                    case 1: td.TripId = input.ReadString(); break;
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
}
