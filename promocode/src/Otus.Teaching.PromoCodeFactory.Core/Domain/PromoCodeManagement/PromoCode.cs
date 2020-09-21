using Otus.Teaching.PromoCodeFactory.Core.Domain.Administration;
using System;
using System.ComponentModel.DataAnnotations;

namespace Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement
{
    public class PromoCode : BaseEntity
    {
        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(100)]
        public string ServiceInfo { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        [MaxLength(100)]
        public string PartnerName { get; set; }

        public Guid EmployeeId { get; set; }

        public Employee PartnerManager { get; set; }

        public Guid PreferenceId { get; set; }

        public Preference Preference { get; set; }

        public Guid CustomerId { get; set; }
    }
}