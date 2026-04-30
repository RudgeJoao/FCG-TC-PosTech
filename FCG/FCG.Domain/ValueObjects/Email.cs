using FCG.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace FCG.Domain.ValueObjects;
public sealed class Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("E-mail não pode ser vazio.");

        if (!EmailRegex.IsMatch(value))
            throw new DomainException($"E-mail '{value}' não possui um formato válido.");

        Value = value.ToLowerInvariant().Trim();
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Email other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(Email email) => email.Value;
}