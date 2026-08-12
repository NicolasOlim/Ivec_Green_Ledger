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

O processo de garantia de qualidade visa assegurar a confiabilidade, o desempenho e a precisão técnica da aplicação durante sua operação nas estações de trabalho e áreas de triagem da fábrica. Por se tratar de um sistema que exige conectividade ativa com a internet e com a Web API Central para a validação de dados e registro no Ledger, os testes focam em garantir que as rotinas funcionem perfeitamente em ambiente online e que qualquer ausência de rede seja identifcada e tratada de forma clara e segura pelo sistema.

---

**O Que Podemos Verificar?**

  - **Integridade Operacional nas Estações de Trabalho:** Validar a usabilidade, as regras de validação de formulários e a estabilidade do aplicativo desktop WPF.
  - **Desempenho e Cache Local (SQLite):** Confirmar a utilização do **SQLite** como banco relacional leve em borda, atuando estritamente como cache local para otimizar o tempo de resposta em consultas frequentes (`ex: CNPJ's e Chassis/Vin's`) já pesquisados.
  - **Tratamento de Indisponibilidade de Conexão:** Validar se o sistema identifica a ausência de conexão com a internet ou com a Web API e exibe alertas imediatos e amigáveis ao operador, impedindo a perda de dados ou inconsistências.
  - **Integrações com API's:** Homologar as camadas e tratamentos de falha para a BrasilAPI, NHTSA API, Google Maps e API Mercado Livre.
  - **Precisão dos Cálculos e Relatórios:** Garantir a exatidão no processamento das emissões de CO2 e na emissão de relatórios.

---

**Mapeamento e Mitigação de Riscos do Projeto:**  

| Risco Identificado | Impacto | Ação Migratória |
| :--- | :--- | :--- |
| Lentidão ou oscilação de rede | Alto | Utilização de cache local (SQLite) para consultas frequentes |
| Queda total de conectividade | Crítico | Teste dos bloqueios de interface e exibição de avisos claros|
| Entrada de chassis (VIN) inválidos | Alto | Testes automatizados de borda com rejeição (`I`, `O`, `Q`) e tamanhos diferentes de 17 dígitos |
| Cadastro de fornecedores incompletos | Médio | Testes de validação de formulário obrigando o preenchimento da categoria ambiental antes de registrar no ledger |
| Erros de cálculo de indicadores ambientais | Crítico | Testes unitários de fórmula matemática para validar fatores de emissão por kg de insumo recebido |

---

