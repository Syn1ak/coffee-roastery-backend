CREATE TABLE coffees (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    name text NOT NULL,
    origin text NOT NULL,
    roast_style text NOT NULL CHECK (roast_style IN ('Filter', 'Espresso')),
    status text NOT NULL CHECK (status IN ('Draft', 'Published', 'Retired')) DEFAULT 'Draft',
    stock_mode text NOT NULL CHECK (stock_mode IN ('Shelf', 'RoastToOrder'))
);

CREATE TABLE coffee_prices (
    coffee_id uuid NOT NULL REFERENCES coffees(id),
    bag_size text NOT NULL CHECK (bag_size IN ('Grams250', 'Kilogram1')),
    amount numeric(10, 2) NOT NULL CHECK (amount > 0),
    currency char(3) NOT NULL CHECK (currency = 'EUR'),
    PRIMARY KEY (coffee_id, bag_size)
);

INSERT INTO coffees (name, origin, roast_style, stock_mode)
VALUES ('Liberica', 'Test origin', 'Espresso', 'RoastToOrder')
RETURNING id;

INSERT INTO coffee_prices (coffee_id, bag_size, amount, currency)
VALUES ('01a11d36-5391-7c11-8b27-060ca6b1822e', 'Grams250', 0, 'EUR');

INSERT INTO coffee_prices (coffee_id, bag_size, amount, currency)
VALUES ('01a11d34-59df-7909-97dd-6079847796ef', 'Kilogram1', 41.50, 'EUR');

SELECT * FROM coffees c JOIN coffee_prices cp 
ON c.id = cp.coffee_id; 

UPDATE coffees SET status = 'Published' WHERE id = '01a11d34-59df-7909-97dd-6079847796ef';