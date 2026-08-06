# 📦🍃 Iveco Green Ledger – Termos de Licenciamento

 <div class="logo-container" align="center">
    <img src="imagens/logo-licenciamento.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

Este documento define os termos de uso, os direitos de propriedade intelectual e a política de versionamento e releases do projeto.

## **1. Informações Gerais**

Esta seção estabelece as informações fundamentais de identificação do software, contextualizando o escopo da aplicação e o ambiente de destino para a definição das diretrizes de licenciamento corporativo, uso de componentes de terceiros e controle de releases do projeto.

- **Nome do Sistema:** Iveco Green Ledger.
- **Versão Atual:** 1.0.0 (Release Operacional Base).
- **Tipo de Software:** Aplicação corporativa distribuída (Desktop Cliente WPF + Web API ASP.NET Core).
- **Proprietário / Cliente Final:** IVECO.
- **Ambiente de Destino:** Automação de triagem física, validação de veículos e monitoramento de emissões de CO2.
- **Modelo de Arquitetura:** Execução local híbrida em borda (SQLite) com consolidação de dados em nuvem pública (Googel Firebase Firestore).

---

## **2. Termos de Licenciamento**

O projeto é regido pelos seguintes termos de uso e propriedade intelectual.

- **Uso Proprietário Exclusivo:** O software foi desenvolvido sob medida para atendimento das demandas logísticas e de governança ambiental da montadora IVECO. O código-fonte, marcas, arquitetura e binários compilados constituem propriedade intelectual restrita.
- **Restrições de Distribuição:** É expressamente proibida a cópia, redistribuição comercial, sublicenciamento, modificação por terceiros não autorizados ou engenharia reversa das aplicações desktop (WPF) e serviços de backend (API REST).
- **Controle de Acesso e Operação:** A utilização da aplicação por operadores de triagem é controlada individualmente via autenticação de perfil na nuvem sendo intransferível.

## **3.Bibliotecas e Componentes**

O ecossistema utiliza dependências e bibliotecas de código aberto para viabilizar funcionalidades específicas. Todas as bibliotecas integradas operam sob licenças permissivas que autorizam o uso corporativo:

| Componente / Biblioteca | Licença de Terceiros | Finalidade no projeto |
| :--- | :--- | :--- |
| **.NET 8 SDK / WPF** | MIT License | Framwework base para desenvolvimento do cliente desktop e da API |
