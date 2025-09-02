namespace Utilitarios.Seguridad
{
    using System.Security.Cryptography;
    using System.Text;

    public class Seguridad
    {
        /// <summary>
        /// Genera un hash SHA256 de un texto (por ejemplo, una contraseña).
        /// </summary>
        public string EncriptarSHA256(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2")); // Convierte a hexadecimal
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Verifica si el texto coincide con el hash almacenado.
        /// </summary>
        public bool VerificarSHA256(string texto, string hashAlmacenado)
        {
            string hashTexto = EncriptarSHA256(texto);
            return hashTexto.Equals(hashAlmacenado);
        }
    }
}
