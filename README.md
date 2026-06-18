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

## Arquitetura do Projeto
 
O projeto segue rigorosamente o padrão **MVVM (Model-View-ViewModel)** na camada de apresentação e uma arquitetura de **serviços desacoplados** na API REST.
 
```
Ivec_Green_Ledger/
├── ApiIveco/                  # Backend principal — ASP.NET Core REST API
│   ├── Controllers/           # Endpoints HTTP (CRUD completo)
│   ├── Data/                  # Configuração do Firebase Client
│   ├── Models/                # Entidades do domínio
│   ├── Services/              # Regras de negócio e acesso ao Firebase
│   └── Program.cs             # Configuração da aplicação, DI, Swagger, CORS
│
├── WpfIveco/                  # Frontend — WPF Desktop (MVVM)
│   ├── Commands/              # RelayCommand (padrão Command do MVVM)
│   ├── Converters/            # IValueConverter para binding de UI
│   ├── Imgs/                  # Recursos de imagem
│   ├── Models/                # Espelho das entidades do domínio
│   ├── Services/              # Serviços de negócio e integrações
│   │   └── Interface/         # Interfaces dos serviços
│   ├── Styles/                # Estilos XAML globais
│   ├── ViewModels/            # Lógica de apresentação
│   └── Views/                 # Janelas e controles XAML
│       └── Controls/          # Dashboard, Rastreabilidade, Análises, ESG, Relatórios...
│
├── Documentaçao/              # Documentação técnica
├── Banco de Dados/            # Scripts e documentação do banco
├── imagens/                   # Recursos visuais (logos, diagramas)
├── Ivec_Green_Ledger.sln      # Solução .NET unificada
└── README.md                  # Este arquivo
```

---
## Arquitetura de Persistência de Dados:

O trânsito da informação entre as duas camadas de persistência obedece a um fluxo síncrono-assíncrono controlado por software:

- **Escrita Local de Contingência:** Em cenários offline, os dados capturados no pátio são estruturados em tabelas relacionais locais no SQLite com carimbos de data/hora (timestamps) e flags de controle de status de sincronização (is_sintonizado = false).

- **Consumo de API e Validação:** Assim que a rede é restabelecida, a aplicação desktop lê o buffer local do SQLite e dispara os payloads via HttpClient para a API em ASP.NET Core 8.

- **Persistência Definitiva:** O back-end recebe os dados, executa as validações externas nas APIs da NHTSA e BrasilAPI, roda o motor matemático do GHG Protocol e persiste o documento final no Firebase Firestore. Após a confirmação de sucesso da nuvem, a flag local no SQLite é atualizada (is_sintonizado = true), mantendo o histórico local apenas para auditoria de desempenho do terminal.

Essa arquitetura híbrida garante que o Iveco Green Ledger ofereça o melhor de dois mundos: a robustez analítica e a segurança centralizada de um banco de dados em nuvem estruturado para governança ESG, sem sacrificar a resiliência e a continuidade operacional exigidas no chão de fábrica de uma montadora automotiva de grande porte.

---
## Viabilidade Técnica:

A análise de viabilidade técnica do ecossistema Iveco Green Ledger foi estruturada para comprovar a capacidade de execução, escalabilidade e resiliência do sistema dentro do ambiente industrial dinâmico da montadora. Abaixo estão detalhados os componentes que sustentam a viabilidade tecnológica da solução.

**Introdução**

A validação técnica de um software voltado para a indústria automobilística pesada exige que as escolhas arquiteturais garantam alta disponibilidade, segurança e capacidade de processamento assíncrono. O ambiente fabril é notoriamente hostil para sistemas puramente dependentes da nuvem devido a oscilações de conectividade e à necessidade de respostas em tempo real no chão de fábrica.

A análise a seguir demonstra que o projeto é plenamente viável, pois utiliza uma pilha tecnológica madura, padronizada e baseada em componentes de mercado amplamente suportados pelas comunidades globais de desenvolvimento, garantindo baixo custo de manutenção e alta eficiência operacional.

**Descrição**

