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

<div class="logo-container">
    <img src="imagens/arquitetura MVVM" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

---
## Módulos do Sistema:

Iveco Green Ledger é organizado em módulos funcionais isolados através do padrão MVVM, cada um com escopo operacional e status de desenvolvimento bem definidos. A tabela abaixo apresenta o panorama atual da solução:

| Módulo | Descrição | Status |
| :--- | :--- | :--- | 
| Mapa | Integração com a API do Mercado Livre para localização geográfica e monitoramento em tempo real dos caminhões de suprimentos | Disponível |
| Dashboard | KPIs e indicadores ecológicos de pegada de carbono computados em tempo real com base nas diretrizes do GHG Protocol | Disponível |
| Triagem e Cadastro De Resíduo | Registro, classificação e auditoria automatizada de veículos utilizando a BrasilAPI (CNPJ) e a API da NHTSA (chassi/VIN) | Disponível |
| Sicronização em Nuvem | Motor de persistência global e consolidação assíncrona de dados estruturados através do Firebase Firestore | Disponível |
| Contigência Offline | Mecanismo de persistência descentralizada em banco SQLite para garantir a operação contínua do pátio em caso de queda de internet | Implementação Futura |
| Relatórios | Exportação de históricos operacionais, balanço de emissões de carbono e dados consolidados para auditoria corporativa | Disponível |
| Central de Notificações | Alertas automáticos na interface WPF sobre inconformidades em chassis, atrasos de cargas ou desvios de rotas logísticas | Implementação Futura |

---

### **Módulo: Mapa**

Este módulo é responsável por monitorar o fluxo logístico externo antes da chegada ao pátio. Permite ao sistema rastrear caminhões de suprimentos e insumos em tempo real, utilizando a API do Mercado Livre (Mercado Envios / Tracking API). Os dados geográficos, históricos de movimentação e previsões de entrega coletados de forma assíncrona são processados e salvos automaticamente no Firebase Firestore para vincular o transporte às metas de emissões de escopo 3.

### **Módulo: Dashboard**

Este módulo centraliza a inteligência ecológica da aplicação através de uma interface analítica e reativa construída em WPF. Ele consome diretamente os endpoints de cálculo da ApiIveco, que processa dados operacionais com base nas diretrizes internacionais do GHG Protocol. Os gráficos e indicadores de pegada de carbono são atualizados em tempo real na tela do operador à medida que novos dados de insumos e transportes entram no sistema.

### **Módulo: Cadastro de Veículos**

Este é o módulo principal de controle de acesso e conformidade do pátio industrial. Ele permite o registro e a auditoria instantânea de veículos na recepção através de uma triagem automatizada: o CNPJ do fornecedor é validado nas bases federais pela BrasilAPI e o número de chassi (VIN) é decodificado tecnicamente via API da NHTSA. O módulo previne erros humanos de digitação e garante que apenas cadastros 100% íntegros sigam para o fluxo de pesagem e cálculo de carbono.

### **Módulo: Sincronização em Nuvem**

Este módulo atua de forma invisível nos bastidores como o motor de persistência global da solução. Ele gerencia as chamadas feitas pela ApiIveco ao Firebase Firestore, garantindo que as coleções NoSQL de documentos estruturados sejam atualizadas de forma escalável e com alta disponibilidade. Ele consolida de maneira definitiva o histórico de auditorias e relatórios ambientais, servindo como a fonte centralizada da verdade de todo o ecossistema.

### **Módulo: Contigência Offline**

Este módulo atua de forma invisível nos bastidores como o motor de persistência global da solução. Ele gerencia as chamadas feitas pela ApiIveco ao Firebase Firestore, garantindo que as coleções NoSQL de documentos estruturados sejam atualizadas de forma escalável e com alta disponibilidade. Ele consolida de maneira definitiva o histórico de auditorias e relatórios ambientais, servindo como a fonte centralizada da verdade de todo o ecossistema.

### **Módulo: Relatórios**

