using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBot
{
    public class EmailService
    {
        public async Task<bool> SendPurchaseEmailAsync(string stockSymbol, int quantity)
        {
            // Simulate asynchronous email sending
            await Task.Delay(500);
            return true; // Simulated email sending success
        }
    }
}
