# Implementation Quick Reference

Quick navigation and checklist for implementing DynamicForms.Core.V2 using the prompts in IMPLEMENTATION_PROMPTS.md.

---

## 📋 Implementation Phases Overview

| Phase | Focus | Prompts | Estimated Time |
|-------|-------|---------|----------------|
| **Phase 1** | Project & Schemas | 1.1 - 1.7 | 1.5 hours |
| **Phase 2** | Runtime State | 2.1 - 2.3 | 30 minutes |
| **Phase 3** | Service Interfaces | 3.1 - 3.5 | 30 minutes |
| **Phase 4** | Service Implementations | 4.1 - 4.3 | 2 hours |
| **Phase 5** | Dependency Injection | 5.1 | 15 minutes |
| **Phase 6** | Unit Tests | 6.1 - 6.5 | 2 hours |
| **Phase 7** | Integration Tests | 7.1 | 45 minutes |
| **Phase 8** | Documentation | 8.1 - 8.2 | 30 minutes |
| **Phase 9** | Verification | 9.1 - 9.2 | 45 minutes |
| **Phase 10** | Final Deliverables | 10.1 - 10.2 | 30 minutes |
| **TOTAL** | | **32 prompts** | **~8 hours** |

---

## 🚀 Quick Start - Essential Prompts Only

If you want to get a minimal working implementation quickly, execute these prompts in order:

### Minimal Implementation (3 hours)
1. **1.1** - Create project structure ⚡
2. **1.2** - Create enums ⚡
3. **1.3** - Create supporting records ⚡
4. **1.5** - Create FormFieldSchema (skip 1.4 TypeConfigs for now) ⚡
5. **1.6** - Create FormModuleSchema ⚡
6. **2.1** - Create FormFieldNode ⚡
7. **2.2** - Create HierarchyMetrics ⚡
8. **2.3** - Create FormModuleRuntime ⚡
9. **3.1** - Create ValidationResult types ⚡
10. **3.2** - Create IFormHierarchyService ⚡
11. **4.1** - Implement FormHierarchyService ⚡
12. **9.2** - Create demo app to verify ⚡

This gives you working schema + hierarchy building. Add validation and tests later.

---

## 📊 Progress Tracker

Copy this to track your progress:

```markdown
### Phase 1: Project Structure & Core Schemas
- [ ] 1.1 - Project structure created
- [ ] 1.2 - RelationshipType enum
- [ ] 1.3 - Supporting records (ConditionalRule, FieldOption)
- [ ] 1.4 - Field type configs (FieldTypeConfig, FileUploadConfig, etc.)
- [ ] 1.5 - FormFieldSchema class ⭐ KEY
- [ ] 1.6 - FormModuleSchema class ⭐ KEY
- [ ] 1.7 - FormWorkflowSchema class

### Phase 2: Runtime State Classes
- [ ] 2.1 - FormFieldNode ⭐ KEY
- [ ] 2.2 - HierarchyMetrics record
- [ ] 2.3 - FormModuleRuntime ⭐ KEY

### Phase 3: Service Interfaces
- [ ] 3.1 - ValidationResult types
- [ ] 3.2 - IFormHierarchyService ⭐ KEY
- [ ] 3.3 - IValidationRule interface
- [ ] 3.4 - IFormValidationService
- [ ] 3.5 - IFormModuleRepository

### Phase 4: Service Implementations
- [ ] 4.1 - FormHierarchyService ⭐ KEY
- [ ] 4.2 - Built-in validation rules
- [ ] 4.3 - FormValidationService

### Phase 5: Dependency Injection
- [ ] 5.1 - ServiceCollectionExtensions

### Phase 6: Unit Tests
- [ ] 6.1 - Test project setup
- [ ] 6.2 - Schema tests
- [ ] 6.3 - Hierarchy service tests ⭐ CRITICAL
- [ ] 6.4 - Validation service tests
- [ ] 6.5 - Validation rule tests

### Phase 7: Integration Tests
- [ ] 7.1 - End-to-end integration test ⭐ CRITICAL

### Phase 8: Documentation
- [ ] 8.1 - README.md with examples
- [ ] 8.2 - XML documentation configuration

### Phase 9: Verification
- [ ] 9.1 - Build and test verification ⭐ CRITICAL
- [ ] 9.2 - Demo application

### Phase 10: Final Deliverables
- [ ] 10.1 - NuGet package metadata
- [ ] 10.2 - Implementation summary
```

