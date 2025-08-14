# WebRTC Screen Sharing Platform

Uma plataforma completa de compartilhamento de tela em tempo real com C# e WebRTC, featuring sistema de IDs de usuário, múltiplas interfaces e gravação automática.

## 🚀 Funcionalidades Principais

- **Sistema de IDs**: Cada usuário tem um ID personalizado para identificação
- **Duas Interfaces Distintas**:
  - **Viewer**: Para assistir compartilhamentos de outros usuários
  - **Sharer**: Para compartilhar sua própria tela
- **Usuários Online**: Visualização em tempo real de quem está conectado
- **Compartilhamento de Tela**: Transmissão em alta qualidade usando WebRTC
- **Gravação por Usuário**: Gravações organizadas por ID do usuário
- **Reprodução Offline**: Acesso a gravações anteriores
- **Comunicação em Tempo Real**: SignalR para atualizações instantâneas
- **API REST**: Endpoints para gerenciar gravações

## 🛠️ Como Usar

### 1. Executar o Servidor C# (Backend)
```bash
cd WebRtc3
dotnet run
```
O servidor WebRTC estará disponível em `http://localhost:5000`

### 2. Executar o Servidor HTTP (Frontend)
```bash
cd client
python -m http.server 8000
```
O cliente web estará disponível em `http://localhost:8000`

### 3. Acessar a Plataforma

#### Página Inicial
Acesse `http://localhost:8000` para escolher:
- **Viewer**: Assistir compartilhamentos de outros usuários
- **Sharer**: Compartilhar sua própria tela

#### Como Viewer
1. Acesse `http://localhost:8000/viewer.html`
2. Veja a lista de usuários online
3. Clique em "Assistir" para ver compartilhamentos ativos
4. Acesse gravações anteriores na seção "Gravações"

#### Como Sharer
1. Acesse `http://localhost:8000/sharer.html`
2. Digite seu ID de usuário personalizado
3. Clique em "Registrar" para entrar online
4. Use "Compartilhar Tela" para iniciar a transmissão
5. Gerencie suas gravações na seção dedicada

**Importante**: Ambos os servidores devem estar rodando simultaneamente.

## Arquitetura

O projeto utiliza:
- **ASP.NET Core**: Framework web
- **SignalR**: Comunicação em tempo real
- **SIPSorcery**: Biblioteca WebRTC para .NET
- **FFMpegCore**: Processamento de vídeo para gravações

## 📁 Estrutura de Arquivos

### Servidor WebRTC (C#)
```
WebRtc3/
├── Program.cs              # Servidor principal com SignalR Hub
├── WebRtcServer.csproj     # Configuração do projeto
└── recordings/             # Gravações organizadas por usuário
```

### Cliente Web (HTML)
```
client/
├── index.html              # Página inicial de seleção
├── viewer.html             # Interface para visualizar
└── sharer.html             # Interface para compartilhar
```

## 📡 Endpoints do Servidor

### SignalR Hub (`/webrtchub`)
- **RegisterUser**: Registrar usuário com ID personalizado
- **GetOnlineUsers**: Obter lista de usuários online
- **Offer**: Enviar oferta WebRTC
- **Answer**: Enviar resposta WebRTC
- **IceCandidate**: Trocar candidatos ICE
- **StartStream**: Iniciar transmissão
- **StopStream**: Parar transmissão
- **OnlineUsersUpdated**: Notificação de mudanças na lista de usuários

### API REST
- **GET** `/api/recordings`: Listar todas as gravações
- **GET** `/api/recordings/{filename}`: Download de gravação específica

## 🎯 Recursos Implementados

✅ **Sistema de IDs Personalizados**: Cada usuário define seu próprio identificador  
✅ **Duas Interfaces Distintas**: Viewer para assistir, Sharer para compartilhar  
✅ **Lista de Usuários Online**: Visualização em tempo real de quem está conectado  
✅ **Status de Compartilhamento**: Indica quem está compartilhando ativamente  
✅ **Compartilhamento de Tela**: Usando `getDisplayMedia` em resolução 1280x720  
✅ **Gravação por Usuário**: Arquivos organizados por ID do usuário  
✅ **Reprodução Offline**: Interface para assistir gravações anteriores  
✅ **SignalR**: Comunicação bidirecional em tempo real  
✅ **Interface Responsiva**: Design moderno e intuitivo  
✅ **Arquitetura Separada**: Cliente e servidor totalmente independentes  

## 🔧 Vantagens da Nova Arquitetura

- **Identificação Única**: Sistema de IDs permite rastreamento e organização
- **Experiência Especializada**: Interfaces otimizadas para cada tipo de usuário
- **Gestão de Sessões**: Controle completo sobre usuários online e offline
- **Organização**: Gravações categorizadas por usuário
- **Escalabilidade**: Suporte a múltiplos viewers e sharers simultâneos
- **Flexibilidade**: Deploy independente de frontend e backend

O sistema está pronto para uso e pode ser facilmente expandido!