# 🐇 POC RabbitMQ

Proof of Concept (POC) para estudo e implementação de mensageria utilizando RabbitMQ com .NET/C#.

## 📋 Sobre o Projeto

Este projeto tem como objetivo demonstrar conceitos de comunicação assíncrona através do RabbitMQ, permitindo o envio e consumo de mensagens entre aplicações desacopladas.

A solução foi criada para fins de aprendizado, testes de arquitetura orientada a eventos e validação de padrões de mensageria.

## 🚀 Tecnologias Utilizadas

* .NET
* C#
* RabbitMQ
* Docker
* Docker Compose

## 📂 Estrutura do Projeto

```text
poc-rabbitmq/
│
├── src/
│   └── Projetos da aplicação
│
├── docker-compose.yml
│
└── poc-mensageria.slnx
```

## 📨 Conceitos Demonstrados

* Producer (Publicador)
* Consumer (Consumidor)
* Filas (Queues)
* Exchanges
* Roteamento de mensagens
* Comunicação assíncrona
* Desacoplamento entre serviços

## ⚙️ Pré-requisitos

Antes de executar o projeto, certifique-se de possuir:

* .NET SDK instalado
* Docker Desktop
* Git

## 🐳 Subindo o RabbitMQ

Execute:

```bash
docker-compose up -d
```

Após a inicialização:

* RabbitMQ: `localhost:5672`
* Painel de Administração: `http://localhost:15672`

Credenciais padrão (caso não tenham sido alteradas):

```text
Usuário: admin
Senha: admin
```

## ▶️ Executando a Aplicação

Clone o repositório:

```bash
git clone https://github.com/eliferretti/poc-rabbitmq.git
```

Acesse a pasta:

```bash
cd poc-rabbitmq
```

Restaure os pacotes:

```bash
dotnet restore
```

Execute o projeto desejado:

```bash
dotnet run
```

## 🔄 Fluxo da Aplicação

```text
Producer
    │
    ▼
 RabbitMQ
    │
    ▼
Consumer
```

1. O Producer publica uma mensagem.
2. O RabbitMQ recebe e armazena a mensagem na fila.
3. O Consumer processa a mensagem recebida.
4. A comunicação ocorre de forma assíncrona.

## 🎯 Objetivos da POC

* Entender o funcionamento do RabbitMQ.
* Demonstrar integração com aplicações .NET.
* Validar estratégias de mensageria.
* Servir como base para arquiteturas orientadas a eventos.

## 📚 Referências

* RabbitMQ Official Documentation: https://www.rabbitmq.com/
* .NET Documentation: https://learn.microsoft.com/dotnet/

## 👨‍💻 Autor

Desenvolvido por Eli Ferretti.

GitHub:
https://github.com/eliferretti