**O Que Será Considerado Como EvidÊncia de Qualidade?**

  - **Evidências visuais ->** Telas operacionais exibindo mensagens de alerta de rede e tags de confirmação (**Sincronizado** na rastreabilidade e **Gravado no Ledger** na Gestão de Componentes.
  - **Eficiência do cache local ->** Comprovação do ganho de velocidade no carregamento de telas através do cache de dados no SQLite local.
  - **Logs da aplicação:** Comprovação do ganho de velocidade no carregamento

---


## **Escopo**

**O que será testado**

  - **Interface do usuário (WPF / .NET8):** Módulos de Autenticação, Fornecedores, Peças e Componentes, Rastreabilidade, Dashboard e Análises de Sustentabilidade.
  - **Backend e serviço (Web API / ASP .NET Core):** Endpoints, validações de DTO's, autenticação de tratamento de erro.
  - **Persistência de dados e cache:** Operações CRUD no **SQL Server** e leitura automatizada via **SQLite**.
  - **Integrações de rede:** Comunicação via HTTPS com BrasilAPI, NHTSA, Google Maps e Mercado Livre.
  - **Exportação de documentos:** Geração de relatórios em PDF.
  - **Indicadores de conectividade:** Exibição do status das API's externas no Dashboard.

**O que não será testado**

   - **Infraestrutura interna dos provedores de API:** A disponibilidade e estabilidade dos servidores próprios de parceiros externos.
   - **Sistemas operacionais incompatíveis:** Execução da aplicação em desktop em sistemas anteriores ao Windows 10(64-bit).
   - **Testes de carga:** Testes com volumes de requisições que não condizem com o fluxo operacional da fábrica da IVECO.
   - **Modo offline completo:** O sistema não funciona sem internet, portanto não serão testados fluxos offline.

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

  - **Manual do Usuário:** Especificação das telas, formulários, botões, requisitos de conectividade e guia de solução de problemas.
  - **Documento de infraestrutura:** Requisitos de hardware, pacotes NuGet, comunicação via HTTPS e integrações com API's.
  - **Plano de Migração e Banco de Dados:** Estrutura de tabelas relacionais do SQL Server, regras ACID e comportamento do cache de navegação em SQLite.
  - **Termos de Licenciamento:** Regras de uso, controle de acessos, privacidade e controle de versão.
  - **Código-fonte:** Solução WPF em C# (.NET 8) e Web API ASP.NET Core.
  - **Documentação da API:** O Swagger/OpenAPI está disponível em: `https://apiivecogreenledger.runasp.net/swagger`.

---

## **Abordagem de testes**

| Abordagem | Descrição no Projeto | Testes Executados |
| :--- | :--- | :--- |
| Testes Funcionais | Verificação do comportamento das telas WPF, validações de campos obrigatórios, navegação pelos menus e execução das ações principais (consultar, registrar, filtrar e exportar) | CT-001, CT-002, CT-005, CT-006, CT-007, CT-008, CT-008, CT-009, CT-010, CT-011
| Testes Não Funcionais | Avaliação da usabilidade da interface, verificação do ganho de velocidade via cache SQLite e tempo de resposta nas consultas de busca | CT-012, CT-003 |
| Testes de Sistemas | Validação do fluxo completo de ponta a ponta: desde a qualificação do fornecedor por CNPJ, passando pelo vínculo da peça ao VIN do veículo, até a exibição da métrica ambiental no Dashboard | CT-001, CT-006, CT-008, CT-009, CT-011 |
| Testes de Integração | Homologação da comunicação entre a aplicação WPF, Web API, SQL Server e serviços das API's de terceiros (BrasilAPI, NHTSA, Google Maps e API Mercado Livre) | CT-003, CT-005, CT-008, CT-009 |
| Testes de Aceitação | Simulação de uso contínuo por operadores para validar a aderência do software aos procedimentos práticos de recebimento da IVECO | CT-001, CT-008, CT-009 |
| Testes Exploratórios | Navegação livre e envio de entradas não padrão no app WPF para identificar comportamentos imprevistos e exceções não tratadas visualmente | CT-007 e CT-010|
| Testes Baseados em Cenários | Execução de rotinas baseadas no cotidiano operacional, tais como instabilidade temporária na consulta á Receita Federal ou interrupção de sinal de rede | CT-006, CT-008 e CT-009|
| Reteste | Reexecução direta de um cenário específico que havia falhado previamente, visando validar se um bug pontual foi efetivamente corrigido | CT-011 |
| Regressão | Execução da suíte de testes após qualquer correção ou inclusão de nova funcionalidade, garantindo que as alterações não introduzam novos efeitos | CT-003 e CT-001 |

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

  ## **Casos de Teste**

| ID | Funcionalidade | Cenário | Entrada | Resultado esperado | Resultado obtido | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :---|
| CT - 001 | Login | Credenciais válidas | Usuário: `admin@iveco.com`, Senha: `Iveco@2026` | Sistema efetua login e exibe a aba `Dashboard` | Login realizado com sucesso e aba de dashboard carregada | - |
| CT - 002 | Login | Senha inválida | Usuário: `admin@iveco.com`, Senha: `SenhaErrada` | Sistema exibe mensagens de erro "Credenciais inválidas" e mantém usuário na tela de login | Mensagem de erro exibida corretamente e o usuário permanece na tela de login | - |
| CT - 003 | Dashboard | Status das API's externas | Navegar até o Dashboard e aguardar, pois tem uma atualização automática | Indicadores (BrasilAPI, NHTSA, MErcado Livre) ficam verdes indicando "sucesso" ou vermelhos indicando "falha" | Brasil API: Verde, Google Maps: Vermelho (por ser uma chave inválida), NHTSA: Verde e Mercado Livre: Vermelho | - |
| CT - 004 | Dashboard | Remoção do card "Falhas de integração" | Visualizar o Dashboard após alteração no código | O card "Falhas de Integração" não deve aparecer. Os outros 3 cards devem estar alinhado | Card removido com sucesso | - |
| CT - 005 | Fornecedores | Consulta CNPJ válido | Digitar CNPJ `00.000.000/0001-91` e clicar em "consultar" | Campos "Razão Social", "Endereço" e "Status RFB" são preenchidos. Categoria ESG é dado como "Não avaliado" | - | - |
| CT - 006 | Fornecedores | Salvar fornecedor | Após consulta, selecionar categoria e clicar em "Registrar no Ledger" | Sistema exibe "Fornecedor registrado com sucesso". E o fornecedor aparece na lista | Todos os campos preenchidos, Status FRB: `ATIVA` e Categoria ESG: `Não avaliado` | - |
| CT - 007 | Peças | Cadastro com dados incompletos | Deixar o campo "Nome da Peça" vazio e clicar em "Registrar" | MessageBox: "Preencha o nome da peça. O cadastro é bloqueado | Sucesso, fornecedor salvo na API e no SQLite local | - |
| CT - 008 | Fornecedores | Cadastro bem sucedido | Selecionar VIN, Fornecedor, Nome e Peso | Sistema exibe sucesso e a peça aparece no topo da lista com status "Gravado no Ledger" | Mens | - |
| CT - 009 | Rastreabilidade | VIN válido IVECO | Digitar VIN `1GNCS18Z2M0115561` e clicar em "Rastrear Origem" | Sistema valida com a NHTSA, salva no Ledger e exibe mensagem de sucesso | - | - |
| CT - 010 | Rastreabilidade | VIN inválido | Digitar VIN `12345678901234567` (não é da IVECO) | Sistema rejeita e exibe "Este VIN não pertence a um veículo IVECO válido" | - | - |
| CT - 011 | Relatórios | Geração de PDF | Selecionar "Veículos" e clicar em "Gerar e baixar PDF" | Ao salvar, o PDF é gerado e aberto automaticamente | - | - |
| CT - 012 | Dashboard | Tempo de resposta da API | Medir o tempo de resposta do endpoint `pegada-media` | O valor deve ser exibido no card "Tempo de Resposta(API)" | - | - |

---

*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 10 de agosto de 2026.*
