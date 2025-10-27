using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Infrastructure.MultiTenancy
{
    public static class SchemaNameHelper
    {
        public static string FromCode(string code)
        {
            // اسم سكيما صالح: حروف/أرقام/شرطة سفلية فقط
            var sb = new StringBuilder("prj_");
            foreach (var ch in code)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_') sb.Append(ch);
            }
            if (sb.Length == 4) sb.Append("PRJ"); // احتياط لو الرمز كله غير صالح
            return sb.ToString();
        }
    }
}
