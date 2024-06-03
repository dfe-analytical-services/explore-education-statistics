

CREATE SEQUENCE data_seq INCREMENT BY 1 MINVALUE 1 MAXVALUE 9223372036854775807 START 217 NO CYCLE;

CREATE TABLE "data"(id UINTEGER PRIMARY KEY, time_period_id INTEGER NOT NULL, geographic_level VARCHAR NOT NULL, locations_la_id INTEGER NOT NULL, locations_nat_id INTEGER NOT NULL, locations_reg_id INTEGER NOT NULL, locations_sch_id INTEGER NOT NULL, academy_type INTEGER NOT NULL, ncyear INTEGER NOT NULL, school_type INTEGER NOT NULL, enrolments VARCHAR NOT NULL, sess_authorised VARCHAR NOT NULL, sess_possible VARCHAR NOT NULL, sess_unauthorised VARCHAR NOT NULL, sess_unauthorised_percent VARCHAR NOT NULL);
CREATE TABLE filter_options(id INTEGER PRIMARY KEY, "label" VARCHAR, public_id VARCHAR, filter_id VARCHAR);
CREATE TABLE indicators(id VARCHAR PRIMARY KEY, "label" VARCHAR, unit VARCHAR, decimal_places TINYINT);
CREATE TABLE location_options(id INTEGER PRIMARY KEY, "label" VARCHAR, "level" VARCHAR, public_id VARCHAR, code VARCHAR, old_code VARCHAR, urn VARCHAR, laestab VARCHAR, ukprn VARCHAR);
CREATE TABLE time_periods(id INTEGER PRIMARY KEY, period VARCHAR, identifier VARCHAR);




