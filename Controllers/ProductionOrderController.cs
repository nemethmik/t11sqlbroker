using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using t11sqlbroker.Models;
namespace t11sqlbroker.Controllers
{   
    /// <summary>
    /// This is for getting access to Production Order via DI API
    /// </summary>
    public class ProductionOrderController : ApiController
    {
        static List<ProductionOrder> pos;
        public ProductionOrderController()
        {
            if(pos == null)
            {
                pos = new List<ProductionOrder>();
                pos.Add(new ProductionOrder { DocEntry = 1, DocNum = 1, DueDate = DateTime.Now, ItemCode = "PROD11", PlannedQty = 56.7m });
                pos.Add(new ProductionOrder { DocEntry = 2, DocNum = 2, DueDate = DateTime.Now.AddDays(1), ItemCode = "PROD12", PlannedQty = 200m });
            }
        }
        /// <summary>
        /// Returns all the Production Orders for a product itemcode
        /// </summary>
        /// <param name="name">Item Code</param>
        /// <returns>Returns a list of production orders</returns>
        [Route("api/GetForProductName/{name}")]
        public IEnumerable<ProductionOrder> GetForProductName(string name)
        {
            return pos.Where(po=>po.ItemCode == name).ToList();
        }
        // GET: api/ProductionOrder
        public IEnumerable<ProductionOrder> Get()
        {
            return pos;
        }

        // GET: api/ProductionOrder/5
        public ProductionOrder Get(int id)
        {
            return pos.Where(po => po.DocEntry == id).FirstOrDefault();
        }

        // POST: api/ProductionOrder
        public void Post([FromBody]ProductionOrder po)
        {
            pos.Add(po);
        }

        // PUT: api/ProductionOrder/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ProductionOrder/5
        public void Delete(int id)
        {
        }
    }
}
