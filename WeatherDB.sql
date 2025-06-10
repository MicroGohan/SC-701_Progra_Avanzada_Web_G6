
-- Crear la base de datos
CREATE DATABASE WeatherDB;

-- Usar la base de datos
USE WeatherDB;

-- Crear la tabla de usuario
CREATE TABLE usuario (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
    email NVARCHAR(100) UNIQUE NOT NULL,
    fecha_registro DATE DEFAULT GETDATE()
);
GO

-- Crear la tabla de favorito
CREATE TABLE favorito (
    id_favorito INT IDENTITY(1,1) PRIMARY KEY,
    id_usuario INT NOT NULL,
    ciudad NVARCHAR(100) NOT NULL,
    pais NVARCHAR(100),
    fecha_agregado DATE DEFAULT GETDATE(),
    FOREIGN KEY (id_usuario) REFERENCES usuario(id_usuario)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
GO
