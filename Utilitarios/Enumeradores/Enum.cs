namespace Utilitarios.Enumeradores
{
    public class Enum
    {
        /// <summary>
        /// Define los codigos de respuesta de la API.
        /// </summary>
        public enum RespuestaAPI
        {
            Ok = 200,
            Validacion = 422,
            Error = 500
        }

        public enum Resultado
        {
            Ok = 1,
            Validacion = 2,
            Error = 3
        }

        public enum SocketTC
        {
            intentosDeReconexion = 3,
            timeOut = 1000,
        }
    }
}
