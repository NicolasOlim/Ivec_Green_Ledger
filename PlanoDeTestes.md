# 📦🍃 Iveco Green Ledger – Plano de Testes

<div class="logo-container" align="center">
 <img src="imagens/logo-plano-de-teste.png" alt="Logo Firebase Firestore" class="logo-img" style="height: 350px; width: auto; vertical-align: middle; margin-left: 15px;">
</div>

## **Informações Gerais**

| Parâmetro | Valor |
| :--- | :--- |
| Nome do Sistema | Iveco Green Ledger |
| Versão | 1.0.0 (Release Operacional Base - .NET 8) |
| Equipe Responsável | Alice Andrade, Erick Silva, Nicolas Oliveira e Vinicius Augusto |
| Data do Planejamento | 08 a 12 de Agosto de 2026 |
| Projeto | TCC – Sistema de rastreabilidade e gestão ambiental para a IVECO |



## **Objetivos dos Testes**

O processo de garantia de qualidade visa assegurar a confiabilidade, o desempenho e a precisão técnica da aplicação durante sua operação nas estações de trabalho e áreas de triagem da fábrica. Por se tratar de um sistema que exige conectividade ativa com a internet e com a Web API Central para a validação de dados e registro no Ledger, os testes focam em garantir que as rotinas funcionem perfeitamente em ambiente online e que qualquer ausência de rede seja identificada e tratada de forma clara e segura pelo sistema.

### **O Que Podemos Verificar?**

- **Integridade Operacional nas Estações de Trabalho:** Validar a usabilidade, as regras de validação de formulários e a estabilidade do aplicativo desktop WPF.
- **Desempenho e Cache Local (SQLite):** Confirmar a utilização do **SQLite** como banco relacional leve em borda, atuando estritamente como cache local para otimizar o tempo de resposta em consultas frequentes (`ex: CNPJ's e Chassis/VIN's`) já pesquisados.
- **Tratamento de Indisponibilidade de Conexão:** Validar se o sistema identifica a ausência de conexão com a internet ou com a Web API e exibe alertas imediatos e amigáveis ao operador, impedindo a perda de dados ou inconsistências.
- **Integrações com API's:** Homologar as camadas e tratamentos de falha para a BrasilAPI, NHTSA API, Google Maps e API Mercado Livre.
- **Precisão dos Cálculos e Relatórios:** Garantir a exatidão no processamento das emissões de CO2 e na emissão de relatórios.

### **Mapeamento e Mitigação de Riscos do Projeto**

| Risco Identificado | Impacto | Ação Mitigatória |
| :--- | :--- | :--- |
| Lentidão ou oscilação de rede | Alto | Utilização de cache local (SQLite) para consultas frequentes |
| Queda total de conectividade | Crítico | Teste dos bloqueios de interface e exibição de avisos claros |
| Entrada de chassis (VIN) inválidos | Alto | Testes automatizados de borda com rejeição (`I`, `O`, `Q`) e tamanhos diferentes de 17 dígitos |
| Cadastro de fornecedores incompletos | Médio | Testes de validação de formulário obrigando o preenchimento da categoria ambiental antes de registrar no ledger |
| Erros de cálculo de indicadores ambientais | Crítico | Testes unitários de fórmula matemática para validar fatores de emissão por kg de insumo recebido |

### **O Que Será Considerado Como Evidência de Qualidade?**

- **Evidências visuais:** Telas operacionais exibindo mensagens de alerta de rede e tags de confirmação (**Sincronizado** na rastreabilidade e **Gravado no Ledger** na Gestão de Componentes).
- **Eficiência do cache local:** Comprovação do ganho de velocidade no carregamento de telas através do cache de dados no SQLite local.
- **Logs da aplicação:** Comprovação do ganho de velocidade no carregamento e registro de chamadas HTTP e exceções.


## **Escopo**

### **O que será testado**

- **Interface do usuário (WPF / .NET8):** Módulos de Autenticação, Fornecedores, Peças e Componentes, Rastreabilidade, Dashboard e Análises de Sustentabilidade.
- **Backend e serviço (Web API / ASP .NET Core):** Endpoints, validações de DTO's, autenticação e tratamento de erro.
- **Persistência de dados e cache:** Operações CRUD no **SQL Server** e leitura automatizada via **SQLite**.
- **Integrações de rede:** Comunicação via HTTPS com BrasilAPI, NHTSA, Google Maps e Mercado Livre.
- **Exportação de documentos:** Geração de relatórios em PDF.
- **Indicadores de conectividade:** Exibição do status das API's externas no Dashboard.

