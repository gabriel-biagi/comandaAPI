# ComandaAPI

API para processamento e estruturação de pedidos recebidos em mensagens de WhatsApp, utilizando inteligência artificial para transformar mensagens em dados organizados.
O projeto foi desenvolvido para uma situação real de uso em uma açaiteria, com o objetivo de reduzir a necessidade de interpretar e organizar manualmente as mensagens de pedidos.

## Justificativa do Projeto

A criação manual de comandas é uma tarefa que consome tempo significativo da equipe, interferindo na eficiência e produtividade operacional. Ao receber pedidos via WhatsApp, funcionários precisam interpretar mensagens desorganizadas, extrair informações relevantes e estruturar tudo manualmente para posterior impressão.
Com a ComandaAPI, esse processo é automatizado: a inteligência artificial extrai e estrutura os dados em tempo real, permitindo que o tempo antes despendido com organização manual seja redirecionado para atividades de maior valor agregado, acelerando significativamente o fluxo de processamento de pedidos.

## Sobre o projeto

Mensagens de pedidos recebidas pelo WhatsApp podem apresentar informações desorganizadas, abreviações e diferentes formas de descrever o mesmo pedido.
A ComandaAPI recebe o texto da mensagem, envia o conteúdo para a API da Groq com instruções específicas de processamento e retorna os dados estruturados em JSON.

### Fluxo

```text
Mensagem do WhatsApp
        ↓
Frontend (Login)
        ↓
ASP.NET Core Web API (JWT + Refresh Token)
        ↓
ComandaController
        ↓
Groq Cloud API
        ↓
JSON estruturado
        ↓
Modal de Revisão/Edição
        ↓
ESC/POS + RawBT (Impressora Térmica)
```

## Funcionalidades

- Processamento de mensagens de pedidos em texto
- Extração de informações relevantes do pedido utilizando IA
- Estruturação da resposta em JSON
- Integração com a Groq Cloud API
- **Autenticação com JWT e Refresh Token** (Access Token 15min, Refresh Token 7 dias)
- **Auto-renovação de tokens** via refresh automático
- ASP.NET Core Identity para gerenciamento de usuários
- Autenticação utilizando cookies HttpOnly, com `Secure` habilitado fora do ambiente de desenvolvimento
- **Modal de revisão e edição** de pedidos antes da impressão
- **Edição completa** de cliente, itens, tamanhos, acompanhamentos, valor, forma de pagamento e endereço
- Adição e remoção dinâmica de pedidos e acompanhamentos
- **Geração de comanda em formato ESC/POS** (80mm de largura)
- **Integração com RawBT** para impressoras Bluetooth
- Remoção automática de acentos para compatibilidade com impressoras térmicas
- Tratamento global de exceções
- Documentação da API com Swagger/OpenAPI
- Interface web responsiva integrada à aplicação
- Validação do conteúdo processado
- Comunicação assíncrona com APIs externas
- **Impressão térmica validada em ambiente real** utilizando RawBT e impressora Bluetooth

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

A API possui autenticação utilizando **ASP.NET Core Identity e JWT com Refresh Token**.

### Fluxo de Tokens

- **Access Token:** 15 minutos de validade, armazenado em cookie HttpOnly
- **Refresh Token:** 7 dias de validade, armazenado em cookie HttpOnly e persistido no banco de dados
- **Renovação Automática:** Frontend detecta 401 (token expirado) e chama `/api/auth/refresh` automaticamente
- **Rotação:** O Refresh Token utilizado é removido e um novo é gerado.

### Claims do JWT

O Access Token inclui:
- `ClaimTypes.Name` — username do usuário
- `ClaimTypes.NameIdentifier` — ID único do usuário (necessário para `GetUserId()`)
- `ClaimTypes.Role` — papel/role do usuário
- `JwtRegisteredClaimNames.Jti` — token ID único

### Endpoints de Autenticação

- `POST /api/auth/login` — Autentica o usuário e define os tokens em cookies HttpOnly
- `POST /api/auth/logout` — Invalida tokens e deleta cookies
- `POST /api/auth/refresh` — Renova Access Token e Refresh Token
- `GET /api/auth/user-logged` — Retorna informações do usuário autenticado
- `POST /api/auth/register` — Cria novo usuário (Admin only)
- `POST /api/auth/change-password` — Altera senha do usuário (Admin only)

Os endpoints protegidos utilizam `[Authorize]` para validar tokens.

## Processamento de pedidos

O endpoint responsável pelo processamento recebe o texto da mensagem e utiliza a Groq Cloud API para interpretar e estruturar as informações.

A comunicação com a API externa é realizada utilizando `HttpClient`.

A requisição enviada à Groq define instruções para que a resposta siga uma estrutura JSON específica.

O resultado é convertido para o modelo de resposta da aplicação antes de ser retornado pela API.

### Fluxo de Processamento

```text
Texto recebido
    ↓
Prompt de processamento (Groq)
    ↓
Groq Cloud API
    ↓
Resposta JSON (Nome, Pedidos[], Valor, Forma de Pagamento, Endereço)
    ↓
Desserialização → ComandaResponse
    ↓
Frontend abre Modal de Revisão
    ↓
Usuário edita/confirma dados
    ↓
Geração ESC/POS
    ↓
Envio via RawBT → Impressora Térmica
```

### Estrutura da Resposta

A resposta estruturada contém:

```json
{
  "nome": "Nome do Cliente",
  "pedidos": [
    {
      "item": "Marmita",
      "tamanho": "dupla",
      "acompanhamentos": ["Paçoca", "Chocolate"]
    }
  ],
  "valor": "55,00",
  "formaDePagamento": "pix",
  "endereço": "Rua..., Número..., Bairro..."
}
```

