begin
    declare @subjectId int = 1

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