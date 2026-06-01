# Bssy.Mediatr 😁

Bssy.Mediatr est un petit package open source de type mediator, cree par Bissaye.

A la base, ce projet a ete fait pour m'entrainer, comprendre plus en profondeur le pattern Mediator, la reflection, l'injection de dependances et la construction d'un package .NET reutilisable. Il a ete fait sans IA. Si le projet peut aussi aider quelqu'un, tant mieux.

## Objectif

Bssy.Mediatr permet de decoupler l'appelant d'une action de son implementation concrete.

Au lieu d'appeler directement un service ou un handler, l'application envoie une request ou publie une notification via `IMediator`. Le mediator retrouve ensuite le bon handler depuis le conteneur d'injection de dependances.

Le package supporte actuellement:

- les requests sans retour;
- les requests avec retour;
- les notifications avec un ou plusieurs handlers;
- les pipeline behaviors pour ajouter des traitements autour des requests;
- l'enregistrement automatique des handlers via `IServiceCollection`.

## Installation

Le projet cible actuellement `.NET 10`:

```xml
<TargetFramework>net10.0</TargetFramework>
```

### Depuis le code source

Clone le repository puis reference le projet `Bssy.Mediatr.Core` depuis ton application:

```bash
dotnet add reference path/to/Bssy.Mediatr.Core/Bssy.Mediatr.Core.csproj
```

### Depuis NuGet

Si le package est publie sur NuGet, tu pourras l'installer avec:

```bash
dotnet add package Bssy.Mediatr.Core
```

## Configuration

Dans ton application, ajoute Bssy.Mediatr au conteneur DI:

```csharp
using Bssy.Mediatr.Core;

builder.Services.AddBssyMediatrCore(typeof(Program).Assembly);
```

Tu peux passer un ou plusieurs assemblies:

```csharp
builder.Services.AddBssyMediatrCore(
    typeof(Program).Assembly,
    typeof(SomeHandler).Assembly);
```

Si aucun assembly n'est fourni, le package tente de charger les assemblies references par l'assembly appelant.

## Requests sans retour

Une request sans retour implemente `IRequest`.

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class CreateUserCommand : IRequest
{
    public string Email { get; init; } = string.Empty;
}
```

Le handler implemente `IRequestHandler<TRequest>`:

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    public Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // logique metier
        return Task.CompletedTask;
    }
}
```

Utilisation:

```csharp
public sealed class UserService
{
    private readonly IMediator _mediator;

    public UserService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task CreateUser(string email, CancellationToken cancellationToken)
    {
        return _mediator.Send(
            new CreateUserCommand { Email = email },
            cancellationToken);
    }
}
```

## Requests avec retour

Une request avec retour implemente `IRequest<TResponse>`.

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid Id { get; init; }
}

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
}
```

Le handler implemente `IRequestHandler<TRequest, TResponse>`:

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class GetUserByIdQueryHandler
    : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        UserDto? user = new UserDto
        {
            Id = request.Id,
            Email = "user@example.com"
        };

        return Task.FromResult(user);
    }
}
```

Utilisation:

```csharp
UserDto? user = await mediator.Send(
    new GetUserByIdQuery { Id = userId },
    cancellationToken);
```

## Notifications

Une notification implemente `INotification`.

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class UserCreatedNotification : INotification
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
}
```

Un handler de notification implemente `INotificationHandler<TNotification>`:

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class SendWelcomeEmailHandler
    : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // envoyer un email de bienvenue
        return Task.CompletedTask;
    }
}
```

Tu peux avoir plusieurs handlers pour la meme notification:

```csharp
public sealed class WriteAuditLogHandler
    : INotificationHandler<UserCreatedNotification>
{
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        // ecrire dans les logs
        return Task.CompletedTask;
    }
}
```

Publication:

```csharp
await mediator.Publish(
    new UserCreatedNotification
    {
        UserId = userId,
        Email = email
    },
    cancellationToken);
```

Les handlers de notification sont executes l'un apres l'autre, dans l'ordre retourne par le conteneur DI.

## Pipeline behaviors

Les pipeline behaviors permettent d'executer du code avant et/ou apres un handler de request.

