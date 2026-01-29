If you're limited to using .NET Framework without Entity Framework Core, you'll need to use ADO.NET for database interactions. Below is a solution using SqlConnection, SqlCommand, and SqlDataReader to perform CRUD operations manually.


---

1. Set Up Your SQL Server Database

Ensure you have a Products table:

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100),
    Price DECIMAL(18,2),
    Quantity INT
);


---

2. Configure the Database Connection

In Web.config, add the connection string:

<connectionStrings>
    <add name="DefaultConnection" connectionString="Server=your_server;Database=your_db;User Id=your_user;Password=your_password;" providerName="System.Data.SqlClient" />
</connectionStrings>


---

3. Create the Data Access Layer

Define a helper class to interact with the database using ADO.NET.

Data/ProductRepository.cs

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository()
    {
        _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
    }

    public List<Product> GetAllProducts()
    {
        List<Product> products = new List<Product>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Products", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"]),
                        Quantity = Convert.ToInt32(reader["Quantity"])
                    });
                }
            }
        }
        return products;
    }

    public void AddProduct(Product product)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO Products (Name, Price, Quantity) VALUES (@Name, @Price, @Quantity)", conn);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
            cmd.ExecuteNonQuery();
        }
    }

    public Product GetProductById(int id)
    {
        Product product = null;
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Products WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    product = new Product
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Name = reader["Name"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"]),
                        Quantity = Convert.ToInt32(reader["Quantity"])
                    };
                }
            }
        }
        return product;
    }

    public void UpdateProduct(Product product)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("UPDATE Products SET Name = @Name, Price = @Price, Quantity = @Quantity WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", product.Id);
            cmd.Parameters.AddWithValue("@Name", product.Name);
            cmd.Parameters.AddWithValue("@Price", product.Price);
            cmd.Parameters.AddWithValue("@Quantity", product.Quantity);
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteProduct(int id)
    {
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.ExecuteNonQuery();
        }
    }
}


---

4. Create the Razor Pages

Pages/Products/Index.cshtml.cs

using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

public class IndexModel : PageModel
{
    private readonly ProductRepository _repository;

    public IndexModel()
    {
        _repository = new ProductRepository();
    }

    public List<Product> Products { get; set; } = new();

    public void OnGet()
    {
        Products = _repository.GetAllProducts();
    }

    public IActionResult OnPostDelete(int id)
    {
        _repository.DeleteProduct(id);
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


---

5. Implement Create and Edit Pages

Pages/Products/Create.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class CreateModel : PageModel
{
    private readonly ProductRepository _repository;

    public CreateModel()
    {
        _repository = new ProductRepository();
    }

    [BindProperty]
    public Product Product { get; set; }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        _repository.AddProduct(Product);
        return RedirectToPage("Index");
    }
}

Pages/Products/Create.cshtml

@page
@model CreateModel
<h2>Create Product</h2>
<form method="post">
    <label>Name</label>
    <input asp-for="Product.Name" class="form-control" />

    <label>Price</label>
    <input asp-for="Product.Price" class="form-control" />

    <label>Quantity</label>
    <input asp-for="Product.Quantity" class="form-control" />

    <button type="submit" class="btn btn-success">Create</button>
    <a asp-page="Index" class="btn btn-secondary">Back</a>
</form>

Pages/Products/Edit.cshtml.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class EditModel : PageModel
{
    private readonly ProductRepository _repository;

    public EditModel()
    {
        _repository = new ProductRepository();
    }

    [BindProperty]
    public Product Product { get; set; }

    public void OnGet(int id)
    {
        Product = _repository.GetProductById(id);
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid) return Page();

        _repository.UpdateProduct(Product);
        return RedirectToPage("Index");
    }
}

Pages/Products/Edit.cshtml

@page
@model EditModel
<h2>Edit Product</h2>
<form method="post">
    <input type="hidden" asp-for="Product.Id" />
    
    <label>Name</label>
    <input asp-for="Product.Name" class="form-control" />

    <label>Price</label>
    <input asp-for="Product.Price" class="form-control" />

    <label>Quantity</label>
    <input asp-for="Product.Quantity" class="form-control" />

    <button type="submit" class="btn btn-success">Save</button>
    <a asp-page="Index" class="btn btn-secondary">Back</a>
</form>


---

Conclusion

This method allows full CRUD operations in Razor Pages without EF Core, using ADO.NET for direct SQL interaction. Let me know if you need any modifications!

