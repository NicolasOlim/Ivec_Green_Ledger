# 📦🍃 Iveco Green Ledger – Plano de Migração

 <div class="logo-container" align="center">
    <img src="imagens/imagemmigração.webp" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

## **1. Dados do Sistema**

Esta seção estabelece as informações fundamentais de identificação do software, contextualizando o escopo da aplicação e o ambiente de destino no processo de migração de dados da Iveco:

| Parâmetro | Detalhe / Valor | 
| :--- | :--- | 
| **Nome do Sistema** | Iveco Green Ledger |   
| **Versão** | Release Operacional Base (.NET 8) | 
| **Domínio de Aplicação** | Triagem logística de pátio, rastreabilidade de suprimentos e cálculo automatizado de pegada de carbono Escopo 3 (GHG Protocol) |
| **Cliente Final** | IVECO (Portaria Logística e Gestão ESG) | 
| **Ambiente Alvo da Migração** | Operações da fábrica e base de dados de fornecedores/clientes da Iveco | 

---

## **2. Banco Utilizado**
### 2.1 Visão Geral da Arquitetura de Dados
O sistema Iveco Green Ledger adota uma arquitetura de banco de dados híbrido, combinando persistência em nuvem (NoSQL) com armazenamento em borda. Essa abordagem foi projetada para atuar diretamente nas demandas críticas de ambientes industriais e pátios logísticos, garantindo alta disponibilidade, sincronização em tempo real, consistência de dados e resiliência a falhas de conectividade.

### 2.2 Tecnologias Selecionadas e Justificativa

**- Firebase Firestore**

**- SQLITE**