### **O que não será testado**

- **Infraestrutura interna dos provedores de API:** A disponibilidade e estabilidade dos servidores próprios de parceiros externos.
- **Sistemas operacionais incompatíveis:** Execução da aplicação em desktop em sistemas anteriores ao Windows 10 (64-bit).
- **Testes de carga:** Testes com volumes de requisições que não condizem com o fluxo operacional da fábrica da IVECO.
- **Modo offline completo:** O sistema não funciona sem internet, portanto não serão testados fluxos offline.

### **Funcionalidades Prioritárias**

| Prioridade | Módulo / Funcionalidade | Descrição e Foco |
| :--- | :--- | :--- |
| P1 (Crítica) | Autenticação e permissões | Validação de login, perfil de acesso e consumo seguro dos endpoints da Web API |
| P1 (Crítica) | Consulta CNPJ | Validação automática da BrasilAPI, tratamento de digitação e salvamento |
| P1 (Crítica) | Cadastro de peças | Validação com 17 dígitos numéricos, vinculação com fornecedor e peso em kg |
| P2 (Alta) | Validação de conectividade | Garantia de respostas rápidas via cache local no SQLite |
| P2 (Alta) | Cálculo de pegada de carbono | Processamento e exibição gráfica das emissões acumuladas por componente / veículo |
| P3 (Média) | Relatórios em PDF | Exportação de auditoria em PDF |



## **Base de Teste**

A elaboração dos testes é fundamentada nos seguintes artefatos e especificações técnicas do projeto:

- **Manual do Usuário:** Especificação das telas, formulários, botões, requisitos de conectividade e guia de solução de problemas.
- **Documento de infraestrutura:** Requisitos de hardware, pacotes NuGet, comunicação via HTTPS e integrações com API's.
- **Plano de Migração e Banco de Dados:** Estrutura de tabelas relacionais do SQL Server, regras ACID e comportamento do cache de navegação em SQLite.
- **Termos de Licenciamento:** Regras de uso, controle de acessos, privacidade e controle de versão.
- **Código-fonte:** Solução WPF em C# (.NET 8) e Web API ASP.NET Core.
- **Documentação da API:** O Swagger/OpenAPI está disponível em: `https://apiivecogreenledger.runasp.net/swagger`.

## **Legenda dos Casos de Teste citados na Abordagem**

| ID | Funcionalidade | Descrição do Cenário |
| :--- | :--- | :--- |
| CT-001 | Login | Validação de credenciais válidas para acesso ao Dashboard |
| CT-002 | Login | Validação de senha inválida e exibição de mensagem de erro |
| CT-003 | Dashboard | Verificação do status das APIs externas (BrasilAPI, NHTSA, Google Maps, Mercado Livre) |
| CT-005 | Fornecedores | Consulta de CNPJ válido e preenchimento automático dos campos |
| CT-006 | Fornecedores | Salvamento de fornecedor com categoria ESG no Ledger |
| CT-007 | Peças | Validação de campos obrigatórios no cadastro de peças |
| CT-008 | Fornecedores | Cadastro bem-sucedido de peça com status "Gravado no Ledger" |
| CT-009 | Rastreabilidade | Validação de VIN IVECO válido e salvamento no Ledger |
| CT-010 | Rastreabilidade | Rejeição de VIN inválido (não IVECO) |
| CT-011 | Relatórios | Geração de PDF e verificação da abertura automática |
| CT-012 | Dashboard | Medição do tempo de resposta do endpoint `pegada-media` |



## **Abordagem de Testes**

