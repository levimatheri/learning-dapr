using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPISample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> _logger;
        private readonly DaprClient _daprClient;

        public SampleController(ILogger<SampleController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        public const string StoreName = "statestore";

        [HttpGet("{account}")]
        public ActionResult<Account> Get([FromState(StoreName)] StateEntry<Account> account)
        {
            if (account.Value is null)
                return NotFound("Account not found");
            return account.Value;
        }

        [Topic("pubsub", "deposit", "amountDeadLetterTopic", false)]
        [HttpPost("deposit")]
        public async Task<ActionResult<Account>> Deposit(Transaction transaction) 
        {
            _logger.LogInformation("Enter deposit");
            var state = await _daprClient.GetStateEntryAsync<Account>(StoreName, transaction.Id);
            state.Value ??= new Account { Id = transaction.Id };
            _logger.LogInformation("Id is {0}, the amount to be deposited is {1}", transaction.Id, transaction.Amount);

            if (transaction.Amount < 0m) 
            {
                return BadRequest("Transaction amount is invalid!");
            }

            state.Value.Balance += transaction.Amount;
            _logger.LogInformation("Balance is {0}", state.Value.Balance);
            await state.SaveAsync();
            return state.Value;
        }
    }
}