O Iveco Green Ledger é um ecossistema distribuído focado na intersecção entre a logística de recebimento de materiais e a governança climática (ESG). A solução consiste em capturar os dados físicos de cubagem e massa dos lotes de matéria-prima no momento do recebimento e, por meio de um motor matemático parametrizado pelo GHG Protocol, calcular a pegada de carbono de Escopo 3 associada a cada componente.

O grande diferencial da solução é vincular esse impacto ecológico de forma indissociável ao número de chassi (VIN) de cada veículo comercial Iveco. Para garantir que o fluxo de produção nunca pare, a solução adota uma arquitetura híbrida de dados: um cliente de pátio que retém as operações localmente em cenários offline e uma API em nuvem que centraliza as validações fiscais/industriais e consolida os dashboards analíticos e relatórios para auditoria.

**Organização Tecnológica:**

A engenharia do sistema foi dividida em camadas lógicas bem definidas, utilizando linguagens e frameworks que conversam nativamente entre si:

- Camada de Apresentação e Operação (Cliente Desktop): Desenvolvida em WPF (Windows Presentation Foundation) com a linguagem C# e .NET 8, utilizando o padrão arquitetural MVVM (Model-View-ViewModel). Essa escolha isola a interface gráfica da lógica de processamento, garantindo uma aplicação responsiva e de fácil manutenção.

- Camada de Persistência Local (Offline-Safe): Utilização do banco de dados embutido SQLite. Por ser serverless e rodar diretamente na memória do processo da aplicação, ele dispensa a instalação de servidores complexos de banco de dados nos terminais físicos do pátio logístico.

- Camada de Serviços e Negócios (Back-End): Uma API RESTful construída com ASP.NET Core 8. O framework fornece injeção de dependência nativa e processamento de requisições de forma puramente assíncrona (async/await), otimizando o consumo de hardware do servidor.

- Camada de Nuvem e Consolidação (Banco de Dados Central): O Firebase Firestore atua como o repositório NoSQL orientado a documentos. Ele foi escolhido por sua escalabilidade horizontal automatizada e pela capacidade de sincronização orientada a eventos em tempo real.

- Componentes Analíticos e de Auditoria: A biblioteca LiveCharts2 foi integrada para a plotagem dinâmica de gráficos vetoriais na interface, enquanto o framework QuestPDF foi selecionada para a renderização sob demanda dos relatórios fiscais assinados.

---
## Arquitetura de Persistência de Dados:

A Metodologia de Implementação do ecossistema Iveco Green Ledger foi reestruturada para refletir o ciclo de vida iterativo e incremental da solução unificada (Ivec_Green_Ledger.sln), mapeando o avanço do software desde a captura física no pátio logístico até a inteligência analítica na nuvem.

O processo de engenharia dividiu-se nas seguintes etapas fundamentais:


**Etapa 1 – Desenvolvimento do Módulo Cliente e Captura Física (WpfIveco)**

A fase inicial concentrou-se na construção da aplicação de chão de fábrica dentro do projeto WpfIveco. A interface com o operador de pátio foi desenvolvida em XAML utilizando o padrão arquitetural MVVM, garantindo a reatividade dos campos de entrada. Nesta etapa, implementou-se a lógica de medição e pesagem dos insumos logísticos, acoplando a biblioteca LiveCharts2 para a plotagem dos dados na View. Para assegurar o funcionamento da linha de montagem em cenários de instabilidade de rede, estruturou-se nesta fase a persistência relacional embutida via SQLite, permitindo que o terminal armazene os dados localmente de forma temporária (offline-safe).

**Etapa 2 – Integração com a Aplicação Back-End (ApiIveco)**

A segunda fase consistiu no desenvolvimento e conectividade com o projeto ApiIveco, uma API RESTful baseada em ASP.NET Core 8. Implementou-se a sincronização assíncrona para que os payloads retidos no SQLite local fossem transmitidos via HTTP POST para o servidor assim que o link de rede fosse restabelecido. Na API, configurou-se o motor matemático do GHG Protocol para calcular a pegada de carbono de Escopo 3 e orquestraram-se as chamadas HTTP externas (BrasilAPI para homologação fiscal e NHTSA para validação de chassis). Após o processamento do back-end, os dados consolidados foram integrados à persistência global do banco de dados NoSQL Firebase Firestore.

**Benefícios Técnicos**

