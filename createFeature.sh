#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# ============================================================================
# VALIDATION FUNCTIONS
# ============================================================================

validate_arguments() {
    if [ $# -eq 0 ]; then
        echo -e "${RED}Error: Feature name is required${NC}"
        echo "Usage: ./createFeature.sh [FeatureName]"
        echo "Example: ./createFeature.sh User"
        exit 1
    fi
}

validate_feature_name() {
    local feature_name=$1
    
    if [[ ! $feature_name =~ ^[A-Z][a-zA-Z0-9]*$ ]]; then
        echo -e "${RED}Error: Feature name must start with an uppercase letter and contain only alphanumeric characters${NC}"
        echo "Example: User, Product, OrderItem"
        exit 1
    fi
}

# ============================================================================
# UTILITY FUNCTIONS
# ============================================================================

pluralize() {
    local word=$1
    echo "${word}s"
}

create_directory() {
    local dir_path=$1
    if [ ! -d "$dir_path" ]; then
        mkdir -p "$dir_path"
        echo -e "${GREEN}✓${NC} Created directory: $dir_path"
    fi
}

create_file_with_content() {
    local file_path=$1
    local content=$2
    
    echo "$content" > "$file_path"
    echo -e "${GREEN}✓${NC} Created file: $file_path"
}

# ============================================================================
# DIRECTORY STRUCTURE FUNCTIONS
# ============================================================================

create_directory_structure() {
    local feature_name=$1
    local feature_plural=$2
    local base_path=$3
    
    echo -e "${BLUE}Creating directory structure for ${feature_plural}...${NC}"
    
    create_directory "$base_path"
    create_directory "$base_path/Models"
    create_directory "$base_path/${feature_name}Dtos"
    create_directory "$base_path/Services"
    create_directory "$base_path/Factories"
    create_directory "$base_path/Repositories"
}

# ============================================================================
# FILE CONTENT GENERATION FUNCTIONS
# ============================================================================

generate_dto_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
namespace YourApp.${feature_plural}.${feature_name}Dtos;

public class ${feature_name}InfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
EOF
}

generate_iservice_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Models;

namespace api.CZ.Features.${feature_plural}.Services;

public interface I${feature_name}Service
{
    Task<IEnumerable<${feature_name}>> GetAll${feature_plural}Async();
    Task<${feature_name}?> Get${feature_name}ByIdAsync(int id);
    Task<${feature_name}> Create${feature_name}Async(${feature_name} ${feature_name,,});
    Task<${feature_name}?> Update${feature_name}Async(int id, ${feature_name} ${feature_name,,});
    Task<bool> Delete${feature_name}Async(int id);
}
EOF
}

generate_service_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Models;
using api.CZ.Features.${feature_plural}.Repositories;

namespace api.CZ.Features.${feature_plural}.Services;

public class ${feature_name}Service : I${feature_name}Service
{
    private readonly I${feature_name}Repository _repository;

    public ${feature_name}Service(I${feature_name}Repository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<${feature_name}>> GetAll${feature_plural}Async()
    {
        throw new NotImplementedException();
    }

    public async Task<${feature_name}?> Get${feature_name}ByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<${feature_name}> Create${feature_name}Async(${feature_name} ${feature_name,,})
    {
        throw new NotImplementedException();
    }

    public async Task<${feature_name}?> Update${feature_name}Async(int id, ${feature_name} ${feature_name,,})
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Delete${feature_name}Async(int id)
    {
        throw new NotImplementedException();
    }
}
EOF
}

generate_ifactory_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Models;

namespace api.CZ.Features.${feature_plural}.Factories;

public interface I${feature_name}Factory
{
    
}
EOF
}

generate_factory_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Models;

namespace api.CZ.Features.${feature_plural}.Factories;

public class ${feature_name}Factory : I${feature_name}Factory
{
    
}
EOF
}

generate_irepository_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Models;

namespace api.CZ.Features.${feature_plural}.Repositories;

public interface I${feature_name}Repository
{
    Task<IEnumerable<${feature_name}>> GetAll${feature_plural}Async();
    Task<${feature_name}?> Get${feature_name}ByIdAsync(int id);
    Task<${feature_name}> Create${feature_name}Async(${feature_name} ${feature_name,,});
    Task<${feature_name}?> Update${feature_name}Async(int id, ${feature_name} ${feature_name,,});
    Task<bool> Delete${feature_name}Async(int id);
}
EOF
}

generate_repository_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Models;

namespace api.CZ.Features.${feature_plural}.Repositories;

public class ${feature_name}Repository : I${feature_name}Repository
{
    public async Task<IEnumerable<${feature_name}>> GetAll${feature_plural}Async()
    {
        throw new NotImplementedException();
    }

