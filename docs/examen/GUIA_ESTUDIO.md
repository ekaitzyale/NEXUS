# NEXUS — Guía de estudio para Programación

**Estudiante:** __________________________  
**Asignatura:** Programación  
**Fecha:** __________________________  
**Situación del examen / actividad:** __________________________  
**Proyecto:** NEXUS — PROTOCOLO IRIS

Esta guía distingue los conceptos generales de C# de los ejemplos comprobados en el repositorio.

## 1. Programación orientada a objetos
- **Clase:** definición de un tipo; por ejemplo, `GameSession` y `CombatTurn` son clases en el proyecto.
- **Objeto:** instancia creada a partir de una clase.
- **Atributo/campo y propiedad:** datos de una instancia; `GameSession` expone `Hp`, `Kits`, `Room` y `Charges` como propiedades.
- **Método:** comportamiento; `GameSession.Move(...)` y `CombatTurn.Choose(...)` son ejemplos.
- **Herencia:** una clase derivada reutiliza o amplía una clase base; se declara como `class Robot : Enemigo`.
- **Polimorfismo:** la misma operación puede ejecutar implementaciones diferentes según el tipo real.
- **Interfaz:** contrato de miembros que un tipo implementa; por ejemplo, `interface IAtacable { void Atacar(); }`.

Los ejemplos `Robot : Enemigo` e `IAtacable` son **didácticos**, no afirmaciones de que existan esas clases en el juego actual.

### virtual y override
```csharp
class Enemigo
{
    public virtual string Atacar() => "Ataque básico";
}
class Robot : Enemigo
{
    public override string Atacar() => "Ataque láser";
}
```
`virtual` permite sobrescribir un miembro; `override` aporta la implementación derivada.

### int.Parse y int.TryParse
```csharp
int valor = int.Parse("25"); // 25; lanza excepción si el texto no es válido
bool correcto = int.TryParse("abc", out int numero); // false, sin excepción por formato
```

## 2. Windows Forms
- **Controles:** `Form`, `Button`, `Label`, `Panel` y otros componentes visuales.
- **Evento Click:** ejecuta el manejador asociado cuando se hace clic.
- **Propiedad Text:** texto mostrado o introducido por un control.
- **MessageBox.Show:** presenta un cuadro de diálogo.
- **Validación:** comprobar valores antes de procesarlos; para enteros introducidos por el usuario, `int.TryParse` es una opción segura.

**En NEXUS:** `WindowsForms/Forms/IrisGameForm.cs` define un formulario `IrisGameForm : Form`, utiliza `Panel`, `Button`, `Label`, y asigna la propiedad `Text` al título de la ventana. La interacción con el teclado está implementada en `ProcessCmdKey`.

Ejemplo didáctico de evento:
```csharp
private void btnAceptar_Click(object sender, EventArgs e)
{
    if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad < 1)
    {
        MessageBox.Show("Ingrese una cantidad válida.");
        return;
    }
    MessageBox.Show($"Cantidad: {cantidad}");
}
```

## 3. SQL Server y CRUD
- **SqlConnection:** conexión con la base de datos.
- **SqlCommand:** instrucción SQL y parámetros.
- **CRUD:** Create (INSERT), Read (SELECT), Update (UPDATE), Delete (DELETE).
- **DataSet:** contenedor de datos en memoria, puede contener varias tablas.
- **DataGridView:** control para mostrar datos tabulares en Windows Forms.
- **Persistencia:** conservación de la partida entre ejecuciones.
- **Transacción:** agrupa operaciones; `Commit` confirma y `Rollback` revierte si corresponde.

**En NEXUS:** `WindowsForms/Data/SaveRepository.cs` implementa:
| Operación | Método real | SQL |
| --- | --- | --- |
| Crear | `SaveAsync(session, create: true)` | `INSERT INTO dbo.Partida` |
| Leer listado | `ListAsync()` | `SELECT ... FROM dbo.Partida` |
| Leer partida | `LoadAsync(id)` | `SELECT ... WHERE Id = @id` |
| Actualizar | `SaveAsync(session)` | `UPDATE dbo.Partida ... WHERE Id = @id` |
| Eliminar | `DeleteAsync(id)` | `DELETE FROM dbo.Partida WHERE Id = @id` |

`SaveAsync` abre una transacción y ejecuta `CommitAsync` tras guardar. Las consultas usan parámetros como `@id` para no concatenar datos del usuario en SQL.

El esquema se encuentra en `sql/01-inicializar.sql`: base `NexusIris`, tabla `dbo.Partida` y columna `Estado` con JSON de la sesión.

**Precisión:** `DataSet` y `DataGridView` forman parte del temario, pero no se han verificado como componentes utilizados por el juego en esta revisión. No afirmar en el examen que NEXUS los usa sin comprobarlo.

## 4. Cómo presentar el proyecto
1. Identificar el proyecto, estudiante, asignatura y situación del examen.
2. Explicar el objetivo: juego táctico de escritorio en C# con cuatro fases y 18 salas.
3. Mostrar la separación de responsabilidades entre `GameSession` (estado), `CombatTurn` (combate), `IrisGameForm` (interfaz) y `SaveRepository` (SQL).
4. Explicar un caso de persistencia: crear partida, guardarla en `dbo.Partida`, cerrar y cargarla después.
5. Diferenciar las funciones existentes de los ejemplos teóricos de herencia, polimorfismo e interfaces.

## 5. Preguntas de práctica
1. ¿Qué diferencia existe entre clase y objeto?
2. ¿Para qué sirven `virtual` y `override`?
3. ¿Qué ocurre si `int.Parse("hola")` intenta convertir ese texto?
4. ¿Por qué conviene `int.TryParse` en un formulario?
5. ¿Qué hace el evento `Click` y qué representa `Text`?
6. ¿Qué diferencia hay entre `SqlConnection` y `SqlCommand`?
7. ¿Qué sentencia SQL corresponde a cada operación CRUD?
8. ¿Por qué `SaveRepository` utiliza parámetros SQL y una transacción?
9. ¿En qué se diferencian `DataSet` y `DataGridView`?
10. ¿Qué archivos de NEXUS explicarías para demostrar interfaz, lógica y persistencia?
