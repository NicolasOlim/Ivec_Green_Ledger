# 📦⚙️ Iveco Green Ledger – Guia de Instalação

Este documento é voltado para descrever todos os procedimentos necessários para realizar a implantação, configuração e manutenção do projeto e portarias da Iveco.

---

## **1. Informações Gerais**
- **Nome do Sistema:** Iveco Green Ledger
- **Versão Atual:** 1.0 (Release Operacional Base)
- **Arquitetura:** Aplicação Desktop (WPF) consumindo Web API (ASP.NET Core) e Firebase.

---

## **2. Requisitos do Sistema**
Antes de iniciar a instalação, certificamos que a máquina de destino atende os seguintes requisitos:

**Requisitos de Hardware:**
- **Processador:** Dual-Core 2.0 GHz ou superior.
- **Memória RAM:** Mínimo de 8 GB.
- **Espaço em Disco:** 500 MB de espaço livre para a aplicação e o banco local que no nosso caso foi o: **SQLite**.

**Requisitos de Software e Dependências:**
- **Sistema Operacional:** Windows 11 ou superior.
- **Dependência Essencial:** [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). (Deve ser instalado previamente na máquina).
- **Conectividade:** Acesso liberado à internet para comunicação com a Web API, Firebase e APIs externas (Brasil API, Google Places, NHTSA).

---

## **3. Processo de Instalação**

Seguimos as seguintes etapas para instalar a aplicação WPF nos computadores:

1. **Download do Pacote:** Tinhamos uma pasta de entrega como por exemplo: (ex: `Versao_1.0.zip`) e extraído em um diretório local.
2. **Execução do Setup:** Localize e execute o arquivo `Setup.exe` (ou o MSI correspondente) do nosso projeto.
3. **Assistente de Instalação:**
   - Aceite os termos de licença (`Licenca.txt`).
   - Escolha o diretório de instalação (Exemplo: `C:\Program Files\Iveco\GreenLedger`).
   - Permita a criação de atalhos na Área de Trabalho e no Menu Iniciar.
4. **Conclusão:** Clique em **"Finalizar"** e a aplicação será iniciada automaticamente.

---

## **4. Configuração Inicial**

Após o primeiro acesso, o sistema necessitará estabelecer a conexão com o banco de dados.

1. **Banco Local (SQLite):** Na primeira execução, a aplicação WPF criará automaticamente o banco de dados SQLite local (`localcache.db`) na pasta `AppData` do usuário do Windows. 
2. **Comunicação com a Web API:** 
   - O arquivo `appsettings.json` armazena a URL base da API.
   - Certifique-se de que a chave `"ApiBaseUrl"` aponta para a URL do servidor de produção (ex: `https://api.ivecogreenledger.com.br`).
3. **Liberação de Firewall:** O suporte de TI da Iveco deve garantir que o tráfego HTTP/HTTPS (Portas 80 e 443) não esteja bloqueando a comunicação do aplicativo com o domínio da API e do Firebase.

---

## **5. Atualização do Sistema**

Para instalar futuras versões:

1. **Feche a Aplicação:** Garanta que o projeto não esteja rodando em segundo plano (Verifique através do Gerenciador de Tarefas).
2. **Execute o Novo Setup:** Inicie o arquivo `Setup.exe` da nova versão.
3. **Substituição Automática:** O instalador detectará a versão antiga e fará a substituição dos arquivos binários (`.dll` e `.exe`).
4. **Preservação de Dados:** O banco SQLite e os arquivos de configuração do usuário em `AppData` **não** serão apagados, garantindo que o operador não perca seu histórico local.

---

## **6. Desinstalação**

Para remover completamente a aplicação da estação de trabalho:

1. Abra as **Configurações do Windows** (ou Painel de Controle) > **Aplicativos e Recursos**.
2. Localize **Iveco Green Ledger** na lista de programas.
3. Clique em **Desinstalar** e siga as instruções do assistente.
4. **Limpeza Manual (Opcional):** Para apagar o cache residual, exclua a pasta `C:\Users\[NomeDoUsuario]\AppData\Roaming\Iveco\GreenLedger` (Atenção: isso apagará o banco SQLite local do usuário).
