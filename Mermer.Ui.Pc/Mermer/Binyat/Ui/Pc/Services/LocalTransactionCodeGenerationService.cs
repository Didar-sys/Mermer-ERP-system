using System;
using System.Threading.Tasks;
using Mermer.Transactions.Services;

namespace Mermer.Ui.Pc.Services
{
    public class LocalTransactionCodeGenerationService : ITransactionCodeGenerationService
    {
        public Task<string> GetNextCode()
        {
            // Генерируем уникальный номер на основе текущей даты и времени, 
            // чтобы вообще не зависеть от Couchbase
            string code = $"DOC-{DateTime.Now:yyMMddHHmmss}";
            return Task.FromResult(code);
        }
    }
}