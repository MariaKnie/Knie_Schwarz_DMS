using AutoMapper;
using NUnit.Framework;
using ASP_Rest_API.DTO;
using ASP_Rest_API.Mappings;
using MyDocDAL.Entities;

namespace Tests
{
    [TestFixture]
    public class MappingProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = config.CreateMapper();
        }

        [Test]
        public void Should_Map_MyDoc_To_MyDocDTO()
        {
            // Arrange
            var myDoc = new MyDoc
            {
                id = 1,
                author = "John Doe",
                title = "Sample Title",
                textfield = "Sample text",
                createddate = DateTime.Now,
                editeddate = DateTime.Now
            };

            // Act
            var myDocDTO = _mapper.Map<MyDocDTO>(myDoc);

            // Assert
            Assert.AreEqual(myDoc.id, myDocDTO.id);
            Assert.AreEqual(myDoc.author, myDocDTO.author);
            Assert.AreEqual(myDoc.title, myDocDTO.title);
            Assert.AreEqual(myDoc.textfield, myDocDTO.textfield);
            Assert.AreEqual(myDoc.createddate, myDocDTO.createddate);
            Assert.AreEqual(myDoc.editeddate, myDocDTO.editeddate);
        }

        [Test]
        public void Should_Map_MyDocDTO_To_MyDoc()
        {
            // Arrange
            var myDocDTO = new MyDocDTO
            {
                id = 1,
                author = "Jane Doe",
                title = "Another Title",
                textfield = "Some other text",
                createddate = DateTime.Now,
                editeddate = DateTime.Now
            };

            // Act
            var myDoc = _mapper.Map<MyDoc>(myDocDTO);

            // Assert
            Assert.AreEqual(myDocDTO.id, myDoc.id);
            Assert.AreEqual(myDocDTO.author, myDoc.author);
            Assert.AreEqual(myDocDTO.title, myDoc.title);
            Assert.AreEqual(myDocDTO.textfield, myDoc.textfield);
            Assert.AreEqual(myDocDTO.createddate, myDoc.createddate);
            Assert.AreEqual(myDocDTO.editeddate, myDoc.editeddate);
        }
    }
}
