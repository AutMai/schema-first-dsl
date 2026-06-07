CREATE TABLE User (
  id                    INTEGER NOT NULL,
  username              VARCHAR(255) NOT NULL,
  email                 VARCHAR(255) NOT NULL,
  password_hash         VARCHAR(255) NOT NULL,
  PRIMARY KEY (id)
);

CREATE TABLE Order (
  id                    INTEGER NOT NULL,
  user_id               INTEGER NOT NULL,
  total                 DECIMAL(10,2) NOT NULL,
  PRIMARY KEY (id),
  FOREIGN KEY (user_id) REFERENCES User(id)
);

CREATE TABLE Product (
  id                    INTEGER NOT NULL,
  name                  VARCHAR(255) NOT NULL,
  price                 DECIMAL(10,2) NOT NULL,
  description           VARCHAR(255),
  PRIMARY KEY (id)
);

CREATE TABLE Order_Product (
  order_id  INTEGER NOT NULL,
  product_id  INTEGER NOT NULL,
  PRIMARY KEY (order_id, product_id),
  FOREIGN KEY (order_id) REFERENCES Order(id),
  FOREIGN KEY (product_id) REFERENCES Product(id)
);
