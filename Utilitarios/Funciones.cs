namespace Utilitarios
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Net.Http.Headers;
    using System.Text;
    using Utilitarios.Entidades;

    public class Funciones
    {
        /// <summary>Obtener fecha actual de colombia.</summary>
        /// <returns>Fecha de colombia.</returns>
        public static DateTime FechaActualColombia()
        {
            // En vez de usar DateTime.Now, usa:
            TimeZoneInfo colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            DateTime colombiaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, colombiaTimeZone);
            return colombiaTime;
        }

        /// <summary>Realiza peticiones a otro MS.</summary>
        /// <param name="urlApi">Ruta del MS que se va a consumir junto con su endpoint.</param>
        /// <param name="filtros">Filtros que se deben enviar con su valor y su resultado.</param>
        /// <returns>String JSON con la información consultada.</returns>
        public static async Task<ResultadoRespuestaObtenerDatos> ObtenerDatos(string urlApi, List<KeyValuePair<string, string>> filtros)
        {
            HttpResponseMessage respuesta = new HttpResponseMessage();
            ResultadoRespuestaObtenerDatos resultado = new ResultadoRespuestaObtenerDatos();

            using (var cliente = new HttpClient())
            {
                string filtrosPeticion = string.Join("&", filtros
                .Where(f => !string.IsNullOrWhiteSpace(f.Value))
                .Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}"));

                // Validar si hay filtros y construir la URL correctamente
                string peticionURL = string.IsNullOrWhiteSpace(filtrosPeticion) ? urlApi : $"{urlApi}?{filtrosPeticion}";

                // Realizar la solicitud GET
                respuesta = cliente.GetAsync(peticionURL).Result;

                // Manejar la respuesta
                string datos = await respuesta.Content.ReadAsStringAsync();
                int codigoRespuesta = (int)respuesta.StatusCode;
                if (codigoRespuesta == (int)Enumeradores.Enum.RespuestaAPI.Ok)
                {
                    resultado.Codigo = codigoRespuesta;
                    resultado.Datos = datos;
                }
                else if (codigoRespuesta == (int)Enumeradores.Enum.RespuestaAPI.Validacion)
                {
                    var errorObj = JsonConvert.DeserializeObject<JObject>(datos);
                    throw new ValidationException(errorObj?["detail"]?.ToString() ?? "Error de validación desconocido", new(datos));
                }
                else
                {
                    string[] partes = urlApi.Split('/');
                    string MicroServicio = partes[partes.Length - 2];
                    throw new ValidationException("Error en comunicación con el MS " + MicroServicio + ".", new(datos));
                }
            }
            return resultado;
        }

        /// <summary>
        /// Envía una solicitud HTTP POST a la URL especificada con datos opcionales y un token Bearer opcional.
        /// </summary>
        /// <param name="urlApi">La URL del endpoint al que se enviará la solicitud.</param>
        /// <param name="datos">
        /// (Opcional) Objeto que será serializado a JSON y enviado en el cuerpo de la solicitud. Si es nulo, se enviará un cuerpo vacío en formato JSON.
        /// </param>
        /// <param name="tokenBearer">
        /// (Opcional) Token de autenticación Bearer. Si se proporciona, será incluido en el encabezado de la solicitud.
        /// </param>
        /// <returns>
        /// Un objeto <see cref="ResultadoRespuestaObtenerDatos"/> que contiene el código de respuesta HTTP y los datos devueltos por la API.
        /// </returns>
        /// <exception cref="ValidationException">
        /// Se lanza si la API devuelve un código de validación o cualquier error diferente a una respuesta exitosa.
        /// </exception>
        public static async Task<ResultadoRespuestaObtenerDatos> EnviarDatosPostAsync(string urlApi, object datos = null, string tokenBearer = null)
        {
            HttpResponseMessage respuesta = new HttpResponseMessage();
            ResultadoRespuestaObtenerDatos resultado = new ResultadoRespuestaObtenerDatos();

            using (var cliente = new HttpClient())
            {
                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrWhiteSpace(tokenBearer))
                {
                    cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenBearer);
                }

                // Serializar los datos (si es nulo, se enviará como "null" en JSON)
                string jsonDatos = JsonConvert.SerializeObject(datos);
                var contenido = new StringContent(jsonDatos, Encoding.UTF8, "application/json");

                // Realizar la solicitud POST
                respuesta = await cliente.PostAsync(urlApi, contenido);

                // Leer la respuesta
                string respuestaContenido = await respuesta.Content.ReadAsStringAsync();
                int codigoRespuesta = (int)respuesta.StatusCode;

                if (codigoRespuesta == (int)Enumeradores.Enum.RespuestaAPI.Ok)
                {
                    resultado.Codigo = codigoRespuesta;
                    resultado.Datos = respuestaContenido;
                }
                else if (codigoRespuesta == (int)Enumeradores.Enum.RespuestaAPI.Validacion)
                {
                    var errorObj = JsonConvert.DeserializeObject<JObject>(respuestaContenido);
                    throw new ValidationException(errorObj?["detail"]?.ToString() ?? "Error de validación desconocido", new(respuestaContenido));
                }
                else
                {
                    string[] partes = urlApi.Split('/');
                    string MicroServicio = partes[partes.Length - 2];
                    throw new ValidationException("Error en comunicación con el MS " + MicroServicio + ".", new(respuestaContenido));
                }
            }

            return resultado;
        }



        /// <summary>Obtener Token.</summary>
        /// <param name="parametros">Parametros de envio.</param>
        /// <returns>Token.</returns>
        /// <exception cref="Exception"></exception>
        public static async Task<string> ObtenerTokenAsync(string urlApi, Dictionary<string, string> parametros)
        {
            using (var cliente = new HttpClient())
            {
                var contenido = new FormUrlEncodedContent(parametros);
                var respuesta = await cliente.PostAsync(urlApi, contenido);

                if (!respuesta.IsSuccessStatusCode)
                {
                    string error = await respuesta.Content.ReadAsStringAsync();
                    throw new Exception("Error al obtener el token: " + error);
                }

                string respuestaContenido = await respuesta.Content.ReadAsStringAsync();
                TokenResponse objRespuesta = JsonConvert.DeserializeObject<TokenResponse>(respuestaContenido);

                return objRespuesta.access_token;
            }
        }
    }
}
