USE master
GO
IF DB_ID('MediaPlayer') IS NOT NULL
	DROP DATABASE MediaPlayer
GO
	CREATE DATABASE MediaPlayer

USE MediaPlayer

CREATE TABLE Song
(
	SongName NCHAR(50),
	SongPath NCHAR(100),
	SongPlaylist NCHAR(50),
)

CREATE TABLE Playlist
(
	PlaylistName NCHAR(50),
)
