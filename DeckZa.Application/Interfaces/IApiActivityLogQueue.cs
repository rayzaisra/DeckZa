using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

public interface IApiActivityLogQueue
{
    bool TryEnqueue(ApiActivityLogEntry entry);
}
