begin
    declare @subjectId NVARCHAR(max) = '803fbf56-600f-490f-8409-6413a891720d'

    declare @filterItemList IdListIntegerType
    insert @filterItemList values (29), (45)

    declare @indicatorList IdListIntegerType
    insert @indicatorList values (50), (114)

    declare @result int
    exec
        @result = FilteredFootnotes
                  @subjectId,
                  @indicatorList,
                  @filterItemList
end