### Modal de Revisão

Após receber a resposta da IA, uma modal permite:

- Editar nome do cliente
- Adicionar/remover pedidos
- Editar item e tamanho de cada pedido
- Adicionar/remover acompanhamentos por pedido
- Alterar valor total
- Modificar forma de pagamento
- Editar endereço de entrega

Todas as alterações são feitas em tempo real antes de confirmar a impressão.

## Frontend

A aplicação possui uma interface web estática hospedada pelo próprio ASP.NET Core.

```text
wwwroot/
├── index.html
├── styles.css
└── app.js
```

### Arquitetura

- **Tela de Login:** Autenticação com JWT e Refresh Token
- **Tela de Comanda:** Entrada de texto e processamento
- **Modal de Revisão:** Edição completa de dados antes de imprimir

### Comunicação

O JavaScript realiza a comunicação com a API utilizando `fetch` com suporte a:

- Auto-renovação de tokens (detecta 401 e chama `/api/auth/refresh`)
- Cookies HttpOnly (tokens enviados automaticamente)
- Requisições autenticadas com `credentials: 'include'`

### Geração de Comanda (ESC/POS)

A aplicação gera comandas formatadas para impressoras térmicas de 80mm:

- Cabeçalho e rodapé com bordas decorativas
- Dados estruturados (cliente, itens, acompanhamentos, total, pagamento, endereço)
- Remoção automática de acentos para compatibilidade
- Codificação em base64 para transmissão via RawBT
- Protocolo `rawbt:base64,{dados}` para integração com impressoras Bluetooth

### Suporte a Dispositivos Móveis

A interface é responsiva e funciona em:

- Navegadores desktop
- Navegadores mobile (iOS/Android)
- Acesso via IP local na rede (ex: `http://192.168.x.x:5084`)
- Integração com aplicativos de impressão Bluetooth (RawBT)

## Banco de dados

O projeto utiliza **SQLite** junto ao Entity Framework Core.

### Tabelas Principais

- `AspNetUsers` — Usuários do sistema (Identity)
- `AspNetRoles` — Papéis/roles (Identity)
- `AspNetUserRoles` — Associação usuário-role (Identity)
- `RefreshTokens` — Refresh tokens persistidos com relação a usuário (cascade delete)

### Refresh Token Persistence

Cada Refresh Token é armazenado como:

```csharp
public class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public string UserId { get; set; }  // FK → AspNetUsers
    public ApplicationUser User { get; set; }
    public string HashedToken { get; set; }  // Token em plaintext
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

- **HashedToken:** Atualmente armazenado em plaintext. A implementação de hash dos refresh tokens está prevista como melhoria de segurança.
- **Relação:** Um usuário pode possuir múltiplos refresh tokens.
- **Revogação:** O refresh token utilizado é removido após a renovação.

As alterações do modelo são controladas através de migrations do Entity Framework Core.

## Tratamento de exceções

A aplicação possui middleware para tratamento global de exceções.

Exceções específicas da aplicação são utilizadas para representar diferentes situações, incluindo:

- Erros de negócio
- Recursos não encontrados
- Erros de validação

O middleware transforma essas exceções em respostas HTTP apropriadas para a API.

## Como executar

### Pré-requisitos

- .NET 8 SDK
- Uma chave da Groq Cloud API
- (Opcional) Impressora Bluetooth com suporte a ESC/POS e app RawBT instalado

### 1. Clone o repositório

```bash
git clone https://github.com/gabriel-biagi/comandaAPI.git
cd comandaAPI
```

### 2. Configure as variáveis de ambiente

```bash
# Configurar chave Groq
dotnet user-secrets set "GroqApiKey" "SUA_CHAVE_GROQ"

# Configurar chave JWT (se não estiver definida)
dotnet user-secrets set "JWT:SecretKey" "uma_chave_segura_de_pelo_menos_32_caracteres"
```

### 3. Execute a aplicação

```bash
# Desenvolvimento (HTTP)
dotnet run --launch-profile http

# Execução com HTTPS
dotnet run
```

### 4. Acesse a aplicação

- **Localhost:** `http://localhost:5084`
- **Rede Local:** `http://{seu_ip}:5084` (mesmo WiFi)

### 5. Login

Utilize as credenciais de um usuário previamente cadastrado.

> Para fins de demonstração, configure um usuário Admin localmente antes de acessar a aplicação.

## Swagger

Durante o desenvolvimento, a API disponibiliza documentação interativa através do Swagger/OpenAPI.

Após iniciar a aplicação, a interface do Swagger pode ser acessada pela URL disponibilizada pelo ASP.NET Core.

## Objetivo do projeto

O projeto foi desenvolvido para uma situação real de uso em uma açaiteria e validado em ambiente real, incluindo o processamento, revisão e impressão das comandas.
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

**MVP funcional** — Sistema validado em ambiente real para processamento,
revisão e impressão de comandas.

### Recursos Implementados ✅

- Autenticação e autorização (JWT + Refresh Token)
- Processamento de pedidos via Groq AI
- Modal de revisão e edição de dados
- Geração de comanda em ESC/POS
- Integração com impressoras Bluetooth (RawBT)
- Tratamento global de exceções
- Suporte a acesso mobile via rede local
- Validação de dados de entrada
- Renovação automática de tokens

### Roadmap Futuro

- [ ] Rate limiting por IP/usuário
- [ ] Auditoria de logins e operações sensíveis
- [ ] Histórico de comandas processadas
- [ ] Webhook para confirmação de impressão
- [ ] Refinamento de prompts Groq
- [ ] Dashboard de relatórios
- [ ] Suporte a múltiplos níveis de permissão