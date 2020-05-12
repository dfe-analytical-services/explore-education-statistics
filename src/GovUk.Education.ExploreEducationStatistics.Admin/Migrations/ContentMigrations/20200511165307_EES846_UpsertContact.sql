DECLARE @Id UNIQUEIDENTIFIER = '3b263e1d-9df3-45a4-acba-086c820db140';
DECLARE @ContactName VARCHAR(MAX) = 'Gabriel Kite';
DECLARE @ContactTelNo VARCHAR(MAX) = '07796275845';
DECLARE @TeamEmail VARCHAR(MAX) = 'HE.statistics@education.gov.uk';
DECLARE @TeamName VARCHAR(MAX) = 'Higher education analysis team';

MERGE dbo.Contacts AS target
USING (SELECT @Id AS Id) AS source
ON (target.Id = source.Id)
WHEN MATCHED THEN
    UPDATE
    SET ContactName  = @ContactName,
        ContactTelNo = @ContactTelNo,
        TeamEmail    = @TeamEmail,
        TeamName     = @TeamName
WHEN NOT MATCHED THEN
    INSERT (Id, ContactName, ContactTelNo, TeamEmail, TeamName)
    VALUES (source.Id, @ContactName, @ContactTelNo, @TeamEmail, @TeamName);