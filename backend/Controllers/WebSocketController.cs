using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("ws")]
public class WebSocketController : ControllerBase
{
    private static readonly List<WebSocket> Clients = new();

    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task HandleWebSocket(WebSocket webSocket)
    {
        Clients.Add(webSocket);
        await Receive(webSocket);
    }

    private async Task Receive(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                Clients.Remove(webSocket);
            }
        }
    }

    public static async Task SendMessage(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var client in Clients)
            if (client.State == WebSocketState.Open)
                await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    CancellationToken.None);
    }
}