# 📦🍃 Iveco Green Ledger – Sistema de Rastreamento Inteligente  
**Trabalho de Conclusão de Curso**  
**Unidade SENAI: Nova Lima**

**Instrutor: Frederico Martins Aguiar**

---

<div class="logo-container">
    <img src="imagens/Iveco_greenLogo.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

**Equipe de Desenvolvimento**  
[🧑‍💻 Nicolas Oliveira Lima](https://github.com/NicolasOlim)  |  [🧑‍💻 Alice Andrade](https://github.com/aliceandradee)  |  [🧑‍💻 Erick Silva](https://github.com/erick190813)  |  [🧑‍💻 Vinicius Augusto](https://github.com/vnxtry)  

---

## Quem somos:

O projeto Iveco Green Ledger foi idealizado, modelado e implementado por um grupo de estudantes do Curso Técnico em Desenvolvimento de Sistemas da Escola de Programação e Robótica – SENAI, atuando sob a orientação do educador Fred Aguiar. Diante do cenário de transformação digital e das crescentes pressões globais por transparência climática, o grupo uniu competências complementares nas áreas de arquitetura de software distribuída, engenharia de dados avançada e análise de balanços de sustentabilidade corporativa (ESG).

Essa sinergia técnica e o aprofundamento nos critérios metodológicos do GHG Protocol permitiram que a equipe projetasse, validasse e construísse uma solução computacional de alto nível. O ecossistema foi desenhado especificamente para mitigar e solucionar gargalos reais e complexos de rastreabilidade logística, controle de insumos industriais e auditoria ambiental, preenchendo uma lacuna crítica no monitoramento do Escopo 3 dentro da cadeia de suprimentos automotiva de carga pesada.

---

## Problema encontrado:
O cenário industrial automobilístico de grande porte é marcado por movimentações logísticas massivas de materiais em suas linhas de montagem, englobando desde ligas metálicas brutas até componentes complexos fornecidos por uma extensa rede parceira. Na maioria das organizações desse setor, o ciclo de vida e o real impacto ecológico desses insumos não são monitorados de maneira integrada desde a sua origem, sendo a pegada de carbono tratada de forma genérica ou totalmente dissociada de cada veículo produzido.

A ausência de sistemas organizados de rastreabilidade e gerenciamento inteligente desses fluxos materiais gera impactos consideráveis em duas dimensões principais. Sob a ótica econômica e operacional, as montadoras enfrentam ineficiências decorrentes de inventários imprecisos e erros em inputs manuais de recebimento, tornando o fluxo sequencial da linha de montagem vulnerável a oscilações de rede e paralisações onerosas no chão de fábrica caso não possuam mecanismos locais estáveis de contingência de dados.

No âmbito ambiental e de governança (ESG), a falta de um rastreamento preciso impede que as empresas calculem com exatidão matemática as suas emissões de Escopo 3 do GHG Protocol, referentes ao impacto indireto da cadeia de suprimentos. Embora a transição para uma economia de baixo carbono seja amplamente discutida, sua aplicação real na indústria pesada é limitada pela escassez de ferramentas tecnológicas acessíveis e integradas de forma direta ao cotidiano operacional das fábricas para transformar a coleta de dados físicos em ativos de conformidade climática.

É nesse cenário desafiador que se insere a proposta do Iveco Green Ledger, uma plataforma tecnológica voltada para a gestão inteligente, rastreabilidade volumétrica e direcionamento de metadados ambientais na linha de produção de veículos comerciais. A solução se justifica por estruturar um modelo operacional híbrido capaz de garantir a resiliência offline no recebimento de materiais por meio de armazenamento relacional local (SQLite) e centralizar a inteligência analítica na nuvem (Firebase Firestore), integrando eficiência de software e sustentabilidade aplicada de acordo com as demandas ecológicas contemporâneas.

---

## Solução a ser trabalhada:

A solução apresentada para linha de montagem é o **Iveco Green Ledger**. Trata-se de um ecossistema de software  projetado especificamente para atuar na intersecção entre o chão de fábrica e a gestão de governança climática (ESG). O sistema automatiza a coleta de metadados logísticos e de cubagem volumétrica, vinculando de forma direta e imutável o impacto ecológico de cada insumo recebido ao número de chassi correspondente (VIN) do veículo comercial em produção.

O grande diferencial técnico do ecossistema reside na sua arquitetura de dados híbrida e resiliente, estruturada para neutralizar as vulnerabilidades típicas do ambiente industrial pesado. A operação de recepção e validação de materiais no galpão é gerenciada por um cliente desktop desenvolvido em WPF sob o padrão de projeto MVVM. Esta interface consome uma camada de persistência local baseada no banco de dados embutido SQLite, garantindo que o sistema opere em regime de total autonomia mesmo diante de oscilações ou quedas na infraestrutura de rede da fábrica, evitando paralisações onerosas na linha de montagem.

Assim que a conectividade com a internet é estabelecida ou normalizada, os dados locais são sincronizados de forma assíncrona com o back-end, cuja inteligência é centralizada na nuvem por meio do Firebase. Essa API RESTful, construída sobre a robustez do framework ASP.NET Core 8, é responsável por orquestrar a comunicação com serviços externos regulatórios (como BrasilAPI e NHTSA response) e rodar o motor algorítmico que calcula a pegada de carbono de Escopo 3 com base nas diretrizes internacionais do GHG Protocol.

Por fim, a solução consolida essas informações complexas em dashboards analíticos de alta performance renderizados em tempo real. Essa camada visual permite que os gestores de logística e os auditores ambientais da Iveco acessem relatórios dinâmicos e transparentes sobre o balanço de carbono da cadeia de suprimentos. Dessa forma, o Iveco Green Ledger converte dados operacionais brutos de manufatura em ativos estratégicos de conformidade socioambiental, unindo de ponta a ponta a eficiência de software à sustentabilidade industrial aplicada.

---

## Objetivos do Projeto:

Desenvolver e homologar uma plataforma tecnológica híbrida (desktop-nuvem) voltada à gestão inteligente, cubagem volumétrica e rastreabilidade ponta a ponta de insumos automotivos, integrando os dados físicos do chão de fábrica a um motor analítico capaz de automatizar o cálculo e a auditoria das emissões de carbono de Escopo 3 por chassi (VIN) na linha de produção da Iveco.

**Objetivos específicos:**

 - **Projetar e construir a interface de chão de fábrica:** Desenvolver o cliente desktop utilizando o framework WPF sob o padrão arquitetural MVVM (Model-View-ViewModel), garantindo uma experiência de usuário fluida, intuitiva e adaptada à rotina operacional dos operadores de recebimento logístico.

- **Garantir a resiliência operacional offline (Offline-Safe):** Implementar uma camada de persistência relacional local utilizando o banco de dados embutido SQLite, permitindo a continuidade da coleta de dados de cubagem e validação mesmo durante instabilidades ou ausência total de conectividade com a internet.

- **Centralizar a inteligência analítica na nuvem:** Estruturar a persistência não relacional (NoSQL) no Firebase Firestore e desenvolver um back-end em ASP.NET Core 8 responsável pelo processamento assíncrono, sincronização dos dados locais e orquestração do ecossistema.

- **Automatizar as validações regulatórias e fiscais:** Integrar o back-end a APIs públicas (como BrasilAPI para dados cadastrais e fiscais de fornecedores e NHTSA para a decodificação técnica do código VIN), eliminando a necessidade de inputs manuais suscetíveis a falhas humanas.

- **Desenvolver o motor de cálculo ecológico:** Codificar o algoritmo matemático para mensuração da pegada de carbono de Escopo 3 com base nos dados físicos de cubagem e nos parâmetros normativos do GHG Protocol.

- **Prover transparência analítica para governança ESG:** Implementar dashboards dinâmicos em tempo real utilizando a biblioteca LiveCharts2, permitindo a geração de relatórios de conformidade ambiental auditáveis para a tomada de decisões gerenciais e estratégicas.


---

## Desenvolvimento do Projeto:

O desenvolvimento do ecossistema distribuído do Iveco Green Ledger foi estruturado em fases cíclicas e incrementais. Essa abordagem visou garantir o rigor técnico exigido pelas metodologias de auditoria climática e a estabilidade da engenharia de software na integração de sistemas. O fluxo metodológico dividiu-se nas seguintes etapas de engenharia:

- **Levantamento de Requisitos e Modelagem Sistêmica:** A etapa inicial concentrou-se na extração e especificação de requisitos funcionais e não funcionais a partir do escopo da Iveco e do GHG Protocol. Foram modelados os diagramas de Caso de Uso e Fluxo de Dados (DFD) para compreender de que maneira a entrada física de componentes e insumos industriais interagia com as APIs regulatórias. Esse mapeamento foi essencial para determinar as regras de negócio aplicadas no vínculo indissociável de dados ecológicos ao chassi (VIN) de cada veículo.

- **Arquitetura de Dados NoSQL na Nuvem:** Nesta fase, estabeleceu-se a modelagem não relacional orientada a documentos e coleções no Firebase Firestore. A modelagem foi planejada de forma desnormalizada para otimizar a velocidade de consultas massivas por chassi (veiculos) e por planta corporativa (fornecedores). Estruturou-se um dicionário de dados focado em integridade e auditoria, onde os relacionamentos lógicos foram firmados por meio de chaves de referência direta (fk_*), garantindo que o histórico ambiental de cada lote de matéria-prima permanecesse indexado para futuras auditorias de compliance ESG.

- **Construção do Back-End e Integrações com Serviços Externos:** Desenvolveu-se a API RESTful central utilizando o framework ASP.NET Core 8, atuando como a camada de inteligência e governança do projeto. O back-end foi equipado com injeção de dependência nativa e rotinas assíncronas (async/await) para suportar cargas elevadas de leitura e escrita. Foram implementadas duas integrações fundamentais com serviços externos via chamadas HTTP: a BrasilAPI para automatizar a consulta cadastral de fornecedores pelo CNPJ, eliminando erros de digitação, e a API da NHTSA para decodificar e validar a legitimidade do chassi Iveco na linha de montagem.

- **Desenvolvimento da Interface de Apresentação Desktop:** A camada do cliente foi concebida através do WPF (Windows Presentation Foundation) no padrão de projeto MVVM (Model-View-ViewModel), isolando rigorosamente a interface visual da lógica de apresentação. Nesta etapa, integraram-se componentes dinâmicos e responsivos em Light Mode para a operação de pátio, incluindo o consumo síncrono e assíncrono dos endpoints da API pelo HttpClient. Além disso, acoplou-se a biblioteca LiveCharts2 para a plotagem em tempo real de gráficos analíticos das emissões indiretas de Escopo 3.

- **Sistema de Telemetria, Geração de Dossiês e Testes de Integração:** A fase final consistiu na implementação de recursos avançados de auditoria e robustez de software. Configurou-se um middleware de log corporativo com o Serilog para monitorar o tempo de resposta e latência de cada requisição no servidor. Paralelamente, utilizou-se a engine do QuestPDF para desenhar o módulo de exportação de dados, capaz de compilar relatórios fiscais paginados que contêm códigos de integridade hash. Por fim, testes funcionais de ponta a ponta validaram a exatidão matemática do algoritmo de pegada de carbono por quilo de insumo, concluindo a homologação técnica do ecossistema.

  ---

## Modelagem do Sistema:
**Diagrama de Caso de Uso**

<div class="logo-container">
    <img src="imagens/diagrama de caso de uso.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>


| ID | Caso de Uso | Ator Principal | Descrição Operacional |
| :--- | :--- | :--- | :--- |
| **UC01** | Efetuar Autenticação (Login) | Administrador / Operador | Realiza a validação das credenciais do usuário comparando o hash da senha no banco de dados. |
| **UC02** | Gerenciar Usuários | Administrador | Permite cadastrar, atualizar e definir os níveis de privilégio (Acesso) dos colaboradores. |
| **UC03** | Cadastrar Fornecedores | Operador / Administrador | Registra empresas parceiras na base de dados, utilizando a integração com a BrasilAPI para preenchimento via CNPJ. |
| **UC04** | Cadastrar Lotes de Matéria-Prima | Operador | Registra a entrada de insumos industriais, especificando o tipo de material, peso em quilogramas e o fator de pegada ecológica. |
| **UC05** | Vincular Componentes ao Veículo | Operador | Associa peças específicas a um chassi através do código VIN, estabelecendo a árvore de rastreabilidade de materiais. |
| **UC06** | Validar Legitimidade Industrial (VIN) | Sistema | Consome de forma automatizada a API da NHTSA VPIC para verificar se o chassi informado pertence à fabricante Iveco. |
| **UC07** | Processar Pegada de Carbono | Sistema | Executa o motor algorítmico que calcula a emissão de CO₂ equivalente ($CO_2e$) com base na massa do lote e no indicador do material (Escopo 3). |
| **UC08** | Monitorar Dashboards Analíticos | Operador / Administrador | Renderiza gráficos em tempo real (LiveCharts2) com o balanço de emissões segmentado e o histórico de produção. |
| **UC09** | Emitir Dossiê Auditável (PDF) | Administrador | Compila os dados consolidados de um chassi ou período em um relatório paginado e criptografado gerado pelo QuestPDF. |
| **UC10** | Registrar Logs de Requisições | Sistema | Intercepta o tráfego HTTP por meio do middleware do Serilog para auditar a latência e o status das operações. |

  ---
  
**Diagrama de Fluxo**


<div class="logo-container">
    <img src="imagens/diagrama de fluxo.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

O ecossistema Iveco Green Ledger opera por meio de um fluxo sequencial e rígido de validações automatizadas que estruturam a lógica do seu diagrama de fluxo:

- **Controle de Acesso e Autenticação:** O sistema inicia verificando as credenciais na tela de login, comparando o hash da senha na coleção usuarios para conceder o acesso e liberar as abas da interface de acordo com o nível de privilégio do usuário (Admin ou Operador).

- **Homologação Fiscal de Fornecedores:** Ao registrar uma empresa parceira, o software realiza uma chamada assíncrona à BrasilAPI para validar a situação do CNPJ na base da Receita Federal e autocompletar as informações cadastrais, mitigando erros humanos de digitação.

- **Validação Industrial de Chassis:** Na etapa de montagem do veículo, o operador insere o código VIN (chassi), e o sistema consome a API da NHTSA para ratificar a legitimidade do código, bloqueando a operação caso o chassi não pertença originalmente ao grupo Iveco.

- **Cálculo de Emissões e Saída de Dados:** Após a validação das peças e dos dados físicos de cubagem dos lotes associados, o motor algorítmico calcula as emissões de Escopo 3 sob as diretrizes do GHG Protocol, atualizando instantaneamente os painéis visuais do LiveCharts2 e disponibilizando os relatórios auditáveis para exportação em PDF via QuestPDF.


  ---
**Diagrama de Sequência**
  
<div class="logo-container">
    <img src="imagens/diagrama de sequencia.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>
O Diagrama de Sequência do Iveco Green Ledger descreve a ordem cronológica em que as requisições e dados trafegam pelas camadas da arquitetura distribuída:

- **Início na Interface (WPF):** O operador insere o VIN e os dados das peças na RastreabilidadeView. Ao salvar, a ViewModel dispara uma requisição assíncrona via HTTP POST contendo o payload em JSON para a API (ApiIveco).

- **Validação de Fronteira (NHTSA):** O DadosController repassa os dados para o DadosService, que consome a API externa da NHTSA. Se o chassi não for validado como original da Iveco, o fluxo é interrompido com um erro HTTP 400; caso contrário, o fluxo avança.

- **Processamento e Cálculo (API):** Com o chassi validado, a camada de serviço busca os fatores de emissão dos lotes das peças no banco de dados e executa o motor matemático para calcular a pegada de carbono de Escopo 3 do veículo.

- **Persistência e Confirmação (Firestore):** O back-end grava os dados consolidados no Firebase Firestore de forma assíncrona. O banco confirma a gravação para a API, que responde com status HTTP 200 para o cliente WPF, atualizando instantaneamente os gráficos do LiveCharts2.

---
## Arquitetura de Persistência de Dados:

O trânsito da informação entre as duas camadas de persistência obedece a um fluxo síncrono-assíncrono controlado por software:

- **Escrita Local de Contingência:** Em cenários offline, os dados capturados no pátio são estruturados em tabelas relacionais locais no SQLite com carimbos de data/hora (timestamps) e flags de controle de status de sincronização (is_sintonizado = false).

- **Consumo de API e Validação:** Assim que a rede é restabelecida, a aplicação desktop lê o buffer local do SQLite e dispara os payloads via HttpClient para a API em ASP.NET Core 8.

- **Persistência Definitiva:** O back-end recebe os dados, executa as validações externas nas APIs da NHTSA e BrasilAPI, roda o motor matemático do GHG Protocol e persiste o documento final no Firebase Firestore. Após a confirmação de sucesso da nuvem, a flag local no SQLite é atualizada (is_sintonizado = true), mantendo o histórico local apenas para auditoria de desempenho do terminal.

Essa arquitetura híbrida garante que o Iveco Green Ledger ofereça o melhor de dois mundos: a robustez analítica e a segurança centralizada de um banco de dados em nuvem estruturado para governança ESG, sem sacrificar a resiliência e a continuidade operacional exigidas no chão de fábrica de uma montadora automotiva de grande porte.




*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 16 de junho de 2026.*
