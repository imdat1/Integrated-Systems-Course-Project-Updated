using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IDatabaseSchemaRetriever
    {
        Task<string> GetDatabaseSchemaAsync(string host, string database, string username, string password);
    }
}
