const API_URL = 'http://localhost:5084';

// ========== FETCH COM AUTO-REFRESH ==========

async function fetchWithRefresh(url, options = {}) {
    let response = await fetch(url, { ...options, credentials: 'include' });

    if (response.status === 401) {
        const refreshResponse = await fetch(API_URL + '/api/auth/refresh', {
            method: 'POST',
            credentials: 'include'
        });

        if (refreshResponse.ok) {
            response = await fetch(url, { ...options, credentials: 'include' });
        } else {
            showLoginScreen();
            return response;
        }
    }

    return response;
}

// Elementos do Login
const loginScreen = document.getElementById('loginScreen');
const comandaScreen = document.getElementById('comandaScreen');
const usernameInput = document.getElementById('username');
const passwordInput = document.getElementById('password');
const btnLogin = document.getElementById('btnLogin');
const loginStatus = document.getElementById('loginStatus');
const btnLogout = document.getElementById('btnLogout');

// Elementos da Comanda
const inputTexto = document.getElementById('inputTexto');
const outputTexto = document.getElementById('outputTexto');
const btnColar = document.getElementById('btnColar');
const btnEnviar = document.getElementById('btnEnviar');
const btnCopiar = document.getElementById('btnCopiar');
const spinner = document.getElementById('spinner');
const btnEnviarText = btnEnviar.querySelector('.btn-text');
const comandaStatus = document.getElementById('comandaStatus');

// Elementos da Modal
const reviewModal = document.getElementById('reviewModal');
const btnCloseModal = document.getElementById('btnCloseModal');
const btnCancelarModal = document.getElementById('btnCancelarModal');
const btnConfirmarComanda = document.getElementById('btnConfirmarComanda');
const btnAdicionarPedido = document.getElementById('btnAdicionarPedido');
const pedidosList = document.getElementById('pedidosList');
const editNome = document.getElementById('editNome');
const editValor = document.getElementById('editValor');
const editFormaPagamento = document.getElementById('editFormaPagamento');
const editEndereco = document.getElementById('editEndereco');

// Variável global pra armazenar comanda atual
let comandaAtual = null;

// ========== FUNÇÕES UTILITÁRIAS ==========

function showStatus(message, type = '', elementId = 'loginStatus') {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.className = `status-message ${type}`;
    if (type !== 'error') {
        setTimeout(() => {
            if (element.textContent === message) {
                element.textContent = '';
                element.className = 'status-message';
            }
        }, 4000);
    }
}

// ========== NAVEGAÇÃO ENTRE TELAS ==========

function showLoginScreen() {
    loginScreen.classList.add('active');
    comandaScreen.classList.remove('active');
    usernameInput.value = '';
    passwordInput.value = '';
}

function showComandaScreen() {
    loginScreen.classList.remove('active');
    comandaScreen.classList.add('active');
}

// Verifica se usuário já está logado ao carregar a página
window.addEventListener('load', async () => {
    try {
        const response = await fetchWithRefresh(API_URL + '/api/auth/user-logged', {
            method: 'GET',
            credentials: 'include'
        });

        if (!response.ok) {
            showLoginScreen()
            showStatus('Usuário não está logado.', 'error', 'loginStatus');
            return;
        }

        showComandaScreen();
    } catch (err) {
        console.error(err);
        showStatus('Erro ao verificar status do usuário', 'error', 'loginStatus');
    }
});

// ========== LOGIN ==========

btnLogin.addEventListener('click', async () => {
    const username = usernameInput.value.trim();
    const password = passwordInput.value.trim();

    if (!username || !password) {
        showStatus('Por favor, preencha usuário e senha.', 'error', 'loginStatus');
        return;
    }

    btnLogin.disabled = true;

    try {
        const response = await fetch(API_URL + '/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ username, password })
        });

        const data = await response.json();

        if (!response.ok) {
            showStatus(data.message || 'Falha ao fazer login', 'error', 'loginStatus');
            return;
        }
        showStatus('Login realizado com sucesso!', 'success', 'loginStatus');
        setTimeout(() => {
            showComandaScreen();
        }, 500);
        } catch (err) {
            console.error(err);
            showStatus('Erro ao conectar ao servidor', 'error', 'loginStatus');
        } finally {
            btnLogin.disabled = false;
        }
    });

