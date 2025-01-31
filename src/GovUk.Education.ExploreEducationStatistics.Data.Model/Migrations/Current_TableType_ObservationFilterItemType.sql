create type ObservationFilterItemType as table
(
    ObservationId uniqueidentifier not null,
    FilterItemId  uniqueidentifier not null,
    FilterId      uniqueidentifier not null
);
