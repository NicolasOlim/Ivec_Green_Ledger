# 📦🍃 Iveco Green Ledger – Testes Executados Na View Model

 <div class="logo-container" align="center">
 <img src="imagens/logo - teste - viewmodel.jpg" alt="Logo Teste Service" class="logo-img" style="height: 350px; width: auto; vertical-align: middle; margin-left: 15px;">
</div>

## **Informações Gerais**
- **Nome do Sistema:** Iveco Green Ledger.
- **Componentes Testados:** `AnalisesViewModel`, `DashboardViewModel`, `FornecedorViewModel`, `MainViewModel`, `PecasViewModel`, `RastreabilidadeViewModel`, `RelatoriosViewModel`, `ViewModelBase`, `DashboardView`, `FornecedoresView` e `MainWindow`.
- **Arquivo de Teste:** `AnalisesViewModelTestes.cs`, `DashboardViewModelTestes.cs`, `FornecedorViewModelTestes.cs`, `MainViewModelTestes.cs`, `PecasViewModelTestes.cs`, `RastreabilidadeViewModelTestes.cs`, `RelatoriosViewModelTestes.cs`, `ViewModelBaseTestes.cs` e suítes de Views.
- **Autor da Suíte:** [🧑‍💻 Erick Silva](https://github.com/erick190813)
- **Data de Entrega:** 25 de Agosto de 2026.
- **Arquitetura Técnica dos Testes:** 

  * **Framework de Testes:** xUnit (v2.9.3).
  * **SDK & Test Runner:** Microsoft.NET.Test.Sdk (v18.9.0) e xunit.runner.visualstudio (v4.0.0).
  * **Simulação de Requisições HTTP (Mocks):** Classe auxiliar da `MockHttpMessageHandler` (que herda de `HttpMessageHandler` e Moq (v4.20.72) para intercepção de endpoints de APIs externas.
  * **Acesso a Membros Privados de UI:** Utilização do (`System.Reflection.BindingFlags`) para invocação de manipuladores de eventos de controle em Views WPF (`TextChanged`, `TextInput`, `Click`)
  * **Testes Parametrizados:** Utilização de `[Theory]` e `[InlineData]` para testes de limites em VINs / Chassis, formatação de valores econômicos e validação de peso de peças.
  * **Padrão de Execução:** AAA (Arrange, Act, Assert) com asserções `Assert.Equal`, `Assert.False`, `Assert.True`, `Assert.Contains`, `Assert.Null`, `Assert.NotNull` e `Assert.IsType`.

---

## **Objetivo e Responsabilidade dos Componentes**

As camadas de ViewModel e View centralizam as regras da interface visual WPF, comandos executáveis (`ICommand`), validação de formulários e integração assíncrona com APIs HTTP.

**1-) `Analises`, `Dashboard`, `Fornecedor`, `Main`, `Pecas`, `Rastreabilidade`, `Relatorios`)**

 * **O que faz?** Controla o estado da interface, executa rotinas assíncronas de chamadas HTTP (cálculo de pegada de carbono, busca de CNPJ e preços de emissões), valida regras de execução de comandos (`CanExecute`) e dispara notificações de propriedade (`INotifyPropertyChanged`).
 * **Problema que resolve:** Isola a lógica da interface gráfica do WPF, impedindo ações inválidas na interface (como salvar fornecedor sem categoria ESG, cadastrar peças sem peso ou pesquisar VINs fora do padrão) e garantindo fallbacks caso APIs externas fiquem indisponíveis.

**2-) `DashboardView`, `FornecedoresView` e `MainWindow`**

 * **O que faz?** Gerencia comportamentos nativos da janela WPF (minimizar, maximizar, fechar), interceptação de entrada de texto nos campos (`TextCompositionEventArgs`) e formatação dinâmica de máscaras em tempo real.
 * **Problema que resolve:** Bloqueia a inserção de caracteres não numéricos em campos numéricos e garante a aplicação correta de máscaras (ex: formato de CNPJ) sem comprometer o fluxo de interação.

**3-) `MockHttpMessageHandler`**

 * **O que faz?** Herda de `HttpMessageHandler` e expõe a propriedade `SendAsyncFunc` para simular retornos `HttpResponseMessage` (200 OK, 500 InternalServerError, 404 Not Found).
 * **Problema que resolve:** Elimina a dependência de serviços online ativos e instabilidades de conexão durante a execução da suíte de testes unitários.

---

## **Mapeamento da Estrutura de Diretórios**

Os arquivos de testes de ViewModel estão organizados na pasta `ViewModel` da suíte de testes:

<img src="imagens/mapeamento - dos - diretorios.jpeg" alt="Logo Firebase Firestore" class="logo-img" style="height: 100px; width: auto; vertical-align: middle; margin-left: 10px;">
