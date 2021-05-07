using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Domain.Security
{
    public struct AccessRightSet
    {
        public AccessRightSet(Boolean canRead = false, Boolean canWrite = false, Boolean canDelete = false, Boolean canReadSensitive = false
            , Boolean canWriteSensitive = false, Boolean canWriteBusiness = false, Boolean canWriteTechnical = false, Boolean canWriteOther = false)
        {
            this.Flags = 0;
            this.CanRead = canRead;
            this.CanWrite = canWrite;
            this.CanDelete = canDelete;
            this.CanReadSensitive = canReadSensitive;
            this.CanWriteSensitive = canWriteSensitive;
            this.CanWriteBusiness = canWriteBusiness;
            this.CanWriteTechnical = canWriteTechnical;
            this.CanWriteOther = canWriteOther;
        }

        public AccessRightSet(int flags)
        {
            this.Flags = flags;
        }

        /// <summary>
        /// Can read non-sensitive template data
        /// </summary>
        public Boolean CanRead { get => (this.Flags & 2) != 0; private set => this.Flags = (value) ? this.Flags | 2 : this.Flags & ~2; }
        /// <summary>
        /// Can change non-sensitive template data
        /// </summary>
        public Boolean CanWrite { get => (this.Flags & 4) != 0; private set => this.Flags = (value) ? this.Flags | 4 : this.Flags & ~4; }
        /// <summary>
        /// Can delete template data
        /// </summary>
        public Boolean CanDelete { get => (this.Flags & 16) != 0; private set => this.Flags = (value) ? this.Flags | 16 : this.Flags & ~16; }
        /// <summary>
        /// Can read sensitive template data
        /// </summary>
        public Boolean CanReadSensitive { get => (this.Flags & 32) != 0; private set => this.Flags = (value) ? this.Flags | 32 : this.Flags & ~32; }
        /// <summary>
        /// Can change sensitive template data
        /// </summary>
        public Boolean CanWriteSensitive { get => (this.Flags & 64) != 0; private set => this.Flags = (value) ? this.Flags | 64 : this.Flags & ~64; }
        /// <summary>
        /// Can change business template data
        /// </summary>
        public Boolean CanWriteBusiness { get => (this.Flags & 128) != 0; private set => this.Flags = (value) ? this.Flags | 128 : this.Flags & ~128; }
        /// <summary>
        /// Can change technical template data
        /// </summary>
        public Boolean CanWriteTechnical { get => (this.Flags & 256) != 0; private set => this.Flags = (value) ? this.Flags | 256 : this.Flags & ~256; }
        /// <summary>
        /// Can change other template data
        /// </summary>
        public Boolean CanWriteOther { get => (this.Flags & 512) != 0; private set => this.Flags = (value) ? this.Flags | 512 : this.Flags & ~512; }

        /// <summary>
        /// Get bit encoded represenatation of access rights
        /// </summary>
        public int Flags { get; private set; }

    }
}
