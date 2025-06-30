CREATE TABLE "ReleaseStatus" (
    "Id" INTEGER PRIMARY KEY,
    "Name" TEXT NOT NULL
);

INSERT INTO "ReleaseStatus" ("Id", "Name") VALUES
    (1, 'Draft'),
    (2, 'Pending'),
    (3, 'Approved'),
    (4, 'Rejected'); 