# Создание таблиц
## Таблица категорий товаров

```sql
CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Таблица товаров

```sql
CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    category_id INTEGER NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    name VARCHAR(255) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    details JSONB NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);
```

## Таблица логов товаров

```sql
CREATE TABLE IF NOT EXISTS products_log (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    action VARCHAR(10) NOT NULL CHECK (action IN ('insert', 'update', 'delete')),
    old_data JSONB,
    new_data JSONB,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Таблица городов

```sql
CREATE TABLE IF NOT EXISTS cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);
```

## Таблица пользователей (исправлена - убран is_admin)

```sql
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    tg_id BIGINT NOT NULL UNIQUE,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    username VARCHAR(100),
    birthday DATE,
    city_id INTEGER REFERENCES cities(id) ON DELETE SET NULL,
    agreed_to_terms BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_activity TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Таблица контактов

```sql
CREATE TABLE IF NOT EXISTS contacts (
    id SERIAL PRIMARY KEY,
    city_id INTEGER NOT NULL REFERENCES cities(id) ON DELETE CASCADE,
    email VARCHAR(255),
    social_links JSONB NOT NULL,
    phones JSONB NOT NULL,
    addresses JSONB NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Таблица администраторов

```sql
CREATE TABLE IF NOT EXISTS admins (
    id SERIAL PRIMARY KEY,
    login VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    last_login TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    permissions JSONB DEFAULT '{}'::JSONB
);
```

<br>

# Создание индексов

## Индексы для таблицы products

```sql
CREATE INDEX IF NOT EXISTS idx_products_category ON products(category_id);
CREATE INDEX IF NOT EXISTS idx_products_active ON products(is_active);
CREATE INDEX IF NOT EXISTS idx_products_price ON products(price);
CREATE INDEX IF NOT EXISTS idx_products_details ON products USING GIN (details);
CREATE INDEX IF NOT EXISTS idx_products_name ON products(name);
```

## Индексы для таблицы users (исправлены)

```sql
CREATE INDEX IF NOT EXISTS idx_users_tg_id ON users(tg_id);
CREATE INDEX IF NOT EXISTS idx_users_city ON users(city_id);
CREATE INDEX IF NOT EXISTS idx_users_is_blocked ON users(is_blocked);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(created_at);
```

## Индексы для таблицы products_log

```sql
CREATE INDEX IF NOT EXISTS idx_products_log_product_id ON products_log(product_id);
CREATE INDEX IF NOT EXISTS idx_products_log_changed_at ON products_log(changed_at);
CREATE INDEX IF NOT EXISTS idx_products_log_action ON products_log(action);
```

## Индексы для таблицы admins

```sql
CREATE INDEX IF NOT EXISTS idx_admins_login ON admins(login);
CREATE INDEX IF NOT EXISTS idx_admins_is_active ON admins(is_active);
```

## Индексы для таблицы contacts

```sql
CREATE INDEX IF NOT EXISTS idx_contacts_city ON contacts(city_id);
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
        jsonb_build_object('name', NEW.name, 'price', NEW.price, 'details', NEW.details)
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
        jsonb_build_object('name', OLD.name, 'price', OLD.price, 'details', OLD.details),
        jsonb_build_object('name', NEW.name, 'price', NEW.price, 'details', NEW.details)
    );
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

-- Функция логирования удаления товаров

```sql
CREATE OR REPLACE FUNCTION log_products_delete()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO products_log (product_id, action, old_data)
    VALUES (
        OLD.id,
        'delete',
        jsonb_build_object('name', OLD.name, 'price', OLD.price, 'details', OLD.details)
    );
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
CREATE EXTENSION IF NOT EXISTS pgcrypto;
```

## Функция создания администратора

```sql
CREATE OR REPLACE FUNCTION create_admin(
    p_login VARCHAR,
    p_password VARCHAR,
    p_permissions JSONB DEFAULT '{}'::JSONB
)
RETURNS INTEGER AS $$
DECLARE
    v_admin_id INTEGER;
BEGIN
    INSERT INTO admins (login, password_hash, is_active, permissions)
    VALUES (
        p_login,
        crypt(p_password, gen_salt('bf')),
        TRUE,
        p_permissions
    )
    RETURNING id INTO v_admin_id;
    
    RETURN v_admin_id;
END;
$$ LANGUAGE plpgsql;
```

## Функция проверки логина администратора

```sql
CREATE OR REPLACE FUNCTION authenticate_admin(
    p_login VARCHAR,
    p_password VARCHAR
)
RETURNS TABLE(
    admin_id INTEGER,
    permissions JSONB,
    is_active BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a.id,
        a.permissions,
        a.is_active
    FROM admins a
    WHERE a.login = p_login
        AND a.password_hash = crypt(p_password, a.password_hash)
        AND a.is_active = true;
    
    -- Обновляем время последнего входа при успешной аутентификации
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
    p_permission VARCHAR
)
RETURNS BOOLEAN AS $$
DECLARE
    v_permission BOOLEAN;
BEGIN
    SELECT (permissions->>p_permission)::BOOLEAN
    INTO v_permission
    FROM admins
    WHERE login = p_login AND is_active = true;
    
    RETURN COALESCE(v_permission, false);
END;
$$ LANGUAGE plpgsql;
```