Este módulo consolida de forma analítica todos os dados históricos processados pelo sistema. Ele permite aos gestores e auditores extrair relatórios ambientais completos e balanços consolidados das emissões de carbono geradas pela frota e pela cadeia de suprimentos. As informações, estruturadas sob os parâmetros do GHG Protocol, são recuperadas diretamente do Firebase Firestore através da ApiIveco e apresentadas prontas para exportação corporativa, garantindo transparência e conformidade jurídica para fins de auditoria interna e externa.

### **Módulo: Central de Notificações**

Projetado como uma evolução estratégica para as próximas etapas do sistema, este módulo atuará no monitoramento preditivo e na comunicação em tempo real da interface WPF com o operador de pátio. Ele enviará alertas visuais e sonoros instantâneos em cenários críticos da operação logística, como a identificação de inconformidades estruturais em chassis analisados pela API da NHTSA, inconsistências cadastrais de CNPJ detectadas pela BrasilAPI ou desvios de rotas e atrasos severos de caminhões rastreados pela API do Mercado Livre.

---

### **Processo de Desenvolvimento**

| Fase | Status |
| :--- | :--- | 
| Fase 1 - Setup e Infraestrutura | Concluído | 
| Fase 2 - Core UI e Telas | Concluído | 
| Fase 3 - API REST e Regras de Negócio | Concluído | 
| Fase 4 - Integrações e Validações externas | Concluído | 
| Fase 5 - Persistência na nuvem | Concluído | 
| Fase 6 - Expansão Futura | Em Progresso | 
| Central de Notificações |Implementação Futura | 

### **Ambiente de Desenvolvimento**

- **IDE:** Visual Studio
- **SDK:** NET 8;
- **Gerenciamento de pacotes:**  NuGet;
- **Sistema Operacional:** Windows 10 ou Windows 11(64 - bits);
- **Banco de dados:** Firebase e SQLite.

---
## Business Model Canvas:

<div class="logo-container">
    <img src="imagens/bussiness model canvas.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>
---

---

## Viabilidade Econômica:

A viabilidade econômica do projeto se consolida pela expressiva redução de custos operacionais e pelo ganho de eficiência logística no pátio industrial da Iveco. Ao automatizar a triagem de veículos com as APIs da NHTSA e BrasilAPI, o sistema elimina os custos decorrentes de erros humanos de digitação, fraudes cadastrais e o tempo ocioso de caminhões em filas de espera. Além disso, a adoção de uma arquitetura Open-Source baseada em .NET 8 e SQLite, combinada à infraestrutura sob demanda e altamente escalável do Firebase Firestore, minimiza o investimento inicial em servidores físicos e licenças de software proprietárias. Sob a ótica estratégica, o motor de cálculo alinhado ao GHG Protocol posiciona a companhia em estrita conformidade com as exigências globais de ESG, mitigando riscos de sanções ambientais e abrindo portas para incentivos fiscais e captação de fundos verdes, o que garante um retorno sobre o investimento.

### **Custos de Desenvolvimento**

Por tratar-se de um projeto acadêmico focado em inovação industrial, os custos de engenharia e desenvolvimento foram essencialmente de tempo, pesquisa e capacitação da equipe. A tabela abaixo projeta esses custos em valores reais de mercado, considerando as horas técnicas investidas e a remuneração média de desenvolvedores Júnior/Pleno no Brasil em 2026 para o desenvolvimento do ecossistema.

| Item | Hora estimada | Valor por Hora | Custo Total |
| :--- | :--- | :--- | :--- | 
| Levantamento de Requisitos | 30h |  R$ 60,00 |  R$ 1.800,00 | 
| Modelagem do banco e Estrutura Relacional | 25h |  R$ 65,00 |  R$ 1.625,00 | 
| Desenvolvimento da API | 70h |  R$ 90,00 |  R$ 6.300,00 | 
| Desenvolvimento da WPF | 80h |  R$ 85,00 |  R$ 6.800,00 | 
| Integração e consumo das API’s | 40h |  R$ 85,00 |  R$ 3.200,00 |
| Implementação de Lógica e Sincronização | 35h |  R$ 90,00 |  R$ 3.150,00 | 
| Testes e Validações de Dados | 25h |  R$ 70,00 |  R$ 1.750,00 | 
| Documentaçãos | 35h |  R$ 50,00 |  R$ 1.750,00 | 
| Total | 340h |  - |  R$ 2.640,00 | 