// ========== LOGOUT ==========

btnLogout.addEventListener('click', async () => {
    try {
        const response = await fetchWithRefresh(API_URL + '/api/auth/logout', {
            method: 'POST',
            credentials: 'include'
        });
        if (response.ok) {
            showLoginScreen();
            showStatus('Desconectado com sucesso', 'success', 'loginStatus');
        }
    } catch (err) {
        console.error(err);
        showStatus('Erro ao desconectar', 'error', 'loginStatus');
    }
});

// ========== COMANDA - COLAR ==========

btnColar.addEventListener('click', async () => {
    try {
        if (!navigator.clipboard || !navigator.clipboard.readText) {
            throw new Error('A API de Área de Transferência não é suportada neste navegador.');
        }
        const text = await navigator.clipboard.readText();
        inputTexto.value = text;
        showStatus('Texto colado com sucesso!', 'success', 'comandaStatus');
    } catch (err) {
        console.error(err);
        showStatus('Falha ao colar. Verifique as permissões do navegador.', 'error', 'comandaStatus');
    }
});

// ========== COMANDA - ENVIAR ==========

btnEnviar.addEventListener('click', async () => {
    const textValue = inputTexto.value.trim();

    if (!textValue) {
        showStatus('Por favor, insira ou cole algum texto primeiro.', 'error', 'comandaStatus');
        inputTexto.focus();
        return;
    }

    // Ativar loading
    btnEnviar.disabled = true;
    spinner.style.display = 'block';
    btnEnviarText.style.display = 'none';
    comandaStatus.textContent = '';

    try {
        const response = await fetchWithRefresh(API_URL + '/api/comanda', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ text: textValue })
        });

        if (!response.ok) {
            throw new Error(`Erro no servidor: ${response.status}`);
        }

        const data = await response.json();

        // Armazena a comanda e abre modal
        comandaAtual = data;
        preencherModal(data);
        abrirModal();

        showStatus('Processado com sucesso!', 'success', 'comandaStatus');
    } catch (err) {
        console.error(err);
        showStatus('Erro ao processar comanda. Tente novamente.', 'error', 'comandaStatus');
    } finally {
        // Desativar loading
        btnEnviar.disabled = false;
        spinner.style.display = 'none';
        btnEnviarText.style.display = 'inline';
    }
});

// ========== MODAL - ABRIR/FECHAR ==========

function abrirModal() {
    reviewModal.classList.add('active');
}

function fecharModal() {
    reviewModal.classList.remove('active');
    comandaAtual = null;
}

btnCloseModal.addEventListener('click', fecharModal);
btnCancelarModal.addEventListener('click', fecharModal);

// Fechar modal ao clicar fora
reviewModal.addEventListener('click', (e) => {
    if (e.target === reviewModal) {
        fecharModal();
    }
});

// ========== MODAL - PREENCHER ==========

function preencherModal(comanda) {
    editNome.value = comanda.nome || '';
    editValor.value = comanda.valor || '';
    editFormaPagamento.value = comanda.formaDePagamento || '';
    editEndereco.value = comanda.endereço || '';

    pedidosList.innerHTML = '';
    
    if (comanda.pedidos && comanda.pedidos.length > 0) {
        comanda.pedidos.forEach((pedido, index) => {
            adicionarPedidoCard(pedido, index);
        });
    }
}

