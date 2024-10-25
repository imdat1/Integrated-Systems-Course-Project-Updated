using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IHuggingFaceQuerryRunner
    {
        Task<string> GenerateQuerry(string schema, string question, string huggingFaceAPIToken);
        Task<string> ExecuteQuerryAsync(string host, string database, string username, string password, string query);

        Task<string> GenerateNaturalLanguageResponse(string schema, string question, string query, string sqlresponse, string huggingFaceAPIToken);

    }
}
