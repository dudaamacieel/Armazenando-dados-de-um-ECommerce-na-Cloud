const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');

// Configurar canvas para tela cheia
canvas.width = window.innerWidth;
canvas.height = window.innerHeight;

// Redimensionar canvas quando a janela for redimensionada
window.addEventListener('resize', () => {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
});

// Objeto Mario (player)
const mario = {
    x: 100,
    y: window.innerHeight / 2,
    width: 90,
    height: 80,
    velocity: 0,
    gravity: 0.5,
    jump: -9,
    image: new Image(),
    isJumping: false
};

// Carregar imagem real do Mario
mario.image.src = 'mario.png';

// Objeto para gerenciar canos
const pipes = [];
const pipeGap = 190;
const pipeWidth = 90;
const pipeFrequency = 100;
let pipeTimer = 50;

// Variáveis do jogo
let score = 0;
let gameRunning = true;
let backgroundColor = '#87CEEB';

//Fundo do jogo
const backgroundImage = new Image();
backgroundImage.src = 'fundo.jpg';

// Desenhar Mario
function drawMario() {
    if (mario.image.complete) {
        ctx.drawImage(mario.image, mario.x, mario.y, mario.width, mario.height);
    } else {
        // Desenhar retângulo vermelho se imagem não carregar
        ctx.fillStyle = '#FF0000';
        ctx.fillRect(mario.x, mario.y, mario.width, mario.height);
        ctx.fillStyle = '#000';
        ctx.fillRect(mario.x + 8, mario.y + 5, 5, 5);
        ctx.fillRect(mario.x + 17, mario.y + 5, 5, 5);
    }
}

// Desenhar canos
function drawPipes() {
    pipes.forEach(pipe => {
        // Criar gradiente verde vibrante para os canos
        let gradient = ctx.createLinearGradient(pipe.x, 0, pipe.x + pipeWidth, 0);
        gradient.addColorStop(0, '#00FF00');
        gradient.addColorStop(0.5, '#00DD00');
        gradient.addColorStop(1, '#00BB00');
        
        ctx.fillStyle = gradient;
        
        // Cano superior
        ctx.fillRect(pipe.x, 0, pipeWidth, pipe.topHeight);
        
        // Cano inferior
        ctx.fillRect(pipe.x, pipe.topHeight + pipeGap, pipeWidth, canvas.height - pipe.topHeight - pipeGap);
        
        // Bordas dos canos com cor mais vibrante
        ctx.strokeStyle = '#009900';
        ctx.lineWidth = 4;
        ctx.strokeRect(pipe.x, 0, pipeWidth, pipe.topHeight);
        ctx.strokeRect(pipe.x, pipe.topHeight + pipeGap, pipeWidth, canvas.height - pipe.topHeight - pipeGap);
        
        // Detalhes nos canos (círculos decorativos)
        ctx.fillStyle = '#005500';
        for (let i = 0; i < pipe.topHeight; i += 15) {
            ctx.beginPath();
            ctx.arc(pipe.x + pipeWidth/2, i + 10, 3, 0, Math.PI * 2);
            ctx.fill();
        }
        for (let i = pipe.topHeight + pipeGap; i < canvas.height; i += 15) {
            ctx.beginPath();
            ctx.arc(pipe.x + pipeWidth/2, i + 10, 3, 0, Math.PI * 2);
            ctx.fill();
        }
    });
}

// Atualizar posição do Mario
function updateMario() {
    // Aplicar gravidade
    mario.velocity += mario.gravity;
    mario.y += mario.velocity;
    
    // Limite superior
    if (mario.y < 0) {
        mario.y = 0;
        mario.velocity = 0;
    }
    
    // Game Over se sair pela parte inferior
    if (mario.y + mario.height > canvas.height) {
        gameRunning = false;
    }
}

// Atualizar canos
function updatePipes() {
    pipeTimer++;
    
    // Criar novo cano
    if (pipeTimer > pipeFrequency) {
        const minTopHeight = 50;
        const maxTopHeight = canvas.height - pipeGap - 50;
        const randomTopHeight = Math.random() * (maxTopHeight - minTopHeight) + minTopHeight;
        
        pipes.push({
            x: canvas.width,
            topHeight: randomTopHeight,
            scored: false
        });
        
        pipeTimer = 0;
    }
    
    // Mover canos e remover quando saem da tela
    pipes.forEach((pipe, index) => {
        pipe.x -= 5;
        
        // Adicionar pontuação quando Mario passa pelo cano
        if (pipe.x + pipeWidth < mario.x && !pipe.scored) {
            score++;
            pipe.scored = true;
            document.getElementById('score').textContent = score;
        }
        
        // Remover cano quando sai da tela
        if (pipe.x + pipeWidth < 0) {
            pipes.splice(index, 1);
        }
    });
}

// Detectar colisão
function checkCollision() {
    pipes.forEach(pipe => {
        // Se Mario atingir cano superior ou inferior
        if (
            mario.x < pipe.x + pipeWidth &&
            mario.x + mario.width > pipe.x &&
            (mario.y < pipe.topHeight || mario.y + mario.height > pipe.topHeight + pipeGap)
        ) {
            gameRunning = false;
        }
    });
}

// Loop do jogo
function gameLoop() {
    // Desenhar fundo da imagem escalada para cobrir toda a tela
    if (backgroundImage.complete) {
        ctx.drawImage(backgroundImage, 0, 0, canvas.width, canvas.height);
    } else {
        // Se imagem não carregou, usar gradiente
        let gradientBg = ctx.createLinearGradient(0, 0, 0, canvas.height);
        gradientBg.addColorStop(0, '#87CEEB');
        gradientBg.addColorStop(0.5, '#E0F0FF');
        gradientBg.addColorStop(1, '#FFD700');
        ctx.fillStyle = gradientBg;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
    }
    
    if (gameRunning) {
        updateMario();
        updatePipes();
        checkCollision();
    }
    
    drawPipes();
    drawMario();
    
    // Game Over
    if (!gameRunning) {
        document.getElementById('gameOver').style.display = 'block';
        document.getElementById('finalScore').textContent = score;
    }
    
    requestAnimationFrame(gameLoop);
}

// Controles
function jump() {
    if (gameRunning) {
        mario.velocity = mario.jump;
        mario.isJumping = true;
    }
}

// Eventos de teclado e mouse
document.addEventListener('keydown', (e) => {
    if (e.code === 'Space') {
        e.preventDefault();
        jump();
    }
});

canvas.addEventListener('click', jump);


// Iniciar jogo
gameLoop();

// Mensagem de boas-vindas no console
console.log('Clique na tela ou pressione ESPAÇO para pular!');