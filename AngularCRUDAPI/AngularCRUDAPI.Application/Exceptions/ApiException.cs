﻿using System;
using System.Globalization;

namespace AngularCrudApi.Application.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException() : base()
        {
        }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, params object[] args)
            : base(String.Format(CultureInfo.CurrentCulture, message, args))
        {
        }

        protected ApiException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }


        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}