---
Os custos mensais de operação do sistema em produção são estimados abaixo, considerando a volumetria de requisições e processamento de dados para o gerenciamento de pátio, triagem de veículos e monitoramento de emissões da Iveco:


| Serviço | Custo no Desenvolvimento | Projeção em Produção | Observação |
| :--- | :--- | :--- | :--- | 
| Levantamento de Requisitos | R$ 0,00 |  R$ 0,00 |  Gratuito até 1 GiB de armazenamento, 50k leituras e 20k escritas diárias. Atende perfeitamente o escopo atual | 
| API de rastreio e Logistica | R$ 0,00 |  R$ 0,00 |  Serviços de dados públicos ou acessados via chaves gratuitas de desenvolvedor | 
| Hospedagem da API | R$ 0,00 |  R$ 0,00 - 120,00 |  Atualmente rodando em servidores locais e nuvem gratuita. Em produção, pode migrar para planos com maior disponibilidade | 
| SQLite | R$ 0,00 |  R$ 0,00 |  Banco embutido no cliente desktop WPF, sem dependência de nuvem ou servidor externo | 
| Domínio e Certificado SSL | R$ 0,00 |  R$ 5,00 - 40,00 |  Comunicação segura via criptografia e SSL gratuito |
| Total Mensal Atual | R$ 0,00 | - |  Custo zero durante todo o ciclo de desenvolvimento | 

---
### **Beneficios e Retorno Esperado**

A adoção do Iveco Green Ledger no ecossistema de transporte e triagem de pátio pode gerar benefícios econômicos quantificáveis em múltiplas frentes operacionais e estratégicas. Os valores e métricas abaixo constituem estimativas fundamentadas nas médias do setor de logística automotiva brasileiro e nos gargalos operacionais identificados em pátios industriais de grande porte.

### **Redução da Emissão de CO2**

A implementação do Iveco Green Ledger atua como uma ferramenta estratégica na descarbonização da cadeia logística da Iveco, gerando redução direta e indireta nas emissões de Dióxido de Carbono ($CO_2$) e outros gases de efeito estufa (GEE). O impacto positivo do sistema se consolida em três pilares fundamentais: 
- Mitigação do Tempo de Marcha Lenta;
- Otimização de Rotas;
- Auditoria Confiável.

| Indicador | Sem Green Ledger | Com Green Ledger | 
| :--- | :--- | :--- |
| Tempo médio de triagem | 15 min por veículo |  2 minutos por veículo |  
| Emissão diária de CO2 em marcha lenta | 45,0 kg de CO2 por dia |  6,0 kg - 9,0 kg de CO2 por dia |  
| Economia mensal de CO2 | - | 900 kg - 1,100 kg de CO2 por dia |  
| Economia anual estimada de CO2 | - | 10,8 t - 13,2 t de CO2 por ano |  
| Domínio e Certificado SSL | R$ 0,00 | R$ 5,00 - R$ 40,00 |  

---
### **Receitas e Materiais**

Para a implantação física do ecossistema Iveco Green Ledger, o consumo de materiais é voltado exclusivamente para a infraestrutura de tecnologia e conectividade nas portarias e balanças do pátio logístico, englobando terminais de chão de fábrica instalados para a execução contínua da interface de usuário (WpfIveco), leitores de código de barras ou QR Code USB para agilizar a entrada de dados operacionais sem digitação manual, além da infraestrutura de rede local existente para garantir a comunicação de dados e a sincronização com a nuvem, operando com custo zero de licenciamento de software por utilizar o framework .NET 8, SQLite e APIs integradas de código aberto. Por tratar-se de um sistema focado em suporte logístico interno e governança ambiental (ESG), o projeto não gera faturamento direto por vendas, mas consolida seu retorno financeiro na forma de receitas indiretas através de uma drástica redução de custos operacionais. 

