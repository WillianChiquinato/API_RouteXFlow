# RouteXFlow — Contexto do Projeto

## 1. Visão geral

RouteXFlow é um sistema pessoal para auxiliar um entregador de delivery e entregadores marketPlace a gerenciar e combinar corridas provenientes de diferentes plataformas.
O projeto completo contará com a integração dos seguintes itens:

* iFood (Delivery)
* 99Food (Delivery)
* Keeta (Delivery)
* Shoppe (Pacotes - MarketPlace)
* Mercado Livre (Pacotes - MarketPlace)
* LalaMove (Pacotes - MarketPlace)
* Uber (Pacotes - MarketPlace)

Porem para inicialmente, será feito com:
* iFood (Delivery)
* 99Food (Delivery)
* Shoppe (Pacotes - MarketPlace)
* Uber (Pacotes - MarketPlace)

Para os deliveries, o objetivo principal é ser um copiloto do entregador para analisar novas ofertas de entrega, em relação às entregas que já estão ativas e informar rapidamente se uma nova corrida possui compatibilidade com a rota atual.

Agora para os MarketPlaces, será diferente, organizará a rota e será o copiloto para setar a rota e dar baixa nos pacotes, em modelos de sistemas do mercado, o entregador é necessário dar baixa no sistema do mercado livre / shoppe por exemplo, para ai sim dar baixa no sistema local ("Como Tracken ou Log Manager") para conseguir seguir a rota, a ideia aqui é o contrario, dar baixa no sistema local e ele faz a comunicação com os fornecedores e da baixa corretamente.

O projeto inicialmente será utilizado apenas pelo próprio desenvolvedor/entregador.

O sistema deve priorizar:

* Funcionamento local;
* Baixa latência;
* Comunicação entre dois celulares;
* Processamento em tempo real;
* Persistência histórica;
* Conseguir comunicação com os fornecedores para teste com cliente final (Entregadores);

---

# 2. Problema principal (Delivery)

Durante as entregas, podem surgir ofertas simultâneas em plataformas diferentes.

Exemplo:

* Uma corrida ativa da 99Food possui destino em determinada região.
* Enquanto essa corrida está ativa, o iFood oferece outra corrida.
* A nova corrida pode estar próxima ou na mesma direção da rota atual.
* O entregador precisa decidir rapidamente se aceita ou não.

O RouteXFlow deve analisar a nova oferta considerando:

* Localização atual;
* Entregas ativas;
* Pontos de coleta;
* Pontos de entrega;
* Distância adicional;
* Tempo adicional;
* Desvio da rota;
* Valor da corrida;
* Compatibilidade entre os destinos.

O sistema deve apresentar essas informações rapidamente no celular utilizado para navegação.

---

# 3. Problema principal (MarketPlace)

Durante a coleta de pacotes, o tempo que o entregador demora para realizar uma baixa no sistema pode acabar prejudicando ele no tempo final.

Exemplo:

* Um entregador X chega na casa do pacote 78, abre o aplicativo da shoppe e etc.. da baixa em 2 minutos, e adiciona mais 1 minuto e 30 segundos para abrir o aplicativo de rotas, dar baixa corretamente e seguir para a proxima.

Com o RouteXFlow é possivel dar baixa direto no aplicativo de rotas (RouteXFlow App) e automaticamente será feito a baixa automatica nos fornecedores, assim reduzindo tempo e ajudando os entregadores a chegarem cedo em casa.

# 4. Arquitetura dos dispositivos

O sistema utiliza dois celulares.

## Celular A — Engine / Manager

Responsabilidades:

* Executar o RouteXFlow;
* Receber/capturar informações das plataformas de delivery / Comunicação com os marketplaces;
* Executar Accessibility Service;
* Identificar novas ofertas;
* Extrair informações disponíveis na tela;
* Enviar informações para o Route Engine;
* Receber resultado da avaliação;
* Comunicar-se com o segundo celular via Bluetooth.

Esse celular pode permanecer no bolso durante as entregas e rotas.

---

## Celular B — Navigation Manager

Responsabilidades:

