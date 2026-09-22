# ComandaAPI

API para transformar mensagens desestruturadas de pedidos em comandas organizadas, utilizando **IA para extração de dados**, autenticação segura e integração com **impressoras térmicas Bluetooth**.

O projeto nasceu de uma necessidade real em uma açaiteria: reduzir o trabalho manual de interpretar pedidos recebidos pelo WhatsApp, revisar as informações e preparar a comanda para impressão.

> **MVP funcional e validado em ambiente real**, incluindo processamento, revisão e impressão térmica.

**Open-source • MIT License**

---

## Como funciona

```text
Mensagem do cliente
        ↓
Frontend Web
        ↓
ASP.NET Core Web API
        ↓
ComandaController
        ↓
IComandaService / ComandaService
        ↓
Groq Cloud API
        ↓
JSON estruturado
        ↓
Revisão e edição pelo usuário
        ↓
ESC/POS
        ↓
RawBT → Impressora Bluetooth
```

A IA atua como **extratora de informações**. O resultado passa por uma etapa de revisão antes da impressão, permitindo que o operador corrija ou complemente os dados.

---

## Funcionalidades

- Processamento de mensagens de pedidos com IA
- Extração de cliente, pedidos, tamanhos, acompanhamentos, valor, pagamento e endereço
- Resposta estruturada em JSON
- Revisão e edição da comanda antes da impressão
- Adição e remoção de pedidos e acompanhamentos
- Geração de comandas em **ESC/POS para impressoras de 80mm**
- Impressão Bluetooth através do **RawBT**
- Interface responsiva para desktop e dispositivos móveis
- Autenticação e autorização com **ASP.NET Core Identity + JWT**
- Access Token e Refresh Token em **cookies HttpOnly**
- Renovação automática e rotação de Refresh Tokens
- Persistência de Refresh Tokens com Entity Framework Core
- Bootstrap automático do usuário e role `Admin`
- Tratamento global de exceções
- Tratamento específico para falhas de serviços externos
- Logs estruturados para investigação de erros da Groq
- Swagger/OpenAPI para documentação da API

---

## Stack

### Backend

- C#
- .NET 8
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- SQLite
- JWT

### Integrações

- Groq Cloud API
- `openai/gpt-oss-120b`
- RawBT
- Impressoras térmicas ESC/POS

### Frontend

- HTML5
- CSS3
- JavaScript

### Ferramentas

- Swagger / OpenAPI
- Git / GitHub
- Visual Studio

---

## Arquitetura

O projeto utiliza uma separação de responsabilidades baseada em **Controllers, Services, Repositories, Infrastructure e Middleware**.

```text
comandaAPI/
├── Controllers/
│   ├── AuthController.cs
│   └── ComandaController.cs
│
├── Services/
│   ├── ComandaService.cs
│   ├── TokenService.cs
│   └── Interfaces/
│
├── Infrastructure/
│   ├── Data/
│   │   ├── Context/
│   │   ├── Entities/
│   │   └── Seed/
│   └── Repositories/
│
├── Domain/
│   └── Exception/
│
├── Middlewares/
├── Models/
│   └── DTOs/
├── Migrations/
├── wwwroot/
└── Program.cs
```

### Responsabilidades

**Controllers**

Responsáveis pela camada HTTP e entrada das requisições.

**Services**

Concentram regras e casos de uso da aplicação.

Por exemplo, `ComandaService` encapsula a integração com a Groq, deixando o `ComandaController` responsável por receber a requisição e delegar o processamento.

**Repositories**

Isolam o acesso a dados específicos, como a persistência e rotação dos Refresh Tokens.

**Infrastructure**

Contém Entity Framework Core, Identity, contexto do banco, entidades e inicialização de dados.

**Middleware**

Centraliza o tratamento de exceções e padroniza as respostas de erro da API.

---

## Processamento com IA

O endpoint de comandas recebe o texto original:

```json
{
  "text": "Gabriel, quero um açaí 500ml com paçoca e leite ninho. R$ 25 no Pix..."
}
```

O `ComandaService` envia o conteúdo para a Groq utilizando um prompt estruturado e transforma a resposta em um `ComandaResponse`.

Exemplo:

```json
{
  "nome": "Gabriel",
  "pedidos": [
    {
      "item": "Açaí",
      "tamanho": "500ml",
      "acompanhamentos": [
        "Paçoca",
        "Leite Ninho"
      ]
    }
  ],
  "valor": "25,00",
  "formaDePagamento": "Pix",
  "endereço": "Não Informado"
}
```

O resultado não é enviado diretamente para impressão. O frontend apresenta os dados para **revisão e edição**, permitindo que o operador corrija ou complemente as informações antes da geração da comanda.

---

## Autenticação

A API utiliza **ASP.NET Core Identity + JWT + Refresh Token**.

### Fluxo

