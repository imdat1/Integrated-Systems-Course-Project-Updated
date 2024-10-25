using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Service.Interface;

namespace Service.Implementation
{
    public class DatabaseSchemaRetriever : IDatabaseSchemaRetriever
    {
        public async Task<string> GetDatabaseSchemaAsync(string host, string database, string username, string password)
        {
            var schemaBuilder = new StringBuilder();

            // Get tables
            using (var connection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var tables = new List<string>();
                        while (await reader.ReadAsync())
                        {
                            tables.Add(reader.GetString(0));
                        }

                        foreach (var table in tables)
                        {
                            schemaBuilder.AppendLine($"CREATE TABLE {table} (");

                            // Get columns
                            using (var columnConnection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
                            {
                                await columnConnection.OpenAsync();
                                using (var columnCommand = new NpgsqlCommand($"SELECT column_name, data_type, is_nullable, character_maximum_length FROM information_schema.columns WHERE table_name = '{table}'", columnConnection))
                                using (var columnReader = await columnCommand.ExecuteReaderAsync())
                                {
                                    var columns = new List<string>();
                                    while (await columnReader.ReadAsync())
                                    {
                                        var columnName = columnReader.GetString(0);
                                        var dataType = columnReader.GetString(1).ToUpper(); // Capitalize data type
                                        var isNullable = columnReader.GetString(2) == "YES" ? "" : "NOT NULL";
                                        var maxLength = columnReader.IsDBNull(3) ? "" : $"({columnReader.GetInt32(3)})";

                                        columns.Add($"    {columnName} {dataType}{maxLength} {isNullable},");
                                    }
                                    schemaBuilder.AppendLine(string.Join(Environment.NewLine, columns));
                                }
                            }

                            // Get primary key
                            using (var pkConnection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
                            {
                                await pkConnection.OpenAsync();
                                using (var pkCommand = new NpgsqlCommand($"SELECT kcu.column_name FROM information_schema.table_constraints AS tc JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name WHERE tc.table_name = '{table}' AND tc.constraint_type = 'PRIMARY KEY'", pkConnection))
                                using (var pkReader = await pkCommand.ExecuteReaderAsync())
                                {
                                    if (await pkReader.ReadAsync())
                                    {
                                        schemaBuilder.AppendLine($"    CONSTRAINT pk_{table} PRIMARY KEY ({pkReader.GetString(0)})");
                                    }
                                }
                            }

                            schemaBuilder.AppendLine(");");
                            schemaBuilder.AppendLine($"/*\n3 rows from {table} table:");

                            // Get example rows
                            using (var exampleConnection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
                            {
                                await exampleConnection.OpenAsync();
                                using (var exampleCommand = new NpgsqlCommand($"SELECT * FROM {table} LIMIT 3", exampleConnection))
                                using (var exampleReader = await exampleCommand.ExecuteReaderAsync())
                                {
                                    var header = string.Join("   ", await GetColumnNamesAsync(host, database, username, password, table));
                                    schemaBuilder.AppendLine(header);

                                    while (await exampleReader.ReadAsync())
                                    {
                                        var row = new List<string>();
                                        for (int i = 0; i < exampleReader.FieldCount; i++)
                                        {
                                            row.Add(exampleReader[i].ToString());
                                        }
                                        schemaBuilder.AppendLine(string.Join("   ", row));
                                    }
                                }
                            }

                            schemaBuilder.AppendLine("*/\n");
                        }
                    }
                }
            }

            return schemaBuilder.ToString();
        }


        private async Task<List<string>> GetColumnNamesAsync(string host, string database, string username, string password, string tableName)
        {
            var columnNames = new List<string>();
            using (var connection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand($"SELECT column_name FROM information_schema.columns WHERE table_name = '{tableName}'", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        columnNames.Add(reader.GetString(0));
                    }
                }
            }
            return columnNames;
        }

    }
}
