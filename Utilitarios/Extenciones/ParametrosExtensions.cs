namespace Utilitarios.Extenciones
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ParametrosExtensions
    {
        /// <summary>Crea un filtro dinámico de tipo Expression<Func<T, bool>> basado en las propiedades no nulas del objeto de parámetros.</summary>
        /// <typeparam name="T">El tipo de entidad para el que se está creando el filtro.</typeparam>
        /// <param name="parametros">El objeto con las propiedades que se utilizarán para construir el filtro.</param>
        /// <returns>Una expresión que representa el filtro.</returns>
        public static Expression<Func<T, bool>> CrearFiltro<T>(this object parametros)
        {
            // Define el parámetro de la expresión (ej. "x" en x => x.Propiedad).
            var parameter = Expression.Parameter(typeof(T), "x");

            // Comienza con una expresión constante que siempre es verdadera.
            Expression combined = Expression.Constant(true);

            // Itera sobre todas las propiedades del objeto de parámetros.
            foreach (PropertyInfo prop in parametros.GetType().GetProperties())
            {
                // Obtiene el valor de la propiedad actual.
                var value = prop.GetValue(parametros);

                // Solo crea una expresión si el valor no es nulo.
                if (value != null)
                {
                    // Crea una expresión que accede a la propiedad de la entidad (ej. "x.Propiedad").
                    var member = Expression.Property(parameter, typeof(T).GetProperty(prop.Name));

                    // Crea una expresión constante con el valor de la propiedad.
                    var constant = Expression.Constant(value);

                    Expression equal = null;

                    // Si la propiedad es de tipo Nullable, manejamos la comparación de manera especial
                    if (prop.PropertyType == typeof(DateTime?))// Maneja el caso especial de DateTime?, comparando solo si tiene valor.
                    {
                        var nullableValue = (DateTime?)value;
                        if (nullableValue.HasValue)
                        {
                            equal = Expression.GreaterThanOrEqual(member, Expression.Constant(nullableValue.Value));
                        }
                    }
                    else if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                    {
                        var memberValue = Expression.Convert(member, Nullable.GetUnderlyingType(prop.PropertyType));
                        equal = Expression.Equal(memberValue, Expression.Convert(constant, Nullable.GetUnderlyingType(prop.PropertyType)));
                    }
                    else
                    {
                        // Crea una expresión de igualdad para otros tipos de propiedades.
                        equal = Expression.Equal(member, constant);
                    }

                    // Combina la nueva expresión con la existente usando "AndAlso" (Y lógico).
                    combined = Expression.AndAlso(combined, equal);
                }
            }

            // Crea y devuelve la expresión lambda combinada.
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }
    }
}