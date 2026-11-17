using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface IEmailTemplateRepository
{
    Task<rf_email_template?> GetActiveByKeyAsync(
        string templateKey,
        CancellationToken cancellationToken = default);
}
