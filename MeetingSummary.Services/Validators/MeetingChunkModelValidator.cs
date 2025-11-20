using FluentValidation;
using MeetingSummary.Core.Models;

namespace MeetingSummary.Services.Validators
{
    public class MeetingChunkModelValidator : AbstractValidator<MeetingChunkModel>
    {
        public MeetingChunkModelValidator()
        {
            RuleFor(x => x.MeetingId)
                .NotEmpty()
                .WithMessage("MeetingId is required");

            RuleFor(x => x.ChunkId)
                .NotEmpty()
                .WithMessage("ChunkId is required");

            RuleFor(x => x.BlobUrl)
                .NotEmpty()
                .WithMessage("BlobUrl is required")
                .Must(BeAValidUrl)
                .WithMessage("BlobUrl must be a valid URL");

        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