* Executar o RouteXFlow;
* Ficar fixado no suporte da motocicleta;
* Exibir informações importantes sobre novas ofertas;
* Exibir informações de rota;
* Percursos para MarketPlaces;
* Executar/abrir o Waze;
* Fornecer localização/GPS;
* Receber eventos do Celular A via Bluetooth.

Esse celular é o dispositivo de visualização durante a condução.

---

# 5. Sincronização entre celulares

Antes de iniciar o trabalho, o usuário abre o RouteXFlow e acessa o Admin.

O fluxo esperado é:

1. Abrir RouteXFlow nos dois celulares.
2. Ativar Bluetooth.
3. No celular principal, acessar o Admin.
4. Clicar em "Sincronizar".
5. O RouteXFlow procura dispositivos compatíveis.
6. O segundo celular é identificado.
7. Os dispositivos fazem handshake.
8. O pareamento é estabelecido.
9. O usuário inicia a sessão de trabalho.

O RouteXFlow deve possuir uma identidade própria para cada dispositivo.

Não utilizar o endereço MAC Bluetooth como identidade principal do dispositivo.

Exemplo:

```text
DeviceIdentifier:
RTXF-8F3A2C91
```

Os dispositivos possuem papéis:

```text
DELIVERY_ENGINE
NAVIGATION_MANAGER
```

---

# 6. Modelo de dados e tabelas de fluxo

Legenda: ✅ implementado (entidade + uso no código) · 🟡 entidade existe, mas sem fluxo/endpoint ainda · ⏳ ainda não modelado.

Toda entidade herda de `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`). Visão geral das relações:

```text
User
 ├── Container ──< ContainerDevices >── Device
 ├── WorkSession ── (ContainerId) ──▶ Container
 │     ├── GpsPositionHistory                (N posições)
 │     └── DeliveryOffers                    (N ofertas detectadas na sessão)
 │           ├── DeliveryStops               (N stops ordenados por Sequence)
 │           ├── RouteEvaluation             (avaliação da oferta)
 │           └── Deliveries                  (só existe se a oferta foi ACEITA)
 ├── FinanceEntry                            (N lançamentos)
 └── FinanceMonthClosure                     (1 por mês)

Status  → tabela genérica (seed) usada por DeliveryOffers e Deliveries
Apps    → plataformas (iFood, 99Food...) referenciadas por DeliveryOffers.AppId
```

## 6.1 WorkSession

Uma `WorkSession` representa um período de trabalho do entregador (uma "corrida" no Admin).

```text
User
  ↓
WorkSession
  └── Container
        ├── Device Manager     (Celular A)
        └── Device Navigation  (Celular B)
```

Campos: `UserId`, `ContainerId`, `State` (`StateSession`), `StartTime`, `EndTime?`.

`StateSession`: `Aberto (0)` · `Trabalhando (1)` · `Finalizado (2)` · `Pausada (3)`.

Regras vigentes:

* Só **uma** sessão aberta por usuário (`GetOpenWorkSessionAsync`) — senão "Já existe uma corrida em andamento".
* Para iniciar: exatamente **um container ativo** do usuário, com um device `Manager` **e** um `Navigation` com `Connected = true`.
* Ao iniciar grava `GpsPositionHistory` `StartPosition`; ao finalizar grava `FinishedPosition` (lat/long vêm no request).
* Só o dono finaliza a sessão; sessão com `EndTime` preenchido não pode ser finalizada de novo.

```text
Aberto ──▶ Trabalhando ──▶ Finalizado (EndTime preenchido)
              ↕
           Pausada
```

* `Aberto`: criada pelo start, ainda sem oferta/corrida.
* `Trabalhando`: passou a receber ofertas/deliveries e rotas.
* `Pausada`: entregador parou temporariamente (a sessão continua "aberta" para a regra de uma-por-usuário).
* `Finalizado`: `finish` preenche `EndTime` e troca o state.

## 6.2 Container e Device