| Abordagem | Descrição no Projeto | Testes Executados |
| :--- | :--- | :--- |
| Testes Funcionais | Verificação do comportamento das telas WPF, validações de campos obrigatórios, navegação pelos menus e execução das ações principais (consultar, registrar, filtrar e exportar) | CT-001, CT-002, CT-005, CT-006, CT-007, CT-008, CT-009, CT-010, CT-011 |
| Testes Não Funcionais | Avaliação da usabilidade da interface, verificação do ganho de velocidade via cache SQLite e tempo de resposta nas consultas de busca | CT-012, CT-003 |
| Testes de Sistema | Validação do fluxo completo de ponta a ponta: desde a qualificação do fornecedor por CNPJ, passando pelo vínculo da peça ao VIN do veículo, até a exibição da métrica ambiental no Dashboard | CT-001, CT-006, CT-008, CT-009, CT-011 |
| Testes de Integração | Homologação da comunicação entre a aplicação WPF, Web API, SQL Server e serviços das API's de terceiros (BrasilAPI, NHTSA, Google Maps e API Mercado Livre) | CT-003, CT-005, CT-008, CT-009 |
| Testes de Aceitação | Simulação de uso contínuo por operadores para validar a aderência do software aos procedimentos práticos de recebimento da IVECO | CT-001, CT-008, CT-009 |
| Testes Exploratórios | Navegação livre e envio de entradas não padrão no app WPF para identificar comportamentos imprevistos e exceções não tratadas visualmente | CT-007 e CT-010 |
| Testes Baseados em Cenários | Execução de rotinas baseadas no cotidiano operacional, tais como instabilidade temporária na consulta à Receita Federal ou interrupção de sinal de rede | CT-006, CT-008 e CT-009 |
| Reteste | Reexecução direta de um cenário específico que havia falhado previamente, visando validar se um bug pontual foi efetivamente corrigido | CT-011 |
| Regressão | Execução da suíte de testes após qualquer correção ou inclusão de nova funcionalidade, garantindo que as alterações não introduzam novos efeitos | CT-003 e CT-001 |



## **Técnicas de Projeto de Testes**

- **Particionamento de equivalência:** Divisão das entradas em classes válidas e inválidas para otimizar a cobertura de testes.
  - **Aplicação:** CNPJ com 14 dígitos válidos e formatos com dígitos ou caracteres inválidos; VIN's com exatamente 17 caracteres e/ou tamanhos incorretos.

- **Análise de valor limite:** Testes nos limites e bordas dos campos de entrada de dados.
  - **Aplicação:** Testando valores negativos, pesos elevados e campo de VIN com entradas de 16, 17 e 18 caracteres.

- **Tabela de decisão:** Mapeamento de combinações de condições complexas de entrada e conectividade com suas respectivas ações do sistema.
  - **Regras Mapeadas:** Se API Externa estiver online e ativa, haverá preenchimento automático via API. Caso a API estiver offline e a rede ativa, exibirá um alerta e liberará o preenchimento manual. Se a rede estiver inativa, exibirá um alerta de falha de conexão e bloqueará o salvamento até o restabelecimento da internet.

- **Testes baseados em cenários:** Construção de sequências de ações fundamentais no cotidiano de uso das estações de recebimento na fábrica da IVECO.

- **Testes exploratórios:** Sessões estruturadas sem roteiro pré-definido focadas em encontrar falhas de usabilidade na interface XAML e exceções visuais não capturadas.



## **Casos de Teste**