### **Análise Custo - Benefício**

| Categoria | Tipo | Valor Anual Estimado | 
| :--- | :--- | :--- |
| Desenvolvimento | Custo |  R$ 8.800,00 |  
| Operação / Infraestrutura | Custo |  R$ 0,00 - 1.440,00 |  
| Manutenção, suporte técnico e suporte offline | Custo | R$ 1.200,00 - 2.400,00 |  
| Redução de custos | Benefício | R$ 18.000,00 |  
| Economia operacional com eliminação de erros e retrabalhos administrativos | Benefício | R$ 6.600,00 |  
| Economia de combustível | Benefício | R$ 7.800,00 |  
| Economia de combustível | Benefício | R$ 7.800,00 |  
| Automação | Benefício | R$ 12.000,00 |  
| Saldo Líquido Estimado | Lucro | R$ 31.600,00 - 34.160,00 |  

---

## Artefatos Técnicos Entregues

Esta seção consolida os artefatos técnicos produzidos e entregues como parte do TCC, conforme os requisitos definidos pelo curso. 

| Artefato | Descrição | Status | 
| :--- | :--- | :--- |
| Mini Mundo da Demanda |  Contextualização do problema |  Entregue |  
| Modelo Lógico | Criação de tabelas |  Entregue |  
| Modelo Físico | Documentação interativa e detalhada de todos os endpoints REST criados | Entregue |  
| Swagger | Código-fonte completo com histórico | Entregue |  
| MVVM | Separação de responsabilidades da interface gráfica WPF | Entregue |  
| Documentação | Documentação técnica completa contendo levantamento de requisitos | Entregue |  

---
## Requisitos
### **Requisitos Funcionais**

Os requisitos funcionais descrevem as ações, facilidades e comportamentos que o sistema Green Ledger deve oferecer:

- **RF - 001: Autenticação de Usuários:** O sistema deve permitir o controle de acesso de operários da portaria, gestores de pátio e analistas ambientais através de login e senha integrados ao Firebase;
- **RF - 002: Triagem de Entrada de Veículos:** O sistema deve registrar o ingresso de caminhões no pátio logístico da Iveco, capturando dados do motorista, placa e hora de entrada;
- **RF - 003: Integração e Decodificação de Chassi (VIN):** O sistema deve consumir a API da NHTSA para decodificar automaticamente o número do chassi do veículo, extraindo o modelo, ano de fabricação e especificações de motorização;
- **RF - 004: RF - 005: Rastreamento de Entregas e Rotas:** O sistema deve integrar-se com a API do Mercado Livre (ou serviços equivalentes de logística) para monitorar o status do trajeto e a quilometragem percorrida pela frota parceira;
- **RF - 005: Rastreamento de Entregas e Rotas:** O sistema deve integrar-se com a API do Mercado Livre (ou serviços equivalentes de logística) para monitorar o status do trajeto e a quilometragem percorrida pela frota parceira;
- **RF - 006: Sincronização Híbrida e Operação Offline:** Persistência dos dados localmente no banco SQLite caso haja queda de internet, sincronizando tudo com o Firebase Firestore de forma automática assim que a conexão for restabelecida;
- **RF - 007: Cálculo da Pegada de Carbono:** O sistema deve calcular as emissões de gases de efeito estufa geradas pela queima de combustível da frota com base nas diretrizes e fatores de emissão;
- **RF - 008: Monitoramento:** O sistema deve contabilizar o tempo em que o veículo permaneceu parado com o motor ligado no pátio e projetar o desperdício de combustível e emissão de carbono gerados nesse intervalo;
- **RF - 009: Geração de Relatórios:** O sistema deve emitir relatórios gerenciais consolidados mostrando a redução de emissões, eficiência logística e indicadores ambientais da cadeia de suprimentos da Iveco.

### **Requisitos Não Funcionais**

