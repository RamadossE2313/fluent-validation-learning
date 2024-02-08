using FluentValidation;

var product = new Product
{
    Id = 1,
    Name = null, // Invalid, as Name is required
    Categories = new List<Category> {
                new Category
                {
                    Id = 1,
                    Name = "TestCategory",
                    Description = "TestDescription",
                    Products = new List<ProductInCategory>
                    {
                        new ProductInCategory { Id = 1, Name = null }, // Invalid, as Name is required
                        new ProductInCategory { Id = 2, Name = "TestProduct" }
                    }
                }
            }
};

// Invoke the validator
var validator = new ProductValidator();
var validationResult = validator.Validate(product);

// Print validation result
if (!validationResult.IsValid)
{
    Console.WriteLine("Validation failed. Errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
else
{
    Console.WriteLine("Validation successful!");
}

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(product => product.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(product => product.Id).NotEmpty().WithMessage("Id is required");

        RuleForEach(product => product.Categories)
            .Cascade(CascadeMode.Stop)
            .Custom((category, context) =>
            {
                var validator = new CategoryValidator();
                var result = validator.Validate(category);
                foreach (var failure in result.Errors)
                {
                    // Append category ID to the error message
                    failure.ErrorMessage = $"Category ID {category.Id}: {failure.ErrorMessage}";
                    // to identify the category value
                    failure.CustomState = category.Id;
                    context.AddFailure(failure);
                }
            });
    }
}

public class CategoryValidator : AbstractValidator<Category>
{
    public CategoryValidator()
    {
        RuleFor(category => category.Name).NotEmpty().WithMessage("Category Name is required");
        RuleFor(category => category.Description).NotEmpty().WithMessage("Category Description is required");
        RuleForEach(category => category.Products).NotNull().SetValidator(new ProductInCategoryValidator());
    }
}

public class ProductInCategoryValidator : AbstractValidator<ProductInCategory>
{
    public ProductInCategoryValidator()
    {
        RuleFor(product => product.Id).NotEmpty().WithMessage("Product ID is required");
        RuleFor(product => product.Name).NotEmpty().WithMessage("Product Name is required");
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Category> Categories { get; set; } = new List<Category>();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ProductInCategory> Products { get; set; } = new List<ProductInCategory>();
}

public class ProductInCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
}
