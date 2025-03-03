﻿using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace AngularCrudApi.Application.Exceptions
{
    public class ValidationException : ApiException
    {
        public ValidationException() : base("One or more validation failures have occurred.")
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; }

        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            foreach (var failure in failures)
            {
                Errors.Add(failure.ErrorMessage);
            }
        }

        public ValidationException(IEnumerable<string> failureMessages)
            : this()
        {
            Errors.AddRange(failureMessages);
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}