namespace Utilitarios.Interfaces.Socket
{
    using Utilitarios.Entidades;

    /// <summary>
    /// Define la interfaz para el servicio de comunicación WebSocket.
    /// </summary>
    public interface ISocketService
    {
        /// <summary>
        /// Establece la conexión con el servidor WebSocket.
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// Cierra la conexión con el servidor WebSocket.
        /// </summary>
        Task DisconnectAsync();
    }
}