using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace t11sqlbroker.Models
{
    /// <summary>
    /// For more info check out SAP B1 DI API documentation
    /// </summary>
    public class ProductionOrder
    {
        public int DocNum;
        public int DocEntry;
        public string ItemCode;
        public DateTime DueDate;
        public decimal PlannedQty;
    }
}