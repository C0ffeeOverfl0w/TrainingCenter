using System.Text.Json.Serialization;
using TrainingCenter.Api.Services;
using TrainingCenter.Api.Services.Discounts;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters
    .Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddProblemDetails();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IDiscountRule, GraduateDiscountRule>();
builder.Services.AddSingleton<IDiscountRule, HotGroupDiscountRule>();
builder.Services.AddScoped<CartPricingService>();
builder.Services.AddScoped<IPrimaryEmailSender, UnavailablePrimaryEmailSender>();
builder.Services.AddScoped<IBackupEmailSender, SimulatedBackupEmailSender>();
builder.Services.AddScoped<BackupLeadService>();
builder.Services.AddOptions<EmailOptions>()
    .BindConfiguration("Email")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TrainingCenter.Api.xml")));

var app = builder.Build();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();
