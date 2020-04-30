INSERT INTO ReleaseSubject (SubjectId, ReleaseId) 
SELECT Id, ReleaseId from Subject AS s
WHERE NOT EXISTS(select * FROM ReleaseSubject WHERE SubjectId = s.Id AND ReleaseId = s.ReleaseId);