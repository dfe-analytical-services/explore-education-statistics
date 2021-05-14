-- Create geometry

CREATE TABLE dbo.geometry
(
	ogr_fid int identity constraint PK_geometry primary key,
	ogr_geometry geometry,
	code nvarchar(max),
	name nvarchar(max),
	lat float,
	long float,
	boundary_level_id bigint constraint geometry_BoundaryLevel_Id_fk references BoundaryLevel
);

CREATE SPATIAL INDEX ogr_dbo_geometry_ogr_geometry_sidx ON dbo.geometry (ogr_geometry)
USING GEOMETRY_GRID
WITH ( BOUNDING_BOX =(-8.6453507402877,49.8823462377178,1.76291602644957,60.860573773178) );

-- Create spatial_ref_sys

CREATE TABLE dbo.spatial_ref_sys
(
    srid      int not null primary key,
    auth_name varchar(256),
    auth_srid int,
    srtext    varchar(2048),
    proj4text varchar(2048)
);

INSERT INTO dbo.spatial_ref_sys (srid, auth_name, auth_srid, srtext, proj4text) VALUES (4326, 'EPSG', 4326, 'GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","4326"]]', '+proj=longlat +datum=WGS84 +no_defs ');

-- Create geometry_columns

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

INSERT INTO dbo.geometry_columns (f_table_catalog, f_table_schema, f_table_name, f_geometry_column, coord_dimension, srid, geometry_type) VALUES ('statistics', 'dbo', 'geometry', 'ogr_geometry', 2, 4326, 'POLYGON');
