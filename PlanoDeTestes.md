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

O processo de garantia de qualidade do **Iveco Green Ledger** visa assegurar a confiabilidade, o desempenho e a precisão técnica da aplicação durante sua operação nas estações de trabalho e áreas de triagem da fábrica. Por se tratar de um sistema corporativo que exige conectividade ativa com a internet e com a Web API central para a validação de dados e registro no Ledger, os testes focam em garantir que as rotinas funcionem perfeitamente em ambiente online e que qualquer ausência de rede seja identificada e tratada de forma clara e segura pelo sistema.

---

**O que podemos verificar?**

  - **Integridade Operacional nas Estações de Trabalho:** Validar a usabilidade, as regras de validação de formulários e a estabilidade do aplicativo desktop WPF.
  - **Desempenho e Cache Local (SQLite):** Confirmar a utilização do **SQLite** como banco relacional leve em borda, atuando estritamente como cache local para otimizar o tempo de resposta em consultas frequentes (`ex: CNPJ's e Chassis/Vin's já pesquisados`).
  - **Tratamento de Indisponibilidade de Conexão:** Validar se o sistema identifica a ausência de conexão com a internet ou com a Web API e exibe alertas imediatos e amigáveis ao operador, impedindo a perda de dados ou inconsistências.
  - **Consistência do Banco de Dados Relacional:** Testar a persistência, integridade referencial e transações ACID no SQL Server.
  - **Integrações com API's:** Homologar as camadas e tratamentos de falha para a BrasilAPI, NHTSA API, Google Maps e API Mercado Livre.
  - **Precisão dos Cálculos e Relatórios:** Garantir a exatidão no processamento das emissões de CO2 e na emissão de relatórios.

---

**Quais riscos pretendemos reduzir:**  

- **Paralisação e Travamentos da Interface:** Mitigar congelamentos da tela WPF em casos de oscilação ou tempo de resposta elevado das API's externas.
- **Inconsistência no Cadastro de Suprimentos:** Evitar a gravação de peças atreladas a Chassis inválidos ou fornecedores que não estão cadastrados.
- **Envio Mudo de Requisições Sem Rede:** Garantir que o sistema notifique o operador imediatamente se a internet cair, evitando tentativas de salvamento sem comunicação com o servidor central.

---

**Mapeamento e Mitigação de Riscos do Projeto**

| Risco Identificado | Impacto Operacional | Ação Migratória pelo Plano de Testes | 
| :--- | :--- | :--- |
| Lentidão ou oscilação de rede | Alto | Validação da consulta em cache local (SQLite) para evitar requisições desnecessárias |
| Queda total de conectividade | Crítico | Teste dos bloqueios de interface e exibição de avisos de que a operação requer conexão ativa |
| Entrada de chassis inválidos| Alto | Teste automatizado de borda com rejeição de caracteres proibidos (`"I", "O", "Q"`) e tamanhos diferentes de 17 dígitos.
| Cadastros de fornecedores incompletos | Médio | Testes de validação de formulário obrigando o preenchimento da Categoria Ambiental antes de registrar no ledger |
| Erros de cálculo de indicadores ambientais| Crítico | Testes unitários de fórmula para validar fatores de emissão por kg de insumo recebido |

---