A separação e posterior integração dos projetos dentro da mesma solução trouxe vantagens de engenharia cruciais para o projeto:

- Desacoplamento e Performance: O processamento pesado dos motores algorítmicos e as requisições para as APIs governamentais ficaram isolados na ApiIveco, mantendo o cliente desktop WpfIveco leve e responsivo para o operador de pátio.
  
- Tolerância a Falhas Industrial: A arquitetura híbrida (SQLite + Firestore) elimina o ponto único de falha de redes industriais, blindando a Iveco contra a perda de dados climáticos ou paralisações de inventário.
  
- Rastreabilidade e Compliance: A integração nativa do Serilog na camada de serviços permitiu auditar a telemetria e latência de ponta a ponta, enquanto o QuestPDF garantiu a geração automatizada de relatórios com assinaturas digitais imutáveis.

---
## Viabilidade Econômica:

O projeto **Iveco Green Ledger** foi concebido como uma solução tecnológica de alta eficiência e baixo custo de implantação, utilizando componentes de hardware convencionais para pátio logístico e desenvolvimento próprio. Essa abordagem reduz significativamente o investimento inicial quando comparada a sistemas industriais proprietários de telemetria ambiental e rastreabilidade de frotas.

---

### Custos Estimados de Implantação

####  Investimento em Hardware
| Item | Quantidade | Valor Unitário | Total |
| :--- | :--- | :--- | :--- |
| Terminal de Chão de Fábrica (Computador Core i5) | 1 | R$ 1.800,00 | R$ 1.800,00 |
| Dispositivo de Entrada de Pátio (Coletor USB) | 1 | R$ 150,00 | R$ 150,00 |
| Balança de Precisão Comercial / Sensor de Cubagem | 1 | R$ 450,00 | R$ 450,00 |
| **Subtotal Hardware** | — | — | **R$ 2.400,00** |

#### Custo de Desenvolvimento (Mão de Obra)
| Métrica de Esforço | Detalhamento | Valor |
| :--- | :--- | :--- |
| **Horas Totais Dedicadas** | Desenvolvimento dos projetos `ApiIveco` e `WpfIveco` | 60 horas |
| **Valor Estimado por Hora** | Custo-hora de engenharia de software júnior | R$ 25,00 |
| **Subtotal Mão de Obra** | **Total Geral Estimado** | **R$ 1.500,00** |

#### Custo Total do Projeto Consolidado
| Categoria de Despesa | Valor Absoluto (R$) | Representação Percentual (%) |
| :--- | :--- | :--- |
| Hardware e Infraestrutura Física | R$ 2.400,00 | 61,54% |
| Mão de Obra e Engenharia de Software | R$ 1.500,00 | 38,46% |
| **Total Geral do Investimento** | **R$ 3.900,00** | **100,00%** |

---

### Benefícios Econômicos e Retorno sobre o Investimento (ROI)

#### Matriz de Ganhos Financeiros e Operacionais
| Benefício Mapeado | Impacto Econômico Direto | Indicador de Sucesso |
| :--- | :--- | :--- |
| **Mitigação de Sanções** | Evita multas ambientais e fiscais através da automação de relatórios precisos do Escopo 3 de acordo com o GHG Protocol. | Zero passivos ambientais em auditorias. |
| **Eliminação de Ociosidade** | O mecanismo *offline-safe* do SQLite impede o travamento do pátio por falta de internet, anulando o altíssimo custo de linha de produção parada. | Disponibilidade contínua de pátio (100% *uptime* local). |
| **Redução do Tempo Operacional** | Consumo automatizado da BrasilAPI e NHTSA elimina preenchimentos manuais lentos e erros humanos de digitação. | Redução no tempo de triagem por caminhão. |
| **Otimização de Suprimentos** | Dashboards visuais em tempo real via LiveCharts2 expõem os lotes de matéria-prima mais poluentes, permitindo troca estratégica por fornecedores mais econômicos. | Economia na escolha de insumos ecológicos. |

---

# 📖 Regra de Negócio: Validação Restritiva de Chassis (VIN)

## 1. Visão Geral
No contexto do ecossistema **Iveco Green Ledger**, é estritamente proibido o registo de componentes logísticos ou a geração de métricas ambientais (Escopo 3) para veículos que não pertençam à fabricante **IVECO**. Esta regra garante a integridade dos relatórios ESG e evita a contaminação da base de dados corporativa com veículos de terceiros.