| ID | Funcionalidade | Cenário | Entrada | Resultado Esperado | Resultado Obtido | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| CT-001 | Login | Credenciais válidas | Usuário: `admin@iveco.com`, Senha: `Iveco@2026` | Sistema efetua login e exibe a aba `Dashboard` | Login realizado com sucesso e aba de dashboard carregada | Sucesso |
| CT-002 | Login | Senha inválida | Usuário: `admin@iveco.com`, Senha: `SenhaErrada` | Sistema exibe mensagem de erro "Credenciais inválidas" e mantém usuário na tela de login | Mensagem de erro exibida corretamente e o usuário permanece na tela de login | Sucesso |
| CT-003 | Dashboard | Status das API's externas | Navegar até o Dashboard e aguardar atualização automática | Indicadores (BrasilAPI, NHTSA, Mercado Livre) ficam verdes indicando "sucesso" ou vermelhos indicando "falha" | Brasil API: Verde, Google Maps: Vermelho (chave inválida), NHTSA: Verde, Mercado Livre: Vermelho | Sucesso |
| CT-004 | Dashboard | Remoção do card "Falhas de integração" | Visualizar o Dashboard após alteração no código | O card "Falhas de Integração" não deve aparecer. Os outros 3 cards devem estar alinhados | Card removido com sucesso | Sucesso |
| CT-005 | Fornecedores | Consulta CNPJ válido | Digitar CNPJ `00.000.000/0001-91` e clicar em "Consultar" | Campos "Razão Social", "Endereço" e "Status RFB" são preenchidos. Categoria ESG é dada como "Não avaliado" | Todos os campos preenchidos, Status RFB: `ATIVA` e Categoria ESG: `Não avaliado` | Sucesso |
| CT-006 | Fornecedores | Salvar fornecedor | Após consulta, selecionar categoria e clicar em "Registrar no Ledger" | Sistema exibe "Fornecedor registrado com sucesso" e o fornecedor aparece na lista | Sucesso e o fornecedor salvo na API e no SQLite local | Sucesso |
| CT-007 | Peças | Cadastro com dados incompletos | Deixar o campo "Nome da Peça" vazio e clicar em "Registrar" | MessageBox: "Preencha o nome da peça." O cadastro é bloqueado | Mensagem exibida e o cadastro não prossegue | Sucesso |
| CT-008 | Fornecedores | Cadastro bem sucedido | Selecionar VIN, Fornecedor, Nome e Peso | Sistema exibe sucesso e a peça aparece no topo da lista com status "Gravado no Ledger" | Sucesso e peça salva online e no SQLite local | Sucesso |
| CT-009 | Rastreabilidade | VIN válido IVECO | Digitar VIN `1GNCS18Z2M0115561` e clicar em "Rastrear Origem" | Sistema valida com a NHTSA, salva no Ledger e exibe mensagem de sucesso | Sucesso e veículo salvo online e no SQLite | Sucesso |
| CT-010 | Rastreabilidade | VIN inválido | Digitar VIN `12345678901234567` (não é da IVECO) | Sistema rejeita e exibe "Este VIN não pertence a um veículo IVECO válido" | Mensagem de erro exibida e veículo não é salvo | Sucesso |
| CT-011 | Relatórios | Geração de PDF | Selecionar "Veículos" e clicar em "Gerar e baixar PDF" | Ao salvar, o PDF é gerado e aberto automaticamente | PDF gerado com dados corretos, porém não abriu automaticamente | Falha |
| CT-012 | Dashboard | Tempo de resposta da API | Medir o tempo de resposta do endpoint `pegada-media` | O valor deve ser exibido no card "Tempo de Resposta (API)" | Valor exibido: 505 ms | Sucesso |



## **Matriz de Rastreabilidade**

| Requisito/Regra | Caso de Teste | Risco Relacionado |
| :--- | :--- | :--- |
| Autenticação | CT-001, CT-002 | Acesso não autorizado; erro de credenciais |
| Status das APIs externas | CT-003, CT-004 | Falha de integração; layout desalinhado |
| Consulta CNPJ válida | CT-005, CT-006 | API Brasil indisponível; dados inválidos |
| Validação de campos obrigatórios | CT-007 | Cadastro incompleto |
| Cadastro de peça bem-sucedido | CT-008 | Erro de persistência |
| Validação de VIN válido | CT-009 | API NHTSA indisponível |
| Validação de VIN inválido | CT-010 | Entrada inválida |
| Geração de PDF | CT-011 | Falha na abertura automática |
| Tempo de resposta da API | CT-012 | Desempenho insatisfatório |



## **Análise de Riscos**

