# 📦🍃 Iveco Green Ledger – Manual do Usuário

<div align="center">
  <img src="imagens/icone_manual.jpg" alt="Logo Iveco Green Ledger" width="200px">
</div>

---

**Documentação Operacional**  
Este manual destina-se aos operadores de estações de trabalho e portarias da **IVECO**, cobrindo a navegação, cadastros operacionais, integrações de APIs externas e a visualização de dados de governança ambiental (ESG) em nuvem por meio da aplicação desktop WPF.

---

## 1. Apresentação e Objetivo do Sistema

O **Iveco Green Ledger** é uma solução corporativa desenvolvida para gerenciar a **triagem logística de pátio**, a **rastreabilidade de suprimentos** e o **cálculo automatizado da pegada de carbono**, alinhado ao **Escopo 3 do GHG Protocol** (*Greenhouse Gas Protocol*).

O objetivo principal da plataforma é integrar a operação fabril física aos compromissos globais de sustentabilidade da IVECO, fornecendo um ambiente centralizado e auditável onde:

- **Qualificação Ambiental:** Fornecedores são avaliados e classificados segundo suas práticas ESG.
- **Controle de Entrada:** A recepção de componentes e matérias-primas é monitorada desde a portaria.
- **Rastreabilidade Unitária:** A montagem dos veículos é acompanhada peça a peça via número de chassi (VIN).
- **Consolidação em Tempo Real:** As emissões de Gases de Efeito Estufa (GEE) são calculadas e consolidadas instantaneamente em ledger distribuído/nuvem.

---

## 2. Requisitos de Acesso e Conectividade

| Requisito | Especificação Mínima |
| :--- | :--- |
| **Sistema Operacional** | Windows 10 (64-bit) ou superior |
| **Runtime / Framework** | .NET 8.0 Desktop Runtime instalado |
| **Conectividade** | Conexão ativa e estável com a internet (obrigatória para consultas a APIs e Ledger) |
| **Credenciais** | E-mail e senha previamente autorizados pela administração do sistema |

---

## 3. Guia Operacional e Navegação

### 3.1. Autenticação e Primeiro Acesso

O acesso ao sistema é restrito para garantir a segurança dos dados logísticos e industriais.

1. Abra a aplicação **Iveco Green Ledger** em sua estação de trabalho.
2. Na tela de login, preencha os campos **E-mail** e **Senha**.
3. Clique no botão **Entrar**.
4. O sistema consultará a Web API de autenticação para validar o perfil e liberar os privilégios de acesso correspondentes.

<div align="center">
  <img src="imagens/login.png" alt="Tela de Login do Sistema" width="600px">
</div>

---

### 3.2. Módulo de Gestão de Fornecedores

Módulo dedicado ao cadastramento, consulta automatizada de dados cadastrais e atribuição de classificação ambiental dos parceiros comerciais na rede permissionada.

#### Passo a Passo para Cadastro e Consulta via Receita Federal:

1. No menu lateral de navegação, clique na opção **Fornecedores**.
2. Na tela **Gestão de Fornecedores**, informe o **CNPJ** da empresa no campo de busca (formato: `00.000.000/0000-00`).
3. Clique no botão azul **Consultar CNPJ** para disparar a integração com a API da Receita Federal (BrasilAPI).
4. O sistema preencherá automaticamente os campos:
   - **Razão Social**
   - **Endereço Sede**
   - **Status RFB** (Situação Cadastral)
5. Revise os dados e confirme o registro do fornecedor no Ledger.

<div align="center">
  <img src="imagens/fornecedor.png" alt="Módulo de Gestão de Fornecedores" width="600px">
</div>

---

### 3.3. Módulo de Peças e Componentes (Rastreabilidade)

Utilizado no pátio logístico e nas etapas de montagem para vincular cada peça física ao seu respectivo veículo por meio do Chassi (VIN).

<div align="center">
  <img src="imagens/gestao_de_componentes.png" alt="Gestão de Componentes" width="600px">
</div>

#### Passo a Passo para Associação de Peça ao Veículo (VIN):

1. No menu lateral, acesse a opção **Peças e Componentes**.
2. Na seção de cadastro, preencha o formulário:
   - **Chassi do Veículo (VIN):** Selecione o código de 17 caracteres do veículo (ex.: `ZCFA1E0200812345`).
   - **Fornecedor:** Selecione a empresa responsável pela peça no menu suspenso (ex.: `ROBERT BOSCH LTDA`).
   - **Nome / Descrição da Peça:** Informe a identificação do componente (ex.: *Volante, Virabrequim, Cabeçote, Bloco do Motor*).
   - **Peso da Peça (kg):** Digite o peso líquido do componente em quilogramas (ex.: `65.00`).
3. Clique no botão verde **+ Registrar Peça**.

#### Histórico e Confirmação de Gravado:

- No painel **Últimas Peças Registradas**, acompanhe o histórico dos componentes associados e o contador de peças atreladas ao veículo.
- A presença da tag verde **`Gravado no Ledger`** confirma que o item foi persistido na rede auditável com sucesso.