---

## 🎯 Key Implementation Files

### Must-Have Files (Core Functionality)
```
Src/DynamicForms.Core.V2/
├── Schemas/
│   ├── FormFieldSchema.cs          ⭐ Prompt 1.5 (350 lines)
│   ├── FormModuleSchema.cs         ⭐ Prompt 1.6 (150 lines)
│   ├── FieldSupport.cs             ⭐ Prompt 1.3 (50 lines)
│   └── FieldTypeConfigs.cs            Prompt 1.4 (100 lines)
├── Runtime/
│   ├── FormFieldNode.cs            ⭐ Prompt 2.1 (80 lines)
│   └── FormModuleRuntime.cs        ⭐ Prompt 2.3 (60 lines)
├── Services/
│   ├── IFormHierarchyService.cs    ⭐ Prompt 3.2 (40 lines)
│   └── FormHierarchyService.cs     ⭐ Prompt 4.1 (250 lines)
└── Enums/
    └── RelationshipType.cs         ⭐ Prompt 1.2 (20 lines)
```

### Nice-to-Have Files (Extended Functionality)
```
Src/DynamicForms.Core.V2/
├── Validation/
│   ├── ValidationResults.cs           Prompt 3.1
│   ├── IValidationRule.cs             Prompt 3.3
│   └── BuiltInRules.cs                Prompt 4.2
├── Services/
│   ├── IFormValidationService.cs      Prompt 3.4
│   ├── FormValidationService.cs       Prompt 4.3
│   └── IFormModuleRepository.cs       Prompt 3.5
└── Extensions/
    └── ServiceCollectionExtensions.cs Prompt 5.1
```

---

## ⚡ Execution Strategies

### Strategy 1: Full Implementation (Recommended)
Execute all 32 prompts in order over 2-3 sessions.
- **Session 1**: Phases 1-3 (project structure, schemas, interfaces)
- **Session 2**: Phases 4-6 (implementations, DI, unit tests)
- **Session 3**: Phases 7-10 (integration tests, docs, verification)

### Strategy 2: MVP First
Execute minimal prompts, verify, then extend:
- **Day 1**: Core implementation (12 essential prompts)
- **Day 2**: Add validation (prompts 3.3, 3.4, 4.2, 4.3)
- **Day 3**: Add tests (phase 6 & 7)
- **Day 4**: Documentation & polish (phases 8-10)

### Strategy 3: Test-Driven
Alternate between implementation and testing:
- Prompts 1.1-1.3, then 6.1 (setup)
- Prompts 1.5-1.6, then 6.2 (schemas + tests)
- Prompts 2.1-2.3, then create hierarchy tests
- Prompts 3.2 + 4.1, then 6.3 (hierarchy service + tests)
- Continue pattern...

---

## 🔍 Quality Gates

After each phase, verify these before continuing:

### After Phase 1 (Schemas)
```bash
✓ dotnet build Src/DynamicForms.Core.V2/
✓ No compiler errors or warnings
✓ Can create field: FormFieldSchema.CreateTextField("id", "Label")
✓ Can create module: FormModuleSchema.Create(1, "Title")
✓ JSON serialization works (manual test)
```

### After Phase 2 (Runtime)
```bash
✓ FormFieldNode can be instantiated
✓ Parent/Children properties work
✓ Level and Path computed properties work
```

