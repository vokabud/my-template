# AI-Assisted Development Guide

This guide helps developers collaborate with AI coding agents in this repository. It is tool-neutral: use it with repository-aware coding agents or with chat assistants that can read files you provide.

Repository-aware agents may discover `AGENTS.md` automatically. Other tools may not. For architecture-sensitive, compatibility-sensitive, or high-risk work, explicitly name the documents the agent must read even if automatic discovery is available.

Bracketed values such as `[objective]` and `[affected area]` are fields for you to replace. They are not unfinished documentation.

## Documentation map

| Document | Purpose and authority |
| --- | --- |
| [README](../README.md) | Human onboarding, runtime prerequisites, and basic commands. |
| [Agent operating contract](../AGENTS.md) | Authoritative repository boundaries, agent workflow, commands, verification expectations, and documentation-maintenance rules. |
| [Architecture reference](architecture.md) | Verified implemented behavior, runtime flows, configuration, and compatibility-sensitive surfaces. |
| [Roadmap](roadmap.md) | Planned capabilities and technical debt. Roadmap entries are not implemented behavior. |
| This guide | Developer workflows and prompt templates for collaborating with AI agents. |

Name the architecture reference when asking how the system currently works. Name the roadmap when discussing future work. For implementation, debugging, and review, also direct the agent to the affected source files.

## A practical prompt model

Effective prompts answer seven questions:

1. **Context:** Which documents and source areas should the agent inspect?
2. **Objective:** What outcome do you need?
3. **Scope:** What is included and excluded?
4. **Constraints:** Which architecture, compatibility, safety, or documentation rules apply?
5. **Process:** Are you authorizing analysis, planning, implementation, diagnosis, or read-only review?
6. **Verification:** What evidence must the agent collect?
7. **Deliverable:** Do you want an explanation, plan, code change, review, or documentation update?

You usually do not need a long prompt. Let repository documentation supply stable context and state the decisions the agent cannot safely infer.

### Base template

```text
Read AGENTS.md and [relevant documentation or source].

Objective:
[desired outcome]

Scope:
- In: [areas or behavior]
- Out: [excluded areas or behavior]

Constraints:
[architecture, compatibility, safety, or documentation rules]

Before acting:
[analyze, ask material questions, diagnose, or prepare a plan]

Verification:
[required checks]
Report anything you could not verify and why.

Deliverable:
[explanation, plan, implementation, review findings, or documentation update]
```

## Architecture questions

Use this workflow to understand the current design, evaluate boundaries, or compare architectural options. It authorizes analysis only unless you explicitly request changes.

Read `AGENTS.md` and `docs/architecture.md`; include `docs/roadmap.md` when the question involves future work. Ask the agent to inspect relevant source rather than relying only on summaries.

### Template

```text
Read AGENTS.md, docs/architecture.md, [other relevant documentation], and [relevant source].

Objective:
Explain [architecture question or decision].

Scope:
- In: [components and interactions to analyze]
- Out: [excluded components or implementation work]

Constraints:
Preserve [boundaries or compatibility-sensitive contracts]. Distinguish verified current-state facts from recommendations and roadmap intent.

Process:
Analyze only. Do not modify files. Identify affected boundaries, data flows, dependencies, and compatibility surfaces. Compare alternatives when more than one is reasonable.

Verification:
Cite repository files for factual claims and state assumptions.
Report anything you could not verify and why.

Deliverable:
A concise explanation with facts, recommendations, trade-offs, and open decisions clearly separated.
```

### Example

> Read `AGENTS.md`, `docs/architecture.md`, `docs/roadmap.md`, and the messaging configuration. Analyze where the planned Kafka consumer should live. Preserve the independently runnable API and existing Kafka contracts. Do not change files. Separate current facts from recommendations and compare the viable service boundaries.

### Accept the result when

- Current-state claims cite source or documentation evidence.
- Recommendations and trade-offs are labeled rather than presented as facts.
- Compatibility effects and unresolved decisions are explicit.
- No files changed.

## Planning

Use this workflow after the objective is understood but before substantial or architecture-sensitive implementation.

### Template

