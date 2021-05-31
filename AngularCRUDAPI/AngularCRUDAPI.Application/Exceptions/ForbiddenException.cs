using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Exceptions
{
    /// <summary>
    /// Exception is thrown when user has not enough privileges to access the resource or execute action
    /// </summary>
    [System.Serializable]
    public class ForbiddenException : ApiException
    {
        public ForbiddenException() { }
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, Exception inner) : base(message, inner) { }
        protected ForbiddenException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
