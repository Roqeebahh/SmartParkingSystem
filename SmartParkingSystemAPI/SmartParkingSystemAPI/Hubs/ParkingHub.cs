using Microsoft.AspNetCore.SignalR;
using SmartParkingSystemAPI.Dtos;
using SmartParkingSystemAPI.Models;
using SmartParkingSystemAPI.Models;

namespace SmartParkingSystemAPI.Hubs
{
    public class ParkingHub : Hub
    {
        private readonly ILogger<ParkingHub> _logger;

        public ParkingHub(ILogger<ParkingHub> logger)
        {
            _logger = logger;
        }

        /// <summary>Called by clients to subscribe to parking updates.</summary>
        public async Task JoinParkingMonitor()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ParkingMonitors");
            _logger.LogInformation("Client {ConnectionId} joined ParkingMonitors group.", Context.ConnectionId);
        }

        /// <summary>Broadcasts a single parking space update to all monitors.</summary>
        public async Task BroadcastParkingUpdate(SignalRUpdateDto update)
        {
            await Clients.Group("ParkingMonitors").SendAsync("ReceiveParkingUpdate", update);
        }

        /// <summary>Sends the full list of spaces to the requesting client.</summary>
        public async Task SendAllSpaces(List<ParkingSpaceDto> spaces)
        {
            await Clients.Caller.SendAsync("ReceiveAllSpaces", spaces);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}