namespace Utilitarios.Entidades
{
    /// <summary>Inicializa una nueva instancia de la clase <see cref="EntidadBase"/></summary>
    public abstract class EntidadBase
    {
        /// <summary>Constructor que inicializa las propiedades de EntidadBase</summary>
        protected EntidadBase() { }
    }

    /* Respuesta despues de ejecutar el metodo de getData a otro MicroServicio  */
    public class ResultadoRespuestaObtenerDatos
    {
        /// <value>Codigo que retorna la API.</value>
        public int Codigo { get; set; }
        /// <value>Datos que retorna la API.</value>
        public string Datos { get; set; }
    }
}