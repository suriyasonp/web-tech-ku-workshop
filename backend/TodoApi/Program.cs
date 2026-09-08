using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueClient", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Todos.Any())
    {
        db.Todos.AddRange(
            new TodoItem
            {
                Title = "Complete the backend lab",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            },
            new TodoItem
            {
                Title = "Connect Vue to the API",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            });
        db.SaveChanges();
    }
}

app.UseCors("VueClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", () => Results.Ok(new
{
    name = "KU Workshop Todo API",
    openApi = "/openapi/v1.json",
    apiReference = "/scalar/v1"
}))
.WithName("ApiInfo")
.WithSummary("Get API information")
.AllowAnonymous();

app.MapPost("/api/auth/login", (LoginDto login, IConfiguration configuration) =>
{
    if (login.Username != "student" || login.Password != "password")
        return Results.Unauthorized();

    var claims = new[] { new Claim(ClaimTypes.Name, login.Username) };
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: configuration["Jwt:Issuer"],
        audience: configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials);

    return Results.Ok(new LoginResponseDto(
        new JwtSecurityTokenHandler().WriteToken(token)));
})
.WithName("Login")
.WithSummary("Sign in with the workshop account")
.Produces<LoginResponseDto>()
.Produces(StatusCodes.Status401Unauthorized)
.AllowAnonymous();

var todos = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .RequireAuthorization();

todos.MapGet("/", async (AppDbContext db) =>
    Results.Ok(await db.Todos
        .OrderBy(x => x.Id)
        .Select(x => new TodoGetDto(x.Id, x.Title, x.IsCompleted))
        .ToListAsync()))
    .WithName("GetTodos")
    .WithSummary("Get all Todos")
    .Produces<List<TodoGetDto>>();

todos.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    return todo is null
        ? Results.NotFound()
        : Results.Ok(new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted));
})
.WithName("GetTodo")
.WithSummary("Get one Todo by ID")
.Produces<TodoGetDto>()
.Produces(StatusCodes.Status404NotFound);

todos.MapPost("/", async (TodoCreateDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { message = "Title is required." });

    var todo = new TodoItem
    {
        Title = dto.Title.Trim(),
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    var result = new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted);
    return Results.Created($"/api/todos/{todo.Id}", result);
})
.WithName("CreateTodo")
.WithSummary("Create a Todo")
.Produces<TodoGetDto>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest);

todos.MapPut("/{id:int}", async (int id, TodoUpdateDto dto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { message = "Title is required." });

    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Title = dto.Title.Trim();
    todo.IsCompleted = dto.IsCompleted;
    await db.SaveChangesAsync();

    return Results.Ok(new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted));
})
.WithName("UpdateTodo")
.WithSummary("Update a Todo")
.Produces<TodoGetDto>()
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status404NotFound);

todos.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteTodo")
.WithSummary("Delete a Todo")
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status404NotFound);

app.Run();
