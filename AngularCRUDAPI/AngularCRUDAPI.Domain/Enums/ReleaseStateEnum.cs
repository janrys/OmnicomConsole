using System.Collections.Generic;

namespace AngularCrudApi.Domain.Enums
{
    public interface IReleaseStateEnum : IEnumeration
    {
    }

    public class ReleaseStateEnum : Enumeration, IReleaseStateEnum
    {
        public static readonly ReleaseStateEnum New = new ReleaseStateEnum(0, "Nový");
        public static readonly ReleaseStateEnum Finished = new ReleaseStateEnum(1, "Ukončený");

        private ReleaseStateEnum(int value, string name) : base(value, name)
        {
        }

        public static IEnumerable<ReleaseStateEnum> GetAll() => GetAll<ReleaseStateEnum>();
    }

    


}
