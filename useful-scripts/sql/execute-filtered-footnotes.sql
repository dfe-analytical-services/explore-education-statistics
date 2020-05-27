begin
    declare @releaseId uniqueidentifier = '4FA4FE8E-9A15-46BB-823F-49BF8E0CDEC5'

    declare @subjectId uniqueidentifier = '803FBF56-600F-490F-8409-6413A891720D'

    declare @filterItemList IdListGuidType
    insert @filterItemList values ('0520D3CB-EE9F-4CC0-BAF7-08D78F6F2C4D'), ('A9FE9FA6-E91F-460B-A0B1-66877B97C581')

    declare @indicatorList IdListGuidType
    insert @indicatorList values ('92D3437A-0A62-4CD7-8DFB-BCCEBA7EEF61'), ('4D536D87-DEA3-4946-2B05-08D78F6F2B08')

    declare @result int
    exec
        @result = FilteredFootnotes
                  @releaseId,
                  @subjectId,
                  @indicatorList,
                  @filterItemList
end