* `Device`: celular com identidade própria (`DeviceIdentifier`, ex.: `RTXF-8F3A2C91` — nunca o MAC), `Name`, `Type` (`Manager` = Celular A, `Navigation` = Celular B, `Other`) e `Connected` (estado de conexão atual).
* `Container`: agrupa o par de celulares do usuário (`Name`, `UserId`, `IsActive`).
* `ContainerDevices`: vínculo N:N (`ContainerId`, `DeviceId`, `PairedAt`, `LastConnectedAt`, `IsActive`).
* Um usuário pode ter vários containers, mas **só um ativo** por vez para iniciar sessão.

## 6.3 DeliveryOffers

Oferta que apareceu numa plataforma. **Detectar ≠ aceitar.**

Ciclo alvo: `DETECTED → ANALYZED → ACCEPTED | REJECTED | EXPIRED`.

## 6.4 DeliveryStops

Ponto pertencente a uma oferta. Uma oferta tem **N** stops, ordenados por `Sequence`.

```text
Offer
 ├── Stop 1 - PICKUP      (Sequence 1)
 ├── Stop 2 - PICKUP      (Sequence 2)
 ├── Stop 3 - DELIVERY    (Sequence 3)
 └── Stop 4 - DELIVERY    (Sequence 4)
```

Necessário porque os deliveries podem ter múltiplas coletas e/ou entregas na mesma oferta.

Pontos de atenção: `Latitude`/`Longitude` estão padronizados como string, pois a coordenada pode ainda não ter sido geocodificada (seção 14: endereço vem antes da coordenada). O `Sequence` é a ordem **proposta pela plataforma**; a ordem final da rota é decidida pelo Route Engine.

## 6.5 RouteEvaluation

Análise da oferta considerando a rota atual. Campos: `DeliveryOfferId`, `Recommended`, `AdditionalDistanceKm`, `AdditionalTimeMinutes`, `RouteDeviationKm`, `ValuePerKm`, `EvaluationScore`, `Comments`. Valor/hora **não é persistido** (calculado sob demanda).

IMPORTANTE: a avaliação **não** é a decisão do usuário — só ajuda a decidir. Ela é gerada pelo Route Engine (seção 15) e pode ser recalculada quando a rota muda; hoje a relação com a oferta permite várias avaliações por oferta (definir se guarda histórico ou só a última).

## 6.6 Deliveries

Oferta efetivamente aceita.

```text
DeliveryOffer ──(ACCEPTED)──▶ Delivery (ACTIVE)
DeliveryOffer ──(REJECTED / EXPIRED)──▶ (nenhuma Delivery)
```

* Relação 1:1 com a oferta (uma oferta aceita gera **uma** Delivery).
* Os stops da Delivery são os da oferta (`DeliveryStops`); não duplicar.
* Distância/duração **reais** ficam aqui; as **estimadas** ficam na oferta — permite comparar previsto × realizado.

## 6.7 GpsPositionHistory

Tabela da localização do registro.

### 6.7.1 RoutePosition

Tabela que guarda a rota com o id do gpsPosition (Identificação de origin location e destination location, para fazer o rastreio no Front) com type pois será para delivery (Order / sequence setado manualmente via APP) e Marketplace Pacotes (Otimizado para criação de rotas).

### 6.7.2 RoutePositionStops

Tabela de paradas para cada rota desenhada.

## 6.8 Finance

`FinanceEntry` (`UserId`, `Type` earning/expense, `Source` manual/resgate, `Category`, `Description`, `Amount`, `Date`) e `FinanceMonthClosure` (`UserId`, `Month`, `Year`, `ClosedAt`, `TotalEarnings`, `TotalExpenses`, `Balance`; índice único por usuário/ano/mês). Inclui relatório de análise por IA (Groq) sobre dados **agregados**.

O `Source = resgate` é o gancho futuro para lançar ganhos automaticamente a partir de `Deliveries` concluídas (hoje é lançamento manual).

## 6.9 Tabelas de apoio

* `Apps`: plataformas (seed: iFood, 99Food, Keeta = Delivery; Shoppe, Mercado Livre = MarketPlace); `AppsVinculatedUser` liga o usuário aos apps que ele usa.
* `Status`: tabela genérica de status (seed 11–20).
* `User` / `Role`: autenticação e perfis (Super-Admin, Admin, Operação).

