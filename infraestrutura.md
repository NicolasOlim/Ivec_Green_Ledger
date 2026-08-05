# Infraestrutura do Sistema  Green Ledger

## 1. Informações do Projeto

| Campo | Descrição |
| :--- | :--- |
| **Nome do Projeto** | Green Ledger – Sistema de Rastreamento Inteligente |
| **Versão** | 1.0 (TCC) |
| **Objetivo** | Automatizar a triagem de materiais no pátio da Iveco com cálculo automático da pegada de carbono, utilizando persistência centralizada em nuvem e integração contínua com APIs externas, garantindo rastreabilidade total |
| **Público‑alvo** | Operadores de portaria, analistas de logística e gestores de sustentabilidade da Iveco. |



## 2. Requisitos de Hardware

### 2.1 Estação de Trabalho (Cliente WPF)

| Componente | Mínimo | Recomendado |
| :--- | :--- | :--- |
| **Processador** | Intel Core i3 | Intel Core i5 ou superior |
| **Memória RAM** | 4 GB |12 GB  |
| **Armazenamento** | 600 MB livres | 1,5 GB livres |
| **Sistema Operacional** | Windows 10 (64‑bit) | Windows 11 (64‑bit) |
| **Rede** | Placa de rede Ethernet/Wi‑Fi com acesso à internet | Conexão banda larga estável |

### 2.2 Servidor (API e Nuvem)

- **API REST:** Hospedagem em servidor Windows/Linux com suporte a ASP.NET Core 8.0 (pode ser máquina virtual ou contêiner).
- **Banco de Dados Cloud:** Firebase Firestore (Google Cloud Platform) – Plano Spark (gratuito) para uso inicial, escalável para Plano mais robusto sob demanda.
- **Conectividade:** Acesso HTTPS outbound liberado para APIs externas (BrasilAPI, NHTSA, Mercado Livre).



## 3. Requisitos de Software

### 3.1 Cliente Desktop

| Software | Versão | Finalidade |
| :--- | :--- | :--- |
| **Microsoft .NET Runtime** | 8.0  | Obrigatório para execução do aplicativo WPF |
| **Windows 10 / 11** | 64‑bit | Sistema operacional base |

### 3.2 Servidor

| Software | Versão | Finalidade |
| :--- | :--- | :--- |
| **ASP.NET Core Runtime** | 8.0.29 | Hospedar a API REST |
| **Firebase Admin SDK** | via NuGet | Comunicação com Firestore |
| **Certificado SSL/TLS** | - | Comunicação HTTPS obrigatória |



## 4. Dependências

### 4.1 Pacotes NuGet (WPF)

| Pacote | Versão | Finalidade |
| :--- | :--- | :--- |
| `Microsoft.Extensions.Http` | **8.0.0** | Cliente HTTP para consumo da API REST e serviços externos |
| `Newtonsoft.Json` | **13.0.3** | Serialização/desserialização JSON (fallback e cache local) |
| `LiveChartsCore.SkiaSharpView.WPF` | **2.0.0** | Exibição de gráficos e dashboards interativos |
| `QuestPDF` | **2024.10.1** | Geração de relatórios e etiquetas em PDF |
| `Serilog.Sinks.File` | **6.0.0** | Registro de logs em arquivo local (offline e diagnóstico) |

### 4.2 Pacotes NuGet (API)

| Pacote | Versão | Finalidade |
| :--- | :--- | :--- |
| `FirebaseAdmin` | **3.1.0** | SDK oficial para autenticação e administração do Firebase |
| `Google.Cloud.Firestore` | **3.9.0** | Cliente nativo para operações CRUD no Firestore (modo Datastore) |
| `Swashbuckle.AspNetCore` | **7.0.0** | Geração automática da documentação Swagger/OpenAPI |
| `Serilog.AspNetCore` | **8.0.3** | Log estruturado no lado do servidor com sinks para arquivo e console |

