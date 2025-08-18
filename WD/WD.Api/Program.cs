var builder = WebApplication.CreateBuilder(args);

// Agregar soporte para controladores (MVC sin vistas)
builder.Services.AddControllers();

// Agregar HttpClient para hacer peticiones HTTP
builder.Services.AddHttpClient();

// Agregar soporte para documentacion con Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar controladores otra vez (linea repetida pero no da error)
builder.Services.AddControllers();

// Definir nombre de la politica de CORS
const string AllowMvc = "AllowMvc";

// Configuracion de CORS para permitir llamadas desde el front-end
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(AllowMvc, p =>
        p.WithOrigins("https://localhost:7153") // origen permitido
         .AllowAnyHeader() // permitir cualquier cabecera
         .AllowAnyMethod()); // permitir cualquier metodo HTTP
});

var app = builder.Build();

// Si el ambiente es de desarrollo se habilita Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redireccion automatica a HTTPS
app.UseHttpsRedirection();

// Habilitar CORS con la politica definida
app.UseCors(AllowMvc);

// Mapear los controladores (activar rutas de la API)
app.MapControllers();

// Iniciar la aplicacion
app.Run();
