Proyecto Weather Dashboard Grupo 6

Integrantes:
ABARCA MADRIGAL NATALIA MARIA
GONZAGA CORTES WILSON JESUS
VALLADARES BERMUDEZ DIEGO ALEJANDRO


Repo Github: https://github.com/MicroGohan/SC-701_Progra_Avanzada_Web_G6


--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


Arquitectura del Proyecto:



	WD.Mvc:
		Entradas principales
			Program.cs: registra DI, AddControllersWithViews, AddSession, DbContext, HttpClient tipado WeatherApiClient y el routing por defecto.
			Layout: Views/Shared/_Layout.cshtml con Bootstrap, jQuery, site.css y secciones.

		Controladores
			HomeController: Dashboard (búsqueda, Top 3, parciales).
			FavoritesController: CRUD de favoritos (Add/Delete/UpdatePrioridad/UpdateDescripcion).
			SettingsController: perfil/contraseña/preferencias y privacidad del Top 3.
			UsuariosController: SignUp/Login/Logout gestionando Session.
			PublicController: expone el Top 3 público por usuario y “Explorar” todos los públicos.

		Servicios (capa de negocio, inyectados)
			UserService: autenticación con Session (“UsuarioId”, “UsuarioNombre”), perfil, cambio de contraseña y preferencias de unidades (C/F).
			FavoritosService: integra repositorio + WeatherApiClient; arma ViewModels con clima (Temperatura/Humedad/Descripción).
			DashboardService: maneja unidades y top para el home; delega a servicios.
			PublicTopService: arma el Top 3 público por usuario y el listado global.
			SettingsAppService: maneja la pantalla de configuración.

		ViewModels
			FavoritoClimaViewModel, UserSettingsViewModel, Login/SignUp, PublicUserTopViewModel.

		Vistas
			_Layout.cshtml, Settings/Index.cshtml, Favorites/Index.cshtml, vistas parciales como _TopFavoritos.

		Integraciones
			WeatherApiClient: consume WD.Api; base URL configurable via env var WEATHER_API_BASEURL.

		Estado y navegación
			TempData para mensajes de éxito/validación en POST-Redirect-GET.



	WD.Api:
		WeatherController
			GET /api/weather/search: recibe q, limit, units; consulta OpenWeather (Geo + Weather); normaliza salida a DTO “WeatherResult” consistente (temp, feels_like, wind, clouds, visibilidad, etc.) y localiza descripciones en español (lang=es).
		
		Swagger (Swashbuckle) habilitado para documentación y pruebas.

	

	WD.Data: 
		WeatherDbContext (EF Core, SQL Server): mapea tablas “usuario” y “favorito”, índices (Email único), defaults (GETDATE()), claves foráneas y tamaños de columnas.
		Paquetes EF Core: Microsoft.EntityFrameworkCore, SqlServer, Tools, etc.
	


	WD.Repository: 
		Repositorios (interfaces + implementaciones) para Usuario y Favorito consumidos por servicios en WD.Mvc.



	WD.Models: 
		Entidades de dominio: Usuario, Favorito.
		DTOs/contratos compartidos: WeatherResult, etc.

	

	Flujo de casos de uso:
		Registro/Login
			UsuariosController -> UserService -> Repositorio. Se guarda en Session: “UsuarioId” y “UsuarioNombre”.

		Dashboard/Búsqueda
			HomeController -> DashboardService -> UserService (unidades) -> WeatherApiClient (-> WD.Api -> OpenWeather).

		Favoritos
			FavoritesController -> FavoritosService -> Repositorios (persistencia) y WeatherApiClient (clima) -> ViewModels.

		Configuración
			SettingsController -> SettingsAppService -> UserService (perfil, contraseña, visibilidad del Top 3).

		Público
			PublicController -> PublicTopService -> Repositorios + FavoritosService (para top de otros usuarios).

	
-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


Paquetes/Libraries utilizados:

	NuGet:
		WD.Api: Swashbuckle.AspNetCore 6.6.2 (Swagger/OpenAPI).
		WD.Data: Microsoft.EntityFrameworkCore 9.0.6 (Core/Relational/SqlServer/Tools/Analyzers/Abstractions), Microsoft.EntityFrameworkCore.Design 9.0.6.
		WD.Models: Microsoft.EntityFrameworkCore.Design 9.0.8.

	Plataforma/Infra:
		HttpClient tipado para WeatherApiClient (registrado en DI en WD.Mvc).
		EF Core DbContext (SQL Server).

	Front-end:
		Bootstrap 5 y Bootstrap Icons (CDN) + bundle local; jQuery; uso de modales y componentes. asp-append-version para cache busting.



-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


Principios SOLID y patrones de diseño aplicados:

	SOLID:
		SRP: cada controlador maneja escenarios de su ámbito; cada servicio encapsula una responsabilidad (usuarios, favoritos, dashboard, settings, top público).
		OCP: servicios extensibles sin modificar controladores (DI); rutas por atributos en PublicController; WeatherApiClient configurable por env var.
		LSP: ViewModels y DTOs respetan contratos esperados por la UI; repositorios cumplen contratos de interfaces.
		ISP: interfaces específicas por agregado (IUsuarioRepository, IFavoritoRepository).
		DIP: controladores y servicios dependen de interfaces/abstracciones y de servicios inyectados por el contenedor.

	Patrones:
		MVC: separación Controller-View con Razor Views y parciales (_TopFavoritos).
		Service Layer: DashboardService, UserService, FavoritosService, PublicTopService, SettingsAppService.
		Repository: capa de datos en WD.Repository abstrae EF Core (WD.Data).
		DTO/ViewModel: WeatherResult (DTO normalizado) y ViewModels para UI (evita filtrar entidades de dominio a vistas).
		DI + Typed HttpClient: HttpClient inyectado para WD.Api; en WD.Api el controlador recibe HttpClient via DI.


















