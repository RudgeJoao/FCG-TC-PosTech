# FIAP Cloud Games - Fase 1

Projeto de estudo para a primeira fase do Tech Challenge.

Ele tem uma API REST em .NET 8 para cadastro de usuarios, login com token, cadastro de jogos e biblioteca de jogos comprados. A persistencia de dados foi feita com Entity Framework Core e SQLite.

## O que tem no projeto

- Cadastro de usuario com nome, e-mail e senha.
- Validacao de e-mail.
- Validacao de senha forte: minimo 8 caracteres, letra, numero e caractere especial.
- Login retornando token JWT.
- Perfil de Usuario e Administrador.
- Rotas protegidas com `Microsoft.AspNetCore.Authentication.JwtBearer`.
- Persistencia de usuarios, jogos e biblioteca com Entity Framework Core.
- Migration inicial para criacao do banco de dados.
- Middleware para erros.
- Logs pelo `ILogger` do ASP.NET.
- Swagger padrao em `/swagger`, com botao `Authorize` para testar rotas com token.
- Testes unitarios com xUnit para regras de negocio.
- TDD aplicado de forma simples no modulo de validacao de usuario.

## O que ainda precisa ser feito por voce

Este projeto e uma base de estudo, nao e uma entrega pronta para postar sem entender.

- Completar a documentacao DDD no Miro ou em uma imagem.
- Gravar o video mostrando as rotas funcionando.
- Revisar nomes, exemplos e texto para ficar com a cara do seu grupo.

## Como rodar

Abra o terminal na pasta do projeto e rode:

```powershell
dotnet run --project src\FiapCloudGames.Api
```

Depois acesse:

```text
http://localhost:5170/swagger
```

Admin inicial:

```text
email: admin@fiap.com.br
senha: Admin@123
```

O banco usado por padrao e:

```text
SQLite
Arquivo: src\FiapCloudGames.Api\fcg.db
```

A API aplica a migration automaticamente quando inicia.

## Como rodar os testes

```powershell
dotnet test tests\FiapCloudGames.Tests
```

Se todos os testes ficarem verdes, as regras principais passaram.

## TDD aplicado

Foi aplicado TDD de forma simples no modulo de usuario. Primeiro foram definidas regras que precisavam passar nos testes, como e-mail invalido nao ser aceito e senha fraca gerar erro. Depois essas regras foram implementadas no `UsuarioService`.

## Explicacao bem simples

Pense na API como uma atendente.

Voce manda pedidos para ela, por exemplo: "crie um usuario" ou "cadastre um jogo". Cada pedido vai para uma rota.

- `Program.cs`: e a entrada da API. Aqui ficam as rotas.
- `Domain`: sao as coisas importantes do sistema, como Usuario e Jogo.
- `Services`: sao as regras. Exemplo: senha precisa ser forte.
- `Tests`: e uma forma de testar as regras sem clicar em tudo manualmente.

O token funciona como uma pulseira de entrada. Primeiro voce faz login. A API te devolve uma pulseira. Depois, para entrar em rotas protegidas, voce mostra essa pulseira no campo `Authorization`.

No Swagger, clique em `Authorize` e cole apenas o token gerado no login.

Em chamadas manuais pelo arquivo `.http`, use assim:

```text
Bearer cole_o_token_aqui
```

## Rotas principais

| Metodo | Rota | Para que serve |
| --- | --- | --- |
| POST | `/usuarios` | cria usuario comum |
| POST | `/login` | faz login |
| GET | `/usuarios` | lista usuarios, so admin |
| GET | `/jogos` | lista jogos, usuario logado |
| POST | `/jogos` | cria jogo, so admin |
| GET | `/usuarios/{id}/biblioteca` | mostra biblioteca do usuario |
| POST | `/usuarios/{usuarioId}/jogos/{jogoId}` | adiciona jogo na biblioteca |

## Observacao importante

A chave JWT esta no `appsettings.json` apenas para facilitar o estudo local. Em projeto real, essa chave deveria ficar em variavel de ambiente ou cofre de segredo.

Foi usado SQLite para facilitar rodar localmente sem instalar SQL Server. Mesmo assim, o gerenciamento do banco esta sendo feito pelo Entity Framework Core, com migrations.
