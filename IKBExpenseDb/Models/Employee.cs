using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace IKBExpenseDb.Models
{

    [Index("Email", IsUnique = true, Name = "UID_Email")]
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(30)]
        public string Password { get; set; }

        public byte[] Admin { get; set; }

        [Column(TypeName = "decimal(11,2)")]
        public decimal ExpensesDue { get; set; }

        [Column(TypeName = "decimal(11,2)")]
        public decimal ExpensesPaid { get; set; }




    }
}