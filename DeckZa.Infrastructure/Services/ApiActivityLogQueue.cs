using System.Threading.Channels;

using Microsoft.Extensions.Options;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.Interfaces;
using DeckZa.Infrastructure.Options;

namespace DeckZa.Infrastructure.Services;

public sealed class ApiActivityLogQueue : IApiActivityLogQueue
{
    private readonly Channel<ApiActivityLogEntry> _channel;

    public ApiActivityLogQueue(IOptions<ApiActivityLogOptions> options)
    {
        var capacity = Math.Clamp(options.Value.QueueCapacity, 100, 100_000);
        _channel = Channel.CreateBounded<ApiActivityLogEntry>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    public bool TryEnqueue(ApiActivityLogEntry entry)
        => _channel.Writer.TryWrite(entry);

    internal ChannelReader<ApiActivityLogEntry> Reader => _channel.Reader;
}