    public async Task<${feature_name}?> Get${feature_name}ByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<${feature_name}> Create${feature_name}Async(${feature_name} ${feature_name,,})
    {
        throw new NotImplementedException();
    }

    public async Task<${feature_name}?> Update${feature_name}Async(int id, ${feature_name} ${feature_name,,})
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Delete${feature_name}Async(int id)
    {
        throw new NotImplementedException();
    }
}
EOF
}

generate_extensions_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using api.CZ.Features.${feature_plural}.Services;
using api.CZ.Features.${feature_plural}.Repositories;
using api.CZ.Features.${feature_plural}.Factories;

namespace api.CZ.Features.${feature_plural};

public static class ${feature_name}Extensions
{
    public static IServiceCollection Add${feature_name}Services(this IServiceCollection services)
    {
        services.AddScoped<I${feature_name}Repository, ${feature_name}Repository>();
        services.AddScoped<I${feature_name}Service, ${feature_name}Service>();
        services.AddScoped<I${feature_name}Factory, ${feature_name}Factory>();

        return services;
    }
}
EOF
}

generate_controller_content() {
    local feature_name=$1
    local feature_plural=$2
    
    cat << EOF
using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.${feature_plural}.Services;

namespace api.CZ.Features.${feature_plural};

[ApiController]
[Route("api/[controller]")]
public class ${feature_name}Controller : ControllerBase
{
    private readonly I${feature_name}Service _service;
    private readonly ILogger<${feature_name}Controller> _logger;

    public ${feature_name}Controller(I${feature_name}Service service, ILogger<${feature_name}Controller> logger)
    {
        _service = service;
        _logger = logger;
    }
}
EOF
}

# ============================================================================
# FILE CREATION FUNCTIONS
# ============================================================================

create_all_files() {
    local feature_name=$1
    local feature_plural=$2
    local base_path=$3
    
    echo -e "${BLUE}Creating files for ${feature_plural}...${NC}"
    
    
    
    # Service Interface
    create_file_with_content \
        "$base_path/Services/I${feature_name}Service.cs" \
        "$(generate_iservice_content "$feature_name" "$feature_plural")"
    
    # Service Implementation
    create_file_with_content \
        "$base_path/Services/${feature_name}Service.cs" \
        "$(generate_service_content "$feature_name" "$feature_plural")"
    
    # Factory Interface
    create_file_with_content \
        "$base_path/Factories/I${feature_name}Factory.cs" \
        "$(generate_ifactory_content "$feature_name" "$feature_plural")"
    
    # Factory Implementation
    create_file_with_content \
        "$base_path/Factories/${feature_name}Factory.cs" \
        "$(generate_factory_content "$feature_name" "$feature_plural")"
    
    # Repository Interface
    create_file_with_content \
        "$base_path/Repositories/I${feature_name}Repository.cs" \
        "$(generate_irepository_content "$feature_name" "$feature_plural")"
    
    # Repository Implementation
    create_file_with_content \
        "$base_path/Repositories/${feature_name}Repository.cs" \
        "$(generate_repository_content "$feature_name" "$feature_plural")"
    
    # Extensions
    create_file_with_content \
        "$base_path/${feature_name}Extensions.cs" \
        "$(generate_extensions_content "$feature_name" "$feature_plural")"
    
    # Controller
    create_file_with_content \
        "$base_path/${feature_name}Controller.cs" \
        "$(generate_controller_content "$feature_name" "$feature_plural")"
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

main() {
    # Validate input
    validate_arguments "$@"
    
    local feature_name=$1
    validate_feature_name "$feature_name"
    
    # Generate feature names
    local feature_plural=$(pluralize "$feature_name")
    local base_path="CZ.Features/$feature_plural"
    
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}Creating feature: ${feature_name}${NC}"
    echo -e "${BLUE}Plural form: ${feature_plural}${NC}"
    echo -e "${BLUE}========================================${NC}"
    echo ""
    
    # Create directory structure
    create_directory_structure "$feature_name" "$feature_plural" "$base_path"
    echo ""
    
    # Create all files with content
    create_all_files "$feature_name" "$feature_plural" "$base_path"
    echo ""
    
    # Success message
    echo -e "${GREEN}========================================${NC}"
    echo -e "${GREEN}✓ Feature ${feature_plural} created successfully!${NC}"
    echo -e "${GREEN}========================================${NC}"
    echo ""
    echo -e "${BLUE}Next steps:${NC}"
    echo "1. Add DbSet<${feature_name}> ${feature_plural} { get; set; } to your DbContext"
    echo "2. Register services in Program.cs: builder.Services.Add${feature_name}Services();"
    echo "3. Run migrations: dotnet ef migrations add Add${feature_plural}"
    echo "4. Update database: dotnet ef database update"
}

# Run the script
main "$@"