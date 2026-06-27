import asyncio
import asyncpg
import os
from datetime import datetime, date
from dotenv import load_dotenv
load_dotenv()

DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'xxxx'),
    'port': os.getenv('DB_PORT', 'xxxx'),
    'database': os.getenv('DB_NAME', 'xxxx'),
    'user': os.getenv('DB_USER', 'xxxx'),
    'password': os.getenv('DB_PASSWORD', 'xxxx')}

CREATE_TABLES_SQL = """
CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    sort_order INTEGER DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS brands (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    category_id INTEGER NOT NULL REFERENCES categories(id) ON DELETE CASCADE);

CREATE TABLE IF NOT EXISTS products (
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

CREATE TABLE IF NOT EXISTS products_log (
    id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    action VARCHAR(10) NOT NULL CHECK (action IN ('insert', 'update', 'delete')),
    old_data JSONB,
    new_data JSONB,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS cities (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE);

CREATE TABLE IF NOT EXISTS users (
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
    last_activity TIMESTAMP DEFAULT CURRENT_TIMESTAMP);


CREATE TABLE IF NOT EXISTS contacts (
    id SERIAL PRIMARY KEY,
    city_id INTEGER NOT NULL REFERENCES cities(id) ON DELETE CASCADE,
    email VARCHAR(255),
    social_links JSONB NOT NULL,
    phones JSONB NOT NULL,
    addresses JSONB NOT NULL,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);

CREATE TABLE IF NOT EXISTS admins (
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
"""

CREATE_INDEXES_SQL = """
CREATE INDEX IF NOT EXISTS idx_products_category ON products(category_id);
CREATE INDEX IF NOT EXISTS idx_products_brand ON products(brand_id);
CREATE INDEX IF NOT EXISTS idx_products_active ON products(is_active);
CREATE INDEX IF NOT EXISTS idx_products_price ON products(price);
CREATE INDEX IF NOT EXISTS idx_products_details ON products USING GIN (details);
CREATE INDEX IF NOT EXISTS idx_products_name ON products(name);
CREATE INDEX IF NOT EXISTS idx_products_image_url ON products(image_url);
CREATE INDEX IF NOT EXISTS idx_products_description ON products USING GIN (to_tsvector('russian', description));
CREATE INDEX IF NOT EXISTS idx_users_tg_id ON users(tg_id);
CREATE INDEX IF NOT EXISTS idx_users_age_verified ON users(age_verified);
CREATE INDEX IF NOT EXISTS idx_users_agreed_to_terms ON users(agreed_to_terms);
CREATE INDEX IF NOT EXISTS idx_users_birth_date ON users(birth_date);
CREATE INDEX IF NOT EXISTS idx_users_blocked_until ON users(blocked_until);
CREATE INDEX IF NOT EXISTS idx_users_city ON users(city_id);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_users_created_at ON users(created_at);
CREATE INDEX IF NOT EXISTS idx_products_log_product_id ON products_log(product_id);
CREATE INDEX IF NOT EXISTS idx_products_log_changed_at ON products_log(changed_at);
CREATE INDEX IF NOT EXISTS idx_products_log_action ON products_log(action);
CREATE INDEX IF NOT EXISTS idx_admins_login ON admins(login);
CREATE INDEX IF NOT EXISTS idx_admins_is_active ON admins(is_active);
CREATE INDEX IF NOT EXISTS idx_admins_full_name ON admins(full_name);
CREATE INDEX IF NOT EXISTS idx_contacts_city ON contacts(city_id);
CREATE INDEX IF NOT EXISTS idx_brands_name ON brands(name);
CREATE INDEX IF NOT EXISTS idx_brands_category ON brands(category_id);
"""

