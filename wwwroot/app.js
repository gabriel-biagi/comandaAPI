const API_URL = 'http://localhost:5084';

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

function getToken() {
    return localStorage.getItem('accessToken');
}

function clearToken() {
    localStorage.removeItem('accessToken');
}

function isTokenValid() {
    return !!getToken();
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
window.addEventListener('load', () => {
    if (isTokenValid()) {
        showComandaScreen();
    } else {
        showLoginScreen();
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
            body: JSON.stringify({ username, password }),
            credentials: 'include'
        });

        const data = await response.json();

        if (!response.ok) {
            showStatus(data.message || 'Falha ao fazer login', 'error', 'loginStatus');
            return;
        }

        if (data.token && data.token.accessToken) {
            setToken(data.token.accessToken);
            showStatus('Login realizado com sucesso!', 'success', 'loginStatus');
            setTimeout(() => {
                showComandaScreen();
            }, 500);
        } else {
            showStatus('Erro ao obter token', 'error', 'loginStatus');
        }
    } catch (err) {
        console.error(err);
        showStatus('Erro ao conectar ao servidor', 'error', 'loginStatus');
    } finally {
        btnLogin.disabled = false;
    }
});

// ========== LOGOUT ==========

btnLogout.addEventListener('click', () => {
    clearToken();
    showLoginScreen();
    showStatus('Desconectado com sucesso', 'success', 'loginStatus');
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

    const token = getToken();
    if (!token) {
        showStatus('Token não encontrado. Faça login novamente.', 'error', 'comandaStatus');
        return;
    }

    // Ativar loading
    btnEnviar.disabled = true;
    spinner.style.display = 'block';
    btnEnviarText.style.display = 'none';
    comandaStatus.textContent = '';

    try {
        const response = await fetch(API_URL + '/api/comanda', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ text: textValue })
        });

        if (!response.ok) {
            if (response.status === 401) {
                showStatus('Token expirado. Faça login novamente.', 'error', 'comandaStatus');
                clearToken();
                showLoginScreen();
                return;
            }
            throw new Error(`Erro no servidor: ${response.status}`);
        }

        const data = await response.json();
        
        // Tratando o retorno
        if (typeof data === 'string') {
            outputTexto.value = data;
        } else if (data) {
            outputTexto.value = JSON.stringify(data, null, 2);
        }

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

// ========== COMANDA - COPIAR ==========

btnCopiar.addEventListener('click', async () => {
    const outputValue = outputTexto.value;

    if (!outputValue) {
        showStatus('Não há conteúdo para copiar.', 'error', 'comandaStatus');
        return;
    }

    try {
        await navigator.clipboard.writeText(outputValue);
        showStatus('Resultado copiado para a área de transferência!', 'success', 'comandaStatus');
    } catch (err) {
        console.error(err);
        outputTexto.select();
        document.execCommand('copy');
        showStatus('Resultado copiado!', 'success', 'comandaStatus');
    }
});