### After Phase 4 (Services)
```bash
✓ dotnet build (still builds)
✓ FormHierarchyService builds hierarchy correctly
✓ Manual test: create module, build hierarchy, verify parent-child refs
```

### After Phase 6 (Unit Tests)
```bash
✓ dotnet test Tests/DynamicForms.Core.V2.Tests/
✓ All tests pass (100% success rate)
✓ No test failures or skipped tests
```

### After Phase 7 (Integration)
```bash
✓ End-to-end test passes
✓ Verifies complete workflow
```

### Final Verification (Phase 9)
```bash
✓ dotnet build (zero errors, zero warnings)
✓ dotnet test (100% pass rate)
✓ Demo app runs successfully
✓ All acceptance criteria met
```

---

## 🐛 Common Issues & Fixes

### Issue: Circular reference in ModalTableConfig
**Symptom**: ModalTableConfig references FormFieldSchema which doesn't exist yet
**Fix**: Create FormFieldSchema (1.5) before using it in ModalTableConfig (1.4), or use forward reference
**Prompt**: Already handled - 1.4 creates configs with note about forward reference

### Issue: Missing using statements
**Symptom**: Types not found even though they're created
**Fix**: Ensure these usings in each file:
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using DynamicForms.Core.V2.Schemas;
using DynamicForms.Core.V2.Runtime;
using DynamicForms.Core.V2.Enums;
```

### Issue: Tests fail with dependency injection errors
**Symptom**: Services not registered
**Fix**: Use Mock<ILogger<T>> in tests, not real DI container
**Prompt**: 6.3 and 6.4 include proper test setup

### Issue: JSON deserialization fails
**Symptom**: Properties are null after deserialization
**Fix**: Ensure properties use `{ get; init; }` not `{ get; set; }`
**Prompt**: All schema classes use init-only setters

### Issue: Hierarchy not building correctly
**Symptom**: Parent/Child references are null
**Fix**: Ensure FormHierarchyService properly sets both Parent and Children
**Prompt**: 4.1 includes detailed algorithm

---

## 📝 Copy-Paste Verification Commands

### Build Commands
```bash
# Build main project
dotnet build Src/DynamicForms.Core.V2/DynamicForms.Core.V2.csproj

# Build tests
dotnet build Tests/DynamicForms.Core.V2.Tests/

# Build demo
dotnet build Demos/DynamicForms.Demo/

# Build entire solution
dotnet build
```

### Test Commands
```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test file
dotnet test --filter "FullyQualifiedName~FormFieldSchemaTests"

# Run tests with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

### Demo Commands
```bash
# Run demo application
dotnet run --project Demos/DynamicForms.Demo/

# Run with colored output
dotnet run --project Demos/DynamicForms.Demo/ | more
```

### Package Commands
```bash
# Create NuGet package
dotnet pack Src/DynamicForms.Core.V2/ -c Release

# Create package with symbols
dotnet pack Src/DynamicForms.Core.V2/ -c Release --include-symbols --include-source
```

---

## 📈 Success Metrics

Track these metrics as you implement:

```markdown
### Code Metrics
- [ ] Lines of code: ~1,500-2,000 (estimated)
- [ ] Number of classes: ~20
- [ ] Number of interfaces: ~5
- [ ] Number of tests: ~30+
- [ ] Test coverage: >80%

### Quality Metrics
- [ ] Zero compiler errors
- [ ] Zero compiler warnings
- [ ] 100% XML documentation on public APIs
- [ ] 100% test pass rate
- [ ] All acceptance criteria met

### Functionality Metrics
- [ ] JSON serialization works
- [ ] Hierarchy building works
- [ ] Validation works
- [ ] All factory methods work
- [ ] DI registration works
- [ ] Demo app runs
```

---

## 🎓 Learning Path

If you're new to the codebase, study in this order:

1. **Read Design First**
   - ALTERNATIVE_DESIGN_PROPOSAL.md (architecture)
   - ALTERNATIVE_DESIGN_EXAMPLES.md (practical usage)
   - DESIGN_COMPARISON.md (why this design)

