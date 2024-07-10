UPDATE "LocationOptionMetaLinks"
SET "PublicId" = (
  SELECT "PublicId" 
  FROM "LocationOptionMetas" 
  WHERE "LocationOptionMetas"."Id" = "LocationOptionMetaLinks"."OptionId"
);
