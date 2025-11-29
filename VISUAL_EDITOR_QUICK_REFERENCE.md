# Visual Form Editor - Quick Reference

Quick navigation, checklists, and commands for implementing the Visual Form Editor using VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md.

---

## Phase Overview

| Phase | Prompts | Time | Key Deliverable |
|-------|---------|------|-----------------|
| 1 | 1.1-1.5 | 2 weeks | Database + Repositories |
| 2 | 2.1-2.8 | 3 weeks | Form Renderer Library |
| 3 | 3.1-3.6 | 2 weeks | Editor State Services |
| 4 | 4.1-4.10 | 4 weeks | Editor UI Components |
| 5 | 5.1-5.4 | 2 weeks | Workflow Editor |
| 6 | 6.1-6.4 | 1 week | Import/Export/Publish |
| 7 | 7.1-7.5 | 2 weeks | Testing |
| 8 | 8.1-8.3 | 1 week | Polish & Documentation |
| **TOTAL** | **45** | **16 weeks** | **Production-Ready Editor** |

---

## Progress Tracker

Copy this to track your progress:

```markdown
### Phase 1: Database & Infrastructure ⏱ 2 weeks
- [ ] 1.1 - SQL Server database schema
- [ ] 1.2 - EF Core DbContext & entities
- [ ] 1.3 - EF Core migrations
- [ ] 1.4 - Repository interfaces
- [ ] 1.5 - Repository implementations ⭐ KEY

### Phase 2: Form Renderer Library ⏱ 3 weeks
- [ ] 2.1 - Renderer project structure
- [ ] 2.2 - FormData & RenderContext models
- [ ] 2.3 - ConditionalLogicEngine ⭐ KEY
- [ ] 2.4 - FieldRendererBase
- [ ] 2.5 - Field renderers (Text, TextArea, DropDown)
- [ ] 2.6 - Field renderers (Date, File, Checkbox)
- [ ] 2.7 - Container renderers (Section, Tab, Panel)
- [ ] 2.8 - DynamicFormRenderer ⭐ KEY

### Phase 3: Editor State Services ⏱ 2 weeks
- [ ] 3.1 - EditorStateService ⭐ KEY
- [ ] 3.2 - UndoRedoService ⭐ KEY
- [ ] 3.3 - AutoSaveService ⭐ KEY
- [ ] 3.4 - FormBuilderService
- [ ] 3.5 - FieldOperationService
- [ ] 3.6 - PublishService ⭐ KEY

### Phase 4: Editor UI Components ⏱ 4 weeks
- [ ] 4.1 - Blazor Server project setup
- [ ] 4.2 - Main layout & navigation
- [ ] 4.3 - Module list page
- [ ] 4.4 - EditorToolbar component
- [ ] 4.5 - EditorStatusBar component
- [ ] 4.6 - FieldListPanel component
- [ ] 4.7 - FieldListItem component (recursive) ⭐ KEY
- [ ] 4.8 - PropertiesPanel component
- [ ] 4.9 - BasicPropertiesSection component
- [ ] 4.10 - Validation & Conditional rule editors ⭐ KEY

### Phase 5: Workflow Editor ⏱ 2 weeks
- [ ] 5.1 - WorkflowEditor page
- [ ] 5.2 - WorkflowModuleList component
- [ ] 5.3 - ConditionalBranchEditor component
- [ ] 5.4 - Workflow preview component

### Phase 6: Import/Export & Publish ⏱ 1 week
- [ ] 6.1 - JSON import functionality
- [ ] 6.2 - JSON export functionality
- [ ] 6.3 - Publish functionality ⭐ KEY
- [ ] 6.4 - Publish history view

### Phase 7: Testing ⏱ 2 weeks
- [ ] 7.1 - Unit tests for services ⭐ CRITICAL
- [ ] 7.2 - Unit tests for renderer
- [ ] 7.3 - Integration tests ⭐ CRITICAL
- [ ] 7.4 - Manual test plan
- [ ] 7.5 - Performance tests

### Phase 8: Polish & Documentation ⏱ 1 week
- [ ] 8.1 - User documentation
- [ ] 8.2 - Developer documentation
- [ ] 8.3 - UI/UX polish ⭐ KEY
```

---

## Must-Have Files (Core Implementation)

