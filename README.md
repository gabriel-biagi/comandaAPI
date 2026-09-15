# ComandaAPI

API para processamento e estruturação de pedidos recebidos em mensagens de WhatsApp, utilizando inteligência artificial para transformar mensagens em dados organizados.

O projeto foi desenvolvido para uma situação real de uso em uma açaiteria, com o objetivo de reduzir a necessidade de interpretar e organizar manualmente as mensagens de pedidos.

## Sobre o projeto

Mensagens de pedidos recebidas pelo WhatsApp podem apresentar informações desorganizadas, abreviações e diferentes formas de descrever o mesmo pedido.

A ComandaAPI recebe o texto da mensagem, envia o conteúdo para a API da Groq com instruções específicas de processamento e retorna os dados estruturados em JSON.

### Fluxo

```text
Mensagem do WhatsApp
        ↓
Frontend
        ↓
ASP.NET Core Web API
        ↓
ComandaController
        ↓
Groq Cloud API
        ↓
JSON estruturado
        ↓
Frontend
```

## Funcionalidades

- Processamento de mensagens de pedidos em texto
- Extração de informações relevantes do pedido utilizando IA
- Estruturação da resposta em JSON
- Integração com a Groq Cloud API
- Autenticação de usuários
- Autorização utilizando JWT
- ASP.NET Core Identity para gerenciamento de usuários
- Autenticação utilizando cookies HttpOnly
- Tratamento global de exceções
- Documentação da API com Swagger/OpenAPI
- Interface web integrada à aplicação
- Validação do conteúdo processado
- Comunicação assíncrona com a API externa

## Tecnologias

### Backend

- C#
- .NET 8
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- JWT
- SQLite

### Integrações

- Groq Cloud API
- Modelo `openai/gpt-oss-120b`

### Frontend

- HTML5
- CSS3
- JavaScript

### Ferramentas

- Swagger/OpenAPI
- Git
- GitHub

## Arquitetura

O projeto utiliza uma estrutura baseada em ASP.NET Core, separando responsabilidades entre os principais componentes da aplicação.

```text
comandaAPI/
├── Controllers/
├── Models/
│   └── Identity/
├── Data/
├── Middlewares/
├── Migrations/
├── Services/
├── wwwroot/
├── Program.cs
└── appsettings.json
```

A aplicação utiliza:

- Controllers para exposição dos endpoints HTTP
- Services para responsabilidades específicas da aplicação
- Models para representação dos dados
- Entity Framework Core para persistência
- ASP.NET Core Identity para gerenciamento de usuários
- Middleware para tratamento global de exceções
- `wwwroot` para os arquivos da interface web

## Autenticação

A API possui autenticação utilizando **ASP.NET Core Identity e JWT**.

O fluxo de autenticação utiliza cookies `HttpOnly` para armazenar o token de acesso, evitando que o token seja diretamente acessível por JavaScript no navegador.

Endpoints relacionados à autenticação incluem operações de:

- Registro
- Login
- Logout
- Renovação de autenticação
- Gerenciamento de usuários

Os endpoints protegidos utilizam autorização baseada em `[Authorize]`.

## Processamento de pedidos

O endpoint responsável pelo processamento recebe o texto da mensagem e utiliza a Groq Cloud API para interpretar e estruturar as informações.

A comunicação com a API externa é realizada utilizando `HttpClient`.

A requisição enviada à Groq define instruções para que a resposta siga uma estrutura JSON específica.

O resultado é convertido para o modelo de resposta da aplicação antes de ser retornado pela API.

### Exemplo conceitual

Entrada:

```text
Mensagem contendo as informações do pedido enviadas pelo cliente.
```

Processamento:

```text
Texto recebido
    ↓
Prompt de processamento
    ↓
Groq Cloud API
    ↓
Resposta JSON
    ↓
Desserialização
    ↓
ComandaResponse
```

A resposta estruturada contém informações relacionadas ao pedido, como:

- Nome
- Pedido
- Acompanhamentos
- Valor
- Forma de pagamento
- Endereço

## Frontend

A aplicação possui uma interface web estática hospedada pelo próprio ASP.NET Core.

```text
wwwroot/
├── index.html
├── styles.css
└── app.js
```

O JavaScript realiza a comunicação com a API utilizando `fetch`.

A interface permite realizar o processamento da mensagem e apresentar o resultado estruturado retornado pela API.

## Banco de dados

O projeto utiliza **SQLite** junto ao Entity Framework Core.

O banco é utilizado principalmente para os recursos relacionados ao ASP.NET Core Identity.

As alterações do modelo são controladas através de migrations do Entity Framework Core.

## Tratamento de exceções

A aplicação possui middleware para tratamento global de exceções.

Exceções específicas da aplicação são utilizadas para representar diferentes situações, incluindo:

- Erros de negócio
- Recursos não encontrados
- Erros de validação

O middleware transforma essas exceções em respostas HTTP apropriadas para a API.

## Configuração

A chave utilizada para comunicação com a Groq Cloud API não deve ser armazenada diretamente no código-fonte.

Em ambiente de desenvolvimento, o projeto utiliza **ASP.NET Core User Secrets** para armazenar configurações sensíveis.

Exemplo:

```bash
dotnet user-secrets set "GroqApiKey" "SUA_CHAVE"
```

Depois de configurar a chave, a aplicação pode ser executada normalmente.

## Como executar

### Pré-requisitos

- .NET 8 SDK
- Uma chave da Groq Cloud API

### 1. Clone o repositório

```bash
git clone https://github.com/gabriel-biagi/comandaAPI.git
cd comandaAPI
```

### 2. Configure a chave da Groq

```bash
dotnet user-secrets set "GroqApiKey" "SUA_CHAVE"
```

### 3. Execute a aplicação

```bash
dotnet run
```

A aplicação iniciará utilizando as configurações definidas no projeto.

## Swagger

Durante o desenvolvimento, a API disponibiliza documentação interativa através do Swagger/OpenAPI.

Após iniciar a aplicação, a interface do Swagger pode ser acessada pela URL disponibilizada pelo ASP.NET Core.

## Objetivo do projeto

O projeto foi desenvolvido como uma aplicação prática para resolver um problema de processamento de pedidos e, ao mesmo tempo, aprofundar conhecimentos em:

- ASP.NET Core
- APIs REST
- Integração com APIs externas
- Autenticação e autorização
- ASP.NET Core Identity
- JWT
- Entity Framework Core
- SQLite
- Middleware
- JavaScript
- Integração entre frontend e backend

## Status

Em desenvolvimento.

A aplicação possui o fluxo de processamento de pedidos e autenticação implementados. Novas funcionalidades podem ser adicionadas conforme a evolução do projeto.