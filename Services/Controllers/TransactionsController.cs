namespace Vueling.OTD.Services.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Vueling.OTD.Contracts;
    using Vueling.OTD.Contracts.DTOs;

    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase, ITransactionsManager
    {
        private readonly ITransactionsManager transactionsManager;

        public TransactionsController(ITransactionsManager transactionsManager)
        {
            this.transactionsManager = transactionsManager;
        }

        [HttpGet("GetAllTransactions")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDTO>), 200)]
        public Task<IEnumerable<TransactionDTO>> GetAllTransactionsAsync()
        {
            return this.transactionsManager.GetAllTransactionsAsync();
        }

        [HttpGet("GetTransactionsBySkuInEuros")]
        [ProducesResponseType(typeof(SkuTransactionsDTO), 200)]
        public Task<SkuTransactionsDTO> GetTransactionsBySkuInEurosAsync([FromQuery]string sku)
        {
            return this.transactionsManager.GetTransactionsBySkuInEurosAsync(sku);
        }
    }
}