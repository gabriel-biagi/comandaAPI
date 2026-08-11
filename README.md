# 📋 Processador de Comandas WhatsApp

## 📌 Sobre o Projeto

O **Processador de Comandas** é uma solução desenvolvida para otimizar a triagem de pedidos via WhatsApp.

### 🎯 O Problema
Em horários de pico, a transcrição manual das mensagens dos clientes para a impressão em impressoras térmicas gerava gargalos, lentidão no atendimento e potenciais erros no registro dos itens.

### 🚀 A Solução e Impacto
A aplicação automatizou a extração dos dados brutos das conversas usando Inteligência Artificial. O atendente apenas cola o texto da mensagem na interface e o sistema entrega os dados limpos (nome, pedido, acompanhamentos, valor, pagamento e endereço) prontos para impressão.

**Ganhos:**
- Agilidade expressiva no fluxo de atendimento durante horários de pico.
- Eliminação do esforço manual de digitação de comandas.
- Padronização no formato dos dados enviados para a produção.

---

## 🛠️ Tecnologias Utilizadas

- **Backend:** .NET 8 / C# Web API
- **Inteligência Artificial:** Groq Cloud (LLaMA 3.3 70B)
- **Front-end:** HTML5, CSS3, JavaScript (ES6+)

---

## ⚙️ Arquitetura e Estrutura do Backend

O backend foi construído em ASP.NET Core Web API, utilizando o padrão Controller para expor as rotas do sistema.

### Estrutura de Pastas
```text
comandaAPI/
├── Controllers/
│   └── ComandaController.cs   # Controller com a rota ProcessarComanda
├── wwwroot/                    # Interface web estática
│   ├── index.html
│   ├── style.css
│   └── script.js
├── appsettings.json            # Configurações do projeto
└── Program.cs                  # Pipeline e inicialização da aplicação
Funcionamento do Controller (ComandaController)
A rota HTTP ProcessarComanda é responsável por:

Receber o payload do front-end contendo a mensagem bruta do WhatsApp via POST.

Executar a comunicação direta com a API da Groq (llama-3.3-70b-versatile), repassando o contexto do prompt do sistema com restrições rígidas de formatação.

Processar a resposta e retornar o texto padronizado do pedido sem poluição de markdown ou mensagens adicionais.

🚀 Como Executar o Projeto
Pré-requisitos
.NET 8 SDK instalado.

Chave de API da Groq Cloud.

Passo a Passo
Clone o repositório:

Bash
git clone https://github.com/gabriel-biagi/comandaAPI.git
cd comandaAPI
Configure a Chave da API:
Adicione sua GroqApiKey nas variáveis de ambiente ou no arquivo appsettings.Development.json.

Execute a aplicação:

Bash
dotnet run
Acesse no navegador:
Abra http://localhost:5084 no seu navegador.
