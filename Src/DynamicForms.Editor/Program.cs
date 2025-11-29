using DynamicForms.Editor;
using DynamicForms.Editor.Data;
using DynamicForms.Editor.Data.Repositories;
using DynamicForms.Editor.Services;
using DynamicForms.Editor.Services.Operations;
using DynamicForms.Editor.Services.State;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// Add Memory Cache
builder.Services.AddMemoryCache();

// Register Repositories
builder.Services.AddScoped<IEditorModuleRepository, EditorModuleRepository>();
builder.Services.AddScoped<IPublishedModuleRepository, PublishedModuleRepository>();
builder.Services.AddScoped<IEditorHistoryRepository, EditorHistoryRepository>();
builder.Services.AddScoped<IEditorConfigurationRepository, EditorConfigurationRepository>();

// Register State Services
builder.Services.AddScoped<EditorStateService>();
builder.Services.AddScoped<UndoRedoService>(sp =>
{
    var maxActions = builder.Configuration.GetValue<int>("EditorSettings:UndoRedoMaxActions", 100);
    return new UndoRedoService(maxActions);
});

// Register Auto-Save Service
builder.Services.AddScoped<AutoSaveService>();

// Register Operation Services
builder.Services.AddScoped<FormBuilderService>();
builder.Services.AddScoped<FieldOperationService>();

// Register Publish Service
builder.Services.AddScoped<PublishService>();

// Add Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