FUNCTIONS_TRIGGERS_SQL = """
CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_products_updated_at
    BEFORE UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_admins_updated_at
    BEFORE UPDATE ON admins
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER trigger_contacts_updated_at
    BEFORE UPDATE ON contacts
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

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

CREATE TRIGGER trigger_products_insert_log
    AFTER INSERT ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_insert();

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
            'description', NEW.description)
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_products_update_log
    AFTER UPDATE ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_update();

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
            'description', OLD.description)
    );
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_products_delete_log
    BEFORE DELETE ON products
    FOR EACH ROW
    EXECUTE FUNCTION log_products_delete();

CREATE OR REPLACE FUNCTION create_admin(
    p_login VARCHAR,
    p_password VARCHAR,
    p_full_name VARCHAR,
    p_permissions JSONB DEFAULT '{}'::JSONB
)
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
        p_permissions
    )
    RETURNING id INTO v_admin_id;
    RETURN v_admin_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION authenticate_admin(
    p_login VARCHAR,
    p_password VARCHAR
)
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
    WHERE login = p_login 
        AND is_active = true 
        AND is_locked = false;
    RETURN COALESCE(v_permission, false);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION check_admin_status(p_login VARCHAR)
RETURNS TABLE(
    is_active BOOLEAN,
    is_locked BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        a.is_active,
        a.is_locked
    FROM admins a
    WHERE a.login = p_login;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION is_adult(p_birth_date DATE)
RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXTRACT(YEAR FROM age(CURRENT_DATE, p_birth_date)) >= 18;
END;
$$ LANGUAGE plpgsql;

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

CREATE OR REPLACE FUNCTION block_user(p_tg_id BIGINT, p_minutes INTEGER DEFAULT 60)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE users 
    SET blocked_until = CURRENT_TIMESTAMP + (p_minutes || ' minutes')::INTERVAL
    WHERE tg_id = p_tg_id;
    RETURN FOUND;
END;
$$ LANGUAGE plpgsql;

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
"""

