using System.Net;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SofiaTransport.Infrastructure.GTFS;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.GTFS;

public class TripUpdateFeedClientTests
{
    private static Mock<HttpMessageHandler> CreateMessageHandler(HttpStatusCode statusCode, byte[] content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new ByteArrayContent(content)
            });
        return handler;
    }

    private static byte[] CreateTripUpdateFeed()
    {
        using var ms = new MemoryStream();
        using var output = new CodedOutputStream(ms);

        // Build entity containing TripUpdate
        using var entityMs = new MemoryStream();
        using var entityOutput = new CodedOutputStream(entityMs);

        // field 1: id = "e1"
        entityOutput.WriteTag(1, WireFormat.WireType.LengthDelimited);
        entityOutput.WriteString("e1");

        // field 4: TripUpdate
        using var tuMs = new MemoryStream();
        using var tuOut = new CodedOutputStream(tuMs);

        // TripDescriptor (field 1)
        using var tripMs = new MemoryStream();
        using var tripOut = new CodedOutputStream(tripMs);
        tripOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        tripOut.WriteString("t1");
        tripOut.WriteTag(5, WireFormat.WireType.LengthDelimited);
        tripOut.WriteString("r-1");
        tripOut.Flush();

        tuOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        tuOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(tripMs.ToArray()));

        // StopTimeEventUpdate (field 2)
        using var stuMs = new MemoryStream();
        using var stuOut = new CodedOutputStream(stuMs);
        stuOut.WriteTag(1, WireFormat.WireType.Varint);
        stuOut.WriteUInt32(1);
        stuOut.WriteTag(4, WireFormat.WireType.LengthDelimited);
        stuOut.WriteString("s-001");
        // Arrival delay
        using var arrMs = new MemoryStream();
        using var arrOut = new CodedOutputStream(arrMs);
        arrOut.WriteTag(1, WireFormat.WireType.Varint);
        arrOut.WriteInt64(180);
        arrOut.Flush();
        stuOut.WriteTag(2, WireFormat.WireType.LengthDelimited);
        stuOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(arrMs.ToArray()));
        stuOut.Flush();

        tuOut.WriteTag(2, WireFormat.WireType.LengthDelimited);
        tuOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(stuMs.ToArray()));

        // VehicleDescriptor (field 3)
        using var vdMs = new MemoryStream();
        using var vdOut = new CodedOutputStream(vdMs);
        vdOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        vdOut.WriteString("v1");
        vdOut.Flush();

        tuOut.WriteTag(3, WireFormat.WireType.LengthDelimited);
        tuOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(vdMs.ToArray()));
        tuOut.Flush();

        entityOutput.WriteTag(3, WireFormat.WireType.LengthDelimited);
        entityOutput.WriteBytes(Google.Protobuf.ByteString.CopyFrom(tuMs.ToArray()));
        entityOutput.Flush();

        // FeedMessage: field 2 = entity
        output.WriteTag(2, WireFormat.WireType.LengthDelimited);
        output.WriteBytes(Google.Protobuf.ByteString.CopyFrom(entityMs.ToArray()));
        output.Flush();

        return ms.ToArray();
    }

    [Fact]
    public async Task FetchTripUpdatesAsync_SuccessfulFetch_ReturnsTripUpdates()
    {
        // Arrange
        var feedBytes = CreateTripUpdateFeed();
        var handler = CreateMessageHandler(HttpStatusCode.OK, feedBytes);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/tripUpdates") };
        var logger = Mock.Of<ILogger<TripUpdateFeedClient>>();
        var client = new TripUpdateFeedClient(httpClient, logger);

        // Act
        var result = await client.FetchTripUpdatesAsync(CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("t1", result[0].TripId);
        Assert.Equal("r-1", result[0].RouteId);
        Assert.Equal("v1", result[0].VehicleId);
        Assert.Single(result[0].StopTimeUpdates);
        Assert.Equal(1, result[0].StopTimeUpdates[0].StopSequence);
        Assert.Equal("s-001", result[0].StopTimeUpdates[0].StopId);
        Assert.Equal(180, result[0].StopTimeUpdates[0].ArrivalDelay);
    }

    [Fact]
    public async Task FetchTripUpdatesAsync_HttpError_Throws()
    {
        // Arrange
        var handler = CreateMessageHandler(HttpStatusCode.InternalServerError, Array.Empty<byte>());
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/error") };
        var logger = Mock.Of<ILogger<TripUpdateFeedClient>>();
        var client = new TripUpdateFeedClient(httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.FetchTripUpdatesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task FetchTripUpdatesAsync_EmptyFeed_ReturnsEmptyList()
    {
        // Arrange
        using var ms = new MemoryStream();
        using var output = new CodedOutputStream(ms);
        output.Flush();

        var handler = CreateMessageHandler(HttpStatusCode.OK, ms.ToArray());
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/empty") };
        var logger = Mock.Of<ILogger<TripUpdateFeedClient>>();
        var client = new TripUpdateFeedClient(httpClient, logger);

        // Act
        var result = await client.FetchTripUpdatesAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
