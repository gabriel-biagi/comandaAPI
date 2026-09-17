// Recebe ComandaResponse da API
// Estrutura esperada:
// {
//   "nome": "Cliente",
//   "pedidos": [
//     { "item": "Marmita", "tamanho": "dupla", "acompanhamentos": ["Paçoca", "chocolate"] },
//     { "item": "Copo", "tamanho": "grande", "acompanhamentos": ["Oreo", "morango"] }
//   ],
//   "valor": "65,00",
//   "formaDePagamento": "pix",
//   "endereço": "Rua dos jacarandás..."
// }

function gerarComanda(comandaResponse) {
    let texto = "";
   
    // Cabeçalho fixo (32 caracteres)
    texto += "================================\n";
    texto += "        PEDIDO / COMANDA        \n";
    texto += "================================\n";
    texto += `Cliente: ${comandaResponse.nome}\n`;
    texto += "--------------------------------\n";

    // Loop para processar os pedidos
    comandaResponse.pedidos.forEach((pedido, index) => {
        // Monta o item com tamanho
        const itemComTamanho = pedido.tamanho && pedido.tamanho !== "Não Informado" 
            ? `${pedido.item} (${pedido.tamanho})`
            : pedido.item;
        
        // Corta se for muito comprido
        const itemCortado = itemComTamanho.substring(0, 32);
        texto += `${itemCortado}\n`;

        // Adiciona acompanhamentos, se houver
        if (pedido.acompanhamentos && pedido.acompanhamentos.length > 0) {
            pedido.acompanhamentos.forEach(acompanhamento => {
                const acompCortado = `  - ${acompanhamento}`.substring(0, 32);
                texto += `${acompCortado}\n`;
            });
        }

        // Linha separadora entre pedidos (exceto no último)
        if (index < comandaResponse.pedidos.length - 1) {
            texto += "--------------------------------\n";
        }
    });

    // Rodapé com informações adicionais
    texto += "--------------------------------\n";
    
    // Valor
    const textoValor = "TOTAL:";
    const valorFormatado = `R$ ${comandaResponse.valor}`;
    const espacosValor = " ".repeat(Math.max(1, 32 - textoValor.length - valorFormatado.length));
    texto += `${textoValor}${espacosValor}${valorFormatado}\n`;

    // Forma de pagamento
    texto += `Pagamento: ${comandaResponse.formaDePagamento}\n`;

    // Endereço
    texto += "--------------------------------\n";
    texto += "Endereço:\n";
    // Quebra endereço em linhas de até 32 caracteres
    const endereco = comandaResponse.endereço;
    for (let i = 0; i < endereco.length; i += 32) {
        texto += endereco.substring(i, i + 32) + "\n";
    }

    texto += "================================\n";
    texto += "\n\n\n"; // Espaço para corte do papel

    console.log("Comanda gerada:\n" + texto);
    dispararRawBT(texto);
}

function dispararRawBT(comandaTexto) {
    try {
        const bytes = new TextEncoder().encode(comandaTexto);
        const base64 = btoa(String.fromCharCode(...bytes));
        window.location.href = `rawbt://base64/${base64}`;
    } catch (erro) {
        alert("Erro ao enviar para o RawBT: " + erro.message);
    }
}

// Exemplo de uso (após receber resposta da API):
// const resposta = await fetch('/api/comanda', {...}).then(r => r.json());
// gerarComanda(resposta);
