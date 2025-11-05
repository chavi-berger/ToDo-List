using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models; // צריך עבור תיעוד Swagger

var builder = WebApplication.CreateBuilder(args);

// הזרקת DbContext לשירותים
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))));

// הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // מאפשר לכל דומיין לגשת
              .AllowAnyMethod()   // מאפשר כל סוג בקשה (GET/POST/PUT/DELETE)
              .AllowAnyHeader();  // מאפשר כל כותרת (header)
    });
});

// נוסיף גם Swagger לבדיקה נוחה עם תיעוד
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "API לניהול משימות"
    });
});

var app = builder.Build();

// הפעלת Swagger רק בזמן פיתוח
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
        c.RoutePrefix = string.Empty; // כך ש־Swagger יוצג ישירות ב־כתובת הראשית /
    });
}

// הפעלת CORS
app.UseCors("AllowAll");

// דוגמה לבדיקה מהירה
app.MapGet("/", () => "Todo API is running! 🚀");

// --- Routes עבור טבלת Items ---

// שליפת כל המשימות
app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

// שליפת משימה לפי מזהה
app.MapGet("/items/{id:int}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

// הוספת משימה חדשה
app.MapPost("/items", async (Item newItem, ToDoDbContext db) =>
{
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

// עדכון משימה קיימת
app.MapPut("/items/{id:int}", async (int id, Item updatedItem, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;
    await db.SaveChangesAsync();

    return Results.Ok(item);
});

// מחיקת משימה
app.MapDelete("/items/{id:int}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
