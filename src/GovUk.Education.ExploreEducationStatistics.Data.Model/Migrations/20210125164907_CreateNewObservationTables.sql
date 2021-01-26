create table ObservationRow
(
    Id              bigint identity(1,1) NOT NULL,
    ObservationId   uniqueidentifier not null,
    SubjectId       uniqueidentifier not null
        CONSTRAINT FK_ObservationRow_Subject_SubjectId
            references Subject
            on delete cascade,
    GeographicLevel nvarchar(6)      not null,
    LocationId      uniqueidentifier not null
        constraint FK_ObservationRow_Location_LocationId
            references Location,
    Year            int              not null,
    TimeIdentifier  nvarchar(6)      not null,
    Measures        nvarchar(max),
    CsvRow          bigint           not null,
    CONSTRAINT PK_ObservationRow PRIMARY KEY (Id)
)
go

create index IX_ObservationRow_GeographicLevel
    on ObservationRow (GeographicLevel)
go

create index IX_ObservationRow_LocationId
    on ObservationRow (LocationId)
go

create index IX_ObservationRow_SubjectId
    on ObservationRow (SubjectId)
go

create index IX_ObservationRow_TimeIdentifier
    on ObservationRow (TimeIdentifier)
go

create index IX_ObservationRow_Year
    on ObservationRow (Year)
go

create index NCI_WI_ObservationRow_SubjectId
    on ObservationRow (SubjectId) include (GeographicLevel, LocationId, TimeIdentifier, Year)
go

create table ObservationRowFilterItem
(
    Id            bigint identity(1,1) NOT NULL,
    ObservationId bigint not null
        constraint FK_ObservationRowFilterItem_ObservationRow_Id
            references ObservationRow (Id)
            on delete cascade,
    OldObservationId uniqueidentifier not null,
    FilterItemId  uniqueidentifier not null
        constraint FK_ObservationRowFilterItem_FilterItem_FilterItemId
            references FilterItem,
    FilterId      uniqueidentifier
        constraint FK_ObservationRowFilterItem_Filter_FilterId
            references Filter,
    constraint PK_ObservationRowFilterItem
        primary key (Id)
)
go

create index IX_ObservationRowFilterItem_FilterItemId_ObservationId
    on ObservationRowFilterItem (FilterItemId, ObservationId)

create index IX_ObservationRowFilterItem_FilterItemId
    on ObservationRowFilterItem (FilterItemId)

create index IX_ObservationRowFilterItem_FilterId
    on ObservationRowFilterItem (FilterId)
go