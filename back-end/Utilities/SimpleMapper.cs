using System.Collections.Concurrent;
using System.Reflection;

namespace Chat.Utilities;

// Mapper "semplice" ma performante:
// - Usa cache per evitare riflessione ripetuta (costosa).
// - Supporta mapping di liste.
// - Gestisce tipi compatibili (es. int -> int?, reference assegnabili).
public static class SimpleMapper
{
    // Cache delle proprietà per singolo tipo (es. tutte le PropertyInfo di User).
    // ConcurrentDictionary = thread-safe per scenari web (richieste concorrenti).
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propsCache = new();

    // Cache della "mappa" tra coppie di tipi (TSource -> TDest).
    // Ogni elemento è un array di coppie (prop sorgente, prop destinazione) già risolte.
    // Così, al momento del mapping, iteriamo solo queste coppie: niente ricerche.
    private static readonly ConcurrentDictionary<(Type Src, Type Dest), (PropertyInfo Src, PropertyInfo Dest)[]> _mapCache = new();

    // Mappa un singolo oggetto TSource in un nuovo TDest.
    public static TDest Map<TSource, TDest>(TSource source)
        where TDest : new() // TDest deve avere costruttore vuoto per poterlo istanziare
    {
        // Safety: se qualcuno passa null, segnaliamo subito l’errore con il nome del parametro.
        if (source == null) throw new ArgumentNullException(nameof(source));

        var srcType = typeof(TSource);
        var destType = typeof(TDest);

        // 1) Recupero (o costruisco e salvo in cache) la mappa di proprietà corrispondenti
        //    tra TSource e TDest. Questo è il pezzo "costoso" che vogliamo fare UNA SOLA VOLTA.
        var pairs = _mapCache.GetOrAdd((srcType, destType), key =>
        {
            var (sType, dType) = key;

            // Prendo le proprietà pubbliche istanza dei due tipi (probabilmente viene dalla cache)
            var srcProps = GetProps(sType);
            var destProps = GetProps(dType);

            // Preparo un lookup O(1) per proprietà di destinazione per nome (case-sensitive, veloce)
            var destByName = destProps
                .Where(p => p.CanWrite) // possiamo impostarle? altrimenti inutile mappare
                .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

            var matches = new List<(PropertyInfo Src, PropertyInfo Dest)>(srcProps.Length);

            // Per ogni proprietà sorgente, provo a trovare la corrispondente in dest
            foreach (var sProp in srcProps)
            {
                if (!destByName.TryGetValue(sProp.Name, out var dProp))
                    continue; // nessuna proprietà con lo stesso nome in dest

                // Verifico compatibilità dei tipi (uguali, nullable compatibile, reference assegnabile)
                if (AreTypesCompatible(sProp.PropertyType, dProp.PropertyType))
                    matches.Add((sProp, dProp));
            }

            // Salvo in cache la mappa "compilata": un array di coppie pronte all’uso.
            return matches.ToArray();
        });

        // 2) Creo l'istanza di destinazione e applico i valori proprietà per proprietà
        var dest = new TDest();

        foreach (var (srcProp, destProp) in pairs)
        {
            // Leggo il valore dalla sorgente via riflessione
            var value = srcProp.GetValue(source);

            // Se è null, setto null e passo oltre
            if (value is null)
            {
                destProp.SetValue(dest, null);
                continue;
            }

            // Se il tipo non è perfettamente uguale, provo a convertirlo se possibile
            // (gestione Nullable<T>, primitive convertibili, reference compatibili)
            var converted = ConvertIfNeeded(value, destProp.PropertyType);

            // Scrivo nella destinazione
            destProp.SetValue(dest, converted);
        }

        return dest;
    }

    // Comodo helper per mappare una sequenza (lista, IEnumerable) di oggetti.
    // Evita di ripetere Select(Map<,>) ovunque e rimane leggibile.
    public static List<TDest> MapList<TSource, TDest>(IEnumerable<TSource> source)
        where TDest : new()
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var result = new List<TDest>();
        foreach (var item in source)
            result.Add(Map<TSource, TDest>(item)); // riuso la logica singola: pulito e DRY
        return result;
    }

    // ---------- Helpers ----------

    // Prende in modo thread-safe e cache-izzato le proprietà pubbliche istanza di un Type.
    // Prima chiamata: riflessione; successive: hit di cache (molto veloce).
    private static PropertyInfo[] GetProps(Type type) =>
        _propsCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public));

    // Definisce cosa consideriamo "compatibile" tra i tipi di proprietà:
    // - Esattamente uguali
    // - Reference assegnabile (es. derivata -> base)
    // - Destinazione nullable del tipo sorgente (int -> int?)
    private static bool AreTypesCompatible(Type src, Type dest)
    {
        if (src == dest) return true;

        // Caso Nullable<T> in destinazione: accettiamo il tipo T "secco" o reference assegnabile
        var destUnderlying = Nullable.GetUnderlyingType(dest);
        if (destUnderlying != null)
            return destUnderlying == src || dest.IsAssignableFrom(src);

        // Per reference types: se dest è assegnabile da src, ok (es. Stream <- MemoryStream)
        return dest.IsAssignableFrom(src);
    }

    // Prova a restituire un valore convertito nel tipo di destinazione quando:
    // - i tipi sono compatibili (ritorna il valore com’è)
    // - destinazione è Nullable<T> (gestisce T)
    // - tipi primitivi convertibili (string -> int, ecc.) con ChangeType
    // In caso di conversione impossibile, ritorna null (fallback sicuro).
    private static object? ConvertIfNeeded(object value, Type destType)
    {
        if (value == null) return null;

        var srcType = value.GetType();

        // Caso più veloce: già assegnabile, non fare nulla
        if (destType.IsAssignableFrom(srcType))
            return value;

        // Gestione Nullable<T> in destinazione
        var underlying = Nullable.GetUnderlyingType(destType);
        if (underlying != null)
        {
            // Se il valore è già del tipo "secco" compatibile, lo accettiamo
            if (underlying.IsAssignableFrom(srcType))
                return value;

            // Tentativo di conversione primitiva verso T (es. "123" -> int)
            try { return Convert.ChangeType(value, underlying); }
            catch { return null; }
        }

        // Ultimo tentativo: conversione primitiva diretta (es. "true" -> bool)
        try { return Convert.ChangeType(value, destType); }
        catch { return null; } // se fallisce, meglio non esplodere: lascio null
    }
}
