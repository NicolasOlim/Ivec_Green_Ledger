# 📦🍃 Iveco Green Ledger – Documentação de Testes Unitários

## 1. Informações Gerais
- **Nome do Aluno:** Erick Silva Fernandes de Araújo
- **Turma:** Desenvolvimento de Sistemas
- **Nome da Equipe:** Green Ledger
- **Data de Entrega:** 25/08/2026
- **Componente Escolhido:** ViewModel (Camada de Apresentação / Padrão MVVM)
- **Classes e Módulos Testados:** `AnalisesViewModel`, `DashboardViewModel`, `FornecedorViewModel`, `MainViewModel`, `PecasViewModel`, `RastreabilidadeViewModel`, `RelatorioViewModel` e `ViewModelBase`
- **Métodos Testados:** `AtualizarAsync()`, `AtualizarPegadaMediaAsync()`, `CanExecute()` dos comandos de busca (`ConsultarCnpjCommand`, `PesquisarVinCommand`, `SalvarFornecedorCommand`, `AdicionarPecaManualCommand`), `FazerLoginCommand.Execute()` e `OnPropertyChanged()`

## 2. Justificativa da Escolha
A camada de ViewModel centraliza todas as regras de apresentação, validação de formulários, comandos executáveis (`ICommand`) e a comunicação com serviços e APIs externas no padrão MVVM. A escolha por testar esse componente se deve ao fato de que falhas na validação de entrada, falta de resiliência a quedas de rede ou inconsistências no estado da interface afetam diretamente a experiência do usuário final no ecossistema WPF. Garantir a cobertura unitária dessa camada assegura que a lógica da interface funcione de forma estável e previsível antes mesmo da integração com telas gráficas ou bancos de dados.

## 3. Responsabilidade do Componente
As ViewModels gerenciam o estado da interface, expõem propriedades com notificação de alteração (`INotifyPropertyChanged`) e fornecem comandos (`ICommand`) para a execução de ações assíncronas e consultas HTTP a APIs externas.

- **O que o componente faz:** Controla os dados exibidos nas telas, valida se as ações dos usuários podem ser executadas e processa as respostas de serviços externos.
- **Problema que resolve:** Isola a lógica da interface gráfica do WPF, garantindo que botões e campos só fiquem habilitados com dados válidos e aplicando respostas de fallback caso APIs externas estejam inacessíveis.

## 4. Estratégia de Testes e Comportamentos Mapeados
Os cenários foram definidos a partir das regras de negócio de cada tela, utilizando o manipulador `MockHttpMessageHandler` para interceptar requisições HTTP e simular as respostas das APIs sem necessidade de conexão com a rede.

- **Cenários Válidos:** Formatação de valores, atualização de médias de emissão e preenchimento de dados de CNPJ válidos.
- **Cenários Inválidos:** Inserção de CNPJ com letras, senhas incorretas no login, ausência de categorias ESG ou ausência de vínculos de VIN e fornecedor.
- **Valores Limites e Exceções:** Validação de peso de peças, tamanhos e caracteres de VIN e tratamento de erros de rede.
- **Situações Não Testadas (Fora do Escopo):** Renderização direta de elementos e conexão em tempo real com o banco de dados.

## 5. Testes Desenvolvidos (Padrão AAA)
Os testes foram executados na estrutura Arrange, Act e Assert:

### Teste 01: CT06_CT07_CarregarTotalEmissoes_DeveFormatarEconomiaGeradaCorretamente (`AnalisesViewModel`)
- **O que verifica:** Se a conversão do total de emissões e preço de carbono é formatada em milhares ("K") ou milhões ("M").
- **Arrange:** Instanciação do `MockHttpMessageHandler` configurado com respostas JSON simuladas para emissões e preço de carbono, criando o HttpClient com a URI mockada.
- **Act:** Chamada do método `viewModel.AtualizarAsync()`.
- **Assert:** Verificação do resultado por meio da asserção `Assert.Equal(formatacaoEsperada, viewModel.EconomiaGerada)`.

### Teste 02: CT09_PrecoCarbonoFalha_DeveUsarFallbackCorretamente (`AnalisesViewModelTestes`)
- **O que verifica:** A utilização do valor padrão de fallback ($150,0) quando a chamada HTTP para obter o preço do carbono retorna erro 500.
- **Arrange:** Configuração do mock de rede para responder com `HttpStatusCode.InternalServerError` na rota de preço do carbono.
- **Act:** Invocação do método `viewModel.AtualizarAsync()`.
- **Assert:** Confirmação por `Assert.Equal("R$ 150,0K", viewModel.EconomiaGerada)`.

### Teste 03: CT03_AtualizarPegadaMedia_ComFalhaDeRede_NaoDeveQuebrarAcesso (`DashboardViewModelTestes`)
- **O que verifica:** O comportamento do sistema ao enfrentar a ausência de conexão com a internet durante a consulta da pegada média.
- **Arrange:** Configuração do `SendAsyncFunc` no mock para lançar uma exceção `HttpRequestException("Sem internet")`.
- **Act:** Execução assíncrona de `viewModel.AtualizarPegadaMediaAsync()`.
- **Assert:** Verificação por `Assert.Equal("Indisponível", viewModel.PegadaMediaFormatada)`.

### Teste 04: CT13_ConsultarCnpj_ComCnpjInvalido_NaoDevePermitirBusca (`FornecedoresViewModelTestes`)
- **O que verifica:** O bloqueio da busca de CNPJ caso a string informada contenha caracteres alfabéticos.
- **Arrange:** Atribuição da string `"123AB/0001"` à propriedade `CnpjBusca` do `FornecedorViewModel`.
- **Act:** Avaliação do método `viewModel.ConsultarCnpjCommand.CanExecute(null)`.
- **Assert:** `Assert.False(podeExecutar)` com mensagem indicando que a consulta não deve ser permitida fora do formato numérico.

