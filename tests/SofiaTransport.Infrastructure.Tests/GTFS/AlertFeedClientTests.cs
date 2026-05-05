using System.Net;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using SofiaTransport.Infrastructure.GTFS;
using Xunit;

namespace SofiaTransport.Infrastructure.Tests.GTFS;

public class AlertFeedClientTests
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

    private static byte[] CreateAlertsFeed()
    {
        using var ms = new MemoryStream();
        using var output = new CodedOutputStream(ms);

        // Build entity containing Alert
        using var entityMs = new MemoryStream();
        using var entityOutput = new CodedOutputStream(entityMs);

        // field 1: id = "e1"
        entityOutput.WriteTag(1, WireFormat.WireType.LengthDelimited);
        entityOutput.WriteString("e1");

        // field 5: Alert
        using var alertMs = new MemoryStream();
        using var alertOut = new CodedOutputStream(alertMs);

        // field 10: header_text (TranslatedText with nested Translation)
        using var headerMs = new MemoryStream();
        using var headerOut = new CodedOutputStream(headerMs);
        // Translation sub-message with field 2 = text
        using var translationMs = new MemoryStream();
        using var transOut = new CodedOutputStream(translationMs);
        transOut.WriteTag(2, WireFormat.WireType.LengthDelimited);
        transOut.WriteString("Route 204 Delayed");
        transOut.Flush();
        // Wrap Translation as field 1 of TranslatedText
        headerOut.WriteTag(1, WireFormat.WireType.LengthDelimited);
        headerOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(translationMs.ToArray()));
        headerOut.Flush();

        alertOut.WriteTag(10, WireFormat.WireType.LengthDelimited);
        alertOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(headerMs.ToArray()));

        // field 6: cause = 1
        alertOut.WriteTag(6, WireFormat.WireType.Varint);
        alertOut.WriteUInt64(1);

        // field 7: effect = 3
        alertOut.WriteTag(7, WireFormat.WireType.Varint);
        alertOut.WriteUInt64(3);

        // field 5: informed_entity with route_id = "r-204"
        using var ieMs = new MemoryStream();
        using var ieOut = new CodedOutputStream(ieMs);
        ieOut.WriteTag(2, WireFormat.WireType.LengthDelimited);
        ieOut.WriteString("r-204");
        ieOut.Flush();

        alertOut.WriteTag(5, WireFormat.WireType.LengthDelimited);
        alertOut.WriteBytes(Google.Protobuf.ByteString.CopyFrom(ieMs.ToArray()));
        alertOut.Flush();

        entityOutput.WriteTag(5, WireFormat.WireType.LengthDelimited);
        entityOutput.WriteBytes(Google.Protobuf.ByteString.CopyFrom(alertMs.ToArray()));
        entityOutput.Flush();

        // FeedMessage: field 2 = entity
        output.WriteTag(2, WireFormat.WireType.LengthDelimited);
        output.WriteBytes(Google.Protobuf.ByteString.CopyFrom(entityMs.ToArray()));
        output.Flush();

        return ms.ToArray();
    }

    [Fact]
    public async Task FetchAlertsAsync_SuccessfulFetch_ReturnsAlerts()
    {
        // Arrange
        var feedBytes = CreateAlertsFeed();
        var handler = CreateMessageHandler(HttpStatusCode.OK, feedBytes);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/alerts") };
        var logger = Mock.Of<ILogger<AlertFeedClient>>();
        var client = new AlertFeedClient(httpClient, logger);

        // Act
        var result = await client.FetchAlertsAsync(CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("e1", result[0].AlertId);
        Assert.Equal("Route 204 Delayed", result[0].HeaderText);
        Assert.Equal(1, result[0].Cause);
        Assert.Equal(3, result[0].Effect);
        Assert.Single(result[0].InformedEntities);
        Assert.Equal("r-204", result[0].InformedEntities[0].RouteId);
    }

    [Fact]
    public async Task FetchAlertsAsync_HttpError_Throws()
    {
        // Arrange
        var handler = CreateMessageHandler(HttpStatusCode.InternalServerError, Array.Empty<byte>());
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/error") };
        var logger = Mock.Of<ILogger<AlertFeedClient>>();
        var client = new AlertFeedClient(httpClient, logger);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.FetchAlertsAsync(CancellationToken.None));
    }

    [Fact]
    public async Task FetchAlertsAsync_EmptyFeed_ReturnsEmptyList()
    {
        // Arrange
        using var ms = new MemoryStream();
        using var output = new CodedOutputStream(ms);
        output.Flush();

        var handler = CreateMessageHandler(HttpStatusCode.OK, ms.ToArray());
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://gtfs.example.com/empty") };
        var logger = Mock.Of<ILogger<AlertFeedClient>>();
        var client = new AlertFeedClient(httpClient, logger);

        // Act
        var result = await client.FetchAlertsAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
