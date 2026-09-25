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

CleanArchMvc — Visão geral da arquitetura/engenharia de software, com foco em separação de responsabilidades, testabilidade e manutenção.
Resumo
Projeto organizado em camadas clássicas de Clean Architecture / Onion:
•	Domain — entidades, validações e interfaces de repositório.
•	Application — DTOs, serviços (use-case layer), comandos/queries e handlers (MediatR), mapeamentos (AutoMapper).
•	Infra.Data — implementação de repositórios, DbContext (EF Core) e identidade.
•	IoC — composição / extensão para registro de dependências, JWT e Swagger.
•	WebUI — aplicação Razor Pages / MVC (controllers + views).
•	API — endpoints REST (controllers) e token JWT.
Objetivo: documentação para entendimento rápido do papel de cada camada, classes principais, métodos e fluxo de execução.
Camada Domain
Papel: modelo de negócio, regras e contratos (interfaces) que representam o núcleo independente de infraestrutura.
Principais arquivos/classes:
•	Entities
•	Domain/Entities/Product.cs
•	Construtores: Product(string name, string description, decimal price, int stock, string? image) e Product(int id, ...)
•	Métodos:
•	Update(string name, string description, decimal price, int stock, string? image, int categoryId) — atualiza campos e valida.
•	ValidateDomain(...) (privado) — valida regras de negócio (nome mínimo, descrição, price >= 0, stock >= 0, tamanho de image).
•	Domain/Entities/Category.cs
•	Construtores e Update(string name) + validação de nome (mínimo 3 caracteres).
•	Validation
•	Domain/Validation/DomainExceptionValidation.cs
•	static void When(bool hasError, string error) — lança exceção de domínio quando regra falha.
•	Interfaces
•	Domain/Interfaces/IProductRepository.cs
•	Contrato: GetByIdAsync, CreateAsync, GetProductsAsync, UpdateAsync, RemoveAsync.
•	Domain/Interfaces/ICategoryRepositry.cs
•	Contrato: GetById, GetCategories, Create, Update, Remove.
•	Account
•	Domain/Account/IAuthenticate.cs
•	Contrato: Authenticate, RegisterUser, Logout.
•	Domain/Account/ISeedUserRoleInitial.cs — seeds de roles/usuários.
Camada Application
Papel: casos de uso, DTOs e orquestração de operações (não depende de EF, depende de interfaces do Domain).
Principais arquivos/classes:
•	DTOs
•	Application/DTOs/ProductDTO.cs
•	Propriedades: Id, Name, Description, Price, Stock, Image, CategoryId, Category
•	DataAnnotations para validação de entrada (Required, MinLength, Range, etc.).
•	Mappings
•	Application/Mappings/DomainToDTOMappingProfile.cs (AutoMapper)
•	CreateMap<Product, ProductDTO>().ReverseMap() e Category <-> CategoryDTO.
•	Serviços (interface + implementação)
•	Application/Interface/IProductService.cs
•	Assina: GetProducts, GetById, Add, Update, Remove.
•	Application/Services/ProductService.cs
•	Dependências: IMapper, IMediator.
•	Métodos:
•	GetProducts() — cria GetProductsQuery, envia via _mediator.Send, mapeia resultado para IEnumerable<ProductDTO>.
•	GetById(int? id) — cria GetProductByIdQuery, envia via mediator, mapeia para ProductDTO.
•	Add(ProductDTO) — mapeia para ProductCreateCommand e envia via mediator.
•	Update(ProductDTO) — mapeia para ProductUpdateCommand e envia.
•	Remove(int? id) — cria ProductRemoveCommand e envia.
•	Application/Interface/ICategoryService.cs e Application/Services/CategoryService.cs
•	Métodos: GetCategories, GetById, Add, Update, Delete — usam ICategoryRepositry + AutoMapper.
•	MediatR — Commands / Queries / Handlers (fluxo interno)
•	Queries:
•	Application/Products/Queries/GetProductsQuery — representa consulta de lista.
•	Application/Products/Queries/GetProductByIdQuery — consulta por id.
•	Commands:
•	Application/Products/Commands/ProductCreateCommand, ProductUpdateCommand, ProductRemoveCommand (definem dados transferidos para handlers).
•	Handlers:
•	GetProductsQueryHandler.Handle — chama _productRepository.GetProductsAsync().
•	GetProductByIdQueryHandler.Handle — chama _productRepository.GetByIdAsync(request.Id).
•	ProductCreateCommandHandler.Handle — cria entidade Product a partir do comando; chama _productRepository.CreateAsync.
•	ProductUpdateCommandHandler.Handle — recupera entidade, chama Update(...), _productRepository.UpdateAsync.
•	ProductRemoveCommandHandler.Handle — recupera e chama _productRepository.RemoveAsync.
Camada Infra.Data
Papel: implementação concreta de persistência (EF Core) e Identity.
Principais arquivos/classes:
•	Infra.Data/Context/ApplicationDbContext.cs
•	Herdado de IdentityDbContext<ApplicationUser>.
•	DbSet<Category> Categories, DbSet<Product> Products.
•	OnModelCreating aplica configurações por assembly.
•	Identity
•	Infra.Data/Identity/ApplicationUser.cs — class ApplicationUser : IdentityUser.
•	Infra.Data/Identity/AuthenticateService.cs — implementação de Domain.Account.IAuthenticate
•	Dependências: SignInManager<ApplicationUser>, UserManager<ApplicationUser>.
•	Métodos:
•	Authenticate(string email, string password) — PasswordSignInAsync retorna result.Succeeded.
•	RegisterUser(string email, string password) — cria ApplicationUser via _userManager.CreateAsync e faz _singInManager.SignInAsync se criado.
•	Logout() — _singInManager.SignOutAsync().
•	Infra.Data/Identity/SeedUserRoleInitial.cs — popula roles e admin (seed).
•	Repositórios (implementações)
•	Infra.Data/Repositories/ProductRepository.cs : implementa IProductRepository
•	CreateAsync(Product) — adiciona e salva contexto.
•	GetByIdAsync(int? id) — inclui Category e retorna produto por id.
•	GetProductsAsync() — lista todos.
•	UpdateAsync(Product) — update + SaveChanges.
•	RemoveAsync(Product) — remove + SaveChanges.
•	Infra.Data/Repositories/CategoryRespository.cs : implementa ICategoryRepositry
•	Métodos: Create, GetById, GetCategories, Update, Remove — operações EF Core básicas.
•	Configurations / Migrations
•	Infra.Data/EntitiesConfiguration/* e Infra.Data/Migrations/* — configurações e migrações EF.
Camada IoC (composição)
Papel: registrar dependências e configurar infra (DB, Identity, AutoMapper, MediatR, JWT, Swagger).
Principais arquivos:
•	IoC/DependencyInjection.cs
•	AddInfrastructure(IServiceCollection, IConfiguration) — registra DbContext, Identity, cookies, repositórios, services, AutoMapper e MediatR (carrega assembly CleanArchMvc.Application).
•	IoC/DependencyInjectionAPI.cs
•	AddInfrastructureAPI(...) — similar a AddInfrastructure, versão utilizada pela API.
•	IoC/DependencyInjectionJWT.cs
•	AddInfrastructureJWT(...) — configura autenticação JWT (validação de emissor, audiência, chave e ClockSkew).
•	IoC/DependencyInjectionSwagger (quando aplicável) — configuração de Swagger (utilizado pela API).
Camada WebUI (Razor / MVC)
Papel: interface web (Views + Controllers) — usa serviços da camada Application.
Principais arquivos/classes (controllers / views):
•	WebUI/Controllers/ProductsController.cs
•	Dependências: IProductService, ICategoryService, IWebHostEnvironment.
•	Ações:
•	Index() — lista produtos via _productService.GetProducts().
•	Create() (GET) — popula ViewBag.CategoryId com GetCategories.
•	Create(ProductDTO) (POST) — valida ModelState, chama _productService.Add.
•	Edit(int? id) (GET) — recupera por id e popula view.
•	Edit(ProductDTO) (POST) — _productService.Update.
•	Delete(int? id) (GET) — mostra confirmação (autorizado apenas para Role = Admin).
•	DeleteConfirmed(int id) (POST) — _productService.Remove.
•	Details(int? id) — exibe detalhes e verifica existência de arquivo de imagem.
•	WebUI/Controllers/AccountController.cs
•	Dependência: IAuthenticate.
•	Ações:
•	Login (GET/POST) — chama _authenticate.Authenticate(...) e redireciona conforme ReturnUrl.
•	Register (GET/POST) — chama _authenticate.RegisterUser(...).
•	Logout() — chama _authenticate.Logout().
Camada API
Papel: endpoints REST protegidos por JWT e integração com serviços Application.
Principais arquivos:
•	API/Controllers/ProductsController.cs
•	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
•	Endpoints: GET /api/products, GET /api/products/{id}, POST, PUT, DELETE.
•	Operações chamam IProductService para executar casos de uso.
•	API/Controllers/TokenController.cs
•	Dependências: IAuthenticate, IConfiguration.
•	Endpoints:
•	POST api/token/LoginUser — chama _authentication.Authenticate(...). Se sucesso, chama GenerateToken(LoginModel) que cria JWT com claims, emissor/audience e tempo de expiração (config via Jwt:SecretKey, Jwt:Issuer, Jwt:Audience).
•	POST api/token/CreateUser — cria usuário via IAuthenticate.RegisterUser (exigido autorização).
•	GenerateToken(LoginModel) — método privado que monta JwtSecurityToken e retorna UserToken contendo Token e Expiration.
Fluxo de comunicação (resumido)
1.	Requisição entra na WebUI (controller action) ou na API (controller endpoint).
2.	Controller valida entrada (ModelState) e invoca o serviço da camada Application (IProductService / ICategoryService / IAuthenticate).
3.	Service (Application) transforma DTOs em Commands/Queries (AutoMapper) e envia para MediatR (_mediator.Send).
4.	MediatR entrega ao Handler correspondente:
•	QueryHandler consulta IProductRepository / ICategoryRepositry.
•	CommandHandler cria/atualiza/ remove entidade e chama repositório.
5.	Repositório (Infra.Data) usa ApplicationDbContext (EF Core) para executar operações no banco (Add/Update/Remove/Include/ToList/Find).
6.	Resultado sobe pelo chain (Handler -> Service -> Controller).
7.	Controller retorna View (WebUI) ou ActionResult/JSON (API). No caso de autenticação JWT:
•	TokenController usa IAuthenticate.Authenticate que delega a AuthenticateService (SignInManager/UserManager) para validar credenciais.
•	Se válido, TokenController.GenerateToken cria e retorna o JWT.
Principais regras de negócio e validações
•	Product.ValidateDomain(...) (Domain) — regras obrigatórias:
•	Name requerido e mínimo 3 caracteres.
•	Description requerido e mínimo 5 caracteres.
•	Price >= 0
•	Stock >= 0
•	Image máximo 250 caracteres.
•	Em violação, DomainExceptionValidation.When lança exceção de domínio.
•	Category.ValidateDomain(...) — nome requerido e mínimo 3 caracteres.
•	Validações de entrada para APIs e formulários são declaradas em ProductDTO com DataAnnotations.
Repositórios / Interfaces (resumo)
•	IProductRepository ⇄ ProductRepository (Infra.Data)
•	CRUD async: CreateAsync, GetByIdAsync, GetProductsAsync, UpdateAsync, RemoveAsync.
•	ICategoryRepositry ⇄ CategoryRespository (Infra.Data)
•	Create, GetById, GetCategories, Update, Remove.
•	IAuthenticate ⇄ AuthenticateService
•	Authenticate, RegisterUser, Logout → usa UserManager e SignInManager.
Integrações externas e mensageria
•	Banco de dados: SQL Server via EF Core (ApplicationDbContext).
•	Autenticação: ASP.NET Core Identity (UserManager, SignInManager, IdentityDbContext).
•	Token: JWT (config via Jwt:SecretKey, Jwt:Issuer, Jwt:Audience).
•	Swagger (IoC) para documentação da API.
•	Não há mensageria assíncrona (fila/pub-sub) na implementação atual.
Fluxo completo de execução (exemplo: criar produto via API)
1.	Cliente POST /api/products com ProductDTO (JWT no Authorization).
2.	ProductsController.Post valida productDto e chama _productService.Add(productDto).
3.	ProductService.Add mapeia productDTO → ProductCreateCommand e envia _mediator.Send.
4.	ProductCreateCommandHandler.Handle cria Product (valida regras de domínio no construtor) e chama _productRepository.CreateAsync(product).
5.	ProductRepository.CreateAsync adiciona no ApplicationDbContext.Products e faz SaveChangesAsync.
6.	Entidade persistida; retorno sobe pelo handler → service → controller.
7.	Controller responde 201 Created com rota para GetProduct (id).
Observações finais
•	Código de composição/registro (IoC) está em CleanArchMvc.IoC — é o ponto central para habilitar AutoMapper, MediatR, Identity, DbContext e repositórios.
•	Validações cruciais estão no Domain (DomainExceptionValidation) — responsabilidade do modelo.
•	Use IProductService e ICategoryService como API da camada Application; esses serviços encapsulam lógica de orquestração com MediatR.
•	Para entender detalhes de cada método, consulte os arquivos indicados (caminhos e nomes estão listados acima).