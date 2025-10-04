# TaskManager.API — Documentação do Projeto

Este repositório contém a API do projeto TaskManager, desenvolvida para um teste técnico. A API expõe endpoints para autenticação de usuários e CRUD de tarefas. O foco é demonstrar arquitetura organizada, regras de autenticação básicas, persistência com SQLite e uma superfície de endpoints consistente para consumo por um frontend React/Vite.

## Tecnologias e Arquitetura
- `.NET 8` com `ASP.NET Core`
- `Controllers` (arquitetura MVC básica) para organização dos endpoints
- `Entity Framework Core` com `SQLite` (`Data Source=tasks.db`)
- `Swagger/OpenAPI` habilitado em `Development` para inspeção dos endpoints
- `CORS` configurado via `FrontendOrigin` (default: `http://localhost:5173`)
- Estrutura de pastas:
  - `Controllers/` — `AuthController`, `UsersController`, `TasksController`
  - `Models/` — `TaskItem`, `User`, DTOs (`AuthDtos.cs`)
  - `Data/` — `AppDbContext`
  - `Services/` — `Security` (hash/salt de senha)
  - `wwwroot/` — pasta para assets estáticos (mantida com `.keep`)

## Configuração e Execução
- Pré-requisitos: `SDK .NET 8`
- Configuração padrão:
  - Banco: `SQLite` em `TaskManager.Api/tasks.db`
  - Origin do frontend: `http://localhost:5173` (alterável via `appsettings.json` ⇒ `FrontendOrigin`)
- Rodando a API:
  - `cd TaskManager.Api`
  - `dotnet restore`
  - `dotnet build`
  - `dotnet run`
- Swagger: disponível em desenvolvimento em `http://localhost:5052/swagger`

### appsettings.json
- `FrontendOrigin`: define a origem permitida no CORS para o frontend.
- `ConnectionStrings:DefaultConnection`: permite alterar o caminho do SQLite.

## Modelos

### User
```
Id: int
Username: string
Email: string
Phone: string
PasswordHash: byte[]
PasswordSalt: byte[]
CreatedAt: DateTime
UpdatedAt: DateTime?
```

### TaskItem
```
Id: int
Title: string
Description: string?
Status: string // "pending" | "in_progress" | "done"
CreatedAt: DateTime
UpdatedAt: DateTime?
UserId: string?
```

### DTOs de Autenticação (AuthDtos.cs)
- `RegisterRequest(username, password, email, phone)`
- `LoginRequest(username, password)`
- `EditUserRequest(email?, phone?, password?)`
- `UserResponse(id, username, email, phone, createdAt, updatedAt)`

## Regras de Autenticação
- Registro exige: `username`, `password`, `email`, `phone` (não há validação de formato de telefone no backend)
- Login exige: `username` e `password`
- Alteração de perfil: permite atualizar `email`, `phone` e `password`
- Senhas são armazenadas com `hash` e `salt` via `Security` (`CreatePasswordHash`, `VerifyPassword`)
- Não há JWT/token — o frontend mantém sessão local com o usuário retornado; pode ser estendido para JWT.

## Endpoints

### Auth
- `POST /api/auth/register`
  - Request (JSON):
    ```json
    {
      "username": "string",
      "password": "string",
      "email": "string",
      "phone": "string"
    }
    ```
  - Responses:
    - `201 Created` com `UserResponse`
    - `400 Bad Request` se campos obrigatórios ausentes
    - `409 Conflict` se `username` ou `email` já existentes

- `POST /api/auth/login`
  - Request (JSON):
    ```json
    { "username": "string", "password": "string" }
    ```
  - Responses:
    - `200 OK` com `UserResponse`
    - `401 Unauthorized` se credenciais inválidas

### Users
- `PUT /api/users/{id}`
  - Request (JSON):
    ```json
    { "email": "string?", "phone": "string?", "password": "string?" }
    ```
  - Responses:
    - `200 OK` com `UserResponse`
    - `404 Not Found` se usuário não existir

### Tasks
- `GET /api/tasks`
  - Responses:
    - `200 OK` com `TaskItem[]`

- `GET /api/tasks/{id}`
  - Responses:
    - `200 OK` com `TaskItem`
    - `404 Not Found` se não existir

- `POST /api/tasks`
  - Request (JSON):
    ```json
    {
      "title": "string",
      "description": "string?",
      "status": "pending|in_progress|done",
      "userId": "string?"
    }
    ```
  - Responses:
    - `201 Created` com `TaskItem`

- `PUT /api/tasks/{id}`
  - Request (JSON): mesmo contrato de `TaskItem` (atualiza campos)
  - Responses:
    - `200 OK` com `TaskItem`
    - `404 Not Found` se não existir

- `DELETE /api/tasks/{id}`
  - Responses:
    - `204 No Content`
    - `404 Not Found` se não existir

## Fluxo de Desenvolvimento e Integração com Frontend
- Frontend padrão em `http://localhost:5173` (React + Vite)
- Garanta que o CORS permita a origem do frontend (`FrontendOrigin`)
- Consumo típico:
  - Registro: chama `POST /api/auth/register`, armazena `UserResponse`
  - Login: chama `POST /api/auth/login`, armazena `UserResponse`
  - Perfil: chama `PUT /api/users/{id}` para atualizar dados
  - Tarefas: CRUD em `/api/tasks`

## Observações e Extensões Sugeridas
- Adicionar JWT (Bearer) para autenticação stateless
- Validações com `DataAnnotations` em modelos/DTOs
- Versionamento de API (`/v1`), filtros globais de exceção e logging estruturado
- Migrações EF (`dotnet ef migrations add <name>`; `dotnet ef database update`) para versionar schema

## Licença
Este projeto inclui um arquivo `LICENSE`. Ajuste conforme necessidade do teste técnico.
