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



*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 16 de junho de 2026.*
