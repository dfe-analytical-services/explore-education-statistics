CREATE TABLE dbo.geometry_columns
(
    f_table_catalog   varchar(128) not null,
    f_table_schema    varchar(128) not null,
    f_table_name      varchar(256) not null,
    f_geometry_column varchar(256) not null,
    coord_dimension   int          not null,
    srid              int          not null,
    geometry_type     varchar(30)  not null,
    constraint geometry_columns_pk
        primary key (f_table_catalog, f_table_schema, f_table_name, f_geometry_column)
);