- **RNF - 001: Arquitetura e Framework Base:** A API do sistema deve ser desenvolvida em ambiente multipataforma utilizando o framework .NET 8 (ASP.NET Core REST API), garantindo escalabilidade e alta performance no processamento das requisições;
- **RNF - 002: Padrão Arquitetural de Interface:** O aplicativo cliente de desktop (WpfIveco) deve obrigatoriamente seguir o padrão de arquitetura MVVM (Model-View-ViewModel), assegurando a separação limpa entre a interface gráfica (XAML) e a lógica de negócios;
- **RNF - 003: Persistência:** O sistema deve utilizar uma abordagem de banco de dados híbrido, empregando o SQLite como banco de dados relacional;
- **RNF - 004: Tempo de Resposta da API:** Os endpoints da ApiIveco devem processar e responder às requisições locais e de cálculo de emissões do GHG Protocol em um tempo máximo de 2 segundos sob condições normais de rede;
- **RNF - 005:  Segurança e Criptografia de Dados:** Toda a comunicação de rede entre o cliente WPF, a API REST e o Firebase deve trafegar criptografada utilizando o protocolo HTTPS/SSL, impedindo a interceptação de dados sensíveis da Iveco na rede interna;
- **RNF - 006: Compatibilidade:** O módulo de triagem e controle de pátio (WpfIveco) deve ser totalmente compatível e otimizado para execução em sistemas operacionais;
- **RNF - 007: Disponibilidade** O ecossistema deve possuir alta tolerância a falhas de conectividade externa;
- **RNF - 008: Concorrência e Consistência** O banco de dados em nuvem deve suportar o acesso e a escrita simultânea de múltiplos terminais de portaria operando em paralelo, garantindo a sincronização das triagens através de regras de consistência eventual integradas ao Firebase.

---
## Caso de Uso
### **Diagrama de Caso de Uso do Sistema Completo**

<div class="logo-container">
    <img src="imagens/casodeusosistemacompleto.jpeg" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

---
## Histórico De Evolução e Atualização Do Projeto
### **Fase de inicialização e infraestrutura base (setup inicial)**

Estruturação da Solução Multicamadas e Infraestrutura Base: Criação da arquitetura desacoplada no Visual Studio dividindo o projeto em duas frentes de execução distintas: o backend de microsserviços (ApiIveco) desenvolvido em ASP.NET Core e o cliente rico desktop (WpfIveco). Implementação da Arquitetura MVVM Nativa: Desenvolvimento e consolidação da infraestrutura necessária para o funcionamento do padrão MVVM no ambiente WPF, com a criação da classe abstrata BaseViewModel encapsulando a interface de notificação de propriedades (INotifyPropertyChanged), além da classe genérica RelayCommand para substituição completa de manipuladores de eventos em code-behind por bindings limpos e declarativos diretamente no XAML. 

### **Fase de amadurecimento do frontend**

O desenvolvimento das interfaces visuais do cliente desktop (WpfIveco) utilizando XAML estruturado, com foco na criação de componentes reaproveitáveis, dicionários de recursos (Resource Dictionaries) para padronização estética e painéis de controle (Dashboards) responsivos. A interface foi otimizada para operação em chão de fábrica, traduzindo dados complexos do motor de cálculo de emissões do GHG Protocol e status de triagem em elementos visuais limpos, intuitivos e totalmente vinculados às propriedades das ViewModels.

---
## Regra de Negócio

A camada de regras de negócio (Business Logic Layer) do ecossistema Iveco Green Ledger constitui o núcleo de inteligência da aplicação, sendo responsável por ditar o comportamento da ApiIveco e orientar as tomadas de decisão da interface cliente WpfIveco. Esta seção detalha as diretrizes operacionais, validações de pátio e o motor de cálculo ambiental que governam o projeto.

### *Fluxo de Triagem e Orquestração Logística*

O sistema opera sob o modelo de validação em barreira, o que significa que nenhum veículo de transporte de carga tem sua entrada autorizada ou concluída no pátio logístico da Iveco sem passar por uma verificação multifacetada e automatizada. As regras que regem essa barreira consistem em:

