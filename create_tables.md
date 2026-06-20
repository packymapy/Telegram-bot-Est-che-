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
    birth_date DATE,
    age_verified BOOLEAN DEFAULT FALSE,
    agreed_to_terms BOOLEAN DEFAULT FALSE,
    verification_attempts INTEGER DEFAULT 0,
    blocked_until TIMESTAMP,
    city_id INTEGER REFERENCES cities(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_activity TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
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
CREATE INDEX idx_users_age_verified ON users(age_verified);
CREATE INDEX idx_users_agreed_to_terms ON users(agreed_to_terms);
CREATE INDEX idx_users_birth_date ON users(birth_date);
CREATE INDEX idx_users_blocked_until ON users(blocked_until);
CREATE INDEX idx_users_city ON users(city_id);
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

## Функция проверки возраста (>= 18 лет)

```sql
CREATE OR REPLACE FUNCTION is_adult(p_birth_date DATE)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXTRACT(YEAR FROM age(CURRENT_DATE, p_birth_date)) >= 18;
END;
$$ LANGUAGE plpgsql;
```

## Функция получения возраста

```sql
CREATE OR REPLACE FUNCTION get_user_age(p_tg_id BIGINT)
RETURNS INTEGER AS $$
DECLARE
    v_birth_date DATE;
BEGIN
    SELECT birth_date INTO v_birth_date
    FROM users
    WHERE tg_id = p_tg_id;
    
    IF v_birth_date IS NULL THEN
        RETURN NULL;
    END IF;
    RETURN EXTRACT(YEAR FROM age(CURRENT_DATE, v_birth_date))::INTEGER;
END;
$$ LANGUAGE plpgsql;
```

## Функция полной проверки доступа пользователя

```sql
CREATE OR REPLACE FUNCTION check_user_access(p_tg_id BIGINT)
RETURNS TABLE(
    can_access BOOLEAN,
    reason TEXT,
    age_verified BOOLEAN,
    agreed_to_terms BOOLEAN,
    is_blocked BOOLEAN,
    attempts_left INTEGER,
    block_until TIMESTAMP,
    age INTEGER
) AS $$
DECLARE
    v_user RECORD;
BEGIN
    SELECT 
        u.age_verified,
        u.agreed_to_terms,
        u.blocked_until,
        u.verification_attempts,
        u.birth_date
    INTO v_user
    FROM users u
    WHERE u.tg_id = p_tg_id;
    IF NOT FOUND THEN
        RETURN QUERY SELECT 
            false, 
            'Пользователь не найден. Напишите /start'::TEXT,
            false, false, false,
            3,
            NULL::TIMESTAMP,
            NULL::INTEGER;
        RETURN;
    END IF;
    IF v_user.blocked_until IS NOT NULL AND v_user.blocked_until > CURRENT_TIMESTAMP THEN
        RETURN QUERY SELECT 
            false, 
            format('Аккаунт заблокирован до %s', to_char(v_user.blocked_until, 'DD.MM.YYYY HH24:MI'))::TEXT,
            v_user.age_verified,
            v_user.agreed_to_terms,
            true,
            0,
            v_user.blocked_until,
            NULL::INTEGER;
        RETURN;
    END IF;
    IF v_user.birth_date IS NULL THEN
        RETURN QUERY SELECT 
            false, 
            'Необходимо указать дату рождения'::TEXT,
            false,
            v_user.agreed_to_terms,
            false,
            3 - v_user.verification_attempts,
            NULL::TIMESTAMP,
            NULL::INTEGER;
        RETURN;
    END IF;
    IF NOT is_adult(v_user.birth_date) THEN
        RETURN QUERY SELECT 
            false, 
            'Вам должно быть 18+ лет для доступа к каталогу'::TEXT,
            false,
            v_user.agreed_to_terms,
            false,
            3 - v_user.verification_attempts,
            NULL::TIMESTAMP,
            EXTRACT(YEAR FROM age(CURRENT_DATE, v_user.birth_date))::INTEGER;
        RETURN;
    END IF;
    IF NOT v_user.agreed_to_terms THEN
        RETURN QUERY SELECT 
            false, 
            'Необходимо принять условия использования'::TEXT,
            v_user.age_verified,
            false,
            false,
            3 - v_user.verification_attempts,
            NULL::TIMESTAMP,
            EXTRACT(YEAR FROM age(CURRENT_DATE, v_user.birth_date))::INTEGER;
        RETURN;
    END IF;
    RETURN QUERY SELECT 
        true, 
        'Доступ разрешен'::TEXT,
        true, true, false,
        3 - v_user.verification_attempts,
        NULL::TIMESTAMP,
        EXTRACT(YEAR FROM age(CURRENT_DATE, v_user.birth_date))::INTEGER;
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

## Функция установки даты рождения

```sql
CREATE OR REPLACE FUNCTION set_user_birth_date(
    p_tg_id BIGINT,
    p_birth_date DATE
)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    is_adult BOOLEAN
) AS $$
DECLARE
    v_is_adult BOOLEAN;
