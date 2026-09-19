# CleanArchMvc — Visão geral da arquitetura

Resumo
------
Projeto organizado em camadas (estilo Clean Architecture / Onion) com foco em Razor Pages/MVC e API REST.

Estrutura de camadas
--------------------
- Domain: entidades, validações e contratos (interfaces).
- Application: DTOs, serviços (casos de uso), comandos/queries e handlers (MediatR), mapeamentos (AutoMapper).
- Infra.Data: implementação de repositórios (EF Core), DbContext e Identity.
- IoC: composição (registro de dependências, JWT, Swagger).
- WebUI: controllers + views (Razor/MVC).
- API: controllers REST protegidos por JWT.

Camada Domain
-------------
Papel: núcleo de negócio e regras invariantes.

Classes principais e métodos
- Entities/Product.cs
  - Construtores: Product(...), Product(id, ...)
  - Update(...) — atualiza campos e chama validação.
  - ValidateDomain(...) — valida regras (nome, descrição, preço, estoque, imagem).
  - Propriedades: Name, Description, Price, Stock, Image, CategoryId, Category.
- Entities/Category.cs
  - Construtores e Update(name) + validação de nome.
- Validation/DomainExceptionValidation.cs
  - When(bool hasError, string error) — lança exceção de domínio.
- Interfaces
  - IProductRepository: GetByIdAsync, CreateAsync, GetProductsAsync, UpdateAsync, RemoveAsync.
  - ICategoryRepositry: GetById, GetCategories, Create, Update, Remove.
  - IAuthenticate: Authenticate, RegisterUser, Logout.

Camada Application
------------------
Papel: orquestrar casos de uso, mapear DTOs e delegar a Handlers/Repositories.

Classes principais e responsabilidades
- DTOs/ProductDTO.cs — anotações de validação (DataAnnotations) e propriedades de transporte.
- Mappings/DomainToDTOMappingProfile.cs — AutoMapper entre entidades e DTOs.
- Services/ProductService.cs (IProductService)
  - GetProducts() — cria GetProductsQuery, envia por MediatR e mapeia resultado para ProductDTO.
  - GetById(id) — cria GetProductByIdQuery e mapeia resultado.
  - Add(ProductDTO) — mapeia para ProductCreateCommand e envia por MediatR.
  - Update(ProductDTO) — mapeia para ProductUpdateCommand e envia.
  - Remove(id) — cria ProductRemoveCommand e envia.
- Services/CategoryService.cs (ICategoryService)
  - GetCategories, GetById, Add, Update, Delete — usam ICategoryRepositry + AutoMapper.

MediatR (Commands / Queries / Handlers)
- Queries: GetProductsQuery, GetProductByIdQuery.
- Commands: ProductCreateCommand, ProductUpdateCommand, ProductRemoveCommand.
- Handlers: GetProductsQueryHandler, GetProductByIdQueryHandler, ProductCreateCommandHandler, ProductUpdateCommandHandler, ProductRemoveCommandHandler.
  - Handlers consultam IProductRepository e executam operações de persistência.

Camada Infra.Data
-----------------
Papel: persistência (EF Core) e Identity.

Classes e responsabilidades
- Context/ApplicationDbContext.cs — IdentityDbContext<ApplicationUser>, DbSet<Category>, DbSet<Product>, OnModelCreating.
- Identity/ApplicationUser.cs — extensão de IdentityUser.
- Identity/AuthenticateService.cs (IAuthenticate)
  - Authenticate(email, password) — PasswordSignInAsync via SignInManager.
  - RegisterUser(email, password) — UserManager.CreateAsync e SignInManager.SignInAsync.
  - Logout() — SignInManager.SignOutAsync.
- Repositories/ProductRepository.cs (IProductRepository)
  - CreateAsync, GetByIdAsync (Include Category), GetProductsAsync, UpdateAsync, RemoveAsync (usam ApplicationDbContext e SaveChangesAsync).
- Repositories/CategoryRespository.cs (ICategoryRepositry)
  - Create, GetById, GetCategories, Update, Remove.

IoC (composição)
----------------
Papel: registrar serviços e infraestrutura.

Principais pontos
- DependencyInjection.AddInfrastructure / AddInfrastructureAPI
  - Registra DbContext (SQL Server), Identity, repositórios, Application services, AutoMapper, MediatR.
- DependencyInjectionJWT.AddInfrastructureJWT
  - Configura autenticação JWT (Issuer, Audience, SecretKey, TokenValidationParameters).

WebUI (Razor / MVC)
-------------------
Papel: camada de apresentação usando Controllers + Views.

Controllers importantes
- ProductsController
  - Index, Create(GET/POST), Edit(GET/POST), Delete(GET/POST), Details.
  - Usa IProductService e ICategoryService; valida ModelState e redireciona/retorna Views.
- AccountController
  - Login(GET/POST), Register(GET/POST), Logout — usa IAuthenticate.

API
---
Papel: endpoints REST consumíveis via JWT.

Controllers
- ProductsController (API)
  - GET /api/products, GET /api/products/{id}, POST, PUT, DELETE — todos usam IProductService.
- TokenController
  - POST api/token/LoginUser — chama IAuthenticate.Authenticate e, se válido, gera JWT com GenerateToken.
  - POST api/token/CreateUser — cria usuário via IAuthenticate.RegisterUser.

Regras de negócio e validações (principais)
- Validações de Product em Domain: nome requerido (>=3), descrição requerida (>=5), price >=0, stock >=0, image max 250 caracteres. Violação lança DomainExceptionValidation.
- Validações de entrada via ProductDTO (DataAnnotations) garantem checks no nível de apresentação.

Integrações externas
- Banco de dados: SQL Server via EF Core (ApplicationDbContext).
- Autenticação: ASP.NET Core Identity (UserManager, SignInManager) e JWT (DependencyInjectionJWT).
- Documentação API: Swagger (IoC).

Fluxo de execução (resumido)
1. Requisição entra (WebUI controller / API controller).
2. Controller valida entrada e chama Application service (IProductService / ICategoryService / IAuthenticate).
3. Service mapeia DTO → Command/Query e envia por MediatR.
4. Handler processa e chama repositório (Infra.Data) para obter/persistir dados.
5. Repositório usa ApplicationDbContext (EF Core) para operar no banco.
6. Resultado sobe: Handler → Service → Controller → View/Response.
7. Para autenticação JWT: TokenController gera token usando IConfiguration (Jwt:SecretKey, Issuer, Audience).

Observações rápidas
-------------------
- A composição de dependências está centralizada em CleanArchMvc.IoC.
- Regras críticas residem na camada Domain (validações via DomainExceptionValidation).
- Não há mensageria externa na implementação atual.

Arquivos de referência (principais)
- Domain: `CleanArchMvc.Domain/Entities`, `CleanArchMvc.Domain/Interfaces`, `CleanArchMvc.Domain/Validation`.
- Application: `CleanArchMvc.Application/DTOs`, `/Services`, `/Products/Commands`, `/Products/Queries`, `/Products/Handlers`, `/Mappings`.
- Infra.Data: `CleanArchMvc.Infra.Data/Context`, `/Repositories`, `/Identity`.
- IoC: `CleanArchMvc.IoC/DependencyInjection*.cs`.
- WebUI: `CleanArchMvc.WebUI/Controllers`.
- API: `CleanArchMvc.API/Controllers`.

---
Este README descreve a implementação atual para acelerar entendimento e onboarding de desenvolvedores. Para detalhes funcionais, consulte os arquivos listados.

