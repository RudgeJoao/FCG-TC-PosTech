# FIAP Cloud Games - Fase 1

API REST desenvolvida em .NET 8 para o Tech Challenge da FIAP Cloud Games.

O projeto implementa cadastro de usuarios, autenticacao JWT, controle de acesso por perfil, cadastro de jogos, biblioteca de jogos adquiridos, persistencia com Entity Framework Core e documentacao dos endpoints com Swagger.

## Funcionalidades

- Cadastro de usuario com nome, e-mail e senha.
- Validacao de formato de e-mail.
- Validacao de senha forte com minimo de 8 caracteres, letras, numeros e caractere especial.
- Autenticacao via token JWT.
- Autorizacao por perfil: `Usuario` e `Administrador`.
- Listagem de jogos para usuarios autenticados.
- Cadastro de jogos restrito a administradores.
- Listagem de usuarios restrita a administradores.
- Biblioteca de jogos por usuario.
- Middleware para tratamento de erros.
- Logs com `ILogger`.
- Swagger para documentacao e teste dos endpoints.
- Testes unitarios com xUnit.
- Migration inicial do banco de dados.

## Arquitetura

O projeto foi desenvolvido como monolito, conforme solicitado para o MVP da fase 1.

Estrutura principal:

```text
src/FiapCloudGames.Api
  Data
  Domain
  Services
  Program.cs

tests/FiapCloudGames.Tests
```

Organizacao:

- `Domain`: entidades e enums do dominio, como `Usuario`, `Jogo` e `PerfilUsuario`.
- `Services`: regras de negocio e servicos da aplicacao.
- `Data`: contexto do Entity Framework Core e migrations.
- `Program.cs`: configuracao da API, rotas Minimal API, Swagger, JWT, middleware e injecao de dependencias.
- `tests`: testes unitarios com xUnit.

## Banco de Dados

A persistencia foi implementada com Entity Framework Core e SQLite.

Configuracao padrao:

```text
ConnectionStrings:DefaultConnection = Data Source=fcg.db
```

A migration inicial esta em:

```text
src/FiapCloudGames.Api/Data/Migrations
```

Ao iniciar a API, as migrations pendentes sao aplicadas automaticamente e o banco `fcg.db` e criado caso ainda nao exista.

## Autenticacao JWT

A API utiliza autenticacao JWT com `Microsoft.AspNetCore.Authentication.JwtBearer`.

Fluxo de autenticacao:

1. O usuario envia e-mail e senha em `POST /login`.
2. A API valida as credenciais.
3. A API gera um token JWT contendo o identificador, e-mail e perfil do usuario.
4. O token deve ser enviado nas rotas protegidas pelo header `Authorization`.

Formato do header:

```text
Authorization: Bearer {token}
```

No Swagger, clique em `Authorize` e cole apenas o token retornado no login.

Perfis:

- `Usuario`: pode acessar a plataforma, listar jogos e gerenciar sua biblioteca.
- `Administrador`: pode cadastrar jogos e administrar usuarios.

Usuario administrador inicial:

```text
email: admin@fiap.com.br
senha: Admin@123
```

## Como Executar

Restaurar pacotes:

```powershell
dotnet restore FiapCloudGames.sln --configfile NuGet.Config
```

Executar a API:

```powershell
dotnet run --project src\FiapCloudGames.Api
```

Acessar o Swagger:

```text
http://localhost:5170/swagger
```

## Como Rodar os Testes

```powershell
dotnet test tests\FiapCloudGames.Tests
```

## TDD

Foi aplicado TDD no modulo de validacao de usuario. As regras de e-mail invalido e senha fraca foram definidas como testes unitarios antes da validacao final da regra de negocio.

Testes implementados:

- E-mail invalido deve gerar erro.
- Senha fraca deve gerar erro.
- E-mail do usuario deve ser normalizado para minusculo.
- Token JWT deve conter o identificador do usuario.
- Biblioteca nao deve duplicar o mesmo jogo.

## Endpoints

| Metodo | Rota | Acesso | Descricao |
| --- | --- | --- | --- |
| POST | `/usuarios` | Publico | Cadastra usuario comum |
| POST | `/login` | Publico | Autentica usuario e retorna token JWT |
| GET | `/usuarios` | Administrador | Lista usuarios |
| GET | `/jogos` | Usuario autenticado | Lista jogos cadastrados |
| POST | `/jogos` | Administrador | Cadastra jogo |
| GET | `/usuarios/{id}/biblioteca` | Usuario dono ou administrador | Lista biblioteca do usuario |
| POST | `/usuarios/{usuarioId}/jogos/{jogoId}` | Usuario dono ou administrador | Adiciona jogo na biblioteca |

## DDD

O dominio foi organizado com entidades e regras de negocio separadas da configuracao da API.

Entidades principais:

- `Usuario`
- `Jogo`
- `PerfilUsuario`

Regras principais:

- Usuario precisa ter nome, e-mail e senha.
- E-mail precisa ter formato valido.
- Senha precisa ser forte.
- Usuario comum nao pode administrar usuarios.
- Usuario comum nao pode cadastrar jogos.
- Biblioteca nao deve duplicar jogos.

A documentacao DDD com Event Storming dos fluxos de criacao de usuarios e criacao de jogos esta em:

```text
docs/ddd-event-storming.md
```
