CREATE TABLE [dbo].[System_Users]
(
	[Id] INT NOT NULL IDENTITY ,
	[Username] NVARCHAR(50) NOT NULL,
	[Password] NVARCHAR(MAX) NOT NULL,
	[RegDate] DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	[Email] NVARCHAR(50) NOT NULL,
	PRIMARY KEY ([Id])
)
GO
CREATE INDEX [IX_System_Users_Username] ON [dbo].[System_Users] ([Username])
GO
INSERT INTO [dbo].[System_Users]
	([Username], [Password], [Email])
VALUES
	('test', 'a94a8fe5ccb19ba61c4c0873d391e987982fbbd3', 'test@test.test')
GO