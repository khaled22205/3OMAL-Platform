using src.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDatabase(builder.Configuration)
    .AddIdentity()
    .AddJwtAuthentication(builder.Configuration)
    .AddApplicationServices()
    .AddFluentValidation()
    .AddSwagger()
    .AddCorsPolicy();

builder.Services.AddControllers();

var app = builder.Build();

app.UseApplicationMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
