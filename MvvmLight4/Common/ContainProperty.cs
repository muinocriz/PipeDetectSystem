using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MvvmLight4.Common
{
    public static class ContainProperty
    {
        public static bool IsContainProperty(Type instance, string propertyName)
        {
            if (instance != null && !string.IsNullOrEmpty(propertyName))
            {
                MemberInfo memberInfo = instance.GetProperty(propertyName);
                return (memberInfo.Name != null);
            }
            return false;
        }
    }
}
