using Microsoft.Data.SqlClient;
using PokeApiNet;

namespace PokemonSimulator.logic
{
    public class FightSqlDb : IFightsDB
    {
        const string fightsTableName = "fights";
        const string attacksTableName = "attacks";
        private string connectionString;
        private Task initDb;
        public FightSqlDb(string connectionString)
        {
            this.connectionString = connectionString;
            initDb = InitDb();
        }

        private async Task InitDb()
        {
            using (SqlConnection connection = new SqlConnection(
              connectionString))
            {
                SqlCommand createFightsTable = new SqlCommand($@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{fightsTableName}' and xtype='U')
CREATE TABLE {fightsTableName} (
id INT NOT NULL  IDENTITY(1,1) PRIMARY KEY, 
pokemon1 VARCHAR(40),
pokemon2 VARCHAR(40),
winner VARCHAR(40));;
", connection);
                await createFightsTable.Connection.OpenAsync();
                await createFightsTable.ExecuteNonQueryAsync();

                SqlCommand createMovesTable = new SqlCommand($@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{attacksTableName}' and xtype='U')
CREATE TABLE {attacksTableName} (
fightId INT NOT NULL,
turn INT NOT NULL,
attacker VARCHAR(40),
attacked VARCHAR(40),
move VARCHAR(40),
hit BIT,
CONSTRAINT PK_{attacksTableName} PRIMARY KEY CLUSTERED ([fightId], [turn]));;
", connection);

                await createMovesTable.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> AddFightAsync(PokemonFightSummary fight)
        {
            await initDb;
            using (SqlConnection connection = new SqlConnection(
              connectionString))
            {
                int id = await InsertFightMetaData(fight, connection);
                for (int i = 1; i <= fight.Attacks.Count(); i++)
                {
                    await InsertAttack(fight, connection, id, i);
                }
                return id;
            }

        }

        private static async Task InsertAttack(PokemonFightSummary fight, SqlConnection connection, int id, int i)
        {
            SqlCommand insertAttack = new SqlCommand($@"insert into {attacksTableName} VALUES (@fightId,@turn,@attacker,@attacked,@move,@hit)", connection);
            insertAttack.Parameters.AddWithValue("@fightId", id);
            insertAttack.Parameters.AddWithValue("@turn", i);
            insertAttack.Parameters.AddWithValue("@attacker", fight.Attacks.ElementAt(i - 1).Attacker.Name);
            insertAttack.Parameters.AddWithValue("@attacked", fight.Attacks.ElementAt(i - 1).Attacked.Name);
            insertAttack.Parameters.AddWithValue("@move", fight.Attacks.ElementAt(i - 1).Move.Name);
            insertAttack.Parameters.AddWithValue("@hit", fight.Attacks.ElementAt(i - 1).Hit);
            await insertAttack.ExecuteNonQueryAsync();
        }

        private static async Task<int> InsertFightMetaData(PokemonFightSummary fight, SqlConnection connection)
        {
            SqlCommand insert = new SqlCommand($@"insert into {fightsTableName} OUTPUT inserted.id VALUES (@pokemon1,@pokemon2,@winner)", connection);
            insert.Parameters.AddWithValue("@pokemon1", fight.Pokemon1.Name);
            insert.Parameters.AddWithValue("@pokemon2", fight.Pokemon2.Name);
            insert.Parameters.AddWithValue("@winner", fight.Winner.Name);
            await insert.Connection.OpenAsync();
            var id = (int)await insert.ExecuteScalarAsync();
            return id;
        }

        public async Task<PokemonFightSummary> GetFightResultsAsync(int fightId)
        {
            await initDb;
            using (SqlConnection connection = new SqlConnection(
              connectionString))
            {
                object pokemon1, pokemon2, winner;
                ReadFightMetaData(fightId, connection, out pokemon1, out pokemon2, out winner);
                List<Attack> attacks = ReadFightAttacks(fightId, connection);
                return new PokemonFightSummary(new Pokemon { Name = (string)pokemon1 }, new Pokemon { Name = (string)pokemon2 }, attacks, new Pokemon { Name = (string)winner });
            }
        }

        private static List<Attack> ReadFightAttacks(int fightId, SqlConnection connection)
        {
            SqlCommand attacksQuery = new SqlCommand(@$"Select * from {attacksTableName} where fightId = {fightId} order by turn", connection);
            var attacksReader = attacksQuery.ExecuteReader();
            var attacks = new List<Attack>();
            while (attacksReader.Read())
            {
                attacks.Add(new Attack(new Pokemon { Name = (string)attacksReader.GetValue(2) }, new Pokemon { Name = (string)attacksReader.GetValue(3) }, new Move { Name = (string)attacksReader.GetValue(4) }, (bool)attacksReader.GetValue(5)));
            }

            return attacks;
        }

        private static void ReadFightMetaData(int fightId, SqlConnection connection, out object pokemon1, out object pokemon2, out object winner)
        {
            SqlCommand fightQuery = new SqlCommand(@$"Select * from {fightsTableName} where id = {fightId}", connection);
            connection.Open();
            var reader = fightQuery.ExecuteReader();
            reader.Read();

            pokemon1 = reader.GetValue(1);
            pokemon2 = reader.GetValue(2);
            winner = reader.GetValue(3);
            reader.Close();
        }
    }
}
