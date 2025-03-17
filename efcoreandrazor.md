To load a SQL Server table into a Razor Page with full CRUD (Create, Read, Update, Delete) operations, follow these steps:

1. Set Up Your Database

Ensure you have a SQL Server database with a table, e.g., Products:

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Price DECIMAL(18,2),
    Quantity INT
);

2. Create an Entity Model

Use Entity Framework Core to interact with SQL Server. First, install the necessary NuGet packages:

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design

Now, define a model class:

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

3. Set Up the Database Context

Create a ApplicationDbContext class to interact with the database:

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}

In appsettings.json, configure the connection string:

"ConnectionStrings": {
    "DefaultConnection": "Server=your_server;Database=your_db;User Id=your_user;Password=your_password;"
}

In Program.cs, add:

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

4. Create the Razor Page

Pages/Products/Index.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Product> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _context.Products.ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}

Pages/Products/Index.cshtml

@page
@model IndexModel
@{
    ViewData["Title"] = "Products";
}

<h2>Products</h2>
<a asp-page="Create" class="btn btn-primary">Add New Product</a>

<table class="table table-bordered">
    <thead>
        <tr>
            <th>Name</th>
            <th>Price</th>
            <th>Quantity</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var product in Model.Products)
        {
            <tr>
                <td>@product.Name</td>
                <td>@product.Price</td>
                <td>@product.Quantity</td>
                <td>
                    <a asp-page="Edit" asp-route-id="@product.Id" class="btn btn-warning">Edit</a>
                    <form method="post" asp-page-handler="Delete" asp-route-id="@product.Id" style="display:inline;">
                        <button type="submit" class="btn btn-danger" onclick="return confirm('Are you sure?')">Delete</button>
                    </form>
                </td>
            </tr>
        }
    </tbody>
</table>

5. Add Create & Edit Pages

Pages/Products/Create.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _context.Products.Add(Product);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}

Pages/Products/Create.cshtml

@page
@model CreateModel
@{
    ViewData["Title"] = "Create Product";
}

<h2>Create Product</h2>
<form method="post">
    <div class="form-group">
        <label>Name</label>
        <input asp-for="Product.Name" class="form-control" />
    </div>
    <div class="form-group">
        <label>Price</label>
        <input asp-for="Product.Price" class="form-control" />
    </div>
    <div class="form-group">
        <label>Quantity</label>
        <input asp-for="Product.Quantity" class="form-control" />
    </div>
    <button type="submit" class="btn btn-success">Create</button>
    <a asp-page="Index" class="btn btn-secondary">Back</a>
</form>

Pages/Products/Edit.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Product Product { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Product = await _context.Products.FindAsync(id);
        if (Product == null)
        {
            return NotFound();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _context.Attach(Product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}

Pages/Products/Edit.cshtml

@page
@model EditModel
@{
    ViewData["Title"] = "Edit Product";
}

<h2>Edit Product</h2>
<form method="post">
    <input type="hidden" asp-for="Product.Id" />
    <div class="form-group">
        <label>Name</label>
        <input asp-for="Product.Name" class="form-control" />
    </div>
    <div class="form-group">
        <label>Price</label>
        <input asp-for="Product.Price" class="form-control" />
    </div>
    <div class="form-group">
        <label>Quantity</label>
        <input asp-for="Product.Quantity" class="form-control" />
    </div>
    <button type="submit" class="btn btn-success">Save</button>
    <a asp-page="Index" class="btn btn-secondary">Back</a>
</form>


---

6. Run Migrations

dotnet ef migrations add InitialCreate
dotnet ef database update


---

7. Run the Application

Start the app and navigate to /Products to view and manage your data.

This provides a full CRUD Razor Pages implementation for managing Products using SQL Server. Let me know if you need modifications!

