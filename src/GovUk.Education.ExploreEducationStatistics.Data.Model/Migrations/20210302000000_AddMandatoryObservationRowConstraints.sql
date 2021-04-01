--         CONSTRAINT FK_ObservationRow_Subject_SubjectId
--             references Subject
--             on delete cascade,
IF (OBJECT_ID('FK_ObservationRow_Subject_SubjectId') IS NULL)
ALTER TABLE ObservationRow
ADD CONSTRAINT FK_ObservationRow_Subject_SubjectId
FOREIGN KEY (SubjectId) REFERENCES Subject (Id)
ON DELETE CASCADE;

-- constraint FK_ObservationRow_Location_LocationId
-- references Location,
IF (OBJECT_ID('FK_ObservationRow_Location_LocationId') IS NULL)
ALTER TABLE ObservationRow
ADD CONSTRAINT FK_ObservationRow_Location_LocationId
FOREIGN KEY (LocationId) REFERENCES Location (Id);

-- constraint FK_ObservationRowFilterItem_ObservationRow_Id
-- references ObservationRow (Id)
-- on delete cascade,
IF (OBJECT_ID('FK_ObservationRowFilterItem_ObservationRow_Id') IS NULL)
ALTER TABLE ObservationRowFilterItem
ADD CONSTRAINT FK_ObservationRowFilterItem_ObservationRow_Id
FOREIGN KEY (ObservationId) REFERENCES ObservationRow (Id)
ON DELETE CASCADE;

-- constraint FK_ObservationRowFilterItem_FilterItem_FilterItemId
-- references FilterItem,
IF (OBJECT_ID('FK_ObservationRowFilterItem_FilterItem_FilterItemId') IS NULL)
ALTER TABLE ObservationRowFilterItem
ADD CONSTRAINT FK_ObservationRowFilterItem_FilterItem_FilterItemId
FOREIGN KEY (FilterItemId) REFERENCES FilterItem (Id);