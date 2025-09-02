namespace Utilitarios.Expresiones
{
    using System.Text.RegularExpressions;

    public static class ExpresionesRegulares
    {

        /// <summary>Validar si es un numero.</summary>
        /// <param name="input">Texto a validar.</param>
        /// <returns>Si cumple o no con la condición.</returns>
        public static bool EsSoloNumeros(string input)
        {
            // Expresión regular para validar que la cadena contiene solo dígitos
            string patron = @"^\d+$";

            // Validar la cadena usando la expresión regular
            return Regex.IsMatch(input, patron);
        }

        /// <summary>Validar si es un correo.</summary>
        /// <param name="email">Texto a validar.</param>
        /// <returns>Si cumple o no con la condición.</returns>
        public static bool EsCorreoValido(string email)
        {
            // Expresión regular para validar un correo electrónico
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Validar la cadena usando la expresión regular
            return Regex.IsMatch(email, patron);
        }

    }
}
