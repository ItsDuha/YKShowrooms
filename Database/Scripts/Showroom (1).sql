USE [master]
GO
/****** Object:  Database [Showroom]    Script Date: 4/17/2026 11:24:02 AM ******/
CREATE DATABASE [Showroom]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Showroom', FILENAME = N'C:\Users\duhah\Showroom.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Showroom_log', FILENAME = N'C:\Users\duhah\Showroom_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Showroom] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Showroom].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Showroom] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Showroom] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Showroom] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Showroom] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Showroom] SET ARITHABORT OFF 
GO
ALTER DATABASE [Showroom] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Showroom] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Showroom] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Showroom] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Showroom] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Showroom] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Showroom] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Showroom] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Showroom] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Showroom] SET  ENABLE_BROKER 
GO
ALTER DATABASE [Showroom] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Showroom] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Showroom] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Showroom] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Showroom] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Showroom] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Showroom] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Showroom] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Showroom] SET  MULTI_USER 
GO
ALTER DATABASE [Showroom] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Showroom] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Showroom] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Showroom] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Showroom] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [Showroom] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [Showroom] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [Showroom] SET QUERY_STORE = ON
GO
ALTER DATABASE [Showroom] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [Showroom]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 4/17/2026 11:24:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[admins]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[admins](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[email] [nvarchar](255) NOT NULL,
	[password_hash] [nvarchar](max) NOT NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[conversations]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[conversations](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](max) NOT NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[feedback]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[feedback](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[visitor_id] [int] NULL,
	[showroom_id] [int] NOT NULL,
	[rating] [int] NOT NULL,
	[comment] [nvarchar](max) NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[messages]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[messages](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[conversation_id] [int] NOT NULL,
	[role] [nvarchar](max) NOT NULL,
	[content] [nvarchar](max) NOT NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[notifications]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[notifications](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[type] [nvarchar](max) NOT NULL,
	[message] [nvarchar](max) NOT NULL,
	[visitor_id] [int] NULL,
	[is_read] [bit] NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[products]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[products](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](max) NOT NULL,
	[name_ar] [nvarchar](max) NULL,
	[category] [nvarchar](max) NULL,
	[image_url] [nvarchar](max) NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[salespersons]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[salespersons](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](max) NOT NULL,
	[showroom_id] [int] NULL,
	[phone] [nvarchar](max) NULL,
	[email] [nvarchar](max) NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[showrooms]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[showrooms](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](max) NOT NULL,
	[name_ar] [nvarchar](max) NULL,
	[location] [nvarchar](max) NULL,
	[phone] [nvarchar](max) NULL,
	[created_at] [datetimeoffset](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[visitors]    Script Date: 4/17/2026 11:24:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[visitors](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](max) NOT NULL,
	[phone] [nvarchar](max) NOT NULL,
	[email] [nvarchar](max) NULL,
	[showroom_id] [int] NOT NULL,
	[product_id] [int] NULL,
	[salesperson_id] [int] NULL,
	[is_interested] [bit] NULL,
	[follow_up_status] [nvarchar](max) NULL,
	[score] [int] NULL,
	[notes] [nvarchar](max) NULL,
	[is_returning] [bit] NULL,
	[language] [nvarchar](max) NULL,
	[visited_at] [datetimeoffset](7) NULL,
	[created_at] [datetimeoffset](7) NULL,
	[follow_up_notes] [nvarchar](max) NULL,
	[cpr] [nvarchar](max) NULL,
	[IsDeleted] [int] NOT NULL,
	[DeletedFrom] [nvarchar](max) NULL,
	[MarketingSource] [nvarchar](50) NULL,
	[MarketingSourceOther] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260408190840_InitialBaseline', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260408191123_AddValidation', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260411094824_AddVisitorValidation', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260411102251_editvalidation', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260412103000_AddSoftDeleteFlag', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260412173232_AddDeletedFrom', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260412174749_AddDeletedFromFix', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260413144631_AIchat', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260413145100_addAIchat', N'10.0.5')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260414105228_remoceaichattb', N'10.0.5')
GO
SET IDENTITY_INSERT [dbo].[admins] ON 

INSERT [dbo].[admins] ([id], [name], [email], [password_hash], [created_at]) VALUES (1001, N'MonneraIsa', N'MonneraIsa@gmail.com', N'$2a$11$/PLXw3V5LC3fHkxt3S0/eeg5knJ5zoAr35kIVy0Q7aciqI9IKpWmK', CAST(N'2026-04-11T16:58:28.3501894+03:00' AS DateTimeOffset))
INSERT [dbo].[admins] ([id], [name], [email], [password_hash], [created_at]) VALUES (1002, N'AmmenaAli', N'AmmenaAli@gmail.com', N'$2a$11$AWZ11EGeVOfiShrKWHcsLuLViO6f7JJdX7iczeNUsYgECy4w/mTaG', CAST(N'2026-04-11T19:30:31.1281366+03:00' AS DateTimeOffset))
INSERT [dbo].[admins] ([id], [name], [email], [password_hash], [created_at]) VALUES (1003, N'DuhaHashem', N'duhahashemalali@gmail.com', N'$2a$11$nS.p2aQ/qP7lCoQy9lTJj./8SkarwnF4wry3y2PgnbNR714xp.YYu', CAST(N'2026-04-13T15:26:48.6394945+03:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[admins] OFF
GO
SET IDENTITY_INSERT [dbo].[conversations] ON 

INSERT [dbo].[conversations] ([id], [title], [created_at]) VALUES (7, N'Chat – 08/04/2026', CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[conversations] OFF
GO
SET IDENTITY_INSERT [dbo].[feedback] ON 

INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (1, 19, 4, 3, N'', CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (2, 20, 4, 3, N'', CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (3, 21, 6, 5, N'', CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (4, 22, 6, 2, N'', CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (5, 23, 4, 3, N'', CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (1001, 1019, 4, 3, N'Excellent', CAST(N'2026-04-11T14:11:04.0769959+03:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (1002, 1020, 5, 5, N'so good', CAST(N'2026-04-11T14:14:31.9607363+03:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (1003, 1021, 4, 2, N'good', CAST(N'2026-04-11T20:09:11.1207836+03:00' AS DateTimeOffset))
INSERT [dbo].[feedback] ([id], [visitor_id], [showroom_id], [rating], [comment], [created_at]) VALUES (1004, 1022, 4, 3, N'good', CAST(N'2026-04-13T19:18:20.6046885+03:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[feedback] OFF
GO
SET IDENTITY_INSERT [dbo].[notifications] ON 

INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (1, N'new_visitor', N'New walk-in: Abdullah Al-Farsi at showroom #1', 19, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (2, N'new_visitor', N'New walk-in: Sarah Johnson at showroom #1', 20, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (3, N'new_visitor', N'New walk-in: Hassan Al-Mutairi at showroom #1', 29, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (4, N'new_visitor', N'New walk-in: sara rara at showroom #2', 31, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (5, N'new_visitor', N'New walk-in: ddd at showroom #2', 32, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (6, N'new_visitor', N'New walk-in: gee ge at showroom #2', 33, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (7, N'new_visitor', N'New walk-in: kkk at showroom #3', 34, 1, CAST(N'2026-04-02T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (8, N'new_visitor', N'New walk-in: luri at showroom #3', 35, 1, CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (9, N'new_visitor', N'New walk-in: jjj at showroom #2', 30, 1, CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (1001, N'new_visitor', N'New walk-in: Sakeena at showroom #4', 1021, 1, CAST(N'2026-04-11T20:09:04.9782883+03:00' AS DateTimeOffset))
INSERT [dbo].[notifications] ([id], [type], [message], [visitor_id], [is_read], [created_at]) VALUES (1002, N'new_visitor', N'New walk-in: fatimasalman at showroom #4', 1022, 1, CAST(N'2026-04-13T19:18:12.4878763+03:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[notifications] OFF
GO
SET IDENTITY_INSERT [dbo].[products] ON 

INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (1, N'Toyota Land Cruiser', N'تويوتا لاند كروزر', N'SUV', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (2, N'Toyota Camry', N'تويوتا كامري', N'Sedan', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (3, N'Lexus LX 600', N'لكسس LX 600', N'Luxury SUV', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (4, N'Toyota Corolla', N'تويوتا كورولا', N'Sedan', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (5, N'Toyota Fortuner', N'تويوتا فورتشنر', N'SUV', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (6, N'Lexus ES 350', N'لكسس ES 350', N'Luxury Sedan', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (7, N'Toyota Hilux', N'تويوتا هايلكس', N'Pickup', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
INSERT [dbo].[products] ([id], [name], [name_ar], [category], [image_url], [created_at]) VALUES (8, N'Toyota Yaris', N'تويوتا يارس', N'Hatchback', NULL, CAST(N'2026-04-08T19:20:21.6463872+03:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[products] OFF
GO
SET IDENTITY_INSERT [dbo].[salespersons] ON 

INSERT [dbo].[salespersons] ([id], [name], [showroom_id], [phone], [email], [created_at]) VALUES (1, N'Ahmed Al-Rashidi', 4, N'+973 5555 1111', N'ahmed@almanashowroom.qa', CAST(N'2026-04-08T19:22:47.0310599+03:00' AS DateTimeOffset))
INSERT [dbo].[salespersons] ([id], [name], [showroom_id], [phone], [email], [created_at]) VALUES (2, N'Mohammed Al-Sayed', 4, N'+973 5555 2222', N'mohammed@almanashowroom.qa', CAST(N'2026-04-08T19:22:47.0310599+03:00' AS DateTimeOffset))
INSERT [dbo].[salespersons] ([id], [name], [showroom_id], [phone], [email], [created_at]) VALUES (3, N'Khalid Ibrahim', 5, N'+973 5111 2222', N'khalid@citycars.sa', CAST(N'2026-04-08T19:22:47.0310599+03:00' AS DateTimeOffset))
INSERT [dbo].[salespersons] ([id], [name], [showroom_id], [phone], [email], [created_at]) VALUES (4, N'Omar Hassan', 5, N'+973 5333 4444', N'omar@citycars.sa', CAST(N'2026-04-08T19:22:47.0310599+03:00' AS DateTimeOffset))
INSERT [dbo].[salespersons] ([id], [name], [showroom_id], [phone], [email], [created_at]) VALUES (5, N'Faisal Al-Mutairi', 6, N'+973 5111 2222', N'faisal@gulfmotors.ae', CAST(N'2026-04-08T19:22:47.0310599+03:00' AS DateTimeOffset))
INSERT [dbo].[salespersons] ([id], [name], [showroom_id], [phone], [email], [created_at]) VALUES (6, N'Nasser Al-Mansouri', 6, N'+973 5333 4444', N'nasser@gulfmotors.ae', CAST(N'2026-04-08T19:22:47.0310599+03:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[salespersons] OFF
GO
SET IDENTITY_INSERT [dbo].[showrooms] ON 

INSERT [dbo].[showrooms] ([id], [name], [name_ar], [location], [phone], [created_at]) VALUES (4, N'Nissan & INFINITI Showroom - Y.K. Almoayyed & Sons', NULL, N'Sitra, Bahrain', NULL, CAST(N'2026-04-08T19:20:21.6453100+03:00' AS DateTimeOffset))
INSERT [dbo].[showrooms] ([id], [name], [name_ar], [location], [phone], [created_at]) VALUES (5, N'Y.K. Almoayyed & Sons - Electronics and Home Appliances', NULL, N'Gudaibiya, Manama, Bahrain', NULL, CAST(N'2026-04-08T19:20:21.6453100+03:00' AS DateTimeOffset))
INSERT [dbo].[showrooms] ([id], [name], [name_ar], [location], [phone], [created_at]) VALUES (6, N'Almoayyed Motors - Ford', NULL, N'Sitra, Bahrain', NULL, CAST(N'2026-04-08T19:20:21.6453100+03:00' AS DateTimeOffset))
SET IDENTITY_INSERT [dbo].[showrooms] OFF
GO
SET IDENTITY_INSERT [dbo].[visitors] ON 

INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (19,N'Abdullah Al-Farsi',N'+973 5511 2233',NULL,4,NULL,NULL,1,N'pending',65,NULL,0,N'ar',CAST(N'2026-05-01T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'880120001',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (20,N'Sarah Johnson',N'+973 5522 3344',N'sarah.johnson@gmail.com',5,NULL,NULL,1,N'pending',78,NULL,0,N'en',CAST(N'2026-05-02T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'920315002',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (21,N'Mohammed Ali',N'+973 5111 9999',NULL,6,NULL,NULL,1,N'pending',50,NULL,0,N'ar',CAST(N'2026-05-03T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'870614003',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (22,N'Ali Hassan',N'+973 5222 8888',N'ali.hassan@email.com',6,NULL,NULL,0,N'not_interested',30,NULL,0,N'ar',CAST(N'2026-05-04T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'910928004',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (23,N'James Miller',N'+973 5444 5555',NULL,4,NULL,NULL,1,N'contacted',90,NULL,1,N'en',CAST(N'2026-05-05T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'850202005',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (24,N'Fatima Al-Zaabi',N'+973 5666 7777',N'fatima@email.com',4,NULL,NULL,1,N'pending',72,NULL,0,N'ar',CAST(N'2026-05-06T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'930711006',0,N'undefined',N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (25,N'David Chen',N'+973 5533 4455',NULL,4,NULL,NULL,1,N'pending',60,NULL,0,N'en',CAST(N'2026-05-07T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'960418007',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (26,N'Noura Al-Rashidi',N'+973 5544 5566',N'noura@email.com',4,NULL,NULL,1,N'contacted',85,NULL,0,N'ar',CAST(N'2026-05-01T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'890523008',0,NULL,N'TikTok',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (27,N'Khalid Mahmoud',N'+973 5333 7777',NULL,6,NULL,NULL,1,N'contacted',80,NULL,0,N'ar',CAST(N'2026-05-02T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'900812009',0,NULL,N'TikTok',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (28,N'Emma Wilson',N'+973 5888 9999',N'emma.wilson@email.com',4,NULL,NULL,1,N'contacted',55,NULL,0,N'en',CAST(N'2026-05-03T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'950630010',0,NULL,N'TikTok',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (29,N'Hassan Al-Mutairi',N'+973 5555 6677',NULL,4,NULL,NULL,1,N'pending',68,NULL,0,N'ar',CAST(N'2026-05-04T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'870301011',0,NULL,N'Website',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (30,N'Rania Khalil',N'+973 5444 6666',N'rania@email.com',6,NULL,NULL,1,N'pending',74,NULL,0,N'ar',CAST(N'2026-05-05T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'920214012',0,NULL,N'Website',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (31,N'sara rara',N'+973 123456789',NULL,6,NULL,NULL,0,N'pending',0,NULL,0,N'en',CAST(N'2026-05-06T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'880901013',0,N'daily',N'Friend',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (32,N'ddd',N'+973 1234567889',N'dd@gmail.com',6,6,NULL,0,N'pending',0,N'very good experience',0,N'en',CAST(N'2026-05-07T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'910415014',0,N'missed',N'Friend',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (33,N'gee ge',N'+973 123456788',N'dd@gmail.com',6,1,NULL,0,N'contacted',0,N'Wants to explore more products.',0,N'en',CAST(N'2026-05-08T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'930722015',0,N'daily',N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (34,N'kkk',N'+973 12345678',NULL,4,NULL,NULL,0,N'pending',0,NULL,0,N'en',CAST(N'2026-05-09T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'850510016',0,N'missed',N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (35,N'luri',N'+973 12345678',NULL,4,NULL,NULL,0,N'pending',0,N'good ',0,N'en',CAST(N'2026-05-10T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-08T00:00:00.0000000+00:00' AS DateTimeOffset),NULL,N'970803017',0,N'undefined',N'YouTube',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1019,N'noor',N'84566345',NULL,4,1,1,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-11T14:10:46.7443167+03:00' AS DateTimeOffset),NULL,N'984563286',0,NULL,N'Instagram',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1020,N'lamaAli',N'45677654',N'lamaAli@gmail.com',5,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-02T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-11T14:14:20.8168843+03:00' AS DateTimeOffset),NULL,N'123456789',0,N'undefined',N'TikTok',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1021,N'Sakeena',N'98674532',NULL,4,4,5,NULL,N'not_interested',0,NULL,0,N'en',CAST(N'2026-05-03T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-11T20:09:04.5673427+03:00' AS DateTimeOffset),NULL,N'658943562',0,NULL,N'YouTube',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1022,N'fatimasalman',N'78905643',N'fatimasalman@gmail.com',4,6,3,NULL,N'pending',0,N'good experience',0,N'en',CAST(N'2026-05-04T00:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-04-13T19:18:12.2289217+03:00' AS DateTimeOffset),NULL,N'234567765',0,N'daily',N'YouTube',NULL)

INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1023,N'Fatima Ahmed',N'33984521',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-04-30T16:01:11.5940810+00:00' AS DateTimeOffset),CAST(N'2026-04-30T16:01:11.5940810+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1024,N'alalawi',N'64783572',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T01:15:09.9490033+00:00' AS DateTimeOffset),CAST(N'2026-05-01T01:15:09.9490033+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1025,N'mdngi',N'34657658',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T01:43:23.3706865+00:00' AS DateTimeOffset),CAST(N'2026-05-01T01:43:23.3706865+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1026,N'S.Mohamed',N'64785755',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T01:52:22.3912527+00:00' AS DateTimeOffset),CAST(N'2026-05-01T01:52:22.3912527+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1027,N'noor',N'32333642',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T15:06:58.9258471+00:00' AS DateTimeOffset),CAST(N'2026-05-01T15:06:58.9258471+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1028,N'Mustafa',N'34630030',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T15:13:42.4998544+00:00' AS DateTimeOffset),CAST(N'2026-05-01T15:13:42.4998544+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1029,N'miset',N'32077772',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-01T16:13:20.0521380+00:00' AS DateTimeOffset),CAST(N'2026-05-01T16:13:20.0521380+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1030,N'عرس',N'34635423',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'ar',CAST(N'2026-05-02T08:14:37.3132202+00:00' AS DateTimeOffset),CAST(N'2026-05-02T08:14:37.3132202+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1031,N'noor',N'36543487',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-02T20:44:51.8947467+00:00' AS DateTimeOffset),CAST(N'2026-05-02T20:44:51.8947467+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1032,N'خان',N'32223242',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'ar',CAST(N'2026-05-02T20:46:32.2192759+00:00' AS DateTimeOffset),CAST(N'2026-05-02T20:46:32.2192759+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1033,N'yusletf',N'32053672',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-02T21:17:42.6082972+00:00' AS DateTimeOffset),CAST(N'2026-05-02T21:17:42.6082972+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1034,N'visitor14',N'34000014',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-03T10:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-05-03T10:00:00.0000000+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1035,N'visitor15',N'34000015',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-03T11:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-05-03T11:00:00.0000000+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1036,N'visitor16',N'34000016',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-03T12:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-05-03T12:00:00.0000000+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1037,N'visitor17',N'34000017',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-04T09:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-05-04T09:00:00.0000000+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1038,N'visitor18',N'34000018',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-04T10:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-05-04T10:00:00.0000000+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
INSERT [dbo].[visitors] ([id],[name],[phone],[email],[showroom_id],[product_id],[salesperson_id],[is_interested],[follow_up_status],[score],[notes],[is_returning],[language],[visited_at],[created_at],[follow_up_notes],[cpr],[IsDeleted],[DeletedFrom],[MarketingSource],[MarketingSourceOther]) VALUES (1039,N'visitor19',N'34000019',NULL,4,NULL,NULL,NULL,N'pending',0,NULL,0,N'en',CAST(N'2026-05-04T11:00:00.0000000+00:00' AS DateTimeOffset),CAST(N'2026-05-04T11:00:00.0000000+00:00' AS DateTimeOffset),NULL,NULL,0,NULL,N'Walk-in',NULL)
SET IDENTITY_INSERT [dbo].[visitors] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__admins__AB6E6164ACC71181]    Script Date: 4/17/2026 11:24:03 AM ******/
ALTER TABLE [dbo].[admins] ADD UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[admins] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[conversations] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[feedback] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[messages] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[notifications] ADD  DEFAULT ('new_visitor') FOR [type]
GO
ALTER TABLE [dbo].[notifications] ADD  DEFAULT ((0)) FOR [is_read]
GO
ALTER TABLE [dbo].[notifications] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[products] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[salespersons] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[showrooms] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT ('pending') FOR [follow_up_status]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT ((0)) FOR [score]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT ((0)) FOR [is_returning]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT ('en') FOR [language]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT (sysdatetimeoffset()) FOR [visited_at]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT (sysdatetimeoffset()) FOR [created_at]
GO
ALTER TABLE [dbo].[visitors] ADD  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[feedback]  WITH CHECK ADD FOREIGN KEY([showroom_id])
REFERENCES [dbo].[showrooms] ([id])
GO
ALTER TABLE [dbo].[feedback]  WITH CHECK ADD FOREIGN KEY([visitor_id])
REFERENCES [dbo].[visitors] ([id])
GO
ALTER TABLE [dbo].[messages]  WITH CHECK ADD FOREIGN KEY([conversation_id])
REFERENCES [dbo].[conversations] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[notifications]  WITH CHECK ADD FOREIGN KEY([visitor_id])
REFERENCES [dbo].[visitors] ([id])
GO
ALTER TABLE [dbo].[salespersons]  WITH CHECK ADD FOREIGN KEY([showroom_id])
REFERENCES [dbo].[showrooms] ([id])
GO
ALTER TABLE [dbo].[visitors]  WITH CHECK ADD FOREIGN KEY([product_id])
REFERENCES [dbo].[products] ([id])
GO
ALTER TABLE [dbo].[visitors]  WITH CHECK ADD FOREIGN KEY([salesperson_id])
REFERENCES [dbo].[salespersons] ([id])
GO
ALTER TABLE [dbo].[visitors]  WITH CHECK ADD FOREIGN KEY([showroom_id])
REFERENCES [dbo].[showrooms] ([id])
GO
USE [master]
GO
ALTER DATABASE [Showroom] SET  READ_WRITE 
GO