BEGIN
    v_is_adult := is_adult(p_birth_date);
    UPDATE users 
    SET 
        birth_date = p_birth_date,
        age_verified = v_is_adult,
        verification_attempts = 0,
        blocked_until = CASE 
            WHEN NOT v_is_adult THEN CURRENT_TIMESTAMP + INTERVAL '60 minutes'
            ELSE NULL
        END
    WHERE tg_id = p_tg_id;
    IF FOUND THEN
        IF v_is_adult THEN
            RETURN QUERY SELECT 
                true, 
                'Дата рождения сохранена. Теперь примите условия использования.'::TEXT,
                true;
        ELSE
            RETURN QUERY SELECT 
                false, 
                'Вам должно быть 18+ лет для доступа к каталогу. Аккаунт заблокирован на 60 минут.'::TEXT,
                false;
        END IF;
    ELSE
        RETURN QUERY SELECT 
            false, 
            'Пользователь не найден. Напишите /start'::TEXT,
            false;
    END IF;
END;
$$ LANGUAGE plpgsql;
```

## Функция принятия условий пользования

```sql
CREATE OR REPLACE FUNCTION accept_terms(p_tg_id BIGINT)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT
) AS $$
DECLARE
    v_user RECORD;
BEGIN
    SELECT 
        age_verified,
        agreed_to_terms,
        verification_attempts,
        blocked_until
    INTO v_user
    FROM users
    WHERE tg_id = p_tg_id;
    IF NOT FOUND THEN
        RETURN QUERY SELECT 
            false, 
            'Пользователь не найден. Напишите /start'::TEXT;
        RETURN;
    END IF;
    IF v_user.blocked_until IS NOT NULL AND v_user.blocked_until > CURRENT_TIMESTAMP THEN
        RETURN QUERY SELECT 
            false, 
            format('Аккаунт заблокирован до %s', to_char(v_user.blocked_until, 'DD.MM.YYYY HH24:MI'))::TEXT;
        RETURN;
    END IF;
    IF NOT v_user.age_verified THEN
        RETURN QUERY SELECT 
            false, 
            'Сначала необходимо подтвердить возраст (18+)'::TEXT;
        RETURN;
    END IF;
    UPDATE users 
    SET agreed_to_terms = true
    WHERE tg_id = p_tg_id;
    RETURN QUERY SELECT 
        true, 
        'Условия использования приняты. Добро пожаловать!'::TEXT;
END;
$$ LANGUAGE plpgsql;
```

## Функция отказа от условий пользования

```sql
CREATE OR REPLACE FUNCTION decline_terms(p_tg_id BIGINT)
RETURNS TABLE(
    success BOOLEAN,
    message TEXT,
    attempts_left INTEGER,
    is_blocked BOOLEAN
) AS $$
DECLARE
    v_user RECORD;
    v_attempts INTEGER;
BEGIN
    SELECT 
        verification_attempts,
        agreed_to_terms
    INTO v_user
    FROM users
    WHERE tg_id = p_tg_id;
    IF NOT FOUND THEN
        RETURN QUERY SELECT 
            false, 
            'Пользователь не найден'::TEXT,
            0,
            false;
        RETURN;
    END IF;
    v_attempts := v_user.verification_attempts + 1;
    IF v_attempts >= 3 THEN
        UPDATE users 
        SET 
            verification_attempts = v_attempts,
            blocked_until = CURRENT_TIMESTAMP + INTERVAL '60 minutes'
        WHERE tg_id = p_tg_id;
        RETURN QUERY SELECT 
            false, 
            'Превышено количество попыток. Аккаунт заблокирован на 60 минут.'::TEXT,
            0,
            true;
    ELSE
        UPDATE users 
        SET verification_attempts = v_attempts
        WHERE tg_id = p_tg_id;
        RETURN QUERY SELECT 
            false, 
            format('Условия не приняты. Осталось попыток: %s', 3 - v_attempts)::TEXT,
            3 - v_attempts,
            false;
    END IF;
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

