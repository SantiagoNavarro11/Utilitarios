namespace Utilitarios
{
    using SocketIOClient;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Utilitarios.Entidades;
    using Utilitarios.Interfaces.Socket;

    /// <summary>
    /// Servicio que gestiona la conexión y comunicación con un servidor WebSocket mediante SocketIOClient.
    /// </summary>
    public class SocketService : ISocketService
    {
        private readonly SocketIO socket;
        private bool isConnected = false;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="SocketService"/> con las opciones de conexión configuradas.
        /// </summary>
        public SocketService()
        {
#if DEBUG
            String url = "http://localhost:3000/tc";
#else
            String url = "https://msalertas.icymoss-12e07bae.eastus2.azurecontainerapps.io/tc";
#endif

            socket = new SocketIO(url, new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = 3,
                ReconnectionDelay = 2000,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ConnectionTimeout = TimeSpan.FromSeconds(10)
            });

            socket.OnConnected += (sender, e) =>
            {
                isConnected = true;
                Console.WriteLine("✅ Conectado al servidor de WebSocket");
            };

            socket.OnDisconnected += (sender, e) =>
            {
                isConnected = false;
                Console.WriteLine("🔌 Desconectado del servidor");
            };

            socket.On("enviarActualizacion", response =>
            {
                Console.WriteLine("📥 Mensaje recibido del servidor: " + response);
            });

            socket.OnError += (sender, error) =>
            {
                Console.WriteLine("❌ Error de conexión: " + error);
            };
        }

        /// <summary>
        /// Conexion.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            if (!isConnected)
                await socket.ConnectAsync();
        }

        /// <summary>
        /// Desconexión.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (isConnected)
            {
                await socket.DisconnectAsync();
                isConnected = false;
            }
        }
    }
}