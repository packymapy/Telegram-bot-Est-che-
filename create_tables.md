# Создание таблиц
## Таблица категорий товаров

```sql
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

## Таблица товаров

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    brand_id INTEGER REFERENCES brands(id) ON DELETE SET NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    image_url VARCHAR(500),
    price DECIMAL(10,2) NOT NULL,
    details JSONB NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE);
```

## Таблица логов товаров

```sql
CREATE TABLE products_log (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    action VARCHAR(10) NOT NULL CHECK (action IN ('insert', 'update', 'delete')),
    old_data JSONB,
    new_data JSONB,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

## Таблица городов

```sql
CREATE TABLE cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE);
```

## Таблица брендов

```sql
CREATE TABLE brands (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    category_id INTEGER NOT NULL REFERENCES categories(id) ON DELETE CASCADE
);
```

## Таблица пользователей

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    tg_id BIGINT NOT NULL UNIQUE,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    username VARCHAR(100),
    birthday DATE,
    city_id INTEGER REFERENCES cities(id) ON DELETE SET NULL,
    is_verified BOOLEAN DEFAULT FALSE,
    agreed_to_terms BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_activity TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

## Таблица контактов

```sql
CREATE TABLE contacts (
    id SERIAL PRIMARY KEY,
    city_id INTEGER NOT NULL REFERENCES cities(id) ON DELETE CASCADE,
    email VARCHAR(255),
    social_links JSONB NOT NULL,
    phones JSONB NOT NULL,
    addresses JSONB NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
```

## Таблица администраторов

```sql
CREATE TABLE admins (
    id SERIAL PRIMARY KEY,
    login VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(200) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    is_locked BOOLEAN DEFAULT FALSE,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    permissions JSONB DEFAULT '{}'::JSONB);
```

<br>

# Создание индексов

## Индексы для таблицы products

```sql
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_active ON products(is_active);
CREATE INDEX idx_products_price ON products(price);
CREATE INDEX idx_products_details ON products USING GIN (details);
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_brand ON products(brand_id);
CREATE INDEX idx_products_image_url ON products(image_url);
CREATE INDEX idx_products_description ON products USING GIN (to_tsvector('russian', description));
```

## Индексы для таблицы users

```sql
CREATE INDEX idx_users_tg_id ON users(tg_id);
CREATE INDEX idx_users_city ON users(city_id);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_created_at ON users(created_at);
CREATE INDEX idx_users_is_verified ON users(is_verified);
```

## Индексы для таблицы products_log

```sql
CREATE INDEX idx_products_log_product_id ON products_log(product_id);
CREATE INDEX idx_products_log_changed_at ON products_log(changed_at);
CREATE INDEX idx_products_log_action ON products_log(action);
```

## Индексы для таблицы admins

```sql
CREATE INDEX idx_admins_login ON admins(login);
CREATE INDEX idx_admins_is_active ON admins(is_active);
CREATE INDEX idx_admins_full_name ON admins(full_name);
```

## Индексы для таблицы contacts

```sql
CREATE INDEX idx_contacts_city ON contacts(city_id);
```

## Индексы для таблицы brands

```sql
CREATE INDEX idx_brands_name ON brands(name);
CREATE INDEX idx_brands_category ON brands(category_id);
```

<br>

# Создание функций и триггеров

## Функция для обновления updated_at

```sql
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

## Триггер для обновления updated_at в products

```sql
CREATE TRIGGER trigger_products_updated_at
    BEFORE UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

## Триггер для обновления updated_at в admins

```sql
CREATE TRIGGER trigger_admins_updated_at
    BEFORE UPDATE ON admins
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

## Триггер для обновления updated_at в contacts

```sql
CREATE TRIGGER trigger_contacts_updated_at
    BEFORE UPDATE ON contacts
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();
```

## Функция логирования вставки товаров

```sql
CREATE OR REPLACE FUNCTION log_products_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, new_data)
    VALUES (
        NEW.id,
        'insert',
        jsonb_build_object(
            'name', NEW.name, 
            'price', NEW.price, 
            'details', NEW.details,
            'category_id', NEW.category_id,
            'brand_id', NEW.brand_id,
            'image_url', NEW.image_url,
            'description', NEW.description
        )
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

## Триггер логирования вставки товаров

```sql
CREATE TRIGGER trigger_products_insert_log
    AFTER INSERT ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_insert();
```

## Функция логирования обновления товаров

```sql
CREATE OR REPLACE FUNCTION log_products_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, old_data, new_data)
    VALUES (
        NEW.id,
        'update',
        jsonb_build_object(
            'name', OLD.name, 
            'price', OLD.price, 
            'details', OLD.details,
            'category_id', OLD.category_id,
            'brand_id', OLD.brand_id,
            'image_url', OLD.image_url,
            'description', OLD.description),
        jsonb_build_object(
            'name', NEW.name, 
            'price', NEW.price, 
            'details', NEW.details,
            'category_id', NEW.category_id,
            'brand_id', NEW.brand_id,
            'image_url', NEW.image_url,
            'description', NEW.description));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

## Триггер логирования обновления товаров

```sql
CREATE TRIGGER trigger_products_update_log
    AFTER UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_update();
```

## Функция логирования удаления товаров

```sql
CREATE OR REPLACE FUNCTION log_products_delete()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, old_data)
    VALUES (
        OLD.id,
        'delete',
        jsonb_build_object(
            'name', OLD.name, 
            'price', OLD.price, 
            'details', OLD.details,
            'category_id', OLD.category_id,
            'brand_id', OLD.brand_id,
            'image_url', OLD.image_url,
            'description', OLD.description));
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;
```

## Триггер логирования удаления товаров

