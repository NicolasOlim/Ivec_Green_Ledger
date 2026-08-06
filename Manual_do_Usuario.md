# 📦🍃 Iveco Green Ledger – Manual do Usuário

<div class="logo-container" align="center">
    <img src="imagens/icone_manual.jpg" alt="Logo Iveco Green Ledger" class="logo-img">
</div>

Essa documentação se aplica ao manual operacional de instruções para o uso da aplicação WPF em estações de trabalho e portarias da IVECO, cobrindo navegação, cadastros, integrações de API's e visualização de dados de governança ambiental em nuvem.



## *1. Apresentação e Objetivo do Sistema*
O *Iveco Green Ledger* é uma solução desenvolvida para gerenciar a triagem logística de pátio, a rastreabilidade de suprimentos e o cálculo automatizado da pegada de carbono, com foco no escopo 3 do GHG Protocol. O objetivo do sistema é unir a operação física da fábrica aos compromissos de sustentabilidade da IVECO, fornecendo um ambiente centralizado onde:

   * Fornecedores são qualificados e avaliados por suas práticas ambientais.
   * A entrada de componentes e matérias-primas é monitorada.
   * A montagem e composição de veículos são rastreadas peça a peça.
   * As emissões de gases de efeito estufa são calculadas e consolidadas em tempo real.



## *2. Requisitos de Acesso e Conectividade*

- *Ambiente:* Computador com sistema operacional Windows 10(64-bit) ou superior e .NET 8 instalado.
- *Conectividade:* A conexão ativa e obrigatória com a internet.
- *Credenciais de Acesso:* E-mail previamente autorizado pela administração do sistema.



## *3. Autenticação e Primeiro Acesso*

O acesso ao sistema é restrito para garantir a segurança dos dados industriais e de logística.

- *1º Etapa - Realizando o login:*

    <img src="imagens/login.png" alt="Tela de Login do Sistema" class="logo-img" style="height: 250px; width: auto; border: 100px solid #ddd; margin: 850px 0;">

* Abra a aplicação em sua estação de trabalho;
* Na tela inicial, insira o *e-mail* e *senha*;
* Clique em *Entrar*, o sistema consultará a Web API para autenticar o perfil e liberar as permissões correspondentes.



- *2º Etapa - Módulo de Fornecedores:*

Módulo voltado para o cadastramento, consulta cadastral automatizada e atribuição de classificação ambiental de parceiros comerciais na rede permissionada do projeto.

*Cadastro e Consulta via Receita Federal*

<img src="imagens/fornecedor.png" alt="Tela de Login do Sistema" class="logo-img" style="height: 250px; width: auto; border: 100px solid #ddd; margin: 850px 0;">

* No menu lateral de navegação, clique na opção *Fornecedores*;
* Na tela *Gestão de Fornecedores, insira o **CNPJ* da empresa no campo de busca(formato 00.000.000/0000-00);
* Clique no botão azul *Consultar CNPJ* para disparar a busca automatizada dos dados;
* O sistema preencherá automaticamente as informações da empresa?
  - *Razão Social*;
  - *Endereço sede*;
  - *Status RFB* que exibe a situação cadastral retomada da consulta.

- *3º Etapa - Módulo de Componentes e Rastreabilidade (Peças e Componentes):*

Utilizado no pátio logístico e na linha de montagem para associar peças recebidas aos veículos cadastrados no sistema:

 <img src="imagens/gestao_de_componentes.png" alt="Tela de Login do Sistema" class="logo-img" style="height: 250px; width: auto; border: 100px solid #ddd; margin: 850px 0;">

 - *3º Etapa - *Associação e Registros de Peças ao Veículo(VIN):*


