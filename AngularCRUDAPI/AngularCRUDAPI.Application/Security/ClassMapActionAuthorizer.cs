using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Application.Interfaces;
using AngularCrudApi.Application.Pipeline.Commands;
using AngularCrudApi.Application.Pipeline.Queries;
using AngularCrudApi.Domain.Enums;
using AngularCrudApi.Domain.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AngularCrudApi.Application.Security
{
    public class ClassMapActionAuthorizer : IActionAuthorizer
    {
        private readonly Dictionary<Type, Func<IAction, SystemModeEnum, Boolean>> allowedConditions = new Dictionary<Type, Func<IAction, SystemModeEnum, bool>>();
        private readonly SystemModeEnum currentSystemMode;

        public ClassMapActionAuthorizer(IOptions<GlobalSettings> globalSettings)
        {
            if (globalSettings is null)
            {
                throw new ArgumentNullException(nameof(globalSettings));
            }

            this.currentSystemMode = globalSettings.Value.Environment.Contains("development", StringComparison.InvariantCultureIgnoreCase) ? SystemModeEnum.RW : SystemModeEnum.RO;

            this.InitAllowedConditions();
        }

        private void InitAllowedConditions()
        {
            this.AddCommands();
            this.AddQueries();
        }

        private void AddCondition<T>(Func<IAction, SystemModeEnum, bool> condition) => this.allowedConditions.Add(typeof(T), condition);

        private void AddQueries()
        {
            this.AddCondition<CodebookAllQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<CodebookByNameQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<CodebookDataQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<LockStateQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<ReleaseAllQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<ReleaseByIdQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<RequestByIdQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<RequestByReleaseQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<UserByTokenQuery>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
        }

        private void AddCommands()
        {
            this.AddCondition<CodebookApplyChangesCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<CreateLockCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<ReleaseCreateCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<ReleaseDeleteCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<ReleaseLockCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<ReleaseUpdateCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<RequestCreateCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<RequestDeleteCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<RequestUpdateCommand>((action, systemMode) => this.IsRwMode(systemMode) && this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<UserLoginCommand>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<UserLoginRefreshCommand>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<UserLogoutCommand>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.GetAll()));
            this.AddCondition<ImportPackageCommand>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
            this.AddCondition<ExportPackageCommand>((action, systemMode) => this.IsUserAllowed(action.User, RoleEnum.Editor, RoleEnum.SysAdmin));
        }

        private bool AllowRwModeAndRoles(SystemModeEnum systemMode, ClaimsPrincipal user, params RoleEnum[] allowedRoles) => this.IsRwMode(systemMode) && this.IsUserAllowed(user, allowedRoles);
        private bool AllowRwModeAndRoles(SystemModeEnum systemMode, ClaimsPrincipal user, IEnumerable<RoleEnum> allowedRoles) => this.AllowRwModeAndRoles(systemMode, user, allowedRoles.ToArray());

        private bool IsUserAllowed(ClaimsPrincipal user, IEnumerable<RoleEnum> allowedRoles) => this.IsUserAllowed(user, allowedRoles.ToArray());
        private bool IsUserAllowed(ClaimsPrincipal user, params RoleEnum[] allowedRoles)
        {
            if (user == null)
            {
                return true;
            }

            return allowedRoles.Any(r => user.IsInRole(r.Name));
        }

        private Boolean IsRwMode(SystemModeEnum systemMode) => systemMode.Equals(SystemModeEnum.RW);
        private Boolean IsRoMode(SystemModeEnum systemMode) => systemMode.Equals(SystemModeEnum.RO);

        public Task Authorize(IAction action)
        {
            Type actionType = action.GetType();
            if (!this.allowedConditions.ContainsKey(actionType))
            {
                throw new ForbiddenException($"Action {actionType.Name} is not allowed for user {action.User.Identity.Name}.");
            }

            Func<IAction, SystemModeEnum, Boolean> condition = this.allowedConditions[actionType];

            if (!condition(action, this.currentSystemMode))
            {
                throw new ForbiddenException($"Action {actionType.Name} is not allowed for user {action.User.Identity.Name}.");
            }

            return Task.CompletedTask;
        }

        public Task<bool> CanAuthorize(IAction action) => this.CanAuthorize(action.GetType());

        public Task<bool> CanAuthorize(Type actionType) => Task.FromResult(this.allowedConditions.ContainsKey(actionType));
    }
}