**O que será considerado como evidência de qualidade**

  - **Evidências visuais ->** Telas operacionais exibindo mensagens de alerta de rede e tags de confirmação (**Sincronizado** na rastreabilidade e **Gravado no Ledger** na Gestão de Componentes.
  - **Eficiência do cache local ->** Comprovação do ganho de velocidade no carregamento de telas através do cache de dados no SQLite local.

---

## **Escopo**

**O que será testado**

  - **Interface do usuário (WPF / .NET8):** Módulos de autenticação, fornecedores, peças e componentes, rastreabilidade, dashboard e análises de sustentabilidade.
  - **Backend e serviço (Web API / ASP .NET Core):** Endpoints, validações de DTO's, autenticação de tratamento de erro.
  - **Persistência de dados e cache:** Operações CRUD no **SQL Server** e leitura automatizada via **SQLite**.
  - **Integrações de rede:** Comunicação via HTTPS com BrasilAPI, NHTSA, Google Maps e Mercado Livre.
  - **Exportação de documentos:** Geração de relatórios em PDF.

**O que não será testado**

   - **Infraestrutura interna dos provedores de API:** A disponibilidade e estabilidade dos servidores próprios de parceiros externos.
   - **Sistemas operacionais incompatíveis:** Execução da aplicação desktop em sistemas anteriores ao Windows 10 (64-bit).
   - **Testes de carga:** Testes de estresse com volumes de requisições que não condizem com o fluxo operacional da IVECO.

**Funcionalidades Prioritárias**

| Prioridade | Módulo / Funcionalidade | Descrição e Foco | 
| :--- | :--- | :--- |
| P1 (Crítica) | Autenticação e permissões | Validação de login, perfil de acesso e consumo seguro dos endpoints da Web API |
| P1 (Crítica) | Consulta CNPJ | Validação automática da BrasilAPI, tratamento de digitação e salvamento |
| P1 (Crítica) | Cadastro de peças | Validação com 17 dígitos númericos, vinculação com fornecedor e peso em kg |
| P2 (Alta) | Validação de conectividade | Garantia de respostas rápidas via cache local no SQLite |
| P2 (Alta) | Cálculo de pegada de carbono | Processamento e exbição gráfica das emissões acumulados por componente / veículo |
| P3 (Média) | Relatórios em PDF | Exportação de auditoria em PDF |

---

## **Base de Teste**

A elaboração dos testes é fundamentada nos seguintes artefatos e especificações técnicas do projeto:

  - **Manual do Usuário:** O `Manual_do_Usuario.md` é a especificação das telas, formulários, botões, requisitos de conectividade obrigatória e guia de solução de problemas.
  - **Documento de infraestrutura:** A `infraestrutura.md` é os requisitos de hardware, pacotes NuGet, comunicação via HTTPS e integrações com API's.
  - **Plano de Migração e Banco de Dados:** A `Migracao.md` é a estrutura de tabelas relacionais do SQL Server, regras ACID e comportamento do cache de navegação em SQLite.
  - **Termos de Licenciamento:** O `Licenciamento.md` é as regras de uso, controle de acessos, privacidade e controle de versão.

---

## **Abordagem de testes**

  - **Testes funcionais:** Verificação do comportamento das telas WPF, validações de campos obrigatórios, navegação pelos menus e execução das ações principais (consultar, registrar, filtrar e exportar).
  - **Testes não funcionais:** Avaliação da usabilidade da interface gráfica, verificação do ganho de velocidade via cache SQLite e tempo de resposta nas consultas de busca.
  - **Testes de integração:** Homologação da comunicação entre a aplicação WPF, Web API, SQL Server e serviços das API's de terceiros.
  - **Testes de sistema:** Validação do fluxo completro de ponta a ponta: desde a qualificação do fornecedor por CNPJ, passando pelo vínculo da peça ao VIN do veículo, até a exbição da métrica ambiental no Dashboard e emissão em PDF.
  - **Testes de aceitação:** Simulação de uso contínuo por operadores logísticos para validar a aderência do software aos procedimentos práticos de recebimento e triagem da IVECO.
  - **Testes exploratórios:** Navegação livre e envio de entradas não padrão no aplicativo WPF para identificar comportamentos imprevistos e exceções não tratadas visualmente.
  - **Testes baseados em cenários:** Execução de rotinas baseadas no cotidiano operacional, tais como instabilidade temporária na consulta á receita federal ou interrupção de sinal de rede na estação.

---

**Ciclo de correção**

O reteste consiste na reexcução direta de um cenário específico que havia falhado previamente, visando validar se um bug pontual foi efetivamente corrigido pela equipe de desenvolvimento, enquanto os testes de regressão envolvem a execução da suíte de testes completa ou de módulos após qualquer correção ou inclusão de nova funcionalidade, garantindo que as alterações no código não introduzam novos defeitos em partes do sistema que já funcionavam perfeitamente.

---

## **Critérios de entrada e saída**

**Critérios de entrada**

  - Código fonte da versão a ser testada devidamente compilado e sem erros de build.
  - Ambiente de teste configurado com o banco SQL Server e SQLite inicializados com dados de homologação.
  - Aplicação WPF instalada ou executável gerado na estação de testes.
  - Web API ativa e respondendo nos endpoints de teste.
  - Casos de testes serão documentados e revisado neste mesmo arquivo.

**Critérios de saída**

  - Grande maioria dos casos de teste serão classificados como Prioridade 1 (P1).
  - Todas as evidências de testes serão devidamente anexadas á documentação de entrega.

---

## **Técnicas de projetos de testes**

 - **Particionamento de equivalência:** Divisão das entradas em classes válidas e inválidas para otimizar a cobertura de testes.
   * **Aplicação ->** CNPJ com 14 dígitos válidos e formatos com dígitos ou caracteres inválidos, VIN's com exatamente 17 caracteres e/ou tamanhos incorretos.

 - **Análise de valor:** Testes nos limites e bordas dos campos de entrada de dados.
   * **Aplicação ->** Testando valores negativos, pesos elevados e campo de VIN com entradas de 16, 17 e 18 caracteres.

   - **Tabela de decisão:** Mapeamento de combinações de condições complexas de entrada e conectividade com suas respectivas ações do sistema.
   * **Regras Mapeadas ->** Se API Externa estiver online e ativa haverá preenchimento automático via API. Caso a API estiver offline e ativa exibirá um alereta e liberará o preenchimento manual, já se a rede estiver inativa exibirá um alerta de falha de conexão e bloqueia o salvamento até o restabelecimento da internet.

   - **Testes baseados em cenários:** Construção de sequências de ações fundamentais no cotidiano de uso das estações de recebimento na fábrica da IVECO.

   - **Testes exploratórios:** Sessões estruturadas sem roteiro pré-definido focadas em encontrar falhas de usabilidade na interface XAML e exceções visuais não capturadas.

  ---

  ## **Modelo e matriz de casos de teste**

  Para a documentação e acompanhamento das validações do sistema conforme a evolução das entregas, adota-se o seguinte padrão estruturado de mapeamento de cenários de teste:

| ID | Funcionalidade | Cenário | Entrada | Resultado esperado | Resultado obtido | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---|
| CT - 001 | - | - | - | - | - | - |
| CT - 002 | - | - | - | - | - | - |
| CT - 003 | - | - | - | - | - | - |
| CT - 004 | - | - | - | - | - | - |
| CT - 005 | - | - | - | - | - | - |

---

*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 10 de agosto de 2026.*
