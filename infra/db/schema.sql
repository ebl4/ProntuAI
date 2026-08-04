-- Schema inicial para ProntuAI (PostgreSQL)

CREATE TABLE IF NOT EXISTS "AspNetUsers" (
	"Id" varchar(450) NOT NULL PRIMARY KEY
	-- Identity default columns will be created by EF migrations; keep this script as fallback
);

CREATE TABLE IF NOT EXISTS notes (
	id uuid PRIMARY KEY,
	subjective text,
	objective text,
	assessment text,
	plan text,
	transcript text,
	createdbyid varchar(450),
	createdat timestamp with time zone DEFAULT now()
);

CREATE TABLE IF NOT EXISTS audiofiles (
	id uuid PRIMARY KEY,
	filename text,
	contenttype text,
	size bigint,
	storedpath text,
	uploadedat timestamp with time zone DEFAULT now(),
	uploadedbyid varchar(450)
);
