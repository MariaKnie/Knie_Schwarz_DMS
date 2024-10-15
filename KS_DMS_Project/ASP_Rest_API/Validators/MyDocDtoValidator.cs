using FluentValidation;
using ASP_Rest_API.DTO;

namespace ASP_Rest_API.Validators
{
    public class MyDocDtoValidator : AbstractValidator<MyDocDTO>
    {
        public MyDocDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is empty")
                .MaximumLength(100).WithMessage("Title cannot be longer than 100 chars");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Author is empty")
                .MaximumLength(100).WithMessage("Author cannot be longer than 100 chars");

            RuleFor(x => x.TextField)
                .NotEmpty().WithMessage("TextField is empty")
                .MaximumLength(10000).WithMessage("TextField cannot be longer than 10000 chars");

        }
    }
}
