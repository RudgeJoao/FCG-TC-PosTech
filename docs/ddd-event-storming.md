# Documentacao DDD - rascunho para passar para o Miro

Este arquivo e um guia para voce montar a documentacao pedida no enunciado.

## Contexto

A FIAP Cloud Games vende jogos digitais educativos e guarda a biblioteca de jogos de cada usuario.

## Linguagem ubiqua

- Usuario: pessoa que entra na plataforma.
- Administrador: pessoa que cadastra jogos e administra usuarios.
- Jogo: produto digital vendido na plataforma.
- Biblioteca: lista de jogos adquiridos por um usuario.
- Token: comprovante de login usado para acessar rotas protegidas.

## Event Storming - Criacao de usuarios

1. Comando: cadastrar usuario.
2. Regra: nome, e-mail e senha sao obrigatorios.
3. Regra: e-mail precisa ter formato valido.
4. Regra: senha precisa ser forte.
5. Evento: usuario cadastrado.
6. Leitura: usuario pode fazer login.
7. Evento: token gerado.
8. Leitura: usuario acessa biblioteca.

## Event Storming - Criacao de jogos

1. Comando: administrador faz login.
2. Regra: apenas administrador cadastra jogos.
3. Comando: cadastrar jogo.
4. Regra: jogo precisa ter nome e preco valido.
5. Evento: jogo cadastrado.
6. Leitura: usuarios logados conseguem listar jogos.
7. Comando: adicionar jogo na biblioteca.
8. Evento: jogo incluido na biblioteca do usuario.

## Entidades

### Usuario

- Id
- Nome
- Email
- SenhaHash
- Perfil
- Biblioteca

### Jogo

- Id
- Nome
- Descricao
- Preco

## Agregados

- Usuario e sua Biblioteca.
- Jogo como catalogo simples.

## Regras principais

- Nao pode cadastrar e-mail invalido.
- Nao pode cadastrar senha fraca.
- Nao pode cadastrar dois usuarios com mesmo e-mail.
- Usuario comum nao pode listar todos os usuarios.
- Usuario comum nao pode criar jogo.
- Biblioteca nao deve repetir o mesmo jogo.
