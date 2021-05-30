using System.Collections.Generic;

namespace AngularCrudApi.Domain.Enums
{
    public interface IRecordChangeOperationEnum : IEnumeration
    {
    }

    public class RecordChangeOperationEnum : Enumeration, IRecordChangeOperationEnum
    {
        public static readonly RecordChangeOperationEnum Insert = new RecordChangeOperationEnum(0, "insert");
        public static readonly RecordChangeOperationEnum Update = new RecordChangeOperationEnum(1, "update");
        public static readonly RecordChangeOperationEnum Delete = new RecordChangeOperationEnum(2, "delete");

        private RecordChangeOperationEnum(int value, string name) : base(value, name)
        {
        }

        public static IEnumerable<RecordChangeOperationEnum> GetAll() => GetAll<RecordChangeOperationEnum>();
    }
}