```text
Read AGENTS.md, docs/architecture.md, docs/roadmap.md, and [relevant source].

Objective:
Prepare an implementation plan for [desired outcome].

Scope:
- In: [planned components or behavior]
- Out: [explicit exclusions]

Constraints:
Preserve [architecture and compatibility requirements]. Include documentation updates required by AGENTS.md.

Process:
Do not change files. Analyze the current state, ask only material clarifying questions, and compare two or three viable approaches with trade-offs before recommending one.

Verification:
Define focused tests, integration checks, build checks, and documentation validation for the future implementation.
Report anything you could not verify and why.

Deliverable:
An ordered implementation plan with exact file areas, dependencies, acceptance criteria, risks, verification, and open decisions.
```

### Example

> Plan Kafka consumption using `AGENTS.md`, the architecture reference, roadmap, and messaging source. Preserve the independently runnable API and compatibility-sensitive Kafka topic and payload contracts. Compare a consumer hosted in the API with a dedicated worker, recommend one, and produce a plan only—do not implement it.

### Accept the result when

- The plan has clear scope, sequencing, dependencies, and acceptance criteria.
- Architecture and compatibility constraints appear in concrete tasks.
- Verification and same-change documentation updates are planned.
- Open decisions are visible instead of silently assumed.

## Implementation

Use this workflow only when you are authorizing file changes. For significant work, reference an approved design or plan.

### Template

```text
Read AGENTS.md, docs/architecture.md, [approved design or plan], and [affected source].

Objective:
Implement [approved outcome].

Scope:
- In: [files, components, and behavior authorized to change]
- Out: [excluded changes and refactoring]

Constraints:
Preserve [architectural invariants and compatibility-sensitive contracts]. Make the smallest coherent change. Update every document made inaccurate by the implementation.

Process:
Implement the approved scope. Stop and ask if a decision would materially expand or contradict it.

Verification:
Run [focused tests/checks] and [broader required checks].
Report anything you could not verify and why.

Deliverable:
The implementation plus a summary of changed files, verification evidence, limitations, compatibility effects, and documentation updates.
```

### Example

> Add a validated optional description filter to the task-list endpoint. Read the agent contract, architecture reference, endpoint, request/response contracts, and handler. Keep the route version and existing response contract compatible, preserve the transactional-outbox invariant, avoid unrelated refactoring, add proportionate tests if a test project exists, and update affected documentation in the same change. Report omitted checks.

### Accept the result when

- The diff stays within the authorized scope and follows repository boundaries.
- Compatibility and architectural invariants are preserved or explicitly discussed.
- Verification evidence is concrete and limitations are disclosed.
- Documentation changed wherever implementation made it inaccurate.

## Debugging

Use this workflow to investigate unexpected behavior. A diagnosis request does not authorize a fix.

### Template

```text
Read AGENTS.md, docs/architecture.md, [symptom evidence], and [affected source or logs].

Objective:
Determine the root cause of [symptom].

Scope:
- In: [systems, time range, scenarios, or files to investigate]
- Out: implementation changes unless separately approved.

Constraints:
Preserve evidence. Do not change code, configuration, data, or external systems. Distinguish symptoms, contributing factors, and root cause.

Process:
Reproduce when safe, collect evidence, test hypotheses, and identify the smallest proven cause. Ask before any destructive or externally visible action.

Verification:
Show the reproduction or diagnostic evidence supporting the conclusion.
Report anything you could not verify and why.

Deliverable:
A diagnosis with evidence, affected scope, confidence, and ranked fix options. Do not implement a fix.
```

### Example

> Diagnose why outbox messages repeatedly fail and retry. Read the agent contract, architecture reference, outbox processor, publisher, configuration, and available logs. Do not modify files or data. Reproduce safely if possible, separate Kafka connectivity symptoms from serialization or retry-state causes, cite evidence, and propose fix options without implementing them.

### Accept the result when

- The report contains reproduction or diagnostic evidence.
- Symptom, contributing factors, and root cause are distinguished.
- Confidence and unverified hypotheses are stated.
- No fix was silently implemented.

## Code review

Use this workflow for a read-only assessment of a diff, branch, pull request, or specified files.

### Template