```sql
CREATE TRIGGER trigger_products_delete_log
    BEFORE DELETE ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_delete();
```

## Расширение для хэширования паролей

```sql
CREATE EXTENSION pgcrypto;
```

## Функция создания администратора

```sql
CREATE OR REPLACE FUNCTION create_admin(
    p_login VARCHAR,
    p_password VARCHAR,
    p_permissions JSONB DEFAULT '{}'::JSONB)
RETURNS INTEGER AS $$
DECLARE
    v_admin_id INTEGER;
BEGIN
    INSERT INTO admins (login, password_hash, is_active, permissions)
    VALUES (
        p_login,
        crypt(p_password, gen_salt('bf')),
        TRUE,
        p_permissions)
    RETURNING id INTO v_admin_id;
    RETURN v_admin_id;
END;
$$ LANGUAGE plpgsql;
```

## Функция проверки логина администратора

```sql
CREATE OR REPLACE FUNCTION authenticate_admin(
    p_login VARCHAR,
    p_password VARCHAR)
RETURNS TABLE(
    admin_id INTEGER,
    full_name VARCHAR,
    permissions JSONB,
    is_active BOOLEAN,
    is_locked BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a.id,
        a.full_name,
        a.permissions,
        a.is_active,
        a.is_locked
    FROM admins a
    WHERE a.login = p_login
        AND a.password_hash = crypt(p_password, a.password_hash)
        AND a.is_active = true
        AND a.is_locked = false;
    IF FOUND THEN
        UPDATE admins 
        SET last_login = CURRENT_TIMESTAMP 
        WHERE login = p_login;
    END IF;
END;
$$ LANGUAGE plpgsql;
```

## Функция проверки прав администратора

```sql
CREATE OR REPLACE FUNCTION check_admin_permission(
    p_login VARCHAR,
    p_permission VARCHAR)
RETURNS BOOLEAN AS $$
DECLARE
    v_permission BOOLEAN;
BEGIN
    SELECT (permissions->>p_permission)::BOOLEAN
    INTO v_permission
    FROM admins
    WHERE login = p_login 
        AND is_active = true 
        AND is_locked = false;
    RETURN COALESCE(v_permission, false);
END;
$$ LANGUAGE plpgsql;
```

## Функция создания администратора

```sql
CREATE OR REPLACE FUNCTION create_admin(
    p_login VARCHAR,
    p_password VARCHAR,
    p_full_name VARCHAR,
    p_permissions JSONB DEFAULT '{}'::JSONB)
RETURNS INTEGER AS $$
DECLARE
    v_admin_id INTEGER;
BEGIN
    INSERT INTO admins (login, password_hash, full_name, is_active, permissions)
    VALUES (
        p_login,
        crypt(p_password, gen_salt('bf')),
        p_full_name,
        TRUE,
        p_permissions)
    RETURNING id INTO v_admin_id;
    RETURN v_admin_id;
END;
$$ LANGUAGE plpgsql;
```

## Функция проверки блокировки администратора

```sql
CREATE OR REPLACE FUNCTION check_admin_status(p_login VARCHAR)
RETURNS TABLE(
    is_active BOOLEAN,
    is_locked BOOLEAN) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a.is_active,
        a.is_locked
    FROM admins a
    WHERE a.login = p_login;
END;
$$ LANGUAGE plpgsql;
```

## Функция блокировки администратора

```sql
CREATE OR REPLACE FUNCTION lock_admin(p_login VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE admins 
    SET is_locked = true
    WHERE login = p_login AND is_active = true;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
```

## Функция разблокировки администратора

```sql
CREATE OR REPLACE FUNCTION unlock_admin(p_login VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE admins 
    SET is_locked = false
    WHERE login = p_login;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
```

## Функция деактивации администратора

```sql
CREATE OR REPLACE FUNCTION deactivate_admin(p_login VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE admins 
    SET is_active = false
    WHERE login = p_login;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
```

## Функция активация администратора

```sql
CREATE OR REPLACE FUNCTION activate_admin(p_login VARCHAR)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE admins 
    SET is_active = true,
        is_locked = false
    WHERE login = p_login;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
```

## Функция получения брендов для конкретной категории

```sql
CREATE OR REPLACE FUNCTION get_brands_by_category(p_category_id INTEGER)
RETURNS TABLE(
    brand_id INTEGER,
    brand_name VARCHAR,
    product_count BIGINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        b.id,
        b.name,
        COUNT(p.id)::BIGINT
    FROM brands b
    LEFT JOIN products p ON p.brand_id = b.id AND p.is_active = true
    WHERE b.category_id = p_category_id AND b.is_active = true
    GROUP BY b.id, b.name
    ORDER BY b.sort_order, b.name;
END;
$$ LANGUAGE plpgsql;
```

## Поиск товаров по категории и бренду

```sql
CREATE OR REPLACE FUNCTION search_products_by_category_brand(
    p_category_id INTEGER DEFAULT NULL,
    p_brand_id INTEGER DEFAULT NULL,
    p_search TEXT DEFAULT NULL
)
RETURNS TABLE(
    product_id INTEGER,
    product_name VARCHAR,
    brand_name VARCHAR,
    price DECIMAL,
    details JSONB
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.id,
        p.name,
        b.name,
        p.price,
        p.details
    FROM products p
    LEFT JOIN brands b ON b.id = p.brand_id
    WHERE (p_category_id IS NULL OR p.category_id = p_category_id)
        AND (p_brand_id IS NULL OR p.brand_id = p_brand_id)
        AND (p_search IS NULL OR p.name ILIKE '%' || p_search || '%')
        AND p.is_active = true
    ORDER BY b.sort_order NULLS LAST, p.name;
END;
$$ LANGUAGE plpgsql;
```