Ils sont utiles pour:

- la validation;
- le logging;
- les transactions;
- la mesure de performance;
- la gestion d'erreurs;
- des regles transverses.

### Behavior pour une request sans retour

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class LoggingBehavior<TRequest>
    : IBssyMPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    public async Task Handle(
        TRequest request,
        Func<Task> next,
        CancellationToken cancellationToken)
    {
        // avant le handler
        await next();
        // apres le handler
    }
}
```

### Behavior pour une request avec retour

```csharp
using Bssy.Mediatr.Core.Abstractions;

public sealed class LoggingBehavior<TRequest, TResponse>
    : IBssyMPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        Func<Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        // avant le handler
        TResponse response = await next();
        // apres le handler

        return response;
    }
}
```

### Enregistrement des pipeline behaviors

Les pipeline behaviors ne sont pas auto-enregistres par defaut.

C'est volontaire: leur ordre d'execution est important. Avec `Microsoft.Extensions.DependencyInjection`, les services recuperes via `GetServices<T>()` sont retournes dans l'ordre d'enregistrement. Bssy.Mediatr inverse ensuite la liste lors de la composition du pipeline pour que le premier behavior enregistre soit le premier execute.

Exemple:

```csharp
services.AddScoped<
    IBssyMPipelineBehavior<CreateUserCommand>,
    LoggingBehavior<CreateUserCommand>>();

services.AddScoped<
    IBssyMPipelineBehavior<CreateUserCommand>,
    ValidationBehavior<CreateUserCommand>>();

services.AddScoped<
    IBssyMPipelineBehavior<CreateUserCommand>,
    TransactionBehavior<CreateUserCommand>>();
```

Ordre d'execution:

```text
LoggingBehavior
ValidationBehavior
TransactionBehavior
CreateUserCommandHandler
```

Pour une request avec retour:

```csharp
services.AddScoped<
    IBssyMPipelineBehavior<GetUserByIdQuery, UserDto?>,
    LoggingBehavior<GetUserByIdQuery, UserDto?>>();
```

## Fonctionnalites disponibles

### `IMediator.Send<TRequest>`

Envoie une request sans retour.

```csharp
Task Send<TRequest>(
    TRequest request,
    CancellationToken cancellationToken = default)
    where TRequest : IRequest;
```

### `IMediator.Send<TResponse>`

Envoie une request avec retour.

```csharp
Task<TResponse> Send<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default);
```

### `IMediator.Publish<TNotification>`

Publie une notification vers tous ses handlers enregistres.

```csharp
Task Publish<TNotification>(
    TNotification notification,
    CancellationToken cancellationToken = default)
    where TNotification : INotification;
```

### Auto-registration des handlers

`AddBssyMediatrCore` scanne les assemblies fournis et enregistre automatiquement:

- `IRequestHandler<TRequest>`;
- `IRequestHandler<TRequest, TResponse>`;
- `INotificationHandler<TNotification>`.

Les handlers sont enregistres avec une duree de vie `Scoped`.

### Cache interne des wrappers

Le mediator utilise un cache interne pour eviter de reconstruire les wrappers de handlers a chaque appel. Cela permet de garder une API simple tout en limitant le cout de la reflection apres le premier appel pour un type donne.

## Exemple complet

```csharp
using Bssy.Mediatr.Core;
using Bssy.Mediatr.Core.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBssyMediatrCore(typeof(Program).Assembly);

var app = builder.Build();

app.MapPost("/users", async (CreateUserCommand command, IMediator mediator) =>
{
    await mediator.Send(command);
    return Results.Ok();
});

app.Run();

public sealed class CreateUserCommand : IRequest
{
    public string Email { get; init; } = string.Empty;
}

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    public Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

## Notes actuelles

- Le package depend de `Microsoft.Extensions.DependencyInjection`.
- Le projet cible actuellement `net10.0`.
- Les handlers sont enregistres automatiquement.
- Les pipeline behaviors doivent etre enregistres explicitement pour garder le controle de l'ordre.
- Les notifications sont executees sequentiellement.

## Licence

Ce projet est open source. Consulte le fichier de licence du repository si une licence est ajoutee.
