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

        [Topic("pubsub", "amountDeadLetterTopic")]
        [HttpPost("deadLetterTopicRoute")]
        public ActionResult<Account> ViewErrorMessage(Transaction transaction) 
        {
            _logger.LogInformation("The amount cannot be negative {0}", transaction.Amount);
            return Ok($"The amount cannot be negative {transaction.Amount}");
        }

        [Topic("pubsub", "withdraw", "amountDeadLetterTopic", false)]
        [HttpPost("withdraw")]
        public async Task<ActionResult<Account>> Withdraw(Transaction transaction)
        {
            _logger.LogInformation("Enter withdraw method...");
            var state = await _daprClient.GetStateEntryAsync<Account>(StoreName, transaction.Id);
            _logger.LogInformation("Id is {0}, the amount to be withdrawn is {1}", transaction.Id, transaction.Amount);

            if (state.Value == null)
            {
                return this.NotFound();
            }
            if (transaction.Amount < 0m)
            {
                return BadRequest(new { statusCode = 400, message = "Amount cannot be negative" });
            }

            state.Value.Balance -= transaction.Amount;
            _logger.LogInformation("Balance is {0}", state.Value.Balance);
            await state.SaveAsync();
            return state.Value;
        }

        [Topic("pubsub", "withdraw", "event.type ==\"withdraw.v2\"", 1)]
        [HttpPost("withdraw.v2")]
        public async Task<ActionResult<Account>> WithdrawV2(TransactionV2 transaction)
        {
            _logger.LogInformation("Enter withdraw.v2");
            if (transaction.Channel == "mobile" && transaction.Amount > 10000)
            {
                return this.Unauthorized("mobile transactions for large amounts are not permitted.");
            }

            var state = await _daprClient.GetStateEntryAsync<Account>(StoreName, transaction.Id);

            if (state.Value == null)
            {
                return this.NotFound();
            }

            state.Value.Balance -= transaction.Amount;
            await state.SaveAsync();
            return state.Value;
        }

        [HttpPost("throwException")]
        public async Task<ActionResult<Account>> ThrowException(Transaction transaction, [FromServices] DaprClient daprClient)
        {
            _logger.LogInformation("Enter ThrowException");
            await Task.Delay(10);
            return BadRequest(new { statusCode = 400, message = "bad request" });
        }
    }
}
