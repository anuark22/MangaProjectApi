-- =============================================================================
-- SEED DATA: Generación de datos de prueba para MangaWebApi
-- =============================================================================

SET NOCOUNT ON;

-- 1. Insertar Tipos de Manga (MangasType)
IF NOT EXISTS (SELECT 1 FROM [dbo].[MangasType] WHERE [MangaTypeName] = 'Manhwa')
BEGIN
    INSERT INTO [dbo].[MangasType] ([MangaTypeName], [MangaTypeImgUrl], [Status])
    VALUES ('Manhwa', 'manhwa_icon.png', 1);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[MangasType] WHERE [MangaTypeName] = 'Manhua')
BEGIN
    INSERT INTO [dbo].[MangasType] ([MangaTypeName], [MangaTypeImgUrl], [Status])
    VALUES ('Manhua', 'manhua_icon.png', 1);
END

-- 2. Insertar Demografías (Demographics)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Demographics] WHERE [Name] = 'Seinen')
BEGIN
    INSERT INTO [dbo].[Demographics] ([Name], [Description], [DemographyImgUrl])
    VALUES ('Seinen', 'Obras dirigidas a hombres adultos y jóvenes con temas más maduros', 'seinen.png');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Demographics] WHERE [Name] = 'Shoujo')
BEGIN
    INSERT INTO [dbo].[Demographics] ([Name], [Description], [DemographyImgUrl])
    VALUES ('Shoujo', 'Obras dirigidas al público femenino juvenil enfocado en emociones y romance', 'shoujo.png');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Demographics] WHERE [Name] = 'Josei')
BEGIN
    INSERT INTO [dbo].[Demographics] ([Name], [Description], [DemographyImgUrl])
    VALUES ('Josei', 'Obras dirigidas a mujeres adultas con dramas realistas', 'josei.png');
END

-- 3. Insertar Géneros (Genres)
DECLARE @GenresList TABLE (Name VARCHAR(100), Description VARCHAR(MAX));
INSERT INTO @GenresList (Name, Description) VALUES
('Action', 'Historias con alta intensidad, peleas y adrenalina'),
('Adventure', 'Viajes fascinantes, exploración de mundos desconocidos'),
('Comedy', 'Situaciones divertidas, humor y momentos cómicos'),
('Drama', 'Historias profundas centradas en conflictos humanos y emociones'),
('Slice of Life', 'Relatos de la vida cotidiana y relaciones interpersonales'),
('Fantasy', 'Mundos de magia, criaturas míticas y poderes sobrenaturales'),
('Magic', 'Sistemas de magia, hechiceros y conjuros'),
('Supernatural', 'Fenómenos paranormales, fantasmas y entidades ocultas'),
('Horror', 'Suspenso, terror psicológico y misterios escalofriantes'),
('Mystery', 'Acertijos, crímenes y enigmas por resolver'),
('Psychological', 'Enfoque en los estados mentales y batallas intelectuales'),
('Romance', 'Historias de amor y conexiones de pareja'),
('Sci-Fi', 'Tecnología avanzada, futuro espacial y mundos futuristas'),
('Thriller', 'Tensión constante y giros inesperados'),
('Sports', 'Competición atlética, superación y trabajo en equipo'),
('Superpowers', 'Personajes con habilidades sobrehumanas'),
('Martial Arts', 'Combate cuerpo a cuerpo, artes marciales y dojos'),
('Reincarnation', 'Vidas pasadas, reencarnaciones en mundos distintos'),
('Isekai', 'Transportado o invocado a otro mundo'),
('School Life', 'Ambiente escolar, clubes y juventud'),
('Mecha', 'Robots gigantes, trajes de combate y guerras mecanizadas'),
('Demons', 'Entidades demoníacas, exorcismos y pactos oscuros');

INSERT INTO [dbo].[Genres] ([GenreName], [GenreDescription], [CreatedAt], [Status], [GenreImgUrl], [UpdatedAt])
SELECT gl.Name, gl.Description, CURRENT_TIMESTAMP, 1, LOWER(REPLACE(gl.Name, ' ', '_')) + '.png', CURRENT_TIMESTAMP
FROM @GenresList gl
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Genres] g WHERE g.[GenreName] = gl.Name);