2. **Understand Schema**
   - FormFieldSchema (data structure)
   - FormModuleSchema (container)
   - Supporting types (ConditionalRule, FieldOption)

3. **Understand Runtime**
   - FormFieldNode (navigation)
   - FormModuleRuntime (built hierarchy)
   - Separation of schema vs runtime

4. **Understand Services**
   - IFormHierarchyService (builds runtime from schema)
   - IFormValidationService (validates form data)
   - Service-based architecture pattern

5. **Execute Prompts**
   - Start with Phase 1
   - Build incrementally
   - Test as you go

---

## 🔗 Quick Links

### Documentation
- [Main Design Document](./ALTERNATIVE_DESIGN_PROPOSAL.md)
- [Practical Examples](./ALTERNATIVE_DESIGN_EXAMPLES.md)
- [Design Comparison](./DESIGN_COMPARISON.md)
- [Full Implementation Prompts](./IMPLEMENTATION_PROMPTS.md)

### Key Sections in Implementation Prompts
- [Phase 1: Schemas](./IMPLEMENTATION_PROMPTS.md#phase-1-project-structure--core-schemas) - START HERE
- [Phase 4: Service Implementations](./IMPLEMENTATION_PROMPTS.md#phase-4-service-implementations) - MOST COMPLEX
- [Phase 6: Unit Tests](./IMPLEMENTATION_PROMPTS.md#phase-6-unit-tests) - QUALITY ASSURANCE
- [Phase 9: Verification](./IMPLEMENTATION_PROMPTS.md#phase-9-verification-and-quality-checks) - FINAL CHECKS

---

## ✅ Daily Checklist Template

Copy this for each implementation session:

```markdown
## Session [Date] - Phase [X]

### Goals
- [ ] Complete prompts X.X through X.X
- [ ] Verify builds successfully
- [ ] Run relevant tests

### Prompts Executed
- [ ] X.X - [Description] - ✓ Done / ⚠️ Issues / ❌ Blocked

### Issues Encountered
- Issue 1: [description] → Fix: [solution]
- Issue 2: [description] → Fix: [solution]

### Tests Status
- Schema tests: [X/Y passed]
- Service tests: [X/Y passed]
- Integration tests: [X/Y passed]

### Next Session
- Start with prompt: X.X
- Focus area: [description]
- Time estimate: [hours]
```

---

## 🎯 Final Deliverable Checklist

Before considering implementation complete:

### Code
- [ ] All 32 prompts executed successfully
- [ ] Project builds with zero errors
- [ ] Project builds with zero warnings
- [ ] All namespaces correct
- [ ] All using statements optimized

### Tests
- [ ] Unit test project builds
- [ ] All schema tests pass
- [ ] All service tests pass
- [ ] All validation rule tests pass
- [ ] Integration test passes
- [ ] Test coverage >80%

### Documentation
- [ ] README.md created with examples
- [ ] XML documentation on all public APIs
- [ ] Implementation summary created
- [ ] Code examples compile and run

### Functionality
- [ ] Can create schema using factory methods
- [ ] Can serialize/deserialize JSON
- [ ] Can build hierarchy from schema
- [ ] Can validate form data
- [ ] Can navigate hierarchy
- [ ] Demo app runs successfully

### Package
- [ ] NuGet metadata configured
- [ ] Can create package: `dotnet pack`
- [ ] Package includes README
- [ ] Package includes XML documentation

### Comparison to Design
- [ ] Matches FormFieldSchema spec
- [ ] Matches FormModuleSchema spec
- [ ] Matches service interfaces spec
- [ ] Implements all required features
- [ ] Performance meets targets (37% smaller JSON, 44% faster)

---

*Use this as your navigation companion while executing IMPLEMENTATION_PROMPTS.md*

*Estimated total implementation time: 6-8 hours focused work*
