CREATE INDEX ix_coffees_published
ON coffees (status)
WHERE status = 'Published';