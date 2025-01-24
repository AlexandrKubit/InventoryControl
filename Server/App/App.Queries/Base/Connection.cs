using Npgsql;

namespace App.Queries.Base;
public static class Connection
{
    public static NpgsqlConnection Get()
    {
        var connectionString = "Host=127.0.0.1;Port=5432;Database=InventoryControl;Username=postgres;Password=MySupperPassword;";
        return new NpgsqlConnection(connectionString);
    }
}
