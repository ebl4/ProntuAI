# ProntuAI (MedNote AI) - MVP

Este repositório contém um protótipo MVP de backend em .NET (minimal API) com endpoints mínimos para upload de áudio, transcrição mock, geração de prontuário no formato SOAP e um frontend estático simples para testes.

Como executar localmente

1. Requisitos
   - .NET 10 SDK
   - Visual Studio 2026 (opcional) ou dotnet CLI

2. Executar a API

```powershell
cd ProntuAI
dotnet run
```

3. Acessar frontend de testes

- Abra https://localhost:5001/ (ou URL exibida no console). A página inicial permite enviar arquivos de áudio (ou arquivos de texto para teste) e gerar um prontuário SOAP mock.

Sobre integrações reais

- A implementação atual usa serviços "mock" (MockTranscriptionService e MockLlmService) para permitir desenvolvimento sem depender de provedores externos.
- Para substituir por provedores reais (OpenAI/Whisper/Azure), implemente ITranscriptionService e ILlmService com chamadas HTTP para as APIs desejadas e registre as implementações no Program.cs.

Persistência

- Notas são salvas localmente em JSON no diretório temporário (%TEMP%/ProntuAI/notes) usando FileNotesRepository. Para produção, trocar por banco (Postgres/EF Core).

RAG

- Um InMemoryVectorStore simples foi implementado para POC. Para RAG real, integre um vector DB (Pinecone, Qdrant, Azure Cognitive Search).

Próximos passos sugeridos

- Implementar transcrição real (Whisper/Azure)
- Implementar LLM real com prompt para saída JSON SOAP
- Adicionar autenticação simples e persistência em Postgres
- Adicionar parsing de PDFs para indexação
