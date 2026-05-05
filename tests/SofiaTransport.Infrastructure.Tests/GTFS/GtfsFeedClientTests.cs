using System.Net;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SofiaTransport.Infrastructure.GTFS;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.GTFS;

public class GtfsFeedClientTests
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

    private static byte[] CreateVehiclePositionsFeed()
    {
        using var ms = new MemoryStream();
        using var output = new CodedOutputStream(ms);

        // Build entity containing VehiclePosition
        using var entityMs = new MemoryStream();
        using var entityOutput = new CodedOutputStream(entityMs);

        // field 1: id = "e1"
        entityOutput.WriteTag(1, WireFormat.WireType.LengthDelimited);
        entityOutput.WriteString("e1");

        // field 8: VehiclePosition
        using var vpMs = new MemoryStream();
        using var vpOut = new CodedOutputStream(vpMs);

        // TripDescriptor (field 1): trip_id, route_id
        using var tripMs = new MemoryStream();
        using var tripOut = new CodedOutputStream(tripMs);
        tripOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        tripOut.WriteString("t1");
        tripOut.WriteTag(5, WireFormat.WireType.LengthDelimited);
        tripOut.WriteString("r-1");
        tripOut.Flush();

        vpOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        vpOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(tripMs.ToArray()));

        // Position (field 2): lat, lon, bearing, speed
        using var posMs = new MemoryStream();
        using var posOut = new CodedOutputStream(posMs);
        posOut.WriteTag(1, WireFormat.WireType.Fixed32);
        posOut.WriteFloat(42.69f);
        posOut.WriteTag(2, WireFormat.WireType.Fixed32);
        posOut.WriteFloat(23.33f);
        posOut.WriteTag(3, WireFormat.WireType.Fixed32);
        posOut.WriteFloat(90f);
        posOut.WriteTag(6, WireFormat.WireType.Fixed32);
        posOut.WriteFloat(40f);
        posOut.Flush();

        vpOut.WriteTag(2, WireFormat.WireType.LengthDelimited);
        vpOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(posMs.ToArray()));

        // VehicleDescriptor (field 8): id
        using var vdMs = new MemoryStream();
        using var vdOut = new CodedOutputStream(vdMs);
        vdOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        vdOut.WriteString("v1");
        vdOut.Flush();

        vpOut.WriteTag(8, WireFormat.WireType.LengthDelimited);
        vpOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(vdMs.ToArray()));
        vpOut.Flush();

        entityOutput.WriteTag(8, WireFormat.WireType.LengthDelimited);
        entityOutput.WriteBytes(Google.Protobuf.ByteString.CopyFrom(vpMs.ToArray()));
        entityOutput.Flush();

        // FeedMessage: field 2 = entity
        output.WriteTag(2, WireFormat.WireType.LengthDelimited);
        output.WriteBytes(Google.Protobuf.ByteString.CopyFrom(entityMs.ToArray()));
        output.Flush();

        return ms.ToArray();
    }

    [Fact]
    public async Task FetchVehiclePositionsAsync_SuccessfulFetch_ReturnsVehicles()
    {
        // Arrange
        var feedBytes = CreateVehiclePositionsFeed();
        var handler = CreateMessageHandler(HttpStatusCode.OK, feedBytes);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/vehiclePositions") };
        var logger = Mock.Of<ILogger<GtfsFeedClient>>();
        var client = new GtfsFeedClient(httpClient, logger);

        // Act
        var result = await client.FetchVehiclePositionsAsync(CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("v1", result[0].VehicleId);
        Assert.Equal("r-1", result[0].RouteId);
        Assert.Equal("t1", result[0].TripId);
        Assert.Equal(42.69, result[0].Location.Lat, 4);
        Assert.Equal(23.33, result[0].Location.Lon, 4);
    }

    [Fact]
    public async Task FetchVehiclePositionsAsync_HttpError_Throws()
    {
        // Arrange
        var handler = CreateMessageHandler(HttpStatusCode.InternalServerError, Array.Empty<byte>());
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/error") };
        var logger = Mock.Of<ILogger<GtfsFeedClient>>();
        var client = new GtfsFeedClient(httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.FetchVehiclePositionsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task FetchVehiclePositionsAsync_EmptyFeed_ReturnsEmptyList()
    {
        // Arrange
        // Create an empty FeedMessage with no entities
        using var ms = new MemoryStream();
        using var output = new CodedOutputStream(ms);
        // No fields written = empty feed
        output.Flush();
        var emptyBytes = ms.ToArray();

        var handler = CreateMessageHandler(HttpStatusCode.OK, emptyBytes);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/empty") };
        var logger = Mock.Of<ILogger<GtfsFeedClient>>();
        var client = new GtfsFeedClient(httpClient, logger);

        // Act
        var result = await client.FetchVehiclePositionsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