```
Solution: DynamicForms.sln

Src/
├── DynamicForms.Core.V2/                [Existing - prerequisite]
│
├── DynamicForms.Editor.Data/            [NEW - Phase 1]
│   ├── ApplicationDbContext.cs          ⭐ Prompt 1.2
│   ├── Entities/
│   │   ├── EditorFormModule.cs
│   │   ├── PublishedFormModule.cs
│   │   └── EditorHistorySnapshot.cs
│   └── Repositories/
│       ├── EditorModuleRepository.cs    ⭐ Prompt 1.5
│       └── PublishedModuleRepository.cs
│
├── DynamicForms.Renderer/               [NEW - Phase 2]
│   ├── Components/
│   │   ├── DynamicFormRenderer.razor    ⭐ Prompt 2.8 (MOST IMPORTANT)
│   │   ├── DynamicFieldRenderer.razor
│   │   ├── Fields/
│   │   │   ├── TextFieldRenderer.razor
│   │   │   ├── DropDownRenderer.razor
│   │   │   └── ... (all field types)
│   │   └── Containers/
│   │       ├── SectionRenderer.razor
│   │       └── TabRenderer.razor
│   ├── Services/
│   │   └── ConditionalLogicEngine.cs    ⭐ Prompt 2.3
│   └── Models/
│       ├── FormData.cs                  ⭐ Prompt 2.2
│       └── RenderContext.cs
│
└── DynamicForms.Editor/                 [NEW - Phase 3-4]
    ├── Pages/
    │   ├── Editor/
    │   │   ├── ModuleEditor.razor       ⭐ Prompt 4.1+ (MAIN EDITOR)
    │   │   └── WorkflowEditor.razor     ⭐ Prompt 5.1
    │   └── ModuleList.razor             ⭐ Prompt 4.3
    ├── Components/
    │   ├── FieldList/
    │   │   ├── FieldListPanel.razor     ⭐ Prompt 4.6
    │   │   └── FieldListItem.razor      ⭐ Prompt 4.7 (recursive!)
    │   ├── Properties/
    │   │   ├── PropertiesPanel.razor    ⭐ Prompt 4.8
    │   │   └── ValidationRuleEditor.razor
    │   └── Shared/
    │       ├── EditorToolbar.razor      ⭐ Prompt 4.4
    │       └── EditorStatusBar.razor
    └── Services/
        ├── State/
        │   ├── EditorStateService.cs    ⭐ Prompt 3.1 (CRITICAL)
        │   ├── UndoRedoService.cs       ⭐ Prompt 3.2 (CRITICAL)
        │   └── AutoSaveService.cs       ⭐ Prompt 3.3 (CRITICAL)
        ├── Operations/
        │   └── FormBuilderService.cs
        └── PublishService.cs            ⭐ Prompt 3.6

Database/
└── CreateEditorDatabase.sql             ⭐ Prompt 1.1

Tests/
├── DynamicForms.Renderer.Tests/         ⭐ Prompt 7.2
├── DynamicForms.Editor.Tests/           ⭐ Prompt 7.1
└── DynamicForms.Integration.Tests/      ⭐ Prompt 7.3
```

---

## Quick Start - Minimal Implementation (4 weeks)

If you want a minimal working editor quickly:

### Week 1: Database + Renderer
1. **Prompts 1.1-1.3** - Database setup
2. **Prompts 2.1-2.3** - Renderer foundation
3. **Prompts 2.5, 2.8** - Basic field renderers + main renderer

### Week 2: Editor State
4. **Prompts 3.1-3.3** - State services (EditorState, UndoRedo, AutoSave)
5. **Prompt 3.4** - FormBuilderService

### Week 3: Editor UI
6. **Prompts 4.1-4.2** - Project setup + layout
7. **Prompts 4.6-4.7** - Field list
8. **Prompts 4.8-4.9** - Properties panel

### Week 4: Testing
9. **Prompt 7.3** - Basic integration test
10. Manual testing and bug fixes

**Result**: Working editor that can create/edit simple forms with text fields and dropdowns.

---

## Quality Gates

### After Phase 1 (Database)
```bash
✓ SQL script runs successfully
✓ dotnet ef database update succeeds
✓ Database has all 6 tables
✓ Can query tables via SQL Server Management Studio
✓ Repository CRUD operations work manually
```

### After Phase 2 (Renderer)
```bash
✓ dotnet build Src/DynamicForms.Renderer/ succeeds
✓ Can create FormData and set/get values
✓ ConditionalLogicEngine evaluates conditions correctly
✓ Can render a form from JSON schema (manual test)
✓ All field types render (create test schemas)
✓ Form submission captures data
```

### After Phase 3 (State Services)
```bash
✓ EditorStateService tracks current module
✓ UndoRedoService undo/redo works (manual test)
✓ AutoSaveService saves periodically
✓ FormBuilderService add/edit/delete field works
✓ PublishService publishes to database
```

### After Phase 4 (Editor UI)
```bash
✓ dotnet run --project Src/DynamicForms.Editor/ works
✓ Can navigate to /editor/module
✓ Can add a field via UI
✓ Can edit field properties
✓ Can save form
✓ Can undo/redo changes
✓ Auto-save indicator shows
```

