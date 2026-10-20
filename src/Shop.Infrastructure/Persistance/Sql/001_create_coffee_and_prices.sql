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