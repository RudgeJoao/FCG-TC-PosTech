using FCG.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FCG.Domain.ValueObjects;
public sealed class Password
{
    public string Value { get; }

    // Mínimo 8 caracteres, ao menos 1 letra, 1 número e 1 caractere especial
    private static readonly Regex PasswordRegex = new
        (
        @"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&\#^()\-_=+\[\]{}|;:',.<>?/`~\\])\S{8,}$", RegexOptions.Compiled
        );
    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Senha não pode ser vazia.");

        if (value.Length < 8)
            throw new DomainException("Senha deve ter no mínimo 8 caracteres.");

        if (!PasswordRegex.IsMatch(value))
            throw new DomainException(
                "Senha deve conter ao menos uma letra, um número e um caractere especial.");

        Value = value;
    }

    public override string ToString() => "********";
    public override bool Equals(object? obj) => obj is Password other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();

    public static implicit operator string(Password password) => password.Value;
}

