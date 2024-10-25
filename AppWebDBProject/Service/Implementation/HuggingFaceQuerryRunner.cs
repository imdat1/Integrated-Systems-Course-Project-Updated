using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Npgsql;
using Service.Interface;

namespace Service.Implementation
{
    public class HuggingFaceQuerryRunner : IHuggingFaceQuerryRunner
    {
        public async Task<string> GenerateQuerry(string schema, string question, string huggingFaceAPIToken)
        {
            var client = new HttpClient();

            var requestUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2/v1/chat/completions";

            // Define the JSON request payload
            var requestData = new
            {
                model = "mistralai/Mistral-7B-Instruct-v0.2",
                messages = new[]
                {
            new { role = "user", content = "Based on the table schema below, write a SQL query that would answer the user's question and always only return the query and no aditional context:\r\n "+ schema +"\r\n\r\n    Question: "+ question +"\r\n    SQL Query:" }
        },
                temperature = 0.5,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestData);

            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Add the Authorization header
            client.DefaultRequestHeaders.Add("Authorization", "Bearer "+huggingFaceAPIToken);

            // Make the HTTP POST request asynchronously
            var response = await client.PostAsync(requestUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // Extract the "content" field
                var content = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                // Print the content
                return content;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return "Error";
            }
        }


        public async Task<string> ExecuteQuerryAsync(string host, string database, string username, string password, string query)
        {
            var results = new List<string>();

            try
            {
                using (var connection = new NpgsqlConnection($"Host={host};Database={database};Username={username};Password={password}"))
                {
                    await connection.OpenAsync();

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Create a string for each row, joining the values with a comma
                            var rowValues = new List<string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                // Handle string values to ensure proper formatting
                                var value = reader[i] is string ? $"'{reader[i]}'" : reader[i].ToString();
                                rowValues.Add(value);
                            }
                            // Add formatted row to results
                            results.Add($"({string.Join(", ", rowValues)})");
                        }
                    }
                }

                // Join all rows into the final output string
                return $"[{string.Join(", ", results)}]";
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine($"Error: {ex.Message}");
                return "Error"; // Return "Error" if something goes wrong
            }
        }

        public async Task<string> GenerateNaturalLanguageResponse(string schema, string question, string query, string sqlresponse, string huggingFaceAPIToken)
        {
            var client = new HttpClient();

            var requestUrl = "https://api-inference.huggingface.co/models/mistralai/Mistral-7B-Instruct-v0.2/v1/chat/completions";

            // Define the JSON request payload
            var requestData = new
            {
                model = "mistralai/Mistral-7B-Instruct-v0.2",
                messages = new[]
                {
            new { role = "user", content = "Based on the table schema below, question, sql query, and sql response, write a natural language response:\r\n "+ schema +"\r\n\r\n    Question: "+ question +"\r\n    SQL Query:"+ query +"\r\n    SQL Response:"+ sqlresponse }
        },
                temperature = 0.5,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestData);

            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Add the Authorization header
            client.DefaultRequestHeaders.Add("Authorization", "Bearer "+huggingFaceAPIToken);

            // Make the HTTP POST request asynchronously
            var response = await client.PostAsync(requestUrl, requestContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response JSON
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // Extract the "content" field
                var content = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                // Print the content
                return content;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return "Error";
            }
        }
    }
}