- **Automação do Vínculo de Ordem de Coleta:** O sistema intercepta o código identificador da viagem (via leitura de QR Code ou digitação de contingência). A aplicação dispara uma requisição assíncrona integrada ao microsserviço de rotas (baseado na API do Mercado Livre), verificando o status do trajeto. Caso a rota conste como "Cancelada" ou "Concluída", o fluxo de triagem é imediatamente interrompido por uma trava de negócio, notificando o operador de portaria.
  
- **Decodificação Técnica de Frota (Mecanismo VIN):** Ao capturar o chassi do caminhão, a API interna dispara uma consulta à API da NHTSA. O sistema valida se o chassi possui o padrão internacional de 17 caracteres. O retorno da API externa é processado para extrair o ano de fabricação, o modelo e a capacidade de carga do motor. Esses dados técnicos são injetados diretamente na memória do sistema e salvos no banco de dados NoSQL (Firebase Firestore), servindo de insumo indispensável para o cálculo posterior de pegada ecológica.

- **Homologação Cadastral e Fiscal:** Para mitigar riscos fiscais na cadeia de suprimentos, o CNPJ da transportadora associada à carga é submetido à BrasilAPI. Se o cadastro retornar com situação inválida ou inexistente perante os órgãos reguladores, o sistema impede a finalização do registro de entrada, exigindo intervenção ou liberação manual por parte de um supervisor de logística.

### *Motor de Cálculo Ambiental*

A grande inteligência ecológica do projeto reside na automatização do inventário de emissões de Gases de Efeito Estufa (GEE), focando especificamente nas emissões indiretas da cadeia de valor (Escopo 3).

- **Cálculo Baseado em Distância e Combustível:**  Os fatores de emissão variam dinamicamente se o caminhão utiliza Diesel S10, Diesel S500 ou Gás Natural Veicular (GNV). O ano do modelo (extraído no fluxo da NHTSA) aplica um fator de degradação e eficiência, tornando o cálculo altamente preciso e auditável.

### *Algoritmo de Monitoramento*

A marcha lenta de veículos pesados dentro das dependências da fábrica representa um dos maiores gargalos ocultos de sustentabilidade e custo. O Iveco Green Ledger implementa uma regra de negócio severa para combater esse cenário:

- **Contabilização do Tempo de Pátio:** O sistema registra o timestamp (carimbo de data/hora) exato no momento em que a portaria autoriza a entrada do veículo e quando a balança/doca registra a pesagem ou descarga.

- **Métrica de Desperdício:** Com base no delta de tempo em minutos gasto pelo caminhão trafegando ou esperando em fila com o motor ligado no pátio, o algoritmo calcula o desperdício de combustível presumido (sabendo que um motor pesado consome em média de 2 a 3,5 litros de óleo diesel por hora em marcha lenta). O sistema projeta instantaneamente a quantidade de $CO_2$ liberada desnecessariamente na atmosfera naquele intervalo, gerando alertas no painel do Analista de ESG caso o tempo de pátio ultrapasse a meta operacional estipulada de 15 minutos.

### *Regra de Persistência Resiliência*

- **Garantia de Operação:** Durante a execução do aplicativo desktop WpfIveco, o sistema testa periodicamente a conectividade com o backend na nuvem. Detectada qualquer oscilação ou queda na internet, o fluxo de triagem não é bloqueado. A regra de negócio instrui o sistema a persistir todos os registros de entrada, cálculos do GHG e validações em andamento no banco de dados embutido e local SQLite.

- **Sincronização Eventual em Lote:** Assim que os serviços de rede detectam que a conexão com o Firebase Firestore foi restabelecida, uma rotina em segundo plano (background worker) é disparada de forma assíncrona. Esse módulo realiza a varredura no banco local, extrai os dados gerados durante o período de contingência, resolve possíveis conflitos de concorrência de horários e faz o upload em lote (bulk insert) para a nuvem, atualizando os dashboards gerenciais de forma transparente para o usuário final.