```text
Login
  ↓
Access Token + Refresh Token
  ↓
Cookies HttpOnly
  ↓
Requisições autenticadas
  ↓
Access Token expira
  ↓
Frontend recebe 401
  ↓
/api/auth/refresh
  ↓
Novos tokens + rotação do Refresh Token
```

- Access Token: **15 minutos**
- Refresh Token: **7 dias**
- Tokens armazenados em cookies `HttpOnly`
- Refresh Tokens persistidos no SQLite
- Rotação do Refresh Token após utilização
- Roles através do ASP.NET Core Identity
- Endpoints protegidos com `[Authorize]`

O usuário e a role `Admin` inicial são configurados através do **Identity Seeder**, evitando depender de credenciais fixas no código.

---

## Tratamento de exceções

A aplicação possui um middleware global que converte exceções conhecidas em respostas HTTP padronizadas.

| Exceção | HTTP |
|---|---:|
| `ValidationException` | 400 |
| `ResourceNotFoundException` | 404 |
| `BusinessException` | 409 |
| `ExternalServiceException` | 502 |
| Exceções inesperadas | 500 |

Falhas da Groq recebem tratamento específico.

O status retornado pelo serviço externo é preservado na `ExternalServiceException` para fins técnicos, enquanto a API retorna **502 Bad Gateway** ao cliente.

Além disso, detalhes da resposta da Groq são registrados no log para facilitar investigação e diagnóstico sem expor essas informações diretamente ao frontend.

---

## Persistência

O projeto utiliza **SQLite + Entity Framework Core**.

O banco armazena:

- Usuários e roles através do ASP.NET Core Identity
- Refresh Tokens
- Relacionamentos entre usuários e tokens

As alterações de estrutura são controladas através de **EF Core Migrations**.

---

## Impressão térmica

Após a revisão, a aplicação gera a comanda no formato **ESC/POS** e utiliza o protocolo do RawBT:

```text
Frontend
   ↓
ESC/POS
   ↓
Base64
   ↓
rawbt:base64,...
   ↓
RawBT
   ↓
Bluetooth
   ↓
Impressora térmica
```

Os caracteres são normalizados para remover acentos quando necessário, aumentando a compatibilidade com impressoras térmicas.

**A impressão foi validada fisicamente em ambiente real utilizando RawBT e uma impressora Bluetooth.**

---

## Endpoints principais

### Autenticação

```text
POST /api/auth/login
POST /api/auth/logout
POST /api/auth/refresh
GET  /api/auth/user-logged
POST /api/auth/register
POST /api/auth/change-password
```

### Comandas

```text
POST /api/comanda
```

O endpoint de processamento de comandas exige autenticação.

---

## Executando o projeto

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Chave da [Groq Cloud API](https://console.groq.com/)
- Opcional: RawBT + impressora Bluetooth ESC/POS

### Configuração

Utilize **User Secrets** para informações sensíveis:

```bash
dotnet user-secrets set "GroqApiKey" "SUA_CHAVE"
dotnet user-secrets set "JWT:SecretKey" "SUA_CHAVE_JWT"
dotnet user-secrets set "Admin:Username" "SEU_ADMIN"
dotnet user-secrets set "Admin:Password" "SUA_SENHA"
```

> **Nunca adicione chaves de API, senhas, tokens ou outros secrets diretamente ao repositório.**

### Executar

```bash
dotnet run --launch-profile http
```

Durante o desenvolvimento, a aplicação pode ser acessada localmente ou através do IP da máquina na rede local para testes em dispositivos móveis.

O Swagger fica disponível durante o desenvolvimento.

---

## Status

**MVP funcional — validado em ambiente real.**

### Implementado

- [x] Processamento de pedidos com IA
- [x] API REST com ASP.NET Core
- [x] Separação Controller / Service
- [x] Repository para Refresh Tokens
- [x] ASP.NET Core Identity
- [x] JWT + Refresh Token
- [x] Renovação automática de autenticação
- [x] Tratamento global de exceções
- [x] Tratamento de falhas de serviços externos
- [x] SQLite + Entity Framework Core
- [x] Interface web responsiva
- [x] Revisão e edição das comandas
- [x] Geração ESC/POS
- [x] Impressão Bluetooth com RawBT
- [x] Validação em ambiente real

### Próximos passos

- [ ] Hash dos Refresh Tokens armazenados
- [ ] Rate limiting
- [ ] Histórico de comandas
- [ ] Refinamento dos prompts e validação da resposta da IA

---

## Licença

Este projeto está licenciado sob a **MIT License**.

Consulte o arquivo [`LICENSE`](./LICENSE) para obter os termos completos da licença.

---

## Objetivo

O projeto foi desenvolvido para resolver um problema operacional real e, ao mesmo tempo, aplicar conceitos de engenharia de software e desenvolvimento backend, incluindo:

- APIs REST
- Separação de responsabilidades
- Injeção de dependência
- Integração com serviços externos
- Autenticação e autorização
- Persistência de dados
- Tratamento de exceções
- Comunicação assíncrona
- Integração com hardware
- Desenvolvimento e validação em ambiente real