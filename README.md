# cesizen-api

# Result Pattern - Guide d'utilisation

## 📋 Table des matières

- [Introduction](#introduction)
- [Installation](#installation)
- [Concepts de base](#concepts-de-base)
- [Cas d'utilisation](#cas-dutilisation)
    - [1. Opérations simples](#1-opérations-simples)
    - [2. Validation avec erreurs multiples](#2-validation-avec-erreurs-multiples)
    - [3. Chaînage d'opérations](#3-chaînage-dopérations)
    - [4. Gestion dans les contrôleurs](#4-gestion-dans-les-contrôleurs)
    - [5. Combinaison de résultats](#5-combinaison-de-résultats)
    - [6. Opérations asynchrones](#6-opérations-asynchrones)
- [Bonnes pratiques](#bonnes-pratiques)
- [Anti-patterns à éviter](#anti-patterns-à-éviter)

---

## Introduction

Le **Result Pattern** permet de gérer les succès et les échecs d'opérations de manière explicite et type-safe, sans utiliser les exceptions pour le contrôle de flux.

### Avantages

✅ **Explicite** - Le type de retour indique clairement qu'une opération peut échouer  
✅ **Type-safe** - Impossible d'accéder à une valeur d'un résultat en échec  
✅ **Pas d'exceptions** - Meilleure performance et code plus prévisible  
✅ **Testable** - Facile à tester et à mocker  
✅ **Chainable** - Supporte la programmation fonctionnelle

---

## Installation

```csharp
// Copiez les fichiers dans votre projet
api.CZ.Common/
  ├── IResult.cs
  └── Result.cs
```

---

## Concepts de base

### Result vs Result<T>

```csharp
// Result - Pour les opérations sans valeur de retour (void)
public async Task<Result> DeleteProductAsync(int id)
{
    var deleted = await _repository.RemoveByIdAsync(id);
    return deleted 
        ? Result.Success() 
        : Result.Failure("Product not found");
}

// Result<T> - Pour les opérations avec valeur de retour
public async Task<Result<Product>> GetProductAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    return product != null
        ? Result.Success(product)
        : Result.Failure<Product>("Product not found");
}
```

### Vérification du résultat

```csharp
var result = await GetProductAsync(1);

// Méthode 1 : IsSuccess / IsFailure
if (result.IsSuccess)
{
    var product = result.Value;
    Console.WriteLine($"Found: {product.Name}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}

// Méthode 2 : Match (pattern matching)
result.Match(
    onSuccess: product => Console.WriteLine($"Found: {product.Name}"),
    onFailure: error => Console.WriteLine($"Error: {error}")
);

// Méthode 3 : GetValueOrDefault
var product = result.GetValueOrDefault(new Product { Name = "Default" });
```

---

## Cas d'utilisation

### 1. Opérations simples

#### ✅ Exemple : Création d'entité

```csharp
public class ProductService
{
    private readonly IBaseRepository<Product> _repository;

    public async Task<Result<Product>> CreateAsync(CreateProductDto dto)
    {
        // Validation simple
        if (string.IsNullOrEmpty(dto.Name))
            return Result.Failure<Product>("Name is required");

        if (dto.Price <= 0)
            return Result.Failure<Product>("Price must be positive");

        // Vérification unicité
        var exists = await _repository.AnyAsync(p => p.Name == dto.Name);
        if (exists)
            return Result.Failure<Product>("Product already exists");

        // Création
        var product = new Product { Name = dto.Name, Price = dto.Price };
        await _repository.AddAsync(product);
        
        return Result.Success(product);
    }
}
```

#### ✅ Exemple : Récupération d'entité

```csharp
public async Task<Result<Product>> GetByIdAsync(int id)
{
    if (id <= 0)
        return Result.Failure<Product>("Invalid ID");

    var product = await _repository.GetByIdAsync(id);
    
    return product != null
        ? Result.Success(product)
        : Result.Failure<Product>($"Product {id} not found");
}
```

#### ✅ Exemple : Suppression d'entité

```csharp
public async Task<Result> DeleteAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    
    if (product == null)
        return Result.Failure("Product not found");

    if (product.HasOrders)
        return Result.Failure("Cannot delete product with existing orders");

    await _repository.RemoveAsync(product);
    return Result.Success();
}
```

---

### 2. Validation avec erreurs multiples

#### ✅ Exemple : Validation complète d'un objet

```csharp
public Result<Product> ValidateProduct(Product product)
{
    var errors = new List<string>();

    if (string.IsNullOrEmpty(product.Name))
        errors.Add("Name is required");

    if (product.Name?.Length > 100)
        errors.Add("Name must be less than 100 characters");

    if (product.Price <= 0)
        errors.Add("Price must be positive");

    if (product.Price > 1000000)
        errors.Add("Price exceeds maximum allowed");

    if (product.Stock < 0)
        errors.Add("Stock cannot be negative");

    return errors.Any()
        ? Result.Failure<Product>(errors)
        : Result.Success(product);
}
```

#### ✅ Utilisation dans le contrôleur

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    var product = _mapper.Map<Product>(dto);
    var validationResult = _service.ValidateProduct(product);

    if (validationResult.IsFailure)
    {
        // Retourne toutes les erreurs
        return BadRequest(new 
        { 
            errors = validationResult.Errors 
        });
    }

    var result = await _service.CreateAsync(dto);
    return result.IsSuccess 
        ? Ok(result.Value) 
        : BadRequest(new { error = result.Error });
}
```

---

### 3. Chaînage d'opérations

#### ✅ Map - Transformer la valeur

```csharp
public async Task<Result<ProductDto>> GetProductDtoAsync(int id)
{
    return (await GetByIdAsync(id))
        .Map(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            DisplayPrice = $"${product.Price:F2}"
        });
}
```

#### ✅ Bind - Chaîner des opérations qui retournent Result

```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderDto dto)
{
    return (await GetProductAsync(dto.ProductId))
        .Bind(product => ValidateStock(product, dto.Quantity))
        .Bind(product => CreateOrder(product, dto));
}

private Result<Product> ValidateStock(Product product, int quantity)
{
    return product.Stock >= quantity
        ? Result.Success(product)
        : Result.Failure<Product>("Insufficient stock");
}

private async Task<Result<Order>> CreateOrder(Product product, CreateOrderDto dto)
{
    var order = new Order
    {
        ProductId = product.Id,
        Quantity = dto.Quantity,
        TotalPrice = product.Price * dto.Quantity
    };

    await _orderRepository.AddAsync(order);
    return Result.Success(order);
}
```

#### ✅ Tap - Exécuter des effets de bord

```csharp
public async Task<Result<Product>> CreateWithLoggingAsync(CreateProductDto dto)
{
    return (await CreateAsync(dto))
        .Tap(product => _logger.LogInformation($"Product created: {product.Name}"))
        .Tap(product => _cache.Set($"product_{product.Id}", product));
}

// Version async
public async Task<Result<Product>> CreateWithNotificationAsync(CreateProductDto dto)
{
    return await (await CreateAsync(dto))
        .TapAsync(async product => 
        {
            await _emailService.SendNewProductNotificationAsync(product);
            await _eventBus.PublishAsync(new ProductCreatedEvent(product));
        });
}
```

---

### 4. Gestion dans les contrôleurs

#### ✅ Méthode simple avec Match

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _service.GetByIdAsync(id);

    return result.Match(
        onSuccess: product => Ok(product),
        onFailure: error => NotFound(new { error })
    );
}
```

#### ✅ Méthode avec vérification manuelle

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    var result = await _service.CreateAsync(dto);

    if (result.IsFailure)
        return BadRequest(new { error = result.Error });

    return CreatedAtAction(
        nameof(GetById), 
        new { id = result.Value.Id }, 
        result.Value
    );
}
```

#### ✅ Méthode avec erreurs multiples

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> Update(int id, UpdateProductDto dto)
{
    var result = await _service.UpdateAsync(id, dto);

    if (result.IsFailure)
    {
        // Si plusieurs erreurs, les retourner toutes
        if (result.Errors.Count > 1)
        {
            return BadRequest(new 
            { 
                message = "Validation failed",
                errors = result.Errors 
            });
        }

        // Sinon, retourner l'erreur unique
        return BadRequest(new { error = result.Error });
    }

    return Ok(result.Value);
}
```

#### ✅ Extension method pour simplifier

```csharp
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? new OkObjectResult(result.Value)
            : new BadRequestObjectResult(new { error = result.Error });
    }

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result, 
        string actionName, 
        object routeValues)
    {
        return result.IsSuccess
            ? new CreatedAtActionResult(actionName, null, routeValues, result.Value)
            : new BadRequestObjectResult(new { error = result.Error });
    }

    public static IActionResult ToNoContentResult(this Result result)
    {
        return result.IsSuccess
            ? new NoContentResult()
            : new BadRequestObjectResult(new { error = result.Error });
    }
}

// Utilisation
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    var result = await _service.GetByIdAsync(id);
    return result.ToActionResult();
}
```

---

### 5. Combinaison de résultats

#### ✅ Combine - Toutes les opérations doivent réussir

```csharp
public async Task<Result> ProcessBatchAsync(List<CreateProductDto> dtos)
{
    var results = new List<Result>();

    foreach (var dto in dtos)
    {
        var result = await CreateAsync(dto);
        results.Add(result);
    }

    // Si au moins une opération échoue, retourne toutes les erreurs
    var combinedResult = Result.Combine(results.ToArray());

    return combinedResult.IsSuccess
        ? Result.Success()
        : Result.Failure(combinedResult.Errors);
}
```

#### ✅ FirstFailureOrSuccess - S'arrête au premier échec

```csharp
public async Task<Result> ValidateAndCreateAsync(CreateProductDto dto)
{
    var nameValidation = ValidateName(dto.Name);
    var priceValidation = ValidatePrice(dto.Price);
    var stockValidation = ValidateStock(dto.Stock);

    // S'arrête à la première erreur
    var validationResult = Result.FirstFailureOrSuccess(
        nameValidation, 
        priceValidation, 
        stockValidation
    );

    if (validationResult.IsFailure)
        return validationResult;

    return await CreateAsync(dto);
}
```

#### ✅ Opérations séquentielles dépendantes

```csharp
public async Task<Result<Invoice>> CreateInvoiceAsync(CreateInvoiceDto dto)
{
    // Étape 1 : Valider le client
    var customerResult = await ValidateCustomerAsync(dto.CustomerId);
    if (customerResult.IsFailure)
        return Result.Failure<Invoice>(customerResult.Error);

    // Étape 2 : Valider les produits
    var productsResult = await ValidateProductsAsync(dto.Items);
    if (productsResult.IsFailure)
        return Result.Failure<Invoice>(productsResult.Error);

    // Étape 3 : Créer la facture
    var invoice = new Invoice
    {
        CustomerId = dto.CustomerId,
        Items = dto.Items,
        Total = dto.Items.Sum(i => i.Price * i.Quantity)
    };

    await _invoiceRepository.AddAsync(invoice);
    return Result.Success(invoice);
}
```

---

### 6. Opérations asynchrones

#### ✅ Chaînage async avec TapAsync

```csharp
public async Task<Result<Order>> CompleteOrderAsync(int orderId)
{
    return await GetOrderAsync(orderId)
        .TapAsync(async order => 
        {
            await _emailService.SendConfirmationAsync(order);
        })
        .TapAsync(async order => 
        {
            await _inventoryService.UpdateStockAsync(order);
        })
        .TapAsync(async order => 
        {
            await _analyticsService.TrackOrderAsync(order);
        });
}
```

#### ✅ Pattern Repository avec Result

```csharp
public class ProductRepository : BaseRepository<Product>
{
    public async Task<Result<Product>> GetActiveProductAsync(int id)
    {
        var product = await FirstOrDefaultAsync(p => 
            p.Id == id && p.IsActive && !p.IsDeleted);

        return product != null
            ? Result.Success(product)
            : Result.Failure<Product>("Active product not found");
    }

    public async Task<Result<IEnumerable<Product>>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await FindAsync(p => 
            p.CategoryId == categoryId && p.IsActive);

        return products.Any()
            ? Result.Success(products)
            : Result.Failure<IEnumerable<Product>>("No products found in this category");
    }
}
```

---

## Bonnes pratiques

### ✅ Retourner Result au lieu de null

```csharp
// ❌ Mauvais
public async Task<Product?> GetProductAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ✅ Bon
public async Task<Result<Product>> GetProductAsync(int id)
{
    var product = await _repository.GetByIdAsync(id);
    return product != null
        ? Result.Success(product)
        : Result.Failure<Product>("Product not found");
}
```

### ✅ Utiliser des messages d'erreur clairs

```csharp
// ❌ Mauvais
return Result.Failure<Product>("Error");

// ✅ Bon
return Result.Failure<Product>($"Product with ID {id} was not found");
```

### ✅ Grouper les validations similaires

```csharp
// ✅ Bon
public Result<Product> ValidateProduct(Product product)
{
    var errors = new List<string>();

    // Validations du nom
    if (string.IsNullOrEmpty(product.Name))
        errors.Add("Name is required");
    else if (product.Name.Length > 100)
        errors.Add("Name is too long");

    // Validations du prix
    if (product.Price <= 0)
        errors.Add("Price must be positive");
    else if (product.Price > 1000000)
        errors.Add("Price is too high");

    return errors.Any()
        ? Result.Failure<Product>(errors)
        : Result.Success(product);
}
```

### ✅ Utiliser Match pour le pattern matching

```csharp
// ✅ Bon - Plus concis et fonctionnel
public async Task<IActionResult> GetProduct(int id)
{
    var result = await _service.GetByIdAsync(id);
    
    return result.Match(
        onSuccess: product => Ok(product),
        onFailure: error => NotFound(new { error })
    );
}
```

---

## Anti-patterns à éviter

### ❌ Accéder à Value sans vérifier IsSuccess

```csharp
// ❌ DANGEREUX - Lance une exception si IsFailure
var result = await GetProductAsync(id);
var product = result.Value; // Exception potentielle!

// ✅ Correct
if (result.IsSuccess)
{
    var product = result.Value;
    // Utiliser product
}

// ✅ Ou utiliser GetValueOrDefault
var product = result.GetValueOrDefault();
```

### ❌ Utiliser Result pour des exceptions système

```csharp
// ❌ Mauvais - Les exceptions système doivent rester des exceptions
public Result<Product> GetProduct(int id)
{
    try
    {
        var product = _repository.GetById(id);
        return Result.Success(product);
    }
    catch (SqlException ex)
    {
        // Ne pas catcher les exceptions système!
        return Result.Failure<Product>(ex.Message);
    }
}

// ✅ Bon - Laisser les exceptions système se propager
public async Task<Result<Product>> GetProductAsync(int id)
{
    // Les exceptions DB/Network doivent se propager
    var product = await _repository.GetByIdAsync(id);
    
    // Result pour les cas métier
    return product != null
        ? Result.Success(product)
        : Result.Failure<Product>("Product not found");
}
```

### ❌ Créer des Result dans les contrôleurs

```csharp
// ❌ Mauvais - La logique métier est dans le contrôleur
[HttpPost]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    if (string.IsNullOrEmpty(dto.Name))
        return BadRequest("Name required");
        
    var product = await _repository.AddAsync(new Product { Name = dto.Name });
    return Ok(product);
}

// ✅ Bon - La logique métier est dans le service
[HttpPost]
public async Task<IActionResult> Create(CreateProductDto dto)
{
    var result = await _service.CreateAsync(dto);
    return result.ToActionResult();
}
```

### ❌ Ignorer les erreurs

```csharp
// ❌ Mauvais
var result = await _service.CreateAsync(dto);
// On continue sans vérifier le résultat

// ✅ Bon
var result = await _service.CreateAsync(dto);
if (result.IsFailure)
{
    _logger.LogError($"Failed to create: {result.Error}");
    return result;
}
```

---

## Conclusion

Le Result Pattern vous permet de :
- ✅ Écrire du code plus robuste et prévisible
- ✅ Gérer les erreurs de manière explicite
- ✅ Éviter les null reference exceptions
- ✅ Faciliter les tests unitaires
- ✅ Améliorer la lisibilité du code

Pour plus d'informations ou des questions, n'hésitez pas à consulter les exemples fournis dans ce guide.