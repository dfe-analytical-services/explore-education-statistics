-- Rename existing boundary level labels to have consistent labels
UPDATE dbo.BoundaryLevel SET Label = N'Countries UK BUC 2017/12' WHERE Id = 1;
UPDATE dbo.BoundaryLevel SET Label = N'Counties and Unitary Authorities England and Wales BUC 2018/12' WHERE Id = 2;
UPDATE dbo.BoundaryLevel SET Label = N'Local Authority Districts UK BUC 2017/12' WHERE Id = 3;
UPDATE dbo.BoundaryLevel SET Label = N'Local Enterprise Partnerships England BUC 2017/04' WHERE Id = 4;
UPDATE dbo.BoundaryLevel SET Label = N'Westminster Parliamentary Constituencies UK BUC 2018/12' WHERE Id = 5;
UPDATE dbo.BoundaryLevel SET Label = N'Regions England BUC 2017/12' WHERE Id = 6;
UPDATE dbo.BoundaryLevel SET Label = N'Wards UK BSC 2018/12' WHERE Id = 7;
UPDATE dbo.BoundaryLevel SET Label = N'Local Authority Districts UK BUC 2019/04' WHERE Id = 8;
UPDATE dbo.BoundaryLevel SET Label = N'Counties and Unitary Authorities England and Wales BUC 2019/04' WHERE Id = 9;
UPDATE dbo.BoundaryLevel SET Label = N'Counties and Unitary Authorities UK BUC 2021/05' WHERE Id = 10;
UPDATE dbo.BoundaryLevel SET Label = N'Local Authority Districts UK BUC 2021/12' WHERE Id = 11;
UPDATE dbo.BoundaryLevel SET Label = N'Counties and Unitary Authorities UK BUC 2023/05' WHERE Id = 12;
UPDATE dbo.BoundaryLevel SET Label = N'Local Skills Improvement Plan Areas England BUC 2023/08' WHERE Id = 13;

-- Insert new boundary levels
DECLARE @Now AS DATETIME = GETUTCDATE();
SET IDENTITY_INSERT dbo.BoundaryLevel ON
INSERT INTO dbo.BoundaryLevel (Id, Level, Label, Published, Created)
VALUES  (14, N'NAT', N'Countries UK BUC 2022/12', N'2023-01-30 00:00:00.0000000', @Now),
        (15, N'REG', N'Regions England BUC 2022/12', N'2023-01-18 00:00:00.0000000', @Now),
        (16, N'LA', N'Counties and Unitary Authorities UK BUC 2023/12', N'2024-02-08 00:00:00.0000000', @Now),
        (17, N'LAD', N'Local Authority Districts UK BUC 2023/12', N'2024-01-25 00:00:00.0000000', @Now),
        (18, N'LEP', N'Local Enterprise Partnerships England BUC 2022/12', N'2023-05-26 00:00:00.0000000', @Now),
        (19, N'PCON', N'Westminster Parliamentary Constituencies UK BUC 2022/12', N'2023-03-24 00:00:00.0000000', @Now),
        (20, N'WARD', N'Wards UK BSC 2023/12', N'2024-01-01 00:00:00.0000000', @Now);
SET IDENTITY_INSERT dbo.BoundaryLevel OFF