TEST_DATA_SQL = """
-- Категории
INSERT INTO categories (name, sort_order) VALUES
('Жевательный табак', 1),
('Жидкости', 2),
('Жидкости без никотина', 3),
('Ароматизаторы', 4),
('Одноразовые устройства', 5),
('Табак для кальяна', 6),
('Расходные материалы', 7),
('ЭСДН', 8),
('Сигариллы и альтернатива', 9),
('Напитки и снэки', 10),
('Баки, моды и аксессуары', 11),
('Ликвидация', 12)
ON CONFLICT (name) DO NOTHING;

INSERT INTO brands (name, category_id) VALUES
('ADEX', 1),
('Geekvape', 8)
ON CONFLICT (name) DO NOTHING;

INSERT INTO cities (name) VALUES
('Киров'),
('Сыктывкар')
ON CONFLICT (name) DO NOTHING;

INSERT INTO products (category_id, name, image_url, price, details, is_active) VALUES (
    1,
    'ADEX',
    'https://vape-shop43.ru/catalog/zhevatelnyi-tabak/adex-1',
    520.00,
    '{
        "strength": ["Strong", "Medium"],
        "size": ["Wide", "Slim", "Mini"],
        "flavors": ["Ice Mint", "Cold Dry", "Ice Cool", "Eucaliptus"]
    }'::JSONB,
    TRUE
),
(
    8,
    'Sonder Q',
    'Sonder Q - изящная капсула в нестареющем дизайне в полоску, предназначенная для ежедневного отдыха.\nДля того, чтобы начать путешествие с мягким вкусом, не нужно нажимать кнопку, и наполнять его сверху не составит труда.\nПолучайте удовольствие, настраивая различные хиты с помощью переключателя воздушного потока,\nи расслабляйтесь, пока для вас ярко горит лампочка "SONDER".\n\n• Работа на Q картриджах;( новые)\n• Автоматическая затяжка ( кнопка отсутствует );\n• Максимальная мощность 20 ватт;\n• Регулировка обдува находится сбоку;\n• Батарея ёмкостью 1000mAh;\n• Надпись Sonder подсвечивается и является индикатором заряда;\n• Вес 39.1г;\n• Зарядный порт - Type-C, находится сбоку.\n\nКомплектация\n- Sonder Q mod\n- сменный картридж 0.8Ω (предустановлен)\n- руководство пользователя';
    'https://vape-shop43.ru/uploads/thumbs/default/rc/v6rGj6u2/uploads/so/sonder-q-6494140bbd23a457336613.jpg',
    1340.00,
    '{
        "colors": ["Black", "Grey", "Green", "Green Purple", "Violet Purple", 
                   "Red Blue", "Rose Pink", "Sky Blue", "White", "Stellar White", 
                   "Blue Whisper", "Starry Night", "Purple Mist", "Mystic Nebula"]
    }'::JSONB,
    1,
    TRUE
);

INSERT INTO contacts (city_id, email, social_links, phones, addresses) VALUES
(
    (SELECT id FROM cities WHERE name = 'Киров'),
    'magazinestche@gmail.com',
    jsonb_build_object(
        'vk', 'https://vk.com/hqd_pods_original',
        'telegram', 'https://t.me/est_che_43'
    ),
    '["+79123379797"]'::JSONB,
    '[
        {"address": "Герцена 87а", "schedule": "ежедневно 11:00–23:00"},
        {"address": "Преображенская 57", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Воровского 137/1", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Ленина 102а", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Пролетарская 22а", "schedule": "ежедневно 10:00–00:00"},
        {"address": "Советская 39", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Луганская 53/2", "schedule": "ежедневно 10:00–21:00"}
    ]'::JSONB
);

INSERT INTO contacts (city_id, email, social_links, phones, addresses) VALUES
(
    (SELECT id FROM cities WHERE name = 'Сыктывкар'),
    'magazinestche@gmail.com',
    jsonb_build_object(
        'vk', 'https://vk.com/est_che_store11',
        'telegram', 'https://t.me/est_che_11'
    ),
    '["+79539434455"]'::JSONB,
    '[
        {"address": "Коммунистическая 19", "schedule": "ежедневно 10:00–22:00"},
        {"address": "Бумажников 53 Д", "schedule": "ежедневно 10:00–22:00"}
    ]'::JSONB
);

async def setup_database():
    print("=" * 60)
    print("НАСТРОЙКА БАЗЫ ДАННЫХ")
    print("=" * 30)
    print()
    print("Конфигурация подключения:")
    print(f"   Host: {DB_CONFIG['host']}")
    print(f"   Port: {DB_CONFIG['port']}")
    print(f"   Database: {DB_CONFIG['database']}")
    print(f"   User: {DB_CONFIG['user']}")
    print()
    try:
        print("Подключение к базе данных")
        conn = await asyncpg.connect(**DB_CONFIG)
        print("Подключение установлено")
        print()
        print("Создание таблиц...")
        await conn.execute(CREATE_TABLES_SQL)
        print("   Таблицы созданы")
        print("Создание индексов...")
        await conn.execute(CREATE_INDEXES_SQL)
        print("   Индексы созданы")
        print("Создание функций и триггеров...")
        await conn.execute(FUNCTIONS_TRIGGERS_SQL)
        print("   Функции и триггеры созданы")
        print("Добавление тестовых данных...")
        await conn.execute(TEST_DATA_SQL)
        print("   Тестовые данные добавлены")
        print()
        print("=" * 60)
        print("БАЗА ДАННЫХ УСПЕШНО СОЗДАНА!")
        print("=" * 30)
        print()
        await conn.close()
        
    except asyncpg.exceptions.InvalidPasswordError:
        print("Ошибка: Неверный пароль!")
        print("   Проверьте DB_PASSWORD в файле .env")
    except asyncpg.exceptions.ConnectionDoesNotExistError:
        print("Ошибка: База данных не существует!")
        print(f"   Создайте базу данных '{DB_CONFIG['database']}' вручную:")
        print(f"   CREATE DATABASE {DB_CONFIG['database']};")
    except Exception as e:
        print(f"Ошибка: {e}")

if __name__ == "__main__":
    asyncio.run(setup_database())