## Функция получения статуса пользователя

```sql
CREATE OR REPLACE FUNCTION get_user_status(p_tg_id BIGINT)
RETURNS TABLE(
    age_verified BOOLEAN,
    agreed_to_terms BOOLEAN,
    is_blocked BOOLEAN,
    attempts_used INTEGER,
    attempts_left INTEGER,
    age INTEGER,
    block_until TIMESTAMP
) AS $$
DECLARE
    v_user RECORD;
BEGIN
    SELECT 
        u.age_verified,
        u.agreed_to_terms,
        u.blocked_until,
        u.verification_attempts,
        u.birth_date
    INTO v_user
    FROM users u
    WHERE u.tg_id = p_tg_id;
    IF NOT FOUND THEN
        RETURN QUERY SELECT 
            false, false, false,
            0, 3,
            NULL::INTEGER,
            NULL::TIMESTAMP;
        RETURN;
    END IF;
    RETURN QUERY SELECT 
        v_user.age_verified,
        v_user.agreed_to_terms,
        v_user.blocked_until IS NOT NULL AND v_user.blocked_until > CURRENT_TIMESTAMP,
        v_user.verification_attempts,
        3 - v_user.verification_attempts,
        CASE 
            WHEN v_user.birth_date IS NOT NULL 
            THEN EXTRACT(YEAR FROM age(CURRENT_DATE, v_user.birth_date))::INTEGER
            ELSE NULL
        END,
        v_user.blocked_until;
END;
$$ LANGUAGE plpgsql;
```

## Функциия сброса верификации (для повторной проверки)

```sql
CREATE OR REPLACE FUNCTION reset_verification(p_tg_id BIGINT)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE users 
    SET 
        age_verified = false,
        agreed_to_terms = false,
        verification_attempts = 0,
        blocked_until = NULL
    WHERE tg_id = p_tg_id;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
```

## Функция разблокировки пользователя (для админов)

```sql
CREATE OR REPLACE FUNCTION unblock_user(p_tg_id BIGINT)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE users 
    SET 
        blocked_until = NULL,
        verification_attempts = 0
    WHERE tg_id = p_tg_id;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;
```

## Функция проверки существования пользователя

```sql
CREATE OR REPLACE FUNCTION user_exists(p_tg_id BIGINT)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM users WHERE tg_id = p_tg_id);
END;
$$ LANGUAGE plpgsql;
```

## Функция создания пользователя, если он не существует

```sql
CREATE OR REPLACE FUNCTION create_user_if_not_exists(
    p_tg_id BIGINT,
    p_first_name VARCHAR DEFAULT NULL,
    p_last_name VARCHAR DEFAULT NULL,
    p_username VARCHAR DEFAULT NULL
)
RETURNS BOOLEAN AS $$
BEGIN
    INSERT INTO users (tg_id, first_name, last_name, username, created_at, last_activity)
    VALUES (p_tg_id, p_first_name, p_last_name, p_username, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
    ON CONFLICT (tg_id) DO NOTHING;
    RETURN FOUND;
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

## Триггер для обновления last_activity

```sql
CREATE OR REPLACE FUNCTION update_last_activity()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_activity = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

```sql
CREATE TRIGGER trigger_users_last_activity
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_last_activity();
```

## Триггер для автоматической разблокировки

```sql
CREATE OR REPLACE FUNCTION auto_unblock_user()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.blocked_until IS NOT NULL 
       AND OLD.blocked_until <= CURRENT_TIMESTAMP 
       AND NEW.blocked_until IS NOT NULL THEN
        NEW.blocked_until = NULL;
        NEW.verification_attempts = 0;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;
```

```sql
CREATE TRIGGER trigger_auto_unblock_user
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION auto_unblock_user();
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

## Функция поиска товаров по категории и бренду

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



