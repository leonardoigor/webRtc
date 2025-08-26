using System;

namespace WebRtcServer.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um ID de usuário
    /// </summary>
    public record UserId
    {
        public string Value { get; }

        public UserId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("User ID cannot be null or empty", nameof(value));
            
            if (value.Length < 3)
                throw new ArgumentException("User ID must be at least 3 characters long", nameof(value));
            
            if (value.Length > 50)
                throw new ArgumentException("User ID cannot be longer than 50 characters", nameof(value));
            
            // Validar caracteres permitidos (alfanuméricos, underscore, hífen)
            if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9_-]+$"))
                throw new ArgumentException("User ID can only contain alphanumeric characters, underscores, and hyphens", nameof(value));

            Value = value;
        }

        public static implicit operator string(UserId userId) => userId.Value;
        public static implicit operator UserId(string value) => new(value);

        public override string ToString() => Value;
    }
}