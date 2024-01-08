using OnlineJson.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
    options.AddPolicy("CorsPolicy", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseCors("CorsPolicy");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<EditorHub>("/editorHub");

app.Run();