-- 4. Insertar Usuarios adicionales para el foro
IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'EnmanuelLopez123')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [Password], [RoleId], [Status], [CreatedAt], [AceptTerms], [IsBlocked])
    VALUES ('EnmanuelLopez123', 'kawai@hotmail.com', '$2a$11$0w88yZgK1nI...', 1, 1, CURRENT_TIMESTAMP, 1, 0);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'kurohige')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [Password], [RoleId], [Status], [CreatedAt], [AceptTerms], [IsBlocked])
    VALUES ('kurohige', 'summer2525@hotmail.com', '$2a$11$0w88yZgK1nI...', 1, 1, CURRENT_TIMESTAMP, 1, 0);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'Helario123')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [Password], [RoleId], [Status], [CreatedAt], [AceptTerms], [IsBlocked])
    VALUES ('Helario123', 'top25@gmail.com', '$2a$11$0w88yZgK1nI...', 1, 1, CURRENT_TIMESTAMP, 1, 0);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'OtakuMaster')
BEGIN
    INSERT INTO [dbo].[Users] ([Username], [Email], [Password], [RoleId], [Status], [CreatedAt], [AceptTerms], [IsBlocked])
    VALUES ('OtakuMaster', 'otaku_master@gmail.com', '$2a$11$0w88yZgK1nI...', 1, 1, CURRENT_TIMESTAMP, 1, 0);
END

-- 5. Insertar Mangas del Frontend
DECLARE @MangaTypeId_Manga INT = (SELECT TOP 1 [MangaTypeId] FROM [dbo].[MangasType] WHERE [MangaTypeName] = 'Manga');
DECLARE @MangaTypeId_Manhwa INT = (SELECT TOP 1 [MangaTypeId] FROM [dbo].[MangasType] WHERE [MangaTypeName] = 'Manhwa');
DECLARE @MangaTypeId_Manhua INT = (SELECT TOP 1 [MangaTypeId] FROM [dbo].[MangasType] WHERE [MangaTypeName] = 'Manhua');

DECLARE @DemographyId_Shounen INT = (SELECT TOP 1 [DemographyId] FROM [dbo].[Demographics] WHERE [Name] = 'Shounen');
DECLARE @DemographyId_Seinen INT = (SELECT TOP 1 [DemographyId] FROM [dbo].[Demographics] WHERE [Name] = 'Seinen');
DECLARE @DemographyId_Shoujo INT = (SELECT TOP 1 [DemographyId] FROM [dbo].[Demographics] WHERE [Name] = 'Shoujo');

-- Manga 1: BOKU NO HERO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaTittle] = 'BOKU NO HERO')
BEGIN
    INSERT INTO [dbo].[Mangas] 
    ([MangaTittle], [MangaDescription], [AlternativeNames], [Mangaka], [CreatedAt], [MangaFrontImgUrl], [RegisteredBy], [MangaTypeId], [EditorialName], [Rating], [Hidden], [Status], [IsAmateur], [IsErotic], [IsFeatured], [ViewCount])
    VALUES
    ('BOKU NO HERO', 'En un mundo donde el 80% de la población nace con dones sobrenaturales, Izuku Midoriya nace sin poderes pero sueña con convertirse en el mejor héroe. Todo cambia tras conocer al héroe número uno, All Might.', 'My Hero Academia', 'Kohei Horikoshi', CURRENT_TIMESTAMP, 'bokunohero.jpg', 1, @MangaTypeId_Manga, 'Shueisha', 4.8, 0, 'Publishing', 0, 0, 1, 25000);
END

