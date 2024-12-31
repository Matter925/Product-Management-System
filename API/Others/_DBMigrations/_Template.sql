DECLARE @MigrationId INT = 1;
DECLARE @MigrationDescription NVARCHAR(400) = N'Put Description here';

-- Check if migration has already been run
IF NOT EXISTS (SELECT 1 FROM [dbo].[_DBMigrations] WHERE Id = @MigrationId)
BEGIN
	-- Place your SQL queries here for the migration
	

	INSERT INTO [dbo].[_DBMigrations] (Id, [Description], CreationDate) 
	VALUES (@MigrationId, @MigrationDescription, GETDATE());
END
ELSE
BEGIN
	SELECT CONCAT('Migration ', @MigrationId, ' has already been run before');
END
GO
