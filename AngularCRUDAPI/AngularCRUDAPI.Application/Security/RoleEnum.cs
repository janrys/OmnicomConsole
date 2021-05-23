using AngularCrudApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Security
{
    public interface IRoleEnum : IEnumeration
    {
        IEnumerable<Guid> ExternalIds { get; }
        AccessRightSet AccessRights { get; }
    }

    public class RoleEnum : Enumeration, IRoleEnum
    {
        /// <summary>
        /// Any authenticated user
        /// </summary>
        public static readonly RoleEnum Guest = new RoleEnum(0, "guest", externalIds: Guid.Empty, canRead: true);
        /// <summary>
        /// Can read public data
        /// </summary>
        public static readonly RoleEnum Reader = new RoleEnum(1, "reader", externalIds: Guid.Parse(RoleGuids.Instance.ReaderId), canRead: true);
        /// <summary>
        /// Can edit any attributes in templates
        /// </summary>
        public static readonly RoleEnum Editor = new RoleEnum(2, "editor", externalIds: Guid.Parse(RoleGuids.Instance.EditorId), canRead: true, canWriteOther: true, canReadSensitive: true, canWriteBusiness: true, canWriteSensitive: true);
        /// <summary>
        /// Can do anything
        /// </summary>
        public static readonly RoleEnum SysAdmin = new RoleEnum(3, "sysadmin", externalIds: Guid.Parse(RoleGuids.Instance.AdminId), canRead: true, canWrite: true, canDelete: true
            , canWriteBusiness: true, canWriteTechnical: true, canWriteSensitive: true, canReadSensitive: true, canWriteOther: true);

        private RoleEnum(int value, string name, bool canRead = false, bool canWrite = false
            , bool canDelete = false, bool canWriteBusiness = false, bool canWriteTechnical = false
            , bool canWriteSensitive = false, bool canReadSensitive = false, bool canWriteOther = false, params Guid[] externalIds)
            : this(value, name, new AccessRightSet(canRead, canWrite, canDelete, canReadSensitive, canWriteSensitive
                , canWriteBusiness, canWriteTechnical, canWriteOther), externalIds)
        {
        }

        private RoleEnum(int value, string name, AccessRightSet accessRights, params Guid[] externalIds) : base(value, name)
        {
            this.ExternalIds = externalIds ?? new Guid[] { };
            this.AccessRights = accessRights;
        }

        public IEnumerable<Guid> ExternalIds { get; }

        public AccessRightSet AccessRights { get; }

        public static IEnumerable<RoleEnum> GetAll() => GetAll<RoleEnum>();
    }
}
