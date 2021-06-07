using System;

namespace AngularCrudApi.Domain.Entities
{
    public class LockState
    {
        public bool IsLocked { get; set; }
        public string ForUserId { get; set; }
        public string ForUserName { get; set; }
        public DateTime Created { get; set; }
        public int ForRequestId { get; set; }
    }
}