---

# 7. Fluxo de uma nova corrida (Delivery)

Pré-condição: existe uma `WorkSession` aberta (seção 6.1) e os dois celulares estão conectados por Bluetooth (seção 5).

```text
iFood / 99Food
        ↓
AccessibilityService        (Celular A)
        ↓
Captura da oferta           (texto bruto → RawData)
        ↓
Normalização → DeliveryOffer + DeliveryStops (modelo neutro, independente da plataforma)
        ↓
Route Engine → RouteEvaluation
        ↓
Bluetooth
        ↓
Navigation Manager          (Celular B)
        ↓
Usuário decide → ACCEPTED (cria Delivery) | REJECTED | EXPIRED
```

# 8. Fluxo de rota (MarketPlace)

Pré-condição: existe uma `WorkSession` aberta (seção 6.1) e pelo menos um celular conectado por Bluetooth (seção 5).

```text
Shoppe / Mercado Livre
        ↓
Estabele conexão com as contas dos fornecedores (Faz login na shoppe e no Mercado livre por exemplo)
        ↓
Coleta todos os endereços.
        ↓
Normalização de localizações
        ↓
Route Engine → Optimization Location with start and finished location
        ↓
Bluetooth
        ↓
Mostra a rota com os pacotes organizados e enumerados, com opção de detectação de pacote.
        ↓
Usuário aceita a corrida ou reotimiza
        ↓
Quando o usuário der baixa no sistema routeXFlow, sera consultado no fornecedor e a baixa será automatica.
```

---

# 9. Redis ⏳

**Ainda não integrado** (não está no código nem no `docker-compose`). Será introduzido quando o Route Engine precisar de estado de tempo real no back — se ele rodar no Android (recomendação em aberto), talvez nem seja necessário no MVP.

Uso previsto:

```text
routexflow:session:{sessionId}:location
```

Localização atual, rota atual, deliveries ativas, stops pendentes, estado temporário do Route Engine. PostgreSQL permanece como armazenamento histórico.

---

# 12. Backend

**Stack:** .NET 10 · EF Core 10 + PostgreSQL 16 · SeaweedFS (S3) · JWT · Groq (IA financeira). Redis planejado.
Arquitetura em camadas `API → Application → Domain` + `Infrastructure`: [specs/backend/architecture.md](specs/backend/architecture.md).

Responsabilidades: autenticação, persistência e histórico, sessões, containers/devices, ofertas/entregas/avaliações (⏳), financeiro, arquivos, configurações.

Contrato HTTP consumido por Admin e Mobile: [specs/backend/api-contract.md](specs/backend/api-contract.md).

A comunicação crítica entre os celulares deve funcionar localmente e **não depender do back nem da internet**:

```text
Celular A ↕ Bluetooth ↕ Celular B
```

---

# 13. Android ⏳

Não iniciado. O Android tem papel central no MVP: Accessibility Service para observar as interfaces dos apps de delivery.

IMPORTANTE: não assumir que todos os dados estarão na árvore de acessibilidade. Podem existir scroll horizontal, componentes customizados, renderização parcial e elementos que não aparecem juntos. Pode ser preciso executar ações de acessibilidade (scroll) para ler conteúdo adicional, e o layout varia entre versões dos apps — por isso os parsers são **por plataforma e versionados**, com testes de fixture.

Spec e decisões pendentes (Kotlin × cross-platform, RFCOMM × BLE, Route Engine no A × no back): [specs/mobile/README.md](specs/mobile/README.md).

---

# 14. CEP e localização

iFood e 99Food podem não exibir CEP. O Route Engine **não pode depender de CEP**.

```text
endereço → geocoding → latitude/longitude → distância → compatibilidade de rota
```

CEP é apenas informação auxiliar, quando existir.

---

# 15. Route Engine ⏳

Responde: "**Como essa nova oferta altera a rota atual?**" Não compara dois CEPs; analisa a **sequência de stops**.

```text
Localização atual → Pickup A → Delivery A → Pickup B → Delivery B
```

