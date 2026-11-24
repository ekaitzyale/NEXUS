namespace WindowsForms;

internal sealed record RoomDefinition(int Phase, int Number, string Name, string Objective);
internal sealed record TerminalDocument(string Id, string Title, string Date, string Origin, string Text);
internal sealed record JarvisLine(string Id, int Phase, int Room, string Trigger, string Text, bool Blocking);