| Risco | Causa | Impacto | Probabilidade | Severidade | Ação Mitigatória | Tipo(s) de Teste |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| API Brasil (CNPJ) indisponível | Instabilidade do serviço externo | Alto (bloqueia cadastro de fornecedores) | Média | Alta | Testar timeout e mensagem de erro; prever entrada manual | Testes Funcionais, Testes de Integração, Testes Baseados em Cenários |
| API NHTSA indisponível | Falha no serviço externo | Alto (bloqueia rastreabilidade) | Média | Alta | Testar tratamento de exceção e mensagem clara | Testes Funcionais, Testes de Integração, Testes Baseados em Cenários |
| Inacessibilidade da Web API Central | Queda do backend | Crítico (impede persistência) | Baixa | Crítica | Testar timeout de 10s e bloqueio de salvamento | Testes Funcionais, Testes de Integração, Testes de Sistema, Testes Não Funcionais |
| Cache do Dashboard obsoleto | Expiração não controlada | Médio (dados desatualizados) | Alta | Média | Validar expiração do cache em 60s e atualização automática | Testes Funcionais, Testes Não Funcionais, Testes de Integração |
| Falha na geração/abertura de PDF | Erro no Process.Start ou geração | Baixo (funcionalidade secundária) | Baixa | Baixa | Testar geração e abertura automática | Testes Funcionais, Testes de Sistema |
| Entrada de VIN inválido | Caracteres proibidos (I, O, Q) ou tamanho incorreto | Alto (dados inconsistentes) | Alta | Alta | Aplicar análise de valor limite e particionamento | Testes Funcionais, Testes Exploratórios |
| Cadastro incompleto de fornecedor | Campos obrigatórios não preenchidos | Médio (registro inconsistente) | Alta | Média | Testar validações de formulário | Testes Funcionais, Testes Exploratórios |
| Erro no cálculo de indicadores ambientais | Fórmula incorreta | Crítico (métricas erradas) | Média | Crítica | Testar fórmulas matemáticas com valores conhecidos | Testes Funcionais, Testes de Sistema |



## **Cenários Selecionados para Reteste/Regressão**

Nossa equipe selecionou 5 cenários que podem apresentar defeito e descrevemos o processo que será utilizado após a correção:

| Cenário | Defeito Relacionado | Reteste | Regressão |
| :--- | :--- | :--- | :--- |
| 1. Geração de PDF | BUG-001 (Em aberto) | Reexecutar CT-011, gerar o PDF e verificar se ele abre automaticamente no leitor padrão após a correção | Validar CT-012 (Tempo de resposta da API) e CT-001 (Login) para garantir que não sejam afetados |
| 2. Remoção do card | BUG-004 (Preventivo) | Verificar se o card realmente sumiu da interface e se os 3 cards restantes continuam alinhados | Verificar CT-003 (Status das API's externas) e CT-001 (Login) para garantir que não sejam afetados |
| 3. Consulta CNPJ com API indisponível | BUG-005 (Preventivo) | Simular falha da API, consultar um CNPJ e verificar se o sistema exibe mensagem de erro | Verificar CT-005 (Consulta CNPJ válido) e CT-006 (Salvar fornecedor) para garantir que continua salvando |
| 4. Validação de VIN inválido | BUG-006 (Preventivo) | Digitar um VIN com caracteres proibidos (`I`, `O`, `Q`) | Verificar CT-009 (VIN válido IVECO) para garantir que continua salvando corretamente |
| 5. Cadastro de peça com dados incompletos | BUG-007 (Preventivo) | Deixar o campo "Nome da Peça" vazio e verificar se o sistema bloqueia o cadastro com a mensagem correta | Verificar CT-008 (Cadastro bem sucedido) para garantir que o cadastro completo continua funcionando |



## **Critérios de Entrada e Saída**

### **Critérios de Entrada**

- Código fonte da versão a ser testada devidamente compilado e sem erros de build;
- Ambiente de teste configurado com o banco SQL Server e SQLite inicializados com dados de homologação;
- Aplicação WPF instalada ou executável gerado na estação de testes;
- Web API ativa e respondendo endpoints de teste;
- Casos de testes documentados e revisados neste mesmo arquivo;
- Dados de testes preparados (usuário admin, CNPJ, VIN).

### **Critérios de Saída**

- 100% dos casos de teste planejados executados;
- Nenhum defeito com severidade crítica em aberto;
- Defeitos com severidade alta corrigidos ou com plano de correção aprovado;
- Evidências de testes consolidadas e anexadas à documentação;
- Riscos residuais avaliados e documentados;
- Defeitos conhecidos de baixa severidade documentados com justificativa para aceite.



## **Evidências e Documentação**


- **Prints:** capturados para cada etapa relevante, nomeados com ID do teste e descrição curta (ex: `CT-001_login_sucesso.png`).
- **Vídeos:** gravados para fluxos complexos ou defeitos intermitentes, em formato MP4.
- **Resultados obtidos:** registrados na tabela de casos de teste (coluna Resultado Obtido).
- **Logs da aplicação:** armazenados na pasta `logs/`, contendo data, hora, chamadas HTTP, exceções e operações de banco.
- **Informações do ambiente:** versão do sistema, sistema operacional, versão do .NET, estado das APIs no momento do teste.
- **Versão testada:** registrada no controle de versão do documento.



## **Erro, Defeito e Falha**

### **Definições**

- **Erro:** ação ou decisão incorreta realizada por uma pessoa durante análise, desenvolvimento ou configuração. Exemplo: desenvolvedor escreve uma condição lógica errada na validação do VIN.
- **Defeito:** problema existente no produto, resultante de um erro, que pode causar uma falha quando ativado. Exemplo: o código de validação aceita VIN com 16 caracteres, embora a regra exija 17.
- **Falha:** comportamento incorreto observado durante a execução do sistema, devido à ativação de um defeito. Exemplo: ao digitar um VIN de 16 caracteres e clicar em "Rastrear", o sistema aceita o registro, mas o esperado era rejeitar.

### **Exemplo Aplicado ao Projeto**

Durante o desenvolvimento, um programador definiu que a validação do VIN permitiria 16 caracteres (erro). Isso gerou um defeito na função de validação. Ao executar o sistema, o operador digitou um VIN de 16 caracteres e o sistema aceitou o veículo (falha). O teste de valor limite foi projetado justamente para detectar essa situação.



## **Severidade e Prioridade**

### **Definições**

 representa o impacto do defeito no sistema:

- **Crítica:** impede a operação principal do sistema; pode causar perda de dados.
- **Alta:** funcionalidade importante inutilizada, mas há contorno.
- **Média:** funcionalidade parcialmente comprometida.
- **Baixa:** defeito cosmético ou de baixo impacto.

**Prioridade** representa a urgência para correção:

- **Alta:** deve ser corrigido imediatamente.
- **Média:** pode ser corrigido na próxima iteração.
- **Baixa:** pode ser postergado.

### **Exemplo de Severidade ≠ Prioridade**

Um erro de ortografia em uma mensagem de boas-vindas tem **severidade baixa** (impacto mínimo), mas **prioridade alta** se for exibido em uma tela inicial do cliente em uma demonstração importante. No Iveco Green Ledger, um defeito no indicador de API externa pode ter severidade média (não impede o cadastro, pois há fallback manual), mas prioridade baixa se a chave inválida não afeta o fluxo principal.



## **Conclusão**

Este Plano de Testes estabelece uma abordagem estruturada para avaliar o Iveco Green Ledger, cobrindo os principais módulos, regras de negócio e integrações. O objetivo não é apenas validar funcionalidades isoladas, mas reduzir riscos operacionais — especialmente aqueles relacionados à conectividade, à persistência de dados e à integração com APIs externas.

Os principais riscos identificados envolvem a indisponibilidade das APIs BrasilAPI e NHTSA, a inacessibilidade da Web API Central e a entrada de dados inválidos (VIN, peso, CNPJ). Para mitigá-los, foram projetados casos de teste específicos, aplicando técnicas como particionamento de equivalência, análise de valor limite, tabela de decisão, cenários e testes exploratórios.

Os critérios de entrada e saída definem condições objetivas para iniciar e encerrar o processo de teste, evitando decisões baseadas apenas na intuição. A execução dos testes, com registro rigoroso de evidências e tratamento de defeitos, fornecerá informações suficientes para que a equipe decida conscientemente sobre a liberação do sistema.

Este documento deve ser mantido atualizado no repositório do projeto, refletindo o estado real do sistema e dos testes executados. Qualquer alteração relevante no código ou nos requisitos deverá ser acompanhada de reteste e regressão, conforme descrito na seção correspondente.

---

## **Controle de Versão do Documento**

| Versão | Data | Alteração | Responsável |
| :--- | :--- | :--- | :--- |
| 1.0 | 10/08/2026 | Criação do Plano de Testes | Equipe |
| 1.1 | 12/08/2026 | Complementação com seções adicionais (rastreabilidade, riscos, evidências, recursos, cronograma, conclusão) | Equipe |


*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 12 de agosto de 2026.*

As evidências serão armazenadas no repositório do projeto, em uma pasta estruturada:
