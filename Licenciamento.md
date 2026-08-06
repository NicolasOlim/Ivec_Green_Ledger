# 📦🍃 Iveco Green Ledger – Termos de Licenciamento

Este documento define os termos de uso, política de versionamento e releases do projeto.

<div class="logo-container" align="center">
    <img src="imagens/logo-licenciamento.png" alt="Logo Iveco Green Ledger" class="logo-img">
</div>


## **1. Informações Gerais**

Esta seção estabelece as informações fundamentais de identificação do software, contextualizando o escopo da aplicação e o ambiente de destino para a definição das diretrizes de licenciamento corporativo, uso de componentes de terceiros e controle de releases do projeto.

- **Nome do Sistema:** Iveco Green Ledger.
- **Versão Atual:** 1.0.0 (Release Operacional Base).
- **Tipo de Software:** Aplicação corporativa distribuída (Desktop Cliente WPF + Web API ASP.NET Core).
- **Proprietário / Cliente Final:** IVECO.
- **Ambiente de Destino:** Automação de triagem física, validação de veículos e monitoramento de emissões de CO2.
- **Modelo de Arquitetura:** Execução local híbrida em borda (SQLite) com consolidação de dados em nuvem pública (Google Firebase Firestore).

---

## **2. Termos de Licenciamento e Uso**

O projeto é regido pelos seguintes termos de uso, propriedade intelectual e conduta operacional.

- **Uso Proprietário Exclusivo:** O software foi desenvolvido sob medida para atendimento das demandas logísticas e de governança ambiental da montadora IVECO. O código-fonte, marcas, arquitetura e binários compilados constituem propriedade intelectual restrita e confidencial.
- **Restrições de Distribuição e Modificação:** É expressamente proibida a cópia, redistribuição comercial, sublicenciamento, modificação por terceiros não autorizados ou tentativa de engenharia reversa das aplicações desktop (WPF) e serviços de backend (API REST).
- **Controle de Acesso e Operação:** A utilização da aplicação por operadores de triagem é controlada individualmente via autenticação de perfil na nuvem. As credenciais são de uso pessoal, intransferível e estritamente vinculadas às funções do colaborador.
- **Auditoria e Monitoramento:** O sistema registra logs de acesso, transações de triagem e alterações de dados. A IVECO reserva-se o direito de monitorar o uso da aplicação para garantir a conformidade operacional, segurança da informação e auditoria ambiental (emissões de CO2).

---

## **3. Garantias e Limitação de Responsabilidade**

Por se tratar de um sistema de uso corporativo interno, aplicam-se as seguintes resguardas legais e operacionais:

- **Disponibilidade e Manutenção:** O software é fornecido para integração com os fluxos da IVECO, estando sua operação contínua sujeita à estabilidade da infraestrutura de rede, hardware local (borda) e serviços de nuvem de terceiros (Google Firebase/Microsoft SQL Server).
- **Isenção de Danos Indiretos:** Os desenvolvedores e mantenedores do projeto não se responsabilizam por eventuais perdas financeiras, interrupções logísticas severas ou danos indiretos causados por mau uso, falhas de inserção de dados pelo operador ou indisponibilidade imprevista de provedores de nuvem.
- **Privacidade e Proteção de Dados (LGPD):** O sistema processa dados corporativos e credenciais de colaboradores em conformidade com as diretrizes da Lei Geral de Proteção de Dados. Nenhuma informação pessoal ou logística sensível é comercializada ou compartilhada fora do ecossistema de controle da IVECO.

---

## **4. Política de Versionamento e Releases**

Para garantir a estabilidade e rastreabilidade da aplicação, o ciclo de vida do software adota as seguintes diretrizes:

- **Atualizações Obrigatórias:** Por questões de segurança, conformidade ambiental e sincronização de dados com a nuvem, os clientes desktop instalados nas estações de triagem devem ser mantidos na versão mais recente homologada pela equipe de TI.
- **Ambientes de Implantação:** Qualquer alteração no código passa obrigatoriamente por ambientes de Homologação (Staging) antes de ser promovida ao ambiente de Produção.

---

## **5. Bibliotecas e Componentes**

O ecossistema utiliza dependências e bibliotecas de código aberto e proprietárias para viabilizar funcionalidades específicas. Todas as bibliotecas integradas operam sob licenças que autorizam o uso corporativo do sistema:

| Componente / Biblioteca | Licença de Terceiros | Finalidade no projeto |
| :--- | :--- | :--- |
| **.NET 8 SDK / WPF** | MIT License | Framework base para desenvolvimento do cliente desktop e da API REST. |
| **Microsoft Visual Studio 2022** | Proprietária / Community | Ambiente de Desenvolvimento Integrado (IDE) utilizado para codificação, depuração e compilação. |
| **Google Firebase (Firestore)** | Apache License 2.0 | Serviço de nuvem (NoSQL) utilizado para consolidação de dados de emissões e autenticação. |
| **Microsoft SQL Server** | Proprietária / Express ou Developer | Banco de dados relacional robusto utilizado para o armazenamento principal e persistência estruturada dos dados. |
| **SQLite** | Domínio público | Banco de dados relacional leve para execução local híbrida e armazenamento na borda. |

---

*Projeto desenvolvido para fins educacionais no Curso Técnico em Desenvolvimento de Sistemas – SENAI / Escola de Programação e Robótica.*  
*Última atualização: 06 de agosto de 2026.*
