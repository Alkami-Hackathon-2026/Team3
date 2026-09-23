using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Alkami.Security
{
    public static class PermissionSets
    {
        public static string GetDisplayName(this Permission enumValue)
        {
            return enumValue.GetType().GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .Name;
        }

        public static string GetDescription(this Permission enumValue)
        {
            return enumValue.GetType().GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DescriptionAttribute>()
                .Description;
        }

        public static string GetCategory(this Permission enumValue)
        {
            return enumValue.GetType().GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<CategoryAttribute>()
                .Category;
        }

        public static List<Permission> GetAccountPermissions()
        {
            return GetPermissionsByCategory("Account");
        }
        public static List<Permission> GetAdministratorPermissions()
        {
            return GetPermissionsByCategory("Administrator");
        }
        public static List<Permission> GetEntityPermissions()
        {
            return GetPermissionsByCategory("Entity");
        }
        public static List<Permission> GetEntityGroupPermissions()
        {
            return GetPermissionsByCategory("EntityGroup");
        }
        public static List<Permission> GetMasqueradePermissions()
        {
            return GetPermissionsByCategory("Masquerade");
        }
        public static List<Permission> GetUnknownPermissions()
        {
            return GetPermissionsByCategory("Unknown");
        }
        public static List<Permission> GetUserPermissions()
        {
            return GetPermissionsByCategory("User");
        }

        private static List<Permission> GetPermissionsByCategory(string category)
        {
            return
                Enum.GetValues(typeof(Permission))
                    .Cast<Permission>()
                    .Where(x => x.GetCategory() == category)
                    .ToList();
        }

    }
}