using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using AngularCrudApi.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularCrudApi.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class CodebooksController : BaseApiController
    {
        private readonly ILogger<CodebooksController> log;

        public CodebooksController(IMediator mediator, ILogger<CodebooksController> log) : base(mediator)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Get list of codebooks
        /// </summary>
        /// <param name="includeRds">Include also RDS codebooks</param>
        /// <returns>Codebook list</returns>
        [ProducesResponseType(typeof(IEnumerable<Codebook>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeRds = false)
        {
            try
            {
                IEnumerable<Codebook> codeBooks = await this.Query().Codebook.All(includeRds);
                return this.Ok(codeBooks);
            }
            catch (Exception exception)
            {
                string errorMessage = "Error loading codebooks";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Get codebook detail
        /// </summary>
        /// <param name="codebookName">Codebook full name</param>
        /// <returns>Codebook list</returns>
        [ProducesResponseType(typeof(CodebookDetail), StatusCodes.Status200OK)]
        [HttpGet("{codebookName}")]
        public async Task<IActionResult> GetCodebook(string codebookName)
        {
            if (string.IsNullOrEmpty(codebookName))
            {
                return this.BadRequest($"Parameter {nameof(codebookName)} is mandatory");
            }

            try
            {
                CodebookDetail codebookDetail = await this.Query().Codebook.ByName(codebookName);

                if (codebookDetail == null)
                {
                    return this.NotFound();
                }

                return this.Ok(codebookDetail);
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error loading codebook detail {codebookName}";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Get codebook data
        /// </summary>
        /// <param name="codebookName">Codebook full name</param>
        /// <returns>Codebook data</returns>
        [ProducesResponseType(typeof(CodebookDetailWithData), StatusCodes.Status200OK)]
        [HttpGet("{codebookName}/data")]
        public async Task<IActionResult> GetCodebookData(string codebookName)
        {
            if (string.IsNullOrEmpty(codebookName))
            {
                return this.BadRequest($"Parameter {nameof(codebookName)} is mandatory");
            }

            try
            {
                CodebookDetailWithData codebookDetail = await this.Query().Codebook.Data(codebookName);

                if (codebookDetail == null)
                {
                    return this.NotFound();
                }

                return this.Ok(codebookDetail);
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error loading codebook detail {codebookName}";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Change codebook data
        /// </summary>
        /// <param name="codebookName">Codebook full name</param>
        /// <param name="recordChanges">List of delete operations</param>
        /// <returns>Codebook data</returns>
        [ProducesResponseType(typeof(CodebookDetailWithData), StatusCodes.Status200OK)]
        [HttpPut("{codebookName}/data")]
        public async Task<IActionResult> ChangeCodebookData(string codebookName, [FromBody] RecordChange[] recordChanges)
        {
            if (string.IsNullOrEmpty(codebookName))
            {
                return this.BadRequest($"Parameter {nameof(codebookName)} is mandatory");
            }

            if (recordChanges == null || !recordChanges.Any())
            {
                return this.BadRequest($"Parameter {nameof(recordChanges)} is mandatory");
            }

            IEnumerable<string> wrongOperations = recordChanges
                .Where(c => !RecordChangeOperationEnum.GetAll().Any(o => o.Name.Equals(c.Operation, StringComparison.InvariantCultureIgnoreCase)))
                .Select(c => c.Operation);

            if (wrongOperations.Any())
            {
                return this.BadRequest($"Wrong operation values: {String.Join(", ", wrongOperations)} ");
            }            

            try
            {
                await this.Command().Codebook.ApplyChanges(codebookName, recordChanges);
                return this.Ok();
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error changing codebook data {codebookName}";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Get current lock state
        /// </summary>
        /// <returns>Codebook data</returns>
        [ProducesResponseType(typeof(LockState), StatusCodes.Status200OK)]
        [HttpGet("lock")]
        public async Task<IActionResult> GetLockState()
        {
            try
            {
                LockState lockState = await this.Query().Codebook.Lock();
                return this.Ok(lockState);
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error loading lock state";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Get current lock state
        /// </summary>
        /// <returns>Codebook data</returns>
        [ProducesResponseType(typeof(LockState), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpPost("lock")]
        public async Task<IActionResult> CreateLockState()
        {
            LockState lockState = null;
            try
            {
                lockState = await this.Command().Codebook.Lock();
                return this.Ok(lockState);
            }
            catch (ValidationException validationException)
            {
                return this.Conflict(validationException.Errors);
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error loading lock state";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Release current lock state
        /// </summary>
        /// <returns>Codebook data</returns>
        [ProducesResponseType(typeof(LockState), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [HttpDelete("lock")]
        public async Task<IActionResult> ReleaseLockState()
        {
            LockState lockState = null;
            try
            {
                lockState = await this.Command().Codebook.Unlock();
                return this.Ok(lockState);
            }
            catch (ValidationException validationException)
            {
                return this.Conflict(validationException.Errors);
            }
            catch (Exception exception)
            {
                string errorMessage = $"Error releasing lock state";
                this.log.LogError(errorMessage, exception);
                throw new Exception(errorMessage);
            }
        }

    }
}
