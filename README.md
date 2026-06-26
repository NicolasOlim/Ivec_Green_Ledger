# 📦🍃 Iveco Green Ledger – Sistema de Rastreamento Inteligente  
**Trabalho de Conclusão de Curso**  
**Unidade SENAI: Nova Lima**

**Instrutor: Frederico Martins Aguiar**

**Equipe de Desenvolvimento**  

[🧑‍💻 Alice Andrade](https://github.com/aliceandradee)  |  [🧑‍💻 Erick Silva](https://github.com/erick190813)  | [🧑‍💻 Nicolas Oliveira Lima](https://github.com/NicolasOlim) | [🧑‍💻 Vinicius Augusto](https://github.com/vnxtry)  

---

<div class="logo-container">
    <img src="imagens/Iveco_greenLogo.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

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

### Objetivo Geral:

Desenvolver uma plataforma tecnológica para a gestão inteligente, cubagem volumétrica e rastreabilidade ambiental de insumos automotivos, composta por uma API, um simulador de sensores IoT industriais e um cliente em WPF estruturado no padrão MVVM, com integração a serviços externos de validação regulatória e persistência em nuvem.

### Objetivos específicos:

 - **Projetar e construir a interface de chão de fábrica:** Desenvolver o cliente desktop utilizando o framework WPF sob o padrão arquitetural MVVM (Model-View-ViewModel), garantindo uma experiência de usuário fluida, intuitiva e adaptada à rotina operacional dos operadores de recebimento logístico.

 - **Automatizar as validações regulatórias e fiscais:** Integrar o back-end a APIs públicas (como BrasilAPI para dados cadastrais e fiscais de fornecedores e NHTSA para a decodificação técnica do código VIN), eliminando a necessidade de inputs manuais suscetíveis a falhas humanas.

- **Centralizar a inteligência analítica na nuvem:** Estruturar a persistência não relacional (NoSQL) no Firebase Firestore e desenvolver um back-end em ASP.NET Core 8 responsável pelo processamento assíncrono, sincronização dos dados locais e orquestração do ecossistema.

- **Prover transparência analítica para governança ESG:** Implementar dashboards dinâmicos em tempo real utilizando a biblioteca LiveCharts2, permitindo a geração de relatórios de conformidade ambiental auditáveis para a tomada de decisões gerenciais e estratégicas.


---

## Metodologia:

O desenvolvimento do ecossistema foi estruturado de forma interativa, sendo dividido em três fases principais: o levantamento de requisitos e modelagem (Fase 1), focado nas regras de negócio. O desenvolvimento da interface e persistência (Fase 2), voltado à construção do cliente WPF, tabelas pelo SQLite e dashboards analíticos. E a construção da API REST com integridade na nuvem (Fase 3), englobando o motor do GHG Protocol, as logs e as integrações assíncronas com as API's. O controle de versão e o gerenciamento do código-fonte foram centralizados na plataforma GitHub, garantindo um histórico de desenvolvimento consistente, seguro e colaborativo entre a equipe.

---

## Mini Mundo Da Demanda:

Este capítulo descreve o contexto organizacional que motivou o desenvolvimento da Green Ledger, os atores envolvidos e as regras de negócio que orientaram a modelagem do sistema. 

### Contexto Organizacional:

Para sanar os gargalos operacionais e ambientais identificados da empresa Iveco, sendo assim foi padronizado uma arquitetura de serviços desacoplados e persistência balanceada. O ciclo de vida do dado inicia-se com o mapeamento físico de insumos e a validação do chassi no fluxo de montagem, onde o administrador pré-configura os índices de emissão do GHG Protocol e homologa os fornecedores na nuvem. Quando o operador executa o recebimento logístico de um material, o sistema captura suas credenciais a partir da camada de autenticação e cria um registro de telemetria local, realizando o cruzamento de cardinalidade entre os dados de cubagem e a árvore de peças vinculadas ao veículo por meio da tabela associativa.


### **Usuário do Sistema**:

- Realiza autenticação no sistema por meio de credenciais cadastradas e validadas;
- Cadastra os lotes de matéria-prima recebidos, informando os dados de cubagem e massa em quilogramas;
- Consulta o histórico e o status de sincronização;
- Associa as peças e os componentes industriais ao número de chassi correspondente por meio do código identificador VIN de 17 caracteres;
- Monitora indicadores de emissões de carbono nos painéis visuais e gera relatórios em formato PDF.

### **Administrador**:

- Gerencia os usuários do sistema;
- Gerencia os fornecedores parceiros e os parâmetros de índices referente á emissão;
- Emite relatórios ambientais e gerencia log.

### **Sistema Green Ledger**:

- Controla a cubagem volumétrica e o inventário de insumos automotivos;
- Mapeia e vincula e as emissões de carbono a cada chassi de veículo produzido;
- Sincroniza os registros de forma assíncrona com a nuvem;
- Valida dados de fornecedores;
- Gera relatórios em formato PDF.

### Regras de Negócio:

- 1: Um lote de matéria-prima pertence obrigatoriamente a um fornecedor;
- 2: Exige a validação cadastral do CNPJ do fornecedor;
- 3: Um componente ou insumo deve estar associado a uma categoria de material; 
- 4: Um chassi de veículo deve ser validado por meio do VIN (17 caracteres);
- 5: Um lote de matéria-prima pode ser vinculado a múltiplos chassis, assim como um chassi consome múltiplos lotes de materiais;
- 6: O sistema registra automaticamente a data, hora exata e o usuário responsável por cada operação;
- 7: O ecossistema controla o ciclo de vida e a integridade de cada registro;

---

## Modelagem do banco de dados:

Este capítulo apresenta os três níveis de modelagem do banco de dados do sistema Green Ledger: conceitual, lógico e físico, conforme as metodologias de modelagem relacional adotadas no curso. 

### **Modelo Conceitual (DER) :**

O modelo conceitual representa as entidades do domínio e seus relacionamentos em nível de abstração, sem preocupação com tipos de dados ou chaves de implementação. A modelagem segue a notação do BRModelo, utilizando Diagrama Entidade- Relacionamento (DER). 

### **Entidades e Atributos:**

#### Tabela - USUARIO
| Atributo | Tipo/papel | Observação |
| :--- | :--- | :--- |
| **id** | PK | Identificador único do usuário |
| **nome** | Atributo | Nome completo do usuário |
| **email** | Atributo | Email de contato corporativo |
| **senhaHash** | Atributo | Senha criptografada para acesso |
| **perfil** | Atributo | Permissão para acesso ao sistema |

---

#### Tabela - FORNECEDOR
| Atributo | Tipo/papel | Observação |
| :--- | :--- | :--- |
| **id** | PK | Identificador único do fornecedor |
| **cnpj** | Atributo | CNPJ do fornecedor |
| **razaoSocial** | Atributo | Razão Social ou nome empresarial |
| **status** | Atributo | Estado lógico do fornecedor no sistema |

---

#### Tabela - LOTE_MATERIA_PRIMA
| Atributo | Tipo/papel | Observação |
| :--- | :--- | :--- |
| **id** | PK | Identificador único do lote |
| **fk_fornecedor** | FK | Chave estrangeira que referencia da tabela fornecedor |
| **tipoMaterial** | Atributo | Descrição do material recebido |
| **quantidadeKg** | Atributo | Massa total do lote em Kg |
| **pegadaCarbonoPorKg** | Atributo | Fator de emissão de carbono po Kg |
| **dataProducao** | Atributo | Data e hora em que o lote foi produzido |

---

#### Tabela - Veiculo
| Atributo | Tipo/papel | Observação |
| :--- | :--- | :--- |
| **vin** | PK | Identificação do veículo |
| **modelo** | Atributo | Nome do veículo |
| **marca** | Atributo | Fabricante do Veículo |
| **dataMontagem** | Atributo | Data e Hora que o veículo entrou na linha de montagem |

#### Tabela - Veiculo_Componente
| Atributo | Tipo/papel | Observação |
| :--- | :--- | :--- |
| **id** | PK | Identificação do veículo do componente |
| **fk_veiculo_vin** | FK | Chave estrangeira que relacionada a tabela VEICULO |
| **fk_lotemateria_id** | PK | Chave estrangeira que relacionada a tabela LOTE_MATERIA_PRIMA |
| **nomeComponente** | Atributo | Nome da peça instalada |
| **pesoKg** | Atributo | Peso Físico da peça e componente |
| **totalCO2eCalculado** | Atributo | Total de carbono calculado para essa peça |

---

### **Relacionamentos:**

Os relacionamentos entre as entidades do sistema são definidos a seguir: 

| Atributo | Tipo/papel | Semântica |
| :--- | :--- | :--- |
| **FORNECEDOR — LOTE_MATERIA_PRIMA** | 1 : N | Um fornecedor pode fornecer vários lotes de matéria-prima, mas um lote pertence obrigatoriamente a um único fornecedor. |
| **VEICULO — VEICULO_COMPONENTE** | 1 : N | Um veículo pode ser associado a vários componentes na linha de montagem. |
| **LOTE_MATERIA_PRIMA — VEICULO_COMPONENTE** | 1 : N | Um lote de matéria-prima pode dar origem ou fornecer insumos para vários registros de componentes aplicados. |
| **VEICULO — LOTE_MATERIA_PRIMA** | N:M | Um veículo consome insumos de vários lotes de matéria-prima, e um lote de matéria-prima pode ser distribuído entre múltiplos veículos. |

---

## Modelo Lógico:

O modelo lógico do ecossistema converte as entidades conceituais em estruturas relacionais normatizadas, definindo as chaves primárias (PK), chaves estrangeiras (FK) e restrições de integridade de cada atributo. A tipagem de identificação foi padronizada como TEXT de forma unificada entre os campos de amarração (id, fk_fornedor, vin, fk_veiculo_vin, fk_loteMateriaPrima_id).

### **Tabela: Usuário**
| Coluna | Chave/Relacionamento | Tipo | Descrição |
| :--- | :--- | :--- | :--- |
| **id** | PK | TEXT | Identificador único do usuário |
| **nome** | - | TEXT | Nome do usuário |
| **email** | - | TEXT | Email de contato corporativo |
| **senhaHash** | - | TEXT | Senha para a autenticação |
| **perfil** | - | TEXT | Nível de permissão |

---

#### Tabela - Fornecedor
| Coluna | Chave/relacionamento | Tipo | Descrição |
| :--- | :--- | :--- |  :--- |
| **id** | PK | TEXT | Identificador único do fornecedor |
| **cnpj** | - | TEXT | CNPJ da empresa |
| **razaosocial** | - | TEXT | Nome empresarial |
| **status** | - | TEXT | Estado do cadastro |
| **perfil** | - | TEXT | Nivel de permissão |

---

#### Tabela - Lote Materia Prima
| Coluna | Chave/relacionamento | Tipo | Descrição |
| :--- | :--- | :--- | :--- |
| **vin** | PK | TEXT | Número de identificação do chassi (17 caracteres) |
| **modelo** | - | TEXT | Modelo do veículo comercial |
| **marca** | - | TEXT | Fabricante do automóvel |
| **dataMontagem** | - | DATETIME | Data e hora de entrada para a montagem |

---

#### Tabela - Veiculo_componente
| Coluna | Chave/relacionamento | Tipo | Descrição |
| :--- | :--- | :---  | :--- |
| **id** | PK | TEXT | Identificador único do veículo |
| **fk_veiculo_vin** | FK | TEXT | Referencia Veiculo (Vin) |
| **fk_loteMateriaPrima_id** | FK | TEXT | Referencia Lote_materia_prima |
| **nomeComponente** | - | REAL | Nome da peça em quilogramas |
| **pesoKg** | - | REAL | Peso de peça em quilogramas |
| **totalCO2Calculado** | - | - | Total de carbono calculado para essa peça |

---

## Arquitetura do Sistema:

O Green Ledger adota uma arquitetura distribuída e desacoplada, separando claramente as responsabilidades entre o cliente desktop de pátio, a API REST corporativa e a camada de persistência híbrida. Esta seção descreve cada componente técnico, suas integrações de borda com serviços externos e os padrões de resiliência a falhas de rede.

**Diagrama de Caso de Uso**

### **Visão Geral da Arquitetura:**

<div class="logo-container">
    <img src="imagens/diagramacasodeuso.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

---

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

### Padrão MVVM no cliente WPF:

O cliente desktop segue rigorosamente o padrão MVVM (Model-View-ViewModel), complementado por uma camada de serviços, estabelecendo quatro divisões claras de responsabilidade:

### **Base ViewModel e INotifyPropertyChanged:**

A BaseViewModel atua como a classe mãe de todas as ViewModels do projeto, centralizando a lógica de comunicação reativa com a interface. Sua principal função é herdar e implementar a interface INotifyPropertyChanged, expondo o evento PropertyChanged. Através de um método auxiliar (geralmente chamado OnPropertyChanged ou RaisePropertyChanged), ela dispara um alerta para a View sempre que o valor de uma propriedade do Model ou do estado interno é alterado pelo operador ou por um processo assíncrono. Isso ativa o mecanismo de Data Binding bidirecional do WPF, garantindo que as telas atualizem seus elementos gráficos em tempo real de forma automática, mantendo o código totalmente limpo e livre de acoplamento visual.

### **Relay Command:**

O RelayCommand é a implementação concreta da interface ICommand no padrão MVVM do WPF, atuando como o gatilho lógico que conecta as ações do operador à ViewModel. Em vez de criar uma classe separada para cada botão do chão de fábrica, ele encapsula os métodos da ViewModel por meio de delegados (Action e Func<bool>), permitindo que cliques, atalhos e eventos visuais invoquem a lógica de negócios diretamente no C#. Ele gerencia duas funções essenciais: a execução da ação propriamente dita (através do método Execute) e a verificação dinâmica se aquela ação é permitida no momento (através do CanExecute), o que habilita ou desabilita automaticamente os controles da interface gráfica (como travar o botão de salvar enquanto os campos obrigatórios do fornecedor não forem validados). Seguindo a seguinte estrutura:

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
## API REST - Endpoits e Integração:

A API REST da ApiIveco é desenvolvida em ASP.NET e exposta no domínio corporativo do ecossistema. Ela é responsável por receber as requisições assíncronas do cliente desktop WPF, processar as regras de negócio automotivas, como os cálculos de emissões e validações integradas à BrasilAPI e NHTSA, e persistir os dados consolidados no banco de dados em nuvem Firebase Firestore. Sendo implementados os seguintes endpoits:

| Método | Endpoint | Descrição |
| :--- | :--- | :--- | 
| **POST** | /api/usuario/login | Realiza a autenticação do usuário e retorna as permissões de perfil (Operador/Admin) | 
| **GET** | /api/usuario | Lista todos os usuários cadastrados (Restrito ao perfil Admin) |
| **GET** | /api/fornecedor | Lista todos os fornecedores parceiros com filtros por status | 
| **POST** | /api/fornecedor | Cadastra um novo fornecedor — consome a BrasilAPI para validação cadastral automática via CNPJ | 
| **GET** | /api/lote-materia-prima | Lista os lotes de matéria-prima recebidos no pátio | 
| **POST** | /api/lote-materia-prima | Cadastra um novo lote de entrada e calcula a pegada de carbono inicial baseada no fator de emissão | 
| **GET** | /api/veiculo/{vin} | Consulta os dados técnicos de um veículo específico pelo chassi | 
| **POST** | /api/veiculo | Registra a entrada de um novo chassi — consome a API da NHTSA para decodificação internacional do VIN | 
| **POST** | /api/veiculo-componente | Associa componentes/lotes a um veículo, processa o cálculo final do GHG Protocol e consolida o registro no Firebase | 

### **Fluxo Detalhado:**

- 1: O operador digita o chassi do veículo e clica em "Validar VIN" ;
- 2: Aciona o ValidarVinCommand;
- 3:  Invoca o método assíncrono correspondente dentro do ApiService;
- 4: Envia uma requisição HTTP POST contendo o código VIN (JSON) para o endpoint;
- 5: API recebe a requisição e repassa o chassi para a camada interna;
- 6: A API constrói uma requisição HTTP segura direcionada para os endpoints da API da NHTSA;
- 7: A API da NHTSA processa a decodificação dos 17 caracteres e retorna os dados técnicos do chassi;
- 8: A ApiIveco valida a integridade dos dados retornados e persiste o novo veículo no banco de dados em nuvem Firebase Firestore;
- 9: A  API backend responde à aplicação desktop retornando o status de sucesso e os dados decodificados em formato JSON;
- 10: O ApiService no WPF recebe o JSON e xibindo os dados técnicos validados;


### **Swagger:**

A Documentação Swagger é o portal interativo da ApiIveco, integrado nativamente ao ecossistema ASP.NET Core 8. Ela elimina a necessidade de manuais externos estáticos ao gerar automaticamente uma interface web dinâmica que mapeia todos os endpoints RESTful do sistema. Através do Swagger, desenvolvedores e engenheiros de software conseguem visualizar instantaneamente os contratos de entrada e saída (JSON), os códigos de status HTTP esperados (como 200 OK, 400 BadRequest ou 401 Unauthorized) e os esquemas exatos das entidades de domínio. Além de servir como uma especificação viva e sempre atualizada do backend, a interface permite realizar testes de requisições em tempo real diretamente pelo navegador, agilizando drasticamente a integração com o cliente desktop WPF e com os serviços externos do Mercado Livre, BrasilAPI e NHTSA.

---

## Viabilidade Técnica:

A análise de viabilidade técnica avalia se o sistema Iveco Green Ledger pode ser desenvolvido, implantado e mantido com os recursos tecnológicos selecionados, considerando o contexto de desenvolvimento da solução .NET unificada e a realidade operacional do chão de fábrica e do pátio logístico a que se destina.

### **Infraestruturas e Tecnologias:**

| Componente | Licença | Maturidade | Observação |
| :--- | :--- | :--- | :--- | 
| **C# / .NET 8** | MIT(open-source) | Alta | Plataforma Microsoft estável, unificada e com suporte LTS (Long-Term Support) garantido. |
| **ASP.NET Core** |  MIT(open-source) | Alta | Framework web de alto desempenho, escalável e utilizado globalmente para APIs empresariais |
| **WPF** | MIT(open-source) | Alta | Framework nativo Windows estável, ideal para aplicações de pátio industrial com interface robusta |
| **Firebase Firestore** | Gratuito (Plano Spark) | Alta | Banco de dados NoSQL do Google na nuvem com 99,95% de SLA e sincronização em tempo real |
| **SQLite** | Domínio Público | Alta | Banco de dados embutido leve e ultrarrápido, ideal para contingência offline no cliente desktop |
| **BrasilAPI** | MIT(open-source) | Alta | Serviço gratuito e integrado de alta disponibilidade para checagem cadastral automatizada de CNPJ |
| **API da NHTSA** | Domínio Público | Alta | Base governamental norte-americana padrão global para decodificação técnica e validação de VIN (Chassi) |
| **Swagger / Open API** | Apache 2.0 | Alta | Padrão internacional de mercado adotado na ApiIveco para documentação e testes de endpoints REST |
| **Github** | Gratuito | Alta | Plataforme de controle de versão do mercado |

### **Requisitos Mínimos de Hardware:**

Para execução do sistema em ambiente de produção, os  requisitos mínimos recomendados são: 

- **Processador:** Intel Core i3;
- **Memória RAM:** 4 GB;
- **Armazenamento:** 500MB de espaço em disco disponível;
- **Sistema Operacional:** Windows 10 ou Windows 11(64 - bits);
- **Conexão com a internet:** Para o funcionamento das integrações em nuvem.

### **Requisitos Mínimos de Software:**

Para execução do sistema em ambiente de produção, os  requisitos mínimos recomendados são: 

- **Microsoft .NET Runtime 8.0:** Requisito obrigatório
- **ASP.NET Core Runtime 8.0:** Necessário para a hospedagem do servidor;
- **Navegador WEB:**  500MB de espaço em disco disponível;
- **Sistema Operacional:** Windows 10 ou Windows 11(64 - bits);
- **Conexão com a internet:** Para o funcionamento das integrações em nuvem.

### **Riscos Técnicos e Mistigações:**

| Risco | Probabilidade | Impacto | Mistigação |
| :--- | :--- | :--- | :--- | 
| Cota gratuita do Firebase excedida com alto volume de dados | Média | Alta | Migrar para o plano Blaze (pay-as-you-go) do Firebase para suportar a escala industrial ou migrar a persistência de longo prazo para uma instância dedicada de banco de dados relacional próprio |
| Cota gratuita ou limites de requisições das APIs externas |  Média | Médio | Implementar uma camada de cache local no banco SQLite da estação de trabalho e na API corporativa, evitando chamadas repetidas para o mesmo CNPJ ou mesmo chassi (VIN) já validados |
| Interrupção de conectividade | Alta | Médio | Utilização automática da camada de persistência em borda com SQLite. O cliente desktop armazena os registros localmente |
| Incompatibilidade ou quebra de contrato em atualizações das API’s | Baixa | Médio | Criação de contratos de integração isolados por meio de interfaces (Services/Interface/) na ApiIveco, permitindo a substituição do provedor de dados de chassi ou CNPJ com impacto zero no cliente WPF |

---
## Escabilidade:

A arquitetura adotada para o Iveco Green Ledger foi projetada estrategicamente para suportar o crescimento gradual do volume de dados e de acessos no pátio logístico sem a necessidade de refatorações estruturais complexas. Os principais aspectos de escalabilidade são:

| Componente | Tipo | Mecanismo de Expansão | Impacto no crescimento de sistema |
| :--- | :--- | :--- | :--- | 
| API REST(ApiIveco) | Horizontal (Stateless) | Adição de novas instâncias da API em paralelo por trás de um balanceador de carga |Suporta o aumento exponencial de requisições |
| Firebase Firestore |  Automática (Nuvem) | Divisão automática de dados | Mantém o tempo de resposta das consultas linear e estável |
| Interface Desktop | Funcional (Padrão MVVM) | Desacoplamento | Permite a acoplagem de novos módulos operacionais |
| Integração com Api’s Externas | Arquitetura | Interfaces abstratas | Facilita a substituição ou adição de novos provedores de dados com impacto zero no cliente desktop |
| Persistência em Borda (SQLite) | Descentralizada | Distribuição da carga| Evita gargalos de escrita e sobrecarga no servidor principal |

---

### **WPF e MVVM:**

O WPF (Windows Presentation Foundation) combinado com o padrão arquitetural MVVM (Model-View-ViewModel) estabelece uma separação rígida e clara entre a interface gráfica com o usuário (UI) e a lógica de apresentação do sistema. No terminal de pátio industrial da solução, as telas em XAML (Views) são completamente desacopladas das regras de negócio. Essa comunicação ocorre de forma reativa e assíncrona por meio de mecanismos nativos de Data Binding e do disparo de comandos (Commands), que interagem diretamente com a ViewModel. A ViewModel, por sua vez, atua como o cérebro da interface: ela consome os serviços locais (como o banco de contingência SQLite) e os endpoints da ApiIveco, expõe propriedades observáveis que notificam a tela sobre mudanças de estado e atualiza os dados operacionais em tempo real para o operador, garantindo uma aplicação robusta, altamente testável e de fácil manutenção.

### **ASP.NET Core Web API:**

A ASP.NET Core Web API constitui o núcleo unificado de serviços e inteligência de negócio do ecossistema do projeto. Desenvolvida sob uma arquitetura stateless (sem retenção de estado), ela atua como uma ponte segura e de alto desempenho entre o cliente desktop WPF e os provedores de persistência e validação. A API é estruturada rigidamente sob o padrão de separação entre controladores (Controllers), que expõem os endpoints RESTful e gerenciam as requisições HTTP (JSON), e serviços (Services), responsáveis por processar as regras de negócio complexas — como os motores de cálculo de indicadores ecológicos e pegada de carbono baseados no GHG Protocol. Ao centralizar o consumo dos serviços externos da BrasilAPI, API da NHTSA e API de rastreamento do Mercado Livre, a ASP.NET Core Web API blinda a aplicação cliente de pátio contra instabilidades externas, padroniza as respostas de dados e garante a consolidação íntegra de todos os registros históricos no banco de dados em nuvem Firebase Firestore.

### **Firebase Firestore:**

O Firebase Firestore constitui a camada principal de persistência global e consolidação de dados em nuvem do ecossistema. Estruturado como um banco de dados NoSQL orientado a documentos, ele organiza as informações críticas do sistema — como o histórico de auditoria de chassis (VIN), o cadastro de fornecedores e os registros logísticos de rastreamento do Mercado Livre — em coleções flexíveis de alta escalabilidade. A escolha pelo Firestore elimina a complexidade de gerenciamento de infraestrutura física de servidores de banco de dados, oferecendo sincronização assíncrona, altíssima disponibilidade e mecanismos nativos de segurança baseados em regras de acesso. Ao receber as requisições tratadas e validadas pela ApiIveco, o Firestore consolida de forma definitiva os cálculos de emissões de carbono em conformidade com o GHG Protocol, servindo como uma fonte única da verdade para a geração de relatórios ecológicos e auditorias corporativas. 

### **BrasilAPI:**

O Firebase Firestore constitui a camada principal de persistência global e consolidação de dados em nuvem do ecossistema. Estruturado como um banco de dados NoSQL orientado a documentos, ele organiza as informações críticas do sistema — como o histórico de auditoria de chassis (VIN), o cadastro de fornecedores e os registros logísticos de rastreamento do Mercado Livre — em coleções flexíveis de alta escalabilidade. A escolha pela BrasilAPI garante uma infraestrutura de altíssima disponibilidade, sem custos de licenciamento ou limites restritivos de uso que inviabilizariam a operação. Ao centralizar essa validação no back-end, o sistema assegura que apenas cadastros íntegros, atualizados e em conformidade fiscal e jurídica sejam consolidados no banco de dados em nuvem, otimizando o fluxo de triagem no pátio industrial e blindando o cliente desktop de oscilações ou complexidades de integração direta com órgãos governamentais.

### **NHTSA Responsive:**

A API da NHTSA (National Highway Traffic Safety Administration) é o serviço internacional de utilidade pública integrado ao ecossistema do projeto para a validação e decodificação técnica automatizada de veículos. Consumida de forma assíncrona pela ApiIveco por meio do protocolo HTTPS, ela atua como uma barreira de segurança e conformidade na triagem de entrada do pátio logístico. A API da NHTSA é o serviço internacional de utilidade pública integrado ao ecossistema do projeto para a validação e decodificação técnica automatizada de veículos. Consumida de forma assíncrona pela ApiIveco por meio do protocolo HTTPS, ela atua como uma barreira de segurança e conformidade na triagem de entrada do pátio logístico.

### **API Mercado Livre:**

O Firebase Firestore constitui a camada principal de persistência global e consolidação de dados em nuvem do ecossistema. Estruturado como um banco de dados NoSQL orientado a documentos, ele organiza as informações críticas do sistema — como o histórico de auditoria de chassis (VIN), o cadastro de fornecedores e os registros logísticos de rastreamento do Mercado Livre — em coleções flexíveis de alta escalabilidade. Ao centralizar o consumo dos serviços externos da BrasilAPI, API da NHTSA e API de rastreamento do Mercado Livre, a ASP.NET Core Web API blinda a aplicação cliente de pátio contra instabilidades externas, padroniza as respostas de dados e garante a consolidação íntegra de todos os registros históricos no banco de dados em nuvem Firebase Firestore.

---
## Arquitetura Do Projeto MVVM:

O padrão arquitetural MVVM (Model-View-ViewModel) foi adotado no desenvolvimento do cliente desktop para garantir o completo desacoplamento entre a interface gráfica com o usuário e as regras de apresentação da aplicação de pátio. Abaixo está o fluxo de comunicação entre as camadas da nossa aplicação.

```js
// ==========================================
// 1. COLEÇÃO: fornecedores
// ==========================================
{
  "id_fornecedor": "fb4b6c12-32a1-4b10-8b9f-09e8d7c6b5a4", // UUID v4 de Amarração
  "cnpj": "12.345.678/0001-99",
  "razaoSocial": "Metalúrgica Estrutural S.A.",
  "status": "Ativo",
  "dataHomologacao": "2026-01-15T10:00:00Z"
}

// ==========================================
// 2. COLEÇÃO: lotes
// ==========================================
{
  "id_lote": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d", // UUID v4 de Amarração
  "fk_fornecedor": "fb4b6c12-32a1-4b10-8b9f-09e8d7c6b5a4", // Elo com o Fornecedor
  "tipoMaterial": "Aço Estrutural",
  "quantidadeKg": 5000.00,
  "pegadaCarbonoPorKg": 1.82, // Coeficiente para cálculo do GHG Protocol
  "dataProducao": "2026-06-23T19:30:00Z"
}

// ==========================================
// 3. COLEÇÃO: veiculos_emissoes
// ==========================================
{
  "_id": "93DIVECO123456789", // O próprio VIN (Chassi de 17 dígitos) atua como ID do Documento
  "modelo": "Iveco S-Way 540",
  "marca": "IVECO",
  "dataMontagem": "2026-06-23T19:34:00Z",
  "totalEmissaoScope3": 2.73, // tCO2e pré-calculado e consolidado pela ApiIveco
  "statusAuditoria": "Aprovado",
  
  // Árvore de Componentes Embutida (Desnormalização para eliminar JOINs)
  "componentes_instalados": [
    {
      "nomeComponente": "Longarina Esquerda",
      "fk_loteMateriaPrima_id": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d", // Elo com o Lote de origem
      "pesoKg": 750.00
    },
    {
      "nomeComponente": "Longarina Direita",
      "fk_loteMateriaPrima_id": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
      "pesoKg": 750.00
    }
  ]
}
```
---

## Dicionário de Dados do SQLite (Persistência Local):

#### Tabela – Estrutura Relacional de Contingência (SQLite - Local / WpfIveco)

| Tabela | Coluna | Tipo | Restrição | Descrição |
| :--- | :--- | :--- | :--- | :--- |
| **tb_componentes_contingencia** | id_componente | TEXT | PRIMARY KEY | Identificador único (GUID) gerado no chão de fábrica. |
| | fk_veiculo_vin | TEXT | NOT NULL | Código de chassi de 17 caracteres que receberá a peça. |
| | nome_componente | TEXT | NOT NULL | Descrição ou nome do insumo logístico associado. |
| | tipo_material | TEXT | NOT NULL | Categoria do material para o GHG Protocol (ex: Aço, Alumínio). |
| | peso_kg | REAL | NOT NULL | Massa física total aferida no recebimento de pátio. |
| | total_co2e | REAL | NOT NULL | Pré-cálculo local e temporário da pegada ecológica. |
| | data_recebimento | TEXT | NOT NULL | Registro de data e hora (Timestamp) da operação local. |
| | is_sincronizado | INTEGER | DEFAULT 0 | Flag de controle de rede (0 = Pendente de envio, 1 = Sincronizado na Nuvem). |

#### Tabela – Catálogo de Endpoints da API RESTful (ApiIveco)

| Método | Endpoint URI | Payload (Request) | Resposta (Response) | Descrição Operacional e Integração |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/api/auth/login` | JSON: `{"email": "...", "senha": "..."}` | JSON: `{"token": "...", "perfil": "..."}` | Realiza a autenticação e valida as permissões de acesso do operador ou administrador na base de usuários. |
| **POST** | `/api/fornecedores` | JSON: `{"cnpj": "..."}` | JSON: `{"id": "...", "razaoSocial": "...", "status": "Ativo"}` + HTTP 201 | Cadastra fornecedores parceiros consumindo síncronamente a **BrasilAPI** para automatizar os dados da Receita Federal. |
| **POST** | `/api/dados/validar-vin` | JSON: `{"vin": "..."}` | JSON: `{"vin": "...", "modelo": "...", "marca": "IVECO"}` + HTTP 200 | Validação de fronteira de chassi (17 caracteres). Consome a API da **NHTSA** para homologar a legitimidade industrial da marca. |
| **POST** | `/api/dados/calcular-escopo3` | JSON: `{"fk_veiculo_vin": "...", "componentes": [...]}` | JSON: `{"vin": "...", "totalEmissaoScope3": 142.50}` + HTTP 200 | Executa o motor algorítmico do **GHG Protocol** baseado na massa física e persiste o documento final no **Firebase Firestore**. |
| **GET** | `/api/relatorios/dossie/{vin}` | Parâmetro de Rota: `vin` (String) | Stream de Arquivo: `Dossie_Ambiental.pdf` + HTTP 200 | Varre a árvore de componentes daquele chassi e renderiza sob demanda o relatório fiscal paginado utilizando a engine do **QuestPDF**. |

---


## Arquitetura do Sistema e Stack Tecnológica:

Para atender aos rigorosos requisitos de escalabilidade, alta disponibilidade e isolamento de falhas exigidos pela indústria automobilística moderna, o ecossistema **Iveco Green Ledger** foi projetado sob o paradigma de sistemas distribuídos e arquitetura fracamente acoplada. A transição do modelo monolítico original para uma topologia dividida em subprojetos independentes permitiu a segregação clara de responsabilidades, onde cada componente opera como um nó especializado dentro da rede fabril.

A comunicação entre os módulos baseia-se no estilo arquitetural **REST (Representational State Transfer)**, utilizando cargas de dados estruturadas em formato **JSON** trafegadas via protocolos criptografados HTTP/HTTPS. Enquanto a camada de aquisição e simulação de dados atua na borda física do chão de fábrica, a intermediação lógica e o processamento analítico são centralizados em uma API de alta performance, que por sua vez se comunica de forma nativa e assíncrona (via gRPC protegido) com os servidores de nuvem.

Essa segmentação garante que picos de tráfego gerados pelos sensores de telemetria não impactem a renderização da interface gráfica do operador, e que manutenções evolutivas em qualquer uma das pontas possam ser homologadas sem a necessidade de paradas programadas no ecossistema completo. 

A solução foi dividida estrategicamente em três subprojetos independentes que formam o ecossistema e se comunicam de maneira segura, sob o suporte de um banco de dados hospedado em nuvem corporativa. A tabela abaixo resume as camadas do sistema e as respectivas ferramentas tecnológicas associadas:

#### Tabela 1: Matriz de Componentes e Stack Tecnológica do Ecossistema

| Componente | Tecnologia | Padrão / Framework | Função no Ecossistema |
| :--- | :--- | :--- | :--- |
| **ApiIveco** *(Back-End)* | .NET 8 / ASP.NET Core | REST API | Centraliza as regras de negócio, executa o cálculo do Escopo 3 (GHG Protocol) e isola o acesso seguro ao banco de dados global. |
| **WpfIveco** *(Front-End)* | C# / WPF | MVVM Toolkit | Interface visual reativa e de alta performance para monitoramento analítico e controle estatístico no pátio. |
| **SimuladorIveco** *(Utilitário)* | C# (Console Application) | Task Parallel Library | Simula dados de telemetria de sensores IoT industriais em tempo real e gerencia a fila de contingência local via SQLite. |
| **Firebase Firestore** *(Banco)* | Google Cloud NoSQL | Document Architecture | Camada de persistência global altamente escalável, distribuída e assíncrona orientada a documentos JSON. |

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

# Camadas do Projeto:

O ecossistema **Iveco Green Ledger** foi estruturado sob o princípio da separação de conceitos. Cada camada possui fronteiras lógicas bem definidas, comunicando-se por meio de interfaces de programação de aplicações (APIs) e contratos de dados rígidos, o que confere ao sistema modularidade, facilidade de manutenção e alta resiliência.

### Camada de Serviços Intermediários - API (`ApiIveco`)

A **ApiIveco** atua como a inteligência central e o núcleo de governança do ecossistema. Construída com a especificação técnica do **ASP.NET Core Web API** no ambiente **.NET 8 LTS**, ela baseia-se estritamente nas restrições de projeto do estilo arquitetural **REST**. 

#### Características Técnicas e Governança da API:
* **Isolamento de Persistência:** A API atua como um escudo de segurança (Proxy) para o banco de dados em nuvem. Nenhuma aplicação cliente (seja o terminal WPF ou o simulador de sensores) possui credenciais diretas de gravação no Firebase Firestore. O acesso é centralizado e auditado pelos controladores da API via políticas de controle de acesso do **Google Cloud**.
  
* **Contratos de Dados via DTOs:** A comunicação externa opera sobre os protocolos seguros HTTP/HTTPS utilizando Objetos de Transferência de Dados, serializados estritamente no formato **JSON**. O uso de DTOs impede a exposição direta das entidades de domínio do banco, reduz o payload trafegado na rede e protege o sistema contra vulnerabilidades de atribuição em massa.
  
* **Motor Integrado:** É nesta camada que reside a implementação algorítmica das equações de conversão do **GHG Protocol (Escopo 3)**. A API intercepta o peso bruto dos insumos em quilogramas enviados pelo pátio, localiza o coeficiente ecológico do lote do material e consolida o valor em toneladas de carbono equivalente antes de disparar o gatilho assíncrono de escrita na nuvem.

---

### Camada da Interface Visual - WPF (`WpfIveco`)

A aplicação **WpfIveco** representa a interface homem-máquina (IHM) de monitoramento e controle administrativo da linha de montagem. O desenvolvimento em WPF adota o padrão de arquitetura MVVM, erradicando o acoplamento entre os elementos gráficos e as regras de negócio.

#### Engenharia de Interface e Performance Visual:
* **Desacoplamento de Telas:** As interfaces visuais são declaradas puramente em sintaxe (Views). Toda a lógica de estado de tela, manipulação de dados em C# e chamadas de rede fica encapsulada nas (ViewModels). A interceptação de interações (como cliques em botões e gatilhos de gravação) utiliza o padrão *Command* por meio da classe especializada **RelayCommand**, eliminando regras de negócio de dentro dos arquivos de retaguarda das telas (`.xaml.cs`).

* **Sincronização Reativa e Data Binding:** O sincronismo entre as propriedades da ViewModel e os componentes visuais do XAML ocorre em tempo real de forma bidirecional. O mecanismo é governado por notificações estruturadas de mudança de estado, suportadas pela interface de infraestrutura **INotifyPropertyChanged**.
  
* **Plotagem de Alta Performance:** Para suportar a taxa de atualização contínua exigida pelo monitoramento industrial, os dados estatísticos coletados são plotados graficamente através da biblioteca **LiveCharts2**. Esta biblioteca opera de forma acelerada via hardware gráfico por meio do motor de renderização vetorial cross-platform, o que preserva a fluidez da linha de renderização e elimina qualquer possibilidade de congelamento visual no terminal do operador.

---

### Camada de Simulação de Sensores (`SimuladorIveco`)

O módulo `SimuladorIveco` consiste em um motor utilitário desenvolvido em modo **Console Application**, responsável por simular com fidelidade o comportamento operacional de dispositivos de telemetria automotiva e sensores IoT implantados fisicamente na planta fabril da Iveco (como antenas de identificação por radiofrequência - RFID, balanças eletrônicas automatizadas e scanners industriais de rastreamento de chassis). 

O utilitário faz uso intensivo da biblioteca de paralelismo **Task Parallel Library (TPL)** do .NET para gerar e despachar pacotes sintéticos contendo números de chassi válidos (VIN), peso líquido das cargas transportadas e a classificação tipológica dos insumos recebidos.

---

## Requisitos Do Sistema E Diretrizes De Instalação

Esta seção descreve os pré-requisitos mínimos e o procedimento técnico para compilar e executar o ecossistema **Iveco Green Ledger** em ambiente de desenvolvimento ou homologação.

### Pré-requisitos de Software e Ambiente
* **SDK do .NET:** Versão `8.0 LTS` ou superior instalada.
* **IDE Recomendada:** Visual Studio 2022 (v17.8+), JetBrains Rider ou Visual Studio Code (com as extensões C# Dev Kit).
* **Banco de Dados Local:** Engine do SQLite instalada (necessária apenas para leitura do arquivo gerado pelo simulador).
* **Infraestrutura em Nuvem:** Uma conta ativa no Google Cloud Platform com um projeto configurado no **Firebase Firestore**.

### Configuração das Chaves de Segurança (Google Cloud IAM)
Por motivos de governança e proteção de dados, as credenciais de acesso ao Firebase NoSQL (`firebase-key.json`) **não** estão incluídas no controle de versão público.
1. Acesse o console do Firebase, vá em *Configurações do Projeto* > *Contas de Serviço*.
2. Gere uma nova chave privada em formato JSON.
3. Renomeie o arquivo baixado para `firebase-key.json`.
4. Cole o arquivo diretamente na raiz do projeto `ApiIveco` (certifique-se de que a propriedade do arquivo esteja configurada como "Copiar se for mais novo" nas propriedades do Visual Studio).
# Regra de Negócio: Validação Restritiva de Chassis (VIN)

---

## Visão Geral sobre o projeto
No contexto do ecossistema **Iveco Green Ledger**, é estritamente proibido o registo de componentes logísticos ou a geração de métricas ambientais (Escopo 3) para veículos que não pertençam à fabricante **IVECO**. Esta regra garante a integridade dos relatórios ESG e evita a contaminação da base de dados corporativa com veículos de terceiros.

## - Atores e Componentes Envolvidos
* **Ator:** Operador de Pátio (via cliente desktop WPF).
* **Sistema Interno:** Back-End em ASP.NET Core 8 (`DadosController` e `DadosService`).
* **Serviço Externo:** API Pública da NHTSA (National Highway Traffic Safety Administration).

## - Fluxo de Validação e Critérios de Aceitação
A validação de um chassi segue um pipeline de verificações síncronas/assíncronas antes da persistência no banco de dados NoSQL (Firebase Firestore):

### - Validação de Fronteira (Cliente/API)
* O utilizador introduz o código VIN.
* O sistema verifica o tamanho da string.
* **Critério:** O código VIN deve conter **exatamente 17 caracteres**.
* **Falha:** Retorna `HTTP 400 (Bad Request)` com a mensagem "O VIN deve ter 17 caracteres."

### - Higienização de Dados (Sanitization)
* Antes de enviar para a entidade externa, o VIN sofre um tratamento.
* **Ação:** Remoção de espaços em branco (`Trim`) e conversão de todos os caracteres para maiúsculas (`ToUpper`).

### - Auditoria Industrial Externa (Integração NHTSA)
* O Back-End consome o endpoint `decodevin` da API VPIC da NHTSA.
* O payload JSON devolvido é analisado em busca da variável correspondente à Marca (`Make`).
* **Critério de Sucesso:** O valor retornado no campo Marca deve conter obrigatoriamente a substring **"IVECO"**.
* **Critério de Rejeição:** Se a marca for nula, vazia ou diferente de IVECO (ex: Volvo, Scania, Ford).

### - Resolução
* **Em caso de Sucesso:** O sistema extrai o modelo do veículo (ou define um genérico caso a API não o forneça) e aprova a transação, avançando para a criação do veículo no Firebase e posterior vínculo das peças/lotes.
* **Em caso de Falha:** O motor de regras de negócio dispara uma exceção crítica. A camada Controller interceta a exceção de forma amigável e retorna o erro para a aplicação WPF, impedindo instantaneamente a continuação da linha de montagem e exibindo a marca não autorizada detetada.

## - Tratamento de Erros e Códigos HTTP

| Cenário de Erro | Código HTTP Retornado | Comportamento do Sistema |
| :--- | :---: | :--- |
| VIN nulo ou em branco | `400 Bad Request` | Rejeição instantânea sem chamada à rede. |
| VIN diferente de 17 caracteres | `400 Bad Request` | Rejeição instantânea sem chamada à rede. |
| Marca detetada não é IVECO | `400 Bad Request` | Controller captura a `Exception` do Service e notifica o Operador com a marca incorreta detetada. |
| Falha na ligação com a NHTSA | `500 / 503` | O sistema notifica indisponibilidade temporária. (Nota: Aqui entra a contingência *Offline-Safe* no lado do Cliente WPF). |

## - Implementação Técnica de Referência
A salvaguarda desta regra está codificada no serviço `DadosService.cs`, garantindo que a regra nunca seja contornada, independentemente do cliente que chame a API:

```csharp
// Extração da Marca
var marca = data.Results.FirstOrDefault(r => r.Variable == "Make")?.Value;

// VALIDAÇÃO CRÍTICA (Regra de Negócio)
if (string.IsNullOrEmpty(marca) || !marca.ToUpper().Contains("IVECO"))
{
    throw new Exception($"VIN inválido para este sistema. A marca detetada foi: {marca ?? "Desconhecida"}. Apenas veículos IVECO são permitidos.");
}

```
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

  ```

---

### 1. Atualização do `ExceptionMiddleware`
Para garantir uma comunicação eficiente e transparente entre a API e a Interface Desktop (WPF), o comportamento de tratamento de erros foi atualizado.
*   **Comportamento Anterior:** Qualquer erro interno, de validação ou de lógica de negócios, resultava numa devolução de mensagens genéricas.
*   **Comportamento Atualizado:** Para os códigos de erro associados à semântica das requisições — `400 Bad Request` (validação de dados) e `422 Unprocessable Entity` (violações à lógica de domínio) —, o middleware agora propaga a **mensagem exata** gerada pela exceção, permitindo que a UI forneça um contexto claro aos operadores (Ex: alertas visuais). Os erros internos (`500`) continuam a devolver apenas mensagens genéricas para garantir a segurança da infraestrutura.

### 2. Proteção Contra Orfandade de Dados (Fornecedores)
Foi implementada uma validação ao processo de eliminação de fornecedores.
*   **Regra Ativa:** É **proibida** a exclusão de um fornecedor caso já exista algum `LoteMateriaPrima` atrelado ao seu ID na base de dados.
*   **Motivo:** Evitar a criação de "Lotes Órfãos" que distorceriam as cadeias logísticas do passado e comprometeriam cálculos retroativos.
*   **Exceção Lançada:** `InvalidOperationException` (Retorna HTTP 422).

### 3. Validações Físicas e Temporais na Criação de Lotes
O método de criação de `LoteMateriaPrima` foi blindado contra valores nocivos à integridade ambiental.
*   **Regras Ativas:**
    *   Não é permitido o registo de lotes com `QuantidadeKg` menor ou igual a zero.
    *   Não é permitido o registo de lotes com valor negativo na métrica `PegadaCarbonoPorKg`.
    *   A `DataProducao` inserida não pode referir-se a um período futuro em relação ao relógio atual da máquina (`DateTime.UtcNow`).
*   **Exceção Lançada:** `ArgumentException` (Retorna HTTP 400).

### 4. Controle Rígido por Balanço de Massa Logística (Cubagem)
Implementou-se a "Trava de Cubagem" durante o vínculo de novas peças (`VeiculoComponente`) a lotes.
*   **Regra Ativa:** Ao tentar registar um componente derivado de um lote específico, o sistema avalia o volume remanescente daquele lote. A soma total dos pesos (Kg) de todas as peças extraídas do lote de origem, incluindo a peça atual, **jamais pode ser maior** que a `QuantidadeKg` total cadastrada no próprio lote.
*   **Motivo:** Bloquear a proliferação artificial de massa produtiva ("criação de matéria"), forçando um espelhamento da realidade do pátio para dentro do sistema.
*   **Exceção Lançada:** `InvalidOperationException` (Retorna HTTP 422).

### 5. Selo de Imutabilidade Pós-Montagem (Anti-Fraude)
Foi adicionada uma trava de edição temporal na manipulação do cadastro de Veículos.
*   **Regra Ativa:** Se um `Veiculo` possui a sua `DataMontagem` já preenchida (veículo finalizado), qualquer tentativa de executar a atualização (método `AtualizarVeiculo` / endpoint `PUT`) resultará em bloqueio integral.
*   **Motivo:** Garantir a fidedignidade da cadeia de fornecimento. Assim que a linha de montagem termina o chassi (VIN), o seu registo é congelado e blindado de falsificações, preservando o valor e o peso final documentado no relatório ESG.
*   **Exceção Lançada:** `InvalidOperationException` (Retorna HTTP 422).

---

## 🏗️ Impactos Arquiteturais
*   Nenhuma tabela ou documento NoSQL foi alterado na sua estrutura base.
*   Como a validação depende de contagens no banco de dados (Ex: somar pesos na regra 4), o tempo de processamento de novos `VeiculoComponente` aumenta marginalmente devido a `reads` adicionais no Firestore.
*   O Client (WPF) não exige refatoração pesada, visto que as mensagens detalhadas de impedimento passarão a surgir nas propriedades de `Response.Content` já consumidas pelo serviço de alertas local.

---

# Resultados e Conclusão:

## Resultados obtidos
A validação prática do ecossistema Iveco Green Ledger comprovou a eficiência da arquitetura distribuída proposta, consolidando os objetivos técnicos, operacionais e ecológicos estabelecidos no início do projeto. Os principais resultados gerados pela integração entre os projetos ApiIveco e WpfIveco são detalhados a seguir:

- **Resiliência Industrial Homologada (Offline-Safe):** Durante os testes de estresse de infraestrutura, simulou-se a perda total de conectividade com a internet no terminal de pátio logístico. A aplicação WpfIveco isolou o erro de rede e ativou com sucesso o buffer local do banco de dados embutido SQLite. O pátio continuou registrando as entradas de insumos e cubagem sem qualquer travamento de interface ou lentidão. Assim que o link de rede foi restabelecido, a rotina assíncrona transmitiu os payloads em lotes via HTTP POST para a ApiIveco, garantindo zero perda de dados e protegendo a montadora contra paradas onerosas na linha de montagem.

- **Precisão Matemática no Cálculo do Escopo 3:** O motor algorítmico codificado em ASP.NET Core 8 eliminou a necessidade de planilhas manuais e estimativas genéricas de carbono. Ao integrar os dados físicos de massa (Kg) extraídos no pátio logístico aos fatores normativos do GHG Protocol armazenados no Firebase Firestore, o sistema passou a discriminar a pegada de carbono equivalente ($CO_2e$) com precisão cirúrgica por componente, vinculando o impacto ambiental diretamente ao código VIN (chassi) consultado e validado na API da NHTSA.

- **Automatização de Processos e Redução de Erros Humanos:** A implementação das validações de fronteira em tempo real transformou a rotina operacional. A integração com a BrasilAPI permitiu que a inserção do CNPJ de um novo fornecedor preenchesse os metadados fiscais e de localização de forma automatizada, enquanto a API da NHTSA bloqueou com sucesso tentativas de cadastro de chassis falsos ou incompatíveis com a frota Iveco. Essas barreiras reduziram drasticamente o tempo de triagem de materiais e anularam erros humanos de digitação.

- **Transparência Analítica e Compliance ESG:** A camada visual alimentada pela biblioteca LiveCharts2 entregou painéis gerenciais dinâmicos e reativos (via padrão MVVM), permitindo que a diretoria da Iveco monitore instantaneamente o balanço de emissões indiretas da cadeia de suprimentos. Complementarmente, o módulo QuestPDF cumpriu seu papel de compliance ao gerar, de forma rápida e ilimitada, relatórios de auditoria paginados e indexados por chassi, prontos para fiscalizações e certificações climáticas externas.


## Conclusão:

O desenvolvimento do Iveco Green Ledger cumpriu com êxito o propósito de desenhar uma solução tecnológica economicamente viável (com custo total estimado de R$ 3.900,00) e arquiteturalmente robusta para responder aos desafios modernos de governança climática na indústria automobilística pesada.

Ao unir o poder de processamento em nuvem e a flexibilidade NoSQL do Firebase Firestore à segurança local serverless do SQLite, o projeto provou que é perfeitamente possível projetar softwares de alta performance que respeitem a dinâmica e a hostilidade do chão de fábrica, sem abrir mão da centralização analítica exigida pelas diretrizes globais de ESG.

O ecossistema não apenas otimizou o monitoramento logístico de insumos, mas transformou dados brutos de manufatura em ativos estratégicos de conformidade socioambiental. A amarração indissociável da pegada de carbono de Escopo 3 ao código VIN estabelece um novo patamar de transparência industrial para o grupo Iveco, permitindo rastrear o histórico ecológico de cada veículo comercial desde a sua matéria-prima até a conclusão na linha de montagem.

*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 16 de junho de 2026.*