Uma oferta nova tem várias posições possíveis de inserção; o engine testa as válidas (pickup antes do respectivo delivery) e calcula o impacto.

Métricas: distância adicional, tempo adicional, desvio, valor/km, valor/hora (calculado), compatibilidade direcional, quantidade de stops, impacto nas entregas existentes.

Versão 1 = heurística de inserção simples (ver spec). Detalhes e perguntas em aberto: [specs/backend/features/offers-route-engine.md](specs/backend/features/offers-route-engine.md).

---

# 16. Princípios de arquitetura

Versão normativa: [specs/constitution.md](specs/constitution.md). Resumo:

* MVP = **monólito**; sem microservices sem necessidade concreta.
* Sem abstrações, tabelas para dados derivados, ou índices prematuros.
* Tempo real fora do PostgreSQL.
* Route Engine desacoplado da UI das plataformas.
* Separação: `Captura → Normalização → Domínio → Route Engine → Comunicação → UI`.
* **Segurança por dono**: todo recurso é filtrado pelo `userId` do token; nunca devolver entidades cruas.

---

# 17. Regras para a IA

Ao trabalhar neste projeto, a IA deve seguir a [constituição](specs/constitution.md) e o fluxo SDD ([specs/README.md](specs/README.md)):

1. Ler a spec da feature antes de codar; se não existir, propor a spec primeiro.
2. Respeitar as entidades e relações definidas; não criar/remover entidade sem justificar o impacto.
3. Diferenciar sempre: oferta · avaliação · entrega · stop · sessão.
4. Não assumir que oferta detectada foi aceita.
5. Não assumir que oferta tem só uma coleta e uma entrega.
6. Não assumir que CEP existe.
7. Não tratar o Bluetooth como dependente do back.
8. Sem microservices no MVP sem necessidade concreta.
9. Priorizar processamento local nas operações críticas de tempo real.
10. Antes de alterar o banco: validar o impacto no fluxo e criar migration + atualizar `data-model.md`.
11. Antes de criar índice: validar a consulta e medir.
12. Ambiguidade arquitetural: apresentar alternativas e impactos antes de alterar o modelo.
13. Mudança de contrato HTTP: atualizar `api-contract.md` primeiro e avisar Admin/Mobile.

---

# 18. Estado atual e roadmap

## Feito ✅
Autenticação (JWT + cookie), cadastro de usuário e apps, containers/devices, work sessions (start/finish/histórico/detalhe), financeiro completo com fechamento mensal e relatório de IA, storage de arquivos, Admin web (login, sync, corridas, financeiro).

## Próximos passos (ordem sugerida)

1. **Estabilizar o back** — corrigir gaps 🔴 de segurança ([specs/backend/gaps.md](specs/backend/gaps.md): G-01, G-02, G-03, G-10, G-13, G-14, G-12) e o bug de `appsActives` (G-28).
2. **Fechar o admin** — endpoints de recuperação de senha, dashboard com dados reais, layout compartilhado ([admin specs](../admin-routeXflow/specs/README.md)).
3. **Decidir e registrar** (ADR): onde roda o Route Engine; Kotlin nativo; Bluetooth RFCOMM × BLE; provedor de geocoding.
4. **Modelar** status de oferta/delivery e ajustar `Deliveries`/`DeliveryStop`/`GpsPosition` (G-07, G-18, G-19).
5. **Implementar** endpoints de oferta/decisão/delivery/GPS em lote.
6. **App Android**: identidade RTXF, sincronização Bluetooth, captura por Accessibility (iFood e 99Food primeiro).
7. **Route Engine v1** com fixtures do exemplo da seção 8.
8. **MarketPlace** (Shoppe/Uber): estudo de viabilidade da baixa automática antes de especificar.
9. **Testes e CI**; otimizar consultas/índices só após medir uso real.

## Questões de negócio a validar
* Aceite automático via acessibilidade pode violar termos das plataformas (risco de banimento) — validar antes de automatizar toques.
* Mercado Livre, Keeta, LalaMove: fora do escopo inicial; Uber e LalaMove ainda sem seed em `apps`.
