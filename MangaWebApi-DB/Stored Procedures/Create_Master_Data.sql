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
	-- ACTUALIZACIÓN DE IMÁGENES DE GÉNEROS (.WEBP OPTIMIZADO)
	-- -------------------------------------------------------------------------
	UPDATE Genres SET GenreImgUrl = 'genre_shounen.webp' WHERE GenreName = 'Shounen';
	UPDATE Genres SET GenreImgUrl = 'genre_action.webp' WHERE GenreName = 'Action';
	UPDATE Genres SET GenreImgUrl = 'genre_adventure.webp' WHERE GenreName = 'Adventure';
	UPDATE Genres SET GenreImgUrl = 'genre_comedy.webp' WHERE GenreName = 'Comedy';
	UPDATE Genres SET GenreImgUrl = 'genre_drama.webp' WHERE GenreName = 'Drama';
	UPDATE Genres SET GenreImgUrl = 'genre_slice_of_life.webp' WHERE GenreName = 'Slice of Life';
	UPDATE Genres SET GenreImgUrl = 'genre_fantasy.webp' WHERE GenreName = 'Fantasy';
	UPDATE Genres SET GenreImgUrl = 'genre_magic.webp' WHERE GenreName = 'Magic';
	UPDATE Genres SET GenreImgUrl = 'genre_supernatural.webp' WHERE GenreName = 'Supernatural';
	UPDATE Genres SET GenreImgUrl = 'genre_horror.webp' WHERE GenreName = 'Horror';
	UPDATE Genres SET GenreImgUrl = 'genre_mystery.webp' WHERE GenreName = 'Mystery';
	UPDATE Genres SET GenreImgUrl = 'genre_psychological.webp' WHERE GenreName = 'Psychological';
	UPDATE Genres SET GenreImgUrl = 'genre_romance.webp' WHERE GenreName = 'Romance';
	UPDATE Genres SET GenreImgUrl = 'genre_sci_fi.webp' WHERE GenreName = 'Sci-Fi';
	UPDATE Genres SET GenreImgUrl = 'genre_thriller.webp' WHERE GenreName = 'Thriller';
	UPDATE Genres SET GenreImgUrl = 'genre_sports.webp' WHERE GenreName = 'Sports';
	UPDATE Genres SET GenreImgUrl = 'genre_superpowers.webp' WHERE GenreName = 'Superpowers';
	UPDATE Genres SET GenreImgUrl = 'genre_martial_arts.webp' WHERE GenreName = 'Martial Arts';
	UPDATE Genres SET GenreImgUrl = 'genre_reincarnation.webp' WHERE GenreName = 'Reincarnation';
	UPDATE Genres SET GenreImgUrl = 'genre_isekai.webp' WHERE GenreName = 'Isekai';
	UPDATE Genres SET GenreImgUrl = 'genre_school_life.webp' WHERE GenreName = 'School Life';
	UPDATE Genres SET GenreImgUrl = 'genre_mecha.webp' WHERE GenreName = 'Mecha';
	UPDATE Genres SET GenreImgUrl = 'genre_demons.webp' WHERE GenreName = 'Demons';

	-- -------------------------------------------------------------------------
	-- ACTUALIZACIÓN DE IMÁGENES DE NOTICIAS (.WEBP OPTIMIZADO)
	-- -------------------------------------------------------------------------
	UPDATE Notices SET NoticeImgUrl = 'notice_one_piece.webp' WHERE NoticeId = 1;
	UPDATE Notices SET NoticeImgUrl = 'notice_chainsaw_man.webp' WHERE NoticeId = 2;
	UPDATE Notices SET NoticeImgUrl = 'notice_jujutsu_kaisen.webp' WHERE NoticeId = 3;
	UPDATE Notices SET NoticeImgUrl = 'notice_my_hero_academia.webp' WHERE NoticeId = 4;
END
