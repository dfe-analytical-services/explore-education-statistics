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

CREATE SPATIAL INDEX ogr_dbo_geometry_ogr_geometry_sidx ON dbo.geometry (ogr_geometry)
USING GEOMETRY_GRID
WITH ( BOUNDING_BOX =(-8.6453507402877,49.8823462377178,1.76291602644957,60.860573773178) );