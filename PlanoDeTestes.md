# 📦🍃 Iveco Green Ledger – Plano de Testes

 <div class="logo-container" align="center">
 <img src="imagens/logo-plano-de-teste.png" alt="Logo Firebase Firestore" class="logo-img" style="height: 350px; width: auto; vertical-align: middle; margin-left: 15px;">
</div>

## **Informações Gerais**

| Parâmetro | Requisito Mínimo |
| :--- | :--- |
| Nome do Sistema | Iveco Green Ledger |
| Versão | 1.0.0 (Release Operacional Base - .NET 8 |
| Equipe Responsável| Alice Andrade, Erick Silva, Nicolas Oliveira e Vinicius Augusto|
| Data do Planejamento| 08 - 12 de Agosto de 2026 |
| Equipe Responsável| IVECO |

---

## **Objetivos dos Testes**

O processo de garantia de qualidade do **Iveco Green Ledger** visa 

**O que podemos verificar?**

  - **Integridade Operacional nas Estações de Trabalho:** Validar a usabilidade, as regras de validação de formulários e a estabilidade do aplicativo desktop WPF.
  - **Desempenho e Cache Local (SQLite):** Confirmar a utilização do **SQLite** como banco relacional leve em borda, atuando estritamente como cache local para otimizar o tempo de resposta em consultas frequentes (`ex: CNPJ's e Chassis/Vin's já pesquisados`).
  - **Tratamento de Indisponibilidade de Conexão:** Validar se o sistema identifica a ausência de conexão com a internet ou com a Web API e exibe alertas imediatos e amigáveis ao operador, impedindo a perda de dados ou inconsistências.
  - **Consistência do Banco de Dados Relacional:** Testar a persistência, integridade referencial e transações ACID no SQL Server.
  - **Integrações com API's:** Homologar as camadas e tratamentos de falha para a BrasilAPI, NHTSA API, Google Maps e API Mercado Livre.
  - **Precisão dos Cálculos e Relatórios:** Garantir a exatidão no processamento das emissões de CO2 e na emissão de relatórios.

**Quais riscos pretendemos reduzir:**  

- **Paralisação e Travamentos da Interface:** Mitigar congelamentos da tela WPF em casos de oscilação ou tempo de resposta elevado das API's externas.
- **Inconsistência no Cadastro de Suprimentos:** Evitar a gravação de peças atreladas a Chassis inválidos ou fornecedores que não estão cadastrados.
- **Envio Mudo de Requisições Sem Rede:** Garantir que o sistema notifique o operador imediatamente se a internet cair, evitando tentativas de salvamento sem comunicação com o servidor central.

**O que será considerado como evidência de qualidade**

- **Relatórios de Testes Unitários e de Integração:** Mitigar congelamentos da tela WPF em casos de oscilação ou tempo de resposta elevado das API's externas.
- 