function adicionarPedidoCard(pedido = null, index = null) {
    const id = index !== null ? index : Date.now();
    const pedidoCard = document.createElement('div');
    pedidoCard.className = 'pedido-card';
    pedidoCard.id = `pedido-${id}`;

    const item = pedido?.item || '';
    const tamanho = pedido?.tamanho || '';
    const acompanhamentos = pedido?.acompanhamentos || [];

    pedidoCard.innerHTML = `
        <div class="pedido-header">
            <h3>Pedido ${index !== null ? index + 1 : ''}</h3>
            <button type="button" class="btn-remover-pedido" onclick="removerPedido(${id})">Remover</button>
        </div>

        <div class="input-group">
            <label>Item</label>
            <input type="text" class="pedido-item" value="${item}" placeholder="Ex: Marmita, Copo">
        </div>

        <div class="input-group">
            <label>Tamanho</label>
            <input type="text" class="pedido-tamanho" value="${tamanho}" placeholder="Ex: dupla, grande">
        </div>

        <div class="input-group">
            <label>Acompanhamentos</label>
            <div class="acompanhamentos-list" id="acompanhamentos-${id}">
                ${acompanhamentos.map((acomp, i) => `
                    <div class="acompanhamento-item">
                        <input type="text" value="${acomp}" placeholder="Acompanhamento">
                        <button type="button" class="btn-remover-acompanhamento" onclick="removerAcompanhamento(${id}, ${i})">-</button>
                    </div>
                `).join('')}
            </div>
            <button type="button" class="btn-adicionar-acompanhamento" onclick="adicionarAcompanhamento(${id})">+ Adicionar Acompanhamento</button>
        </div>
    `;

    pedidosList.appendChild(pedidoCard);
}

function removerPedido(id) {
    const card = document.getElementById(`pedido-${id}`);
    if (card) card.remove();
}

function adicionarAcompanhamento(pedidoId) {
    const acompList = document.getElementById(`acompanhamentos-${pedidoId}`);
    const acompItem = document.createElement('div');
    acompItem.className = 'acompanhamento-item';
    acompItem.innerHTML = `
        <input type="text" placeholder="Novo acompanhamento">
        <button type="button" class="btn-remover-acompanhamento" onclick="this.parentElement.remove()">-</button>
    `;
    acompList.appendChild(acompItem);
}

function removerAcompanhamento(pedidoId, index) {
    const acompList = document.getElementById(`acompanhamentos-${pedidoId}`);
    const items = acompList.querySelectorAll('.acompanhamento-item');
    if (items[index]) items[index].remove();
}

btnAdicionarPedido.addEventListener('click', () => {
    adicionarPedidoCard();
});

// ========== MODAL - CONFIRMAR ==========

btnConfirmarComanda.addEventListener('click', () => {
    const comandaEditada = coletarDadosModal();
    
    if (!comandaEditada) {
        showStatus('Preencha os dados obrigatórios da comanda.', 'error', 'comandaStatus');
        return;
    }

    fecharModal();
    gerarComanda(comandaEditada);
    showStatus('Comanda enviada para impressão!', 'success', 'comandaStatus');
});

function coletarDadosModal() {
    const nome = editNome.value.trim();
    const valor = editValor.value.trim();
    const formaDePagamento = editFormaPagamento.value.trim();
    const endereco = editEndereco.value.trim();

    if (!nome || !valor || !formaDePagamento) {
        return null;
    }

    const pedidos = [];
    document.querySelectorAll('.pedido-card').forEach(card => {
        const item = card.querySelector('.pedido-item').value.trim();
        const tamanho = card.querySelector('.pedido-tamanho').value.trim();
        const acompanhamentos = Array.from(card.querySelectorAll('.acompanhamento-item input'))
            .map(input => input.value.trim())
            .filter(val => val);

        if (item) {
            pedidos.push({
                item,
                tamanho: tamanho || 'Não Informado',
                acompanhamentos
            });
        }
    });

    if (pedidos.length === 0) {
        return null;
    }

    return {
        nome,
        pedidos,
        valor,
        formaDePagamento,
        endereço: endereco || 'Não Informado'
    };
}

// ========== GERAR COMANDA (ESC/POS) ==========

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

function removerAcentos(texto) {
    return texto.normalize("NFD").replace(/[\u0300-\u036f]/g, "");
}

function dispararRawBT(comandaTexto) {
    try {
        const textoLimpo = removerAcentos(comandaTexto);

        const bytes = new TextEncoder().encode(textoLimpo);
        const base64 = btoa(String.fromCharCode(...bytes));

        window.location.href = `rawbt:base64,${base64}`;
    } catch (erro) {
        console.error('Erro ao enviar para RawBT:', erro);
        showStatus("Erro ao enviar para o RawBT: " + erro.message, 'error', 'comandaStatus');
    }
}