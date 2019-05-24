CREATE TABLE dbo.geometry
(
    ogr_fid         int identity constraint PK_geometry primary key,
    ogr_geometry    geometry,
    geographiclevel nvarchar(max),
    year            int,
    code            nvarchar(max),
    name            nvarchar(max),
    lat             float,
    long            float
);

-- CREATE INDEX ogr_dbo_geometry_ogr_geometry_sidx ON geometry (ogr_geometry);