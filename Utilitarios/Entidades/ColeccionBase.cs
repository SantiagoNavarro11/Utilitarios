namespace Utilitarios.Entidades
{
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>Inicializa una nueva instancia de la clase <see cref="ColeccionBase"/></summary>
    public class ColeccionBase
    {
        /// <value>Llave primaria de la colección.</value>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>Constructor que inicializa las propiedades de ColeccionBase.</summary>
        public ColeccionBase()
        {
            Id = string.Empty;
        }
    }
}