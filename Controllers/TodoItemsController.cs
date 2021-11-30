using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApiDTO.Core.Models;
using TodoApiDTO.Infrastructure;
using TodoApiDTO.Infrastructure.Data;
using TodoApiDTO.Core.DTOModels;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly ILogger<TodoItemsController> logger;

        public TodoItemsController(TodoContext context, ILogger<TodoItemsController> _logger)
        {
            _context = context;
            logger = _logger;
        }

        

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDTO>>> GetTodoItems()
        {
            var result = await _context.TodoItems
                .Select(x => ItemToDTO(x))
                .ToListAsync();
            if (result != null)
            {
                logger.LogInformation($"Returned null array of Items on path {HttpContext.Request.Path}");
            }
            else
            {
                logger.LogInformation($"Returned {result.Count} Item(s) on path {HttpContext.Request.Path}");
            }
            return result;


        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDTO>> GetTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                logger.LogInformation($"Path {HttpContext.Request.Path}: No item found for id: {id}.");
                return NotFound();
            }

            logger.LogInformation($"Item found for id: {id}.");
            return ItemToDTO(todoItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoItem(long id, TodoItemDTO todoItemDTO)
        {
            if (id != todoItemDTO.Id)
            {
                logger.LogInformation($"{nameof(UpdateTodoItem)} action: Attempt to modify item failed. ItemID ({todoItemDTO.Id}) does not matches path Id parameter: {id}.");
                return BadRequest();
            }

            var todoItem = await _context.TodoItems.FindAsync(id);
            if (todoItem == null)
            {
                logger.LogInformation($"{nameof(UpdateTodoItem)} action: Attempt to modify item failed. ItemID ({todoItemDTO.Id}) has no matches any id in the database: {id}.");
                return NotFound();
            }

            todoItem.Name = todoItemDTO.Name;
            todoItem.IsComplete = todoItemDTO.IsComplete;

            try
            {
                await _context.SaveChangesAsync();
                logger.LogInformation($"{nameof(UpdateTodoItem)} action: Item with {id} has been successfully updated.");
            }
            catch (DbUpdateConcurrencyException e) when (!TodoItemExists(id))
            {
                logger.LogInformation($"{nameof(UpdateTodoItem)} catch block on db entry update: Item with {id}. - \n{e.Message}");
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<TodoItemDTO>> CreateTodoItem(TodoItemDTO todoItemDTO)
        {
            var todoItem = new TodoItem
            {
                IsComplete = todoItemDTO.IsComplete,
                Name = todoItemDTO.Name
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            logger.LogInformation($"{nameof(CreateTodoItem)} action: New item added to DB: Name: {todoItem.Name}, Completed: {todoItem.IsComplete}");

            return CreatedAtAction(
                nameof(GetTodoItem),
                new { id = todoItem.Id },
                ItemToDTO(todoItem));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var todoItem = await _context.TodoItems.FindAsync(id);

            if (todoItem == null)
            {
                logger.LogInformation($"{nameof(DeleteTodoItem)} action: Attempt to delete item failed. ItemID ({id}) has no matches in the database.");
                return NotFound();
            }

            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
            logger.LogInformation($"{nameof(DeleteTodoItem)} action: New item deleted from DB: Name: {todoItem.Name}, Completed: {todoItem.IsComplete}");

            return NoContent();
        }

        private bool TodoItemExists(long id) =>
             _context.TodoItems.Any(e => e.Id == id);

        private static TodoItemDTO ItemToDTO(TodoItem todoItem) =>
            new TodoItemDTO
            {
                Id = todoItem.Id,
                Name = todoItem.Name,
                IsComplete = todoItem.IsComplete
            };       
    }
}