-- Manga 2: TALES OF DEMONS AND GODS
IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaTittle] = 'TALES OF DEMONS AND GODS')
BEGIN
    INSERT INTO [dbo].[Mangas] 
    ([MangaTittle], [MangaDescription], [AlternativeNames], [Mangaka], [CreatedAt], [MangaFrontImgUrl], [RegisteredBy], [MangaTypeId], [EditorialName], [Rating], [Hidden], [Status], [IsAmateur], [IsErotic], [IsFeatured], [ViewCount])
    VALUES
    ('TALES OF DEMONS AND GODS', 'Nie Li, el más fuerte espiritista demoníaco, reencarna a sus 13 años conservando todos sus recuerdos tras ser asesinado por el Sabio Emperador. Ahora tiene una nueva oportunidad para proteger a su ciudad y seres queridos.', 'Yao Shen Ji', 'Mad Snail', CURRENT_TIMESTAMP, 'talesofdemon.jpg', 1, @MangaTypeId_Manhua, 'Kuaikan Manhua', 4.7, 0, 'Publishing', 0, 0, 1, 18000);
END

-- Manga 3: SOLO LEVELING
IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaTittle] = 'SOLO LEVELING')
BEGIN
    INSERT INTO [dbo].[Mangas] 
    ([MangaTittle], [MangaDescription], [AlternativeNames], [Mangaka], [CreatedAt], [MangaFrontImgUrl], [RegisteredBy], [MangaTypeId], [EditorialName], [Rating], [Hidden], [Status], [IsAmateur], [IsErotic], [IsFeatured], [ViewCount])
    VALUES
    ('SOLO LEVELING', 'Conocido como el cazador más débil de la humanidad, Sung Jin-woo se encuentra al borde de la muerte dentro de una misteriosa mazmorra doble. Al despertar, obtiene acceso a un sistema de registro único.', 'Na Honjaman Rebeleop', 'Chugong / DUBU', CURRENT_TIMESTAMP, 'sololeveling.jpg', 1, @MangaTypeId_Manhwa, 'D&C Media', 4.9, 0, 'Finished', 0, 0, 1, 45000);
END

-- Manga 4: NANATSU NO TAIZAI
IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaTittle] = 'NANATSU NO TAIZAI')
BEGIN
    INSERT INTO [dbo].[Mangas] 
    ([MangaTittle], [MangaDescription], [AlternativeNames], [Mangaka], [CreatedAt], [MangaFrontImgUrl], [RegisteredBy], [MangaTypeId], [EditorialName], [Rating], [Hidden], [Status], [IsAmateur], [IsErotic], [IsFeatured], [ViewCount])
    VALUES
    ('NANATSU NO TAIZAI', 'Los Siete Pecados Capitales, una legendaria orden de caballeros acusados de conspirar contra el reino, se dispersaron. La princesa Elizabeth emprende un viaje para encontrarlos y salvar a Britania.', 'The Seven Deadly Sins', 'Nakaba Suzuki', CURRENT_TIMESTAMP, 'nanatsu.jpg', 1, @MangaTypeId_Manga, 'Kodansha', 4.6, 0, 'Finished', 0, 0, 1, 21000);
END

-- Manga 5: BLACK CLOVER
IF NOT EXISTS (SELECT 1 FROM [dbo].[Mangas] WHERE [MangaTittle] = 'BLACK CLOVER')
BEGIN
    INSERT INTO [dbo].[Mangas] 
    ([MangaTittle], [MangaDescription], [AlternativeNames], [Mangaka], [CreatedAt], [MangaFrontImgUrl], [RegisteredBy], [MangaTypeId], [EditorialName], [Rating], [Hidden], [Status], [IsAmateur], [IsErotic], [IsFeatured], [ViewCount])
    VALUES
    ('BLACK CLOVER', 'Asta y Yuno fueron abandonados en la misma iglesia el mismo día. Criados juntos, supieron del Rey Mago y juraron competir por el trono. Pero mientras Yuno es un genio, Asta nació sin magia alguna.', 'Burakku Kuroba', 'Yūki Tabata', CURRENT_TIMESTAMP, 'blackclover.jpg', 1, @MangaTypeId_Manga, 'Shueisha', 4.7, 0, 'Publishing', 0, 0, 1, 19500);
END

