CREATE PROCEDURE [dbo].[Create_Master_Data]
AS
BEGIN
	-- -------------------------------------------------------------------------
	-- ROLES DEFAULT
	-- -------------------------------------------------------------------------
	IF NOT EXISTS(SELECT * FROM Roles WHERE RoleId = 1)
	BEGIN
		SET IDENTITY_INSERT Roles ON
		INSERT INTO Roles (RoleId, RoleName, RoleDescription, IsActive, IsOwnerRole, IsDefaultRole)
		VALUES (1, 'CLIENTE', 'Role de Cliente Default', 1, 0, 1);
		SET IDENTITY_INSERT Roles OFF
	END
	ELSE 
		UPDATE Roles SET 
			RoleName = 'CLIENTE', 
			RoleDescription = 'Role de Cliente Default',
			IsActive = 1,
			IsDefaultRole = 1
		WHERE RoleId = 1

	IF NOT EXISTS(SELECT * FROM Roles WHERE RoleId = 2)
	BEGIN
		SET IDENTITY_INSERT Roles ON
		INSERT INTO Roles (RoleId, RoleName, RoleDescription, IsActive, IsOwnerRole, IsDefaultRole)
		VALUES (2, 'ADMIN', 'Role de Administración Default', 1, 0, 1);
		SET IDENTITY_INSERT Roles OFF
	END
	ELSE 
		UPDATE Roles SET 
			RoleName = 'ADMIN', 
			RoleDescription = 'Role de Administración Default',
			IsActive = 1,
			IsDefaultRole = 1
		WHERE RoleId = 2

	IF NOT EXISTS(SELECT * FROM Roles WHERE RoleId = 3)
	BEGIN
		SET IDENTITY_INSERT Roles ON
		INSERT INTO Roles (RoleId, RoleName, RoleDescription, IsActive, IsOwnerRole, IsDefaultRole)
		VALUES (3, 'SCAN', 'Rol de Scanlation y subida de capítulos', 1, 0, 1);
		SET IDENTITY_INSERT Roles OFF
	END
	ELSE 
		UPDATE Roles SET 
			RoleName = 'SCAN', 
			RoleDescription = 'Rol de Scanlation y subida de capítulos',
			IsActive = 1,
			IsDefaultRole = 1
		WHERE RoleId = 3

	-- -------------------------------------------------------------------------
	-- ACTUALIZACIÓN DE IMÁGENES DE GÉNEROS
	-- -------------------------------------------------------------------------
	UPDATE Genres SET GenreImgUrl = 'genre_shounen.png' WHERE GenreName = 'Shounen';
	UPDATE Genres SET GenreImgUrl = 'genre_action.png' WHERE GenreName = 'Action';
	UPDATE Genres SET GenreImgUrl = 'genre_adventure.png' WHERE GenreName = 'Adventure';
	UPDATE Genres SET GenreImgUrl = 'genre_comedy.png' WHERE GenreName = 'Comedy';
	UPDATE Genres SET GenreImgUrl = 'genre_drama.png' WHERE GenreName = 'Drama';
	UPDATE Genres SET GenreImgUrl = 'genre_slice_of_life.png' WHERE GenreName = 'Slice of Life';
	UPDATE Genres SET GenreImgUrl = 'genre_fantasy.png' WHERE GenreName = 'Fantasy';
	UPDATE Genres SET GenreImgUrl = 'genre_magic.png' WHERE GenreName = 'Magic';
	UPDATE Genres SET GenreImgUrl = 'genre_supernatural.png' WHERE GenreName = 'Supernatural';
	UPDATE Genres SET GenreImgUrl = 'genre_horror.png' WHERE GenreName = 'Horror';
	UPDATE Genres SET GenreImgUrl = 'genre_mystery.png' WHERE GenreName = 'Mystery';
	UPDATE Genres SET GenreImgUrl = 'genre_psychological.png' WHERE GenreName = 'Psychological';
	UPDATE Genres SET GenreImgUrl = 'genre_romance.png' WHERE GenreName = 'Romance';
	UPDATE Genres SET GenreImgUrl = 'genre_sci_fi.png' WHERE GenreName = 'Sci-Fi';
	UPDATE Genres SET GenreImgUrl = 'genre_thriller.png' WHERE GenreName = 'Thriller';
	UPDATE Genres SET GenreImgUrl = 'genre_sports.png' WHERE GenreName = 'Sports';
	UPDATE Genres SET GenreImgUrl = 'genre_superpowers.png' WHERE GenreName = 'Superpowers';
	UPDATE Genres SET GenreImgUrl = 'genre_martial_arts.png' WHERE GenreName = 'Martial Arts';
	UPDATE Genres SET GenreImgUrl = 'genre_reincarnation.png' WHERE GenreName = 'Reincarnation';
	UPDATE Genres SET GenreImgUrl = 'genre_isekai.png' WHERE GenreName = 'Isekai';
	UPDATE Genres SET GenreImgUrl = 'genre_school_life.png' WHERE GenreName = 'School Life';
	UPDATE Genres SET GenreImgUrl = 'genre_mecha.png' WHERE GenreName = 'Mecha';
	UPDATE Genres SET GenreImgUrl = 'genre_demons.png' WHERE GenreName = 'Demons';

	-- -------------------------------------------------------------------------
	-- ACTUALIZACIÓN DE IMÁGENES DE NOTICIAS
	-- -------------------------------------------------------------------------
	UPDATE Notices SET NoticeImgUrl = 'notice_one_piece.png' WHERE NoticeId = 1;
	UPDATE Notices SET NoticeImgUrl = 'notice_chainsaw_man.png' WHERE NoticeId = 2;
	UPDATE Notices SET NoticeImgUrl = 'notice_jujutsu_kaisen.png' WHERE NoticeId = 3;
	UPDATE Notices SET NoticeImgUrl = 'notice_my_hero_academia.png' WHERE NoticeId = 4;
END
