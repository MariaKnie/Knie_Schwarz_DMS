using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using MyDocDAL.Data;
using MyDocDAL.Entities;
using MyDocDAL.Repositories;
using System.Collections.Generic;
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

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