-- 6. Asociar Demografías a los Mangas
DECLARE @IdBoku INT = (SELECT [MangaId] FROM [dbo].[Mangas] WHERE [MangaTittle] = 'BOKU NO HERO');
DECLARE @IdTales INT = (SELECT [MangaId] FROM [dbo].[Mangas] WHERE [MangaTittle] = 'TALES OF DEMONS AND GODS');
DECLARE @IdSolo INT = (SELECT [MangaId] FROM [dbo].[Mangas] WHERE [MangaTittle] = 'SOLO LEVELING');
DECLARE @IdNanatsu INT = (SELECT [MangaId] FROM [dbo].[Mangas] WHERE [MangaTittle] = 'NANATSU NO TAIZAI');
DECLARE @IdBlack INT = (SELECT [MangaId] FROM [dbo].[Mangas] WHERE [MangaTittle] = 'BLACK CLOVER');

IF @IdBoku IS NOT NULL AND @DemographyId_Shounen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[MangasDemographics] WHERE [MangaId] = @IdBoku)
    INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId]) VALUES (@DemographyId_Shounen, @IdBoku);

IF @IdTales IS NOT NULL AND @DemographyId_Shounen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[MangasDemographics] WHERE [MangaId] = @IdTales)
    INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId]) VALUES (@DemographyId_Shounen, @IdTales);

IF @IdSolo IS NOT NULL AND @DemographyId_Seinen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[MangasDemographics] WHERE [MangaId] = @IdSolo)
    INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId]) VALUES (@DemographyId_Seinen, @IdSolo);

IF @IdNanatsu IS NOT NULL AND @DemographyId_Seinen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[MangasDemographics] WHERE [MangaId] = @IdNanatsu)
    INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId]) VALUES (@DemographyId_Seinen, @IdNanatsu);

IF @IdBlack IS NOT NULL AND @DemographyId_Shounen IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[MangasDemographics] WHERE [MangaId] = @IdBlack)
    INSERT INTO [dbo].[MangasDemographics] ([DemographyId], [MangaId]) VALUES (@DemographyId_Shounen, @IdBlack);

-- 7. Asociar Géneros a Mangas
DECLARE @GenreAction INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Action');
DECLARE @GenreAdventure INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Adventure');
DECLARE @GenreSuperpowers INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Superpowers');
DECLARE @GenreSchool INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'School Life');
DECLARE @GenreFantasy INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Fantasy');
DECLARE @GenreMagic INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Magic');
DECLARE @GenreSupernatural INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Supernatural');
DECLARE @GenreReincarnation INT = (SELECT [GenreId] FROM [dbo].[Genres] WHERE [GenreName] = 'Reincarnation');

-- Boku no Hero (Action, Superpowers, School Life)
IF @IdBoku IS NOT NULL
BEGIN
    IF @GenreAction IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdBoku AND GenreId = @GenreAction)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdBoku, @GenreAction);
    IF @GenreSuperpowers IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdBoku AND GenreId = @GenreSuperpowers)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdBoku, @GenreSuperpowers);
    IF @GenreSchool IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdBoku AND GenreId = @GenreSchool)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdBoku, @GenreSchool);
END

-- Solo Leveling (Action, Fantasy, Supernatural)
IF @IdSolo IS NOT NULL
BEGIN
    IF @GenreAction IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdSolo AND GenreId = @GenreAction)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdSolo, @GenreAction);
    IF @GenreFantasy IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdSolo AND GenreId = @GenreFantasy)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdSolo, @GenreFantasy);
    IF @GenreSupernatural IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdSolo AND GenreId = @GenreSupernatural)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdSolo, @GenreSupernatural);
END

-- Tales of Demons and Gods (Fantasy, Reincarnation, Action)
IF @IdTales IS NOT NULL
BEGIN
    IF @GenreAction IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdTales AND GenreId = @GenreAction)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdTales, @GenreAction);
    IF @GenreFantasy IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdTales AND GenreId = @GenreFantasy)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdTales, @GenreFantasy);
    IF @GenreReincarnation IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdTales AND GenreId = @GenreReincarnation)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdTales, @GenreReincarnation);
END

-- Black Clover (Action, Magic, Fantasy)
IF @IdBlack IS NOT NULL
BEGIN
    IF @GenreAction IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdBlack AND GenreId = @GenreAction)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdBlack, @GenreAction);
    IF @GenreMagic IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdBlack AND GenreId = @GenreMagic)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdBlack, @GenreMagic);
    IF @GenreFantasy IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaGenres WHERE MangaId = @IdBlack AND GenreId = @GenreFantasy)
        INSERT INTO MangaGenres (MangaId, GenreId) VALUES (@IdBlack, @GenreFantasy);
