CREATE TYPE ObservationFilterItemType AS table
(
    ObservationId uniqueidentifier NOT NULL,
    FilterItemId  uniqueidentifier NOT NULL,
    FilterId uniqueidentifier NOT NULL
);