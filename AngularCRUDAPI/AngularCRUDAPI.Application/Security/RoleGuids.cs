﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Security
{
    /// <summary>
    /// Role object ids from AAD for different environments
    /// </summary>
    public class RoleGuids
    {
        public static RoleGuids Instance;

        public static void SetupInstance(string environment)
        {
            if (string.IsNullOrEmpty(environment))
            {
                throw new ArgumentNullException(nameof(environment));
            }

            if (environment.Equals("Default", StringComparison.InvariantCultureIgnoreCase))
            {
                Instance = DebugInstance;
            }
            else if (environment.Equals("DevelopmentAzure", StringComparison.InvariantCultureIgnoreCase))
            {
                Instance = AzDebugInstance;
            }
            else if (environment.Equals("Testing", StringComparison.InvariantCultureIgnoreCase))
            {
                Instance = TestingInstance;
            }
            else if (environment.Equals("Production", StringComparison.InvariantCultureIgnoreCase))
            {
                Instance = ReleaseInstance;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(environment), "Role guids are not defined for this environment");
            }
        }

        public string ReaderId { get; init; }
        public string EditorId { get; init; }
        public string AdminId { get; init; }

        private static RoleGuids DebugInstance = new RoleGuids()
        {
            ReaderId = "646103eb-c5f2-4dea-b7e0-acd744d707f5",
            EditorId = "e145de07-932c-47e1-baf7-1e764d0fc99e",
            AdminId = "d58d9f95-6c51-498a-bd6c-42b34e74822c",
        };

        private static RoleGuids AzDebugInstance = new RoleGuids()
        {
            ReaderId = "646103eb-c5f2-4dea-b7e0-acd744d707f5",
            EditorId = "e145de07-932c-47e1-baf7-1e764d0fc99e",
            AdminId = "d58d9f95-6c51-498a-bd6c-42b34e74822c",
        };

        private static RoleGuids TestingInstance = new RoleGuids()
        {
            ReaderId = "646103eb-c5f2-4dea-b7e0-acd744d707f5",
            EditorId = "e145de07-932c-47e1-baf7-1e764d0fc99e",
            AdminId = "d58d9f95-6c51-498a-bd6c-42b34e74822c",
        };

        private static RoleGuids ReleaseInstance = new RoleGuids()
        {
            ReaderId = "c27c0ea6-1206-45b0-bd09-3cef141633bc",
            EditorId = "928ff6b6-e4dc-4a10-bf25-ab30e4e6b546",
            AdminId = "46ca8336-368f-4da3-b5b4-438aca1fdd1c",
        };
    }
}