### Teste 05: CT20_CT21_ValidarVin_EntradasInvalidasELimites_DevemSerRejeitadas (`RastreabilidadeViewModelTestes` / `MainViewModelTestes`)
- **O que verifica:** A validação do campo de busca por VIN, rejeitando códigos de 16 ou 18 caracteres ou contendo as letras 'I', 'O' e 'Q', e aceitando o padrão correto de 17 caracteres.
- **Arrange:** Injeção parametrizada de strings via `[Theory]` e `[InlineData]` na propriedade.
- **Act:** Execução de `PesquisarVinCommand.CanExecute(null)`.
- **Assert:** Comparação do resultado retornado pelo comando com a regra de validação esperada.

### Teste 06: CT17_CT18_AdicionarPeca_ValidacaoDePeso (`PecasViewModelTestes`)
- **O que verifica:** A aceitação de pesos positivos e iguais a zero (0.0 e 65.50) e o bloqueio para pesos negativos (-5.0).
- **Arrange:** Atribuição do peso de teste e preenchimento dos vínculos obrigatórios (VIN, fornecedor e nome da peça).
- **Act:** Execução de `viewModel.AdicionarPecaManualCommand.CanExecute(null)`.
- **Assert:** `Assert.Equal(esperado, podeExecutar)`.

### Teste 07: `OnPropertyChanged_DeveDispararEvento_ComONomeDaPropriedadeCorreta` (`ViewModelBaseTestes`)
- **O que verifica:** Se o método `OnPropertyChanged` dispara o evento `PropertyChanged` notificando o nome exato da propriedade modificada.
- **Arrange:** Instanciação da classe de teste `ViewModelMock` e subscrição ao evento `PropertyChanged`.
- **Act:** Alteração da propriedade `viewModel.MinhaPropriedade = "Teste Green Ledger"`.
- **Assert:** Confirmação por `Assert.Equal(nameof(ViewModelMock.MinhaPropriedade), propriedadeAlterada)`.

## 6. Técnicas e Recursos Aplicados
A tabela a seguir relaciona as bibliotecas, dependências do projeto e técnicas utilizadas na criação dos testes unitários:

| Recurso / Ferramenta | Versão / Tipo | Aplicação nos Testes |
| :--- | :--- | :--- |
| **xUnit** | 2.9.3 / Framework | Framework de testes responsáveis pela estrutura das rotinas e pelas anotações `Fact`, `Theory` e `InlineData`. |
| **Microsoft.NET.Test.Sdk** | 18.9.0 / SDK | Pacote SDK para descoberta, compilação e suporte à execução no ecossistema .NET. |
| **xunit.runner.visualstudio** | 4.0.0 / Adaptador | Adaptador para integração e execução no Gerenciador de Testes do Visual Studio. |
| **Moq** | 4.20.72 / Biblioteca | Biblioteca utilizada para criação de mocks e simulação de objetos. |
| **MockHttpMessageHandler** | Classe Customizada | Herda de `HttpMessageHandler` para interceptar chamadas HTTP e injetar respostas simuladas via delegando `SendAsyncFunc`. |

> Para validar a entrada de dados (como CNPJ, VIN e peso de peças), os testes foram divididos entre dados que o sistema deve aceitar e dados que deve rejeitar. Também foram testados os limites exatos das regras, garantindo que o sistema bloqueie pesos negativos enquanto aceita valores a partir do zero, e valide o tamanho exato do VIN (aceitando apenas 17).

## 7. Relato de Defeitos e Correções
Durante a fase de desenvolvimento e execução dos testes unitários da classe `DashboardViewModelTestes`, identificou-se uma inconformidade no tratamento de exceções de rede.

- **Identificação:** Ocorreu durante a execução do teste `CT03_AtualizarPegadaMedia`.
- **Comportamento Observado:** A simulação de falha de conexão com a internet através da injeção de uma `HttpRequestException` resultava na interrupção do método `AtualizarPegadaMediaAsync()`, pois a exceção não estava sendo tratada na ViewModel.
- **Comportamento Esperado:** A ViewModel deveria capturar a exceção de rede e definir o texto "Indisponível" na propriedade `PegadaMediaFormatada` para exibição segura na interface gráfica.
- **Correção Aplicada:** Foi implementado um bloco `try-catch` capturando especificamente `HttpRequestException` dentro do método `AtualizarPegadaMediaAsync()` na classe `DashboardViewModel`. Com a alteração, a propriedade passou a receber a string amigável em caso de erro de conexão, garantindo a aprovação do teste e a estabilidade do sistema.

## 8. Aprendizado e Contribuição para o Projeto
A criação dos testes unitários para a camada ViewModel permitiu validar a lógica de apresentação e as regras de entrada do projeto Green Ledger sem dependência direta de redes externas ou bancos de dados ativos. O uso da classe `MockHttpMessageHandler` garantiu a simulação de falhas de servidor (HTTP 500) e ausência de internet, assegurando que os mecanismos de fallback e tratamento defensivo funcionem conforme o esperado.

Em termos de aprendizado individual, a atividade proporcionou o domínio prático do padrão MVVM desacoplado, o uso do framework xUnit com testes parametrizados (`[Theory]`) e a estruturação de mocks HTTP no ambiente .NET.

---
*Documentação elaborada como requisito para a avaliação do projeto de Trabalho de Conclusão de Curso (TCC) do Curso Técnico em Desenvolvimento de Sistemas.*  
**SENAI - Nova Lima, MG | 2026**
