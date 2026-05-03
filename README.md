# FIAP Cloud Games - Fase 1

Projeto de estudo para a primeira fase do Tech Challenge.

Ele tem uma API REST em .NET 8 para cadastro de usuarios, login com token, cadastro de jogos e biblioteca de jogos comprados. Eu deixei sem MongoDB porque no enunciado ele aparece como opcional.

## O que tem no projeto

- Cadastro de usuario com nome, e-mail e senha.
- Validacao de e-mail.
- Validacao de senha forte: minimo 8 caracteres, letra, numero e caractere especial.
- Login retornando token no formato parecido com JWT.
- Perfil de Usuario e Administrador.
- Rotas protegidas por token.
- Middleware para erros.
- Logs pelo `ILogger` do ASP.NET.
- Uma pagina simples em `/swagger` e JSON em `/swagger/v1/swagger.json`.
- Testes simples em console para regras de negocio.

## O que ainda precisa ser feito por voce

Este projeto e uma base de estudo, nao e uma entrega pronta para postar sem entender.

- Trocar o armazenamento em memoria por Entity Framework Core.
- Criar migrations do banco.
- Se o professor exigir Swagger visual completo, instalar o pacote do Swagger/Swashbuckle.
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

## Como rodar os testes

```powershell
dotnet run --project tests\FiapCloudGames.Tests
```

Se aparecer `OK` em todos os testes, as regras principais passaram.

## Explicacao bem simples

Pense na API como uma atendente.

Voce manda pedidos para ela, por exemplo: "crie um usuario" ou "cadastre um jogo". Cada pedido vai para uma rota.

- `Program.cs`: e a entrada da API. Aqui ficam as rotas.
- `Domain`: sao as coisas importantes do sistema, como Usuario e Jogo.
- `Services`: sao as regras. Exemplo: senha precisa ser forte.
- `Tests`: e uma forma de testar as regras sem clicar em tudo manualmente.

O token funciona como uma pulseira de entrada. Primeiro voce faz login. A API te devolve uma pulseira. Depois, para entrar em rotas protegidas, voce mostra essa pulseira no campo `Authorization`.

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

O token foi feito manualmente para o projeto conseguir rodar sem instalar pacotes externos. Em um projeto real, o certo seria usar `Microsoft.AspNetCore.Authentication.JwtBearer`.
