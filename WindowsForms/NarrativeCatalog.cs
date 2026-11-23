using System.Diagnostics;
using System.Text.Json;

namespace WindowsForms;

internal sealed class NarrativeCatalog
{
    private readonly Dictionary<int, RoomDefinition> rooms = Read("salas_y_objetivos").EnumerateArray()
        .Select(room => new RoomDefinition(Number(room, "fase"), Number(room, "sala"), Text(room, "nombre"), Text(room, "objetivo")))
        .ToDictionary(room => room.Number);

    private readonly ILookup<int, TerminalDocument> documents = Read("documentos_encontrables").EnumerateArray()
        .ToLookup(doc => Number(doc, "sala"), doc => new TerminalDocument(Text(doc, "id"), Text(doc, "titulo"),
            Text(doc, "fecha"), Text(doc, "origen"), Text(doc, "texto")));

    private readonly Dictionary<(int, int, string), JarvisLine> lines = Read("jarvis_dialogos").EnumerateArray()
        .Select(line => new JarvisLine(Text(line, "id"), Number(line, "fase"), Number(line, "sala"),
            Text(line, "disparador"), Text(line, "texto"), line.GetProperty("bloqueante").GetBoolean()))
        .ToDictionary(line => (line.Phase, line.Room, line.Trigger));

    private readonly Dictionary<string, string[]> labels = Read("textos_interfaz").Deserialize<Dictionary<string, string[]>>()!;

    public RoomDefinition Room(int number) => rooms[number];
    public IEnumerable<TerminalDocument> Documents(int room) => documents[room];
    public JarvisLine Line(int phase, int room, string trigger) => lines[(phase, room, trigger)];
    public JarvisLine? FindLine(string id) => lines.Values.FirstOrDefault(line => line.Id == id);
    public string[] Labels(string screen) => labels[screen];

    private static JsonElement Read(string name)
    {
        using var stream = typeof(NarrativeCatalog).Assembly.GetManifestResourceStream($"Narrativa.{name}.json")
            ?? throw new InvalidDataException($"Falta el recurso narrativo {name}.");
        using var json = JsonDocument.Parse(stream);
        return json.RootElement.Clone();
    }

    private static string Text(JsonElement node, string name) => node.GetProperty(name).GetString()!;
    private static int Number(JsonElement node, string name) => node.GetProperty(name).GetInt32();
}

internal sealed class AudioManager
{
    public int Music { get; set; } = 70;
    public int Effects { get; set; } = 90;
    public int Voice { get; set; } = 100;

    public void QueueVoice(string eventId) => Trace.TraceInformation("JARVIS: {0}", eventId);
}