END

-- 8. Insertar Capítulos de prueba
-- Boku no Hero Chapters
IF @IdBoku IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaChapters WHERE MangaId = @IdBoku)
BEGIN
    INSERT INTO MangaChapters (ChapterNumber, ChapterTitle, MangaId, UploadedBy, UploadedAt, ScanId, ViewAmount, Rating, ChapterStatus)
    VALUES
    (1, 'Izuku Midoriya: Origen', @IdBoku, 1, DATEADD(day, -30, CURRENT_TIMESTAMP), 1, 3500, 4.9, 1),
    (2, 'Rugido en el examen', @IdBoku, 1, DATEADD(day, -23, CURRENT_TIMESTAMP), 1, 2800, 4.8, 1),
    (3, 'Línea de salida', @IdBoku, 1, DATEADD(day, -16, CURRENT_TIMESTAMP), 1, 2400, 4.7, 1),
    (4, 'Entrenamiento de combate', @IdBoku, 1, DATEADD(day, -9, CURRENT_TIMESTAMP), 1, 2100, 4.8, 1),
    (5, '¡Furia, idiota!', @IdBoku, 1, DATEADD(day, -2, CURRENT_TIMESTAMP), 1, 1900, 4.9, 1);
END

-- Solo Leveling Chapters
IF @IdSolo IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MangaChapters WHERE MangaId = @IdSolo)
BEGIN
    INSERT INTO MangaChapters (ChapterNumber, ChapterTitle, MangaId, UploadedBy, UploadedAt, ScanId, ViewAmount, Rating, ChapterStatus)
    VALUES
    (1, 'El cazador más débil de rango E', @IdSolo, 1, DATEADD(day, -35, CURRENT_TIMESTAMP), 1, 8500, 5.0, 1),
    (2, 'La doble mazmorra', @IdSolo, 1, DATEADD(day, -28, CURRENT_TIMESTAMP), 1, 7200, 4.9, 1),
    (3, 'Los mandamientos del templo', @IdSolo, 1, DATEADD(day, -21, CURRENT_TIMESTAMP), 1, 6800, 4.9, 1),
    (4, 'Misión diaria: Despertar', @IdSolo, 1, DATEADD(day, -14, CURRENT_TIMESTAMP), 1, 6100, 5.0, 1),
    (5, 'Nivel 1 al infinito', @IdSolo, 1, DATEADD(day, -7, CURRENT_TIMESTAMP), 1, 5900, 4.9, 1);
END

-- 9. Insertar Comentarios en las discusiones del Foro existentes
DECLARE @PostSource INT = (SELECT TOP 1 [PostId] FROM [dbo].[ForumPosts] WHERE [Title] LIKE '%original source%');
DECLARE @PostUpload INT = (SELECT TOP 1 [PostId] FROM [dbo].[ForumPosts] WHERE [Title] LIKE '%upload your own%');
DECLARE @UserEnmanuel INT = (SELECT [UserId] FROM [dbo].[Users] WHERE [Username] = 'EnmanuelLopez123');
DECLARE @UserKurohige INT = (SELECT [UserId] FROM [dbo].[Users] WHERE [Username] = 'kurohige');
DECLARE @UserHelario INT = (SELECT [UserId] FROM [dbo].[Users] WHERE [Username] = 'Helario123');

IF @PostSource IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[ForumComments] WHERE [PostId] = @PostSource)
BEGIN
    INSERT INTO [dbo].[ForumComments] ([PostId], [UserId], [Content], [CreatedAt])
    VALUES
    (@PostSource, ISNULL(@UserEnmanuel, 1), 'Generalmente en páginas oficiales como Shonen Jump Plus o los sitios web de las editoriales en Japón.', DATEADD(minute, -45, CURRENT_TIMESTAMP)),
    (@PostSource, ISNULL(@UserKurohige, 1), '¡Exacto! También en BookWalker y ComicWalker puedes encontrar los raws oficiales de forma legal.', DATEADD(minute, -20, CURRENT_TIMESTAMP)),
    (@PostSource, 1, '¡Muchas gracias por las recomendaciones! Me ayudaron muchísimo.', DATEADD(minute, -5, CURRENT_TIMESTAMP));
