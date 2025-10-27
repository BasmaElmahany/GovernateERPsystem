using Governate_ERP_System.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Governate_ERP_System.Application.Interfaces
{
    // تجهيز مشروع جديد: إنشاء قاعدة بيانات/تشغيل الهجرات/تجهيز دليل الحسابات
    public interface IProjectProvisioner
    {
        Task ProvisionAsync(Project project);
    }
}
