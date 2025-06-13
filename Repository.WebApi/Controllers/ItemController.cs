using Microsoft.AspNetCore.Mvc;
using Repository.WebApi.Data;

namespace Repository.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemController : ControllerBase
{
    private readonly IRepository<Item> repository;
    private readonly ILogger<ItemController> logger;
    
    public ItemController(IRepository<Item> repository, ILogger<ItemController> logger)
    {
        this.repository = repository;
        this.logger = logger;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Item>>> GetAllItems()
    {
        var items = await repository.GetAllAsync();
        return Ok(items);
    }

  
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Item>> GetItemById(int id)
    {
        var item = await repository.GetByIdAsync(id);
        if (item == null)
        {
            logger.LogWarning("Item with Id: {ItemId} not found.", id);
            return NotFound(); 
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Item>> CreateItem([FromBody] Item item)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Item model validation failed. Errors: {Errors}", ModelState);
            return BadRequest(ModelState);
        }
    
        try
        {
            await repository.AddAsync(item);
            logger.LogInformation("Item created successfully with Id: {ItemId}", item.Id);
            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating item.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating item.");
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] Item item)
    {
        if (id != item.Id)
        {
            return BadRequest("Item ID in route does not match item ID in body.");
        }

        if (!ModelState.IsValid)
        {
            logger.LogWarning("Item model validation failed for update. Errors: {Errors}", ModelState);
            return BadRequest(ModelState);
        }
    
        try
        {
            await repository.UpdateAsync(item);
            logger.LogInformation("Item with Id: {ItemId} updated successfully.", id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Item with Id: {ItemId} not found during update.", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating item with Id: {ItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating item.");
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(int id)
    {
        try
        {
            await repository.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            logger.LogWarning("Item with Id: {ItemId} not found during deletion.", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting item with Id: {ItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting item.");
        }
    }
}
