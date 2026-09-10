# CajaVenta — Sistema de Punto de Venta (POS)

Sistema web de punto de venta ASP.NET Core (MVC, .NET 8) con **soporte multi-sucursal**: stock por sucursal, cajas por punto de venta, turnos por caja y usuarios con sucursal asignada.

---

## Contenido

- [Características](#características)
- [Arquitectura](#arquitectura)
- [Tecnologías](#tecnologías)
- [Requisitos](#requisitos)
- [Puesta en marcha](#puesta-en-marcha)
- [Base de datos](#base-de-datos)
- [Usuarios por defecto](#usuarios-por-defecto)
- [Funcionalidad por rol](#funcionalidad-por-rol)
- [Módulos](#módulos)
- [Diseño multi-sucursal](#diseño-multi-sucursal)
- [Modelo de datos](#modelo-de-datos)
- [Migración de esquema](#migración-de-esquema)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Flujo de una venta](#flujo-de-una-venta)
- [Desarrollo](#desarrollo)

---

## Características

- **Punto de venta (POS)**: cobro por código de barras, carrito, ticket y corte de caja (corte X).
- **Multi-sucursal**: cada sucursal tiene su propio stock independiente.
- **Cajas por punto de venta**: una caja = un turno abierto a la vez.
- **Turnos de caja**: apertura con fondo inicial, cierre con corte X.
- **Inventario por sucursal**: entradas, salidas, ajustes, historial por producto y alertas de stock bajo.
- **Ventas con clientes**: catálogo de clientes y constancias (RFC).
- **Usuarios y roles**: Admin (global) y Cajero (asignado a una sucursal).
- **Reportes**: ventas por rango de fechas y sucursal; stock bajo por sucursal.
- **SQLite**: base de datos ligera, sin servidor (archivo `src/CajaVenta.db`).

---

## Arquitectura

Proyecto en capas (Clean Architecture ligera):

```
src/
├── CajaVenta.Domain          → Entidades, enums e interfaces de dominio
├── CajaVenta.Application     → DTOs, servicios de aplicación y reglas de negocio
├── CajaVenta.Infrastructure  → EF Core (DbContext, configuraciones, repositorios, migración)
└── CajaVenta.Web             → ASP.NET Core MVC (controladores, vistas, seed, POS)
```

La `Infrastructure` referencia la `Application`, y la `Web` referencia a ambas. El dominio no depende de nada externo.

## Tecnologías

| Capa | Tecnología |
|---|---|
| Framework | .NET 8 (ASP.NET Core MVC) |
| ORM | Entity Framework Core 8 |
| Base de datos | SQLite |
| Frontend | Razor Views + Bootstrap 5 (CDN) + Bootstrap Icons |
| Autenticación | Cookies + Claims con roles (`Admin`, `Cajero`) |
| Pasarelas de pago | Abstracción `IPasarelaPago`: Stripe, MercadoPago y modo Demo |

---

## SaaS Plataforma (Portal CajaVenta)

El sistema se renta como **SaaS por suscripción mensual** a través del **Portal CajaVenta**, una aplicación ASP.NET Core separada (`src/CajaVenta.Portal/`, puerto `5001` por defecto) enfocada en administrar clientes arrendatarios.

### Modelo de negocio

- **Un cliente = una base de datos aislada.** Al crear un cliente, el portal **provisiona una base SQLite nueva** (`src/CajaVenta.Portal/data/clientes/{id}.db`) con todo el esquema y el seed (usuarios, sucursal, caja, productos).
- **Planes fijos mensuales** (`Básico`, `Pro`, `Empresarial`) con precio configurable.
- **Suscripciones** por cliente: fecha de inicio, vencimiento y estado (Activa / Pendiente / Vencida / Cancelada). Al registrar/confirmar un pago, la suscripción se activa y se extiende 1 mes.
- **Pagos** vía pasarela configurable. La integración se hace por una abstracción `IPasarelaPago`:

### Pasarelas de pago

| Proveedor | Config `Pasarela:` | Uso |
|---|---|---|
| Demo | `Proveedor="Demo"` (default) | Página de pago simulada para pruebas locales; un clic registra el cobro |
| Stripe | `Proveedor="Stripe"`, `Stripe:ClaveSecreta` | Crea una Checkout Session real y recibe webhooks en `POST /api/pagos/webhook/stripe` |
| MercadoPago | `Proveedor="MercadoPago"`, `MercadoPago:TokenAcceso` | Crea una preferencia de pago real; webhook en `POST /api/pagos/webhook/mercadopago` |

> En producción se configura `Pasarela:Proveedor` y la clave correspondiente en el `appsettings.json` del **portal**. Con `Demo` no se requiere ninguna credencial.

### Módulos del portal

- **Dashboard** (`/`): KPIs de clientes, suscripciones activas/vencidas, ingresos del mes y totales, próximos vencimientos y últimos pagos.
- **Clientes** (`/Suscriptores`): alta, edición, estados (Activo / Suspendido / Cancelado / Prueba), detalle con suscripciones y pagos. El alta **crea y provisiona la base de datos del cliente**.
- **Suscripciones** (`/Suscripciones`): asignar plan, editar fechas/estado, cancelar, activar, marcar vencida y **botón "Cobrar"** que lanza el cobro por la pasarela.
- **Pagos** (`/Pagos`): historial, registrar pago manual y página de pago simulada en modo Demo.
- **Planes** (`/Planes`): CRUD de planes con precio mensual y activo/inactivo.

### API de licencias

`GET /api/licencia/{urlAcceso}` devuelve el estado de la licencia del cliente (nombre, plan, vencimiento, último pago, si está activa), útil para que cada instancia en producción valide su suscripción.

### Usuarios del portal

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `portal123` | Admin (se cambia en producción) |

### Puesta en marcha del portal

```bash
dotnet run --project src/CajaVenta.Portal --no-launch-profile --urls http://localhost:5001
```

`http://localhost:5001` → login del portal. La base del portal es `src/CajaVenta.Portal/data/portal.db`; las de los clientes quedan en `src/CajaVenta.Portal/data/clientes/`.

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- macOS / Linux / Windows

## Puesta en marcha

```bash
# En la raíz del repositorio
./run.sh
```

O manualmente:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet build CajaVenta.sln
dotnet run --project src/CajaVenta.Web --no-launch-profile
```

La aplicación escucha en `http://localhost:5000` y en el primer arranque:

1. **Crea** la base de datos `src/CajaVenta.db` (si no existe).
2. **Aplica** la migración de esquema (`EsquemaMigracion`) sobre bases existentes (columnas nuevas, tablas `Sucursales`/`Cajas`/`StocksInventario` y backfill de datos).
3. **Sembra** la base con datos de ejemplo si está vacía (`SeedData`).

Para partir de cero:

```bash
./reset-db.sh    # elimina src/CajaVenta.db; se recrea con seed en el próximo arranque
```

## Base de datos

- Es un archivo SQLite en `src/CajaVenta.db` (no se envía al repositorio; está en `.gitignore`).
- Se usa `Database.EnsureCreated()` para crear el esquema inicial.
- La evolución de esquema de bases existentes se hace con **SQL idempotente** en `EsquemaMigracion.Aplicar()`: agrega columnas, crea tablas e índices con `IF NOT EXISTS` y hace *backfill* (asignación de sucursal principal, creación de stock por sucursal a partir del stock legado, etc.).

## Usuarios por defecto

| Usuario | Contraseña | Rol | Sucursal |
|---|---|---|---|
| `admin` | `admin123` | Admin | Global (todas) |
| `cajero` | `cajero123` | Cajero | Sucursal Principal |

> **Ojo**: cambia estas contraseñas en producción.

## Funcionalidad por rol

| Acción | Admin | Cajero |
|---|---|---|
| POS (cobrar) | ✔ | ✔ (solo su sucursal) |
| Turnos (abrir/cerrar/corte) | ✔ | ✔ (solo su sucursal) |
| Productos | ✔ | ✔ (solo su sucursal) |
| Inventario (entradas/salidas/ajustes) | ✔ | ✔ (solo su sucursal) |
| Clientes | ✔ | ✔ |
| Reportes | ✔ | ✔ (solo su sucursal) |
| Sucursales y cajas | ✔ | ✖ |
| Usuarios | ✔ | ✖ |

---

## Módulos

### POS (`/Pos`)
- Búsqueda y cobro por código de barras.
- Carrito en sesión; al añadir un producto se valida y muestra el **stock de la sucursal actual**.
- Venta registra el movimiento de inventario (`Salida`) **en la sucursal del turno** y emite ticket.

### Turnos (`/Turnos`)
- Un turno abierto por caja; al abrir se registra sucursal y caja.
- Cierre con conteo de efectivo y **corte X** imprimible (ventas, tickets, fondo).
- Historial con sucursal y caja.

### Inventario (`/Inventarios`)
- Selector de sucursal; catálogo de stock por producto (existencias, valor, estado OK/por debajo del mínimo/sin existencias).
- Movimientos filtrables por sucursal/fechas (entrada, salida, ajuste).
- Historial por producto.

### Productos (`/Productos`)
- CRUD de productos (código de barras, costo, precio, impuesto, stock mínimo). El stock ya **no** vive en el producto: vive en `StocksInventario` por sucursal.

### Clientes (`/Clientes`)
- CRUD de clientes con RFC y generación de **constancia**.

### Sucursales (`/Sucursales`) — solo Admin
- CRUD de sucursales (activa/inactiva) y de **cajas** por sucursal.

### Usuarios (`/Usuarios`) — solo Admin
- CRUD de usuarios, rol (Admin/Cajero), sucursal asignada y restablecimiento de contraseña.

### Reportes (`/Reportes`)
- Ventas por rango de fechas y sucursal (total, impuestos, efectivo, tickets).
- Alertas de stock bajo y sin existencias por sucursal.

---

## Diseño multi-sucursal

- **Stock por sucursal**: la entidad `StockInventario` tiene clave primaria compuesta `(ProductoId, SucursalId)`. El campo `Producto.StockActual` se eliminó del dominio.
- **Cajas por sucursal**: la entidad `Caja` pertenece a una `Sucursal`.
- **Turnos por caja**: `TurnoCaja` guarda `SucursalId` + `CajaId`; cada caja solo permite un turno abierto a la vez (`ObtenerAbiertoEnCajaAsync`).
- **Usuarios**: `Admin` no tiene sucursal (acceso global). `Cajero` tiene `SucursalId`; al iniciar sesión su *claim* `SucursalId` limita lo que ve (POS, turnos, inventario, reportes solo de esa sucursal).
- **Movimientos de inventario**: cada `MovimientoInventario` lleva `SucursalId`; una venta genera la salida en la sucursal del turno.

**IDs fijos** usados por migración/seed:

| Elemento | GUID |
|---|---|
| Sucursal Principal | `4556A2B0-0E51-4E6B-9F3E-000000000001` |
| Caja 1 | `4556A2B0-0E51-4E6B-9F3E-000000000002` |

---

## Modelo de datos

Entidades principales en `CajaVenta.Domain/Entities`:

```
Sucursal ─┬─ Caja
          └─ Usuario (SucursalId, opcional)
          └─ StockInventario (ProductoId + SucursalId, StockActual)
          └─ TurnoCaja (SucursalId, CajaId, UsuarioId)
              └─ Venta (TurnoCajaId) ─ VentasDetalle (ProductoId)
          └─ MovimientoInventario (ProductoId, SucursalId, Tipo, Cantidad, CostoUnitario)
Producto ─┴─ StockInventario / MovimientosInventario / VentasDetalle
Cliente ── Venta (ClienteId)
```

Configuraciones EF en `Infrastructure/Persistence/Configurations`.

## Migración de esquema

`CajaVenta.Infrastructure/Persistence/EsquemaMigracion.cs` (`Aplicar(context)`) es idempotente (se puede ejecutar varias veces) y sobre una base existente:

1. Agrega columnas si faltan: `Usuarios.SucursalId`, `TurnosCaja.SucursalId`, `TurnosCaja.CajaId`, `MovimientosInventario.SucursalId`.
2. Crea tablas si no existen: `Sucursales`, `Cajas`, `StocksInventario` (+ índices).
3. *Backfill*: crea la sucursal/caja principal, asigna los usuarios no-Admin a la sucursal principal, rellena `SucursalId`/`CajaId` de turnos y movimientos existentes, y genera `StocksInventario` a partir de la columna legada `Productos.StockActual` (si existía).

El seed (`SeedData`, en `CajaVenta.Infrastructure/Seed`) solo corre cuando la base está vacía y crea: sucursal/caja principales, `admin` (global) y `cajero` (Sucursal Principal), productos de ejemplo, y por cada producto un movimiento de entrada + su fila de `StocksInventario`. El Portal reutiliza el mismo seed al **provisionar la base de cada cliente SaaS**.

---

## Estructura del proyecto

```
src/
├── CajaVenta.Domain/
│   ├── Common/            BaseEntity
│   ├── Entities/          Sucursal, Caja, StockInventario, Producto, Usuario,
│   │                      TurnoCaja, Venta, VentaDetalle, MovimientoInventario, Cliente...
│   ├── Enums/             RolUsuario, EstadoTurno, TipoMovimientoInventario, EstadoVenta...
│   └── Interfaces/        ISucursalRepository, IUsuarioRepository, IInventarioRepository...
├── CajaVenta.Application/
│   ├── Common/            Result<T>, PasswordHasher
│   ├── DTOs/             SucursalDto, CajaDto, StockProductoDto, TurnoCajaDto, CorteXDto...
│   ├── Interfaces/       ISucursalService, IUsuarioService, IInventarioService, ITurnoService...
│   └── Services/         SucursalService, UsuarioService, ProductoService, InventarioService,
│                         TurnoService, VentaService, ClienteService
├── CajaVenta.Infrastructure/
│   ├── DependencyInjection.cs
│   ├── Seed/SeedData.cs
│   └── Persistence/
│       ├── CajaVentaDbContext.cs
│       ├── EsquemaMigracion.cs
│       ├── Configurations/  (una clase por entidad)
│       └── Repositories/    (implementaciones de las interfaces de dominio)
├── CajaVenta.Web/
│   ├── Program.cs           (DI, auth, EnsureCreated, migración, seed)
│   ├── SucursalContext.cs   (helper de claims: sucursal actual del usuario)
│   ├── Controllers/         Auth, Home, Pos, Turnos, Productos, Inventarios,
│   │                        Clientes, Reportes, Sucursales, Usuarios
│   ├── Models/              (modelos de las vistas)
│   └── Views/               Razor views por controlador
└── CajaVenta.Portal/
    ├── Program.cs           (DI, auth, EnsureCreated, seed del portal)
    ├── Data/                PortalDbContext, EsquemaPortal (sembrado de planes/admin)
    ├── Entities/            Plan, Suscriptor, Suscripcion, Pago, UsuarioPortal
    ├── Services/            SuscriptorService, SuscripcionService, PagoService,
    │   │                    PlanService, LicenciaService, TenantProvisioner
    │   └── Pasarelas/       IPasarelaPago, StripePasarela, MercadoPagoPasarela, DemoPasarela
    ├── Controllers/         Auth, Dashboard, Suscriptores, Suscripciones, Pagos,
    │                        Planes + API (licencia, webhooks)
    ├── Models/              (modelos de las vistas)
    ├── Views/               Razor views por controlador
    └── data/                portal.db + bases SQLite provisionadas por cliente
```

## Flujo de una venta

1. El cajero abre un turno en una caja de su sucursal (`/Turnos`).
2. En `POS` escribe el código de barras o busca el producto; se valida stock de la sucursal del turno.
3. `ConfirmarCobro` crea la venta con cliente (opcional), método de pago y cambio, asociados al turno.
4. `VentaService.CrearAsync`:
   - Descuenta `StockInventario` de la sucursal del turno.
   - Registra `MovimientoInventario` tipo Salida con `SucursalId` del turno.
   - Devuelve el folio; el POS redirige al ticket.
5. Al cerrar el turno se genera el corte X (ventas, efectivo, tickets, fondo).

---

## Desarrollo

- **Regenerar la app en caliente**: `dotnet run watch --project src/CajaVenta.Web`.
- **Restablecer la DB**: `./reset-db.sh` y volver a arrancar.
- **Build**: `dotnet build CajaVenta.sln` (debe quedar en 0 errores).
- **Pruebas manuales (E2E)**:
  - Login: `POST /Auth/Login` con `nombreUsuario` y `contrasena`
  - Crear sucursal: `POST /Sucursales/Crear` (`Nombre`)
  - Crear caja: `POST /Sucursales/CrearCaja` (`SucursalId`, `Nombre`)
  - Crear cajero: `POST /Usuarios/Crear` (`Nombre`, `NombreUsuario`, `Contrasena`, `Rol=Cajero`, `SucursalId`)
  - Abrir turno: `POST /Turnos/Abrir` (`cajaId`, `fondoInicial`)
  - Venta: `POST /Pos/Agregar` (`codigo`) → `POST /Pos/ConfirmarCobro` (`MontoRecibido`)

---

## Licencia

Proyecto interno. No se distribuye.