### After Phase 6 (Complete Features)
```bash
✓ Can import JSON file
✓ Can export JSON file
✓ Can publish form to production
✓ Published form appears in PublishedFormModules table
✓ Workflow editor works (if implemented)
```

### After Phase 7 (Testing)
```bash
✓ dotnet test (all tests passing)
✓ Unit test coverage >80%
✓ Integration tests pass
✓ Performance tests meet targets
✓ Manual test plan executed
```

### Production Ready
```bash
✓ All tests passing
✓ UI polished and consistent
✓ Documentation complete
✓ No compiler warnings
✓ Accessibility tested
✓ Cross-browser tested
```

---

## Common Commands

### Database Commands

```bash
# Create initial migration
dotnet ef migrations add InitialEditorDatabase \
  --project Src/DynamicForms.Editor.Data

# Apply migration to database
dotnet ef database update \
  --project Src/DynamicForms.Editor.Data

# Remove last migration (if needed)
dotnet ef migrations remove \
  --project Src/DynamicForms.Editor.Data

# Generate SQL script from migration
dotnet ef migrations script \
  --project Src/DynamicForms.Editor.Data \
  --output Database/migrations.sql
```

### Build Commands

```bash
# Build entire solution
dotnet build DynamicForms.sln

# Build renderer only
dotnet build Src/DynamicForms.Renderer/

# Build editor only
dotnet build Src/DynamicForms.Editor/

# Build with warnings as errors
dotnet build --warnaserror
```

### Run Commands

```bash
# Run editor app
dotnet run --project Src/DynamicForms.Editor/

# Run with specific environment
dotnet run --project Src/DynamicForms.Editor/ \
  --environment Development

# Run and launch browser
dotnet watch run --project Src/DynamicForms.Editor/
```

### Test Commands

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/DynamicForms.Renderer.Tests/

# Run with code coverage
dotnet test /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName~ConditionalLogicEngineTests"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Package Commands

```bash
# Pack renderer as NuGet package
dotnet pack Src/DynamicForms.Renderer/ \
  -c Release \
  -o artifacts/

# Pack with version
dotnet pack Src/DynamicForms.Renderer/ \
  -c Release \
  /p:Version=1.0.0 \
  -o artifacts/
```

---

## Troubleshooting

### Issue: Migration fails with "Database already exists"

**Solution**:
```bash
# Drop database first
dotnet ef database drop --project Src/DynamicForms.Editor.Data

# Then update
dotnet ef database update --project Src/DynamicForms.Editor.Data
```

### Issue: Renderer doesn't update when schema changes

**Cause**: Blazor caching or not detecting parameter change

**Solution**:
```csharp
// In DynamicFormRenderer.razor, add:
protected override bool ShouldRender()
{
    return true;  // Force re-render
}

// Or use @key directive:
<DynamicFormRenderer @key="@Schema.ModuleId" Schema="@Schema" />
```

### Issue: Undo/Redo not working

**Check**:
1. Is `UndoRedoService.PushSnapshot()` being called?
2. Is snapshot JSON being serialized correctly?
3. Are stacks being cleared when they shouldn't be?

**Debug**:
```csharp
// Add logging to UndoRedoService
_logger.LogInformation("Undo stack count: {Count}", _undoStack.Count);
_logger.LogInformation("Pushing snapshot: {Action}", actionDescription);
```

### Issue: Auto-save not triggering

**Check**:
1. Is `AutoSaveService.Start()` being called?
2. Is timer interval configured correctly?
3. Is `IsDirty` flag set to true when editing?

**Debug**:
```csharp
// In AutoSaveService
_logger.LogInformation("Auto-save check: IsDirty={IsDirty}", _stateService.IsDirty);
```

### Issue: Conditional logic not hiding fields

**Check**:
1. Is `ConditionalLogicEngine.EvaluateCondition()` returning correct result?
2. Is `FormData` being updated when field values change?
3. Is `StateHasChanged()` being called after evaluation?

**Debug**:
```csharp
// Add logging in ConditionalLogicEngine
_logger.LogInformation("Evaluating: {FieldId} {Operator} {Value} = {Result}",
    rule.TargetFieldId, rule.Operator, rule.Value, result);
```

### Issue: Published form not appearing in production app

**Check**:
1. Was publish successful? Check `PublishedFormModules` table
2. Is `IsActive = 1`?
3. Is production app querying correct database?
4. Is cache cleared if using caching?

**SQL Check**:
```sql
SELECT * FROM PublishedFormModules
WHERE ModuleId = 123
ORDER BY Version DESC;
```

---

## Performance Optimization Checklist

