DECLARE @RemainingSubjectCount INT = (SELECT COUNT(ID) 
FROM Subject 
WHERE SoftDeleted = 0 
AND Id NOT IN (SELECT DISTINCT SubjectId FROM ObservationRow)); 

EXEC MigrateObservationsAndObservationFilterItems @SubjectsBatchSize = @RemainingSubjectCount;