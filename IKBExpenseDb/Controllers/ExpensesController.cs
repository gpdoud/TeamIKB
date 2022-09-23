using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IKBExpenseDb.Models;

namespace IKBExpenseDb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {

        public static string NEW = "NEW";
        public static string MODIFIED = "MODIFIED";
        public static string APPROVED = "APPROVED";
        public static string REJECTED = "REJECTED";
        public static string REVIEW = "REVIEW";
        public static string PAID = "PAID";

        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses()
        {
            return await _context.Expenses.ToListAsync();
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);

            if (expense == null)
            {
                return NotFound();
            }

            return expense;
        }

        // PUT: api/Expenses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpense(int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            _context.Entry(expense).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        // Put: api/expenses/pay/5
        [HttpPut("pay/{expenseId}")]
        public async Task<IActionResult> PayExpense(int expenseId)
        {
            var exp = await _context.Expenses.FindAsync(expenseId);
            if (exp is null)
            {
                return NotFound();
            }
            if (exp.Status != APPROVED)
            {
                return BadRequest();
            }
            exp.Status = PAID;
            var empl = await _context.Employees.FindAsync(exp.EmployeeId);
            if (empl is null)
            {
                throw new Exception("Corrupted FK in expense");
            }
            empl.ExpensesDue -= exp.Total;
            empl.ExpensesPaid += exp.Total;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // api/expenses/review/5
        [HttpPut("review/{id}")]
        public async Task<IActionResult> ReviewExpense(int id, Expense expense)
        {
            expense.Status = (expense.Total <= 75) ? APPROVED : REVIEW;
            return await PutExpense(id, expense);
        }

        // api/expenses/approve/5
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> ApproveExpense(int id, Expense expense)
        {
            expense.Status = APPROVED;
            return await PutExpense(id, expense);
        }

        // api/expenses/reject/5
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> RejectExpense(int id, Expense expense)
        {
            expense.Status = REJECTED;
            return await PutExpense(id, expense);
        }

            // POST: api/Expenses
            // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
            [HttpPost]
        public async Task<ActionResult<Expense>> PostExpense(Expense expense)
        {
            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpense", new { id = expense.Id }, expense);
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
            {
                return NotFound();
            }

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<IActionResult> UpdateEmployeeExpensesDueAndPaid(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if(employee is null)
            {
                return NotFound();
            }

            employee.ExpensesDue = (from l in _context.Expenselines
                                    join e in _context.Expenses
                                    on l.ExpenseId equals e.Id
                                    join i in _context.Items
                                    on l.ItemId equals i.Id
                                    where employee.Id == employeeId && e.Status == "APPROVED"
                                    select new
                                    {
                                        Subtotal = l.Quantity * i.Price
                                    }).Sum(x => x.Subtotal);
            
            employee.ExpensesPaid = (from l in _context.Expenselines
                                    join e in _context.Expenses
                                    on l.ExpenseId equals e.Id
                                    join i in _context.Items
                                    on l.ItemId equals i.Id
                                    where employee.Id == employeeId && e.Status == "PAID"
                                    select new
                                    {
                                        Subtotal = l.Quantity * i.Price
                                    }).Sum(x => x.Subtotal);


            return Ok();
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}
