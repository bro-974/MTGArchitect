---
applyTo: "back/**/*.{cs,csproj,props,targets,json}"
---
You are an expert in C#, .NET, and scalable web application development. You write functional, maintainable, performant, and accessible code following .NET best practices and clean architecture principles.
# 🤖 Copilot / AI Assistant Rules – C# Project

## 🎯 Goal
Generate clean, layered, maintainable code following strict architecture rules.

---

# 🧱 ARCHITECTURE (MANDATORY)

Layers:
- API → Controller → Client → gRPC Service → Core (Mapping)

Never skip layers.

---

# 🚫 HARD RULES (ALWAYS ENFORCED)

## ❌ NEVER DO THIS

- Business logic in API or gRPC Service
- Mapping outside /core/
- Direct gRPC usage outside Client/Service
- new Service() (no manual instantiation)
- .Result or .Wait()
- Missing CancellationToken
- Circular dependencies

---

# 🌐 API RULES

## ✅ DO

- Use EndpointExtensions
- Call Controller only

## ❌ DON'T

- No LINQ (.Select, .Where)
- No mapping
- No logic

### Pattern
app.MapGet("/route", async (..., [FromServices] XController ctrl, CancellationToken ct)
    => await ctrl.Method(...));

---

# 🧠 CONTROLLER RULES

## ✅ DO

- Business logic
- Orchestration
- Use interfaces

## ❌ DON'T

- No mapping
- No proto usage
- No ServerCallContext

---

# 🔌 CLIENT RULES

## ✅ DO

- Wrap gRPC calls
- Map proto ⇄ domain
- gRPC wrapper behind interfaces

## ❌ DON'T

- No business logic

---

# ⚙️ GRPC SERVICE RULES

## ✅ DO

- Call Controller
- Return mapped result

## ❌ DON'T

- No logic
- No loops
- No conditions

---

# 🔄 MAPPING RULES

## ONLY in /core/

## ✅ DO

- Static helpers
- Pure functions

## ❌ DON'T

- No services
- No logic

---

# 🧩 DI RULES

## ✅ DO

- Constructor injection
- Interfaces
- Module registration

## ❌ DON'T

- No new
- No service locator

---

# 📁 NAMING

- Controller → *Controller
- Service → *Service
- Client → *Client
- Interface → I*
- Mapping → *MappingHelpers

---

# ⚡ METHOD RULES

- async only
- CancellationToken required
- ≤ 20 lines recommended

---

# 🧠 AI DECISION GUIDE

If you need to:

- Business logic → Controller
- External call → Client
- Mapping → Core
- Endpoint → API
- gRPC → Service + Controller

---

# ✅ CHECK BEFORE GENERATING

- Correct layer?
- No forbidden patterns?
- Mapping isolated?
- Interfaces used?
- Async respected?

---

# 📌 GOLDEN RULE

Lowest valid layer wins.
Never break separation of concerns.
