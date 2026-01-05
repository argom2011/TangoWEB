using Microsoft.EntityFrameworkCore;
using TangoWEB.Data;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Registrar MVC con vistas
builder.Services.AddControllersWithViews();

// 2️⃣ Registrar AppDbContext para inyección de dependencias
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TangoDB")));

// 3️⃣ Habilitar Swagger (opcional, útil para probar APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4️⃣ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    // Solo en desarrollo, habilita Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // para assets estáticos
app.UseRouting();
app.UseAuthorization();

// 5️⃣ Mapear rutas de MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 6️⃣ Mapear rutas de API
app.MapControllers();

app.Run();
