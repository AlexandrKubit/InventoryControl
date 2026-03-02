namespace App.Queries.Base;
using Npgsql;

public static class Connection
{
    private static string connectionString = "";

    public static void SetCommectionString(string cs)
    {
        if (connectionString == "")
            connectionString = cs;
    }

    public static NpgsqlConnection Get()
    {
        return new NpgsqlConnection(connectionString);
    }
}