> **Observação:** O .NET Runtime utilizado (tanto no cliente quanto no servidor) deve ser a versão **8.0.10** (último patch LTS estável). Todos os pacotes acima são compatíveis entre si e com o ecossistema .NET 8.

### 4.3 Serviços Externos (APIs de Terceiros)

| API | Função | Dependência de Internet |
| :--- | :--- | :---: |
| **BrasilAPI** | Validação de CNPJ de fornecedores | Sim |
| **NHTSA VPIC** | Decodificação do chassi (VIN) e validação da Iveco | Sim |
| **Mercado Livre Developers** | Rastreamento da rota de entrega (status) | Sim |
| **Firebase Firestore** | Persistência definitiva e dashboards | Sim (para sync) |



## 5. Arquitetura do Sistema

| Componente | Detalhes |
| :--- | :--- |
| **Cliente WPF (NET .8)** | Arquitetura MVVM,  logs NoSQL e sincronização com retry.  |
| **API REST** |  Centraliza as regras de negócio, orquestração e validações. Consome APIs externas (NHTSA, BrasilAPI, ML) e persiste os dados finais no Firebase. |
| **Banco de Dados** | Combina **SQLite**  com **Firebase Firestore / NoSQL** . O Firestore armazena os dados consolidados. |



## 6. Riscos Identificados

| Risco | Probabilidade | Impacto | Mitigação |
| :--- | :--- | :--- | :--- |
| **Queda de conectividade** | Alta | Médio | Operação offline com SQLite; sincronização automática ao reconectar. |
| **Cota gratuita do Firebase excedida** | Média | Alto | Migrar para plano Blaze ou banco relacional próprio. |
| **Indisponibilidade das APIs externas** (NHTSA, BrasilAPI) | Média | Médio | Cache local de CNPJ/VIN já validados; fallback para verificação manual. |
| **Incompatibilidade de versão do .NET Runtime** | Baixa | Alto | Verificar instalação do runtime 8.0 no checklist de implantação; distribuir instalador junto. |
| **Falha de hardware no terminal de pátio** | Baixa | Médio | Manter máquina reserva configurada; protocolo de registro manual até substituição. |
| **Ataques de brut force ou SQL Injection** | Baixa | Crítico | HTTPS mandatório, CORS restrito, hash de senhas, validação de inputs. |



## 7. Plano de Contingência

### 7.1 Perda de Conexão com a Internet

- O sistema **continua operando** localmente: todos os registros de triagem são persistidos no SQLite.
- Ao restabelecer a rede, um processo em background (worker) varre o SQLite e envia os dados em lote para a API → Firestore.
- Dashboards gerenciais serão atualizados automaticamente após a sincronização.

### 7.2 Indisponibilidade de API Externa (ex.: BrasilAPI fora do ar)

- Se a validação de CNPJ falhar por timeout, o sistema exibe um alerta e permite a **digitação manual** do fornecedor, marcando o registro como “pendente de verificação fiscal”.
- Uma rotina posterior (ou job agendado) reprocessa as pendências.

### 7.3 Falha Física do Terminal de Pátio

- A equipe de portaria retorna temporariamente ao **formulário em papel** (plano de continuidade operacional).
- Suporte de TI substitui a máquina com o software pré‑instalado em até 1 hora.
- Dados registrados em papel são digitados no sistema após a recuperação.

### 7.4 Backup dos Dados

- **Firestore:** Backup automático diário configurado no Console do Google Cloud.
- **SQLite:** Cópia do arquivo local agendada para unidade de rede ou nuvem (OneDrive/SharePoint) a cada 24h.
- **API:** Código‑fonte versionado no GitHub, com pipeline de CI/CD para deploy rápido.

---

*Documento elaborado para o Projeto de TCC – SENAI Nova Lima, conforme atividade prática de Infraestrutura de Software.*
*Última atualização: 04/08/2026.*

