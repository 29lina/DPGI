USE [LR4]
GO

/****** Object:  Table [dbo].[Студенти]    Script Date: 25.03.2025 23:39:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Студенти](
	[Номер_залікової_книги] [varchar](10) NOT NULL,
	[ПІБ] [varchar](100) NOT NULL,
	[Група] [varchar](10) NOT NULL,
	[Адреса] [text] NOT NULL,
 CONSTRAINT [PK_Студенти] PRIMARY KEY CLUSTERED 
(
	[Номер_залікової_книги] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

