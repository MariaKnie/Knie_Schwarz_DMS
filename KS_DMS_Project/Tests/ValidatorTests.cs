using FluentValidation.TestHelper;
using NUnit.Framework;
using ASP_Rest_API.DTO;
using ASP_Rest_API.Validators;

namespace Tests
{
    [TestFixture]
    public class MyDocDtoValidatorTests
    {
        private MyDocDtoValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new MyDocDtoValidator();
        }

        [Test]
        public void Should_Have_Error_When_Title_Is_Empty()
        {
            // Arrange
            var myDocDto = new MyDocDTO { title = string.Empty };

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.title);
            Assert.AreEqual("Title is empty", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void Should_Have_Error_When_Author_Is_Empty()
        {
            // Arrange
            var myDocDto = new MyDocDTO { title = "a", textfield = "a", author = string.Empty };

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.author);
            Assert.AreEqual("Author is empty", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void Should_Have_Error_When_TextField_Is_Empty()
        {
            // Arrange
            var myDocDto = new MyDocDTO { title = "a", author = "a", textfield = string.Empty };

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.textfield);
            Assert.AreEqual("TextField is empty", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void Should_Have_Error_When_Title_Exceeds_Max_Length()
        {
            // Arrange
            var myDocDto = new MyDocDTO { title = new string('a', 101) }; // 101 chars

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.title);
            Assert.AreEqual("Title cannot be longer than 100 chars", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void Should_Have_Error_When_Author_Exceeds_Max_Length()
        {
            // Arrange
            var myDocDto = new MyDocDTO {title = "a", textfield = "a", author = new string('a', 101) }; // 101 chars

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.author);
            Assert.AreEqual("Author cannot be longer than 100 chars", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void Should_Have_Error_When_TextField_Exceeds_Max_Length()
        {
            // Arrange
            var myDocDto = new MyDocDTO { title = "a", author = "a", textfield = new string('a', 10001) }; // 10001 chars

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.textfield);
            Assert.AreEqual("TextField cannot be longer than 10000 chars", result.Errors[0].ErrorMessage);
        }

        [Test]
        public void Should_Not_Have_Error_When_All_Properties_Are_Valid()
        {
            // Arrange
            var myDocDto = new MyDocDTO
            {
                title = "Valid Title",
                author = "Valid Author",
                textfield = "Valid text content"
            };

            // Act
            var result = _validator.TestValidate(myDocDto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
