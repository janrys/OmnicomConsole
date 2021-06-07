using System.Collections.Generic;

namespace AngularCrudApi.Domain.Enums
{
    public interface IRequestStateEnum : IEnumeration
    {
    }

    public class RequestStateEnum : Enumeration, IRequestStateEnum
    {
        public static readonly RequestStateEnum New = new RequestStateEnum(0, "Nový");
        public static readonly RequestStateEnum Authorized = new RequestStateEnum(1, "Schválený");
        public static readonly RequestStateEnum Exported = new RequestStateEnum(1, "Exportovaný");

        private RequestStateEnum(int value, string name) : base(value, name)
        {
        }

        public static IEnumerable<RequestStateEnum> GetAll() => GetAll<RequestStateEnum>();
    }


}
