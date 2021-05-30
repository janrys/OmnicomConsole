using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Domain.Enums
{
    public interface ISystemModeEnum : IEnumeration
    {
    }

    public class SystemModeEnum : Enumeration, ISystemModeEnum
    {
        public static readonly SystemModeEnum RW = new SystemModeEnum(0, "read_write");
        public static readonly SystemModeEnum RO = new SystemModeEnum(1, "read_only");

        private SystemModeEnum(int value, string name) : base(value, name)
        {
        }

        public static IEnumerable<SystemModeEnum> GetAll() => GetAll<SystemModeEnum>();
    }

    
}
