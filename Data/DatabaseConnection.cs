using System.Data.SQLite;

namespace DataStructureTest.Data
{
    class DatabaseConnection
    {
        private readonly string INMEMORY_CONNECTION_URI = "Data Source=:memory:";
        private readonly string CONNECTION_URI = @"URI=file:C:\Users\User\source\repos\DataStructureTest\DataStructureTest\NestedSet.db";
        private SQLiteConnection conn;
        private static DatabaseConnection _instance = new DatabaseConnection();

        private DatabaseConnection() {
            conn = new SQLiteConnection(CONNECTION_URI);
            conn.Open();
        }

        public static DatabaseConnection GetInstance()
        {
            return _instance;
        }

        public static SQLiteConnection GetConnection()
        {
            return _instance.conn;
        }
    }
}