### Renderer Performance
- [ ] Use `@key` directive for list rendering stability
- [ ] Implement virtualization for >50 fields (`<Virtualize>`)
- [ ] Debounce field validation (300ms delay)
- [ ] Use `ShouldRender()` to prevent unnecessary re-renders
- [ ] Lazy-load container children (tabs, sections)

### Database Performance
- [ ] Indexes on frequently queried columns (ModuleId, Status, Version)
- [ ] Use `AsNoTracking()` for read-only queries
- [ ] Implement caching for published forms (MemoryCache)
- [ ] Cleanup old history snapshots (>90 days)
- [ ] Use compiled queries for hot paths

### Blazor Server Performance
- [ ] Configure SignalR `MaximumReceiveMessageSize` (1MB)
- [ ] Set `DisconnectedCircuitRetentionPeriod` (3 minutes)
- [ ] Use `@preservewhitespace false` to reduce payload
- [ ] Minimize JavaScript interop calls
- [ ] Use streaming rendering where appropriate

---

## Configuration Checklist

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DynamicFormsEditor;..."
  },
  "EditorSettings": {
    "AutoSave": {
      "Enabled": true,
      "IntervalSeconds": 30
    },
    "UndoRedo": {
      "MaxActions": 100
    },
    "History": {
      "RetentionDays": 90
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DynamicForms.Editor": "Debug"
    }
  }
}
```

### Program.cs Service Registration

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories
builder.Services.AddScoped<IEditorModuleRepository, EditorModuleRepository>();
builder.Services.AddScoped<IPublishedModuleRepository, PublishedModuleRepository>();

// State Services (Scoped - per user session)
builder.Services.AddScoped<EditorStateService>();
builder.Services.AddScoped<UndoRedoService>();
builder.Services.AddScoped<AutoSaveService>();

// Operation Services
builder.Services.AddScoped<FormBuilderService>();
builder.Services.AddScoped<PublishService>();

// Core V2 Services
builder.Services.AddScoped<IFormHierarchyService, FormHierarchyService>();
builder.Services.AddScoped<IFormValidationService, FormValidationService>();

// Renderer Services
builder.Services.AddScoped<ConditionalLogicEngine>();

// Caching
builder.Services.AddMemoryCache();

// Blazor
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
```

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+S` | Save current form |
| `Ctrl+Z` | Undo last change |
| `Ctrl+Y` | Redo last undone change |
| `Ctrl+E` | Export JSON |
| `Escape` | Cancel edit / Close dialog |
| `Delete` | Delete selected field |
| `Ctrl+C` | Clone selected field |
| `Alt+↑` | Move field up |
| `Alt+↓` | Move field down |

---

## Success Metrics

### Technical Metrics
- **Render Time**: < 500ms for 100 fields
- **Auto-Save Latency**: < 1 second
- **Undo/Redo Response**: < 100ms
- **Build Time**: < 30 seconds
- **Test Coverage**: > 80%
- **Bundle Size**: < 5MB (compressed)

### User Experience Metrics
- **Time to Create Simple Form**: < 5 minutes
- **Time to Create Complex Form**: < 20 minutes
- **Learning Curve**: User productive within 30 minutes
- **Error Rate**: < 5% user actions result in errors

---

## Next Steps Decision Tree

```
Are you starting fresh?
├─ Yes → Start with Prompt 1.1 (Database schema)
└─ No  → Where are you?
    ├─ Database done → Prompt 2.1 (Renderer)
    ├─ Renderer done → Prompt 3.1 (State services)
    ├─ Services done → Prompt 4.1 (Editor UI)
    ├─ Editor UI done → Prompt 5.1 (Workflow) or 6.1 (Import/Export)
    ├─ Features done → Prompt 7.1 (Testing)
    └─ Tests done → Prompt 8.1 (Documentation & Polish)
```

---

## Quick Links

### Documentation
- [Main Design Proposal](./VISUAL_EDITOR_DESIGN_PROPOSAL.md)
- [Component Design](./VISUAL_EDITOR_COMPONENTS.md)
- [Implementation Prompts](./VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md)
- [Start Here Guide](./VISUAL_EDITOR_START_HERE.md)

### Key Sections in Implementation Prompts
- [Phase 1: Database](./VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md#phase-1-database--infrastructure)
- [Phase 2: Renderer](./VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md#phase-2-form-renderer-library)
- [Phase 3: State Services](./VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md#phase-3-editor-state-services)
- [Phase 4: Editor UI](./VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md#phase-4-editor-ui-components)

---

*Use this as your companion while executing VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md*

*Estimated total: 16 weeks | Minimal: 4 weeks*

**Document Version**: 1.0
**Last Updated**: January 2025
