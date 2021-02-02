--         CONSTRAINT FK_ObservationRow_Subject_SubjectId
--             references Subject
--             on delete cascade,
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'FK_ObservationRow_Subject_SubjectId')
ALTER TABLE ObservationRow
ADD CONSTRAINT FK_ObservationRow_Subject_SubjectId
FOREIGN KEY (SubjectId) REFERENCES Subject (Id)
ON DELETE CASCADE;

-- constraint FK_ObservationRow_Location_LocationId
-- references Location,
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'FK_ObservationRow_Location_LocationId')
ALTER TABLE ObservationRow
ADD CONSTRAINT FK_ObservationRow_Location_LocationId
FOREIGN KEY (LocationId) REFERENCES Location (Id);




-- constraint FK_ObservationRowFilterItem_ObservationRow_Id
-- references ObservationRow (Id)
-- on delete cascade,
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'FK_ObservationRowFilterItem_ObservationRow_Id')
ALTER TABLE ObservationRowFilterItem
ADD CONSTRAINT FK_ObservationRowFilterItem_ObservationRow_Id
FOREIGN KEY (ObservationId) REFERENCES ObservationRow (Id)
ON DELETE CASCADE;

-- constraint FK_ObservationRowFilterItem_FilterItem_FilterItemId
-- references FilterItem,
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'FK_ObservationRowFilterItem_FilterItem_FilterItemId')
ALTER TABLE ObservationRowFilterItem
ADD CONSTRAINT FK_ObservationRowFilterItem_FilterItem_FilterItemId
FOREIGN KEY (FilterItemId) REFERENCES FilterItem (Id);

-- constraint FK_ObservationRowFilterItem_Filter_FilterId
-- references Filter,
IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'FK_ObservationRowFilterItem_Filter_FilterId')
ALTER TABLE ObservationRowFilterItem
ADD CONSTRAINT FK_ObservationRowFilterItem_Filter_FilterId
FOREIGN KEY (FilterId) REFERENCES Filter (Id);