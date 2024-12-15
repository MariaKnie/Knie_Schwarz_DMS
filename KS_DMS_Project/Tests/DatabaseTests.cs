using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using MyDocDAL.Data;
using MyDocDAL.Entities;
using MyDocDAL.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[TestFixture]
public class MyDocRepositoryTests
{
    private MyDocContext _context;
    private MyDocRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<MyDocContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new MyDocContext(options);
        _repository = new MyDocRepository(_context);
    }

    [Test]
    public async Task GetAllAsync_Should_Return_All_Items()
    {
        var myDocs = new List<MyDoc>
        {
            new MyDoc { id = 1, author = "Author 1", title = "Title 1", textfield = "Text 1" },
            new MyDoc { id = 2, author = "Author 2", title = "Title 2", textfield = "Text 2" }
        };

        await _context.MyDocItems.AddRangeAsync(myDocs);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.AreEqual(2, result.Count());
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_Item_When_Id_Exists()
    {
        var myDoc = new MyDoc { id = 1, author = "Author 1", title = "Title 1", textfield = "Text 1" };
        await _context.MyDocItems.AddAsync(myDoc);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(1);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.id);
        Assert.AreEqual("Author 1", result.author);
    }

    [Test]
    public async Task GetByIdAsync_Should_Return_Null_When_Id_Does_Not_Exist()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.IsNull(result);
    }

    [Test]
    public async Task AddAsync_Should_Add_New_Item()
    {
        var myDoc = new MyDoc { author = "New Author", title = "New Title", textfield = "New Text" };

        await _repository.AddAsync(myDoc);
        await _context.SaveChangesAsync();

        var addedItem = await _context.MyDocItems.FirstOrDefaultAsync(m => m.author == "New Author");

        Assert.IsNotNull(addedItem);
        Assert.AreEqual("New Author", addedItem.author);
    }

    [Test]
    public async Task UpdateAsync_Should_Update_Existing_Item()
    {
        var myDoc = new MyDoc { id = 1, author = "Old Author", title = "Old Title", textfield = "Old Text" };
        await _context.MyDocItems.AddAsync(myDoc);
        await _context.SaveChangesAsync();

        myDoc.author = "Updated Author";
        await _repository.UpdateAsync(myDoc);

        var updatedItem = await _context.MyDocItems.FirstOrDefaultAsync(m => m.id == 1);

        Assert.IsNotNull(updatedItem);
        Assert.AreEqual("Updated Author", updatedItem.author);
    }

    [Test]
    public async Task DeleteAsync_Should_Delete_Item()
    {
        var myDoc = new MyDoc { id = 1, author = "Author 1", title = "Title 1", textfield = "Text 1" };
        await _context.MyDocItems.AddAsync(myDoc);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(1);

        var deletedItem = await _context.MyDocItems.FirstOrDefaultAsync(m => m.id == 1);

        Assert.IsNull(deletedItem);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