<div align="center">
  <img src="imagens/RASTREABILIDADEPRINT.png" alt="Histórico de Rastreabilidade" width="600px">
</div>

---

### 3.4. Painel de Análises ESG

1. No menu lateral de navegação, selecione **Análises ESG**.
2. A tela apresentará gráficos consolidados, indicadores de emissões de CO₂ equivalente por veículo e relatórios de conformidade dos fornecedores.

<div align="center">
  <img src="imagens/ESG.png" alt="Painel de Análises ESG" width="600px">
</div>

---

## 4. Solução de Problemas e Perguntas Frequentes (FAQ)

<div align="center">
  <img src="imagens/logo-faq.webp" alt="FAQ e Suporte" width="200px">
</div>

### O CNPJ não é preenchido automaticamente ao clicar em "Consultar CNPJ"
* **Causa Provável:** Instabilidade momentânea no serviço da BrasilAPI ou formatação incorreta do CNPJ.
* **Procedimento:**
  - Verifique se o CNPJ digitado contém exatamente 14 dígitos numéricos (`00.000.000/0000-00`).
  - Acesse a tela **Dashboard** para verificar se o status da integração com a BrasilAPI está online.
  - Caso a integração esteja indisponível, faça o preenchimento manual dos campos **Razão Social** e **Endereço Sede**.

---

### A validação do Chassi (VIN) falhou ou retornou erro
* **Causa Provável:** O código possui quantidade de caracteres incorreta, contém caracteres inválidos ou a API da NHTSA está inacessível.
* **Procedimento:**
  - Certifique-se de que o VIN possui exatamente **17 caracteres alfanuméricos**.
  - Lembre-se de que as letras **I**, **O** e **Q** **não são utilizadas** em números de chassi para evitar confusão com os numerais `1` e `0`.
  - Verifique no **Dashboard** se o indicador da API NHTSA está ativo (verde).

---

### O endereço do Fornecedor não é sugerido no campo "Endereço Sede"
* **Causa Provável:** Oscilação de conexão com a API do Google Places ou limite de requisições excedido.
* **Procedimento:**
  - Digite o endereço completo manualmente (logradouro, número, bairro, cidade e UF).
  - Se o status da API do Google no Dashboard indicar inatividade, conclua o preenchimento manual antes de clicar em **Registrar no Ledger**.

---

### Não recebi o e-mail de acesso ou há falha na autenticação
* **Causa Provável:** Credenciais digitadas incorretamente ou bloqueio por regras de firewall/spam do servidor de e-mail.
* **Procedimento:**
  - Confirme a digitação exata do **Endereço de E-mail** e **Senha**.
  - Verifique a pasta de **Lixo Eletrônico / Spam** da sua caixa de entrada.
  - Se o problema persistir, solicite o reenvio de credenciais à equipe de administração do sistema.

---

### Um ou mais indicadores de API aparecem com luz vermelha no Dashboard
* **Causa Provável:** Oscilação na conexão de internet da estação de trabalho ou indisponibilidade temporária nos serviços externos (BrasilAPI, Google Places, NHTSA, Mercado Livre).
* **Procedimento:**
  - Verifique os cartões **Falha de Integração** e **Tempos de Resposta (API)** no **Dashboard**.
  - Confirme se a estação de trabalho possui conexão ativa com a rede/internet.
  - O sistema permite o preenchimento manual contingencial até a normalização dos serviços.

---

### Não consigo registrar um componente na tela "Peças e Componentes"
* **Causa Provável:** O fornecedor da peça não foi cadastrado previamente ou algum campo obrigatório não foi selecionado.
* **Procedimento:**
  - Acesse a tela **Fornecedores** no menu lateral e confirme se o fornecedor já consta gravado no Ledger.
  - Volte à tela **Peças e Componentes** e selecione obrigatoriamente o **Chassi do Veículo (VIN)** e o **Fornecedor** nos menus suspensos antes de clicar em **+ Registrar Peça**.

---

### A peça cadastrada não é listada em "Últimas Peças Registradas"
* **Causa Provável:** Instabilidade momentânea no banco de dados do Ledger ou necessidade de atualização visual da interface.
* **Procedimento:**
  - Certifique-se de ter informado a **Descrição da Peça** e o **Peso da Peça (kg)**.
  - Após clicar em registrar, verifique se o card do item exibe a tag verde **`Gravado no Ledger`**.

---

### Preciso relatar uma falha técnica ou solicitar suporte
* **Causa Provável:** Ocorrência de erro não previsto, divergência em dados cadastrais ou necessidade de manutenção na estação.
* **Procedimento:**
  - Acesse o menu **Dashboard** e navegue até o painel **Registrar Solicitação de Suporte**.
  - Selecione o **Tipo de Problema** (Ex.: *Erro de API, Falha no Ledger, Dúvida Operacional*), descreva ocorrido e clique em **Enviar Chamado**.

---

*Documento elaborado para o Projeto de TCC – SENAI Nova Lima, conforme atividade prática de Infraestrutura de Software.*
*Última atualização: 06/08/2026.*