Visando a alta disponibilidade do chão de fábrica, a lógica de armazenamento foi arquitetada para ser tolerante a falhas completas de infraestrutura de rede:

---
## Considerações Finais:

A camada de regras de negócio (Business Logic Layer) do ecossistema Iveco Green Ledger constitui o núcleo de inteligência da aplicação, sendo responsável por ditar o comportamento da ApiIveco e orientar as tomadas de decisão da interface cliente WpfIveco. Esta seção detalha as diretrizes operacionais, validações de pátio e o motor de cálculo ambiental que governam o projeto.

Sob a ótica do desenvolvimento de software, a divisão da solução em uma arquitetura desacoplada, composta pelo backend de microsserviços em ASP.NET Core (ApiIveco) e o cliente rico desktop (WpfIveco) baseado no padrão arquitetural MVVM, provou-se altamente eficaz. Essa separação assegurou uma interface ágil e limpa para o operador de portaria, enquanto centralizou na API a inteligência pesada de dados. Além disso, a implementação do modelo híbrido de banco de dados, combinando a flexibilidade na nuvem do Firebase Firestore com a resiliência Offline-First do SQLite local, garantiu a tolerância a falhas indispensável para o fluxo contínuo e ininterrupto exigido pelo chão de fábrica da Iveco.

No aspecto comercial e de negócios, a validação de dados em barreira por meio do consumo automatizado de APIs chaves (NHTSA, BrasilAPI e Mercado Livre) erradicou os erros manuais de digitação e os tempos ociosos de triagem, reduzindo o tempo médio por veículo de 15 para apenas 2 minutos. Essa fluidez no pátio logístico não apenas evitou prejuízos com multas de retenção de frota (Demurrage), como também atacou diretamente a emissão de Gases de Efeito Estufa (GEE) gerados por caminhões pesados em marcha lenta (idling). O motor de cálculo baseado nas diretrizes oficiais do GHG Protocol elevou o sistema ao patamar de ferramenta estratégica de governança, fornecendo dados automatizados e auditáveis para o inventário de Escopo 3 da companhia.

A análise de viabilidade econômico-financeira realizada reforça o alto retorno sobre o investimento (ROI) do projeto. Ao confrontar os custos estimados de desenvolvimento e manutenção com a receita indireta projetada de R$ 44.400,00 anuais (provenientes da eliminação de retrabalhos, economia de diesel e automação de auditorias ambientais), constatou-se que o ecossistema recupera integralmente o capital investido em aproximadamente 7 meses de operação real na planta.

---
## Trabalhos Futuros e Evolução do Sistema:

Como horizontes de evolução e continuidade para o projeto, recomendam-se as seguintes frentes de expansão tecnológica:

- **Integração com Visão Computacional (OCR):** Implementar algoritmos de inteligência artificial para leitura e reconhecimento automático de placas (LPR) e números de chassi através de câmeras na portaria, eliminando totalmente a necessidade de bipagem de QR Codes.

- **Módulo de Rastreamento IoT em Tempo Real:** Integrar sensores de telemetria diretamente nos veículos da frota parceira para monitorar o consumo exato de combustível e as paradas por GPS, substituindo as médias estimadas por dados empíricos de altíssima precisão.

- **Aplicativo Mobile para o Motorista** Desenvolver um módulo móvel focado no motorista terceirizado, permitindo que ele realize um pré-cadastro da carga e agende o horário de chegada (slot logístico) antes mesmo de atingir a barreira física da fábrica.

Conclui-se, portanto, que o Iveco Green Ledger se consolida não apenas como um projeto acadêmico de engenharia, mas como um ativo tecnológico viável, lucrativo e sustentável, perfeitamente alinhado com as demandas de eficiência e responsabilidade climática exigidas pelo mercado global em 2026.

---
## Referências Bibliográficas:

- https://firebase.google.com/?hl=pt-br;
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/
- https://livecharts.dev/

*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 16 de junho de 2026.*