## 2. Atores e Componentes Envolvidos
* **Ator:** Operador de Pátio (via cliente desktop WPF).
* **Sistema Interno:** Back-End em ASP.NET Core 8 (`DadosController` e `DadosService`).
* **Serviço Externo:** API Pública da NHTSA (National Highway Traffic Safety Administration).

## 3. Fluxo de Validação e Critérios de Aceitação
A validação de um chassi segue um pipeline de verificações síncronas/assíncronas antes da persistência no banco de dados NoSQL (Firebase Firestore):

### Passo 3.1: Validação de Fronteira (Cliente/API)
* O utilizador introduz o código VIN.
* O sistema verifica o tamanho da string.
* **Critério:** O código VIN deve conter **exatamente 17 caracteres**.
* **Falha:** Retorna `HTTP 400 (Bad Request)` com a mensagem "O VIN deve ter 17 caracteres."

### Passo 3.2: Higienização de Dados (Sanitization)
* Antes de enviar para a entidade externa, o VIN sofre um tratamento.
* **Ação:** Remoção de espaços em branco (`Trim`) e conversão de todos os caracteres para maiúsculas (`ToUpper`).

### Passo 3.3: Auditoria Industrial Externa (Integração NHTSA)
* O Back-End consome o endpoint `decodevin` da API VPIC da NHTSA.
* O payload JSON devolvido é analisado em busca da variável correspondente à Marca (`Make`).
* **Critério de Sucesso:** O valor retornado no campo Marca deve conter obrigatoriamente a substring **"IVECO"**.
* **Critério de Rejeição:** Se a marca for nula, vazia ou diferente de IVECO (ex: Volvo, Scania, Ford).

### Passo 3.4: Resolução
* **Em caso de Sucesso:** O sistema extrai o modelo do veículo (ou define um genérico caso a API não o forneça) e aprova a transação, avançando para a criação do veículo no Firebase e posterior vínculo das peças/lotes.
* **Em caso de Falha:** O motor de regras de negócio dispara uma exceção crítica. A camada Controller interceta a exceção de forma amigável e retorna o erro para a aplicação WPF, impedindo instantaneamente a continuação da linha de montagem e exibindo a marca não autorizada detetada.

## 4. Tratamento de Erros e Códigos HTTP

| Cenário de Erro | Código HTTP Retornado | Comportamento do Sistema |
| :--- | :---: | :--- |
| VIN nulo ou em branco | `400 Bad Request` | Rejeição instantânea sem chamada à rede. |
| VIN diferente de 17 caracteres | `400 Bad Request` | Rejeição instantânea sem chamada à rede. |
| Marca detetada não é IVECO | `400 Bad Request` | Controller captura a `Exception` do Service e notifica o Operador com a marca incorreta detetada. |
| Falha na ligação com a NHTSA | `500 / 503` | O sistema notifica indisponibilidade temporária. (Nota: Aqui entra a contingência *Offline-Safe* no lado do Cliente WPF). |

## 5. Implementação Técnica de Referência
A salvaguarda desta regra está codificada no serviço `DadosService.cs`, garantindo que a regra nunca seja contornada, independentemente do cliente que chame a API:

```csharp
// Extração da Marca
var marca = data.Results.FirstOrDefault(r => r.Variable == "Make")?.Value;

// VALIDAÇÃO CRÍTICA (Regra de Negócio)
if (string.IsNullOrEmpty(marca) || !marca.ToUpper().Contains("IVECO"))
{
    throw new Exception($"VIN inválido para este sistema. A marca detetada foi: {marca ?? "Desconhecida"}. Apenas veículos IVECO são permitidos.");
}

---

# 📖 Regra de Negócio: Cálculo Ambiental e Emissões

## 1. Visão Geral
Para garantir a conformidade com as métricas corporativas de governança ambiental (ESG), o ecossistema **Iveco Green Ledger** automatiza o cálculo da pegada de carbono indireta da cadeia de suprimentos. Esta regra dita que o impacto ecológico de cada veículo fabricado deve ser apurado de forma individualizada, cruzando a massa física das peças instaladas com os indicadores ambientais dos seus lotes de origem, seguindo estritamente as diretrizes do **GHG Protocol**.

## 2. Atores e Componentes Envolvidos
* **Sistema Interno:** Back-End em ASP.NET Core 8 (Motor de Cálculo no `DadosService` e Endpoint no `DadosController`).
* **Base de Dados:** Firebase Firestore (Coleções `veiculo_componentes` e `lotes_materia_prima`).
* **Interface Visual (Frontend):** Painel Desktop WPF (`AnalisesViewModel` e gráficos `LiveCharts2`).

## 3. Fluxo de Cálculo e Critérios de Aceitação
O processamento do balanço ecológico por chassi (VIN) ocorre de forma assíncrona, obedecendo ao seguinte pipeline lógico:

### Passo 3.1: Identificação da Árvore de Componentes
* O sistema recebe a solicitação de auditoria ESG para um determinado número de chassi (VIN).
* **Ação:** O Back-End varre a coleção de componentes e isola todas as peças cuja chave estrangeira (`fk_Veiculo_Vin`) corresponda ao chassi solicitado.
* **Falha/Critério:** Se o chassi não possuir peças associadas, a pegada de carbono retornada é automaticamente `0` (zero).

### Passo 3.2: Rastreabilidade de Lotes e Fatores de Emissão
* Para cada peça associada ao chassi, o motor de cálculo identifica o lote logístico de origem (`fk_LoteMateriaPrima_Id`).
* O sistema extrai o indicador de pegada ecológica real do material utilizado nesse lote (ex: Kg de CO₂ equivalente por cada Kg de matéria-prima).

### Passo 3.3: Aplicação do Motor Algorítmico e Contingência
* O sistema aplica a fórmula padrão de Escopo 3: **Massa Física da Peça (kg) × Fator de Emissão do Lote**.
* **Regra de Contingência:** Caso a peça esteja associada a um lote que foi removido, ou se o lote não possuir um indicador ambiental registado, o sistema aplica um **fator de emissão padrão/contingência de 2.5 kg CO₂e/kg** (valor típico para componentes metálicos automotivos), garantindo que a métrica ESG nunca seja subnotificada.

### Passo 3.4: Consolidação ESG
* O sistema soma os valores processados de todas as peças e consolida a Pegada de Carbono Total (em Kg) daquele veículo.
* O valor final é devolvido via payload JSON para que o Frontend (`AnalisesViewModel`) efetue a plotagem em tempo real no dashboard.

## 4. Matriz de Tratamento Algorítmico

| Cenário de Cálculo | Fator Utilizado | Ação do Motor Algorítmico |
| :--- | :--- | :--- |
| Peça associada a Lote válido | Fator Real do Lote (`PegadaCarbonoPorKg`) | Multiplica o peso da peça (Kg) pelo fator específico do lote de matéria-prima. |
| Peça com Lote inexistente/inválido | Fator de Contingência (`2.5`) | Aplica o multiplicador padrão de segurança (2.5) pelo peso da peça para evitar evasão ESG. |
| Chassi sem peças associadas | `0` | Retorna zero, indicando que o veículo ainda não iniciou a montagem física no pátio. |

## 5. Implementação Técnica de Referência
A garantia do vínculo indissociável da pegada de carbono por VIN encontra-se no serviço `DadosService.cs`, através da seguinte arquitetura algorítmica:

```csharp
double pegadaTotalVeiculo = 0;
const double FatorEmissaoContingencia = 2.5; // Fator padrão (metais) para ausência de lote

foreach (var comp in componentesDoVeiculo)
{
    // Rastreia o lote de origem da matéria-prima
    var loteOrigem = lotes.FirstOrDefault(l => l.Id == comp.fk_LoteMateriaPrima_Id);

    // Valida a contingência caso falhe o fator real
    double fatorEmissao = loteOrigem != null ? loteOrigem.PegadaCarbonoPorKg : FatorEmissaoContingencia;
    
    // Aplica o motor matemático GHG Protocol
    pegadaTotalVeiculo += comp.PesoKg * fatorEmissao;
}

return pegadaTotalVeiculo;

  

*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 16 de junho de 2026.*
