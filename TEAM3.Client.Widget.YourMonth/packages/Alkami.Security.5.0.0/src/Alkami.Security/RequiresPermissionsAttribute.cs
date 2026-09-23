using System;
using System.Diagnostics.CodeAnalysis;

namespace Alkami.Security
{
    [ExcludeFromCodeCoverage]
    public class RequiresPermissionsAttribute : Attribute
    {
        public RequiresPermissionsAttribute(params Permission[] requiredPermissions)
        {
            Permissions = requiredPermissions;
        }

        public Permission[] Permissions { get; private set; }
    }
}