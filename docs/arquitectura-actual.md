# Arquitectura Actual (NEXUS H3)

## 1. Componentes y Clases
- `Personaje.cs`: Clase base abstracta con propiedades encapsuladas `Nombre`, `Salud` y método abstracto `Reaccionar()`.
- `Cadete.cs` / `Robot.cs` / `Enemigo.cs`: Subclases que implementan el comportamiento específico de cada entidad. `Robot` implementa la interfaz `IActivable`.
- `Inventario.cs` / `Objeto.cs`: Gestión interna de colecciones `List<Objeto>` aplicando consultas LINQ.
- `MapaRealidad.cs`: Matriz lógica para la gestión de coordenadas e identidades.
- `ConsolaIris.cs`: Interfaz de usuario basada en comandos de texto (`System.Console`).

## 2. Limitaciones Técnicas
- La interacción con el usuario se limita a la consola de comandos.
- Ausencia de persistencia en disco o base de datos; todo el ciclo de vida de los datos reside exclusivamente en la memoria RAM volátil.
