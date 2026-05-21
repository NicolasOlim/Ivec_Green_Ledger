# Implementacao da arquitetura MVVM no Projeto Iveco
Repositório criado para a realização documentação da implementação do MVVM ao nosso projeto da iveco

# 🚛 Documentação do Banco de Dados: Cadeia de Suprimentos e Veículos

Bem-vindo à documentação do esquema de banco de dados desenvolvido para o gerenciamento e rastreabilidade da cadeia de suprimentos de veículos. Este modelo permite o controle de fornecedores, lotes de matérias-primas (incluindo o monitoramento da pegada de carbono), registro de veículos e a associação exata de quais componentes e lotes compõem cada veículo.

## 📑 Índice
- [Visão Geral](#visao-geral)
- [Diagramas](#diagramas)
- [Dicionário de Dados](#dicionario-de-dados)
- [Relacionamentos](#relacionamentos)
- [Script SQL (SQLite)](#script-sql)
- [Estrutura de Diretórios](#estrutura-de-diretorios)

---

<a id="visao-geral"></a>
## 🎯 Visão Geral

O sistema é composto por 4 entidades principais, estruturadas para suportar um banco de dados relacional (focado em SQLite):
1. **Fornecedor**: Gerencia os dados das empresas que fornecem os materiais.
2. **LoteMateriaPrima**: Registra as entradas de materiais, quantidades e métricas ambientais (pegada de carbono).
3. **Veiculo**: Cadastro dos veículos produzidos/montados.
4. **VeiculoComponente**: Tabela associativa que rastreia qual peça exata, originada de qual lote de matéria-prima, foi instalada em um veículo específico.

---

<a id="diagramas"></a>
## 📊 Diagramas

Para facilitar o entendimento da arquitetura, consulte os diagramas abaixo que representam as fases de modelagem.

### Modelo Conceitual (MER)
Representação de alto nível das entidades, atributos e suas cardinalidades.
