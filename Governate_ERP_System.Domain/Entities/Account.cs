using Governate_ERP_System.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Domain.Entities
{
    public class Account : BaseEntity
    {
     
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public AccountType Type { get; set; }
        public NormalSide NormalSide { get; set; }
        public int? ParentAccountId { get; set; }
        public Account? ParentAccount { get; set; }
        public ICollection<Account> Children { get; set; } = new List<Account>();
        public bool IsActive { get; set; } = true;
        public int Level { get; set; } // لهيكلة الدليل
        public bool IsLeaf { get; set; } = true;
    }

}
