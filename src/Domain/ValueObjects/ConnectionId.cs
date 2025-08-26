using System;

namespace WebRtcServer.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um ID de conexão
    /// </summary>
    public record ConnectionId
    {
        public string Value { get; }

        public ConnectionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Connection ID cannot be null or empty", nameof(value));
            
            if (value.Length < 10)
                throw new ArgumentException("Connection ID must be at least 10 characters long", nameof(value));

            Value = value;
        }

        public static ConnectionId Generate()
        {
            return new ConnectionId(Guid.NewGuid().ToString());
        }

        public static implicit operator string(ConnectionId connectionId) => connectionId.Value;
        public static implicit operator ConnectionId(string value) => new(value);

        public override string ToString() => Value;
    }
}