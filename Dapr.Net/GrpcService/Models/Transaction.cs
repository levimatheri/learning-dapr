using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService.Models
{
    public class Transaction
    {
        [Required]
        public string Id { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}
