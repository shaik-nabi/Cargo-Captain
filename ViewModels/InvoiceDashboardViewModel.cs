using System.Collections.Generic;
using CargoCaptain.Models;

namespace CargoCaptain.ViewModels
{
    public class InvoiceDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal OutstandingRevenue { get; set; }
        public decimal PaidRevenue { get; set; }
        
        public int PaidInvoicesCount { get; set; }
        public int UnpaidInvoicesCount { get; set; }

        public List<FreightInvoice> RecentInvoices { get; set; } = new List<FreightInvoice>();
    }
}
