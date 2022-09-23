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
    public class ExpenselinesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExpenselinesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Expenselines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expenseline>>> GetExpenselines()
        {
            return await _context.Expenselines.ToListAsync();
        }

        // GET: api/Expenselines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expenseline>> GetExpenseline(int id)
        {
            var expenseline = await _context.Expenselines.FindAsync(id);

            if (expenseline == null)
            {
                return NotFound();
            }

            return expenseline;
        }

        // PUT: api/Expenselines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpenseline(int id, Expenseline expenseline)
        {
            if (id != expenseline.Id)
            {
                return BadRequest();
            }

            _context.Entry(expenseline).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                await RecalculateExpenseTotal(expenseline.ExpenseId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenselineExists(id))
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

        // POST: api/Expenselines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Expenseline>> PostExpenseline(Expenseline expenseline)
        {
            _context.Expenselines.Add(expenseline);
            await _context.SaveChangesAsync();
            await RecalculateExpenseTotal(expenseline.ExpenseId);

            return CreatedAtAction("GetExpenseline", new { id = expenseline.Id }, expenseline);
        }

        // DELETE: api/Expenselines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpenseline(int id)
        {
            var expenseline = await _context.Expenselines.FindAsync(id);
            if (expenseline == null)
            {
                return NotFound();
            }

            _context.Expenselines.Remove(expenseline);
            await _context.SaveChangesAsync();
            await RecalculateExpenseTotal(expenseline.ExpenseId);
            return NoContent();
        }

        //RecalculateExpenseTotal is only called by Expenselines Methods that modify data.
        private async Task<IActionResult> RecalculateExpenseTotal(int expenseId)
        {
            var expense = await _context.Expenses.FindAsync(expenseId);
            if(expense is null)
            {
                return NotFound();
            }
            expense.Total = (from l in _context.Expenselines
                             join i in _context.Items
                             on l.ItemId equals i.Id
                             where i.Id == expenseId
                             select new
                             {
                                 Subtotal = l.Quantity * i.Price
                             }).Sum(x => x.Subtotal);

            await _context.SaveChangesAsync();
            return Ok();
        }

        private bool ExpenselineExists(int id)
        {
            return _context.Expenselines.Any(e => e.Id == id);
        }
    }
}
