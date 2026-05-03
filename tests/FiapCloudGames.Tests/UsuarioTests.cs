using FiapCloudGames.Api.Domain;
using FiapCloudGames.Api.Services;
using Xunit;

namespace FiapCloudGames.Tests;

public class UsuarioTests
{
    [Fact]
    public void ValidarEmail_DeveDarErro_QuandoEmailForInvalido()
    {
        Assert.Throws<ArgumentException>(() => UsuarioService.ValidarEmail("email-errado"));
    }

    [Fact]
    public void ValidarSenha_DeveDarErro_QuandoSenhaForFraca()
    {
        Assert.Throws<ArgumentException>(() => UsuarioService.ValidarSenha("123"));
    }

    [Fact]
    public void Usuario_DeveSalvarEmailEmMinusculo()
    {
        var usuario = new Usuario("Ana", "ANA@EMAIL.COM", "hash-de-teste", PerfilUsuario.Usuario);

        Assert.Equal("ana@email.com", usuario.Email);
    }

    [Fact]
    public void Biblioteca_NaoDeveDuplicarMesmoJogo()
    {
        var usuario = new Usuario("Carla", "carla@email.com", "hash-de-teste", PerfilUsuario.Usuario);
        var jogo = new Jogo("Banco de Dados", "Jogo sobre consultas SQL.", 10);

        usuario.AdicionarJogo(jogo);
        usuario.AdicionarJogo(jogo);

        Assert.Single(usuario.Biblioteca);
    }
}
