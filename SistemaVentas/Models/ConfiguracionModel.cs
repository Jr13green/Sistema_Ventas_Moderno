namespace SistemaVentas.Models
{
    /// <summary>
    /// Configuraciones globales del sistema
    /// </summary>
    public class ConfiguracionModel
    {
        public string Clave { get; set; }
        public string Valor { get; set; }

        // Propiedades útiles
        public int ObtenerValorInt(int predeterminado = 0)
        {
            if (int.TryParse(Valor, out int resultado))
                return resultado;
            return predeterminado;
        }

        public decimal ObtenerValorDecimal(decimal predeterminado = 0)
        {
            if (decimal.TryParse(Valor, out decimal resultado))
                return resultado;
            return predeterminado;
        }

        public bool ObtenerValorBool(bool predeterminado = false)
        {
            return Valor == "1" || Valor?.ToLower() == "true" || predeterminado;
        }
    }
}