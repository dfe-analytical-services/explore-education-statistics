CREATE OR ALTER VIEW dbo.geojson AS
SELECT geographiclevel AS GeographicLevel,
       year AS Year,
       code AS Code,
       name AS Name,
       (
           SELECT 'Feature'                                   AS 'type',
                  code                                        AS 'properties.code',
                  lat                                         AS 'properties.lat',
                  long                                        AS 'properties.long',
                  name                                        AS 'properties.name',
                  JSON_QUERY(dbo.geometry2json(ogr_geometry)) AS geometry
                  FOR JSON PATH
       ) as Value
FROM dbo.geometry