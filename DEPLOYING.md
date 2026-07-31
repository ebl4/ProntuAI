# Deploy para Azure App Service (guia rápido)

Este documento descreve passos básicos para publicar a API ProntuAI no Azure App Service.

1) Criar App Service (Windows) no Azure Portal com .NET 10 como runtime.

2) Publicar via Visual Studio
   - Abra a solução no Visual Studio
   - Clique com o botão direito no projeto ProntuAI > Publish
   - Escolha Azure > Azure App Service > Create New ou Select Existing

3) Variáveis de ambiente
   - Configure as chaves de API dos provedores (ex.: OPENAI_API_KEY, AZURE_OPENAI_KEY) em Configuration > Application settings no portal do App Service.

4) CI/CD (opcional)
   - Configure GitHub Actions para build e deploy automático usando a ação azure/webapps-deploy.

5) Observações
   - Substitua os serviços mock por implementações reais antes de enviar dados sensíveis.
   - Considere habilitar HTTPS obrigatório e Application Insights para monitoramento.
