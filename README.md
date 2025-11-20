# Argus - Projeto de Estudo em Clean Architecture

## Objetivo do Projeto

Este repositório contém o código-fonte do Argus, um projeto desenvolvido com fins estritamente educacionais. O objetivo principal foi exercitar e demonstrar a implementação prática dos princípios da Clean Architecture (Arquitetura Limpa) utilizando o ecossistema .NET moderno.

O sistema simula um ambiente de monitoramento remoto de hardware, servindo como cenário para a aplicação de padrões de design, desacoplamento de componentes e comunicação em tempo real. Não se trata de um produto comercial, mas de uma prova de conceito técnica para estudo de arquitetura de software.

O sistema é composto por três partes distintas para simular um ambiente distribuído:

**Backend (ArgusCloud):** API central e Hub de comunicação.

**Client (Argus.Agent):** Serviço de coleta de dados em background.

**Frontend:** Interface de visualização dos dados.

## Estrutura Arquitetural

A organização da solução Backend (ArgusCloud) segue a separação de responsabilidades proposta pela Clean Architecture, garantindo que o núcleo da aplicação não dependa de frameworks ou tecnologias externas.

### Camadas da Solução

**Domain Layer (ArgusCloud.Domain):** Camada central e pura. Contém as Entidades (Usuario, Maquina) e as Interfaces que definem os contratos de repositório. O objetivo é manter a lógica de negócio agnóstica a banco de dados ou protocolos web.

**Application Layer (ArgusCloud.Application):** Responsável pela orquestração dos fluxos de dados. Implementa o padrão CQRS (Command Query Responsibility Segregation) utilizando a biblioteca MediatR para separar operações de leitura e escrita, utilizando DTOs para transporte de dados.

**Infrastructure Layer (ArgusCloud.Infrastructure):** Contém as implementações concretas dos contratos definidos no Domínio. Nesta camada residem o contexto do Entity Framework Core, configurações do provedor MySQL e implementações de serviços de infraestrutura.

**Presentation Layer (ArgusCloud.API):** Ponto de entrada da aplicação. Responsável pela exposição de endpoints via Controllers, configuração do contêiner de Injeção de Dependência (DI) e gerenciamento de conexões via SignalR Hubs.

## Tecnologias e Padrões

Este projeto utiliza as seguintes tecnologias e bibliotecas:

### Backend & Core

ASP.NET Core

.NET 9: Framework base para a API e bibliotecas de classe.

Clean Architecture: Estilo arquitetural adotado.

CQRS & Mediator: Padrão de design implementado com a biblioteca MediatR.

SignalR: Utilizado para comunicação bidirecional em tempo real (WebSockets) entre o servidor, o agente e o frontend.

Entity Framework Core: ORM utilizado com provedor Pomelo para MySQL.

JWT & BCrypt: Implementação de autenticação via tokens e hashing de senhas.

Mapster: Biblioteca para mapeamento de objetos (DTOs para Entidades).

### Client (Agente)

.NET 8 Worker Service: Serviço de execução contínua (Windows Service / Linux Daemon).

System.Diagnostics: Utilizado para coleta de métricas de processos e hardware.

ProtectedData: Utilizado para criptografia de credenciais locais (DPAPI).

### Frontend

Vue.js 3: Framework JavaScript progressivo utilizando Composition API.

TypeScript: Superset de JavaScript para tipagem estática.

Vite: Ferramenta de build e servidor de desenvolvimento.

Pug: Template engine para HTML.

BeerCSS: Framework CSS para estilização.

## Funcionalidades Implementadas

**Monitoramento de Processos:** O agente coleta os 20 processos com maior consumo de memória e envia ao servidor periodicamente.

**Comunicação em Tempo Real:** O Dashboard é atualizado instantaneamente via SignalR assim que o servidor recebe dados do agente.

**Autenticação Híbrida:**

Usuários autenticam-se via JWT para acesso ao painel.

Agentes utilizam um fluxo de registro com token persistente vinculado à máquina.

**Comandos Remotos:** Capacidade do servidor enviar instruções para o agente (ex: habilitar/desabilitar funcionalidades remotamente).

## Instruções para Execução

### Pré-requisitos

.NET SDK 9.0 (Backend)

.NET SDK 8.0 (Agente)

Node.js (v20 ou superior)

Servidor MySQL

### Configuração do Backend

Acesse o diretório backend/ArgusCloud.

Configure a string de conexão com o banco de dados no arquivo appsettings.json.

Aplique as migrações do banco de dados:

Bash

dotnet ef database update --project ../ArgusCloud.Infrastructure
Inicie a API:

Bash

dotnet run --project ArgusCloud.API

### Execução do Frontend

Acesse o diretório frontend.

Instale as dependências e inicie o servidor:

Bash

npm install
npm run dev

### Execução do Agente (Simulação)

Acesse o diretório backend/Argus.Agent.

Na primeira execução, registre o agente (simulação de instalação e vínculo):

Bash

dotnet run -- --register
Após o registro, inicie o serviço de monitoramento:

Bash

dotnet run
Este código foi desenvolvido para fins de aprendizado e demonstração de arquitetura.
