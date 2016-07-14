﻿CREATE TABLE [dbo].[FilterProjects]
(
	[FilterID] INT NOT NULL , 
    [ProjectID] INT NOT NULL, 
	CONSTRAINT [FK_FilterToProjectsBindings_Filters] FOREIGN KEY ([FilterID]) REFERENCES [dbo].[Filters] ([FilterID]),
    CONSTRAINT [FK_FilterToProjectsBindings_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ProjectID]), 
    CONSTRAINT [PK_FilterProjects] PRIMARY KEY ([FilterID], [ProjectID])
)