```text
Read AGENTS.md, docs/architecture.md, [requirements or plan], and [change set].

Objective:
Review [change] against its requirements and repository constraints.

Scope:
- In: [diff, files, or behaviors to review]
- Out: unrelated existing code and implementation changes.

Constraints:
Review read-only. Check architecture boundaries, compatibility-sensitive contracts, security, correctness, verification quality, and required documentation updates.

Process:
Prioritize findings as Critical, Important, or Minor. Cite file and line, explain impact, and propose a correction. Separate defects from optional improvements.

Verification:
Inspect supplied verification evidence. Run only focused read-only checks needed for a concrete risk.
Report anything you could not verify and why.

Deliverable:
Severity-ranked findings, strengths, unresolved questions, and a clear readiness verdict. Do not modify files.
```

### Example

> Review the task endpoint contract change against its requirements, `AGENTS.md`, and the architecture reference. Check route and DTO compatibility, handler boundaries, persistence effects, and whether mutations preserve the transactional outbox. Review read-only, cite exact lines, rank findings by severity, and give a readiness verdict.

### Accept the result when

- Findings cite evidence and explain practical impact.
- Blocking defects are distinct from optional polish.
- Compatibility, tests, and documentation are considered.
- No files changed.

## Documentation updates

Use this workflow when implementation or operating practices make existing documentation inaccurate.

### Template

```text
Read AGENTS.md, README.md, docs/architecture.md, docs/roadmap.md, and [changed source or verified implementation evidence].

Objective:
Update documentation to reflect [verified change].

Scope:
- In: [documents made inaccurate by the change]
- Out: application changes and unsupported roadmap/status changes.

Constraints:
Verify every current-state claim against source. Keep implemented behavior separate from planned work. Update roadmap status only when implementation and verification evidence exist. Avoid duplicating details whose primary home is another document.

Process:
Identify every affected document, make the smallest consistent updates, and preserve relative links and command accuracy.

Verification:
Validate claims against source, check relative links and documented commands, and scan for contradictory current/planned language.
Report anything you could not verify and why.

Deliverable:
Documentation updates plus a claim-to-source summary, link/command check results, and any remaining uncertainty.
```

### Example

> Automated unit and integration tests have been added and verified. Update documentation based on the actual test projects and commands. Review `README.md`, `AGENTS.md`, the architecture reference, and the roadmap test item. Mark roadmap work complete only if its acceptance outcome is satisfied. Validate links and commands; do not change application code.

### Accept the result when

- Documentation claims agree with source and verification evidence.
- Current behavior and roadmap intent remain distinct.
- Status changes have observable supporting evidence.
- Links and commands resolve and no unnecessary duplication was added.

## Weak and effective prompts

Weak:

> Add Kafka consumer.

This does not say whether you want architecture analysis, a plan, or implementation. It omits service scope, compatibility constraints, acceptance criteria, verification, and documentation obligations.

Effective:

> Read `AGENTS.md`, `docs/architecture.md`, `docs/roadmap.md`, and the messaging source. Prepare a plan for the roadmap's Kafka consumption capability; do not implement it. Compare hosting it in `Template.Api` with a dedicated worker. Preserve the independently runnable API and existing Kafka topic/payload contracts. Define tests, failure handling, observability, and documentation updates. Report anything you could not verify and why. Deliver an ordered plan with trade-offs and open decisions.

The effective prompt adds decisions the agent cannot safely infer. It does not prescribe unnecessary implementation detail.

## Safety and collaboration rules

- Say **analyze**, **plan**, **diagnose**, or **review read-only** when you are not authorizing changes.
- Separate planning approval from implementation for substantial or architecture-sensitive work.
- Require confirmation before destructive actions, data changes, deployments, messages, or other external effects.
- Ask for command output or equivalent evidence, not a generic claim that checks passed.
- Require the agent to report checks it could not run and why.
- Review the final diff, verification evidence, compatibility effects, and documentation updates before accepting work.
- Never paste passwords, tokens, connection strings, or other secrets into prompts. Refer to configuration keys and approved secret stores.

## Before accepting agent work

- Did the agent stay within the authorized workflow and scope?
- Are factual claims backed by source or command evidence?
- Were architectural and compatibility constraints preserved?
- Are skipped checks and limitations explicit?
- Does the diff avoid unrelated changes and destructive actions?
- Were all documents made inaccurate by the change updated?
- Do current-state documents and roadmap status still agree?