END

IF @PostUpload IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [dbo].[ForumComments] WHERE [PostId] = @PostUpload)
BEGIN
    INSERT INTO [dbo].[ForumComments] ([PostId], [UserId], [Content], [CreatedAt])
    VALUES
    (@PostUpload, ISNULL(@UserHelario, 1), 'Excelente guía, muy bien explicado paso por paso.', DATEADD(hour, -2, CURRENT_TIMESTAMP)),
    (@PostUpload, 1, 'Si tienen dudas sobre el formato de los capítulos, déjenlas aquí y con gusto respondo.', DATEADD(hour, -1, CURRENT_TIMESTAMP));
END

-- 10. Insertar Discusiones adicionales del Foro (para cubrir categorías de SideBar.jsx)
IF NOT EXISTS (SELECT 1 FROM [dbo].[ForumPosts] WHERE [Title] LIKE '%mejores Manhwas de acción%')
BEGIN
    INSERT INTO [dbo].[ForumPosts] ([Title], [Content], [Topic], [SubTopic], [UserId], [ViewsCount], [CreatedAt], [IsPinned], [IsClosed])
    VALUES
    ('¿Cuáles son los mejores Manhwas de acción recomendados?', 'Estoy buscando títulos similares a Solo Leveling y The Beginning After The End. ¿Qué recomendaciones tienen?', 'Library & Mangas', 'Recommendations', ISNULL(@UserEnmanuel, 1), 8400, DATEADD(hour, -5, CURRENT_TIMESTAMP), 0, 0),
    ('Reseña sin spoilers: El desarrollo de personajes en Boku no Hero', 'Analizamos la evolución de Deku y Bakugo a lo largo de las últimas temporadas y cómo cambió su relación.', 'Library & Mangas', 'Reviews', ISNULL(@UserKurohige, 1), 6200, DATEADD(day, -1, CURRENT_TIMESTAMP), 0, 0),
    ('Reclutamiento Scan: Buscamos Editores y Coloristas con experiencia', '¡Nuestro grupo está creciendo! Si sabes usar Photoshop o Illustrator y amas el manga, ¡te estamos buscando!', 'Scanlation Groups', 'Recruitment', ISNULL(@UserHelario, 1), 4300, DATEADD(day, -2, CURRENT_TIMESTAMP), 1, 0),
    ('¡Bienvenidos a la nueva versión de OurMangaSite!', 'Hemos renovado la interfaz para hacerla más veloz y fácil de usar. Déjanos tus sugerencias y comentarios.', 'OMS Community', 'Announcements', 1, 23000, DATEADD(day, -4, CURRENT_TIMESTAMP), 1, 0);
END

-- 11. Insertar Noticias adicionales para NoticeCard
IF NOT EXISTS (SELECT 1 FROM [dbo].[Notices] WHERE [Title] LIKE '%Chainsaw Man%')
BEGIN
    INSERT INTO [dbo].[Notices] ([Title], [Content], [NoticeImgUrl], [CreatedAt], [CreatedBy], [IsActive])
    VALUES
    ('Chainsaw Man: La película del Arco de Reze estrena nuevo teaser', 'Mappa revela imágenes oficiales y anuncia ventana de estreno en cines internacionales.', 'notice1.webp', DATEADD(day, -1, CURRENT_TIMESTAMP), 1, 1),
    ('Jujutsu Kaisen entra en su etapa culminante con récord de ventas', 'El manga alcanza más de 90 millones de copias en circulación a nivel mundial.', 'notice1.webp', DATEADD(day, -2, CURRENT_TIMESTAMP), 1, 1),
    ('My Hero Academia celebra el lanzamiento de su volumen final', 'Kohei Horikoshi comparte una ilustración especial de agradecimiento a los fans.', 'notice1.webp', DATEADD(day, -3, CURRENT_TIMESTAMP), 1, 1);
END
