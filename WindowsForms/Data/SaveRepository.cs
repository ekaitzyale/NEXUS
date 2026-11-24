using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace WindowsForms;

internal sealed record SavedGame(Guid Id, string Name, DateTime SavedAt, int Phase, int Room, bool Completed)
{
    public override string ToString() =>
        $"{Name}  ·  FASE {Phase} / SALA {Room:00}{(Completed ? "  ·  FASE COMPLETADA" : "")}  ·  {SavedAt.ToLocalTime():dd/MM/yyyy HH:mm}";
}

internal sealed class SaveRepository(string connectionString)
{
    public const string LocalConnection =
        @"Server=lpc:.\SQLEXPRESS;Database=NexusIris;Integrated Security=True;Encrypt=Optional;TrustServerCertificate=True;Connect Timeout=5";

    public async Task SaveAsync(GameSession session, bool create = false)
    {
        if (!session.IsValidSave()) throw new InvalidDataException("El estado de la partida no es válido.");
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = create
            ? """
                INSERT INTO dbo.Partida (Id, Cadete, Fase, Sala, FaseCompletada, Estado)
                VALUES (@id, @name, @phase, @room, @completed, @state);
                """
            : """
                UPDATE dbo.Partida SET FechaGuardado = SYSUTCDATETIME(), Fase = @phase, Sala = @room,
                    FaseCompletada = @completed, Estado = @state
                WHERE Id = @id;
                """;
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = session.Id;
        if (create) command.Parameters.Add("@name", SqlDbType.NVarChar, 60).Value = session.Name;
        command.Parameters.Add("@phase", SqlDbType.TinyInt).Value = session.Phase;
        command.Parameters.Add("@room", SqlDbType.TinyInt).Value = session.Room;
        command.Parameters.Add("@completed", SqlDbType.Bit).Value = session.PhaseComplete;
        command.Parameters.Add("@state", SqlDbType.NVarChar, -1).Value = JsonSerializer.Serialize(session);
        if (await command.ExecuteNonQueryAsync() != 1) throw new InvalidDataException("La partida ya no existe.");
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<SavedGame>> ListAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Cadete, FechaGuardado, Fase, Sala, FaseCompletada
            FROM dbo.Partida ORDER BY FechaGuardado DESC, Id;
            """;
        await using var reader = await command.ExecuteReaderAsync();
        var saves = new List<SavedGame>();
        while (await reader.ReadAsync())
        {
            saves.Add(new SavedGame(reader.GetGuid(0), reader.GetString(1),
                DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc), reader.GetByte(3), reader.GetByte(4), reader.GetBoolean(5)));
        }
        return saves;
    }

    public async Task<GameSession> LoadAsync(Guid id)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Cadete, Fase, Sala, FaseCompletada, Estado FROM dbo.Partida WHERE Id = @id;";
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) throw new InvalidDataException("La partida ya no existe.");
        var session = JsonSerializer.Deserialize<GameSession>(reader.GetString(4));
        if (session is null || session.Id != id || !session.IsValidSave()
            || session.Name != reader.GetString(0) || session.Phase != reader.GetByte(1)
            || session.Room != reader.GetByte(2) || session.PhaseComplete != reader.GetBoolean(3))
            throw new InvalidDataException("El estado guardado no es válido.");
        return session;
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM dbo.Partida WHERE Id = @id;";
        command.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
        await command.ExecuteNonQueryAsync();
    }
}