* No menu lateral, acesse a opção *Peças e Componentes*;
* Na tela *Gestão de Componentes*, preencha o formulário de cadastro;
  - *Chassi do Veículo(VIN):* Selecione o código de 17 caracteres do veículo atrelado (ex: ZCFA1E0200812345.
  - *Fornecedor:* Selecione o fornecedor responsável pela peça (ex: ROBERT BOSCH LIM).
  - *Nome / Descrição da Peça:* Digite o nome do componentes (ex: Volante, Virabrequim, Cabeçote, Bloco do motor).
  - *Peso da Peça(kg):* Insira o peso do componente em quilogramas (ex: 65.00).
* Clique no botão verde *+ Registrar Peça*.

*Histórico e Confirmação de Registros*

* No painel *Últimas Peças Registradas* acompanha o histórico de itens associados e o contador total de peças;
* A tag verde *Gravado no Ledger* indica que o componente foi slavo e vinculado ao histórico auditável do veículo.

 <img src="imagens/RASTREABILIDADEPRINT.png" alt="Tela de Login do Sistema" class="logo-img" style="height: 250px; width: auto; border: 100px solid #ddd; margin: 850px 0;">

*4º Etapa - Visao Geral e Analises ESG*

* No menu lateral, acesse *Analises ESG*;

* *Análises ESG:* Acesse a opção do menu lateral para visualizar métricas de impacto e consolidados ambientais;
  
 <img src="imagens/ESG.png" alt="Tela de Login do Sistema" class="logo-img" style="height: 250px; width: auto; border: 100px solid #ddd; margin: 850px 0;">
  
## *9. Solução de Problemas e Perguntas Frequentes (FAQ)*

 <img src="imagens/logo-faq.webp" alt="Tela de Login do Sistema" class="logo-img" style="height: 250px; width: auto; border: 100px solid #ddd; margin: 850px 0;">

*-) O CNPJ não é preenchido automaticamente ao clicar em "Consultar CNPJ"*

 * *Causa Provável:* Instabilidade no serviço da BrasilAPI ou CNPJ digitado em formato incorreto.
 * *Procedimento:* 

  - Certique-se de digitar os 14 números do CNPJ no formato (00.000.000./0000.00);
  - Acesse a tela *Dashboard* para verificar se a integração com a BrasilAPI está online;
  - Caso esteja indisponível, preencha manualmente a *Razão Social* e o *Endereço sede*.



*-) A validação do Chassi(VIN) falhou ou retornou erro na consulta*

 * *Causa Provável:* O código digitado possui tamanho incorreto, contém caracteres proibidos ou a API da NHTSA está inacessível.
 * *Procedimento:*

  - Verifique que o código possui exatamente 17 caracteres alfanuméricos;
  - Lembre de que as letras *I, **O* e *Q* não são utilizadas em chassis para evitar confusão com números;
  - Confira no Dashboard se o indicador da NHTSA está verde;



*-) O endereço do Fornecedor não é sugerido no campo "Endereço Sede"*

 * *Causa Provável:* Oscilação na API do Google ou limite de requições atingido.
 * *Procedimento:*

  - Digite o endereço completo contendo rua, número, cidade e estado;
  - Se o status da API do Google estiver inativo no *Dashboard, faça o preenchimento manual do campo antes de clicar no botão **Registrar no Ledger*.



*-) Não recebi o e-mail de acesso ou falha na autenticação da plataforma*

 * *Causa Provável:* Digitação incorreta das credenciais na tela *Acessar Plataforma* ou bloqueio pelo firewall do email da Iveco Green Ledger.
 * *Procedimento:*

  - Verifica os campos *Endereço de E-mail* e *Palavra Passe* foram preenchidos corretamente;
  - Verifica as pastas de "lixo eletrônico" ou "Spam";
  - Se os problemas persistir, solicite liberação junto á equipe.



*-) Um ou mais indicadores de API aparecem com luz vermelha no Dashboard"*

 * *Causa Provável:* Oscilação na conexão com a internet da estação de trabalho ou indisponibilidade temporária nos servidores externos(Brasil API, Places, NHTSA, Mercado Livre).
 * *Procedimento:*

  - Verifica os cartões *Falha de Integração* e *Tempos de Resposta(API)* no Dashboard;
  - Certifique-se de que o computador está conectado á rede;
  - O sistema permite o preenchimento e salvamento manual até o restabelecimento das conexões.



*-) Não consigo registrar um componente na tela "Peças e Componentes"*

 * *Causa Provável:* O fornecedor do item não foi previamente cadastrado na rede ou os campos obrigatórios não foram selecionados.
 * *Procedimento:*

  - Acesse a tela *Fornecedores* no menu lateral e confirme se a empresa foi devidamente gravada no Ledger;
  - Volte em *Peças e Componentes, selecione o **Chassi do Veículo(VIN)* e o *Fornecedor* nos menus suspensos antes de clicar em *+ Registar Peça*.



*-) A peça cadastrada não é listada em "Últimas Peças Registradas"*

 * *Causa Provável:* Falha na comunicação com o banco de dados do Ledger ou necessidade de atualização na interface.
 * *Procedimento:*

  - Certifique-se de preencher a *Descrição da Peça* e o *Peso da peça(kg)*;
  - Após registar, verifique-se o card exige a tag verde *Gravado no Ledger*.



*-) Preciso relatar uma falha ou solicitar suporte para o sistema"*

 * *Causa Provável:* Ocorrência de problemas técnicos, inconsistências nos dados ou necessidade de manutenção na estação de trabalho.
 * *Procedimento:*

  - Acesse o menu *Dashboard* e vá até o painel *Registar Solicitação de Suporte*;
  - Selecione o *Tipo de Pr*

Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.  
Última atualização: 05 de agosto de 2026.
*# 📦🍃 Iveco Green Ledger – Manual do Usuário

*
