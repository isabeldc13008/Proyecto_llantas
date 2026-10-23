using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
namespace SistemaLlantas.Domain.Common;
public static class Nomenclatura
{
    public static string Diseno(string value) => new(value.Normalize(NormalizationForm.FormD).ToUpperInvariant()
        .Where(c => c is >= 'A' and <= 'Z' or >= '0' and <= '9').ToArray());
    public static string Referencia(string marca, string diseno, int maximo = 30)
    {
        var normal = Diseno(diseno);
        var code = $"RF_{marca}_{normal}";
        if (normal.Length == 0 || code.Length > maximo) throw new ArgumentException("Diseño vacío o código de referencia demasiado largo.");
        return code;
    }
    public static string Abreviatura(string value)
    {
        var v=value.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(v, "\\A[A-Z]{2}\\z")) throw new ArgumentException("Abreviatura: exactamente dos letras A-Z.");
        return v;
    }
    public static long Siguiente(IEnumerable<string> existentes, string prefijo, long minimo, long reservado=0)
    {
        var max=Math.Max(minimo-1,reservado);
        foreach(var code in existentes)
            if(code.StartsWith(prefijo,StringComparison.OrdinalIgnoreCase))
            {
                var suffix=code[prefijo.Length..];
                if(suffix.Length>0 && suffix.All(char.IsAsciiDigit) && long.TryParse(suffix,NumberStyles.None,CultureInfo.InvariantCulture,out var n)) max=Math.Max(max,n);
            }
        return checked(max+1);
    }
}
