using FluentValidation;

namespace plr.Entities
{
    internal class StationValidator : AbstractValidator<Station>
    {
        public StationValidator()
        {
            RuleFor(x => x.Uri).Must(x => x.Length > 0);
            RuleForEach(x => x.Uri).NotEmpty();
            RuleFor(x => x.Country).NotEmpty();
            RuleFor(x => x.Language).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Style).Must(x => x.Length > 0);
            RuleForEach(x => x.Style).NotEmpty();
        }
    }
}