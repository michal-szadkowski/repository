using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Repository.WebApi;
using Repository.WebApi.Controllers;
using Repository.WebApi.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute.ExceptionExtensions;

namespace Repository.UnitTests;

[TestClass]
public class ItemControllerTests
{
    private IRepository<Item> repository;
    private ILogger<ItemController> logger;
    private ItemController controller;

    [TestInitialize]
    public void Setup()
    {
        repository = Substitute.For<IRepository<Item>>();
        logger = Substitute.For<ILogger<ItemController>>();
        controller = new ItemController(repository, logger);
    }

    [TestMethod]
    public async Task GetAllItems_ReturnsOkResult_WithItems()
    {
        // Arrange
        var expectedItems = new List<Item>
        {
            new Item { Id = 1, Name = "Item 1", Price = 10 },
            new Item { Id = 2, Name = "Item 2", Price = 10 }
        };
        repository.GetAllAsync().Returns(expectedItems);

        // Act
        var result = await controller.GetAllItems();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var items = okResult.Value as IEnumerable<Item>;
        Assert.IsNotNull(items);
        Assert.AreEqual(expectedItems.Count, items.Count());
    }

    [TestMethod]
    public async Task GetItemById_WithExistingItem_ReturnsOkResult()
    {
        // Arrange
        var expectedItem = new Item { Id = 1, Name = "Item", Price = 10 };
        repository.GetByIdAsync(1).Returns(expectedItem);

        // Act
        var result = await controller.GetItemById(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var item = okResult.Value as Item;
        Assert.IsNotNull(item);
        Assert.AreEqual(expectedItem.Id, item.Id);
    }

    [TestMethod]
    public async Task GetItemById_WithNonexistentItem_ReturnsNotFound()
    {
        // Arrange
        repository.GetByIdAsync(999).Returns((Item?)null);

        // Act
        var result = await controller.GetItemById(999);

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task CreateItem_WithValidItem_ReturnsCreatedAtAction()
    {
        // Arrange
        var newItem = new Item { Name = "Item", Price = 10 };
        repository.AddAsync(Arg.Any<Item>())
            .Returns(Task.CompletedTask)
            .AndDoes(x => ((Item)x[0]).Id = 1);

        // Act
        var result = await controller.CreateItem(newItem);

        // Assert
        var createdAtActionResult = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdAtActionResult);
        var item = createdAtActionResult.Value as Item;
        Assert.IsNotNull(item);
        Assert.AreEqual(1, item.Id);
        Assert.AreEqual("New Item", item.Name);
    }

    [TestMethod]
    public async Task UpdateItem_WithValidItem_ReturnsNoContent()
    {
        // Arrange
        var item = new Item { Id = 1, Name = "Item", Price = 10 };
        repository.UpdateAsync(Arg.Any<Item>()).Returns(Task.CompletedTask);

        // Act
        var result = await controller.UpdateItem(1, item);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task UpdateItem_WithNonexistentItem_ReturnsNotFound()
    {
        // Arrange
        var item = new Item { Id = 999, Name = "Item", Price = 10 };
        repository.UpdateAsync(Arg.Any<Item>())
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await controller.UpdateItem(999, item);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task DeleteItem_WithExistingItem_ReturnsNoContent()
    {
        // Arrange
        repository.DeleteAsync(1).Returns(Task.CompletedTask);

        // Act
        var result = await controller.DeleteItem(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task DeleteItem_WithNonexistentItem_ReturnsNotFound()
    {
        // Arrange
        repository.DeleteAsync(999)
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await controller.DeleteItem(999);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

}
